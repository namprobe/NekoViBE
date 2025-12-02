-- =============================================
-- Badge-Coupon Linking - Data Migration Script
-- Database: NekoViDb
-- Date: December 1, 2025
-- Purpose: Create coupons for each existing badge and link them
-- =============================================

USE [NekoViDb]
GO

DECLARE @CurrentDate DATETIME = GETUTCDATE()
DECLARE @SystemUser UNIQUEIDENTIFIER = 'F69BB602-7B2F-4090-9DD1-A4610EB03CEA'

-- =============================================
-- CREATE COUPONS FOR EACH BADGE
-- =============================================

-- 1. Naruto <parameter name="keyness"> (Badge ID from row 2)
DECLARE @CouponId1 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId1 UNIQUEIDENTIFIER = '45F1BD15-3D6C-445C-8693-08DDFB1D7EF6'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId1, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId1 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Naruto évnestiss', 
    0, 50.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId1, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId1

-- 2. Thành Viên Đồng (Badge ID from row 3)
DECLARE @CouponId2 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId2 UNIQUEIDENTIFIER = '15D26AB4-A8F2-4D9E-9194-0AF7B4F9C657'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId2, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId2 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Thành Viên Đồng', 
    0, 2.50, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId2, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId2

-- 3. Khách Hàng VIP (Badge ID from row 4)
DECLARE @CouponId3 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId3 UNIQUEIDENTIFIER = '49FEC928-E92A-4BEF-BFB4-131ECB9A522D'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId3, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId3 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Khách Hàng VIP', 
    0, 7.50, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId3, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId3

-- 4. Bậc Thầy Đánh Giá (Badge ID from row 5)
DECLARE @CouponId4 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId4 UNIQUEIDENTIFIER = 'F36DADC9-2CF1-4237-8210-139709BFE2AE'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId4, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId4 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Bậc Thầy Đánh Giá', 
    0, 3.50, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId4, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId4

-- 5. Thành Viên Bạch Kim (Badge ID from row 6)
DECLARE @CouponId5 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId5 UNIQUEIDENTIFIER = '04D0EFD8-56E3-4E14-9E50-146962FF34C4'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId5, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId5 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Thành Viên Bạch Kim', 
    0, 8.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId5, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId5

-- 6. Người Đánh Giá Tích Cực (Badge ID from row 7)
DECLARE @CouponId6 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId6 UNIQUEIDENTIFIER = 'D1285AB7-8C23-41EB-9ACF-158A10C9EE22'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId6, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId6 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Người Đánh Giá Tích Cực', 
    0, 2.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId6, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId6

-- 7. Người Mua Sắm Thường Xuyên (Badge ID from row 8)
DECLARE @CouponId7 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId7 UNIQUEIDENTIFIER = 'ECE07762-2D14-43DE-AAB8-5BAADBABE13C'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId7, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId7 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Người Mua Sắm Thường Xuyên', 
    0, 3.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId7, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId7

-- 8. Thành Viên Bạc (Badge ID from row 9)
DECLARE @CouponId8 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId8 UNIQUEIDENTIFIER = 'FA3BD437-E412-48EA-A67C-7108C6FFC926'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId8, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId8 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Thành Viên Bạc', 
    0, 4.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId8, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId8

-- 9. Chim Sớm (Badge ID from row 10)
DECLARE @CouponId9 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId9 UNIQUEIDENTIFIER = '5BBF31AC-00A5-4823-B8A5-72FCEDF17B08'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId9, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId9 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Chim Sớm', 
    0, 5.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId9, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId9

-- 10. Khách Hàng Thân Thiết (Badge ID from row 11)
DECLARE @CouponId10 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId10 UNIQUEIDENTIFIER = '3CC8F827-8933-4620-8F1F-7988EA7DD336'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId10, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId10 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Khách Hàng Thân Thiết', 
    0, 2.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId10, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId10

-- 11. Bước Đầu Tiên (Badge ID from row 12)
DECLARE @CouponId11 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId11 UNIQUEIDENTIFIER = 'A2FB7EFC-5B68-469E-9E69-7AEFE3D44BB8'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId11, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId11 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Bước Đầu Tiên', 
    0, 1.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId11, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId11

-- 12. Người Đánh Giá Đầu Tiên (Badge ID from row 13)
DECLARE @CouponId12 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId12 UNIQUEIDENTIFIER = '81BB1E14-C645-42F1-B7D6-852DC6313241'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId12, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId12 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Người Đánh Giá Đầu Tiên', 
    0, 1.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId12, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId12

-- 13. Người Mới Bắt Đầu (Badge ID from row 14)
DECLARE @CouponId13 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId13 UNIQUEIDENTIFIER = 'D6D5BE78-59D9-45AD-9C7A-8AD482605295'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId13, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId13 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Người Mới Bắt Đầu', 
    0, 1.50, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId13, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId13

-- 14. Ưu Tú Kim Cương (Badge ID from row 15)
DECLARE @CouponId14 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId14 UNIQUEIDENTIFIER = 'FB60CB72-5B97-4E6B-8BC8-A99CBB49A8FA'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId14, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId14 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Ưu Tú Kim Cương', 
    0, 10.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId14, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId14

-- 15. Chuyên Gia Mua Sắm (Badge ID from row 16)
DECLARE @CouponId15 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId15 UNIQUEIDENTIFIER = '59F9A2D0-1C54-4C00-B F70-C4449F392CA0'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId15, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId15 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Chuyên Gia Mua Sắm', 
    0, 5.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId15, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId15

-- 16. Người Đóng Góp Cộng Đồng (Badge ID from row 17)
DECLARE @CouponId16 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId16 UNIQUEIDENTIFIER = '889992A3-E11B-4077-A31F-C6DCB62C219E'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId16, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId16 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Người Đóng Góp Cộng Đồng', 
    0, 5.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId16, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId16

-- 17. Black Friday 2025 (Badge ID from row 18)
DECLARE @CouponId17 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId17 UNIQUEIDENTIFIER = 'FDCBDFB1-D048-49DD-A4D5-C8428E99ADF3'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId17, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId17 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Black Friday 2025', 
    0, 15.00, NULL, 0, '2025-11-28', '2025-11-30',
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId17, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId17

-- 18. Chuyên Gia Anime (Badge ID from row 19)
DECLARE @CouponId18 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId18 UNIQUEIDENTIFIER = 'ADC1D70F-AFF7-433C-B8BA-CC3780DA7917'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId18, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId18 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Chuyên Gia Anime', 
    0, 4.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId18, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId18

-- 19. Thành Viên Vàng (Badge ID from row 20)
DECLARE @CouponId19 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId19 UNIQUEIDENTIFIER = '3C6A5CBD-333A-48DC-B9E1-DBD6170A08BE'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId19, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId19 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Thành Viên Vàng', 
    0, 6.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId19, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId19

-- 20. Siêu Sưu Tầm (Badge ID from row 21)
DECLARE @CouponId20 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId20 UNIQUEIDENTIFIER = '0054DD41-C5AC-4B79-B8E8-EF539FE52B35'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId20, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId20 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Siêu Sưu Tầm', 
    0, 8.00, NULL, 0, @CurrentDate, DATEADD(YEAR, 10, @CurrentDate),
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId20, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId20

-- 21. Giáng Sinh 2025 (Badge ID from row 22)
DECLARE @CouponId21 UNIQUEIDENTIFIER = NEWID()
DECLARE @BadgeId21 UNIQUEIDENTIFIER = '6FA2BBEC-8A11-48A8-BFA5-EFEDB0AEF02C'

INSERT INTO [dbo].[Coupons] (
    [Id], [Code], [Description], [DiscountType], [DiscountValue],
    [MaxDiscountCap], [MinOrderAmount], [StartDate], [EndDate],
    [UsageLimit], [CurrentUsage], [IsBadgeCoupon],
    [CreatedAt], [CreatedBy], [UpdatedAt], [UpdatedBy],
    [IsDeleted], [DeletedBy], [DeletedAt], [Status]
) VALUES (
    @CouponId21, 'SYS_BADGE_' + REPLACE(CAST(@BadgeId21 AS VARCHAR(50)), '-', ''), 
    N'Auto-generated coupon for badge: Giáng Sinh 2025', 
    0, 12.00, NULL, 0, '2025-12-20', '2025-12-26',
    NULL, 0, 1,
    @CurrentDate, @SystemUser, @CurrentDate, @SystemUser,
    0, NULL, NULL, 1
)

UPDATE [dbo].[Badges] SET [LinkedCouponId] = @CouponId21, [UpdatedAt] = @CurrentDate WHERE [Id] = @BadgeId21

GO

-- =============================================
-- VERIFICATION QUERIES
-- =============================================

PRINT '============================================='
PRINT 'BADGE-COUPON LINKING VERIFICATION'
PRINT '============================================='
PRINT ''

-- Count linked badges
SELECT 
    'Total Badges Linked to Coupons' AS [Metric],
    COUNT(*) AS [Count]
FROM [dbo].[Badges]
WHERE [LinkedCouponId] IS NOT NULL

-- View all badge-coupon pairs
SELECT 
    b.[Id] AS [BadgeId],
    b.[Name] AS [BadgeName],
    b.[DiscountPercentage] AS [BadgeDiscount],
    c.[Id] AS [CouponId],
    c.[Code] AS [CouponCode],
    c.[DiscountValue] AS [CouponDiscount],
    c.[IsBadgeCoupon],
    CASE WHEN b.[LinkedCouponId] = c.[Id] THEN 'Linked' ELSE 'Not Linked' END AS [LinkStatus]
FROM [dbo].[Badges] b
LEFT JOIN [dbo].[Coupons] c ON b.[LinkedCouponId] = c.[Id]
WHERE b.[Status] = 1
ORDER BY b.[Name]

-- Count badge coupons
SELECT 
    'Total Badge Coupons Created' AS [Metric],
    COUNT(*) AS [Count]
FROM [dbo].[Coupons]
WHERE [IsBadgeCoupon] = 1

-- Check for any unlinked badges
SELECT 
    [Id],
    [Name],
    [DiscountPercentage],
    'WARNING: No linked coupon' AS [Status]
FROM [dbo].[Badges]
WHERE [LinkedCouponId] IS NULL
    AND [Status] = 1

GO

PRINT ''
PRINT '============================================='
PRINT 'MIGRATION COMPLETE'
PRINT '21 coupons created and linked to 21 badges'
PRINT '============================================='
