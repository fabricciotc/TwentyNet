using MediatR;

namespace TwentyNet.Application.ApiKeys.DeleteApiKey;

public sealed record DeleteApiKeyCommand(Guid Id) : IRequest;
