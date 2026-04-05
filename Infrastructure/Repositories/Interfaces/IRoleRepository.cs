using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(int id);
    Task<Role?> GetByNameAsync(string roleName);
    Task<int> CreateAsync(Role role);
    Task<IEnumerable<Role>> GetByUserIdAsync(int userId);
}
