using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class CreatePaymentRequest
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string PaymentMode { get; set; } = string.Empty;
}
