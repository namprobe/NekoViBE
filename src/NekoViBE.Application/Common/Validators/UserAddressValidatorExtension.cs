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
    /// Validates city with business rules
    /// Required | 2-50 chars | Letters and spaces
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidCity<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("City is required")
            .Length(2, 50).WithMessage("City must be between 2 and 50 characters")
            .Matches(@"^[\p{L}\s\-'.]+$").WithMessage("City can only contain letters, spaces, hyphens, apostrophes, and dots");
    }
    
    /// <summary>
    /// Validates state (optional field)
    /// Only validates when not null/empty
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidState<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Length(2, 50).WithMessage("State must be between 2 and 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x as string));
    }
    
    /// <summary>
    /// Validates postal code with business rules
    /// Required | 4-10 chars | Alphanumeric with optional spaces/hyphens
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidPostalCode<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Postal code is required")
            .Length(4, 10).WithMessage("Postal code must be between 4 and 10 characters")
            .Matches(@"^[a-zA-Z0-9\s\-]+$").WithMessage("Postal code can only contain letters, numbers, spaces, and hyphens");
    }
    
    /// <summary>
    /// Validates country with business rules
    /// Required | 2-50 chars | Letters and spaces
    /// </summary>
    public static IRuleBuilderOptions<T, string> ValidCountry<T>(this IRuleBuilder<T, string> ruleBuilder)
    {
        return ruleBuilder
            .NotEmpty().WithMessage("Country is required")
            .Length(2, 50).WithMessage("Country must be between 2 and 50 characters")
            .Matches(@"^[\p{L}\s\-'.]+$").WithMessage("Country can only contain letters, spaces, hyphens, apostrophes, and dots");
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
            
        validator.RuleFor(x => requestFunc(x).City)
            .ValidCity();
            
        validator.RuleFor(x => requestFunc(x).PostalCode)
            .ValidPostalCode();
            
        validator.RuleFor(x => requestFunc(x).Country)
            .ValidCountry();
            
        validator.RuleFor(x => requestFunc(x).AddressType)
            .ValidAddressType();
            
        // Use ValidEntityStatus from PaymentValidatorExtension (shared validation)
        validator.RuleFor(x => requestFunc(x).Status)
            .ValidEntityStatus();
        
        // Optional fields validation
        if (requireAll)
        {
            // For Update: State is required if provided initially
            validator.RuleFor(x => requestFunc(x).State)
                .NotEmpty().WithMessage("State is required")
                .Length(2, 50).WithMessage("State must be between 2 and 50 characters")
                .When(x => requestFunc(x).State != null);
                
            // For Update: PhoneNumber is required if provided initially
            validator.RuleFor(x => requestFunc(x).PhoneNumber)
                .NotEmpty().WithMessage("Phone number is required")
                .Matches(@"^(0|\+84)[0-9]{9,10}$").WithMessage("Phone number must be a valid Vietnamese phone number")
                .When(x => requestFunc(x).PhoneNumber != null);
        }
        else
        {
            // For Create: Optional fields validation
            validator.RuleFor(x => requestFunc(x).State)
                .ValidState();
                
            validator.RuleFor(x => requestFunc(x).PhoneNumber)
                .ValidAddressPhoneNumber();
        }
    }
    
    #endregion
}

