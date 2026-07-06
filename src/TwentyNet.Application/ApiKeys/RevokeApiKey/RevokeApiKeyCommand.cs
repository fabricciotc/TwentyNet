using MediatR;

namespace TwentyNet.Application.ApiKeys.RevokeApiKey;

public sealed record RevokeApiKeyCommand(Guid Id) : IRequest;
