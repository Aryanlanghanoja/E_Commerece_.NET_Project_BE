using System.ComponentModel.DataAnnotations;

namespace ECommerceApp.Application.DTOs.Request;

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;
}
