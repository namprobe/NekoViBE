using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.UserBadge
{
    public class UserBadgesResponse
    {
        public List<UserBadgeDto> UserBadges { get; set; } = new();
    }
}
