using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Billing.ListPlans;

public sealed class ListSubscriptionPlansQueryHandler : IRequestHandler<ListSubscriptionPlansQuery, IReadOnlyList<SubscriptionPlanDto>>
{
    private readonly IRepository<SubscriptionPlan> _repository;
    private readonly IMapper _mapper;

    public ListSubscriptionPlansQueryHandler(IRepository<SubscriptionPlan> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<SubscriptionPlanDto>> Handle(ListSubscriptionPlansQuery request, CancellationToken cancellationToken)
    {
        var plans = await _repository.ListAsync(p => p.IsActive, cancellationToken);
        return _mapper.Map<IReadOnlyList<SubscriptionPlanDto>>(plans);
    }
}
