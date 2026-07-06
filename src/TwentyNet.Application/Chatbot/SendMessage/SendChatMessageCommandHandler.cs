using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Chatbot.SendMessage;

public sealed class SendChatMessageCommandHandler : IRequestHandler<SendChatMessageCommand, ChatMessageDto>
{
    private readonly IRepository<ChatSession> _sessionRepository;
    private readonly IRepository<ChatMessage> _messageRepository;
    private readonly IChatbotProvider _chatbotProvider;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public SendChatMessageCommandHandler(
        IRepository<ChatSession> sessionRepository,
        IRepository<ChatMessage> messageRepository,
        IChatbotProvider chatbotProvider,
        IMapper mapper,
        IAuthContext authContext)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _chatbotProvider = chatbotProvider;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<ChatMessageDto> Handle(SendChatMessageCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue || !_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace and user are required.");
        }

        var sessions = await _sessionRepository.ListAsync(
            s => s.Id == request.SessionId
                 && s.WorkspaceId == _authContext.WorkspaceId.Value
                 && s.UserId == _authContext.UserId.Value,
            cancellationToken);

        var session = sessions.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Chat session {request.SessionId} not found.");

        var userMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "user",
            Content = request.Content
        };

        await _messageRepository.AddAsync(userMessage, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        var history = session.Messages
            .Select(m => new ChatMessageInput(m.Role, m.Content))
            .ToList();

        var answer = await _chatbotProvider.AskAsync(history, cancellationToken);

        var assistantMessage = new ChatMessage
        {
            SessionId = session.Id,
            Role = "assistant",
            Content = answer
        };

        await _messageRepository.AddAsync(assistantMessage, cancellationToken);
        await _messageRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ChatMessageDto>(assistantMessage);
    }
}
