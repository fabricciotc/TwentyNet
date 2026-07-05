using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ConnectedAccounts.CreateConnectedAccount;

public sealed class CreateConnectedAccountCommandHandler : IRequestHandler<CreateConnectedAccountCommand, ConnectedAccountDto>
{
    private readonly IRepository<ConnectedAccount> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;
    private readonly ITokenEncryptionService _tokenEncryptionService;

    public CreateConnectedAccountCommandHandler(
        IRepository<ConnectedAccount> repository,
        IMapper mapper,
        IAuthContext authContext,
        ITokenEncryptionService tokenEncryptionService)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
        _tokenEncryptionService = tokenEncryptionService;
    }

    public async Task<ConnectedAccountDto> Handle(CreateConnectedAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        if (!_authContext.UserId.HasValue)
        {
            throw new UnauthorizedAccessException("User is required.");
        }

        var account = new ConnectedAccount
        {
            UserId = _authContext.UserId.Value,
            WorkspaceId = _authContext.WorkspaceId.Value,
            Provider = request.Provider,
            Email = request.Email,
            AccessToken = _tokenEncryptionService.Encrypt(request.AccessToken),
            RefreshToken = _tokenEncryptionService.Encrypt(request.RefreshToken),
            ExpiresAt = request.ExpiresAt
        };

        await _repository.AddAsync(account, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ConnectedAccountDto>(account);
    }
}
