using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class HomeImageQueryBuilder
    {
        // HomeImageQueryBuilder.cs
        public static Expression<Func<HomeImage, bool>> BuildPredicate(this HomeImageFilter filter)
        {
            var predicate = PredicateBuilder.True<HomeImage>();
            predicate = predicate.CombineAnd(x => !x.IsDeleted);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<HomeImage>();
                searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (filter.AnimeSeriesId.HasValue)
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId == filter.AnimeSeriesId.Value);

            if (filter.HasAnimeSeries == true)
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId != null);
            else if (filter.HasAnimeSeries == false)
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId == null);

            return predicate;
        }

        public static Expression<Func<HomeImage, object>> BuildOrderBy(this HomeImageFilter filter)
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
}