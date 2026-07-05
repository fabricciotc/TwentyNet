using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Tasks.ListTasks;

public sealed class ListTasksQueryHandler : IRequestHandler<ListTasksQuery, IReadOnlyList<TaskDto>>
{
    private readonly IRepository<TaskItem> _taskRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListTasksQueryHandler(IRepository<TaskItem> taskRepository, IMapper mapper, IAuthContext authContext)
    {
        _taskRepository = taskRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<TaskDto>> Handle(ListTasksQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var tasks = await _taskRepository.ListAsync(
            t => t.WorkspaceId == workspaceId
                 && (!request.CompanyId.HasValue || t.CompanyId == request.CompanyId.Value)
                 && (!request.PersonId.HasValue || t.PersonId == request.PersonId.Value),
            cancellationToken);

        return _mapper.Map<IReadOnlyList<TaskDto>>(tasks);
    }
}
