using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class ProductQueryBuilder
    {
        public static Expression<Func<Product, bool>> BuildPredicate(this ProductFilter filter)
        {
            var predicate = PredicateBuilder.True<Product>();

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<Product>();
                searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.ProductImages.Any(img => img.ImagePath.Contains(filter.Search)));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                predicate = predicate.CombineAnd(x => x.Name == filter.Name);
            }

            if (filter.CategoryId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.CategoryId == filter.CategoryId.Value);
            }

            if (filter.AnimeSeriesId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.AnimeSeriesId == filter.AnimeSeriesId.Value);
            }

            if (filter.HasImage.HasValue)
            {
                predicate = predicate.CombineAnd(x => filter.HasImage.Value ? x.ProductImages.Any() : !x.ProductImages.Any());
            }

            return predicate;
        }

        public static Expression<Func<Product, object>> BuildOrderBy(this ProductFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "name" => x => x.Name,
                "price" => x => x.Price,
                "stockquantity" => x => x.StockQuantity,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
