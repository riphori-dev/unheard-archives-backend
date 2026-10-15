using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Tywynh.API.Controllers;

// Gateway-authenticated admission only. Owner credentials remain on the frontend server.
[ApiController, Route("api/owner/access-attempt"), EnableRateLimiting("owner-login")]
public class OwnerAccessController : ControllerBase
{
    [HttpPost]
    public IActionResult ReserveAttempt() => NoContent();
}