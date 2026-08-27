using Microsoft.AspNetCore.Mvc;
using MediatR;
using Tywynh.Application.DailyEchoes.Queries.GetDailyEcho;
using Tywynh.API.Services;
using Tywynh.Application.DailyEchoes.Commands.AddInteraction;
using Tywynh.Application.DailyEchoes.DTOs;
using Microsoft.AspNetCore.RateLimiting;

[ApiController]
[Route("api/daily-echo")]
public class DailyEchoController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly VisitorTokenService _visitorTokenService;

    public DailyEchoController(IMediator mediator, VisitorTokenService visitorTokenService)
    {
        _mediator = mediator;
        _visitorTokenService = visitorTokenService;
    }

    [HttpGet]
    public async Task<ActionResult<DailyEchoResponseDto>> GetDailyEcho()
    {
        var result = await _mediator.Send(new GetDailyEchoQuery());
        return Ok(result);
    }

    public record EchoInteractRequest(bool RitualCompleted, bool Echoed);

    [HttpPost("interact")]
    [EnableRateLimiting("echo")]
    public async Task<IActionResult> Interact([FromBody] EchoInteractRequest req)
    {
        var token = _visitorTokenService.GetOrCreateVisitorToken(HttpContext);
        var hash = _visitorTokenService.HashToken(token);

        var result = await _mediator.Send(new AddInteractionCommand(DateTime.UtcNow.Date, hash, req.RitualCompleted, req.Echoed));
        return Ok(new { echo_count = result.EchoCount, is_new = result.IsNew });
    }
}
