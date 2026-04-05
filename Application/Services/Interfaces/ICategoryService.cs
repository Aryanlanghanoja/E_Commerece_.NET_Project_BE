using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface ICategoryService
{
    Task<CategoryResponse?> GetByIdAsync(int id);
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryResponse> CreateAsync(CreateCategoryRequest request);
    Task<bool> UpdateAsync(int id, CreateCategoryRequest request);
    Task<bool> DeleteAsync(int id);
}
