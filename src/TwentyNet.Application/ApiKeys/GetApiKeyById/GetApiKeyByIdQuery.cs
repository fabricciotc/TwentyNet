using MediatR;

namespace TwentyNet.Application.ApiKeys.GetApiKeyById;

public sealed record GetApiKeyByIdQuery(Guid Id) : IRequest<ApiKeyDto?>;
