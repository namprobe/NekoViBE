using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class UpdateCouponRequest
    {
        [StringLength(500)]
        public string? Code { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, 1000000000000)]
        public decimal DiscountValue { get; set; }

        [Range(0, 1000000000000)]
        public decimal MinOrderAmount { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        [Range(1, 1000000)]
        public int? UsageLimit { get; set; }

        public EntityStatusEnum Status { get; set; }
    }
}
