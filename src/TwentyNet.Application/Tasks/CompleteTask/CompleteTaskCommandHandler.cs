using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TaskStatus = TwentyNet.Domain.Enums.TaskStatus;
using TwentyNet.Domain.Events;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.CompleteTask;

public sealed class CompleteTaskCommandHandler : IRequestHandler<CompleteTaskCommand, TaskDto>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly IPublisher _publisher;

    public CompleteTaskCommandHandler(
        IRepository<TaskItem> taskRepository,
        IMapper mapper,
        IAuthContext authContext,
        IPublisher publisher)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _authContext = authContext;
        _publisher = publisher;
    }

    public async Task<TaskDto> Handle(CompleteTaskCommand request, CancellationToken cancellationToken)
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

        var tasks = await _taskRepository.ListAsync(
            t => t.Id == request.Id && t.WorkspaceId == workspaceId,
            cancellationToken);

        var task = tasks.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Task with id {request.Id} not found.");

        task.Status = TaskStatus.Done;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new TaskCompletedEvent(workspaceId, task.Id, task.CompanyId, task.PersonId, userId),
            cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }
}
