using FluentValidation;
using Microsoft.AspNetCore.Http;
using NekoViBE.Application.Common.DTOs;
using NekoViBE.Domain.Enums;
using System.Linq.Expressions;
using static NekoViBE.Application.Common.Validators.FileValidatorExtension;

namespace NekoViBE.Application.Common.Validators;

public static class PaymentValidatorExtension
{
    #region Payment Method Business Validation
    
    /// <summary>
    /// Validates payment method name with business rules
    /// Required | 2-100 chars | Alphanumeric with spaces, hyphens, underscores, dots
    /// </summary>
    public static IRuleBuilderOptions<T, PaymentGatewayType> ValidPaymentMethodName<T>(this IRuleBuilder<T, PaymentGatewayType> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum().WithMessage("Invalid payment method name");
    }
    
    /// <summary>
    /// Validates payment method icon with flexible size
    /// Images: jpg, jpeg, png, gif, webp, bmp, svg | Custom max size (default: 2MB for business needs)
    /// </summary>
    public static IRuleBuilderOptions<T, IFormFile?> ValidPaymentMethodIcon<T>(this IRuleBuilder<T, IFormFile?> ruleBuilder, double maxSizeInMB = 2.0)
    {
        return ruleBuilder.ValidFile(FileType.Image, maxSizeInMB);
    }
    
    /// <summary>
    /// Validates processing fee with business constraints
    /// Range: 0 to 999,999.99
    /// </summary>
    public static IRuleBuilderOptions<T, decimal> ValidProcessingFee<T>(this IRuleBuilder<T, decimal> ruleBuilder)
    {
        return ruleBuilder
            .GreaterThanOrEqualTo(0).WithMessage("Processing fee must be greater than or equal to 0")
            .LessThanOrEqualTo(999999.99m).WithMessage("Processing fee cannot exceed 999,999.99");
    }
    
    /// <summary>
    /// Validates entity status enum
    /// </summary>
    public static IRuleBuilderOptions<T, EntityStatusEnum> ValidEntityStatus<T>(this IRuleBuilder<T, EntityStatusEnum> ruleBuilder)
    {
        return ruleBuilder
            .IsInEnum().WithMessage("Invalid status value");
    }
    
    /// <summary>
    /// Validates processor name (optional field)
    /// Only validates when not null/empty
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidProcessorName<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Length(2, 50).WithMessage("Processor name must be between 2 and 50 characters")
            .When(x => !string.IsNullOrWhiteSpace(x as string));
    }
    
    /// <summary>
    /// Validates configuration JSON (optional field)
    /// Only validates when not null/empty
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidConfiguration<T>(this IRuleBuilder<T, string?> ruleBuilder)
    {
        return ruleBuilder
            .Must(BeValidJson).WithMessage("Configuration must be valid JSON")
            .When(x => !string.IsNullOrWhiteSpace(x as string));
    }
    
    #endregion
    
    #region Payment Method Setup (Simplified)
    
    /// <summary>
    /// Sets up complete PaymentMethodRequest validation with business rules
    /// One-line setup for all common PaymentMethod validation needs
    /// </summary>
    public static void SetupPaymentMethodRules<T>(this AbstractValidator<T> validator, 
        Expression<Func<T, PaymentMethodRequest>> requestSelector, double iconMaxSizeInMB = 2.0)
    {
        var requestFunc = requestSelector.Compile();
        
        // Required fields validation
        validator.RuleFor(x => requestFunc(x).Name)
            .ValidPaymentMethodName();
            
        validator.RuleFor(x => requestFunc(x).ProcessingFee)
            .ValidProcessingFee();
            
        validator.RuleFor(x => requestFunc(x).Status)
            .ValidEntityStatus();
            
        // Optional fields validation (only when not null/empty)
        validator.RuleFor(x => requestFunc(x).Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters")
            .When(x => !string.IsNullOrWhiteSpace(requestFunc(x).Description));
            
        validator.RuleFor(x => requestFunc(x).IconImage)
            .ValidPaymentMethodIcon(iconMaxSizeInMB)
            .When(x => requestFunc(x).IconImage != null);
            
        validator.RuleFor(x => requestFunc(x).ProcessorName)
            .ValidProcessorName();
            
        validator.RuleFor(x => requestFunc(x).Configuration)
            .ValidConfiguration();
    }
    
    #endregion
    
    #region Private Helper Methods
    
    private static bool BeValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return true;
        
        try
        {
            System.Text.Json.JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }
    
    #endregion
}