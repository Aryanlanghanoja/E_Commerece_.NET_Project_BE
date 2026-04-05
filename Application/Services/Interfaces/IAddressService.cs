using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IAddressService
{
    Task<AddressResponse?> GetByIdAsync(int id);
    Task<IEnumerable<AddressResponse>> GetByUserIdAsync(int userId);
    Task<AddressResponse> CreateAsync(CreateAddressRequest request, int userId);
    Task<bool> UpdateAsync(int id, CreateAddressRequest request, int userId);
    Task<bool> DeleteAsync(int id, int userId);
}
