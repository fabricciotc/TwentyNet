using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Timeline.GetTimeline;
using TwentyNet.Contracts.Timeline;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api")]
public sealed class TimelineController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public TimelineController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet("companies/{id:guid}/timeline")]
    public async Task<ActionResult<IReadOnlyList<TimelineActivityResponse>>> GetCompanyTimeline(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTimelineQuery("Company", id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<TimelineActivityResponse>>(result));
    }

    [HttpGet("people/{id:guid}/timeline")]
    public async Task<ActionResult<IReadOnlyList<TimelineActivityResponse>>> GetPersonTimeline(Guid id, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetTimelineQuery("Person", id), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<TimelineActivityResponse>>(result));
    }
}
