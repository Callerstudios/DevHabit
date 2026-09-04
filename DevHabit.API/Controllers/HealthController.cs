using Microsoft.AspNetCore.Mvc;

namespace DevHabit.API.Controllers;

[Route("/health")]
[ApiController]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult GetHealth()
    {
        return Ok(new { status = "Healthy" });
    }
}
