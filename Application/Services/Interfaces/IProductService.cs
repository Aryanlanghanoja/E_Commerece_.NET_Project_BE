using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IProductService
{
    Task<ProductResponse?> GetByIdAsync(int id);
    Task<PagedResponse<ProductResponse>> GetAllAsync(int? categoryId = null, int page = 1, int pageSize = 20);
    Task<IEnumerable<ProductResponse>> GetByVendorIdAsync(int vendorId);
    Task<ProductResponse> CreateAsync(CreateProductRequest request, int vendorId);
    Task<bool> UpdateAsync(UpdateProductRequest request, int vendorId);
    Task<bool> DeleteAsync(int id, int vendorId);
}
