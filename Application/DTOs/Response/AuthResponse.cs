namespace ECommerceApp.Application.DTOs.Response;

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime ExpiresAt { get; set; }
    public UserResponse User { get; set; } = new();
}
