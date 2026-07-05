using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Webhooks.ListWebhooks;

public sealed class ListWebhooksQueryHandler : IRequestHandler<ListWebhooksQuery, IReadOnlyList<WebhookDto>>
{
    private readonly IRepository<Webhook> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListWebhooksQueryHandler(
        IRepository<Webhook> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<WebhookDto>> Handle(ListWebhooksQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var webhooks = await _repository.ListAsync(
            w => w.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<WebhookDto>>(webhooks);
    }
}
