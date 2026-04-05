-- =============================================
-- E-Commerce Database Schema
-- SQL Server Migration Script
-- =============================================

-- Create Database (if not exists)
-- IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'ECommerceDB')
-- BEGIN
--     CREATE DATABASE ECommerceDB;
-- END 
-- GO

-- Use Database
-- USE ECommerceDB;
-- GO

-- =============================================
-- 1. Users Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'users')
BEGIN
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
    
    CREATE INDEX idx_users_email ON users(email);
END
GO

-- =============================================
-- 2. Categories Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'categories')
BEGIN
    CREATE TABLE categories (
        id INT IDENTITY(1,1) PRIMARY KEY,
        name NVARCHAR(255) UNIQUE NOT NULL,
        created_at DATETIME2 DEFAULT GETDATE(),
        updated_at DATETIME2 NULL,
        is_deleted BIT DEFAULT 0
    );
END
GO

-- =============================================
-- 3. Products Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'products')
BEGIN
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
    
    CREATE INDEX idx_products_category_id ON products(category_id);
    CREATE INDEX idx_products_vendor_id ON products(vendor_id);
END
GO

-- =============================================
-- 4. Addresses Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'addresses')
BEGIN
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
    
    CREATE INDEX idx_addresses_user_id ON addresses(user_id);
END
GO

-- =============================================
-- 5. Orders Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'orders')
BEGIN
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
    
    CREATE INDEX idx_orders_customer_id ON orders(customer_id);
    CREATE INDEX idx_orders_status ON orders(status);
END
GO

-- =============================================
-- 6. Order Items Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'order_items')
BEGIN
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
    
    CREATE INDEX idx_order_items_order_id ON order_items(order_id);
END
GO

-- =============================================
-- 7. Feedback Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'feedback')
BEGIN
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
    
    CREATE INDEX idx_feedback_product_id ON feedback(product_id);
END
GO

-- =============================================
-- 8. Payments Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'payments')
BEGIN
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
    
    CREATE INDEX idx_payments_order_id ON payments(order_id);
END
GO

-- =============================================
-- 9. Roles Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'roles')
BEGIN
    CREATE TABLE roles (
        id INT IDENTITY(1,1) PRIMARY KEY,
        role NVARCHAR(50) NOT NULL,
        created_at DATETIME2 DEFAULT GETDATE(),
        updated_at DATETIME2 NULL,
        is_deleted BIT DEFAULT 0
    );
END
GO

-- =============================================
-- 10. User Roles Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'user_roles')
BEGIN
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
END
GO

-- =============================================
-- 11. Tokens Table
-- =============================================
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'tokens')
BEGIN
    CREATE TABLE tokens (
        id INT IDENTITY(1,1) PRIMARY KEY,
        user_id INT NOT NULL,
        token NVARCHAR(500) NOT NULL,
        type VARCHAR(30) CHECK (type IN ('password_reset','verification','refresh_token')),
        expiry DATETIME2 NOT NULL,
        is_revoked BIT DEFAULT 0,
        created_at DATETIME2 DEFAULT GETDATE(),
        updated_at DATETIME2 NULL,
        is_deleted BIT DEFAULT 0,
        FOREIGN KEY (user_id) REFERENCES users(id)
    );
    
    CREATE INDEX idx_tokens_user_id ON tokens(user_id);
    CREATE INDEX idx_tokens_token ON tokens(token);
END
GO

-- =============================================
-- Insert Default Roles
-- =============================================
IF NOT EXISTS (SELECT * FROM roles WHERE role = 'Admin')
BEGIN
    INSERT INTO roles (role, created_at, is_deleted) VALUES ('Admin', GETDATE(), 0);
END

IF NOT EXISTS (SELECT * FROM roles WHERE role = 'Vendor')
BEGIN
    INSERT INTO roles (role, created_at, is_deleted) VALUES ('Vendor', GETDATE(), 0);
END

IF NOT EXISTS (SELECT * FROM roles WHERE role = 'Customer')
BEGIN
    INSERT INTO roles (role, created_at, is_deleted) VALUES ('Customer', GETDATE(), 0);
END
GO

-- =============================================
-- Insert Sample Categories
-- =============================================
IF NOT EXISTS (SELECT * FROM categories WHERE name = 'Electronics')
BEGIN
    INSERT INTO categories (name, created_at, is_deleted) VALUES ('Electronics', GETDATE(), 0);
END

IF NOT EXISTS (SELECT * FROM categories WHERE name = 'Clothing')
BEGIN
    INSERT INTO categories (name, created_at, is_deleted) VALUES ('Clothing', GETDATE(), 0);
END

IF NOT EXISTS (SELECT * FROM categories WHERE name = 'Home & Garden')
BEGIN
    INSERT INTO categories (name, created_at, is_deleted) VALUES ('Home & Garden', GETDATE(), 0);
END
GO

-- =============================================
-- Stored Procedure: Cleanup Expired Tokens
-- =============================================
IF EXISTS (SELECT * FROM sys.objects WHERE type = 'P' AND name = 'sp_CleanupExpiredTokens')
    DROP PROCEDURE sp_CleanupExpiredTokens
GO

CREATE PROCEDURE sp_CleanupExpiredTokens
AS
BEGIN
    UPDATE tokens 
    SET is_deleted = 1, updated_at = GETDATE() 
    WHERE expiry < GETDATE() OR is_revoked = 1;
END
GO

-- =============================================
-- Grant Execute on Cleanup Procedure
-- =============================================
-- Note: Run this as a SQL Agent job daily for production
-- EXEC sp_CleanupExpiredTokens;

PRINT 'Database migration completed successfully!';
GO
