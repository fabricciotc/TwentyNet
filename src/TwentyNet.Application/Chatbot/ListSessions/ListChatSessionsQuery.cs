using MediatR;

namespace TwentyNet.Application.Chatbot.ListSessions;

public sealed record ListChatSessionsQuery : IRequest<IReadOnlyList<ChatSessionDto>>;
