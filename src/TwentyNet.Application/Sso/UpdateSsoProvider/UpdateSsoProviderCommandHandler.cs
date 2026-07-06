using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sso.UpdateSsoProvider;

public sealed class UpdateSsoProviderCommandHandler : IRequestHandler<UpdateSsoProviderCommand, SsoProviderDto>
{
    private readonly IRepository<SsoProvider> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateSsoProviderCommandHandler(
        IRepository<SsoProvider> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<SsoProviderDto> Handle(UpdateSsoProviderCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var providers = await _repository.ListAsync(
            s => s.Id == request.Id && s.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var provider = providers.FirstOrDefault()
            ?? throw new KeyNotFoundException($"SSO provider with id {request.Id} not found.");

        provider.Name = request.Name;
        provider.IsActive = request.IsActive;
        provider.Type = request.Type;
        provider.ClientId = request.ClientId;
        provider.ClientSecret = request.ClientSecret;
        provider.AuthorizationEndpoint = request.AuthorizationEndpoint;
        provider.TokenEndpoint = request.TokenEndpoint;
        provider.UserInfoEndpoint = request.UserInfoEndpoint;
        provider.EntityId = request.EntityId;
        provider.SingleSignOnUrl = request.SingleSignOnUrl;
        provider.Certificate = request.Certificate;
        provider.MetadataUrl = request.MetadataUrl;

        _repository.Update(provider);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<SsoProviderDto>(provider);
    }
}
