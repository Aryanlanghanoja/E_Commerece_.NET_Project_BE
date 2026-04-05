using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(int id);
    Task<int> CreateAsync(Address address);
    Task<bool> UpdateAsync(Address address);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<Address>> GetByUserIdAsync(int userId);
}
