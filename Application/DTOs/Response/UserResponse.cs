namespace ECommerceApp.Application.DTOs.Response;

public class UserResponse
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}
