using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sso.GetSsoProviderById;

public sealed class GetSsoProviderByIdQueryHandler : IRequestHandler<GetSsoProviderByIdQuery, SsoProviderDto?>
{
    private readonly IRepository<SsoProvider> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetSsoProviderByIdQueryHandler(
        IRepository<SsoProvider> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<SsoProviderDto?> Handle(GetSsoProviderByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var providers = await _repository.ListAsync(
            s => s.Id == request.Id && s.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var provider = providers.FirstOrDefault();
        return provider is null ? null : _mapper.Map<SsoProviderDto>(provider);
    }
}
