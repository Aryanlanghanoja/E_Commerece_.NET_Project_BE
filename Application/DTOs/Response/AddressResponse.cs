namespace ECommerceApp.Application.DTOs.Response;

public class AddressResponse
{
    public int Id { get; set; }
    public string? Line1 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? Pincode { get; set; }
    public string? Country { get; set; }
    public DateTime CreatedAt { get; set; }
}
