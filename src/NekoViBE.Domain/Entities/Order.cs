using NekoViBE.Domain.Common;
using NekoViBE.Domain.Enums;

namespace NekoViBE.Domain.Entities;

public class Order : BaseEntity
{
    public Guid? UserId { get; set; }
    public bool IsOneClick { get; set; } = false;
    public string? GuestEmail { get; set; }
    public string? GuestFirstName { get; set; }
    public string? GuestLastName { get; set; }
    public string? GuestPhone { get; set; }
    public string? OneClickAddress { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal DiscountAmount { get; set; } = 0;
    public decimal TaxAmount { get; set; } = 0;
    //public decimal ShippingAmount { get; set; } = 0;
    public decimal FinalAmount { get; set; }
    public PaymentStatusEnum PaymentStatus { get; set; } = PaymentStatusEnum.Pending;
    public OrderStatusEnum OrderStatus { get; set; } = OrderStatusEnum.Processing;
    public string? Notes { get; set; }
    
    // navigation properties
    public virtual AppUser? User { get; set; }
    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public virtual ICollection<OrderShippingMethod> OrderShippingMethods { get; set; } = new List<OrderShippingMethod>();
    public virtual Payment? Payment { get; set; } // 1-1 relationship
    public virtual ICollection<UserCoupon> UserCoupons { get; set; } = new List<UserCoupon>();
}
