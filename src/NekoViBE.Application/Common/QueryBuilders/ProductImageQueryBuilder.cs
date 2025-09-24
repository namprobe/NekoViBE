using NekoViBE.Application.Common.DTOs.ProductImage;
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
    public static class ProductImageQueryBuilder
    {
        public static Expression<Func<ProductImage, bool>> BuildPredicate(this ProductImageFilter filter)
        {
            var predicate = PredicateBuilder.True<ProductImage>();

            if (filter.ProductId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ProductId == filter.ProductId.Value);
            }

            if (filter.IsPrimary.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.IsPrimary == filter.IsPrimary.Value);
            }

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            return predicate;
        }

        public static Expression<Func<ProductImage, object>> BuildOrderBy(this ProductImageFilter filter)
        {
            return x => x.CreatedAt!;
        }
    }
}
