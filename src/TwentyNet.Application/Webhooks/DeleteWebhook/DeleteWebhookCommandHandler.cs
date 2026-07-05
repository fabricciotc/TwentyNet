using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Webhooks.DeleteWebhook;

public sealed class DeleteWebhookCommandHandler : IRequestHandler<DeleteWebhookCommand, Unit>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IAuthContext _authContext;

    public DeleteWebhookCommandHandler(IRepository<Webhook> repository, IAuthContext authContext)
    {
        _repository = repository;
        _authContext = authContext;
    }

    public async Task<Unit> Handle(DeleteWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var webhooks = await _repository.ListAsync(
            w => w.Id == request.Id && w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        if (webhooks.FirstOrDefault() is null)
        {
            throw new KeyNotFoundException($"Webhook with id {request.Id} not found.");
        }

        await _repository.DeleteAsync(request.Id, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
