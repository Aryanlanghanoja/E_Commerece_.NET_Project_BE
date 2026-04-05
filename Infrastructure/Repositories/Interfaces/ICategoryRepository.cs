using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Category?> GetByNameAsync(string name);
    Task<int> CreateAsync(Category category);
    Task<bool> UpdateAsync(Category category);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
}
