using Microsoft.AspNetCore.Mvc;

namespace DiskiTrack.Services.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { Status = "ok" });
}
