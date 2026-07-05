using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Views.ListViews;

public sealed class ListViewsQueryHandler : IRequestHandler<ListViewsQuery, IReadOnlyList<ViewDto>>
{
    private readonly IRepository<View> _viewRepository;
    private readonly IRepository<ViewFilter> _filterRepository;
    private readonly IRepository<ViewSort> _sortRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListViewsQueryHandler(
        IRepository<View> viewRepository,
        IRepository<ViewFilter> filterRepository,
        IRepository<ViewSort> sortRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _viewRepository = viewRepository;
        _filterRepository = filterRepository;
        _sortRepository = sortRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<ViewDto>> Handle(ListViewsQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var views = await _viewRepository.ListAsync(
            v => v.WorkspaceId == workspaceId && (request.ObjectName == null || v.ObjectName == request.ObjectName),
            cancellationToken);

        var result = new List<ViewDto>();
        foreach (var view in views)
        {
            var filters = await _filterRepository.ListAsync(f => f.ViewId == view.Id, cancellationToken);
            var sorts = await _sortRepository.ListAsync(s => s.ViewId == view.Id, cancellationToken);

            result.Add(_mapper.Map<ViewDto>(view) with
            {
                Filters = _mapper.Map<IReadOnlyList<ViewFilterDto>>(filters),
                Sorts = _mapper.Map<IReadOnlyList<ViewSortDto>>(sorts)
            });
        }

        return result;
    }
}
