
namespace NekoViBE.Application.Common.Helpers.Coupon
{
    public class CouponDiscountResult
    {
        public Domain.Entities.Coupon Coupon { get; set; } = null!;
        public decimal DiscountAmount { get; set; }
        public bool IsValid { get; set; }
    }
}
