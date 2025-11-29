using System;
using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.UserCoupon;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.QueryBuilders;

public static class UserCouponQueryBuilder
{
    public static Expression<Func<UserCoupon, bool>> BuildPredicate(this UserCouponFilter filter)
    {
        var predicate = PredicateBuilder.True<UserCoupon>();

        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        if (filter.UserId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.UserId == filter.UserId.Value);
        }

        if (filter.CouponId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.CouponId == filter.CouponId.Value);
        }

        if (filter.IsUsed.HasValue)
        {
            predicate = filter.IsUsed.Value
                ? predicate.CombineAnd(x => x.UsedDate != null)
                : predicate.CombineAnd(x => x.UsedDate == null);
        }

        if (filter.IsExpired.HasValue)
        {
            var now = DateTime.UtcNow;
            predicate = filter.IsExpired.Value
                ? predicate.CombineAnd(x => x.Coupon.EndDate < now)
                : predicate.CombineAnd(x => x.Coupon.EndDate >= now);
        }

        if (filter.OnlyActiveCoupons == true)
        {
            predicate = predicate.CombineAnd(x => x.Coupon.Status == EntityStatusEnum.Active);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            predicate = predicate.CombineAnd(x =>
                x.Coupon.Code.Contains(search) ||
                (x.Coupon.Description != null && x.Coupon.Description.Contains(search)));
        }

        return predicate;
    }

    public static Expression<Func<UserCoupon, object>> BuildOrderBy(this UserCouponFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
        {
            return x => x.CreatedAt!;
        }

        return filter.SortBy.ToLowerInvariant() switch
        {
            "code" => x => x.Coupon.Code,
            "startdate" => x => x.Coupon.StartDate,
            "enddate" => x => x.Coupon.EndDate,
            "useddate" => x => x.UsedDate ?? DateTime.MaxValue,
            "updatedat" => x => x.UpdatedAt ?? x.CreatedAt!,
            _ => x => x.CreatedAt!
        };
    }
}

