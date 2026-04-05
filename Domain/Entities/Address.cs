namespace ECommerceApp.Domain.Entities;

public class Address : BaseEntity
{
    public int UserId { get; set; }
    public string? Line1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
}
