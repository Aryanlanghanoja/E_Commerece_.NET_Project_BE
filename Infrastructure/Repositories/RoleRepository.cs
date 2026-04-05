using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RoleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Role?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, role, created_at, updated_at, is_deleted 
                    FROM roles WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { Id = id });
    }

    public async Task<Role?> GetByNameAsync(string roleName)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, role, created_at, updated_at, is_deleted 
                    FROM roles WHERE role = @RoleName AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Role>(sql, new { RoleName = roleName });
    }

    public async Task<int> CreateAsync(Role role)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO roles (role, created_at, is_deleted) 
                    VALUES (@RoleName, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, new { role.RoleName, role.CreatedAt });
    }

    public async Task<IEnumerable<Role>> GetByUserIdAsync(int userId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT r.id, r.role, r.created_at, r.updated_at, r.is_deleted
                    FROM roles r
                    INNER JOIN user_roles ur ON r.id = ur.role_id
                    WHERE ur.user_id = @UserId AND r.is_deleted = 0 AND ur.is_deleted = 0";
        return await connection.QueryAsync<Role>(sql, new { UserId = userId });
    }
}
