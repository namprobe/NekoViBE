// NekoViBE.Application.Common.QueryBuilders/BlogPostQueryBuilder.cs
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class BlogPostQueryBuilder
    {
        public static Expression<Func<BlogPost, bool>> BuildPredicate(this BlogPostFilter filter)
        {
            var predicate = PredicateBuilder.True<BlogPost>()
                .CombineAnd(x => !x.IsDeleted);

            if (filter.Status.HasValue)
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<BlogPost>();
                searchPredicate = searchPredicate.CombineOr(x => x.Title.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Content.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (!string.IsNullOrWhiteSpace(filter.Title))
                predicate = predicate.CombineAnd(x => x.Title.Contains(filter.Title));

            if (filter.CategoryId.HasValue)
                predicate = predicate.CombineAnd(x => x.PostCategoryId == filter.CategoryId.Value);

            if (filter.TagIds != null && filter.TagIds.Any())
                predicate = predicate.CombineAnd(x => x.PostTags.Any(pt => filter.TagIds.Contains(pt.TagId)));

            if (filter.IsPublished.HasValue)
                predicate = predicate.CombineAnd(x => x.IsPublished == filter.IsPublished.Value);

            return predicate;
        }

        public static Expression<Func<BlogPost, object>> BuildOrderBy(this BlogPostFilter filter)
        {
            return filter.SortBy?.ToLowerInvariant() switch
            {
                "title" => x => x.Title,
                "publishdate" => x => x.PublishDate,
                "createdat" => x => x.CreatedAt!,
                _ => x => x.PublishDate
            };
        }

        public static IQueryable<BlogPost> ApplyIncludes(this IQueryable<BlogPost> query)
        {
            return query
                .Include(x => x.Author)
                .Include(x => x.PostCategory)
                .Include(x => x.PostTags)
                    .ThenInclude(pt => pt.Tag);
        }
    }
}