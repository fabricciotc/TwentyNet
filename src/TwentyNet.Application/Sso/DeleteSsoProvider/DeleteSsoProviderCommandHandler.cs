using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sso.DeleteSsoProvider;

public sealed class DeleteSsoProviderCommandHandler : IRequestHandler<DeleteSsoProviderCommand>
{
    private readonly IRepository<SsoProvider> _repository;
    private readonly IAuthContext _authContext;

    public DeleteSsoProviderCommandHandler(IRepository<SsoProvider> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteSsoProviderCommand request, CancellationToken cancellationToken)
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

        await _repository.DeleteAsync(provider.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
