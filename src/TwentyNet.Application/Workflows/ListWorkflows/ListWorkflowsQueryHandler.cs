using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workflows.ListWorkflows;

public sealed class ListWorkflowsQueryHandler : IRequestHandler<ListWorkflowsQuery, IReadOnlyList<WorkflowDto>>
{
    private readonly IRepository<Workflow> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListWorkflowsQueryHandler(
        IRepository<Workflow> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<WorkflowDto>> Handle(ListWorkflowsQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        Expression<Func<Workflow, bool>> predicate = request.IsActive.HasValue
            ? w => w.WorkspaceId == _authContext.WorkspaceId.Value && w.IsActive == request.IsActive.Value
            : w => w.WorkspaceId == _authContext.WorkspaceId.Value;

        var workflows = await _repository.ListAsync(predicate, cancellationToken);
        return _mapper.Map<IReadOnlyList<WorkflowDto>>(workflows);
    }
}
