-- =============================================
-- Badge System - Seed Data Script
-- Database: NekoViDb
-- Date: November 30, 2025
-- Purpose: Create 20 badges and assign 3 to test user
-- =============================================

USE [NekoViDb]
GO

DECLARE @UserId UNIQUEIDENTIFIER = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'
DECLARE @CurrentDate DATETIME = GETUTCDATE()
DECLARE @SystemUser UNIQUEIDENTIFIER = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA' -- Using test user as creator

-- =============================================
-- INSERT 20 BADGES
-- =============================================

-- 1. First Steps Badge (OrderCount: 1)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Bước Đầu Tiên', N'Hoàn thành đơn hàng đầu tiên của bạn', N'/badges/first-order.png', 1.0,
    0, '1', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 2. Regular Customer (OrderCount: 5)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Khách Hàng Thân Thiết', N'Hoàn thành 5 đơn hàng', N'/badges/regular-customer.png', 2.0,
    0, '5', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 3. Frequent Buyer (OrderCount: 10)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Người Mua Sắm Thường Xuyên', N'Hoàn thành 10 đơn hàng', N'/badges/frequent-buyer.png', 3.0,
    0, '10', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 4. Shopping Expert (OrderCount: 25)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Chuyên Gia Mua Sắm', N'Hoàn thành 25 đơn hàng', N'/badges/shopping-expert.png', 5.0,
    0, '25', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 5. VIP Customer (OrderCount: 50)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Khách Hàng VIP', N'Hoàn thành 50 đơn hàng', N'/badges/vip-customer.png', 7.5,
    0, '50', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 6. Starter Spender (TotalSpent: 500,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Người Mới Bắt Đầu', N'Chi tiêu trên 500,000 VND', N'/badges/starter-spender.png', 1.5,
    1, '500000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 7. Bronze Member (TotalSpent: 1,000,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Thành Viên Đồng', N'Chi tiêu trên 1,000,000 VND', N'/badges/bronze-member.png', 2.5,
    1, '1000000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 8. Silver Member (TotalSpent: 3,000,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Thành Viên Bạc', N'Chi tiêu trên 3,000,000 VND', N'/badges/silver-member.png', 4.0,
    1, '3000000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 9. Gold Member (TotalSpent: 5,000,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Thành Viên Vàng', N'Chi tiêu trên 5,000,000 VND', N'/badges/gold-member.png', 6.0,
    1, '5000000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 10. Platinum Member (TotalSpent: 10,000,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Thành Viên Bạch Kim', N'Chi tiêu trên 10,000,000 VND', N'/badges/platinum-member.png', 8.0,
    1, '10000000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 11. Diamond Elite (TotalSpent: 20,000,000 VND)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Ưu Tú Kim Cương', N'Chi tiêu trên 20,000,000 VND', N'/badges/diamond-elite.png', 10.0,
    1, '20000000', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 12. First Reviewer (ReviewCount: 1)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Người Đánh Giá Đầu Tiên', N'Viết đánh giá sản phẩm đầu tiên', N'/badges/first-reviewer.png', 1.0,
    2, '1', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 13. Active Reviewer (ReviewCount: 5)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Người Đánh Giá Tích Cực', N'Viết 5 đánh giá sản phẩm', N'/badges/active-reviewer.png', 2.0,
    2, '5', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 14. Review Master (ReviewCount: 10)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Bậc Thầy Đánh Giá', N'Viết 10 đánh giá sản phẩm', N'/badges/review-master.png', 3.5,
    2, '10', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 15. Community Contributor (ReviewCount: 25)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Người Đóng Góp Cộng Đồng', N'Viết 25 đánh giá sản phẩm', N'/badges/community-contributor.png', 5.0,
    2, '25', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 16. Black Friday 2025 (Time-Limited Custom)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Black Friday 2025', N'Tham gia sự kiện Black Friday 2025', N'/badges/black-friday-2025.png', 15.0,
    3, 'BlackFriday2025', 1, '2025-11-28', '2025-11-30',
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 17. Early Bird (Custom)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Chim Sớm', N'Người dùng đầu tiên của hệ thống', N'/badges/early-bird.png', 5.0,
    3, 'EarlyBird', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 18. Christmas 2025 (Time-Limited Custom)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Giáng Sinh 2025', N'Tham gia sự kiện Giáng Sinh 2025', N'/badges/christmas-2025.png', 12.0,
    3, 'Christmas2025', 1, '2025-12-20', '2025-12-26',
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 19. Anime Expert (Custom)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Chuyên Gia Anime', N'Kiến thức sâu rộng về Anime', N'/badges/anime-expert.png', 4.0,
    3, 'AnimeExpert', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

-- 20. Super Collector (Custom)
INSERT INTO [dbo].[Badges] (
    [Id], [Name], [Description], [IconPath], [DiscountPercentage],
    [ConditionType], [ConditionValue], [IsTimeLimited], [StartDate], [EndDate],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), N'Siêu Sưu Tầm', N'Bộ sưu tập đặc biệt và hiếm', N'/badges/super-collector.png', 8.0,
    3, 'SuperCollector', 0, NULL, NULL,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

GO

-- =============================================
-- ASSIGN 3 BADGES TO TEST USER
-- =============================================

DECLARE @UserId UNIQUEIDENTIFIER = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'
DECLARE @CurrentDate DATETIME = GETUTCDATE()
DECLARE @SystemUser UNIQUEIDENTIFIER = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'

-- Get Badge IDs (we'll assign Bronze Member, First Reviewer, and Early Bird)
DECLARE @BronzeMemberId UNIQUEIDENTIFIER
DECLARE @FirstReviewerId UNIQUEIDENTIFIER
DECLARE @EarlyBirdId UNIQUEIDENTIFIER

SELECT @BronzeMemberId = Id FROM [dbo].[Badges] WHERE [Name] = N'Thành Viên Đồng'
SELECT @FirstReviewerId = Id FROM [dbo].[Badges] WHERE [Name] = N'Người Đánh Giá Đầu Tiên'
SELECT @EarlyBirdId = Id FROM [dbo].[Badges] WHERE [Name] = N'Chim Sớm'

-- Assign Badge 1: Bronze Member (equipped)
INSERT INTO [dbo].[UserBadges] (
    [Id], [UserId], [BadgeId], [EarnedDate], [IsActive],
    [ActivatedFrom], [ActivatedTo],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), @UserId, @BronzeMemberId, DATEADD(DAY, -15, @CurrentDate), 1,
    DATEADD(DAY, -10, @CurrentDate), NULL,
    DATEADD(DAY, -15, @CurrentDate), @SystemUser, DATEADD(DAY, -10, @CurrentDate), @SystemUser,
    0, NULL, NULL, 1
)

-- Assign Badge 2: First Reviewer (not equipped)
INSERT INTO [dbo].[UserBadges] (
    [Id], [UserId], [BadgeId], [EarnedDate], [IsActive],
    [ActivatedFrom], [ActivatedTo],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), @UserId, @FirstReviewerId, DATEADD(DAY, -7, @CurrentDate), 0,
    NULL, NULL,
    DATEADD(DAY, -7, @CurrentDate), @SystemUser, DATEADD(DAY, -7, @CurrentDate), @SystemUser,
    0, NULL, NULL, 1
)

-- Assign Badge 3: Early Bird (not equipped)
INSERT INTO [dbo].[UserBadges] (
    [Id], [UserId], [BadgeId], [EarnedDate], [IsActive],
    [ActivatedFrom], [ActivatedTo],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    NEWID(), @UserId, @EarlyBirdId, DATEADD(DAY, -30, @CurrentDate), 0,
    NULL, NULL,
    DATEADD(DAY, -30, @CurrentDate), @SystemUser, DATEADD(DAY, -30, @CurrentDate), @SystemUser,
    0, NULL, NULL, 1
)

GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

-- View all created badges
SELECT 
    [Id],
    [Name],
    [Description],
    [DiscountPercentage],
    CASE [ConditionType]
        WHEN 0 THEN 'OrderCount'
        WHEN 1 THEN 'TotalSpent'
        WHEN 2 THEN 'ReviewCount'
        WHEN 3 THEN 'Custom'
        ELSE 'Unknown'
    END AS [ConditionType],
    [ConditionValue],
    [IsTimeLimited],
    CASE [Status]
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Inactive'
        ELSE 'Unknown'
    END AS [Status]
FROM [dbo].[Badges]
ORDER BY [ConditionType], CAST([ConditionValue] AS DECIMAL(18,2))

-- View user's assigned badges
SELECT 
    ub.[Id] AS [UserBadgeId],
    b.[Name] AS [BadgeName],
    b.[Description],
    b.[DiscountPercentage],
    ub.[EarnedDate],
    ub.[IsActive] AS [IsEquipped],
    ub.[ActivatedFrom],
    ub.[ActivatedTo],
    CASE ub.[Status]
        WHEN 1 THEN 'Active'
        WHEN 2 THEN 'Inactive'
        ELSE 'Unknown'
    END AS [Status]
FROM [dbo].[UserBadges] ub
INNER JOIN [dbo].[Badges] b ON ub.[BadgeId] = b.[Id]
WHERE ub.[UserId] = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'
ORDER BY ub.[EarnedDate] DESC

-- Summary statistics
SELECT 
    'Total Badges Created' AS [Metric],
    COUNT(*) AS [Count]
FROM [dbo].[Badges]
WHERE [Status] = 1

UNION ALL

SELECT 
    'Badges Assigned to User' AS [Metric],
    COUNT(*) AS [Count]
FROM [dbo].[UserBadges]
WHERE [UserId] = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'
    AND [Status] = 1

UNION ALL

SELECT 
    'Equipped Badge' AS [Metric],
    COUNT(*) AS [Count]
FROM [dbo].[UserBadges]
WHERE [UserId] = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'
    AND [IsActive] = 1
    AND [Status] = 1

GO

-- =============================================
-- SCRIPT COMPLETE
-- =============================================
-- Summary:
-- - 20 badges created covering all condition types
-- - 3 badges assigned to user F69BB602-7B2F-4090-9DD1-A4610EB03CEA
--   * Bronze Member (equipped)
--   * First Reviewer (unlocked)
--   * Early Bird (unlocked)
-- =============================================
