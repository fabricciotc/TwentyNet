using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Webhooks.CreateWebhook;

public sealed class CreateWebhookCommandHandler : IRequestHandler<CreateWebhookCommand, WebhookDto>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public CreateWebhookCommandHandler(
        IRepository<Webhook> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WebhookDto> Handle(CreateWebhookCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var webhook = new Webhook
        {
            WorkspaceId = _authContext.WorkspaceId.Value,
            TargetUrl = request.TargetUrl,
            Secret = request.Secret,
            Events = request.Events
        };

        await _repository.AddAsync(webhook, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<WebhookDto>(webhook);
    }
}
