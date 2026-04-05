using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class OrderItemRepository : IOrderItemRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public OrderItemRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(OrderItem orderItem)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO order_items (order_id, product_id, item_count, purchase_price, created_at, is_deleted) 
                    VALUES (@OrderId, @ProductId, @ItemCount, @PurchasePrice, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, orderItem);
    }

    public async Task<bool> CreateBulkAsync(IEnumerable<OrderItem> orderItems)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO order_items (order_id, product_id, item_count, purchase_price, created_at, is_deleted) 
                    VALUES (@OrderId, @ProductId, @ItemCount, @PurchasePrice, @CreatedAt, 0)";
        
        var rows = await connection.ExecuteAsync(sql, orderItems);
        return rows > 0;
    }

    public async Task<IEnumerable<OrderItem>> GetByOrderIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT oi.id, oi.order_id, oi.product_id, oi.item_count, oi.purchase_price, 
                           oi.created_at, oi.updated_at, oi.is_deleted,
                           p.name as product_name
                    FROM order_items oi
                    INNER JOIN products p ON oi.product_id = p.id
                    WHERE oi.order_id = @OrderId AND oi.is_deleted = 0";
        return await connection.QueryAsync<OrderItem>(sql, new { OrderId = orderId });
    }
}
