using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Enums;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.SubscribeWorkspace;

public sealed class SubscribeWorkspaceCommandHandler : IRequestHandler<SubscribeWorkspaceCommand, WorkspaceSubscriptionDto>
{
    private readonly IRepository<WorkspaceSubscription> _subscriptionRepository;
    private readonly IRepository<SubscriptionPlan> _planRepository;
    private readonly IBillingService _billingService;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public SubscribeWorkspaceCommandHandler(
        IRepository<WorkspaceSubscription> subscriptionRepository,
        IRepository<SubscriptionPlan> planRepository,
        IBillingService billingService,
        IMapper mapper,
        IAuthContext authContext)
    {
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
        _billingService = billingService;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WorkspaceSubscriptionDto> Handle(SubscribeWorkspaceCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var plans = await _planRepository.ListAsync(
            p => p.Id == request.PlanId && p.IsActive,
            cancellationToken);
        var plan = plans.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Plan {request.PlanId} not found or inactive.");

        var result = await _billingService.CreateSubscriptionAsync(workspaceId, plan.Id, cancellationToken);

        var subscription = new WorkspaceSubscription
        {
            WorkspaceId = workspaceId,
            PlanId = plan.Id,
            Status = result.Status,
            StartedAt = DateTime.UtcNow,
            EndsAt = result.EndsAt,
            ExternalSubscriptionId = result.ExternalSubscriptionId
        };

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _subscriptionRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WorkspaceSubscriptionDto>(subscription);
    }
}
