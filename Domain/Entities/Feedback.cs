namespace ECommerceApp.Domain.Entities;

public class Feedback : BaseEntity
{
    public int UserId { get; set; }
    public int ProductId { get; set; }
    public int Rating { get; set; }
    public string? Review { get; set; }
}
