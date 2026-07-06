using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sso.CreateSsoProvider;

public sealed class CreateSsoProviderCommandHandler : IRequestHandler<CreateSsoProviderCommand, SsoProviderDto>
{
    private readonly IRepository<SsoProvider> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public CreateSsoProviderCommandHandler(
        IRepository<SsoProvider> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<SsoProviderDto> Handle(CreateSsoProviderCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var provider = new SsoProvider
        {
            Name = request.Name,
            WorkspaceId = _authContext.WorkspaceId.Value,
            Type = request.Type,
            ClientId = request.ClientId,
            ClientSecret = request.ClientSecret,
            AuthorizationEndpoint = request.AuthorizationEndpoint,
            TokenEndpoint = request.TokenEndpoint,
            UserInfoEndpoint = request.UserInfoEndpoint,
            EntityId = request.EntityId,
            SingleSignOnUrl = request.SingleSignOnUrl,
            Certificate = request.Certificate,
            MetadataUrl = request.MetadataUrl
        };

        await _repository.AddAsync(provider, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SsoProviderDto>(provider);
    }
}
