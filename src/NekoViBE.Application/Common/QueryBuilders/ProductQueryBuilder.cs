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

            if (!string.IsNullOrWhiteSpace(filter.PriceRange))
            {
                predicate = filter.PriceRange.ToLowerInvariant() switch
                {
                    "under-500k" => predicate.CombineAnd(x => x.Price < 500000),
                    "500k-1m" => predicate.CombineAnd(x => x.Price >= 500000 && x.Price < 1000000),
                    "1m-2m" => predicate.CombineAnd(x => x.Price >= 1000000 && x.Price < 2000000),
                    "over-2m" => predicate.CombineAnd(x => x.Price >= 2000000),
                    _ => predicate
                };
            }

            if (filter.TagIds != null && filter.TagIds.Any())
            {
                var tagPredicate = PredicateBuilder.False<Product>();
                foreach (var tagId in filter.TagIds)
                {
                    var id = tagId; // capture variable
                    tagPredicate = tagPredicate.CombineOr(p => p.ProductTags.Any(pt => pt.TagId == id));
                }
                predicate = predicate.CombineAnd(tagPredicate);
            }

            return predicate;
        }

        public static Expression<Func<Product, object>> BuildOrderBy(this ProductFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortType))
                return x => x.CreatedAt!; // mặc định: mới nhất

            return filter.SortType.ToLowerInvariant() switch
            {
                "price-asc" => x => x.Price,
                "price-desc" => x => x.Price,
                "name-asc" => x => x.Name,
                "name-desc" => x => x.Name,
                _ => x => x.CreatedAt!
            };
        }

    }
}
