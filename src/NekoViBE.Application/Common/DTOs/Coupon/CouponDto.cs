using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Coupon
{
    public class CouponDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountTypeEnum DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal MinOrderAmount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int? UsageLimit { get; set; }
        public int CurrentUsage { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public EntityStatusEnum Status { get; set; }
        public string StatusName => Status.ToString();
        public bool IsActive => Status == EntityStatusEnum.Active &&
                               StartDate <= DateTime.UtcNow &&
                               EndDate >= DateTime.UtcNow &&
                               (UsageLimit == null || CurrentUsage < UsageLimit);
    }
}
