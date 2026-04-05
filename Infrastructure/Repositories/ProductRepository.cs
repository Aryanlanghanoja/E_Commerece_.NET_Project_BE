using Dapper;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Data;
using ECommerceApp.Infrastructure.Repositories.Interfaces;

namespace ECommerceApp.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, category_id, description, price, vendor_id, created_at, updated_at, is_deleted 
                    FROM products WHERE id = @Id AND is_deleted = 0";
        return await connection.QueryFirstOrDefaultAsync<Product>(sql, new { Id = id });
    }

    public async Task<int> CreateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"INSERT INTO products (name, category_id, description, price, vendor_id, created_at, is_deleted) 
                    VALUES (@Name, @CategoryId, @Description, @Price, @VendorId, @CreatedAt, 0);
                    SELECT CAST(SCOPE_IDENTITY() as int)";
        return await connection.QuerySingleAsync<int>(sql, product);
    }

    public async Task<bool> UpdateAsync(Product product)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE products SET name = @Name, category_id = @CategoryId, description = @Description, 
                    price = @Price, updated_at = @UpdatedAt 
                    WHERE id = @Id AND is_deleted = 0";
        var rows = await connection.ExecuteAsync(sql, product);
        return rows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"UPDATE products SET is_deleted = 1, updated_at = @UpdatedAt WHERE id = @Id";
        var rows = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rows > 0;
    }

    public async Task<IEnumerable<Product>> GetAllAsync(int? categoryId = null, int? vendorId = null, int page = 1, int pageSize = 20)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, category_id, description, price, vendor_id, created_at, updated_at, is_deleted 
                    FROM products WHERE is_deleted = 0";
        
        if (categoryId.HasValue)
            sql += " AND category_id = @CategoryId";
        if (vendorId.HasValue)
            sql += " AND vendor_id = @VendorId";
        
        sql += " ORDER BY created_at DESC OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";
        
        return await connection.QueryAsync<Product>(sql, new { 
            CategoryId = categoryId, 
            VendorId = vendorId, 
            Offset = (page - 1) * pageSize, 
            PageSize = pageSize 
        });
    }

    public async Task<int> GetTotalCountAsync(int? categoryId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT COUNT(1) FROM products WHERE is_deleted = 0";
        
        if (categoryId.HasValue)
            sql += " AND category_id = @CategoryId";
        
        return await connection.QuerySingleAsync<int>(sql, new { CategoryId = categoryId });
    }

    public async Task<IEnumerable<Product>> GetByVendorIdAsync(int vendorId)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"SELECT id, name, category_id, description, price, vendor_id, created_at, updated_at, is_deleted 
                    FROM products WHERE vendor_id = @VendorId AND is_deleted = 0 ORDER BY created_at DESC";
        return await connection.QueryAsync<Product>(sql, new { VendorId = vendorId });
    }
}
