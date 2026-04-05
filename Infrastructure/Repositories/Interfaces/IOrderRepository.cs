using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(int id);
    Task<int> CreateAsync(Order order);
    Task<bool> UpdateAsync(Order order);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page = 1, int pageSize = 20);
    Task<int> GetTotalCountByCustomerIdAsync(int customerId);
    Task<Order?> GetByIdWithDetailsAsync(int id);
}
