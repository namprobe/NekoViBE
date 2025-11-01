// File: Application/Common/QueryBuilders/BlogPostQueryBuilder.cs
using Google.Apis.Gmail.v1.Data;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class BlogPostQueryBuilder
    {
        public static Expression<Func<BlogPost, bool>> BuildPredicate(this BlogPostFilter filter)
        {
            var predicate = PredicateBuilder.True<BlogPost>();

            // Status
            if (filter.Status.HasValue)
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            else
                predicate = predicate.CombineAnd(x => x.Status == EntityStatusEnum.Active);

            // Search
            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicate = predicate.CombineAnd(x =>
                    x.Title.ToLower().Contains(search) ||
                    (x.Content != null && x.Content.ToLower().Contains(search)));
            }

            // Title
            if (!string.IsNullOrWhiteSpace(filter.Title))
                predicate = predicate.CombineAnd(x => x.Title.Contains(filter.Title));

            // Category
            if (filter.PostCategoryId.HasValue)
                predicate = predicate.CombineAnd(x => x.PostCategoryId == filter.PostCategoryId.Value);

            // Author
            if (filter.AuthorId.HasValue)
                predicate = predicate.CombineAnd(x => x.AuthorId == filter.AuthorId.Value);

            // Published
            if (filter.IsPublished.HasValue)
                predicate = predicate.CombineAnd(x => x.IsPublished == filter.IsPublished.Value);

            // Tags (ANY)
            if (filter.TagIds != null && filter.TagIds.Any())
            {
                predicate = predicate.CombineAnd(x => x.PostTags.Any(pt => filter.TagIds.Contains(pt.TagId)));
            }

            return predicate;
        }

        public static Expression<Func<BlogPost, object>> BuildOrderBy(this BlogPostFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.PublishDate;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "title" => x => x.Title,
                "publishdate" => x => x.PublishDate,
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.PublishDate
    };
}
    }
}