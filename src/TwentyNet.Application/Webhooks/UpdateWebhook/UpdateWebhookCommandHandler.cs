using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Webhooks.UpdateWebhook;

public sealed class UpdateWebhookCommandHandler : IRequestHandler<UpdateWebhookCommand, WebhookDto>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateWebhookCommandHandler(
        IRepository<Webhook> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WebhookDto> Handle(UpdateWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var webhooks = await _repository.ListAsync(
            w => w.Id == request.Id && w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var webhook = webhooks.FirstOrDefault()
            ?? throw new KeyNotFoundException($"Webhook with id {request.Id} not found.");

        webhook.TargetUrl = request.TargetUrl;
        webhook.Secret = request.Secret;
        webhook.Events = request.Events;
        webhook.IsActive = request.IsActive;

        _repository.Update(webhook);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WebhookDto>(webhook);
    }
}
