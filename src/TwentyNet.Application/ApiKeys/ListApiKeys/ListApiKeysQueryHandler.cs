using System.Linq.Expressions;
using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.ApiKeys.ListApiKeys;

public sealed class ListApiKeysQueryHandler : IRequestHandler<ListApiKeysQuery, IReadOnlyList<ApiKeyDto>>
{
    private readonly IRepository<ApiKey> _repository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListApiKeysQueryHandler(
        IRepository<ApiKey> repository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _repository = repository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<ApiKeyDto>> Handle(ListApiKeysQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        Expression<Func<ApiKey, bool>> predicate = request.IsActive.HasValue
            ? a => a.WorkspaceId == _authContext.WorkspaceId.Value && a.IsActive == request.IsActive.Value
            : a => a.WorkspaceId == _authContext.WorkspaceId.Value;

        var apiKeys = await _repository.ListAsync(predicate, cancellationToken);
        return _mapper.Map<IReadOnlyList<ApiKeyDto>>(apiKeys);
    }
}
