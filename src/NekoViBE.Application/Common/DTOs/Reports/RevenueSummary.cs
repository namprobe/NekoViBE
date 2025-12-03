using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Reports
{
    public class RevenueSummary
    {
        public decimal CurrentMonth { get; set; }
        public decimal PreviousMonth { get; set; }
        public double PercentageChange { get; set; } // +20.1 hoặc -15.3
        public bool IsPositive => PercentageChange >= 0;
    }
}
