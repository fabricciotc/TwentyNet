using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sso.ListSsoProviders;

public sealed class ListSsoProvidersQueryHandler : IRequestHandler<ListSsoProvidersQuery, IReadOnlyList<SsoProviderDto>>
{
    private readonly IRepository<SsoProvider> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListSsoProvidersQueryHandler(
        IRepository<SsoProvider> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<SsoProviderDto>> Handle(ListSsoProvidersQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var providers = await _repository.ListAsync(
            s => s.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<SsoProviderDto>>(providers);
    }
}
