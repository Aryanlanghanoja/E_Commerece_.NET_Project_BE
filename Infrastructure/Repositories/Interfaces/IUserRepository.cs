using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<int> CreateAsync(User user);
    Task<bool> UpdateAsync(User user);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<User>> GetAllAsync();
    Task<bool> ExistsAsync(int id);
}
