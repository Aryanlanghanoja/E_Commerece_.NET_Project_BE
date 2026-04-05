using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Shared;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed", 
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var result = await _authService.RegisterAsync(request);
        return Ok(ApiResponse<Application.DTOs.Response.AuthResponse>.SuccessResponse(result, "Registration successful"));
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed",
                ModelState.Values.SelectMany(v => v.Errors.Select(e => e.ErrorMessage)).ToList()));
        }

        var result = await _authService.LoginAsync(request);
        return Ok(ApiResponse<Application.DTOs.Response.AuthResponse>.SuccessResponse(result, "Login successful"));
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        await _authService.ForgotPasswordAsync(request);
        return Ok(ApiResponse.SuccessResponse("If an account with that email exists, a password reset link has been sent"));
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ApiResponse.ErrorResponse("Validation failed"));
        }

        await _authService.ResetPasswordAsync(request);
        return Ok(ApiResponse.SuccessResponse("Password reset successful"));
    }

    [HttpGet("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromQuery] string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return BadRequest(ApiResponse.ErrorResponse("Token is required"));
        }

        await _authService.VerifyEmailAsync(token);
        return Ok(ApiResponse.SuccessResponse("Email verified successfully"));
    }

    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser()
    {
        var userId = GetUserIdFromToken();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.ErrorResponse("Unauthorized"));
        }

        var user = await _authService.GetCurrentUserAsync(userId.Value);
        if (user == null)
        {
            return NotFound(ApiResponse.ErrorResponse("User not found"));
        }

        return Ok(ApiResponse<Application.DTOs.Response.UserResponse>.SuccessResponse(user));
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
