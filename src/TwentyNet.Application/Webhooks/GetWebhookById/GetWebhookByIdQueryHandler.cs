using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Webhooks.GetWebhookById;

public sealed class GetWebhookByIdQueryHandler : IRequestHandler<GetWebhookByIdQuery, WebhookDto?>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetWebhookByIdQueryHandler(
        IRepository<Webhook> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<WebhookDto?> Handle(GetWebhookByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var webhooks = await _repository.ListAsync(
            w => w.Id == request.Id && w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var webhook = webhooks.FirstOrDefault();
        return webhook is null ? null : _mapper.Map<WebhookDto>(webhook);
    }
}
