using ECommerceApp.Application.DTOs.Request;
using ECommerceApp.Application.DTOs.Response;
using ECommerceApp.Application.Services.Interfaces;
using ECommerceApp.Domain.Entities;
using ECommerceApp.Infrastructure.Repositories.Interfaces;
using ECommerceApp.Shared.Constants;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace ECommerceApp.Application.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly ITokenRepository _tokenRepository;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        ITokenRepository tokenRepository,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _tokenRepository = tokenRepository;
        _logger = logger;
    }

    public async Task<AuthResponse> RegisterAsync(RegisterRequest request)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email);
        if (existingUser != null)
        {
            throw new InvalidOperationException("User with this email already exists");
        }

        var user = new User
        {
            Email = request.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
            MobileNo = request.MobileNo,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        var userId = await _userRepository.CreateAsync(user);

        var verificationToken = await GenerateEmailVerificationTokenAsync(userId);

        _logger.LogInformation("User registered with ID: {UserId}, verification token generated", userId);

        var authResponse = await GenerateAuthResponseAsync(user);
        return authResponse;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        _logger.LogInformation("User logged in: {UserId}", user.Id);

        return await GenerateAuthResponseAsync(user);
    }

    public async Task ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user == null)
        {
            _logger.LogWarning("Password reset requested for non-existent email: {Email}", request.Email);
            return;
        }

        await _tokenRepository.RevokeAllByUserIdAsync(user.Id, "password_reset");

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(24);
        await _tokenRepository.CreateAsync(user.Id, token, "password_reset", expiry);

        _logger.LogInformation("Password reset token generated for user: {UserId}", user.Id);
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request)
    {
        var tokenData = await _tokenRepository.ValidateTokenAsync(request.Token, "password_reset");
        if (tokenData == null)
        {
            throw new InvalidOperationException("Invalid or expired token");
        }

        var user = await _userRepository.GetByIdAsync(tokenData.Value.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _tokenRepository.RevokeAsync(tokenData.Value.Id);

        _logger.LogInformation("Password reset completed for user: {UserId}", user.Id);
    }

    public async Task VerifyEmailAsync(string token)
    {
        var tokenData = await _tokenRepository.ValidateTokenAsync(token, "verification");
        if (tokenData == null)
        {
            throw new InvalidOperationException("Invalid or expired token");
        }

        var user = await _userRepository.GetByIdAsync(tokenData.Value.UserId);
        if (user == null)
        {
            throw new InvalidOperationException("User not found");
        }

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        await _tokenRepository.RevokeAsync(tokenData.Value.Id);

        _logger.LogInformation("Email verified for user: {UserId}", user.Id);
    }

    public async Task<UserResponse?> GetCurrentUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return null;

        return new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            MobileNo = user.MobileNo,
            IsVerified = user.IsVerified,
            CreatedAt = user.CreatedAt
        };
    }

    private async Task<string> GenerateEmailVerificationTokenAsync(int userId)
    {
        await _tokenRepository.RevokeAllByUserIdAsync(userId, "verification");

        var token = Guid.NewGuid().ToString();
        var expiry = DateTime.UtcNow.AddHours(24);
        await _tokenRepository.CreateAsync(userId, token, "verification", expiry);

        return token;
    }

    private async Task<AuthResponse> GenerateAuthResponseAsync(User user)
    {
        var token = GenerateJwtToken(user);
        var refreshToken = GenerateRefreshToken();

        await _tokenRepository.CreateAsync(user.Id, refreshToken, "refresh_token", DateTime.UtcNow.AddDays(AppConstants.TokenSettings.RefreshTokenExpiryDays));

        return new AuthResponse
        {
            Token = token,
            RefreshToken = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddHours(AppConstants.TokenSettings.ExpiryHours),
            User = new UserResponse
            {
                Id = user.Id,
                Email = user.Email,
                MobileNo = user.MobileNo,
                IsVerified = user.IsVerified,
                CreatedAt = user.CreatedAt
            }
        };
    }

    private string GenerateJwtToken(User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AppConstants.TokenSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: AppConstants.TokenSettings.Issuer,
            audience: AppConstants.TokenSettings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(AppConstants.TokenSettings.ExpiryHours),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    private static string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }
}
