using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class CreateOrderRequest
{
    [Required]
    public int AddressId { get; set; }

    public DateTime? ArrivingTime { get; set; }

    [Required]
    [MinLength(1)]
    public List<OrderItemRequest> Items { get; set; } = new();
}

public class OrderItemRequest
{
    [Required]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
