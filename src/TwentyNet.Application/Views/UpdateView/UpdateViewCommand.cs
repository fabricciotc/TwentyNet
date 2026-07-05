using MediatR;
using TwentyNet.Application.Common;

namespace TwentyNet.Application.Views.UpdateView;

public sealed record UpdateViewCommand(
    Guid Id,
    string ObjectName,
    string Name,
    bool IsDefault,
    IReadOnlyList<FilterInput> Filters,
    IReadOnlyList<SortInput> Sorts) : IRequest<ViewDto>;
