using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class CreateCouponRequest
    {
        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public DiscountTypeEnum DiscountType { get; set; }

        [Range(0, 1000000000000)]
        public decimal DiscountValue { get; set; }

        [Range(0, 1000000000000)]
        public decimal? MaxDiscountCap { get; set; }

        [Range(0, 1000000000000)]
        public decimal MinOrderAmount { get; set; } = 0;

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Range(1, 1000000)]
        public int? UsageLimit { get; set; }
    }

}
