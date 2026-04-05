namespace ECommerceApp.Domain.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? MobileNo { get; set; }
    public bool IsVerified { get; set; } = false;
}
