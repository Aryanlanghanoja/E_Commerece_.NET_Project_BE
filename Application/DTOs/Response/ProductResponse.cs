namespace ECommerceApp.Application.DTOs.Response;

public class ProductResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int VendorId { get; set; }
    public double? AverageRating { get; set; }
    public int? ReviewCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
