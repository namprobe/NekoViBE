using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.ProductReview;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class ProductReviewQueryBuilder
    {
        public static Expression<Func<ProductReview, bool>> BuildPredicate(this ProductReviewFilter filter)
        {
            var predicate = PredicateBuilder.True<ProductReview>()
                .CombineAnd(x => !x.IsDeleted);

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<ProductReview>();
                searchPredicate = searchPredicate.CombineOr(x => x.Title != null && x.Title.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Comment != null && x.Comment.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (filter.ProductId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.UserId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.UserId == filter.UserId.Value);
            }

            if (filter.Rating.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Rating == filter.Rating.Value);
            }

            return predicate;
        }

        public static Expression<Func<ProductReview, object>> BuildOrderBy(this ProductReviewFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "rating" => x => x.Rating,
                "title" => x => x.Title ?? "",
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}