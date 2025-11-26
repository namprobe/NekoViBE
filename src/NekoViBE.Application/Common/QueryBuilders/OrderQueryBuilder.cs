using System;
using System.Linq;
using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders;

public static class OrderQueryBuilder
{
    public static Expression<Func<Order, object>> BuildOrderBy(this OrderFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return o => o.CreatedAt!;

        return filter.SortBy.ToLowerInvariant() switch
        {
            "finalamount" => o => o.FinalAmount,
            "totalamount" => o => o.TotalAmount,
            "paymentstatus" => o => o.PaymentStatus,
            "orderstatus" => o => o.OrderStatus,
            "useremail" => o => o.User != null
                ? (o.User.Email ?? string.Empty)
                : (o.GuestEmail ?? string.Empty),
            "createdat" => o => o.CreatedAt!,
            _ => o => o.CreatedAt!
        };
    }

    public static Expression<Func<Order, bool>> BuildPredicate(this OrderFilter filter)
    {
        var predicate = PredicateBuilder.True<Order>();

        // Base predicate (Status filter)
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.Status == filter.Status.Value);
        }

        // Search predicate
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.ToLowerInvariant();
            var searchPredicate = PredicateBuilder.False<Order>();

            searchPredicate = searchPredicate.CombineOr(
                o => o.User != null && o.User.Email != null && o.User.Email.ToLower().Contains(search));
            searchPredicate = searchPredicate.CombineOr(
                o => o.GuestEmail != null && o.GuestEmail.ToLower().Contains(search));
            searchPredicate = searchPredicate.CombineOr(
                o => o.GuestFirstName != null && o.GuestFirstName.ToLower().Contains(search));
            searchPredicate = searchPredicate.CombineOr(
                o => o.GuestLastName != null && o.GuestLastName.ToLower().Contains(search));
            searchPredicate = searchPredicate.CombineOr(
                o => o.OrderItems.Any(item => item.Product.Name.ToLower().Contains(search)));

            predicate = predicate.CombineAnd(searchPredicate);
        }

        if (filter.UserId.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.UserId == filter.UserId.Value);
        }

        if (filter.IsOneClick.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.IsOneClick == filter.IsOneClick.Value);
        }

        if (filter.PaymentStatus.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.PaymentStatus == filter.PaymentStatus.Value);
        }

        if (filter.OrderStatus.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.OrderStatus == filter.OrderStatus.Value);
        }

        // Amount range filters
        if (filter.MinAmount.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.FinalAmount >= filter.MinAmount.Value);
        }

        if (filter.MaxAmount.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.FinalAmount <= filter.MaxAmount.Value);
        }

        // Date range filters
        if (filter.DateFrom.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.CreatedAt >= filter.DateFrom.Value);
        }

        if (filter.DateTo.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.CreatedAt <= filter.DateTo.Value);
        }

        // Coupon usage filter
        if (filter.HasCoupon.HasValue)
        {
            predicate = filter.HasCoupon.Value
                ? predicate.CombineAnd(o => o.UserCoupons.Any())
                : predicate.CombineAnd(o => !o.UserCoupons.Any());
        }

        if (filter.CategoryId.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.OrderItems.Any(item => item.Product.CategoryId == filter.CategoryId.Value));
        }

        if (filter.AnimeSeriesId.HasValue)
        {
            predicate = predicate.CombineAnd(o => o.OrderItems.Any(item => item.Product.AnimeSeriesId == filter.AnimeSeriesId.Value));
        }

        return predicate;
    }
}
