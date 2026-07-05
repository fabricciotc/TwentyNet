using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.CreateTask;

public sealed class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IRepository<Company> _companyRepository;
    private readonly IRepository<Person> _personRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IPublisher _publisher;

    public CreateTaskCommandHandler(
        IRepository<TaskItem> taskRepository,
        IRepository<Company> companyRepository,
        IRepository<Person> personRepository,
        IMapper mapper,
        IAuthContext authContext,
        IPublisher publisher)
    {
        _taskRepository = taskRepository;
        _companyRepository = companyRepository;
        _personRepository = personRepository;
        _mapper = mapper;
        _authContext = authContext;
        _publisher = publisher;
    }

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;
        var userId = _authContext.UserId.Value;

        await ValidateRecordExistsAsync(request, workspaceId, cancellationToken);

        var task = new TaskItem
        {
            Title = request.Title,
            Status = TaskStatus.Todo,
            WorkspaceId = workspaceId,
            AssignedToUserId = request.AssignedToUserId,
            DueDate = request.DueDate,
            CompanyId = request.CompanyId,
            PersonId = request.PersonId
        };

        await _taskRepository.AddAsync(task, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TaskCreatedEvent(workspaceId, task.Id, request.CompanyId, request.PersonId, userId),
            cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }

    private async Task ValidateRecordExistsAsync(CreateTaskCommand request, Guid workspaceId, CancellationToken cancellationToken)
    {
        if (request.CompanyId.HasValue)
        {
            var companies = await _companyRepository.ListAsync(
                c => c.Id == request.CompanyId.Value && c.WorkspaceId == workspaceId,
                cancellationToken);

            if (!companies.Any())
            {
                throw new KeyNotFoundException($"Company with id {request.CompanyId.Value} not found.");
            }
        }

        if (request.PersonId.HasValue)
        {
            var people = await _personRepository.ListAsync(
                p => p.Id == request.PersonId.Value && p.WorkspaceId == workspaceId,
                cancellationToken);

            if (!people.Any())
            {
                throw new KeyNotFoundException($"Person with id {request.PersonId.Value} not found.");
            }
        }
    }
}
