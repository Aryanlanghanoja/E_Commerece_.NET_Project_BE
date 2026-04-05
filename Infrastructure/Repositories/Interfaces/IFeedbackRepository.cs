using ECommerceApp.Domain.Entities;

namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface IFeedbackRepository
{
    Task<Feedback?> GetByIdAsync(int id);
    Task<int> CreateAsync(Feedback feedback);
    Task<IEnumerable<Feedback>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 10);
    Task<double> GetAverageRatingAsync(int productId);
    Task<int> GetTotalCountByProductIdAsync(int productId);
}
