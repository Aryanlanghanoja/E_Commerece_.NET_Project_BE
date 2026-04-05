namespace ECommerceApp.Infrastructure.Repositories.Interfaces;

public interface ITokenRepository
{
    Task<int> CreateAsync(int userId, string token, string type, DateTime expiry);
    Task<bool> RevokeAsync(int id);
    Task<bool> RevokeByTokenAsync(string token);
    Task<bool> RevokeAllByUserIdAsync(int userId, string type);
    Task<(int UserId, int Id)?> ValidateTokenAsync(string token, string type);
    Task CleanupExpiredTokensAsync();
}
