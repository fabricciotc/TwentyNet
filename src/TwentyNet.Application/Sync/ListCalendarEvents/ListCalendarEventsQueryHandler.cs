using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Sync.ListCalendarEvents;

public sealed class ListCalendarEventsQueryHandler : IRequestHandler<ListCalendarEventsQuery, IReadOnlyList<CalendarEventDto>>
{
    private readonly IRepository<CalendarEvent> _repository;
    private readonly IMapper _mapper;

    public ListCalendarEventsQueryHandler(IRepository<CalendarEvent> repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<CalendarEventDto>> Handle(ListCalendarEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await _repository.ListAsync(
            e => e.ConnectedAccountId == request.ConnectedAccountId,
            cancellationToken);

        return _mapper.Map<IReadOnlyList<CalendarEventDto>>(events);
    }
}
