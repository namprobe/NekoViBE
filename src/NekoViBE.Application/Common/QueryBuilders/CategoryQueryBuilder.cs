using NekoViBE.Application.Common.DTOs.Category;
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
    public static class CategoryQueryBuilder
    {
        public static Expression<Func<Category, bool>> BuildPredicate(this CategoryFilter filter)
        {
            var predicate = PredicateBuilder.True<Category>();

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<Category>();
                searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.ImagePath != null && x.ImagePath.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                predicate = predicate.CombineAnd(x => x.Name == filter.Name);
            }

            if (filter.IsRoot == true)
            {
                predicate = predicate.CombineAnd(x => x.ParentCategoryId == null);
            }
            else if (filter.ParentCategoryId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.ParentCategoryId == filter.ParentCategoryId.Value);
            }

            if (filter.HasImage.HasValue)
            {
                predicate = predicate.CombineAnd(x => filter.HasImage.Value ? x.ImagePath != null : x.ImagePath == null);
            }

            return predicate;
        }

        public static Expression<Func<Category, object>> BuildOrderBy(this CategoryFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "name" => x => x.Name,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                "imagepath" => x => x.ImagePath ?? "",
                _ => x => x.CreatedAt!
            };
        }
    }
}
