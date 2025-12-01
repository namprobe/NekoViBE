# Badge-Coupon Integration Implementation

## Overview
This implementation allows badge discounts to be applied through the existing coupon system, avoiding duplicate pricing logic and maintaining a single source of truth for discounts in the Order service.

## Key Design Decisions

### 1. Hard Link Approach
- **Badge Entity**: Added `LinkedCouponId` (nullable FK to Coupons)
- **Coupon Entity**: Added `IsBadgeCoupon` (bool flag)
- **Relationship**: One-to-one between Badge and Coupon

### 2. Database Migration
```sql
-- Add LinkedCouponId to Badges
ALTER TABLE Badges ADD LinkedCouponId uniqueidentifier NULL;

-- Add IsBadgeCoupon to Coupons  
ALTER TABLE Coupons ADD IsBadgeCoupon bit NOT NULL DEFAULT 0;

-- Create foreign key with unique constraint
CREATE UNIQUE INDEX IX_Badges_LinkedCouponId ON Badges(LinkedCouponId) 
WHERE LinkedCouponId IS NOT NULL;

ALTER TABLE Badges ADD CONSTRAINT FK_Badges_Coupons_LinkedCouponId
FOREIGN KEY (LinkedCouponId) REFERENCES Coupons(Id) ON DELETE RESTRICT;
```

## Implementation Details

### 1. Auto-Generate Coupon When Badge is Created

**Location**: `CreateBadgeCommandHandler.cs`

**Logic**:
```csharp
// When admin creates badge
var badgeCoupon = new Coupon {
    Code = $"SYS_BADGE_{Guid.NewGuid():N}",
    DiscountType = Percentage,
    DiscountValue = badge.DiscountPercentage,
    IsBadgeCoupon = true,
    UsageLimit = null, // Unlimited
    MinOrderAmount = 0
};

// Save coupon first
await couponRepo.AddAsync(badgeCoupon);

// Link badge to coupon
badge.LinkedCouponId = badgeCoupon.Id;
```

**Benefits**:
- ✅ Single source of truth for discount value
- ✅ No code changes needed in Order service
- ✅ Badge discount automatically synced with coupon

### 2. Prevent Manual Collection of Badge Coupons

**Location**: `CollectCouponCommandHandler.cs`

**Validation**:
```csharp
if (coupon.IsBadgeCoupon) {
    return Result.Failure(
        "This coupon is linked to a badge and cannot be collected manually. " +
        "Equip the badge to use this discount.",
        ErrorCodeEnum.InvalidOperation
    );
}
```

**Location**: `GetAvailableCouponsQueryHandler.cs`

**Filter**:
```csharp
.Where(c => 
    c.Status == Active &&
    c.StartDate <= now &&
    c.EndDate >= now &&
    !c.IsBadgeCoupon  // Hide from public coupon list
)
```

**Why**:
- ❌ Users cannot click "Collect" on badge coupons
- ❌ Badge coupons don't appear in available coupons list
- ✅ Only way to use badge coupon is to equip the badge

### 3. Get Active Badge Coupon Endpoint

**Endpoint**: `GET /api/customer/badges/active-coupon`

**Purpose**: Return the coupon linked to user's currently equipped badge

**Query**: `GetActiveBadgeCouponQuery`

**Logic**:
```csharp
// Find equipped badge
var activeBadge = await userBadges
    .Include(ub => ub.Badge.LinkedCoupon)
    .Where(ub => ub.IsActive && ub.Status == Active)
    .FirstOrDefaultAsync();

// Return linked coupon
return activeBadge?.Badge.LinkedCoupon;
```

**Returns**: `CouponItem?` (null if no badge equipped or no linked coupon)

## Frontend Integration

### 1. Display Badge Coupon Info

```typescript
// Fetch active badge coupon
const { data: badgeCoupon } = await api.get('/api/customer/badges/active-coupon')

// Show in cart/checkout
if (badgeCoupon) {
  return (
    <div className="badge-discount">
      <Award className="icon" />
      <span>Badge Discount: {badgeCoupon.discountValue}%</span>
      <span className="auto-applied">Auto-applied</span>
    </div>
  )
}
```

### 2. Checkout Flow (Option A - Recommended)

**Auto-apply badge coupon**:
```typescript
// Before checkout
const activeBadgeCoupon = await fetchActiveBadgeCoupon()

// Include in checkout request
const checkoutData = {
  ...orderData,
  couponCode: activeBadgeCoupon?.code || userSelectedCoupon?.code
}
```

### 3. Checkout Flow (Option B - Backend Auto-inject)

**Backend intercepts checkout** (if you prefer zero frontend changes):

```csharp
// In PlaceOrderCommandHandler
var activeBadge = await GetUserActiveBadge(userId);
if (activeBadge?.LinkedCouponId != null && request.CouponId == null) {
    request.CouponId = activeBadge.LinkedCouponId; // Auto-inject
}
```

## Validation Rules

### Badge Coupon Can Only Be Used If:
1. ✅ User owns the badge (`UserBadge` exists)
2. ✅ Badge is equipped (`UserBadge.IsActive = true`)
3. ✅ Coupon is valid (not expired, active status)

### Validation in Apply Coupon Logic

**Location**: When applying coupon to order

```csharp
// Check if coupon is badge coupon
if (coupon.IsBadgeCoupon) {
    // Verify user has equipped badge
    var hasBadge = await userBadges
        .Where(ub => 
            ub.UserId == userId &&
            ub.Badge.LinkedCouponId == couponId &&
            ub.IsActive &&  // Must be equipped
            ub.Status == Active
        ).AnyAsync();
        
    if (!hasBadge) {
        return Result.Failure(
            "You must equip the corresponding badge to use this coupon",
            ErrorCodeEnum.Unauthorized
        );
    }
}
```

## Testing Scenarios

### Scenario 1: Create Badge
1. Admin creates "Gold Member" badge with 5% discount
2. ✅ System auto-creates coupon `SYS_BADGE_xxx` with 5% discount
3. ✅ Badge.LinkedCouponId = Coupon.Id
4. ✅ Coupon.IsBadgeCoupon = true

### Scenario 2: User Tries to Collect Badge Coupon
1. User visits `/coupons` page
2. ❌ Badge coupons are hidden (not in list)
3. User manually tries to collect via API
4. ❌ API returns error: "Cannot collect badge coupon manually"

### Scenario 3: User Equips Badge
1. User equips "Gold Member" badge
2. ✅ `UserBadge.IsActive = true`
3. User calls `/api/customer/badges/active-coupon`
4. ✅ Returns the linked coupon with 5% discount

### Scenario 4: Checkout with Badge
1. User has "Gold Member" badge equipped
2. Frontend fetches active badge coupon
3. ✅ Coupon code is passed to checkout
4. ✅ Order service applies 5% discount (thinks it's a normal coupon)
5. ✅ No changes needed in Order service

### Scenario 5: User Unequips Badge
1. User equips different badge
2. ✅ Old badge `IsActive = false`
3. User calls `/api/customer/badges/active-coupon`
4. ✅ Returns new badge's coupon (or null)
5. Next checkout uses new discount

### Scenario 6: Malicious User Attack
1. User doesn't own badge
2. User tries to use coupon code `SYS_BADGE_xxx` directly
3. ✅ Validation checks `UserBadge` with `IsActive = true`
4. ❌ Validation fails - coupon rejected

## API Endpoints

### Customer Endpoints

```
GET  /api/customer/badges/active-coupon
     → Returns coupon linked to equipped badge (or null)
     → Auth: Required
     → Response: CouponItem?
```

### No Changes Needed
- ❌ `/api/customer/coupons/available` - Already filters out badge coupons
- ❌ `/api/customer/coupons/collect` - Already blocks badge coupon collection  
- ❌ `/api/customer/orders` - Uses existing coupon validation

## Migration Steps

### 1. Database
```bash
# Run migration
dotnet ef migrations add AddBadgeCouponLinking
dotnet ef database update
```

### 2. Update Existing Badges (One-time Script)
```sql
-- For each existing badge without linked coupon
DECLARE @badgeId uniqueidentifier;
DECLARE @badgeName nvarchar(255);
DECLARE @discount decimal(5,2);
DECLARE @newCouponId uniqueidentifier;

-- Cursor for each badge
DECLARE badge_cursor CURSOR FOR
SELECT Id, Name, DiscountPercentage 
FROM Badges 
WHERE LinkedCouponId IS NULL;

OPEN badge_cursor;
FETCH NEXT FROM badge_cursor INTO @badgeId, @badgeName, @discount;

WHILE @@FETCH_STATUS = 0
BEGIN
    SET @newCouponId = NEWID();
    
    -- Create coupon
    INSERT INTO Coupons (Id, Code, Description, DiscountType, DiscountValue, 
                         StartDate, EndDate, IsBadgeCoupon, Status, CreatedAt)
    VALUES (@newCouponId, 
            'SYS_BADGE_' + REPLACE(CAST(@badgeId AS nvarchar(50)), '-', ''),
            'Auto-generated for badge: ' + @badgeName,
            0, -- Percentage
            @discount,
            GETUTCDATE(),
            DATEADD(year, 10, GETUTCDATE()),
            1, -- IsBadgeCoupon
            1, -- Active
            GETUTCDATE());
    
    -- Link badge
    UPDATE Badges SET LinkedCouponId = @newCouponId WHERE Id = @badgeId;
    
    FETCH NEXT FROM badge_cursor INTO @badgeId, @badgeName, @discount;
END;

CLOSE badge_cursor;
DEALLOCATE badge_cursor;
```

### 3. Deploy Backend
```bash
# Build and deploy
dotnet build
dotnet publish
```

### 4. Frontend Changes (Minimal)
```typescript
// Add API call in checkout
const badgeCoupon = await fetchActiveBadgeCoupon()
if (badgeCoupon) {
  checkoutData.couponCode = badgeCoupon.code
}
```

## Benefits

✅ **Zero Changes to Order Service** - Pricing logic untouched  
✅ **Single Source of Truth** - One place for discount calculations  
✅ **Automatic Sync** - Badge discount = Coupon discount  
✅ **Secure** - Badge coupons can't be manually collected or abused  
✅ **Maintainable** - No duplicate logic between badges and coupons  
✅ **Scalable** - Easy to add new badges with discounts  

## Team Coordination

### Your Responsibility
- ✅ Badge creation auto-generates coupon
- ✅ Badge equip/unequip logic
- ✅ Validation that badge coupon can't be collected
- ✅ API to get active badge coupon

### Teammate's Responsibility (Zero Changes)
- ❌ No changes to Order service
- ❌ No changes to pricing calculations
- ❌ No changes to coupon application logic

### Frontend Team
- ✅ Call `/api/customer/badges/active-coupon` before checkout
- ✅ Pass coupon code to existing checkout flow
- ✅ Display badge discount in cart (optional)

## Summary

The badge discount is now a **special kind of coupon** that:
1. Cannot be manually collected
2. Is automatically available when badge is equipped
3. Uses the exact same validation/application logic as normal coupons
4. Requires zero changes to the Order service

Your teammate's concern is addressed: **No duplicate pricing logic. The Order service only knows how to handle Coupons, and that's all it needs to know.**
