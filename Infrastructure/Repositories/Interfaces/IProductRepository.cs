using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id);
    Task<int> CreateAsync(Product product);
    Task<bool> UpdateAsync(Product product);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null, int? vendorId = null, int page = 1, int pageSize = 20);
    Task<int> GetTotalCountAsync(int? categoryId = null);
    Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId);
}
