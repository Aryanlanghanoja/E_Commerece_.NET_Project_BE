using ECommerceApp.Domain.Enums;

namespace ECommerceApp.Domain.Entities;

public class Token : BaseEntity
{
    public int UserId { get; set; }
    public string TokenValue { get; set; } = string.Empty;
    public TokenType Type { get; set; }
    public DateTime Expiry { get; set; }
    public bool IsRevoked { get; set; } = false;
}
