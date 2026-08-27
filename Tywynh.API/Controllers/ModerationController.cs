using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tywynh.API.Filters;
using Tywynh.Application.Confessions.Queries.GetModeratedConfessions;
using Tywynh.Application.Confessions.Commands.ApproveConfession;
using Tywynh.Application.Confessions.Commands.RejectConfession;
using Tywynh.Application.Confessions.Commands.SoftDeleteConfession;

[ApiController]
[Route("api/moderation")]
[AdminKeyAuth]
public class ModerationController : ControllerBase
{
    private readonly IMediator _mediator;
    public ModerationController(IMediator mediator) { _mediator = mediator; }

    [HttpGet("confessions")]
    public async Task<IActionResult> Get([FromQuery] string status = "pending")
    {
        var result = await _mediator.Send(new GetModeratedConfessionsQuery(status));
        return Ok(result);
    }

    [HttpPost("confessions/{id:guid}/approve")]
    public async Task<IActionResult> Approve(Guid id)
    {
        await _mediator.Send(new ApproveConfessionCommand(id));
        return NoContent();
    }

    public record RejectRequest(string? Reason);

    [HttpPost("confessions/{id:guid}/reject")]
    public async Task<IActionResult> Reject(Guid id, [FromBody] RejectRequest req)
    {
        await _mediator.Send(new RejectConfessionCommand(id, req.Reason));
        return NoContent();
    }

    [HttpDelete("confessions/{id:guid}")]
    public async Task<IActionResult> SoftDelete(Guid id)
    {
        await _mediator.Send(new SoftDeleteConfessionCommand(id));
        return NoContent();
    }
}
