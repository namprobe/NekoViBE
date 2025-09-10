using NekoViBE.Domain.Common;

namespace NekoViBE.Domain.Entities;

public class PaymentMethod : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconPath { get; set; }
    public bool IsOnlinePayment { get; set; } = true;
    public decimal ProcessingFee { get; set; } = 0;
    public string? ProcessorName { get; set; } // VnPay, PayPal, Stripe, etc.
    public string? Configuration { get; set; } // JSON config for payment processor
    
    // navigation properties
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
