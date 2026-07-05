using TwentyNet.Domain.Common;

namespace TwentyNet.Domain.Interfaces;

public interface IRealTimeNotifier
{
    Task NotifyAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
