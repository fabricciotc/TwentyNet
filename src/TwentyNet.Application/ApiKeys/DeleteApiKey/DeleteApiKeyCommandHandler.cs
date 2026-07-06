using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ApiKeys.DeleteApiKey;

public sealed class DeleteApiKeyCommandHandler : IRequestHandler<DeleteApiKeyCommand>
{
    private readonly IRepository<ApiKey> _repository;
    private readonly IAuthContext _authContext;

    public DeleteApiKeyCommandHandler(IRepository<ApiKey> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task Handle(DeleteApiKeyCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var apiKeys = await _repository.ListAsync(
            a => a.Id == request.Id && a.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var apiKey = apiKeys.FirstOrDefault()
            ?? throw new KeyNotFoundException($"API key with id {request.Id} not found.");

        await _repository.DeleteAsync(apiKey.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}
