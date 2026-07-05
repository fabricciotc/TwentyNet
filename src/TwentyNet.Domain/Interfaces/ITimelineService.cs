using TwentyNet.Domain.Entities;

namespace TwentyNet.Domain.Interfaces;

public interface ITimelineService
{
    Task AddActivityAsync(TimelineActivity activity, CancellationToken cancellationToken = default);
}
