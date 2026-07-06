using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ApiKeys.GetApiKeyById;

public sealed class GetApiKeyByIdQueryHandler : IRequestHandler<GetApiKeyByIdQuery, ApiKeyDto?>
{
    private readonly IRepository<ApiKey> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetApiKeyByIdQueryHandler(
        IRepository<ApiKey> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<ApiKeyDto?> Handle(GetApiKeyByIdQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var apiKeys = await _repository.ListAsync(
            a => a.Id == request.Id && a.WorkspaceId == _authContext.WorkspaceId.Value,
            cancellationToken);

        var apiKey = apiKeys.FirstOrDefault();
        return apiKey is null ? null : _mapper.Map<ApiKeyDto>(apiKey);
    }
}
