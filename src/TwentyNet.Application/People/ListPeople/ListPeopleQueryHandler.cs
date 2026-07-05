using AutoMapper;
using MediatR;
using TwentyNet.Application.Common;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.People.ListPeople;

public sealed class ListPeopleQueryHandler : IRequestHandler<ListPeopleQuery, PagedResult<PersonDto>>
{
    private readonly IRepository<Person> _personRepository;
    private readonly IRepository<View> _viewRepository;
    private readonly IRepository<ViewFilter> _filterRepository;
    private readonly IRepository<ViewSort> _sortRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public ListPeopleQueryHandler(
        IRepository<Person> personRepository,
        IRepository<View> viewRepository,
        IRepository<ViewFilter> filterRepository,
        IRepository<ViewSort> sortRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _personRepository = personRepository;
        _viewRepository = viewRepository;
        _filterRepository = filterRepository;
        _sortRepository = sortRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<PagedResult<PersonDto>> Handle(ListPeopleQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var workspaceId = _authContext.WorkspaceId.Value;

        var filters = request.Filters?.ToList() ?? new List<FilterInput>();
        var sorts = request.Sorts?.ToList() ?? new List<SortInput>();

        if (request.ViewId.HasValue)
        {
            var view = (await _viewRepository.ListAsync(
                v => v.Id == request.ViewId.Value && v.WorkspaceId == workspaceId,
                cancellationToken)).FirstOrDefault();

            if (view is not null)
            {
                var viewFilters = await _filterRepository.ListAsync(f => f.ViewId == view.Id, cancellationToken);
                var viewSorts = await _sortRepository.ListAsync(s => s.ViewId == view.Id, cancellationToken);

                filters = filters
                    .Concat(viewFilters.Select(f => new FilterInput(f.Field, f.Operator, f.Value)))
                    .ToList();

                if (!sorts.Any())
                {
                    sorts = viewSorts.Select(s => new SortInput(s.Field, s.Direction)).ToList();
                }
            }
        }

        var query = _personRepository.AsQueryable().Where(p => p.WorkspaceId == workspaceId);
        query = DynamicFilterApplicator.ApplyFilters(query, filters);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            query = DynamicFilterApplicator.ApplySearch(query, request.Search);
        }

        var totalCount = await _personRepository.CountAsync(query, cancellationToken);

        query = DynamicSorter.ApplySorts(query, sorts);

        var items = await _personRepository.ToListAsync(
            query.Skip(request.Skip).Take(request.Take),
            cancellationToken);

        var dtos = _mapper.Map<IReadOnlyList<PersonDto>>(items);
        return new PagedResult<PersonDto>(dtos, totalCount, request.Skip, request.Take);
    }
}
