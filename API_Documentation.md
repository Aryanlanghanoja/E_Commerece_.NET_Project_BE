# E-Commerce API Documentation
## Version 1.0.0 | Production Ready

---

## Table of Contents
1. [Overview](#overview)
2. [Getting Started](#getting-started)
3. [Authentication](#authentication)
4. [API Reference](#api-reference)
5. [Error Handling](#error-handling)
6. [Rate Limiting](#rate-limiting)
7. [Data Models](#data-models)
8. [Examples](#examples)

---

## Overview

| Property | Value |
|----------|-------|
| Base URL | `https://api.yourdomain.com/api/v1` |
| Protocol | HTTPS |
| Format | JSON |
| Authentication | JWT Bearer Token |

---

## Getting Started

### Prerequisites
- .NET 10.0 SDK or higher
- SQL Server 2019+
- Postman or any HTTP client

### Installation

1. **Clone the repository**
```bash
git clone https://github.com/your-org/e-commerce-api.git
cd e-commerce-api
```

2. **Configure Database**
```bash
# Update appsettings.json with your connection string
Server=localhost;Database=ECommerceDB;Trusted_Connection=True
```

3. **Run Database Migration**
```bash
# Execute Database/migration.sql in SQL Server Management Studio
```

4. **Build and Run**
```bash
dotnet restore
dotnet build
dotnet run
```

5. **Access Swagger UI**
```
http://localhost:5000
```

---

## Authentication

### Overview
The API uses **JWT (JSON Web Token)** for authentication. All protected endpoints require a valid JWT token in the Authorization header.

### Token Format
```
Authorization: Bearer <your_jwt_token>
```

### Endpoints

#### POST `/api/auth/register`
Register a new user account.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!",
  "mobileNo": "+1234567890"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Registration successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresAt": "2026-04-07T12:00:00Z",
    "user": {
      "id": 1,
      "email": "user@example.com",
      "mobileNo": "+1234567890",
      "isVerified": false,
      "createdAt": "2026-04-06T12:00:00Z"
    }
  }
}
```

---

#### POST `/api/auth/login`
Authenticate and receive JWT token.

**Request:**
```json
{
  "email": "user@example.com",
  "password": "SecurePassword123!"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Login successful",
  "data": {
    "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    "refreshToken": "dGhpcyBpcyBhIHJlZnJlc2ggdG9rZW4...",
    "expiresAt": "2026-04-07T12:00:00Z",
    "user": {
      "id": 1,
      "email": "user@example.com",
      "mobileNo": "+1234567890",
      "isVerified": true,
      "createdAt": "2026-04-06T12:00:00Z"
    }
  }
}
```

---

#### POST `/api/auth/forgot-password`
Request password reset token.

**Request:**
```json
{
  "email": "user@example.com"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "If an account with that email exists, a password reset link has been sent"
}
```

---

#### POST `/api/auth/reset-password`
Reset password using token.

**Request:**
```json
{
  "token": "abc123-def456-ghi789",
  "newPassword": "NewSecurePassword123!"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Password reset successful"
}
```

---

#### GET `/api/auth/verify-email`
Verify user email with token.

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| token | string | Yes | Verification token from email |

**Response (200):**
```json
{
  "success": true,
  "message": "Email verified successfully"
}
```

---

## API Reference

### Categories

#### GET `/api/categories`
Get all categories.

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "name": "Electronics",
      "createdAt": "2026-04-06T12:00:00Z"
    },
    {
      "id": 2,
      "name": "Clothing",
      "createdAt": "2026-04-06T12:00:00Z"
    }
  ]
}
```

---

#### GET `/api/categories/{id}`
Get category by ID.

**Path Parameters:**
| Parameter | Type | Description |
|-----------|------|-------------|
| id | int | Category ID |

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 1,
    "name": "Electronics",
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

**Response (404):**
```json
{
  "success": false,
  "message": "Category not found"
}
```

---

#### POST `/api/categories`
Create new category. **[Admin Only]**

**Headers:**
```
Authorization: Bearer <token>
```

**Request:**
```json
{
  "name": "Home & Garden"
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Category created successfully",
  "data": {
    "id": 3,
    "name": "Home & Garden",
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

---

#### PUT `/api/categories/{id}`
Update category. **[Admin Only]**

**Request:**
```json
{
  "name": "Electronics & Gadgets"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Category updated successfully"
}
```

---

#### DELETE `/api/categories/{id}`
Delete category. **[Admin Only]**

**Response (200):**
```json
{
  "success": true,
  "message": "Category deleted successfully"
}
```

---

### Products

#### GET `/api/products`
Get all products with pagination.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| categoryId | int | null | Filter by category |
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page |

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": 1,
        "name": "Smartphone X",
        "categoryId": 1,
        "categoryName": "Electronics",
        "description": "Latest flagship smartphone",
        "price": 999.99,
        "vendorId": 2,
        "averageRating": 4.5,
        "reviewCount": 128,
        "createdAt": "2026-04-06T12:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

#### GET `/api/products/{id}`
Get product by ID.

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "id": 1,
    "name": "Smartphone X",
    "categoryId": 1,
    "categoryName": "Electronics",
    "description": "Latest flagship smartphone with amazing camera",
    "price": 999.99,
    "vendorId": 2,
    "averageRating": 4.5,
    "reviewCount": 128,
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

---

#### POST `/api/products`
Create new product. **[Vendor/Admin Only]**

**Request:**
```json
{
  "name": "Smartphone X Pro",
  "categoryId": 1,
  "description": "Professional edition with enhanced camera",
  "price": 1299.99
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Product created successfully",
  "data": {
    "id": 2,
    "name": "Smartphone X Pro",
    "categoryId": 1,
    "categoryName": "Electronics",
    "description": "Professional edition with enhanced camera",
    "price": 1299.99,
    "vendorId": 1,
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

---

#### PUT `/api/products`
Update product. **[Vendor/Admin Only]**

**Request:**
```json
{
  "id": 2,
  "name": "Smartphone X Pro Max",
  "categoryId": 1,
  "description": "Updated description",
  "price": 1399.99
}
```

---

#### DELETE `/api/products/{id}`
Delete product. **[Vendor/Admin Only]**

---

### Addresses

#### GET `/api/addresses`
Get user's addresses.

**Headers:**
```
Authorization: Bearer <token>
```

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": [
    {
      "id": 1,
      "line1": "123 Main Street",
      "city": "New York",
      "state": "NY",
      "pincode": "10001",
      "country": "USA",
      "createdAt": "2026-04-06T12:00:00Z"
    }
  ]
}
```

---

#### POST `/api/addresses`
Add new address.

**Request:**
```json
{
  "line1": "456 Oak Avenue",
  "city": "Los Angeles",
  "state": "CA",
  "pincode": "90001",
  "country": "USA"
}
```

---

#### PUT `/api/addresses/{id}`
Update address.

---

#### DELETE `/api/addresses/{id}`
Delete address.

---

### Orders

#### GET `/api/orders`
Get user's orders.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 20 | Items per page |

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": 1,
        "customerId": 1,
        "addressId": 1,
        "arrivingTime": "2026-04-10T14:00:00Z",
        "status": "Pending",
        "totalAmount": 1499.98,
        "createdAt": "2026-04-06T12:00:00Z",
        "items": [
          {
            "id": 1,
            "productId": 1,
            "itemCount": 1,
            "purchasePrice": 999.99
          },
          {
            "id": 2,
            "productId": 3,
            "itemCount": 1,
            "purchasePrice": 499.99
          }
        ]
      }
    ],
    "page": 1,
    "pageSize": 20,
    "totalCount": 5,
    "totalPages": 1,
    "hasPreviousPage": false,
    "hasNextPage": false
  }
}
```

---

#### GET `/api/orders/{id}`
Get order by ID.

---

#### POST `/api/orders`
Create new order.

**Request:**
```json
{
  "addressId": 1,
  "arrivingTime": "2026-04-10T14:00:00",
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 3, "quantity": 1 }
  ]
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Order created successfully",
  "data": {
    "id": 2,
    "customerId": 1,
    "addressId": 1,
    "arrivingTime": "2026-04-10T14:00:00Z",
    "status": "Pending",
    "totalAmount": 2499.97,
    "createdAt": "2026-04-06T12:00:00Z",
    "items": [
      {
        "id": 3,
        "productId": 1,
        "itemCount": 2,
        "purchasePrice": 999.99
      },
      {
        "id": 4,
        "productId": 3,
        "itemCount": 1,
        "purchasePrice": 499.99
      }
    ]
  }
}
```

---

#### PATCH `/api/orders/{id}/status`
Update order status. **[Admin/Vendor Only]**

**Request:**
```json
{
  "status": "Shipped"
}
```

**Valid Status Values:** `Pending`, `Shipped`, `Delivered`, `Cancelled`

---

#### POST `/api/orders/{id}/cancel`
Cancel order.

---

### Payments

#### GET `/api/payments/{id}`
Get payment by ID.

---

#### GET `/api/payments/order/{orderId}`
Get payment by order ID.

---

#### POST `/api/payments`
Initiate payment.

**Request:**
```json
{
  "orderId": 1,
  "paymentMode": "UPI"
}
```

**Valid Payment Modes:** `CashOnDelivery`, `UPI`, `NetBanking`

**Response (200):**
```json
{
  "success": true,
  "message": "Payment initiated successfully",
  "data": {
    "id": 1,
    "orderId": 1,
    "amount": 1499.98,
    "paymentMode": "UPI",
    "status": "Pending",
    "transactionId": null,
    "paidAt": null,
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

---

#### PATCH `/api/payments/{id}/status`
Update payment status. **[Admin Only]**

**Query Parameters:**
| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| status | string | Yes | New status |
| transactionId | string | No | Transaction ID |

**Valid Status Values:** `Pending`, `Success`, `Failed`

---

### Feedback

#### GET `/api/feedback/product/{productId}`
Get product reviews.

**Query Parameters:**
| Parameter | Type | Default | Description |
|-----------|------|---------|-------------|
| page | int | 1 | Page number |
| pageSize | int | 10 | Items per page |

**Response (200):**
```json
{
  "success": true,
  "message": "Success",
  "data": {
    "items": [
      {
        "id": 1,
        "productId": 1,
        "userId": 2,
        "userEmail": "j***@example.com",
        "rating": 5,
        "review": "Excellent product! Highly recommended.",
        "createdAt": "2026-04-05T12:00:00Z"
      }
    ],
    "page": 1,
    "pageSize": 10,
    "totalCount": 128,
    "totalPages": 13,
    "hasPreviousPage": false,
    "hasNextPage": true
  }
}
```

---

#### GET `/api/feedback/product/{productId}/rating`
Get average rating for product.

**Response (200):**
```json
{
  "success": true,
  "message": "Average rating retrieved",
  "data": 4.5
}
```

---

#### POST `/api/feedback`
Submit product review.

**Request:**
```json
{
  "productId": 1,
  "rating": 5,
  "review": "Amazing product! The camera quality is outstanding."
}
```

**Response (201):**
```json
{
  "success": true,
  "message": "Feedback submitted successfully",
  "data": {
    "id": 15,
    "productId": 1,
    "userId": 1,
    "rating": 5,
    "review": "Amazing product! The camera quality is outstanding.",
    "createdAt": "2026-04-06T12:00:00Z"
  }
}
```

---

### Health

#### GET `/api/health`
API health check.

**Response (200):**
```json
{
  "success": true,
  "message": "E-Commerce API is running"
}
```

---

#### GET `/api/health/ping`
Detailed health status.

**Response (200):**
```json
{
  "status": "healthy",
  "timestamp": "2026-04-06T12:00:00Z"
}
```

---

## Error Handling

### Error Response Format
All errors follow a consistent format:

```json
{
  "success": false,
  "message": "Error description",
  "errors": ["Detailed error 1", "Detailed error 2"]
}
```

### HTTP Status Codes

| Code | Status | Description |
|------|--------|-------------|
| 200 | OK | Request successful |
| 201 | Created | Resource created |
| 400 | Bad Request | Invalid request data |
| 401 | Unauthorized | Invalid or missing token |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 409 | Conflict | Resource conflict |
| 422 | Unprocessable Entity | Validation failed |
| 429 | Too Many Requests | Rate limit exceeded |
| 500 | Internal Server Error | Server error |

### Common Error Scenarios

**Validation Error (400):**
```json
{
  "success": false,
  "message": "Validation failed",
  "errors": [
    "Email is required",
    "Password must be at least 6 characters"
  ]
}
```

**Unauthorized (401):**
```json
{
  "success": false,
  "message": "Invalid email or password"
}
```

**Not Found (404):**
```json
{
  "success": false,
  "message": "Product not found"
}
```

**Rate Limited (429):**
```json
{
  "success": false,
  "message": "Too many requests. Please try again later."
}
```

---

## Rate Limiting

### Policy
- **General Endpoints:** 100 requests per minute per IP
- **Auth Endpoints:** No rate limiting (for login attempts monitoring)
- **Response Header:** `Retry-After` when rate limited

### Best Practices
1. Implement exponential backoff for retries
2. Cache responses where appropriate
3. Use pagination for large datasets

---

## Data Models

### User
| Field | Type | Description |
|-------|------|-------------|
| id | int | Unique identifier |
| email | string | Email address (unique) |
| password | string | Hashed password |
| mobileNo | string? | Mobile number |
| isVerified | bool | Email verification status |
| createdAt | datetime | Creation timestamp |
| updatedAt | datetime? | Last update timestamp |

### Product
| Field | Type | Description |
|-------|------|-------------|
| id | int | Unique identifier |
| name | string | Product name |
| categoryId | int | Category reference |
| description | string? | Product description |
| price | decimal | Product price |
| vendorId | int | Vendor reference |
| averageRating | double? | Average rating |
| reviewCount | int? | Number of reviews |

### Order
| Field | Type | Description |
|-------|------|-------------|
| id | int | Unique identifier |
| customerId | int | Customer reference |
| addressId | int | Delivery address |
| status | enum | Pending/Shipped/Delivered/Cancelled |
| totalAmount | decimal | Order total |
| arrivingTime | datetime? | Expected arrival |
| items | array | Order items |

### Payment
| Field | Type | Description |
|-------|------|-------------|
| id | int | Unique identifier |
| orderId | int | Order reference |
| amount | decimal | Payment amount |
| paymentMode | enum | CashOnDelivery/UPI/NetBanking |
| status | enum | Pending/Success/Failed |
| transactionId | string? | Gateway transaction ID |
| paidAt | datetime? | Payment timestamp |

---

## Examples

### cURL Examples

#### Register User
```bash
curl -X POST https://api.yourdomain.com/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!",
    "mobileNo": "+1234567890"
  }'
```

#### Login
```bash
curl -X POST https://api.yourdomain.com/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "SecurePassword123!"
  }'
```

#### Create Order (Authenticated)
```bash
curl -X POST https://api.yourdomain.com/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_JWT_TOKEN" \
  -d '{
    "addressId": 1,
    "arrivingTime": "2026-04-10T14:00:00",
    "items": [
      {"productId": 1, "quantity": 2}
    ]
  }'
```

---

### JavaScript (Fetch) Examples

#### Register
```javascript
const response = await fetch('https://api.yourdomain.com/api/auth/register', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json'
  },
  body: JSON.stringify({
    email: 'user@example.com',
    password: 'SecurePassword123!',
    mobileNo: '+1234567890'
  })
});

const data = await response.json();
const token = data.data.token;
```

#### Get Products
```javascript
const response = await fetch('https://api.yourdomain.com/api/products?page=1&pageSize=10', {
  headers: {
    'Authorization': `Bearer ${token}`
  }
});

const data = await response.json();
const products = data.data.items;
```

---

### Python (Requests) Examples

#### Login and Create Order
```python
import requests

# Login
login_response = requests.post(
    'https://api.yourdomain.com/api/auth/login',
    json={
        'email': 'user@example.com',
        'password': 'SecurePassword123!'
    }
)

token = login_response.json()['data']['token']

# Create Order
order_response = requests.post(
    'https://api.yourdomain.com/api/orders',
    headers={'Authorization': f'Bearer {token}'},
    json={
        'addressId': 1,
        'items': [{'productId': 1, 'quantity': 2}]
    }
)
```

---

## Security Best Practices

1. **Token Storage**
   - Store tokens securely (httpOnly cookies preferred)
   - Never log tokens
   - Implement token refresh mechanism

2. **Password Requirements**
   - Minimum 8 characters
   - Include uppercase, lowercase, numbers
   - Special characters recommended

3. **API Security**
   - Always use HTTPS
   - Implement proper CORS policies
   - Validate all input server-side

4. **Rate Limiting**
   - Monitor for abuse patterns
   - Implement IP blocking for severe violations
   - Use CAPTCHA for repeated failures

---

## Support & Contact

| Channel | Details |
|---------|---------|
| Email | support@yourdomain.com |
| Documentation | docs.yourdomain.com |
| Status Page | status.yourdomain.com |

---

**Version:** 1.0.0  
**Last Updated:** April 2026
