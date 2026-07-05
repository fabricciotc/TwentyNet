using AutoMapper;
using MediatR;
using TwentyNet.Domain.Entities;
using TwentyNet.Domain.Interfaces;

namespace TwentyNet.Application.Timeline.GetTimeline;

public sealed class GetTimelineQueryHandler : IRequestHandler<GetTimelineQuery, IReadOnlyList<TimelineActivityDto>>
{
    private readonly IRepository<TimelineActivity> _timelineRepository;
    private readonly IMapper _mapper;
    private readonly IAuthContext _authContext;

    public GetTimelineQueryHandler(
        IRepository<TimelineActivity> timelineRepository,
        IMapper mapper,
        IAuthContext authContext)
    {
        _timelineRepository = timelineRepository;
        _mapper = mapper;
        _authContext = authContext;
    }

    public async Task<IReadOnlyList<TimelineActivityDto>> Handle(GetTimelineQuery request, CancellationToken cancellationToken)
    {
        if (!_authContext.WorkspaceId.HasValue)
        {
            throw new UnauthorizedAccessException("Workspace is required.");
        }

        var activities = await _timelineRepository.ListAsync(
            a => a.WorkspaceId == _authContext.WorkspaceId.Value
                 && a.ObjectName == request.ObjectName
                 && a.RecordId == request.RecordId,
            cancellationToken);

        var ordered = activities.OrderByDescending(a => a.CreatedAt).ToList();
        return _mapper.Map<IReadOnlyList<TimelineActivityDto>>(ordered);
    }
}
