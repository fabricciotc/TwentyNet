using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.GetTaskById;

public sealed class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskDto?>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetTaskByIdQueryHandler(IRepository<TaskItem> taskRepository, IMapper mapper, IAuthContext authContext)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<TaskDto?> Handle(GetTaskByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var tasks = await _taskRepository.ListAsync(
            t => t.Id == request.Id && t.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var task = tasks.FirstOrDefault();
        return task is null ? null : _mapper.Map<TaskDto>(task);
    }
}
