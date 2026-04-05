using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using Microsoft.Extensions.Logging;

namespace ECommerceApp.Application.Services.Implementations;

public class FeedbackService : IFeedbackService
{
    private readonly IFeedbackRepository _feedbackRepository;
    private readonly IProductRepository _productRepository;
    private readonly ILogger<FeedbackService> _logger;

    public FeedbackService(
        IFeedbackRepository feedbackRepository,
        IProductRepository productRepository,
        ILogger<FeedbackService> logger)
    {
        _feedbackRepository = feedbackRepository;
        _productRepository = productRepository;
        _logger = logger;
    }

    public async Task<PagedResponse<FeedbackResponse>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 10)
    {
        var feedbacks = await _feedbackRepository.GetByProductIdAsync(productId, page, pageSize);
        var totalCount = await _feedbackRepository.GetTotalCountByProductIdAsync(productId);

        return new PagedResponse<FeedbackResponse>
        {
            Items = feedbacks.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        return await _feedbackRepository.GetAverageRatingAsync(productId);
    }

    public async Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request, int userId)
    {
        var product = await _productRepository.GetByIdAsync(request.ProductId);
        if (product == null)
        {
            throw new KeyNotFoundException("Product not found");
        }

        var feedback = new Feedback
        {
            UserId = userId,
            ProductId = request.ProductId,
            Rating = request.Rating,
            Review = request.Review,
            CreatedAt = DateTime.UtcNow
        };

        var id = await _feedbackRepository.CreateAsync(feedback);
        feedback.Id = id;

        _logger.LogInformation("Feedback created: {FeedbackId} for product: {ProductId} by user: {UserId}", 
            id, request.ProductId, userId);

        return MapToResponse(feedback);
    }

    private static FeedbackResponse MapToResponse(Feedback feedback)
    {
        return new FeedbackResponse
        {
            Id = feedback.Id,
            ProductId = feedback.ProductId,
            UserId = feedback.UserId,
            Rating = feedback.Rating,
            Review = feedback.Review,
            CreatedAt = feedback.CreatedAt
        };
    }
}
