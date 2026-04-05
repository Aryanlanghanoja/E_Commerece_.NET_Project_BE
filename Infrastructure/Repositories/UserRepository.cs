using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, email, password, mobile_no, is_verified, created_at, updated_at, is_deleted 
                    FROM users WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Id = id });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, email, password, mobile_no, is_verified, created_at, updated_at, is_deleted 
                    FROM users WHERE email = @Email AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<User>(sql, new { Email = email });
    }

    public async Task<int> CreateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO users (email, password, mobile_no, is_verified, created_at, is_deleted) 
                    VALUES (@Email, @Password, @MobileNo, @IsVerified, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, user);
    }

    public async Task<bool> UpdateAsync(User user)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE users SET email = @Email, password = @Password, mobile_no = @MobileNo, 
                    is_verified = @IsVerified, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, user);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE users SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, email, password, mobile_no, is_verified, created_at, updated_at, is_deleted 
                    FROM users WHERE is_deleted = 0";
        return await connection.QueryAsync<User>(sql);
    }

    public async Task<bool> ExistsAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT COUNT(1) FROM users WHERE id = @Id AND is_deleted = 0";
        var count = await connection.QuerySingleAsync<int>(sql, new { Id = id });
        return count > 0;
    }
}
