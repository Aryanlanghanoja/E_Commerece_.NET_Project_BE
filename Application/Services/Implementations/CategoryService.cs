using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<CategoryService> _logger;

    public CategoryService(ICategoryRepository categoryRepository, ILogger<CategoryService> logger)
    {
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<CategoryResponse?> GetByIdAsync(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        return category == null ? null : MapToResponse(category);
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return categories.Select(MapToResponse);
    }

    public async Task<CategoryResponse> CreateAsync(CreateCategoryRequest request)
    {
        var existingCategory = await _categoryRepository.GetByNameAsync(request.Name);
        if (existingCategory != null)
        {
            throw new InvalidOperationException("Category with this name already exists");
        }

        var category = new Category
        {
            Name = request.Name,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _categoryRepository.CreateAsync(category);
        category.Id = id;

        _logger.LogInformation("Category created: {CategoryId}", id);

        return MapToResponse(category);
    }

    public async Task<bool> UpdateAsync(int id, CreateCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        category.Name = request.Name;
        category.UpdatedAt = DateTime.UtcNow;

        var result = await _categoryRepository.UpdateAsync(category);
        
        if (result)
            _logger.LogInformation("Category updated: {CategoryId}", id);

        return result;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var result = await _categoryRepository.DeleteAsync(id);
        
        if (result)
            _logger.LogInformation("Category deleted: {CategoryId}", id);

        return result;
    }

    private static CategoryResponse MapToResponse(Category category)
    {
        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            CreatedAt = category.CreatedAt
        };
    }
}
