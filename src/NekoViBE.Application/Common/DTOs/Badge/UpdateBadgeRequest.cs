using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Badge
{
    public class UpdateBadgeRequest
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public IFormFile? IconPath { get; set; }
        public decimal DiscountPercentage { get; set; }
        public ConditionTypeEnum ConditionType { get; set; }
        public string ConditionValue { get; set; } = string.Empty;
        public bool IsTimeLimited { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public EntityStatusEnum Status { get; set; }
    }
}
