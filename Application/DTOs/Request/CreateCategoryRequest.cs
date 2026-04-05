using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class CreateCategoryRequest
{
    [Required]
    [MaxLength(255)]
    public string Name { get; set; } = string.Empty;
}
