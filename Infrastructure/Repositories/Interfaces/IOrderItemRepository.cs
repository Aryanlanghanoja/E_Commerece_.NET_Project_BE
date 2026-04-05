using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IOrderItemRepository
{
    Task<int> CreateAsync(OrderItem orderItem);
    Task<bool> CreateBulkAsync(IEnumerable<OrderItem> orderItems);
    Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId);
}
