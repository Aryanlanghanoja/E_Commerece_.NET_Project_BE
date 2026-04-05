using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Order?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, customer_id, address_id, arriving_time, status, total_amount, created_at, updated_at, is_deleted 
                    FROM orders WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Order>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO orders (customer_id, address_id, arriving_time, status, total_amount, created_at, is_deleted) 
                    VALUES (@CustomerId, @AddressId, @ArrivingTime, @Status, @TotalAmount, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, order);
    }

    public async Task<bool> UpdateAsync(Order order)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE orders SET status = @Status, arriving_time = @ArrivingTime, 
                    total_amount = @TotalAmount, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, order);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE orders SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(int customerId, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, customer_id, address_id, arriving_time, status, total_amount, created_at, updated_at, is_deleted 
                    FROM orders WHERE customer_id = @CustomerId AND is_deleted = 0 
                    ORDER BY created_at DESC 
                    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        return await connection.QueryAsync<Order>(sql, new { 
            CustomerId = customerId, 
            Offset = (page - 1) * pageSize, 
            PageSize = pageSize 
        });
    }

    public async Task<int> GetTotalCountByCustomerIdAsync(int customerId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT COUNT(1) FROM orders WHERE customer_id = @CustomerId AND is_deleted = 0";
        return await connection.QuerySingleAsync<int>(sql, new { CustomerId = customerId });
    }

    public async Task<Order?> GetByIdWithDetailsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT o.id, o.customer_id, o.address_id, o.arriving_time, o.status, o.total_amount, 
                           o.created_at, o.updated_at, o.is_deleted,
                           u.email as customer_email,
                           a.line1, a.city, a.state, a.pincode, a.country
                    FROM orders o
                    INNER JOIN users u ON o.customer_id = u.id
                    INNER JOIN addresses a ON o.address_id = a.id
                    WHERE o.id = @Id AND o.is_deleted = 0";
        
        var result = await connection.QueryAsync<Order, User, Address, Order>(
            sql,
            (order, customer, address) =>
            {
                order.AddressId = address.Id;
                return order;
            },
            new { Id = id },
            splitOn: "customer_email,line1"
        );
        
        return result.FirstOrDefault();
    }
}
