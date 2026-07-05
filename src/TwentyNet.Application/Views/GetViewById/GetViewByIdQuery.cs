using MediatR;

namespace TwentyNet.Application.Views.GetViewById;

public sealed record GetViewByIdQuery(Guid Id) : IRequest<ViewDto?>;
