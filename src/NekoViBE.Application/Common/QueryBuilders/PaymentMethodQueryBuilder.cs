using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.PaymentMethod;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders;

/// <summary>
/// Extension methods cho PaymentMethodFilter để build query trực tiếp
/// Không cần inject QueryBuilder vào CQRS handlers
/// </summary>
public static class PaymentMethodQueryBuilder
{
    /// <summary>
    /// Build complete predicate từ PaymentMethodFilter
    /// </summary>
    public static Expression<Func<PaymentMethod, bool>> BuildPredicate(this PaymentMethodFilter filter)
    {
        var predicate = PredicateBuilder.True<PaymentMethod>();

        // Base predicate (Status filter từ BasePaginationFilter)
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        // Search predicate
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<PaymentMethod>();
            
            // Search trong Name
            searchPredicate = searchPredicate.CombineOr<PaymentMethod>(
                x => x.Name.Contains(filter.Search));
            
            // Search trong Description (null-safe)
            searchPredicate = searchPredicate.CombineOr<PaymentMethod>(
                x => x.Description != null && x.Description.Contains(filter.Search));
            
            // Search trong ProcessorName (null-safe)
            searchPredicate = searchPredicate.CombineOr<PaymentMethod>(
                x => x.ProcessorName != null && x.ProcessorName.Contains(filter.Search));

            predicate = predicate.CombineAnd(searchPredicate);
        }

        // Custom filters
        if (filter.IsOnlinePayment.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.IsOnlinePayment == filter.IsOnlinePayment.Value);
        }

        if (filter.MinProcessingFee.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ProcessingFee >= filter.MinProcessingFee.Value);
        }

        if (filter.MaxProcessingFee.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ProcessingFee <= filter.MaxProcessingFee.Value);
        }

        return predicate;
    }

    /// <summary>
    /// Build OrderBy expression từ PaymentMethodFilter
    /// Default: CreatedAt giảm dần (newest first)
    /// </summary>
    public static Expression<Func<PaymentMethod, object>> BuildOrderBy(this PaymentMethodFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return x => x.CreatedAt!; // Default: CreatedAt giảm dần

        return filter.SortBy.ToLowerInvariant() switch
        {
            "name" => x => x.Name,
            "processingfee" => x => x.ProcessingFee,
            "processorname" => x => x.ProcessorName ?? "",
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt! // Fallback to CreatedAt
        };
    }
}
