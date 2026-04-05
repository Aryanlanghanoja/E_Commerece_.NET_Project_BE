namespace ECommerceApp.Shared.Constants;

public static class AppConstants
{
    public const string DefaultConnectionStringName = "DefaultConnection";
    
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Vendor = "Vendor";
        public const string Customer = "Customer";
    }
    
    public static class TokenSettings
    {
        public const string SecretKey = "YourSuperSecretKeyForJwtTokenGeneration2024!@#$";
        public const string Issuer = "ECommerceApp";
        public const string Audience = "ECommerceAppUsers";
        public const int ExpiryHours = 24;
        public const int RefreshTokenExpiryDays = 7;
    }
    
    public static class CacheKeys
    {
        public const string Categories = "categories";
        public const string Products = "products";
    }
}
