using ECommerceApp.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok(ApiResponse.SuccessResponse("E-Commerce API is running"));
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}
