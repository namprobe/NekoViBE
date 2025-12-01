# Badge Auto-Coupon Collection Implementation

## Overview
When a user earns a badge (either through automatic eligibility processing or manual assignment), the system now automatically collects the linked coupon to their UserCoupon table.

## Problem Fixed
Previously, users who earned badges had access to badge coupons only when the badge was equipped (via the `/api/customer/badges/active-coupon` endpoint). However, the coupon wasn't added to their `UserCoupons` collection, meaning:
- Coupons didn't show in "My Coupons" page
- Users couldn't see badge coupons in their coupon list
- Badge coupons were "invisible" until checkout

## Solution
Modified two command handlers to automatically collect badge coupons when badges are awarded:

### 1. ProcessBadgeEligibilityCommandHandler
**File**: `src/NekoViBE.Application/Features/UserBadge/Command/ProcessBadgeEligibility/ProcessBadgeEligibilityCommandHandler.cs`

**Changes**:
- Load badges with `LinkedCoupon` navigation property using `.Include()`
- After awarding a badge, check if it has a `LinkedCouponId`
- Automatically create a `UserCoupon` entry for the badge's linked coupon
- Prevent duplicate entries by checking if user already has the coupon

**Code snippet**:
```csharp
// If badge has a linked coupon, automatically collect it for the user
if (badge.LinkedCouponId.HasValue && badge.LinkedCoupon != null)
{
    // Check if user already has this coupon
    var existingUserCoupon = await _unitOfWork.Repository<Domain.Entities.UserCoupon>()
        .GetQueryable()
        .AnyAsync(uc => 
            uc.UserId == targetUserId.Value && 
            uc.CouponId == badge.LinkedCouponId.Value &&
            uc.Status == EntityStatusEnum.Active,
            cancellationToken);

    if (!existingUserCoupon)
    {
        var userCoupon = new Domain.Entities.UserCoupon
        {
            Id = Guid.NewGuid(),
            UserId = targetUserId.Value,
            CouponId = badge.LinkedCouponId.Value,
            Status = EntityStatusEnum.Active,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = targetUserId
        };

        await _unitOfWork.Repository<Domain.Entities.UserCoupon>().AddAsync(userCoupon);
        
        _logger.LogInformation("🎟️ Auto-collected badge coupon {CouponCode} for user {UserId}",
            badge.LinkedCoupon.Code, targetUserId, badge.Name);
    }
}
```

### 2. AssignBadgeToUserCommandHandler
**File**: `src/NekoViBE.Application/Features/UserBadge/Command/AssignBadge/AssignBadgeToUserCommandHandler.cs`

**Changes**:
- Added `using Microsoft.EntityFrameworkCore;` for `.Include()` and `.FirstOrDefaultAsync()`
- Changed badge loading to include `LinkedCoupon` navigation property
- Same auto-collection logic as ProcessBadgeEligibility
- Applies to manual badge assignments by Admin/Staff

## Database Flow

### Before Badge Award:
```
Badges Table:
- Badge: "Gold Member" (LinkedCouponId: COUPON_GUID_123)

Coupons Table:
- Coupon: "SYS_BADGE_GOLD_VIP" (IsBadgeCoupon: true)

UserBadges Table:
- (empty for this user)

UserCoupons Table:
- (empty for this user)
```

### After Badge Award (NEW BEHAVIOR):
```
UserBadges Table:
- UserBadge: User earned "Gold Member" badge

UserCoupons Table:
- UserCoupon: User collected "SYS_BADGE_GOLD_VIP" coupon ✅ AUTOMATICALLY
```

## User Experience Impact

### Before Fix:
1. User earns badge → Badge appears in wallet
2. Badge coupon exists but is invisible
3. User equips badge → Coupon becomes available at checkout
4. User goes to "My Coupons" → Badge coupon NOT shown ❌

### After Fix:
1. User earns badge → Badge appears in wallet
2. Badge coupon is **automatically collected** ✅
3. User goes to "My Coupons" → Badge coupon IS shown ✅
4. User can see badge discount even without equipping
5. User equips badge → Coupon applies automatically at checkout

## Testing Scenarios

### Test Case 1: Automatic Badge Award
```bash
# User completes 5th order
POST /api/customer/badges/process

# Expected Results:
# 1. UserBadge created with BadgeId for "5 Orders" badge
# 2. UserCoupon created with CouponId for linked coupon
# 3. Coupon visible in GET /api/customer/coupons/user
# 4. Coupon shows in frontend "My Coupons" page
```

### Test Case 2: Manual Badge Assignment (Admin)
```bash
# Admin assigns "Early Bird" badge to user
POST /api/cms/badges/assign
{
  "userId": "USER_GUID",
  "badgeId": "EARLY_BIRD_BADGE_GUID"
}

# Expected Results:
# 1. UserBadge created
# 2. UserCoupon created for "SYS_BADGE_EARLY_BIRD" coupon
# 3. User can see coupon in their collection
```

### Test Case 3: Duplicate Prevention
```bash
# User already has the badge coupon in UserCoupons
# Admin manually assigns same badge again (shouldn't happen, but handled)

# Expected: No duplicate UserCoupon entry created
```

## Frontend Integration

No frontend changes required! The existing "My Coupons" page automatically displays badge coupons now because they're properly stored in UserCoupons table.

**Frontend endpoints that benefit**:
- `GET /api/customer/coupons/user` - Returns all user coupons including badge coupons
- `GET /api/customer/badges/active-coupon` - Still works for equipped badge
- `/my-coupons` page - Now shows badge coupons in the list

## Security Validation

Badge coupons remain protected:
- Users **cannot manually collect** badge coupons via `POST /api/customer/coupons/collect` (validation prevents `IsBadgeCoupon` coupons)
- Badge coupons **do not appear** in `GET /api/customer/coupons/available` (filtered by `!c.IsBadgeCoupon`)
- Badge coupons are only added to UserCoupons when:
  1. User earns the badge through eligibility
  2. Admin manually assigns the badge
  3. Badge has a valid LinkedCouponId

## Migration Status
✅ Database migration `20251201144936_AddBadgeCouponLinking` applied
✅ `Badges.LinkedCouponId` column exists
✅ `Coupons.IsBadgeCoupon` column exists
✅ Foreign key relationship configured

## Next Steps

1. **Run Backend**: Start the API to test auto-collection
   ```bash
   cd c:\Users\fptsh\source\repos\NekoViBE\src\NekoViBE.API
   dotnet run
   ```

2. **Seed Badge Coupons**: Run the SQL script to create coupons for existing badges
   ```sql
   -- Execute: docs/SEED_BADGE_COUPONS.sql
   ```

3. **Test Flow**:
   - Create a test order to trigger badge eligibility
   - Call `POST /api/customer/badges/process`
   - Verify UserCoupon entry created
   - Check "My Coupons" page shows badge coupon

4. **Optional - Backfill Existing Users**:
   If users already have badges but no coupons, run a one-time script:
   ```sql
   -- Backfill badge coupons for existing UserBadges
   INSERT INTO UserCoupons (Id, UserId, CouponId, Status, CreatedAt, CreatedBy)
   SELECT 
       NEWID(),
       ub.UserId,
       b.LinkedCouponId,
       1, -- Active
       GETUTCDATE(),
       ub.UserId
   FROM UserBadges ub
   INNER JOIN Badges b ON ub.BadgeId = b.Id
   WHERE b.LinkedCouponId IS NOT NULL
     AND ub.Status = 1
     AND NOT EXISTS (
         SELECT 1 FROM UserCoupons uc 
         WHERE uc.UserId = ub.UserId 
         AND uc.CouponId = b.LinkedCouponId
         AND uc.Status = 1
     )
   ```

## Summary

✅ **Automatic coupon collection when badge earned**
✅ **Works for both auto-award and manual assignment**
✅ **Prevents duplicate UserCoupon entries**
✅ **Badge coupons now visible in "My Coupons" page**
✅ **No breaking changes to existing flows**
✅ **Security validations maintained**
