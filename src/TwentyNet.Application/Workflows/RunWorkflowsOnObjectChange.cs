using System.Reflection;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using TwentyNet.Domain.Common;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Workflows;
using TaskItemEntity = TwentyNet.Domain.Entities.TaskItem;

namespace TwentyNet.Application.Workflows;

public sealed class RunWorkflowsOnObjectChange :
    INotificationHandler<ObjectRecordCreatedEvent>,
    INotificationHandler<ObjectRecordUpdatedEvent>
{
    private readonly IRepository<Workflow> _workflowRepository;
    private readonly IRepository<TaskItemEntity> _taskRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<Person> _personRepository;
    private readonly ISecureHttpClient _secureHttpClient;
    private readonly ILogger<RunWorkflowsOnObjectChange> _logger;

    public RunWorkflowsOnObjectChange(
        IRepository<Workflow> workflowRepository,
        IRepository<TaskItemEntity> taskRepository,
        IRepository<Company> companyRepository,
        IRepository<Person> personRepository,
        ISecureHttpClient secureHttpClient,
        ILogger<RunWorkflowsOnObjectChange> logger)
    {
        _workflowRepository = workflowRepository;
        _taskRepository = taskRepository;
        _companyRepository = companyRepository;
        _personRepository = personRepository;
        _secureHttpClient = secureHttpClient;
        _logger = logger;
    }

    public Task Handle(ObjectRecordCreatedEvent notification, CancellationToken cancellationToken)
        => RunAsync(notification, WorkflowTriggerType.RecordCreated, cancellationToken);

    public Task Handle(ObjectRecordUpdatedEvent notification, CancellationToken cancellationToken)
        => RunAsync(notification, WorkflowTriggerType.RecordUpdated, cancellationToken);

    private async Task RunAsync(IDomainEvent domainEvent, WorkflowTriggerType triggerType, CancellationToken cancellationToken)
    {
        var (workspaceId, objectName, recordId) = domainEvent switch
        {
            ObjectRecordCreatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId),
            ObjectRecordUpdatedEvent e => (e.WorkspaceId, e.ObjectName, e.RecordId),
            _ => throw new NotSupportedException($"Domain event type '{domainEvent.GetType().Name}' is not supported.")
        };

        var workflows = await _workflowRepository.ListAsync(
            w => w.WorkspaceId == workspaceId
                 && w.IsActive
                 && w.TriggerType == triggerType
                 && (w.TriggerObjectName == null || w.TriggerObjectName == objectName),
            cancellationToken);

        if (workflows.Count == 0)
        {
            return;
        }

        var record = await LoadRecordAsync(objectName, recordId, cancellationToken);
        if (record is null)
        {
            _logger.LogWarning("Record {ObjectName}:{RecordId} not found for workflow execution.", objectName, recordId);
            return;
        }

        foreach (var workflow in workflows)
        {
            try
            {
                await ExecuteWorkflowAsync(workflow, objectName, recordId, record, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Workflow {WorkflowId} execution failed.", workflow.Id);
            }
        }
    }

    private async Task<object?> LoadRecordAsync(string objectName, Guid recordId, CancellationToken cancellationToken)
    {
        if (objectName == "Company")
        {
            return await _companyRepository.GetByIdAsync(recordId, cancellationToken);
        }

        if (objectName == "Person")
        {
            return await _personRepository.GetByIdAsync(recordId, cancellationToken);
        }

        return null;
    }

    private async Task ExecuteWorkflowAsync(
        Workflow workflow,
        string objectName,
        Guid recordId,
        object record,
        CancellationToken cancellationToken)
    {
        var actions = JsonSerializer.Deserialize<List<WorkflowActionConfig>>(workflow.Actions)
            ?? new List<WorkflowActionConfig>();

        foreach (var action in actions)
        {
            switch (action.Type)
            {
                case WorkflowActionType.SendWebhook when action.Webhook is not null:
                    await ExecuteWebhookAsync(action.Webhook, objectName, recordId, cancellationToken);
                    break;

                case WorkflowActionType.CreateTask when action.Task is not null:
                    await ExecuteCreateTaskAsync(workflow.WorkspaceId, action.Task, objectName, recordId, cancellationToken);
                    break;

                case WorkflowActionType.UpdateField when action.UpdateField is not null:
                    await ExecuteUpdateFieldAsync(record, action.UpdateField, cancellationToken);
                    break;
            }
        }
    }

    private async Task ExecuteWebhookAsync(
        SendWebhookActionConfig config,
        string objectName,
        Guid recordId,
        CancellationToken cancellationToken)
    {
        var payload = new
        {
            eventName = $"{objectName.ToLowerInvariant()}.workflow",
            objectName,
            recordId,
            timestamp = DateTime.UtcNow
        };

        var json = JsonSerializer.Serialize(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        await _secureHttpClient.PostAsync(config.Url, content, cancellationToken);
    }

    private async Task ExecuteCreateTaskAsync(
        Guid workspaceId,
        CreateTaskActionConfig config,
        string objectName,
        Guid recordId,
        CancellationToken cancellationToken)
    {
        var task = new TaskItemEntity
        {
            Title = config.Title,
            WorkspaceId = workspaceId,
            Status = Domain.Enums.TaskStatus.Todo
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }

    private async Task ExecuteUpdateFieldAsync(object record, UpdateFieldActionConfig config, CancellationToken cancellationToken)
    {
        var property = record.GetType().GetProperty(config.FieldName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        if (property is null || !property.CanWrite)
        {
            _logger.LogWarning("Property {FieldName} not found on {RecordType}.", config.FieldName, record.GetType().Name);
            return;
        }

        var converted = Convert.ChangeType(config.FieldValue, property.PropertyType);
        property.SetValue(record, converted);

        switch (record)
        {
            case Company company:
                _companyRepository.Update(company);
                await _companyRepository.SaveChangesAsync(cancellationToken);
                break;
            case Person person:
                _personRepository.Update(person);
                await _personRepository.SaveChangesAsync(cancellationToken);
                break;
        }
    }
}
