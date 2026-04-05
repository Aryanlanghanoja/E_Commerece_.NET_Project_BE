using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IAuthService
{
    Task<AuthResponse> RegisterAsync(RegisterRequest request);
    Task<AuthResponse> LoginAsync(LoginRequest request);
    Task ForgotPasswordAsync(ForgotPasswordRequest request);
    Task ResetPasswordAsync(ResetPasswordRequest request);
    Task VerifyEmailAsync(string token);
    Task<UserResponse?> GetCurrentUserAsync(int userId);
}
