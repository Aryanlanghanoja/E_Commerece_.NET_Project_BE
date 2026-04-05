using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class ProductService : IProductService
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly ILogger<ProductService> _logger;

    public ProductService(
        IProductRepository productRepository,
        ICategoryRepository categoryRepository,
        IFeedbackRepository feedbackRepository,
        ILogger<ProductService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _feedbackRepository = feedbackRepository;
        _logger = logger;
    }

    public async Task<ProductResponse?> GetByIdAsync(int id)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null) return null;

        var response = MapToResponse(product);
        
        response.AverageRating = await _feedbackRepository.GetAverageRatingAsync(id);
        var reviewCount = await _feedbackRepository.GetTotalCountByProductIdAsync(id);
        response.ReviewCount = reviewCount;

        var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
        response.CategoryName = category?.Name;

        return response;
    }

    public async Task<PagedResponse<ProductResponse>> GetAllAsync(int? categoryId = null, int page = 1, int pageSize = 20)
    {
        var products = await _productRepository.GetAllAsync(categoryId, null, page, pageSize);
        var totalCount = await _productRepository.GetTotalCountAsync(categoryId);

        var productList = products.ToList();
        var responses = new List<ProductResponse>();

        foreach (var product in productList)
        {
            var response = MapToResponse(product);
            response.AverageRating = await _feedbackRepository.GetAverageRatingAsync(product.Id);
            var category = await _categoryRepository.GetByIdAsync(product.CategoryId);
            response.CategoryName = category?.Name;
            responses.Add(response);
        }

        return new PagedResponse<ProductResponse>
        {
            Items = responses,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IEnumerable<ProductResponse>> GetByVendorIdAsync(int vendorId)
    {
        var products = await _productRepository.GetByVendorIdAsync(vendorId);
        return products.Select(p =>
        {
            var response = MapToResponse(p);
            response.AverageRating = _feedbackRepository.GetAverageRatingAsync(p.Id).Result;
            return response;
        });
    }

    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, int vendorId)
    {
        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        var product = new Product
        {
            Name = request.Name,
            CategoryId = request.CategoryId,
            Description = request.Description,
            Price = request.Price,
            VendorId = vendorId,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _productRepository.CreateAsync(product);
        product.Id = id;

        _logger.LogInformation("Product created: {ProductId} by vendor: {VendorId}", id, vendorId);

        var response = MapToResponse(product);
        response.CategoryName = category.Name;
        return response;
    }

    public async Task<bool> UpdateAsync(UpdateProductRequest request, int vendorId)
    {
        var product = await _productRepository.GetByIdAsync(request.Id);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        if (product.VendorId != vendorId)
        {
            throw new UnauthorizedAccessException("You can only update your own products");
        }

        var category = await _categoryRepository.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            throw new KeyNotFoundException("Category not found");
        }

        product.Name = request.Name;
        product.CategoryId = request.CategoryId;
        product.Description = request.Description;
        product.Price = request.Price;
        product.UpdatedAt = DateTime.UtcNow;

        var result = await _productRepository.UpdateAsync(product);
        
        if (result)
            _logger.LogInformation("Product updated: {ProductId}", request.Id);

        return result;
    }

    public async Task<bool> DeleteAsync(int id, int vendorId)
    {
        var product = await _productRepository.GetByIdAsync(id);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        if (product.VendorId != vendorId)
        {
            throw new UnauthorizedAccessException("You can only delete your own products");
        }

        var result = await _productRepository.DeleteAsync(id);
        
        if (result)
            _logger.LogInformation("Product deleted: {ProductId}", id);

        return result;
    }

    private static ProductResponse MapToResponse(Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            CategoryId = product.CategoryId,
            Description = product.Description,
            Price = product.Price,
            VendorId = product.VendorId,
            CreatedAt = product.CreatedAt
        };
    }
}
