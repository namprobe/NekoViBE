using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class Payment : BaseEntity
{
    public Guid OrderId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public string? TransactionId { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
    public DateTime? PaymentDate { get; set; }
    public string? Notes { get; set; } // For error messages, additional info
    public string? ProcessorResponse { get; set; } // Raw response from payment processor
    
    // navigation properties - 1-1 with Order
    public virtual Order Order { get; set; } = null!;
    public virtual PaymentMethod PaymentMethod { get; set; } = null!;
}
