using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.AnimeSeries;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders;

public static class AnimeSeriesQueryBuilder
{
    public static Expression<Func<AnimeSeries, bool>> BuildPredicate(this AnimeSeriesFilter filter)
    {
        var predicate = PredicateBuilder.True<AnimeSeries>();

        // Base: Status filter
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        // Search (title, description)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<AnimeSeries>();

            searchPredicate = searchPredicate.CombineOr(x => x.Title.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));

            predicate = predicate.CombineAnd(searchPredicate);
        }

        // Title exact match
        if (!string.IsNullOrWhiteSpace(filter.Title))
        {
            predicate = predicate.CombineAnd(x => x.Title == filter.Title);
        }

        // ReleaseYear
        if (filter.ReleaseYear.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ReleaseYear == filter.ReleaseYear.Value);
        }

        return predicate;
    }

    public static Expression<Func<AnimeSeries, object>> BuildOrderBy(this AnimeSeriesFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return x => x.CreatedAt!; // Default newest first

        return filter.SortBy.ToLowerInvariant() switch
        {
            "title" => x => x.Title,
            "releaseyear" => x => x.ReleaseYear,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt!
        };
    }
}
