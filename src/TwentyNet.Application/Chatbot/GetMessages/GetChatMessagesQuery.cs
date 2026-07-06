using MediatR;

namespace TwentyNet.Application.Chatbot.GetMessages;

public sealed record GetChatMessagesQuery(Guid SessionId) : IRequest<IReadOnlyList<ChatMessageDto>>;
