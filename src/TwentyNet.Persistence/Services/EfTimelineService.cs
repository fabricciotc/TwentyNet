using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Persistence.Services;

public sealed class EfTimelineService : ITimelineService
{
    private readonly IRepository<TimelineActivity> _repository;

    public EfTimelineService(IRepository<TimelineActivity> repository)
    {
        _repository = repository;
    }

    public async Task AddActivityAsync(TimelineActivity activity, CancellationToken cancellationToken = default)
    {
        await _repository.AddAsync(activity, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
