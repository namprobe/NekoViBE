using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class UserBadgeDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid BadgeId { get; set; }
        public DateTime EarnedDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime? ActivatedFrom { get; set; }
        public DateTime? ActivatedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public EntityStatusEnum Status { get; set; }

        // Additional info
        public string? UserName { get; set; }
        public string? BadgeName { get; set; }
        public decimal? BadgeDiscount { get; set; }
    }
}
