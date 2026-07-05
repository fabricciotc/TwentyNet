using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.DeleteTask;

public sealed class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IAuthContext _authContext;

    public DeleteTaskCommandHandler(IRepository<TaskItem> taskRepository, IAuthContext authContext)
    {
        _taskRepository = taskRepository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var tasks = await _taskRepository.ListAsync(
            t => t.Id == request.Id && t.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var task = tasks.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Task with id {request.Id} not found.");

        await _taskRepository.DeleteAsync(task.Id, cancellationToken);
        await _taskRepository.SaveChangesAsync(cancellationToken);
    }
}
