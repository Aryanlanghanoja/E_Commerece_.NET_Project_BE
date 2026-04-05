namespace ECommerceApp.Application.DTOs.Response;

public class OrderResponse
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public DateTime? ArrivingTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemResponse>? Items { get; set; }
    public AddressResponse? Address { get; set; }
}

public class OrderItemResponse
{
    public int Id { get; set; }
    public int ProductId { get; set; }
    public string? ProductName { get; set; }
    public int ItemCount { get; set; }
    public decimal PurchasePrice { get; set; }
}
