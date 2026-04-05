using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class FeedbackController : ControllerBase
{
    private readonly IFeedbackService _feedbackService;
    private readonly ILogger<FeedbackController> _logger;

    public FeedbackController(IFeedbackService feedbackService, ILogger<FeedbackController> logger)
    {
        _feedbackService = feedbackService;
        _logger = logger;
    }

    [HttpGet("product/{productId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByProductId(int productId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var feedback = await _feedbackService.GetByProductIdAsync(productId, page, pageSize);
        return Ok(ApiResponse<Application.DTOs.Response.PagedResponse<Application.DTOs.Response.FeedbackResponse>>.SuccessResponse(feedback));
    }

    [HttpGet("product/{productId}/rating")]
    [AllowAnonymous]
    public async Task<IActionResult> GetAverageRating(int productId)
    {
        var rating = await _feedbackService.GetAverageRatingAsync(productId);
        return Ok(ApiResponse<decimal>.SuccessResponse((decimal)rating, "Average rating retrieved"));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateFeedbackRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var feedback = await _feedbackService.CreateAsync(request, userId.Value);
        return CreatedAtAction(nameof(GetByProductId), new { productId = request.ProductId },
            ApiResponse<Application.DTOs.Response.FeedbackResponse>.SuccessResponse(feedback, "Feedback submitted successfully"));
    }

    private int? GetUserIdFromToken()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out var userId))
        {
            return null;
        }

        return userId;
    }
}
