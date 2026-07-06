using MediatR;

namespace TwentyNet.Application.Sync.ListEmailMessages;

public sealed record ListEmailMessagesQuery(Guid ConnectedAccountId) : IRequest<IReadOnlyList<EmailMessageDto>>;
