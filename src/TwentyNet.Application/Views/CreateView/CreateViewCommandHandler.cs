using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Views.CreateView;

public sealed class CreateViewCommandHandler : IRequestHandler<CreateViewCommand, ViewDto>
{
    private readonly IRepository<View> _viewRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public CreateViewCommandHandler(IRepository<View> viewRepository, IMapper mapper, IAuthContext authContext)
    {
        _viewRepository = viewRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<ViewDto> Handle(CreateViewCommand request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var view = new View
        {
            WorkspaceId = _authContext.WorkspaceId.Value,
            ObjectName = request.ObjectName,
            Name = request.Name,
            IsDefault = request.IsDefault
        };

        foreach (var filter in request.Filters)
        {
            view.Filters.Add(new ViewFilter
            {
                Field = filter.Field,
                Operator = filter.Operator,
                Value = filter.Value
            });
        }

        foreach (var sort in request.Sorts)
        {
            view.Sorts.Add(new ViewSort
            {
                Field = sort.Field,
                Direction = sort.Direction
            });
        }

        await _viewRepository.AddAsync(view, cancellationToken);
        await _viewRepository.SaveChangesAsync(cancellationToken);

        return _mapper.Map<ViewDto>(view);
    }
}
