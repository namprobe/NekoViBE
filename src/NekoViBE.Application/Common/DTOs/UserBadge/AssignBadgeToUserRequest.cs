using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class AssignBadgeToUserRequest
    {
        public Guid UserId { get; set; }
        public Guid BadgeId { get; set; }
        //public bool IsActive { get; set; } = true;
        //public DateTime? ActivatedFrom { get; set; }
        //public DateTime? ActivatedTo { get; set; }
    }
}
