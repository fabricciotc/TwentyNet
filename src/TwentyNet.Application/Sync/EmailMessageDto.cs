namespace TwentyNet.Application.Sync;

public sealed record EmailMessageDto(
    Guid Id,
    Guid ConnectedAccountId,
    string ExternalId,
    string ThreadId,
    string Subject,
    string Body,
    string FromAddress,
    string ToAddresses,
    DateTime ReceivedAt,
    bool IsRead,
    DateTime CreatedAt,
    DateTime UpdatedAt);
