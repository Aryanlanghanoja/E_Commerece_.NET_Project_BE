using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class UpdateOrderStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty;
}
