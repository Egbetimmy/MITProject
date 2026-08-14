-- 1. Seed Users (10,000 rows) in UserDb
USE UserDb;
TRUNCATE TABLE Users;

WITH
  t1 AS (SELECT 1 AS n UNION ALL SELECT 1),
  t2 AS (SELECT 1 AS n FROM t1 CROSS JOIN t1 AS b),
  t3 AS (SELECT 1 AS n FROM t2 CROSS JOIN t2 AS b),
  t4 AS (SELECT 1 AS n FROM t3 CROSS JOIN t3 AS b),
  t5 AS (SELECT 1 AS n FROM t4 CROSS JOIN t4 AS b),
  t6 AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num FROM t5)
INSERT INTO Users (Email, FullName, CreatedAt)
SELECT 
    'user' + CAST(row_num AS VARCHAR(20)) + '@example.com',
    'User ' + CAST(row_num AS VARCHAR(20)),
    GETUTCDATE()
FROM t6
WHERE row_num <= 10000;

-- 2. Seed Products (50,000 rows) in ProductDb
USE ProductDb;
TRUNCATE TABLE Products;

WITH
  t1 AS (SELECT 1 AS n UNION ALL SELECT 1),
  t2 AS (SELECT 1 AS n FROM t1 CROSS JOIN t1 AS b),
  t3 AS (SELECT 1 AS n FROM t2 CROSS JOIN t2 AS b),
  t4 AS (SELECT 1 AS n FROM t3 CROSS JOIN t3 AS b),
  t5 AS (SELECT 1 AS n FROM t4 CROSS JOIN t4 AS b),
  t6 AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num FROM t5)
INSERT INTO Products (Name, Description, Price, Stock, CreatedAt)
SELECT 
    'Product ' + CAST(row_num AS VARCHAR(20)),
    'Description for product ' + CAST(row_num AS VARCHAR(20)),
    ROUND(10.0 + (RAND(CHECKSUM(NEWID())) * 990.0), 2),
    CAST(10 + (RAND(CHECKSUM(NEWID())) * 1000) AS INT),
    GETUTCDATE()
FROM t6
WHERE row_num <= 50000;

-- 3. Seed Orders (100,000 rows) in OrderDb
USE OrderDb;
TRUNCATE TABLE Orders;

WITH
  t1 AS (SELECT 1 AS n UNION ALL SELECT 1),
  t2 AS (SELECT 1 AS n FROM t1 CROSS JOIN t1 AS b),
  t3 AS (SELECT 1 AS n FROM t2 CROSS JOIN t2 AS b),
  t4 AS (SELECT 1 AS n FROM t3 CROSS JOIN t3 AS b),
  t5 AS (SELECT 1 AS n FROM t4 CROSS JOIN t4 AS b),
  t6 AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num FROM t5),
  t7 AS (SELECT a.row_num AS r1, b.row_num AS r2 FROM t6 a CROSS JOIN (SELECT TOP 2 row_num FROM t6) b),
  t8 AS (SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS row_num FROM t7)
INSERT INTO Orders (UserId, ProductId, Quantity, TotalAmount, Status, CreatedAt)
SELECT 
    CAST(1 + (RAND(CHECKSUM(NEWID())) * 9999) AS INT),
    CAST(1 + (RAND(CHECKSUM(NEWID())) * 49999) AS INT),
    CAST(1 + (RAND(CHECKSUM(NEWID())) * 5) AS INT),
    ROUND(5.0 + (RAND(CHECKSUM(NEWID())) * 500.0), 2),
    CASE CAST(RAND(CHECKSUM(NEWID())) * 3 AS INT)
        WHEN 0 THEN 'Pending'
        WHEN 1 THEN 'Completed'
        ELSE 'Shipped'
    END,
    DATEADD(second, -CAST(RAND(CHECKSUM(NEWID())) * 2592000 AS INT), GETUTCDATE())
FROM t8
WHERE row_num <= 100000;

-- Verify Row Counts
USE UserDb;
SELECT 'Users Count' AS TableName, COUNT(*) AS [TotalRows] FROM Users;
USE ProductDb;
SELECT 'Products Count' AS TableName, COUNT(*) AS [TotalRows] FROM Products;
USE OrderDb;
SELECT 'Orders Count' AS TableName, COUNT(*) AS [TotalRows] FROM Orders;
