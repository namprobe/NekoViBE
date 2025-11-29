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
            
            // Search trong PostalCode (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.PostalCode != null && x.PostalCode.Contains(filter.Search));
            
            // Search trong ProvinceName/DistrictName/WardName (null-safe)
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.ProvinceName != null && x.ProvinceName.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.DistrictName != null && x.DistrictName.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.WardName != null && x.WardName.Contains(filter.Search));
            searchPredicate = searchPredicate.CombineOr<UserAddress>(
                x => x.WardCode != null && x.WardCode.Contains(filter.Search));
            
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

        if (filter.ProvinceId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.ProvinceId == filter.ProvinceId.Value);
        }

        if (filter.DistrictId.HasValue)
        {
            predicate = predicate.CombineAnd(x => x.DistrictId == filter.DistrictId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.WardCode))
        {
            predicate = predicate.CombineAnd(x => x.WardCode == filter.WardCode);
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
            "provincename" => x => x.ProvinceName!,
            "districtname" => x => x.DistrictName!,
            "wardname" => x => x.WardName!,
            "postalcode" => x => x.PostalCode!,
            "phonenumber" => x => x.PhoneNumber!,
            "createdat" => x => x.CreatedAt!,
            "updatedat" => x => x.UpdatedAt!,
            _ => x => x.CreatedAt! // Fallback to CreatedAt
        };
    }
}