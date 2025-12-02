using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Dashboard
{
    public class DashboardDataDto
    {
        public int TotalUsers { get; set; }
        public int NewUsersThisMonth { get; set; }
        public double NewUsersChangePercent { get; set; }

        public int TotalProducts { get; set; }
        public int NewProductsThisMonth { get; set; }
        public double NewProductsChangePercent { get; set; }

        public int TotalOrders { get; set; }
        public int OrdersThisMonth { get; set; }
        public double OrdersChangePercent { get; set; }

        public int PendingOrders { get; set; }
        public int PendingThisMonth { get; set; }
        public double PendingChangePercent { get; set; }

        public int ProcessingOrders { get; set; }
        public int ProcessingThisMonth { get; set; }
        public double ProcessingChangePercent { get; set; }

        public int CompletedOrders { get; set; }
        public int CompletedThisMonth { get; set; }
        public double CompletedChangePercent { get; set; }

        public decimal TotalRevenue { get; set; }
        public decimal RevenueThisMonth { get; set; }
        public double RevenueChangePercent { get; set; }

        public decimal AvgOrderValue { get; set; }
        public decimal AvgOrderValueThisMonth { get; set; }
        public double AovChangePercent { get; set; }

        public List<RecentActivityDto> RecentActivities { get; set; } = new();
    }
}
