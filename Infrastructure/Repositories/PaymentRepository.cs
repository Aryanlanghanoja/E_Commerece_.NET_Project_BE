using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PaymentRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Payment?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, order_id, amount, payment_mode, status, transaction_id, paid_at, created_at, updated_at, is_deleted 
                    FROM payments WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { Id = id });
    }

    public async Task<Payment?> GetByOrderIdAsync(int orderId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, order_id, amount, payment_mode, status, transaction_id, paid_at, created_at, updated_at, is_deleted 
                    FROM payments WHERE order_id = @OrderId AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Payment>(sql, new { OrderId = orderId });
    }

    public async Task<int> CreateAsync(Payment payment)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO payments (order_id, amount, payment_mode, status, transaction_id, paid_at, created_at, is_deleted) 
                    VALUES (@OrderId, @Amount, @PaymentMode, @Status, @TransactionId, @PaidAt, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, payment);
    }

    public async Task<bool> UpdateAsync(Payment payment)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE payments SET status = @Status, transaction_id = @TransactionId, 
                    paid_at = @PaidAt, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, payment);
        return rows > 0;
    }
}
