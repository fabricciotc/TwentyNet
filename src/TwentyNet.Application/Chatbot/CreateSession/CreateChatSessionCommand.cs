using MediatR;

namespace TwentyNet.Application.Chatbot.CreateSession;

public sealed record CreateChatSessionCommand(string Title) : IRequest<ChatSessionDto>;
