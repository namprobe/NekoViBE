using NekoViBE.Application.Common.DTOs.Coupon;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Extensions
{
    public static class CouponFilterExtensions
    {
        public static Expression<Func<Coupon, object>>? BuildOrderBy(this CouponFilter filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
                return null;

            return filter.SortBy.ToLower() switch
            {
                "code" => x => x.Code,
                "discountvalue" => x => x.DiscountValue,
                "minorderamount" => x => x.MinOrderAmount,
                "startdate" => x => x.StartDate,
                "enddate" => x => x.EndDate,
                "currentusage" => x => x.CurrentUsage,
                "createdat" => x => x.CreatedAt,
                "status" => x => x.Status,
                _ => x => x.CreatedAt // Default ordering
            };
        }

        public static Expression<Func<Coupon, bool>>? BuildPredicate(this CouponFilter filter)
        {
            var predicates = new List<Expression<Func<Coupon, bool>>>();

            // Search across multiple fields
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicates.Add(c =>
                    c.Code.ToLower().Contains(search) ||
                    (c.Description != null && c.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(filter.Code))
                predicates.Add(c => c.Code.Contains(filter.Code));

            if (filter.DiscountType.HasValue)
                predicates.Add(c => c.DiscountType == filter.DiscountType.Value);

            if (filter.Status.HasValue)
                predicates.Add(c => c.Status == filter.Status.Value);

            // Date range filters
            if (filter.StartDateFrom.HasValue)
                predicates.Add(c => c.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                predicates.Add(c => c.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                predicates.Add(c => c.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                predicates.Add(c => c.EndDate <= filter.EndDateTo.Value);

            // Boolean filters
            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                    predicates.Add(c => c.Status == EntityStatusEnum.Active);
                else
                    predicates.Add(c => c.Status == EntityStatusEnum.Inactive);
            }

            if (filter.IsExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsExpired.Value)
                    predicates.Add(c => c.EndDate < now);
                else
                    predicates.Add(c => c.EndDate >= now);
            }

            if (filter.IsValid.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsValid.Value)
                {
                    predicates.Add(c => c.Status == EntityStatusEnum.Active);
                    predicates.Add(c => c.StartDate <= now);
                    predicates.Add(c => c.EndDate >= now);
                    predicates.Add(c => !c.UsageLimit.HasValue || c.CurrentUsage < c.UsageLimit.Value);
                }
                else
                {
                    predicates.Add(c => c.Status == EntityStatusEnum.Inactive ||
                                       c.StartDate > now ||
                                       c.EndDate < now ||
                                       (c.UsageLimit.HasValue && c.CurrentUsage >= c.UsageLimit.Value));
                }
            }

            if (filter.HasUsageLimit.HasValue)
            {
                if (filter.HasUsageLimit.Value)
                    predicates.Add(c => c.UsageLimit.HasValue);
                else
                    predicates.Add(c => !c.UsageLimit.HasValue);
            }

            if (predicates.Count == 0)
                return null;

            // Combine all predicates with AND
            return predicates.Aggregate((current, next) =>
            {
                var parameter = Expression.Parameter(typeof(Coupon), "c");
                var body = Expression.AndAlso(
                    Expression.Invoke(current, parameter),
                    Expression.Invoke(next, parameter));
                return Expression.Lambda<Func<Coupon, bool>>(body, parameter);
            });
        }
    }
}
