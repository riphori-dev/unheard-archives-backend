using Microsoft.AspNetCore.Mvc;
using Tywynh.API.Models;
using Tywynh.API.Services;

[ApiController]
[Route("api/reflections")]
public class ReflectionsController : ControllerBase
{
    private readonly IReflectionService _reflectionService;
    private readonly ILogger<ReflectionsController> _logger;

    public ReflectionsController(IReflectionService reflectionService, ILogger<ReflectionsController> logger)
    {
        _reflectionService = reflectionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ReflectionRequest req, CancellationToken cancellationToken)
    {
        if (req is null || string.IsNullOrWhiteSpace(req.ConfessionText))
            return BadRequest("confessionText is required");

        if (req.ConfessionText.Length > 5000)
            return BadRequest("confessionText must not exceed 5000 characters");

        try
        {
            var reflection = await _reflectionService.GenerateReflectionAsync(req.ConfessionText, cancellationToken);
            if (reflection is null)
            {
                _logger.LogWarning("Reflection generation returned no content");
                return StatusCode(502, "Failed to generate reflection");
            }

            return Ok(new ReflectionResponse { Reflection = reflection });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Configuration error while generating reflection");
            return StatusCode(500, "Server configuration error");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Request cancelled by client");
            return StatusCode(499); // Client Closed Request (non-standard)
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while generating reflection");
            return StatusCode(502, "Failed to generate reflection");
        }
    }
}
