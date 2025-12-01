-- =============================================
-- Backfill Badge Coupons for Existing UserBadges
-- Database: NekoViDb
-- Date: December 1, 2025
-- Purpose: Add badge coupons to UserCoupons for users who already have badges
-- =============================================

USE [NekoViDb]
GO

PRINT '============================================='
PRINT 'BACKFILL BADGE COUPONS - START'
PRINT '============================================='
PRINT ''

-- Check current state
PRINT '📊 CURRENT STATE:'
PRINT '----------------'

SELECT 
    'Total UserBadges (Active)' AS Metric,
    COUNT(*) AS Count
FROM UserBadges
WHERE Status = 1

UNION ALL

SELECT 
    'Badges with LinkedCouponId' AS Metric,
    COUNT(*) AS Count
FROM Badges
WHERE LinkedCouponId IS NOT NULL

UNION ALL

SELECT 
    'Total UserCoupons' AS Metric,
    COUNT(*) AS Count
FROM UserCoupons

GO

PRINT ''
PRINT '🔍 FINDING MISSING BADGE COUPONS...'
PRINT '------------------------------------'

-- Show users who have badges but missing the linked coupons
SELECT 
    ub.UserId,
    u.Email,
    b.Name AS BadgeName,
    b.LinkedCouponId,
    c.Code AS CouponCode,
    c.DiscountValue,
    'MISSING' AS Status
FROM UserBadges ub
INNER JOIN Badges b ON ub.BadgeId = b.Id
INNER JOIN AspNetUsers u ON ub.UserId = u.Id
LEFT JOIN Coupons c ON b.LinkedCouponId = c.Id
WHERE b.LinkedCouponId IS NOT NULL
  AND ub.Status = 1
  AND NOT EXISTS (
      SELECT 1 
      FROM UserCoupons uc 
      WHERE uc.UserId = ub.UserId 
        AND uc.CouponId = b.LinkedCouponId
        AND uc.Status = 1
  )
ORDER BY u.Email, b.Name

GO

PRINT ''
PRINT '➕ INSERTING MISSING BADGE COUPONS...'
PRINT '--------------------------------------'

DECLARE @InsertedCount INT = 0

-- Insert missing UserCoupons for badge holders
INSERT INTO UserCoupons (Id, UserId, CouponId, Status, CreatedAt, CreatedBy)
SELECT 
    NEWID() AS Id,
    ub.UserId,
    b.LinkedCouponId AS CouponId,
    1 AS Status, -- Active
    GETUTCDATE() AS CreatedAt,
    ub.UserId AS CreatedBy
FROM UserBadges ub
INNER JOIN Badges b ON ub.BadgeId = b.Id
WHERE b.LinkedCouponId IS NOT NULL
  AND ub.Status = 1
  AND NOT EXISTS (
      SELECT 1 
      FROM UserCoupons uc 
      WHERE uc.UserId = ub.UserId 
        AND uc.CouponId = b.LinkedCouponId
        AND uc.Status = 1
  )

SET @InsertedCount = @@ROWCOUNT

PRINT '✅ Inserted ' + CAST(@InsertedCount AS VARCHAR) + ' badge coupons into UserCoupons'

GO

PRINT ''
PRINT '📊 FINAL STATE:'
PRINT '----------------'

SELECT 
    'Total UserCoupons (After)' AS Metric,
    COUNT(*) AS Count
FROM UserCoupons
WHERE Status = 1

UNION ALL

SELECT 
    'Badge Coupons (IsBadgeCoupon)' AS Metric,
    COUNT(DISTINCT uc.Id) AS Count
FROM UserCoupons uc
INNER JOIN Coupons c ON uc.CouponId = c.Id
WHERE c.IsBadgeCoupon = 1
  AND uc.Status = 1

GO

PRINT ''
PRINT '🔍 VERIFICATION - Users with Badges and Their Coupons:'
PRINT '------------------------------------------------------'

SELECT 
    u.Email,
    b.Name AS BadgeName,
    c.Code AS CouponCode,
    c.DiscountValue,
    uc.CreatedAt AS CollectedDate,
    CASE WHEN uc.Id IS NOT NULL THEN 'HAS COUPON ✅' ELSE 'MISSING ❌' END AS Status
FROM UserBadges ub
INNER JOIN Badges b ON ub.BadgeId = b.Id
INNER JOIN AspNetUsers u ON ub.UserId = u.Id
LEFT JOIN Coupons c ON b.LinkedCouponId = c.Id
LEFT JOIN UserCoupons uc ON uc.UserId = ub.UserId AND uc.CouponId = b.LinkedCouponId AND uc.Status = 1
WHERE b.LinkedCouponId IS NOT NULL
  AND ub.Status = 1
ORDER BY u.Email, b.Name

GO

PRINT ''
PRINT '============================================='
PRINT 'BACKFILL COMPLETE!'
PRINT '============================================='
PRINT ''
PRINT 'Next steps:'
PRINT '1. Verify coupons appear in GET /api/customer/coupons/my-coupons'
PRINT '2. Check frontend "My Coupons" page shows badge coupons'
PRINT '3. Future badges will auto-collect coupons when awarded'
PRINT ''
