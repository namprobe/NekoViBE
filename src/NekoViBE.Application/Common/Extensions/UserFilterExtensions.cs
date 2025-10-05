using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Extensions
{
    public static class UserFilterExtensions
    {
        public static Expression<Func<AppUser, object>>? BuildOrderBy(this UserFilter filter)
        {
            if (string.IsNullOrEmpty(filter.SortBy))
                return null;

            return filter.SortBy.ToLower() switch
            {
                "email" => x => x.Email,
                "firstname" => x => x.FirstName,
                "lastname" => x => x.LastName,
                "createdat" => x => x.CreatedAt ?? x.JoiningAt,
                "lastloginat" => x => x.LastLoginAt,
                "status" => x => x.Status,
                "joiningat" => x => x.JoiningAt,
                "phonenumber" => x => x.PhoneNumber,
                _ => x => x.CreatedAt ?? x.JoiningAt // Default ordering
            };
        }

        public static Expression<Func<AppUser, bool>>? BuildPredicate(this UserFilter filter)
        {
            var predicates = new List<Expression<Func<AppUser, bool>>>();

            if (!string.IsNullOrEmpty(filter.Search))
            {
                var search = filter.Search.ToLower();
                predicates.Add(u =>
                    u.Email.ToLower().Contains(search) ||
                    u.FirstName.ToLower().Contains(search) ||
                    u.LastName.ToLower().Contains(search) ||
                    (u.PhoneNumber != null && u.PhoneNumber.Contains(search)));
            }

            if (!string.IsNullOrEmpty(filter.Email))
                predicates.Add(u => u.Email.Contains(filter.Email));

            if (!string.IsNullOrEmpty(filter.FirstName))
                predicates.Add(u => u.FirstName.Contains(filter.FirstName));

            if (!string.IsNullOrEmpty(filter.LastName))
                predicates.Add(u => u.LastName.Contains(filter.LastName));

            if (!string.IsNullOrEmpty(filter.PhoneNumber))
                predicates.Add(u => u.PhoneNumber != null && u.PhoneNumber.Contains(filter.PhoneNumber));

            if (filter.Status.HasValue)
                predicates.Add(u => u.Status == filter.Status.Value);

            if (filter.HasAvatar.HasValue)
            {
                if (filter.HasAvatar.Value)
                    predicates.Add(u => !string.IsNullOrEmpty(u.AvatarPath));
                else
                    predicates.Add(u => string.IsNullOrEmpty(u.AvatarPath));
            }

            // Handle role filtering (you'll need to join with user roles table)
            if (filter.RoleId.HasValue)
            {
                // This would require a more complex query joining with AspNetUserRoles
                // You might need to handle this differently
            }

            //if (filter.UserType.HasValue())
            //{
            //    // User type filtering would be handled in the query based on roles
            //}

            if (predicates.Count == 0)
                return null;

            // Combine all predicates with AND
            return predicates.Aggregate((current, next) =>
            {
                var parameter = Expression.Parameter(typeof(AppUser), "u");
                var body = Expression.AndAlso(
                    Expression.Invoke(current, parameter),
                    Expression.Invoke(next, parameter));
                return Expression.Lambda<Func<AppUser, bool>>(body, parameter);
            });
        }
    }
}
