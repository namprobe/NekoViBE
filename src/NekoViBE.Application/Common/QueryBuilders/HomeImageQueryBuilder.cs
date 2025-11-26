using NekoViBE.Application.Common.DTOs.HomeImage;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class HomeImageQueryBuilder
    {
        public static Expression<Func<HomeImage, bool>> BuildPredicate(this HomeImageFilter filter)
        {
            var predicate = PredicateBuilder.True<HomeImage>();

            // Luôn loại bỏ các bản đã bị xóa mềm
            predicate = predicate.CombineAnd(x => !x.IsDeleted);

            if (filter.AnimeSeriesId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId == filter.AnimeSeriesId.Value);
            }

            if (filter.HasAnimeSeries == true)
            {
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId != null);
            }
            else if (filter.HasAnimeSeries == false)
            {
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId == null);
            }

            return predicate;
        }

        public static Expression<Func<HomeImage, object>> BuildOrderBy(this HomeImageFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}