using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Reports
{
    public class DashboardSummaryResponse
    {
        public RevenueSummary Revenue { get; set; } = new();
        public OrderSummary Orders { get; set; } = new();
        public UserSummary ActiveUsers { get; set; } = new();
        public List<RecentActivityItem> RecentActivities { get; set; } = new();
    }
}
