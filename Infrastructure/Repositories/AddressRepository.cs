using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public AddressRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Address?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, user_id, line1, city, state, pincode, country, created_at, updated_at, is_deleted 
                    FROM addresses WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Address>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Address address)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO addresses (user_id, line1, city, state, pincode, country, created_at, is_deleted) 
                    VALUES (@UserId, @Line1, @City, @State, @Pincode, @Country, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, address);
    }

    public async Task<bool> UpdateAsync(Address address)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE addresses SET line1 = @Line1, city = @City, state = @State, 
                    pincode = @Pincode, country = @Country, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, address);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE addresses SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<IEnumerable<Address>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, user_id, line1, city, state, pincode, country, created_at, updated_at, is_deleted 
                    FROM addresses WHERE user_id = @UserId AND is_deleted = 0";
        return await connection.QueryAsync<Address>(sql, new { UserId = userId });
    }
}
