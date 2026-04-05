using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CategoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, created_at, updated_at, is_deleted 
                    FROM categories WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Id = id });
    }

    public async Task<Category?> GetByNameAsync(string name)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, created_at, updated_at, is_deleted 
                    FROM categories WHERE name = @Name AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Category>(sql, new { Name = name });
    }

    public async Task<int> CreateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO categories (name, created_at, is_deleted) 
                    VALUES (@Name, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, category);
    }

    public async Task<bool> UpdateAsync(Category category)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE categories SET name = @Name, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, category);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE categories SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, created_at, updated_at, is_deleted 
                    FROM categories WHERE is_deleted = 0 ORDER BY name";
        return await connection.QueryAsync<Category>(sql);
    }
}
