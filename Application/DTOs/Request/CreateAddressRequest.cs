using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class CreateAddressRequest
{
    [MaxLength(255)]
    public string? Line1 { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(20)]
    public string? Pincode { get; set; }

    [MaxLength(100)]
    public string? Country { get; set; }
}
