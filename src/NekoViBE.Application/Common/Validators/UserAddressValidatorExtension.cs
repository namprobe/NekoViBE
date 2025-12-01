using FluentValidation;
using NekoViBE.Application.Common.DTOs.UserAddress;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;

namespace NekoViBE.Application.Common.Validators;

public static class UserAddressValidatorExtension
{
    #region User Address Business Validation
    
    /// <summary>
    /// Validates full name with business rules
    /// Required | 2-100 chars | Letters, spaces, and Vietnamese characters
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidFullName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Full name is required")
            .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters")
            .Matches(@"^[\p{L}\s\-'.]+$").WithMessage("Full name can only contain letters, spaces, hyphens, apostrophes, and dots");
    }
    
    /// <summary>
    /// Validates address with business rules
    /// Required | 5-200 chars
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidAddress<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Address is required")
            .Length(5, 200).WithMessage("Address must be between 5 and 200 characters");
    }
    
    /// <summary>
    /// Validates GHN ProvinceId
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidProvinceId<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("ProvinceId is required");
    }
    
    /// <summary>
    /// Validates GHN ProvinceName
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidProvinceName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Province name is required")
            .Length(2, 100).WithMessage("Province name must be between 2 and 100 characters");
    }
    
    /// <summary>
    /// Validates GHN DistrictId
    /// </summary>
    public static IRuleBuilderOptions<T, int> ValidDistrictId<T>(this IRuleBuilder<T, int> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThan(0).WithMessage("DistrictId is required");
    }
    
    /// <summary>
    /// Validates GHN DistrictName
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidDistrictName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("District name is required")
            .Length(2, 100).WithMessage("District name must be between 2 and 100 characters");
    }
    
    /// <summary>
    /// Validates GHN WardCode
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidWardCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Ward code is required")
            .Length(2, 20).WithMessage("Ward code must be between 2 and 20 characters")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Ward code can only contain letters and numbers");
    }
    
    /// <summary>
    /// Validates GHN WardName
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidWardName<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Ward name is required")
            .Length(2, 100).WithMessage("Ward name must be between 2 and 100 characters");
    }
    
    /// <summary>
    /// Validates postal code when provided
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidOptionalPostalCode<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Length(4, 10).WithMessage("Postal code must be between 4 and 10 characters")
            .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Postal code can only contain letters, numbers, spaces, and hyphens")
            .When(x => !string.IsNullOrWhiteSpace(x as string));
    }
    
    /// <summary>
    /// Validates phone number for address (optional field)
    /// Only validates when not null/empty
    /// Format: Vietnamese phone numbers (10-11 digits)
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidAddressPhoneNumber<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Matches(@"^(0|\+84)[0-9]{9,10}$").WithMessage("Phone number must be a valid Vietnamese phone number (10-11 digits, starting with 0 or +84)")
            .When(x => !string.IsNullOrWhiteSpace(x as string));
    }
    
    /// <summary>
    /// Validates address type enum
    /// </summary>
    public static IRuleBuilderOptions<T, AddressTypeEnum> ValidAddressType<T>(this IRuleBuilder<T, AddressTypeEnum> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum().WithMessage("Invalid address type value");
    }
    
    #endregion
    
    #region User Address Setup (Simplified)
    
    /// <summary>
    /// Sets up complete UserAddressRequest validation with business rules
    /// One-line setup for all common UserAddress validation needs
    /// </summary>
    /// <param name="requireAll">If true, all optional fields become required (for Update operations)</param>
    public static void SetupUserAddressRules<T>(this AbstractValidator<T> validator, 
        Expression<Func<T, UserAddressRequest>> requestSelector, bool requireAll = false)
    {
        var requestFunc = requestSelector.Compile();
        
        // Required fields validation
        validator.RuleFor(x => requestFunc(x).FullName)
            .ValidFullName();
            
        validator.RuleFor(x => requestFunc(x).Address)
            .ValidAddress();
            
        validator.RuleFor(x => requestFunc(x).ProvinceId)
            .ValidProvinceId();
            
        validator.RuleFor(x => requestFunc(x).ProvinceName)
            .ValidProvinceName();
            
        validator.RuleFor(x => requestFunc(x).DistrictId)
            .ValidDistrictId();
            
        validator.RuleFor(x => requestFunc(x).DistrictName)
            .ValidDistrictName();
            
        validator.RuleFor(x => requestFunc(x).WardCode)
            .ValidWardCode();
            
        validator.RuleFor(x => requestFunc(x).WardName)
            .ValidWardName();
            
        validator.RuleFor(x => requestFunc(x).PostalCode)
            .ValidOptionalPostalCode();
            
        validator.RuleFor(x => requestFunc(x).AddressType)
            .ValidAddressType();
            
        // Use ValidEntityStatus from PaymentValidatorExtension (shared validation)
        validator.RuleFor(x => requestFunc(x).Status)
            .ValidEntityStatus();
        
        // Optional fields validation
        if (requireAll)
        {
            // For Update: PhoneNumber is required if provided initially
            validator.RuleFor(x => requestFunc(x).PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^(0|\+84)[0-9]{9,10}$").WithMessage("Phone number must be a valid Vietnamese phone number")
                .When(x => requestFunc(x).PhoneNumber != null);
        }
        else
        {
            validator.RuleFor(x => requestFunc(x).PhoneNumber)
                .ValidAddressPhoneNumber();
        }
    }
    
    #endregion
}

