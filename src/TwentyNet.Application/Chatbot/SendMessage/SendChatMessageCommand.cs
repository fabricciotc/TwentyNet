using MediatR;

namespace TwentyNet.Application.Chatbot.SendMessage;

public sealed record SendChatMessageCommand(Guid SessionId, string Content) : IRequest<ChatMessageDto>;
