using MediatR;

namespace TwentyNet.Application.Tasks.DeleteTask;

public sealed record DeleteTaskCommand(Guid Id) : IRequest;
