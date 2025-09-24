using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Badge
{
    public class BadgeDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? IconPath { get; set; }
        public decimal DiscountPercentage { get; set; }
        public ConditionTypeEnum ConditionType { get; set; }
        public string ConditionValue { get; set; } = string.Empty;
        public bool IsTimeLimited { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public Guid? UpdatedBy { get; set; }
        public EntityStatusEnum Status { get; set; }
    }
}
