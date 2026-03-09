-- Script tạo Roles cho ứng dụng TechPro
-- Chạy script này trong SQL Server Management Studio sau khi đã tạo database

USE TechProDb;
GO

-- Xóa roles cũ nếu có (tùy chọn)
-- DELETE FROM AspNetRoles WHERE Name IN ('SystemAdmin', 'StoreAdmin', 'Technician', 'Support');

-- Tạo các Roles
IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'SystemAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'SystemAdmin', 'SYSTEMADMIN', NEWID());
    PRINT 'Đã tạo role: SystemAdmin';
END
ELSE
    PRINT 'Role SystemAdmin đã tồn tại';

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'StoreAdmin')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'StoreAdmin', 'STOREADMIN', NEWID());
    PRINT 'Đã tạo role: StoreAdmin';
END
ELSE
    PRINT 'Role StoreAdmin đã tồn tại';

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Technician')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Technician', 'TECHNICIAN', NEWID());
    PRINT 'Đã tạo role: Technician';
END
ELSE
    PRINT 'Role Technician đã tồn tại';

IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Name = 'Support')
BEGIN
    INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
    VALUES (NEWID(), 'Support', 'SUPPORT', NEWID());
    PRINT 'Đã tạo role: Support';
END
ELSE
    PRINT 'Role Support đã tồn tại';

PRINT 'Hoàn tất tạo roles!';
GO

