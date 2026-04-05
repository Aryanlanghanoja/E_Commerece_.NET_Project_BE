# E-Commerce Database Documentation (MS SQL + Dapper Ready)

## Overview
This schema is designed for **Microsoft SQL Server** and optimized for use with **Dapper ORM** in .NET. It follows:
- snake_case naming convention
- soft delete support (`is_deleted`)
- audit fields (`created_at`, `updated_at`)
- constraint-driven enums using `CHECK`

---

## Global Conventions

| Field | Type | Description |
|------|------|------------|
| id | INT IDENTITY | Primary Key |
| created_at | DATETIME2 | Record creation timestamp |
| updated_at | DATETIME2 | Last update timestamp |
| is_deleted | BIT | Soft delete flag (0 = active, 1 = deleted) |

---

## 1. users

```sql
CREATE TABLE users (
    id INT IDENTITY(1,1) PRIMARY KEY,
    email NVARCHAR(255) UNIQUE NOT NULL,
    password NVARCHAR(500) NOT NULL,
    mobile_no NVARCHAR(20),
    is_verified BIT DEFAULT 0,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0
);
```

---

## 2. categories

```sql
CREATE TABLE categories (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) UNIQUE NOT NULL,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0
);
```

---

## 3. products

```sql
CREATE TABLE products (
    id INT IDENTITY(1,1) PRIMARY KEY,
    name NVARCHAR(255) NOT NULL,
    category_id INT NOT NULL,
    description NVARCHAR(MAX),
    price DECIMAL(18,2) NOT NULL,
    vendor_id INT NOT NULL,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (category_id) REFERENCES categories(id),
    FOREIGN KEY (vendor_id) REFERENCES users(id)
);
```

---

## 4. addresses

```sql
CREATE TABLE addresses (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    line1 NVARCHAR(255),
    city NVARCHAR(100),
    state NVARCHAR(100),
    pincode NVARCHAR(20),
    country NVARCHAR(100),

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (user_id) REFERENCES users(id)
);
```

---

## 5. orders

```sql
CREATE TABLE orders (
    id INT IDENTITY(1,1) PRIMARY KEY,
    customer_id INT NOT NULL,
    address_id INT NOT NULL,

    arriving_time DATETIME2 NULL,

    status VARCHAR(20) CHECK (status IN ('Pending','Shipped','Delivered','Cancelled')),

    total_amount DECIMAL(18,2),

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (customer_id) REFERENCES users(id),
    FOREIGN KEY (address_id) REFERENCES addresses(id)
);
```

---

## 6. order_items

```sql
CREATE TABLE order_items (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    product_id INT NOT NULL,
    item_count INT NOT NULL,
    purchase_price DECIMAL(18,2) NOT NULL,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);
```

---

## 7. feedback

```sql
CREATE TABLE feedback (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    product_id INT NOT NULL,
    rating INT CHECK (rating BETWEEN 1 AND 5),
    review NVARCHAR(MAX),

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);
```

---

## 8. payments

```sql
CREATE TABLE payments (
    id INT IDENTITY(1,1) PRIMARY KEY,
    order_id INT NOT NULL,
    amount DECIMAL(18,2) NOT NULL,

    payment_mode VARCHAR(20) CHECK (payment_mode IN ('CashOnDelivery','UPI','NetBanking')),
    status VARCHAR(20) CHECK (status IN ('Pending','Success','Failed')),

    transaction_id NVARCHAR(255),
    paid_at DATETIME2,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (order_id) REFERENCES orders(id)
);
```

---

## 9. roles

```sql
CREATE TABLE roles (
    id INT IDENTITY(1,1) PRIMARY KEY,
    role NVARCHAR(50) NOT NULL,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0
);
```

---

## 10. user_roles

```sql
CREATE TABLE user_roles (
    user_id INT NOT NULL,
    role_id INT NOT NULL,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    PRIMARY KEY (user_id, role_id),

    FOREIGN KEY (user_id) REFERENCES users(id),
    FOREIGN KEY (role_id) REFERENCES roles(id)
);
```

---

## 11. tokens

```sql
CREATE TABLE tokens (
    id INT IDENTITY(1,1) PRIMARY KEY,
    user_id INT NOT NULL,
    token NVARCHAR(500) NOT NULL,

    type VARCHAR(30) CHECK (type IN ('password_reset','verification')),

    expiry DATETIME2 NOT NULL,
    is_revoked BIT DEFAULT 0,

    created_at DATETIME2 DEFAULT GETDATE(),
    updated_at DATETIME2 NULL,
    is_deleted BIT DEFAULT 0,

    FOREIGN KEY (user_id) REFERENCES users(id)
);
```

---

## Index Recommendations

```sql
CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_products_category_id ON products(category_id);
CREATE INDEX idx_orders_customer_id ON orders(customer_id);
CREATE INDEX idx_order_items_order_id ON order_items(order_id);
CREATE INDEX idx_payments_order_id ON payments(order_id);
```

---

## Dapper Usage Notes

- Always filter soft deletes:
```sql
WHERE is_deleted = 0
```

- Use parameterized queries to prevent SQL injection
- Prefer `Query<T>` and `QuerySingle<T>` for mapping
- Use multi-mapping for joins

---

## Final Notes

This schema is:
- Production-ready
- Optimized for SQL Server
- Fully compatible with Dapper
- Scalable for future features (inventory, coupons, etc.)

---

If needed, next step can include:
- Repository layer (Dapper)
- ASP.NET Core APIs
- Stored procedures

