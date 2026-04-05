using Dapper;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class TokenRepository : ITokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(int userId, string token, string type, DateTime expiry)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO tokens (user_id, token, type, expiry, is_revoked, created_at, is_deleted) 
                    VALUES (@UserId, @Token, @Type, @Expiry, 0, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, new { 
            UserId = userId, 
            Token = token, 
            Type = type, 
            Expiry = expiry, 
            CreatedAt = DateTime.UtcNow 
        });
    }

    public async Task<bool> RevokeAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE tokens SET is_revoked = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<bool> RevokeByTokenAsync(string token)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE tokens SET is_revoked = 1, updated_at = @UpdatedAt WHERE token = @Token";
        var rows = await connection.ExecuteAsync(sql, new { Token = token, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<bool> RevokeAllByUserIdAsync(int userId, string type)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE tokens SET is_revoked = 1, updated_at = @UpdatedAt 
                    WHERE user_id = @UserId AND type = @Type AND is_revoked = 0";
        var rows = await connection.ExecuteAsync(sql, new { UserId = userId, Type = type, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<(int UserId, int Id)?> ValidateTokenAsync(string token, string type)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT user_id, id FROM tokens 
                    WHERE token = @Token AND type = @Type AND is_revoked = 0 AND is_deleted = 0 AND expiry > @Now";
        return await connection.QueryFirstOrDefaultAsync<(int UserId, int Id)?>(sql, new { 
            Token = token, 
            Type = type, 
            Now = DateTime.UtcNow 
        });
    }

    public async Task CleanupExpiredTokensAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE tokens SET is_deleted = 1, updated_at = @UpdatedAt 
                    WHERE expiry < @Now OR is_revoked = 1";
        await connection.ExecuteAsync(sql, new { Now = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
    }
}
