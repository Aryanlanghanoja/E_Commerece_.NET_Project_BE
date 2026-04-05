using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IOrderService
{
    Task<OrderResponse?> GetByIdAsync(int id);
    Task<PagedResponse<OrderResponse>> GetByCustomerIdAsync(int customerId, int page = 1, int pageSize = 20);
    Task<OrderResponse> CreateAsync(CreateOrderRequest request, int customerId);
    Task<bool> UpdateStatusAsync(int id, UpdateOrderStatusRequest request);
    Task<bool> CancelAsync(int id, int customerId);
}
