using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tywynh.Application.Confessions.Queries.GetConfessions;
using Tywynh.Application.Confessions.Queries.GetConfessionById;
using Tywynh.Application.Confessions.Commands.CreateConfession;
using Tywynh.Domain.Enums;
using Tywynh.API.Services;
using Tywynh.Application.Resonances.Commands.AddResonance;
using Tywynh.Application.Resonances.Commands.RemoveResonance;
using Tywynh.Application.Resonances.DTOs;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/confessions")]
public class ConfessionsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly VisitorTokenService _visitorTokenService;

    public ConfessionsController(IMediator mediator, VisitorTokenService visitorTokenService)
    {
        _mediator = mediator;
        _visitorTokenService = visitorTokenService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? category, [FromQuery] string sort = "latest", [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetConfessionsQuery(category, sort, page, Math.Clamp(pageSize, 1, 100)));
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.Send(new GetConfessionByIdQuery(id));
        return Ok(result);
    }

    public record SubmitConfessionRequest(string Text, string Category, short Intensity);
    public record SubmitConfessionResponseDto(Guid Id);

    [HttpPost]
        public async Task<IActionResult> Post([FromBody] SubmitConfessionRequest req)
    {
        if (!Enum.TryParse<ConfessionCategory>(req.Category, true, out var parsed))
            return BadRequest("Invalid category");
        var cmd = new Tywynh.Application.Confessions.Commands.CreateConfession.CreateConfessionCommand(req.Text, parsed, req.Intensity, "anonymous");
        var result = await _mediator.Send(cmd);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, new SubmitConfessionResponseDto(result.Id));
    }

    [HttpPost("{id:guid}/resonate")]
    [EnableRateLimiting("resonance")]
    public async Task<IActionResult> Resonate(Guid id)
    {
        var token = _visitorTokenService.GetOrCreateVisitorToken(HttpContext);
        var hash = _visitorTokenService.HashToken(token);

        var result = await _mediator.Send(new AddResonanceCommand(id, null, hash));
        return Ok(result);
    }

    [HttpDelete("{id:guid}/resonate")]
    public async Task<IActionResult> RemoveResonate(Guid id)
    {
        var token = _visitorTokenService.GetOrCreateVisitorToken(HttpContext);
        var hash = _visitorTokenService.HashToken(token);

        var result = await _mediator.Send(new RemoveResonanceCommand(id, hash));
        return Ok(result);
    }
}
