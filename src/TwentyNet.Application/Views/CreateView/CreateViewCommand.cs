using MediatR;
using TwentyNet.Application.Common;

namespace TwentyNet.Application.Views.CreateView;

public sealed record CreateViewCommand(
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<FilterInput> Filters,
    IReadOnlyList<SortInput> Sorts) : IRequest<ViewDto>;
