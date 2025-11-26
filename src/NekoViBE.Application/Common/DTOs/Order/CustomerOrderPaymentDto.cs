using System;
using System.Text.Json.Serialization;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Application.Common.DTOs.Order;

public class CustomerOrderPaymentDto
{
    public Guid PaymentId { get; set; }

    public Guid PaymentMethodId { get; set; }

    public string PaymentMethodName { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public PaymentStatusEnum PaymentStatus { get; set; }

    public string? TransactionNo { get; set; }

    public DateTime? PaymentDate { get; set; }
}

