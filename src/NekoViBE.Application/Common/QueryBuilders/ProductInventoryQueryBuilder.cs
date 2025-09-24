using NekoViBE.Application.Common.DTOs.ProductInventory;
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
    public static class ProductInventoryQueryBuilder
    {
        public static Expression<Func<ProductInventory, bool>> BuildPredicate(this ProductInventoryFilter filter)
        {
            var predicate = PredicateBuilder.True<ProductInventory>();

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (filter.ProductId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ProductId == filter.ProductId.Value);
            }

            return predicate;
        }

        public static Expression<Func<ProductInventory, object>> BuildOrderBy(this ProductInventoryFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "productid" => x => x.ProductId,
                "quantity" => x => x.Quantity,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
