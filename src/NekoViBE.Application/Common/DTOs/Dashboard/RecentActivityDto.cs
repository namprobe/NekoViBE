using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Dashboard
{
    public class RecentActivityDto
    {
        public string Message { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string Type { get; set; } = string.Empty; // order-new, order-shipping, low-stock, user-new
        public string TimeAgo => FormatTimeAgo(Timestamp);

        private string FormatTimeAgo(DateTime dt)
        {
            var diff = DateTime.UtcNow - dt;
            return diff.TotalMinutes < 1 ? "vừa xong" :
                   diff.TotalMinutes < 60 ? $"{(int)diff.TotalMinutes} phút trước" :
                   diff.TotalHours < 24 ? $"{(int)diff.TotalHours} giờ trước" :
                   diff.TotalDays < 7 ? $"{(int)diff.TotalDays} ngày trước" :
                   dt.ToString("dd/MM/yyyy");
        }
    }
}
