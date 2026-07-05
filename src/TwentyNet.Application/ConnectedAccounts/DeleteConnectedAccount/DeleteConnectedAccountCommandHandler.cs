using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ConnectedAccounts.DeleteConnectedAccount;

public sealed class DeleteConnectedAccountCommandHandler : IRequestHandler<DeleteConnectedAccountCommand, Unit>
{
    private readonly IRepository<ConnectedAccount> _repository;
    private readonly IAuthContext _authContext;

    public DeleteConnectedAccountCommandHandler(IRepository<ConnectedAccount> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(DeleteConnectedAccountCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var accounts = await _repository.ListAsync(
            a => a.Id == request.Id && a.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        if (accounts.FirstOrDefault() is null)
        {
            throw new KeyNotFoundException($"Connected account with id {request.Id} not found.");
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
