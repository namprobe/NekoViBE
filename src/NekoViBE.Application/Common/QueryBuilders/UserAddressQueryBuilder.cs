using System.Linq.Expressions;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Domain.Entities;

namespace NekoViBE.Application.Common.QueryBuilders;

public static class UserAddressQueryBuilder
{
    public static Expression<Func<UserAddress, bool>> BuildPredicate(this UserAddressFilter filter)
    {
        var predicate = PredicateBuilder.True<UserAddress>();

        // Base predicate (Status filter từ BasePaginationFilter)
        if (filter.Status.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.Status == filter.Status.Value);
        }

        // Search predicate
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var searchPredicate = PredicateBuilder.False<UserAddress>();
            
            // Search trong FullName
            searchPredicate = searchPredicate.CombineOr(
                x => x.FullName.Contains(filter.Search));
            
            // Search trong Address (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.Address != null && x.Address.Contains(filter.Search));
            
            // Search trong City (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.City != null && x.City.Contains(filter.Search));
            
            // Search trong State (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.State != null && x.State.Contains(filter.Search));
            
            // Search trong PostalCode (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.PostalCode != null && x.PostalCode.Contains(filter.Search));
            
            // Search trong Country (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.Country != null && x.Country.Contains(filter.Search));
            
            // Search trong PhoneNumber (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.PhoneNumber != null && x.PhoneNumber.Contains(filter.Search));

            predicate = predicate.CombineAnd(searchPredicate);
        }

        // Custom filters
        if (filter.UserId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.UserId == filter.UserId.Value);
        }

        if (filter.IsDefault.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.IsDefault == filter.IsDefault.Value);
        }

        if (filter.AddressType.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.AddressType == filter.AddressType.Value);
        }

        return predicate;
    }

    /// <summary>
    /// Build OrderBy expression từ PaymentMethodFilter
    /// Default: CreatedAt giảm dần (newest first)
    /// </summary>
    public static Expression<Func<UserAddress, object>> BuildOrderBy(this UserAddressFilter filter)
    {
        if (string.IsNullOrWhiteSpace(filter.SortBy))
            return x => x.CreatedAt!; // Default: CreatedAt giảm dần

        return filter.SortBy.ToLowerInvariant() switch
        {
            "fullname" => x => x.FullName,
            "address" => x => x.Address,
            "city" => x => x.City,
            "state" => x => x.State!,
            "postalcode" => x => x.PostalCode,
            "country" => x => x.Country,
            "phonenumber" => x => x.PhoneNumber!,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt! // Fallback to CreatedAt
        };
    }
}