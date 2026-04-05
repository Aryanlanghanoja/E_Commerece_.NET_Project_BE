# E-Commerce Backend Implementation Guide (Production Ready)

## Tech Stack
- ASP.NET Core Web API
- Dapper ORM
- Microsoft SQL Server
- Clean Architecture (Layered)

---

## Architecture Overview

```
Controller → Service → Repository → Database
```

- Controllers: Handle HTTP requests
- Services: Business logic
- Repositories: Data access using Dapper

---

## Folder Structure

```
/ECommerceApp
│
├── /Api
│   └── /Controllers
│       ├── OrderController.cs
│       ├── UserController.cs
│
├── /Application
│   ├── /Interfaces
│   │   ├── IOrderService.cs
│   │   ├── IUserService.cs
│   │
│   ├── /Services
│   │   ├── OrderService.cs
│   │   ├── UserService.cs
│
├── /Domain
│   ├── Order.cs
│   ├── Product.cs
│   ├── User.cs
│
├── /Infrastructure
│   ├── /Interfaces
│   │   ├── IOrderRepository.cs
│   │   ├── IUserRepository.cs
│   │
│   ├── /Repositories
│   │   ├── OrderRepository.cs
│   │   ├── UserRepository.cs
│   │
│   ├── DbConnectionFactory.cs
│
├── /DTOs
│   ├── /Request
│   │   ├── CreateOrderRequest.cs
│   ├── /Response
│   │   ├── OrderResponseDto.cs
│
├── /Shared
│   ├── Constants.cs
│   ├── ApiResponse.cs
│
├── appsettings.json
└── Program.cs
```

---

## Layer Responsibilities

### 1. Controller Layer
- Accept request
- Validate model
- Call service
- Return response

```
[HttpPost]
public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
{
    var result = await _orderService.CreateOrderAsync(request);
    return Ok(result);
}
```

---

### 2. Service Layer
- Business logic
- Validation
- Data transformation

```
public async Task<int> CreateOrderAsync(CreateOrderRequest request)
{
    if (request.Items.Count == 0)
        throw new Exception("Order must have items");

    var total = request.Items.Sum(x => x.Price * x.Quantity);

    return await _orderRepository.CreateOrderAsync(request, total);
}
```

---

### 3. Repository Layer (Dapper)
- SQL queries
- Data mapping

```
public async Task<int> CreateOrderAsync(CreateOrderRequest request, decimal total)
{
    using var connection = _db.CreateConnection();

    var sql = @"INSERT INTO orders (customer_id, total_amount)
                VALUES (@CustomerId, @TotalAmount);
                SELECT CAST(SCOPE_IDENTITY() as int);";

    return await connection.QuerySingleAsync<int>(sql, new
    {
        request.CustomerId,
        TotalAmount = total
    });
}
```

---

### 4. Domain Models
- Represent DB entities

```
public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
}
```

---

### 5. DTOs
- Request/Response shaping

```
public class CreateOrderRequest
{
    public int CustomerId { get; set; }
    public List<OrderItemDto> Items { get; set; }
}
```

---

## Dependency Injection Setup (Program.cs)

```
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
```

---

## Database Connection Factory

```
public class DbConnectionFactory
{
    private readonly IConfiguration _config;

    public DbConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public IDbConnection CreateConnection()
    {
        return new SqlConnection(_config.GetConnectionString("DefaultConnection"));
    }
}
```

---

## Best Practices

### 1. Soft Delete
```
WHERE is_deleted = 0
```

---

### 2. Transactions for Orders

```
using var connection = _db.CreateConnection();
using var transaction = connection.BeginTransaction();
```

---

### 3. Validation
- Use FluentValidation (recommended)

---

### 4. Logging
- Use Serilog

---

### 5. Error Handling
- Global exception middleware

---

## Optional Enhancements

- JWT Authentication
- Role-based authorization
- Caching (Redis)
- Pagination & filtering

---

## Email Verification (Using tokens table)

### Overview
- On registration, generate a verification token
- Store it in `tokens` table
- Send verification link via email
- User clicks link → API verifies token → marks `is_verified = 1`

---

## 1. Token Generation (Service)

```
public async Task<string> GenerateEmailVerificationTokenAsync(int userId)
{
    var token = Guid.NewGuid().ToString();

    var sql = @"INSERT INTO tokens (user_id, token, type, expiry, is_revoked, created_at, is_deleted)
                VALUES (@UserId, @Token, 'verification', DATEADD(HOUR, 24, GETDATE()), 0, GETDATE(), 0)";

    using var connection = _db.CreateConnection();
    await connection.ExecuteAsync(sql, new { UserId = userId, Token = token });

    return token;
}
```

---

## 2. Send Email (Pseudo)

```
var verificationLink = $"https://yourdomain.com/api/auth/verify-email?token={token}";

await _emailService.SendAsync(user.Email,
    "Verify your email",
    $"Click here to verify: {verificationLink}");
```

---

## 3. Verify Email API

### Controller

```
[HttpGet("verify-email")]
public async Task<IActionResult> VerifyEmail(string token)
{
    await _authService.VerifyEmailAsync(token);
    return Ok("Email verified successfully");
}
```

---

## 4. Verification Logic (Service)

```
public async Task VerifyEmailAsync(string token)
{
    using var connection = _db.CreateConnection();

    var sql = @"SELECT * FROM tokens
                WHERE token = @Token
                AND type = 'verification'
                AND is_revoked = 0
                AND is_deleted = 0";

    var tokenData = await connection.QueryFirstOrDefaultAsync<Token>(sql, new { Token = token });

    if (tokenData == null || tokenData.Expiry < DateTime.UtcNow)
        throw new Exception("Invalid or expired token");

    // Mark user verified
    var updateUserSql = @"UPDATE users SET is_verified = 1, updated_at = GETDATE()
                          WHERE id = @UserId";

    await connection.ExecuteAsync(updateUserSql, new { UserId = tokenData.UserId });

    // Revoke token
    var revokeSql = @"UPDATE tokens SET is_revoked = 1 WHERE id = @Id";
    await connection.ExecuteAsync(revokeSql, new { Id = tokenData.Id });
}
```

---

## 5. Enforce Verification at Login

```
if (!user.IsVerified)
    throw new Exception("Email not verified");
```

---

## 6. Token Cleanup (Optional Job)

```
DELETE FROM tokens WHERE expiry < GETDATE() OR is_revoked = 1;
```

---

## 7. Security Notes

- Tokens should be single-use (`is_revoked`)
- Set expiry (e.g., 24 hours)
- Always validate `is_deleted = 0`
- Use HTTPS links only

---

## Final Notes

Now your system supports:
- JWT Authentication
- Role-based Authorization
- Email Verification flow

---

## Next Steps

- Refresh Tokens (important)
- Forgot Password (reuse tokens table)
- Rate limiting on auth APIs

