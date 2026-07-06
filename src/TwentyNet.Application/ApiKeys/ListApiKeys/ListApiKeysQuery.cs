using MediatR;

namespace TwentyNet.Application.ApiKeys.ListApiKeys;

public sealed record ListApiKeysQuery(bool? IsActive = null) : IRequest<IReadOnlyList<ApiKeyDto>>;
