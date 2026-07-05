using MediatR;

namespace TwentyNet.Application.Views.ListViews;

public sealed record ListViewsQuery(string? ObjectName = null) : IRequest<IReadOnlyList<ViewDto>>;
