namespace ECommerceApp.Domain.Entities;

public class OrderItem : BaseEntity
{
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int ItemCount { get; set; }
    public decimal PurchasePrice { get; set; }
}
