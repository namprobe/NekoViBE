// NekoViBE.Application.Common.QueryBuilders/PostCategoryQueryBuilder.cs
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class PostCategoryQueryBuilder
    {
        public static Expression<Func<PostCategory, bool>> BuildPredicate(this PostCategoryFilter filter)
        {
            var predicate = PredicateBuilder.True<PostCategory>()
                .CombineAnd(x => !x.IsDeleted);

            if (filter.Status.HasValue)
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicate = predicate.CombineAnd(x =>
                    x.Name.ToLower().Contains(search) ||
                    (x.Description != null && x.Description.ToLower().Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
                predicate = predicate.CombineAnd(x => x.Name.Contains(filter.Name));

            return predicate;
        }

        public static Expression<Func<PostCategory, object>> BuildOrderBy(this PostCategoryFilter filter)
        {
            return filter.SortBy?.ToLowerInvariant() switch
            {
                "name" => x => x.Name,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}