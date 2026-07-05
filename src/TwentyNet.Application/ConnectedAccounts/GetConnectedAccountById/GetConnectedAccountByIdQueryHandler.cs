using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ConnectedAccounts.GetConnectedAccountById;

public sealed class GetConnectedAccountByIdQueryHandler : IRequestHandler<GetConnectedAccountByIdQuery, ConnectedAccountDto?>
{
    private readonly IRepository<ConnectedAccount> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetConnectedAccountByIdQueryHandler(
        IRepository<ConnectedAccount> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<ConnectedAccountDto?> Handle(GetConnectedAccountByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var accounts = await _repository.ListAsync(
            a => a.Id == request.Id && a.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var account = accounts.FirstOrDefault();
        return account is null ? null : _mapper.Map<ConnectedAccountDto>(account);
    }
}
