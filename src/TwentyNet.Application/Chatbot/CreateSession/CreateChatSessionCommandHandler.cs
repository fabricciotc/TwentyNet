using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Chatbot.CreateSession;

public sealed class CreateChatSessionCommandHandler : IRequestHandler<CreateChatSessionCommand, ChatSessionDto>
{
    private readonly IRepository<ChatSession> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public CreateChatSessionCommandHandler(
        IRepository<ChatSession> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<ChatSessionDto> Handle(CreateChatSessionCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue || !_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace and user are required.");
        }

        var session = new ChatSession
        {
            Title = request.Title,
            WorkspaceId = _authContext.WorkspaceId.Value,
            UserId = _authContext.UserId.Value
        };

        await _repository.AddAsync(session, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ChatSessionDto>(session);
    }
}
