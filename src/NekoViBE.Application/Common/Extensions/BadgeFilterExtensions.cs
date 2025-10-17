using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Extensions
{
    public static class BadgeFilterExtensions
    {
        public static Expression<Func<Domain.Entities.Badge, object>>? BuildOrderBy(this BadgeFilter filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
                return null;

            return filter.SortBy.ToLower() switch
            {
                "name" => x => x.Name,
                "discountpercentage" => x => x.DiscountPercentage,
                "conditiontype" => x => x.ConditionType,
                "startdate" => x => x.StartDate ?? DateTime.MinValue,
                "enddate" => x => x.EndDate ?? DateTime.MinValue,
                "createdat" => x => x.CreatedAt,
                "status" => x => x.Status,
                "usercount" => x => x.UserBadges.Count,
                _ => x => x.CreatedAt // Default ordering
            };
        }

        public static Expression<Func<Domain.Entities.Badge, bool>>? BuildPredicate(this BadgeFilter filter)
        {
            var predicates = new List<Expression<Func<Domain.Entities.Badge, bool>>>();

            // Search across multiple fields
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicates.Add(b =>
                    b.Name.ToLower().Contains(search) ||
                    (b.Description != null && b.Description.ToLower().Contains(search)) ||
                    b.ConditionValue.ToLower().Contains(search));
            }

            if (!string.IsNullOrEmpty(filter.Name))
                predicates.Add(b => b.Name.Contains(filter.Name));

            if (filter.ConditionType.HasValue)
                predicates.Add(b => b.ConditionType == filter.ConditionType.Value);

            if (filter.Status.HasValue)
                predicates.Add(b => b.Status == filter.Status.Value);

            if (filter.IsTimeLimited.HasValue)
                predicates.Add(b => b.IsTimeLimited == filter.IsTimeLimited.Value);

            // Date range filters
            if (filter.StartDateFrom.HasValue)
                predicates.Add(b => b.StartDate >= filter.StartDateFrom.Value);

            if (filter.StartDateTo.HasValue)
                predicates.Add(b => b.StartDate <= filter.StartDateTo.Value);

            if (filter.EndDateFrom.HasValue)
                predicates.Add(b => b.EndDate >= filter.EndDateFrom.Value);

            if (filter.EndDateTo.HasValue)
                predicates.Add(b => b.EndDate <= filter.EndDateTo.Value);

            // Boolean filters
            if (filter.IsActive.HasValue)
            {
                if (filter.IsActive.Value)
                    predicates.Add(b => b.Status == EntityStatusEnum.Active);
                else
                    predicates.Add(b => b.Status == EntityStatusEnum.Inactive);
            }

            if (filter.IsExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsExpired.Value)
                    predicates.Add(b => b.IsTimeLimited && b.EndDate.HasValue && b.EndDate < now);
                else
                    predicates.Add(b => !b.IsTimeLimited || !b.EndDate.HasValue || b.EndDate >= now);
            }

            if (filter.IsValid.HasValue)
            {
                var now = DateTime.UtcNow;
                if (filter.IsValid.Value)
                {
                    predicates.Add(b => b.Status == EntityStatusEnum.Active);
                    predicates.Add(b => !b.IsTimeLimited ||
                                       (b.StartDate <= now && b.EndDate >= now));
                }
                else
                {
                    predicates.Add(b => b.Status == EntityStatusEnum.Inactive ||
                                       (b.IsTimeLimited && (b.StartDate > now || b.EndDate < now)));
                }
            }

            if (filter.HasUsers.HasValue)
            {
                if (filter.HasUsers.Value)
                    predicates.Add(b => b.UserBadges.Any());
                else
                    predicates.Add(b => !b.UserBadges.Any());
            }

            if (predicates.Count == 0)
                return null;

            // Combine all predicates with AND
            return predicates.Aggregate((current, next) =>
            {
                var parameter = Expression.Parameter(typeof(Domain.Entities.Badge), "b");
                var body = Expression.AndAlso(
                    Expression.Invoke(current, parameter),
                    Expression.Invoke(next, parameter));
                return Expression.Lambda<Func<Domain.Entities.Badge, bool>>(body, parameter);
            });
        }
    }
}
