using System;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

/// <summary>
/// Payment information for CMS order detail view.
/// Contains detailed payment method and transaction information.
/// </summary>
public class OrderPaymentDto
{
    public Guid Id { get; set; }
    public Guid PaymentMethodId { get; set; }
    public string PaymentMethodName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string? TransactionNo { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; }
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; }
    public string? ProcessorResponse { get; set; }
}

