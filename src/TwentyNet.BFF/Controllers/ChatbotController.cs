using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwentyNet.Application.Chatbot.CreateSession;
using TwentyNet.Application.Chatbot.GetMessages;
using TwentyNet.Application.Chatbot.ListSessions;
using TwentyNet.Application.Chatbot.SendMessage;
using TwentyNet.Contracts.Chatbot;

namespace TwentyNet.BFF.Controllers;

[ApiController]
[Authorize(Policy = "RequireMember")]
[Route("api/chatbot")]
public sealed class ChatbotController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IMapper _mapper;

    public ChatbotController(ISender sender, IMapper mapper)
    {
        _sender = sender;
        _mapper = mapper;
    }

    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<ChatSessionResponse>>> ListSessions(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new ListChatSessionsQuery(), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ChatSessionResponse>>(result));
    }

    [HttpPost("sessions")]
    public async Task<ActionResult<ChatSessionResponse>> CreateSession(
        [FromBody] CreateChatSessionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new CreateChatSessionCommand(request.Title), cancellationToken);
        return CreatedAtAction(nameof(GetMessages), new { sessionId = result.Id }, _mapper.Map<ChatSessionResponse>(result));
    }

    [HttpGet("sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<IReadOnlyList<ChatMessageResponse>>> GetMessages(
        Guid sessionId,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetChatMessagesQuery(sessionId), cancellationToken);
        return Ok(_mapper.Map<IReadOnlyList<ChatMessageResponse>>(result));
    }

    [HttpPost("sessions/{sessionId:guid}/messages")]
    public async Task<ActionResult<ChatMessageResponse>> SendMessage(
        Guid sessionId,
        [FromBody] SendChatMessageRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new SendChatMessageCommand(sessionId, request.Content), cancellationToken);
        return Ok(_mapper.Map<ChatMessageResponse>(result));
    }
}
