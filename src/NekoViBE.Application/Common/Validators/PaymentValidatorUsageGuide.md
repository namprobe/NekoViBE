# PaymentMethod Validator Usage Guide

## Overview
Hệ thống validator cho PaymentMethod được thiết kế để tối ưu, ngắn gọn và có thể tái sử dụng cho cả Create và Update operations.

## Components

### 1. PaymentValidatorExtension.cs
Chứa tất cả validation logic cho PaymentMethod:
- **Individual Field Validators**: Extension methods cho từng field riêng lẻ
- **Common Setup Methods**: Methods để setup tất cả rules một lần
- **Helper Methods**: Private methods để validate file, JSON, etc.

### 2. Field-specific Validators

```csharp
// Individual field validation
ruleBuilder.ValidPaymentMethodName();           // Name validation
ruleBuilder.ValidPaymentMethodDescription();    // Description validation
ruleBuilder.ValidPaymentMethodIcon();          // Icon file validation
ruleBuilder.ValidProcessingFee();              // Processing fee validation
ruleBuilder.ValidProcessorName(isOnlinePredicate); // Processor name (conditional)
ruleBuilder.ValidConfiguration(isOnlinePredicate);  // JSON config (conditional)
ruleBuilder.ValidEntityStatus();               // Status validation
```

## Usage Patterns

### Pattern 1: Direct PaymentMethodRequest Validation
Cho validators mà validate trực tiếp PaymentMethodRequest:

```csharp
public class DirectPaymentMethodValidator : AbstractValidator<PaymentMethodRequest>
{
    public DirectPaymentMethodValidator()
    {
        this.SetupPaymentMethodRequestRules();
    }
}
```

### Pattern 2: Nested PaymentMethodRequest Validation
Cho commands chứa PaymentMethodRequest (như CreatePaymentMethodCommand):

```csharp
public class CreatePaymentMethodValidator : AbstractValidator<CreatePaymentMethodCommand>
{
    public CreatePaymentMethodValidator()
    {
        RuleFor(x => x.Request)
            .NotNull().WithMessage("Payment method request is required");

        When(x => x.Request != null, () =>
        {
            this.SetupNestedPaymentMethodRequestRules(x => x.Request);
        });
    }
}
```

### Pattern 3: Custom Validation Setup
Cho cases đặc biệt cần custom validation logic:

```csharp
public class CustomPaymentMethodValidator : AbstractValidator<CustomCommand>
{
    public CustomPaymentMethodValidator()
    {
        this.SetupCommonPaymentMethodRules(
            x => x.SomeField.Name,
            x => x.SomeField.Description,
            x => x.SomeField.IconImage,
            x => x.SomeField.IsOnlinePayment,
            x => x.SomeField.ProcessingFee,
            x => x.SomeField.ProcessorName,
            x => x.SomeField.Configuration,
            x => x.SomeField.Status
        );
    }
}
```

## Validation Rules Details

### Name Validation
- Required
- Length: 2-100 characters
- Pattern: letters, numbers, spaces, hyphens, underscores, dots only

### Description Validation
- Optional
- Max length: 500 characters

### Icon Image Validation
- Optional
- Allowed formats: jpg, jpeg, png, gif
- Max size: 2MB

### Processing Fee Validation
- Range: 0 to 999,999.99
- Required

### Processor Name Validation
- Required only for online payments
- Length: 2-50 characters

### Configuration Validation
- Required only for online payments
- Must be valid JSON

### Status Validation
- Must be valid EntityStatusEnum value

## Benefits

### ✅ Reusability
- Same validation logic for Create/Update/Custom scenarios
- No code duplication
- Consistent validation across application

### ✅ Maintainability
- Single source of truth for validation rules
- Easy to modify rules in one place
- Clear separation of concerns

### ✅ Flexibility
- Multiple usage patterns for different scenarios
- Can combine with additional custom validation
- Conditional validation support

### ✅ Type Safety
- Full compile-time checking
- IntelliSense support
- Expression-based selectors

## Adding New Validators

### For new commands with PaymentMethodRequest:
```csharp
public class NewCommandValidator : AbstractValidator<NewCommand>
{
    public NewCommandValidator()
    {
        // Add command-specific validation
        RuleFor(x => x.Id).NotEmpty();
        
        // Add common PaymentMethod validation
        this.SetupNestedPaymentMethodRequestRules(x => x.Request);
    }
}
```

### For new direct PaymentMethodRequest scenarios:
```csharp
public class NewDirectValidator : AbstractValidator<PaymentMethodRequest>
{
    public NewDirectValidator()
    {
        // Use the pre-built setup
        this.SetupPaymentMethodRequestRules();
        
        // Add any additional rules if needed
        RuleFor(x => x.Name).Must(BeUniquePaymentMethodName);
    }
}
```

## Performance Considerations
- Extension methods are compiled once and reused
- Expression trees are compiled only when needed
- Validation is fast and memory-efficient
