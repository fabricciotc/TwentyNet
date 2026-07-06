using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Chatbot.GetMessages;

public sealed class GetChatMessagesQueryHandler : IRequestHandler<GetChatMessagesQuery, IReadOnlyList<ChatMessageDto>>
{
    private readonly IRepository<ChatSession> _sessionRepository;
    private readonly IRepository<ChatMessage> _messageRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetChatMessagesQueryHandler(
        IRepository<ChatSession> sessionRepository,
        IRepository<ChatMessage> messageRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _sessionRepository = sessionRepository;
        _messageRepository = messageRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<ChatMessageDto>> Handle(GetChatMessagesQuery request, CancellationToken cancellationToken)
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

        if (sessions.Count == 0)
        {
            throw new KeyNotFoundException($"Chat session {request.SessionId} not found.");
        }

        var messages = await _messageRepository.ListAsync(
            m => m.SessionId == request.SessionId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ChatMessageDto>>(messages);
    }
}
