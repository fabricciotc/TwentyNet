using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.GetSubscription;

public sealed class GetWorkspaceSubscriptionQueryHandler : IRequestHandler<GetWorkspaceSubscriptionQuery, WorkspaceSubscriptionDto?>
{
    private readonly IRepository<WorkspaceSubscription> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetWorkspaceSubscriptionQueryHandler(
        IRepository<WorkspaceSubscription> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WorkspaceSubscriptionDto?> Handle(GetWorkspaceSubscriptionQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var subscriptions = await _repository.ListAsync(
            s => s.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var subscription = subscriptions.OrderByDescending(s => s.CreatedAt).FirstOrDefault();
        return subscription is null ? null : _mapper.Map<WorkspaceSubscriptionDto>(subscription);
    }
}
