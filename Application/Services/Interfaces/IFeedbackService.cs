using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;

namespace ECommerceApp.Application.Services.Interfaces;

public interface IFeedbackService
{
    Task<PagedResponse<FeedbackResponse>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 10);
    Task<double> GetAverageRatingAsync(int productId);
    Task<FeedbackResponse> CreateAsync(CreateFeedbackRequest request, int userId);
}
