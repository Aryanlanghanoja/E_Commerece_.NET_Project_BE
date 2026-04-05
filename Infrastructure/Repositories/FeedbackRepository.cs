using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class FeedbackRepository : IFeedbackRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public FeedbackRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Feedback?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, user_id, product_id, rating, review, created_at, updated_at, is_deleted 
                    FROM feedback WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Feedback>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Feedback feedback)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO feedback (user_id, product_id, rating, review, created_at, is_deleted) 
                    VALUES (@UserId, @ProductId, @Rating, @Review, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, feedback);
    }

    public async Task<IEnumerable<Feedback>> GetByProductIdAsync(int productId, int page = 1, int pageSize = 10)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT f.id, f.user_id, f.product_id, f.rating, f.review, f.created_at, f.updated_at, f.is_deleted,
                           u.email as user_email
                    FROM feedback f
                    INNER JOIN users u ON f.user_id = u.id
                    WHERE f.product_id = @ProductId AND f.is_deleted = 0
                    ORDER BY f.created_at DESC
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        return await connection.QueryAsync<Feedback>(sql, new { 
            ProductId = productId, 
            Offset = (page - 1) * pageSize, 
            PageSize = pageSize 
        });
    }

    public async Task<double> GetAverageRatingAsync(int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT ISNULL(AVG(CAST(rating AS FLOAT)), 0) FROM feedback 
                    WHERE product_id = @ProductId AND is_deleted = 0";
        return await connection.QuerySingleAsync<double>(sql, new { ProductId = productId });
    }

    public async Task<int> GetTotalCountByProductIdAsync(int productId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT COUNT(1) FROM feedback WHERE product_id = @ProductId AND is_deleted = 0";
        return await connection.QuerySingleAsync<int>(sql, new { ProductId = productId });
    }
}
