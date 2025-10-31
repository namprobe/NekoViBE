using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class CouponsResponse
    {
        public List<CouponDto> Coupons { get; set; } = new();
    }
}
