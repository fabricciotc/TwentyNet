using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.UpdateTask;

public sealed class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskDto>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateTaskCommandHandler(
        IRepository<TaskItem> taskRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<TaskDto> Handle(UpdateTaskCommand request, CancellationToken cancellationToken)
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

        task.Title = request.Title;
        task.Status = request.Status;
        task.AssignedToUserId = request.AssignedToUserId;
        task.DueDate = request.DueDate;

        _taskRepository.Update(task);
        await _taskRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<TaskDto>(task);
    }
}
