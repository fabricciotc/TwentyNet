using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Views.UpdateView;

public sealed class UpdateViewCommandHandler : IRequestHandler<UpdateViewCommand, ViewDto>
{
    private readonly IRepository<View> _viewRepository;
    private readonly IRepository<ViewFilter> _filterRepository;
    private readonly IRepository<ViewSort> _sortRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public UpdateViewCommandHandler(
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

    public async Task<ViewDto> Handle(UpdateViewCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var view = (await _viewRepository.ListAsync(
            v => v.Id == request.Id && v.WorkspaceId == workspaceId,
            cancellationToken)).FirstOrDefault()
            ?? throw new KeyNotFoundException($"View with id {request.Id} not found.");

        view.ObjectName = request.ObjectName;
        view.Name = request.Name;
        view.IsDefault = request.IsDefault;

        var existingFilters = await _filterRepository.ListAsync(f => f.ViewId == view.Id, cancellationToken);
        foreach (var filter in existingFilters)
        {
            await _filterRepository.DeleteAsync(filter.Id, cancellationToken);
        }

        view.Filters.Clear();
        foreach (var filter in request.Filters)
        {
            view.Filters.Add(new ViewFilter
            {
                ViewId = view.Id,
                Field = filter.Field,
                Operator = filter.Operator,
                Value = filter.Value
            });
        }

        var existingSorts = await _sortRepository.ListAsync(s => s.ViewId == view.Id, cancellationToken);
        foreach (var sort in existingSorts)
        {
            await _sortRepository.DeleteAsync(sort.Id, cancellationToken);
        }

        view.Sorts.Clear();
        foreach (var sort in request.Sorts)
        {
            view.Sorts.Add(new ViewSort
            {
                ViewId = view.Id,
                Field = sort.Field,
                Direction = sort.Direction
            });
        }

        _viewRepository.Update(view);
        await _viewRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ViewDto>(view);
    }
}
