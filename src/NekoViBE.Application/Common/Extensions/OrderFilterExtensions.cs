using NekoViBE.Application.Common.DTOs.Order;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Extensions
{
    public static class OrderFilterExtensions
    {
        public static Expression<Func<Domain.Entities.Order, object>>? BuildOrderBy(this OrderFilter filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
                return null;

            return filter.SortBy.ToLower() switch
            {
                "ordernumber" => x => x.OrderNumber,
                "finalamount" => x => x.FinalAmount,
                "totalamount" => x => x.TotalAmount,
                "createdat" => x => x.CreatedAt,
                "paymentstatus" => x => x.PaymentStatus,
                "orderstatus" => x => x.OrderStatus,
                "useremail" => x => x.User != null ? x.User.Email : x.GuestEmail ?? "",
                _ => x => x.CreatedAt // Default ordering by creation date
            };
        }

        public static Expression<Func<Domain.Entities.Order, bool>>? BuildPredicate(this OrderFilter filter)
        {
            var predicates = new List<Expression<Func<Domain.Entities.Order, bool>>>();

            // Search across multiple fields
            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicates.Add(o =>
                    o.OrderNumber.ToLower().Contains(search) ||
                    (o.User != null && o.User.Email.ToLower().Contains(search)) ||
                    (o.GuestEmail != null && o.GuestEmail.ToLower().Contains(search)) ||
                    (o.GuestFirstName != null && o.GuestFirstName.ToLower().Contains(search)) ||
                    (o.GuestLastName != null && o.GuestLastName.ToLower().Contains(search)));
            }

            if (!string.IsNullOrEmpty(filter.OrderNumber))
                predicates.Add(o => o.OrderNumber.Contains(filter.OrderNumber));

            if (filter.UserId.HasValue)
                predicates.Add(o => o.UserId == filter.UserId.Value);

            if (!string.IsNullOrEmpty(filter.UserEmail))
                predicates.Add(o => o.User != null && o.User.Email.Contains(filter.UserEmail));

            if (!string.IsNullOrEmpty(filter.GuestEmail))
                predicates.Add(o => o.GuestEmail != null && o.GuestEmail.Contains(filter.GuestEmail));

            if (filter.IsOneClick.HasValue)
                predicates.Add(o => o.IsOneClick == filter.IsOneClick.Value);

            if (filter.PaymentStatus.HasValue)
                predicates.Add(o => o.PaymentStatus == filter.PaymentStatus.Value);

            if (filter.OrderStatus.HasValue)
                predicates.Add(o => o.OrderStatus == filter.OrderStatus.Value);

            if (filter.Status.HasValue)
                predicates.Add(o => o.Status == filter.Status.Value);

            // Amount range filters
            if (filter.MinAmount.HasValue)
                predicates.Add(o => o.FinalAmount >= filter.MinAmount.Value);

            if (filter.MaxAmount.HasValue)
                predicates.Add(o => o.FinalAmount <= filter.MaxAmount.Value);

            // Date range filters
            if (filter.DateFrom.HasValue)
                predicates.Add(o => o.CreatedAt >= filter.DateFrom.Value);

            if (filter.DateTo.HasValue)
                predicates.Add(o => o.CreatedAt <= filter.DateTo.Value);

            // Coupon usage filter
            if (filter.HasCoupon.HasValue)
            {
                if (filter.HasCoupon.Value)
                    predicates.Add(o => o.UserCoupons.Any());
                else
                    predicates.Add(o => !o.UserCoupons.Any());
            }

            if (predicates.Count == 0)
                return null;

            // Combine all predicates with AND
            return predicates.Aggregate((current, next) =>
            {
                var parameter = Expression.Parameter(typeof(Domain.Entities.Order), "o");
                var body = Expression.AndAlso(
                    Expression.Invoke(current, parameter),
                    Expression.Invoke(next, parameter));
                return Expression.Lambda<Func<Domain.Entities.Order, bool>>(body, parameter);
            });
        }
    }
}
