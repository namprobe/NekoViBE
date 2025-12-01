-- ============================================================================
-- CLEANUP DUPLICATE UserCoupon ENTRIES
-- ============================================================================
-- Purpose: Remove duplicate UserCoupon entries that were created due to 
--          race conditions in the sync process
-- Date: 2025-12-01
-- ============================================================================

SET QUOTED_IDENTIFIER ON;
GO

USE NekoViDb;
GO

-- Step 1: Find duplicates
PRINT '=== STEP 1: Checking for duplicate UserCoupons ===';
SELECT 
    uc.UserId,
    uc.CouponId,
    c.Code AS CouponCode,
    COUNT(*) AS DuplicateCount
FROM UserCoupons uc
INNER JOIN Coupons c ON uc.CouponId = c.Id
WHERE uc.Status = 1  -- Active only
GROUP BY uc.UserId, uc.CouponId, c.Code
HAVING COUNT(*) > 1
ORDER BY DuplicateCount DESC;

-- Step 2: Show which records will be kept (earliest CreatedAt)
PRINT '';
PRINT '=== STEP 2: Records that will be KEPT (earliest created) ===';
WITH RankedCoupons AS (
    SELECT 
        uc.Id,
        uc.UserId,
        uc.CouponId,
        c.Code AS CouponCode,
        uc.CreatedAt,
        ROW_NUMBER() OVER (
            PARTITION BY uc.UserId, uc.CouponId 
            ORDER BY uc.CreatedAt ASC
        ) AS RowNum
    FROM UserCoupons uc
    INNER JOIN Coupons c ON uc.CouponId = c.Id
    WHERE uc.Status = 1
)
SELECT 
    Id,
    UserId,
    CouponId,
    CouponCode,
    CreatedAt,
    'KEEP' AS Action
FROM RankedCoupons
WHERE RowNum = 1
    AND CouponId IN (
        SELECT CouponId 
        FROM RankedCoupons 
        GROUP BY UserId, CouponId 
        HAVING COUNT(*) > 1
    )
ORDER BY UserId, CouponCode;

-- Step 3: Show which records will be DELETED
PRINT '';
PRINT '=== STEP 3: Records that will be DELETED (duplicates) ===';
WITH RankedCoupons AS (
    SELECT 
        uc.Id,
        uc.UserId,
        uc.CouponId,
        c.Code AS CouponCode,
        uc.CreatedAt,
        ROW_NUMBER() OVER (
            PARTITION BY uc.UserId, uc.CouponId 
            ORDER BY uc.CreatedAt ASC
        ) AS RowNum
    FROM UserCoupons uc
    INNER JOIN Coupons c ON uc.CouponId = c.Id
    WHERE uc.Status = 1
)
SELECT 
    Id,
    UserId,
    CouponId,
    CouponCode,
    CreatedAt,
    'DELETE' AS Action
FROM RankedCoupons
WHERE RowNum > 1
ORDER BY UserId, CouponCode, RowNum;

-- Step 4: BACKUP - Create a backup table before deletion
PRINT '';
PRINT '=== STEP 4: Creating backup of duplicate records ===';
IF OBJECT_ID('UserCoupons_Duplicates_Backup', 'U') IS NOT NULL
    DROP TABLE UserCoupons_Duplicates_Backup;

WITH RankedCoupons AS (
    SELECT 
        uc.*,
        ROW_NUMBER() OVER (
            PARTITION BY uc.UserId, uc.CouponId 
            ORDER BY uc.CreatedAt ASC
        ) AS RowNum
    FROM UserCoupons uc
    WHERE uc.Status = 1
)
SELECT * 
INTO UserCoupons_Duplicates_Backup
FROM RankedCoupons
WHERE RowNum > 1;

PRINT CONCAT('Backed up ', @@ROWCOUNT, ' duplicate records to UserCoupons_Duplicates_Backup');

-- Step 5: DELETE duplicates (keep the earliest created one)
PRINT '';
PRINT '=== STEP 5: Deleting duplicate UserCoupons ===';
BEGIN TRANSACTION;

DECLARE @DeletedCount INT;

WITH RankedCoupons AS (
    SELECT 
        uc.Id,
        ROW_NUMBER() OVER (
            PARTITION BY uc.UserId, uc.CouponId 
            ORDER BY uc.CreatedAt ASC
        ) AS RowNum
    FROM UserCoupons uc
    WHERE uc.Status = 1
)
DELETE FROM UserCoupons
WHERE Id IN (
    SELECT Id 
    FROM RankedCoupons 
    WHERE RowNum > 1
);

SET @DeletedCount = @@ROWCOUNT;
PRINT CONCAT('Deleted ', @DeletedCount, ' duplicate UserCoupon records');

-- Step 6: Verify no duplicates remain
PRINT '';
PRINT '=== STEP 6: Verification - Checking for remaining duplicates ===';
SELECT 
    uc.UserId,
    uc.CouponId,
    c.Code AS CouponCode,
    COUNT(*) AS Count
FROM UserCoupons uc
INNER JOIN Coupons c ON uc.CouponId = c.Id
WHERE uc.Status = 1
GROUP BY uc.UserId, uc.CouponId, c.Code
HAVING COUNT(*) > 1;

IF @@ROWCOUNT = 0
BEGIN
    PRINT '✅ SUCCESS: No duplicates found after cleanup!';
    COMMIT TRANSACTION;
    PRINT '✅ Transaction committed successfully.';
END
ELSE
BEGIN
    PRINT '❌ WARNING: Duplicates still exist! Rolling back transaction.';
    ROLLBACK TRANSACTION;
    PRINT '❌ Transaction rolled back.';
END

GO

-- Step 7: Unique constraint has been added via EF Core migration
PRINT '';
PRINT '=== STEP 7: Database Protection ===';
PRINT '✅ Unique constraint UX_UserCoupons_UserId_CouponId_Active has been added via migration.';
PRINT '✅ This prevents future duplicates at the database level.';
PRINT '✅ Any attempt to insert duplicate active UserCoupon will be rejected by the database.';

GO

PRINT '';
PRINT '=== CLEANUP COMPLETE ===';
PRINT 'To restore duplicates if needed, query: SELECT * FROM UserCoupons_Duplicates_Backup';
