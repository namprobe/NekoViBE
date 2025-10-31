using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders;

public static class TagQueryBuilder
{
    // Xây dựng biểu thức điều kiện từ TagFilter
    public static Expression<Func<Tag, bool>> BuildPredicate(this TagFilter filter)
    {
        var predicate = PredicateBuilder.True<Tag>();

        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<Tag>();
            searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));
            predicate = predicate.CombineAnd(searchPredicate);
        }

        if (!string.IsNullOrWhiteSpace(filter.Name))
        {
            predicate = predicate.CombineAnd(x => x.Name.Contains(filter.Name));
        }

        return predicate;
    }

    // Xây dựng biểu thức sắp xếp từ TagFilter
    public static Expression<Func<Tag, object>> BuildOrderBy(this TagFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return x => x.CreatedAt!;

        return filter.SortBy.ToLowerInvariant() switch
        {
            "name" => x => x.Name,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt!
        };
    }
}