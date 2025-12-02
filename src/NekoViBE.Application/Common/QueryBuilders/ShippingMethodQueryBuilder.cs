using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.ShippingMethod;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.QueryBuilders;

/// <summary>
/// Extension methods cho ShippingMethodFilter để build query trực tiếp
/// Không cần inject QueryBuilder vào CQRS handlers
/// </summary>
public static class ShippingMethodQueryBuilder
{
    /// <summary>
    /// Build complete predicate từ ShippingMethodFilter
    /// </summary>
    public static Expression<Func<ShippingMethod, bool>> BuildPredicate(this ShippingMethodFilter filter)
    {
        var predicate = PredicateBuilder.True<ShippingMethod>();

        // Base predicate (Status filter từ BasePaginationFilter)
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        // Search predicate
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<ShippingMethod>();
            
            // Search trong Name
            searchPredicate = searchPredicate.CombineOr<ShippingMethod>(
                x => x.Name.Contains(filter.Search));
            
            // Search trong Description (null-safe)
            searchPredicate = searchPredicate.CombineOr<ShippingMethod>(
                x => x.Description != null && x.Description.Contains(filter.Search));

            predicate = predicate.CombineAnd(searchPredicate);
        }

        // Custom filters
        if (filter.MinCost.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Cost >= filter.MinCost.Value);
        }

        if (filter.MaxCost.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Cost <= filter.MaxCost.Value);
        }

        if (filter.MaxEstimatedDays.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.EstimatedDays == null || x.EstimatedDays <= filter.MaxEstimatedDays.Value);
        }

        return predicate;
    }

    /// <summary>
    /// Build OrderBy expression từ ShippingMethodFilter
    /// Default: CreatedAt giảm dần (newest first)
    /// </summary>
    public static Expression<Func<ShippingMethod, object>> BuildOrderBy(this ShippingMethodFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return x => x.CreatedAt!; // Default: CreatedAt giảm dần

        return filter.SortBy.ToLowerInvariant() switch
        {
            "name" => x => x.Name,
            "cost" => x => x.Cost,
            "estimateddays" => x => x.EstimatedDays ?? int.MaxValue,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt! // Fallback to CreatedAt
        };
    }
}

