using MediatR;

namespace TwentyNet.Application.Views.DeleteView;

public sealed record DeleteViewCommand(Guid Id) : IRequest<Unit>;
