using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Chatbot.ListSessions;

public sealed class ListChatSessionsQueryHandler : IRequestHandler<ListChatSessionsQuery, IReadOnlyList<ChatSessionDto>>
{
    private readonly IRepository<ChatSession> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListChatSessionsQueryHandler(
        IRepository<ChatSession> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<ChatSessionDto>> Handle(ListChatSessionsQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue || !_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace and user are required.");
        }

        var sessions = await _repository.ListAsync(
            s => s.WorkspaceId == _authContext.WorkspaceId.Value && s.UserId == _authContext.UserId.Value,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ChatSessionDto>>(sessions);
    }
}
