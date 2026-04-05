using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse?> GetByIdAsync(int id);
    Task<PaymentResponse?> GetByOrderIdAsync(int orderId);
    Task<PaymentResponse> CreateAsync(CreatePaymentRequest request);
    Task<bool> UpdateStatusAsync(int id, string status, string? transactionId = null);
}
