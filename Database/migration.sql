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
-- DUMMY DATA SEED SECTION
-- All tables have 15 rows minimum
-- =============================================

-- =============================================
-- 1. Seed Roles (15 rows)
-- =============================================
SET IDENTITY_INSERT roles ON;

MERGE INTO roles AS target
USING (VALUES 
    (1, 'Admin', DATEADD(day, -365, GETDATE()), NULL, 0),
    (2, 'Vendor', DATEADD(day, -365, GETDATE()), NULL, 0),
    (3, 'Customer', DATEADD(day, -365, GETDATE()), NULL, 0),
    (4, 'SuperAdmin', DATEADD(day, -360, GETDATE()), NULL, 0),
    (5, 'Manager', DATEADD(day, -350, GETDATE()), NULL, 0),
    (6, 'Operator', DATEADD(day, -340, GETDATE()), NULL, 0),
    (7, 'Support', DATEADD(day, -330, GETDATE()), NULL, 0),
    (8, 'Analyst', DATEADD(day, -320, GETDATE()), NULL, 0),
    (9, 'ContentManager', DATEADD(day, -310, GETDATE()), NULL, 0),
    (10, 'Marketing', DATEADD(day, -300, GETDATE()), NULL, 0),
    (11, 'Logistics', DATEADD(day, -290, GETDATE()), NULL, 0),
    (12, 'Warehouse', DATEADD(day, -280, GETDATE()), NULL, 0),
    (13, 'Delivery', DATEADD(day, -270, GETDATE()), NULL, 0),
    (14, 'Finance', DATEADD(day, -260, GETDATE()), NULL, 0),
    (15, 'HR', DATEADD(day, -250, GETDATE()), NULL, 0)
) AS source (id, role, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, role, created_at, updated_at, is_deleted)
    VALUES (source.id, source.role, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT roles OFF;
GO

-- =============================================
-- 2. Seed Users (15 rows)
-- Password: Password123! (BCrypt hashed)
-- =============================================
SET IDENTITY_INSERT users ON;

MERGE INTO users AS target
USING (VALUES 
    (1, 'admin@ecommerce.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551001', 1, DATEADD(day, -180, GETDATE()), NULL, 0),
    (2, 'vendor1@ecommerce.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551002', 1, DATEADD(day, -170, GETDATE()), NULL, 0),
    (3, 'vendor2@ecommerce.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551003', 1, DATEADD(day, -160, GETDATE()), NULL, 0),
    (4, 'customer1@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551004', 1, DATEADD(day, -150, GETDATE()), NULL, 0),
    (5, 'customer2@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551005', 1, DATEADD(day, -140, GETDATE()), NULL, 0),
    (6, 'customer3@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551006', 1, DATEADD(day, -130, GETDATE()), NULL, 0),
    (7, 'customer4@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551007', 1, DATEADD(day, -120, GETDATE()), NULL, 0),
    (8, 'customer5@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551008', 1, DATEADD(day, -110, GETDATE()), NULL, 0),
    (9, 'customer6@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551009', 1, DATEADD(day, -100, GETDATE()), NULL, 0),
    (10, 'customer7@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551010', 1, DATEADD(day, -90, GETDATE()), NULL, 0),
    (11, 'customer8@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551011', 1, DATEADD(day, -80, GETDATE()), NULL, 0),
    (12, 'customer9@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551012', 1, DATEADD(day, -70, GETDATE()), NULL, 0),
    (13, 'customer10@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551013', 1, DATEADD(day, -60, GETDATE()), NULL, 0),
    (14, 'customer11@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551014', 1, DATEADD(day, -50, GETDATE()), NULL, 0),
    (15, 'customer12@example.com', '$2a$11$K3kZQvZ5e7xGHvQ8p5vH5u8rYJqM9F5nW4xLmK2pQ3rS6tU7vW8Xy', '+12025551015', 1, DATEADD(day, -40, GETDATE()), NULL, 0)
) AS source (id, email, password, mobile_no, is_verified, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, email, password, mobile_no, is_verified, created_at, updated_at, is_deleted)
    VALUES (source.id, source.email, source.password, source.mobile_no, source.is_verified, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT users OFF;
GO

-- =============================================
-- 3. Seed User Roles (15 rows)
-- =============================================
MERGE INTO user_roles AS target
USING (VALUES 
    (1, 1, DATEADD(day, -180, GETDATE()), NULL, 0),  -- User 1 = Admin
    (2, 2, DATEADD(day, -170, GETDATE()), NULL, 0),  -- User 2 = Vendor
    (3, 2, DATEADD(day, -160, GETDATE()), NULL, 0),  -- User 3 = Vendor
    (4, 3, DATEADD(day, -150, GETDATE()), NULL, 0),  -- User 4 = Customer
    (5, 3, DATEADD(day, -140, GETDATE()), NULL, 0),  -- User 5 = Customer
    (6, 3, DATEADD(day, -130, GETDATE()), NULL, 0),  -- User 6 = Customer
    (7, 3, DATEADD(day, -120, GETDATE()), NULL, 0),  -- User 7 = Customer
    (8, 3, DATEADD(day, -110, GETDATE()), NULL, 0),  -- User 8 = Customer
    (9, 3, DATEADD(day, -100, GETDATE()), NULL, 0),  -- User 9 = Customer
    (10, 3, DATEADD(day, -90, GETDATE()), NULL, 0),  -- User 10 = Customer
    (11, 3, DATEADD(day, -80, GETDATE()), NULL, 0),  -- User 11 = Customer
    (12, 3, DATEADD(day, -70, GETDATE()), NULL, 0),  -- User 12 = Customer
    (13, 3, DATEADD(day, -60, GETDATE()), NULL, 0),  -- User 13 = Customer
    (14, 3, DATEADD(day, -50, GETDATE()), NULL, 0),  -- User 14 = Customer
    (15, 3, DATEADD(day, -40, GETDATE()), NULL, 0)   -- User 15 = Customer
) AS source (user_id, role_id, created_at, updated_at, is_deleted)
ON target.user_id = source.user_id AND target.role_id = source.role_id
WHEN NOT MATCHED THEN
    INSERT (user_id, role_id, created_at, updated_at, is_deleted)
    VALUES (source.user_id, source.role_id, source.created_at, source.updated_at, source.is_deleted);
GO

-- =============================================
-- 4. Seed Categories (15 rows)
-- =============================================
SET IDENTITY_INSERT categories ON;

MERGE INTO categories AS target
USING (VALUES 
    (1, 'Electronics', DATEADD(day, -365, GETDATE()), NULL, 0),
    (2, 'Clothing', DATEADD(day, -360, GETDATE()), NULL, 0),
    (3, 'Home & Garden', DATEADD(day, -355, GETDATE()), NULL, 0),
    (4, 'Sports & Outdoors', DATEADD(day, -350, GETDATE()), NULL, 0),
    (5, 'Books & Media', DATEADD(day, -345, GETDATE()), NULL, 0),
    (6, 'Health & Beauty', DATEADD(day, -340, GETDATE()), NULL, 0),
    (7, 'Toys & Games', DATEADD(day, -335, GETDATE()), NULL, 0),
    (8, 'Automotive', DATEADD(day, -330, GETDATE()), NULL, 0),
    (9, 'Pet Supplies', DATEADD(day, -325, GETDATE()), NULL, 0),
    (10, 'Office Products', DATEADD(day, -320, GETDATE()), NULL, 0),
    (11, 'Food & Grocery', DATEADD(day, -315, GETDATE()), NULL, 0),
    (12, 'Baby Products', DATEADD(day, -310, GETDATE()), NULL, 0),
    (13, 'Jewelry', DATEADD(day, -305, GETDATE()), NULL, 0),
    (14, 'Musical Instruments', DATEADD(day, -300, GETDATE()), NULL, 0),
    (15, 'Tools & Hardware', DATEADD(day, -295, GETDATE()), NULL, 0)
) AS source (id, name, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, name, created_at, updated_at, is_deleted)
    VALUES (source.id, source.name, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT categories OFF;
GO

-- =============================================
-- 5. Seed Products (15 rows)
-- Vendors: User 2 and User 3
-- =============================================
SET IDENTITY_INSERT products ON;

MERGE INTO products AS target
USING (VALUES 
    (1, 'Smartphone X Pro', 1, 'Latest flagship smartphone with 6.7-inch display, 5G connectivity, and 108MP camera', 999.99, 2, DATEADD(day, -100, GETDATE()), NULL, 0),
    (2, 'Wireless Earbuds Pro', 1, 'Premium wireless earbuds with active noise cancellation', 249.99, 2, DATEADD(day, -95, GETDATE()), NULL, 0),
    (3, 'Smart Watch Series 5', 1, 'Advanced smartwatch with health monitoring features', 399.99, 2, DATEADD(day, -90, GETDATE()), NULL, 0),
    (4, 'Cotton T-Shirt Classic', 2, '100% organic cotton t-shirt, available in multiple colors', 29.99, 3, DATEADD(day, -85, GETDATE()), NULL, 0),
    (5, 'Denim Jeans Slim Fit', 2, 'Classic denim jeans with modern slim fit design', 79.99, 3, DATEADD(day, -80, GETDATE()), NULL, 0),
    (6, 'Running Shoes Air Max', 4, 'Lightweight running shoes with air cushioning technology', 149.99, 2, DATEADD(day, -75, GETDATE()), NULL, 0),
    (7, 'Yoga Mat Premium', 4, 'Non-slip yoga mat with alignment lines', 49.99, 3, DATEADD(day, -70, GETDATE()), NULL, 0),
    (8, 'LED Desk Lamp', 3, 'Adjustable LED desk lamp with multiple brightness levels', 69.99, 2, DATEADD(day, -65, GETDATE()), NULL, 0),
    (9, 'Coffee Maker Deluxe', 3, '12-cup programmable coffee maker with thermal carafe', 129.99, 3, DATEADD(day, -60, GETDATE()), NULL, 0),
    (10, 'Bluetooth Speaker', 1, 'Portable waterproof Bluetooth speaker with 20-hour battery', 89.99, 2, DATEADD(day, -55, GETDATE()), NULL, 0),
    (11, 'Gaming Headset', 1, 'Surround sound gaming headset with noise-canceling mic', 179.99, 2, DATEADD(day, -50, GETDATE()), NULL, 0),
    (12, 'Backpack Travel Pro', 4, '40L travel backpack with laptop compartment and USB port', 99.99, 3, DATEADD(day, -45, GETDATE()), NULL, 0),
    (13, 'Cookware Set 10-Piece', 3, 'Stainless steel cookware set with non-stick coating', 199.99, 3, DATEADD(day, -40, GETDATE()), NULL, 0),
    (14, 'Fitness Tracker Band', 1, 'Waterproof fitness tracker with heart rate monitor', 79.99, 2, DATEADD(day, -35, GETDATE()), NULL, 0),
    (15, 'Laptop Stand Adjustable', 10, 'Ergonomic aluminum laptop stand with cooling', 59.99, 3, DATEADD(day, -30, GETDATE()), NULL, 0)
) AS source (id, name, category_id, description, price, vendor_id, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, name, category_id, description, price, vendor_id, created_at, updated_at, is_deleted)
    VALUES (source.id, source.name, source.category_id, source.description, source.price, source.vendor_id, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT products OFF;
GO

-- =============================================
-- 6. Seed Addresses (15 rows)
-- =============================================
SET IDENTITY_INSERT addresses ON;

MERGE INTO addresses AS target
USING (VALUES 
    (1, 4, '123 Main Street, Apt 4B', 'New York', 'NY', '10001', 'USA', DATEADD(day, -150, GETDATE()), NULL, 0),
    (2, 5, '456 Oak Avenue', 'Los Angeles', 'CA', '90001', 'USA', DATEADD(day, -140, GETDATE()), NULL, 0),
    (3, 6, '789 Pine Road, Suite 100', 'Chicago', 'IL', '60601', 'USA', DATEADD(day, -130, GETDATE()), NULL, 0),
    (4, 7, '321 Maple Lane', 'Houston', 'TX', '77001', 'USA', DATEADD(day, -120, GETDATE()), NULL, 0),
    (5, 8, '654 Cedar Boulevard', 'Phoenix', 'AZ', '85001', 'USA', DATEADD(day, -110, GETDATE()), NULL, 0),
    (6, 9, '987 Birch Street', 'Philadelphia', 'PA', '19101', 'USA', DATEADD(day, -100, GETDATE()), NULL, 0),
    (7, 10, '147 Elm Court', 'San Antonio', 'TX', '78201', 'USA', DATEADD(day, -90, GETDATE()), NULL, 0),
    (8, 11, '258 Walnut Drive', 'San Diego', 'CA', '92101', 'USA', DATEADD(day, -80, GETDATE()), NULL, 0),
    (9, 12, '369 Cherry Avenue', 'Dallas', 'TX', '75201', 'USA', DATEADD(day, -70, GETDATE()), NULL, 0),
    (10, 13, '741 Spruce Way', 'San Jose', 'CA', '95101', 'USA', DATEADD(day, -60, GETDATE()), NULL, 0),
    (11, 14, '852 Ash Road', 'Austin', 'TX', '73301', 'USA', DATEADD(day, -50, GETDATE()), NULL, 0),
    (12, 15, '963 Willow Place', 'Jacksonville', 'FL', '32099', 'USA', DATEADD(day, -40, GETDATE()), NULL, 0),
    (13, 4, '159 Poplar Street', 'Fort Worth', 'TX', '76101', 'USA', DATEADD(day, -35, GETDATE()), NULL, 0),
    (14, 5, '753 Hickory Lane', 'Columbus', 'OH', '43085', 'USA', DATEADD(day, -30, GETDATE()), NULL, 0),
    (15, 6, '357 Sycamore Drive', 'Charlotte', 'NC', '28201', 'USA', DATEADD(day, -25, GETDATE()), NULL, 0)
) AS source (id, user_id, line1, city, state, pincode, country, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, user_id, line1, city, state, pincode, country, created_at, updated_at, is_deleted)
    VALUES (source.id, source.user_id, source.line1, source.city, source.state, source.pincode, source.country, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT addresses OFF;
GO

-- =============================================
-- 7. Seed Orders (15 rows)
-- =============================================
SET IDENTITY_INSERT orders ON;

MERGE INTO orders AS target
USING (VALUES 
    (1, 4, 1, DATEADD(day, 3, GETDATE()), 'Pending', 1249.98, DATEADD(day, -20, GETDATE()), NULL, 0),
    (2, 5, 2, DATEADD(day, 2, GETDATE()), 'Shipped', 399.99, DATEADD(day, -18, GETDATE()), NULL, 0),
    (3, 6, 3, DATEADD(day, 4, GETDATE()), 'Delivered', 179.98, DATEADD(day, -15, GETDATE()), NULL, 0),
    (4, 7, 4, DATEADD(day, 5, GETDATE()), 'Delivered', 229.98, DATEADD(day, -14, GETDATE()), NULL, 0),
    (5, 8, 5, DATEADD(day, 1, GETDATE()), 'Pending', 89.99, DATEADD(day, -12, GETDATE()), NULL, 0),
    (6, 9, 6, DATEADD(day, 6, GETDATE()), 'Shipped', 549.97, DATEADD(day, -10, GETDATE()), NULL, 0),
    (7, 10, 7, DATEADD(day, 2, GETDATE()), 'Delivered', 149.99, DATEADD(day, -8, GETDATE()), NULL, 0),
    (8, 11, 8, DATEADD(day, 3, GETDATE()), 'Pending', 299.97, DATEADD(day, -6, GETDATE()), NULL, 0),
    (9, 12, 9, DATEADD(day, 4, GETDATE()), 'Shipped', 79.99, DATEADD(day, -5, GETDATE()), NULL, 0),
    (10, 13, 10, DATEADD(day, 5, GETDATE()), 'Delivered', 199.99, DATEADD(day, -4, GETDATE()), NULL, 0),
    (11, 14, 11, DATEADD(day, 6, GETDATE()), 'Pending', 129.99, DATEADD(day, -3, GETDATE()), NULL, 0),
    (12, 15, 12, DATEADD(day, 2, GETDATE()), 'Shipped', 99.99, DATEADD(day, -2, GETDATE()), NULL, 0),
    (13, 4, 13, DATEADD(day, 7, GETDATE()), 'Pending', 449.96, DATEADD(day, -1, GETDATE()), NULL, 0),
    (14, 5, 14, DATEADD(day, 3, GETDATE()), 'Pending', 79.99, DATEADD(day, 0, GETDATE()), NULL, 0),
    (15, 6, 15, DATEADD(day, 4, GETDATE()), 'Pending', 239.98, DATEADD(day, 0, GETDATE()), NULL, 0)
) AS source (id, customer_id, address_id, arriving_time, status, total_amount, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, customer_id, address_id, arriving_time, status, total_amount, created_at, updated_at, is_deleted)
    VALUES (source.id, source.customer_id, source.address_id, source.arriving_time, source.status, source.total_amount, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT orders OFF;
GO

-- =============================================
-- 8. Seed Order Items (20 rows)
-- =============================================
SET IDENTITY_INSERT order_items ON;

MERGE INTO order_items AS target
USING (VALUES 
    (1, 1, 1, 1, 999.99, DATEADD(day, -20, GETDATE()), NULL, 0),
    (2, 1, 2, 1, 249.99, DATEADD(day, -20, GETDATE()), NULL, 0),
    (3, 2, 3, 1, 399.99, DATEADD(day, -18, GETDATE()), NULL, 0),
    (4, 3, 11, 1, 179.99, DATEADD(day, -15, GETDATE()), NULL, 0),
    (5, 4, 6, 1, 149.99, DATEADD(day, -14, GETDATE()), NULL, 0),
    (6, 4, 7, 1, 49.99, DATEADD(day, -14, GETDATE()), NULL, 0),
    (7, 5, 10, 1, 89.99, DATEADD(day, -12, GETDATE()), NULL, 0),
    (8, 6, 1, 1, 999.99, DATEADD(day, -10, GETDATE()), NULL, 0),
    (9, 6, 14, 2, 79.99, DATEADD(day, -10, GETDATE()), NULL, 0),
    (10, 6, 15, 1, 59.99, DATEADD(day, -10, GETDATE()), NULL, 0),
    (11, 7, 6, 1, 149.99, DATEADD(day, -8, GETDATE()), NULL, 0),
    (12, 8, 2, 1, 249.99, DATEADD(day, -6, GETDATE()), NULL, 0),
    (13, 8, 3, 1, 49.99, DATEADD(day, -6, GETDATE()), NULL, 0),
    (14, 9, 14, 1, 79.99, DATEADD(day, -5, GETDATE()), NULL, 0),
    (15, 10, 9, 1, 129.99, DATEADD(day, -4, GETDATE()), NULL, 0),
    (16, 11, 8, 1, 69.99, DATEADD(day, -3, GETDATE()), NULL, 0),
    (17, 11, 13, 1, 59.99, DATEADD(day, -3, GETDATE()), NULL, 0),
    (18, 12, 15, 1, 99.99, DATEADD(day, -2, GETDATE()), NULL, 0),
    (19, 13, 3, 1, 399.99, DATEADD(day, -1, GETDATE()), NULL, 0),
    (20, 13, 11, 1, 49.99, DATEADD(day, -1, GETDATE()), NULL, 0)
) AS source (id, order_id, product_id, item_count, purchase_price, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, order_id, product_id, item_count, purchase_price, created_at, updated_at, is_deleted)
    VALUES (source.id, source.order_id, source.product_id, source.item_count, source.purchase_price, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT order_items OFF;
GO

-- =============================================
-- 9. Seed Feedback (15 rows)
-- =============================================
SET IDENTITY_INSERT feedback ON;

MERGE INTO feedback AS target
USING (VALUES 
    (1, 7, 1, 5, 'Absolutely love this smartphone! The camera quality is amazing and battery life exceeds expectations.', DATEADD(day, -15, GETDATE()), NULL, 0),
    (2, 8, 1, 4, 'Great phone overall. Slightly expensive but worth it for the features.', DATEADD(day, -14, GETDATE()), NULL, 0),
    (3, 9, 2, 5, 'Best wireless earbuds I have ever owned. Sound quality is incredible!', DATEADD(day, -13, GETDATE()), NULL, 0),
    (4, 10, 3, 4, 'Love the fitness tracking features. Battery could last a bit longer though.', DATEADD(day, -12, GETDATE()), NULL, 0),
    (5, 11, 4, 5, 'Super comfortable and fits perfectly. Will buy more colors.', DATEADD(day, -11, GETDATE()), NULL, 0),
    (6, 12, 5, 4, 'Good quality jeans. Fit is exactly as described.', DATEADD(day, -10, GETDATE()), NULL, 0),
    (7, 13, 6, 5, 'Perfect running shoes! Very lightweight and comfortable.', DATEADD(day, -9, GETDATE()), NULL, 0),
    (8, 14, 7, 4, 'Great yoga mat. Non-slip surface works perfectly.', DATEADD(day, -8, GETDATE()), NULL, 0),
    (9, 15, 8, 5, 'Excellent desk lamp. Multiple brightness levels are very useful.', DATEADD(day, -7, GETDATE()), NULL, 0),
    (10, 7, 9, 4, 'Makes great coffee every morning. Easy to use and clean.', DATEADD(day, -6, GETDATE()), NULL, 0),
    (11, 8, 10, 5, 'Amazing speaker! Sound quality is rich and bass is powerful.', DATEADD(day, -5, GETDATE()), NULL, 0),
    (12, 9, 11, 4, 'Great for gaming. Surround sound really enhances the experience.', DATEADD(day, -4, GETDATE()), NULL, 0),
    (13, 10, 12, 5, 'Perfect travel backpack. Lots of compartments and USB port is handy.', DATEADD(day, -3, GETDATE()), NULL, 0),
    (14, 11, 13, 5, 'Excellent cookware set. Heats evenly and easy to clean.', DATEADD(day, -2, GETDATE()), NULL, 0),
    (15, 12, 14, 4, 'Tracks everything accurately. Waterproof feature is great for swimming.', DATEADD(day, -1, GETDATE()), NULL, 0)
) AS source (id, user_id, product_id, rating, review, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, user_id, product_id, rating, review, created_at, updated_at, is_deleted)
    VALUES (source.id, source.user_id, source.product_id, source.rating, source.review, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT feedback OFF;
GO

-- =============================================
-- 10. Seed Payments (15 rows)
-- =============================================
SET IDENTITY_INSERT payments ON;

MERGE INTO payments AS target
USING (VALUES 
    (1, 1, 1249.98, 'UPI', 'Success', 'TXN001ABC123', DATEADD(day, -20, GETDATE()), DATEADD(day, -20, GETDATE()), NULL, 0),
    (2, 2, 399.99, 'NetBanking', 'Success', 'TXN002DEF456', DATEADD(day, -18, GETDATE()), DATEADD(day, -18, GETDATE()), NULL, 0),
    (3, 3, 179.98, 'CashOnDelivery', 'Success', NULL, DATEADD(day, -14, GETDATE()), DATEADD(day, -14, GETDATE()), NULL, 0),
    (4, 4, 229.98, 'UPI', 'Success', 'TXN004GHI789', DATEADD(day, -14, GETDATE()), DATEADD(day, -14, GETDATE()), NULL, 0),
    (5, 5, 89.99, 'NetBanking', 'Pending', NULL, NULL, DATEADD(day, -12, GETDATE()), NULL, 0),
    (6, 6, 549.97, 'UPI', 'Success', 'TXN006JKL012', DATEADD(day, -10, GETDATE()), DATEADD(day, -10, GETDATE()), NULL, 0),
    (7, 7, 149.99, 'CashOnDelivery', 'Success', NULL, DATEADD(day, -7, GETDATE()), DATEADD(day, -7, GETDATE()), NULL, 0),
    (8, 8, 299.97, 'UPI', 'Pending', NULL, NULL, DATEADD(day, -6, GETDATE()), NULL, 0),
    (9, 9, 79.99, 'NetBanking', 'Success', 'TXN009MNO345', DATEADD(day, -5, GETDATE()), DATEADD(day, -5, GETDATE()), NULL, 0),
    (10, 10, 199.99, 'UPI', 'Success', 'TXN010PQR678', DATEADD(day, -4, GETDATE()), DATEADD(day, -4, GETDATE()), NULL, 0),
    (11, 11, 129.99, 'CashOnDelivery', 'Pending', NULL, NULL, DATEADD(day, -3, GETDATE()), NULL, 0),
    (12, 12, 99.99, 'NetBanking', 'Success', 'TXN012STU901', DATEADD(day, -2, GETDATE()), DATEADD(day, -2, GETDATE()), NULL, 0),
    (13, 13, 449.96, 'UPI', 'Pending', NULL, NULL, DATEADD(day, -1, GETDATE()), NULL, 0),
    (14, 14, 79.99, 'NetBanking', 'Pending', NULL, NULL, DATEADD(day, 0, GETDATE()), NULL, 0),
    (15, 15, 239.98, 'UPI', 'Pending', NULL, NULL, DATEADD(day, 0, GETDATE()), NULL, 0)
) AS source (id, order_id, amount, payment_mode, status, transaction_id, paid_at, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, order_id, amount, payment_mode, status, transaction_id, paid_at, created_at, updated_at, is_deleted)
    VALUES (source.id, source.order_id, source.amount, source.payment_mode, source.status, source.transaction_id, source.paid_at, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT payments OFF;
GO

-- =============================================
-- 11. Seed Tokens (15 rows)
-- =============================================
SET IDENTITY_INSERT tokens ON;

MERGE INTO tokens AS target
USING (VALUES 
    (1, 4, 'verif_token_abc123def456', 'verification', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -1, GETDATE()), NULL, 0),
    (2, 5, 'verif_token_xyz789ghi012', 'verification', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -2, GETDATE()), NULL, 0),
    (3, 6, 'reset_token_pwd001abc', 'password_reset', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -3, GETDATE()), NULL, 0),
    (4, 7, 'refresh_token_r1s2t3u4', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -1, GETDATE()), NULL, 0),
    (5, 8, 'refresh_token_r5s6t7u8', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -1, GETDATE()), NULL, 0),
    (6, 9, 'verif_token_jkl345mno', 'verification', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -4, GETDATE()), NULL, 0),
    (7, 10, 'refresh_token_r9s0t1u2', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -1, GETDATE()), NULL, 0),
    (8, 11, 'reset_token_pwd002def', 'password_reset', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -5, GETDATE()), NULL, 0),
    (9, 12, 'refresh_token_r3s4t5u6', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -1, GETDATE()), NULL, 0),
    (10, 13, 'verif_token_pqr678stu', 'verification', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -6, GETDATE()), NULL, 0),
    (11, 14, 'refresh_token_r7s8t9u0', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -1, GETDATE()), NULL, 0),
    (12, 15, 'reset_token_pwd003ghi', 'password_reset', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -7, GETDATE()), NULL, 0),
    (13, 4, 'refresh_token_r1s2t3u4_v2', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(hour, -12, GETDATE()), NULL, 0),
    (14, 5, 'verif_token_vw9x0y1z2', 'verification', DATEADD(day, 1, GETDATE()), 0, DATEADD(hour, -8, GETDATE()), NULL, 0),
    (15, 6, 'refresh_token_r2s3t4u5', 'refresh_token', DATEADD(day, 7, GETDATE()), 0, DATEADD(day, -2, GETDATE()), NULL, 0)
) AS source (id, user_id, token, type, expiry, is_revoked, created_at, updated_at, is_deleted)
ON target.id = source.id
WHEN NOT MATCHED THEN
    INSERT (id, user_id, token, type, expiry, is_revoked, created_at, updated_at, is_deleted)
    VALUES (source.id, source.user_id, source.token, source.type, source.expiry, source.is_revoked, source.created_at, source.updated_at, source.is_deleted);

SET IDENTITY_INSERT tokens OFF;
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

PRINT '=============================================';
PRINT 'Database migration completed successfully!';
PRINT 'Dummy data seeded:';
PRINT '  - 15 Roles';
PRINT '  - 15 Users';
PRINT '  - 15 User Roles';
PRINT '  - 15 Categories';
PRINT '  - 15 Products';
PRINT '  - 15 Addresses';
PRINT '  - 15 Orders';
PRINT '  - 20 Order Items';
PRINT '  - 15 Feedback';
PRINT '  - 15 Payments';
PRINT '  - 15 Tokens';
PRINT '=============================================';
GO
