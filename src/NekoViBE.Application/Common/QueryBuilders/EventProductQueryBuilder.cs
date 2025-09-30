using NekoViBE.Application.Common.DTOs.EventProduct;
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
    public static class EventProductQueryBuilder
    {
        public static Expression<Func<EventProduct, bool>> BuildPredicate(this EventProductFilter filter)
        {
            var predicate = PredicateBuilder.True<EventProduct>()
                .CombineAnd(x => !x.IsDeleted);

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (filter.EventId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.EventId == filter.EventId.Value);
            }

            if (filter.ProductId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.IsFeatured.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.IsFeatured == filter.IsFeatured.Value);
            }

            return predicate;
        }

        public static Expression<Func<EventProduct, object>> BuildOrderBy(this EventProductFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "eventid" => x => x.EventId,
                "productid" => x => x.ProductId,
                "isfeatured" => x => x.IsFeatured,
                "discountpercentage" => x => x.DiscountPercentage,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
