using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TwentyNet.Application.Workflows;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;
using TwentyNet.Domain.Workflows;
using TwentyNet.Persistence;
using TwentyNet.Persistence.Repositories;
using TaskItemEntity = TwentyNet.Domain.Entities.TaskItem;

namespace TwentyNet.Application.Tests.Workflows;

public sealed class RunWorkflowsOnObjectChangeTests : TestBase
{
    private static RunWorkflowsOnObjectChange CreateRunner(
        AppDbContext context,
        ISecureHttpClient? secureHttpClient = null,
        ILogger<RunWorkflowsOnObjectChange>? logger = null)
    {
        return new RunWorkflowsOnObjectChange(
            new EfRepository<Workflow>(context),
            new EfRepository<TaskItemEntity>(context),
            new EfRepository<Company>(context),
            new EfRepository<Person>(context),
            secureHttpClient ?? Substitute.For<ISecureHttpClient>(),
            logger ?? Substitute.For<ILogger<RunWorkflowsOnObjectChange>>());
    }

    [Fact]
    public async Task Handle_RecordCreated_ShouldCreateTask_WhenWorkflowMatches()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Workflows.Add(new Workflow
        {
            Name = "Create task on company",
            WorkspaceId = workspaceId,
            IsActive = true,
            TriggerType = WorkflowTriggerType.RecordCreated,
            TriggerObjectName = "Company",
            Actions = JsonSerializer.Serialize(new List<WorkflowActionConfig>
            {
                new(WorkflowActionType.CreateTask, Task: new CreateTaskActionConfig("Follow up"))
            })
        });
        await context.SaveChangesAsync();

        var runner = CreateRunner(context);
        var company = new Company { Name = "Acme", WorkspaceId = workspaceId };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        await runner.Handle(new ObjectRecordCreatedEvent(workspaceId, "Company", company.Id), CancellationToken.None);

        var task = context.TaskItems.Single();
        Assert.Equal("Follow up", task.Title);
        Assert.Equal(workspaceId, task.WorkspaceId);
    }

    [Fact]
    public async Task Handle_RecordCreated_ShouldNotRun_WhenObjectNameDoesNotMatch()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Workflows.Add(new Workflow
        {
            Name = "Company only",
            WorkspaceId = workspaceId,
            IsActive = true,
            TriggerType = WorkflowTriggerType.RecordCreated,
            TriggerObjectName = "Company",
            Actions = JsonSerializer.Serialize(new List<WorkflowActionConfig>
            {
                new(WorkflowActionType.CreateTask, Task: new CreateTaskActionConfig("Follow up"))
            })
        });
        await context.SaveChangesAsync();

        var runner = CreateRunner(context);
        var person = new Person { FirstName = "John", LastName = "Doe", WorkspaceId = workspaceId };
        context.People.Add(person);
        await context.SaveChangesAsync();

        await runner.Handle(new ObjectRecordCreatedEvent(workspaceId, "Person", person.Id), CancellationToken.None);

        Assert.Empty(context.TaskItems);
    }

    [Fact]
    public async Task Handle_RecordCreated_ShouldCallWebhook_WhenActionIsSendWebhook()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Workflows.Add(new Workflow
        {
            Name = "Webhook on company",
            WorkspaceId = workspaceId,
            IsActive = true,
            TriggerType = WorkflowTriggerType.RecordCreated,
            TriggerObjectName = "Company",
            Actions = JsonSerializer.Serialize(new List<WorkflowActionConfig>
            {
                new(WorkflowActionType.SendWebhook, Webhook: new SendWebhookActionConfig("https://example.com/webhook"))
            })
        });
        await context.SaveChangesAsync();

        var secureHttpClient = Substitute.For<ISecureHttpClient>();
        secureHttpClient.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>(), Arg.Any<CancellationToken>())
            .Returns(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

        var runner = CreateRunner(context, secureHttpClient);
        var company = new Company { Name = "Acme", WorkspaceId = workspaceId };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        await runner.Handle(new ObjectRecordCreatedEvent(workspaceId, "Company", company.Id), CancellationToken.None);

        await secureHttpClient.Received(1).PostAsync(
            "https://example.com/webhook",
            Arg.Any<HttpContent>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_RecordUpdated_ShouldUpdateField_WhenActionIsUpdateField()
    {
        await using var context = CreateInMemoryContext();
        var workspaceId = Guid.NewGuid();
        context.Workflows.Add(new Workflow
        {
            Name = "Update company address",
            WorkspaceId = workspaceId,
            IsActive = true,
            TriggerType = WorkflowTriggerType.RecordUpdated,
            TriggerObjectName = "Company",
            Actions = JsonSerializer.Serialize(new List<WorkflowActionConfig>
            {
                new(WorkflowActionType.UpdateField, UpdateField: new UpdateFieldActionConfig("Address", "Updated by workflow"))
            })
        });
        await context.SaveChangesAsync();

        var runner = CreateRunner(context);
        var company = new Company { Name = "Acme", WorkspaceId = workspaceId };
        context.Companies.Add(company);
        await context.SaveChangesAsync();

        await runner.Handle(new ObjectRecordUpdatedEvent(workspaceId, "Company", company.Id), CancellationToken.None);

        var updated = await context.Companies.FindAsync(company.Id);
        Assert.Equal("Updated by workflow", updated!.Address);
    }
}
