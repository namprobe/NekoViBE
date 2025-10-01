using NekoViBE.Application.Common.DTOs.ProductTag;
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
    public static class ProductTagQueryBuilder
    {
        // Xây dựng biểu thức điều kiện từ ProductTagFilter
        public static Expression<Func<ProductTag, bool>> BuildPredicate(this ProductTagFilter filter)
        {
            var predicate = PredicateBuilder.True<ProductTag>();

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (filter.ProductId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.TagId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.TagId == filter.TagId.Value);
            }

            return predicate;
        }

        // Xây dựng biểu thức sắp xếp từ ProductTagFilter
        public static Expression<Func<ProductTag, object>> BuildOrderBy(this ProductTagFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "productid" => x => x.ProductId,
                "tagid" => x => x.TagId,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
