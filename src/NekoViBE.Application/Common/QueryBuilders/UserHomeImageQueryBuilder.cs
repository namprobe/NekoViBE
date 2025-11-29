using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.QueryBuilders
{
    public static class UserHomeImageQueryBuilder
    {
        public static Expression<Func<UserHomeImage, bool>> BuildPredicate(this UserHomeImageFilter filter)
        {
            var predicate = PredicateBuilder.True<UserHomeImage>()
                .CombineAnd(x => !x.IsDeleted);

            if (filter.UserId.HasValue)
            {
                predicate = predicate.CombineAnd(x => x.UserId == filter.UserId.Value);
            }

            return predicate;
        }
    }
}