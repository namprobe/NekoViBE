using NekoViBE.Application.Common.DTOs.Event;
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
    public static class EventQueryBuilder
    {
        public static Expression<Func<Event, bool>> BuildPredicate(this EventFilter filter)
        {
            var predicate = PredicateBuilder.True<Event>()
                .CombineAnd(x => !x.IsDeleted); // Exclude deleted events by default

            if (filter.Status.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Search))
            {
                var searchPredicate = PredicateBuilder.False<Event>();
                searchPredicate = searchPredicate.CombineOr(x => x.Name.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Description != null && x.Description.Contains(filter.Search));
                searchPredicate = searchPredicate.CombineOr(x => x.Location != null && x.Location.Contains(filter.Search));
                predicate = predicate.CombineAnd(searchPredicate);
            }

            if (!string.IsNullOrWhiteSpace(filter.Name))
            {
                predicate = predicate.CombineAnd(x => x.Name == filter.Name);
            }

            if (filter.StartDate.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.StartDate >= filter.StartDate.Value);
            }

            if (filter.EndDate.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.EndDate <= filter.EndDate.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.Location))
            {
                predicate = predicate.CombineAnd(x => x.Location == filter.Location);
            }

            return predicate;
        }

        public static Expression<Func<Event, object>> BuildOrderBy(this EventFilter filter)
        {
            if (string.IsNullOrWhiteSpace(filter.SortBy))
                return x => x.CreatedAt!;

            return filter.SortBy.ToLowerInvariant() switch
            {
                "name" => x => x.Name,
                "startdate" => x => x.StartDate,
                "enddate" => x => x.EndDate,
                "location" => x => x.Location ?? "",
                "createdat" => x => x.CreatedAt!,
                "updatedat" => x => x.UpdatedAt!,
                _ => x => x.CreatedAt!
            };
        }
    }
}
