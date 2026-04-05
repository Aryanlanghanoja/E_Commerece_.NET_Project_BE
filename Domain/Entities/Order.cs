using ECommerceApp.Domain.Enums;

namespace ECommerceApp.Domain.Entities;

public class Order : BaseEntity
{
    public int CustomerId { get; set; }
    public int AddressId { get; set; }
    public DateTime? ArrivingTime { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public decimal TotalAmount { get; set; }
}
