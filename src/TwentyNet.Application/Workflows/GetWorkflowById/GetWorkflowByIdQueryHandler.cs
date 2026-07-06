using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Workflows.GetWorkflowById;

public sealed class GetWorkflowByIdQueryHandler : IRequestHandler<GetWorkflowByIdQuery, WorkflowDto?>
{
    private readonly IRepository<Workflow> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetWorkflowByIdQueryHandler(
        IRepository<Workflow> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WorkflowDto?> Handle(GetWorkflowByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workflows = await _repository.ListAsync(
            w => w.Id == request.Id && w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var workflow = workflows.FirstOrDefault();
        return workflow is null ? null : _mapper.Map<WorkflowDto>(workflow);
    }
}
