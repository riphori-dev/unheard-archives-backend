using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using MediatR;
using Tywynh.API.Services;
using Tywynh.Application.Confessions.Queries.GetConfessions;
using Tywynh.Application.Confessions.Queries.GetConfessionById;
using Tywynh.Application.Resonances.Commands.AddResonance;
using Tywynh.Application.Resonances.Commands.RemoveResonance;
[ApiController, Route("api/confessions")]
public class ConfessionsController(IMediator mediator, VisitorTokenService visitors, ArchiveWorkflowService workflow) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(string? category, string sort = "latest", int page = 1, int pageSize = 20, CancellationToken ct = default)
    {
        if (page < 1 || page > 100000 || pageSize < 1 || pageSize > 100 || !new[] { "latest", "resonated", "random" }.Contains(sort)) return BadRequest(new { title = "Invalid pagination or sort." });
        if (!string.IsNullOrWhiteSpace(category) && category != "all") ArchiveWorkflowService.Category(category);
        return Ok(await mediator.Send(new GetConfessionsQuery(category, sort, page, pageSize), ct));
    }
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct) => Ok(await mediator.Send(new GetConfessionByIdQuery(id), ct));
    [HttpPost, EnableRateLimiting("submission")]
    public async Task<IActionResult> Post(ArchiveWorkflowService.Submission input, CancellationToken ct) => StatusCode(201, await workflow.Submit(input, Request.Headers["Idempotency-Key"].ToString(), ct));
    [HttpPost("{id:guid}/resonate"), EnableRateLimiting("resonance")]
    public async Task<IActionResult> Resonate(Guid id, CancellationToken ct)
    {
        var hash = visitors.HashToken(visitors.GetOrCreateVisitorToken(HttpContext));
        return Ok(await mediator.Send(new AddResonanceCommand(id, null, hash), ct));
    }
    [HttpDelete("{id:guid}/resonate"), EnableRateLimiting("resonance")]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        var hash = visitors.HashToken(visitors.GetOrCreateVisitorToken(HttpContext));
        return Ok(await mediator.Send(new RemoveResonanceCommand(id, hash), ct));
    }
}
