using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ConnectedAccounts.ListConnectedAccounts;

public sealed class ListConnectedAccountsQueryHandler : IRequestHandler<ListConnectedAccountsQuery, IReadOnlyList<ConnectedAccountDto>>
{
    private readonly IRepository<ConnectedAccount> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListConnectedAccountsQueryHandler(
        IRepository<ConnectedAccount> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<ConnectedAccountDto>> Handle(ListConnectedAccountsQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var accounts = await _repository.ListAsync(
            a => a.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ConnectedAccountDto>>(accounts);
    }
}
