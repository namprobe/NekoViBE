using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Reports
{
    public class OrderSummary
    {
        public int CurrentMonth { get; set; }
        public int PreviousMonth { get; set; }
        public double PercentageChange { get; set; }
        public bool IsPositive => PercentageChange >= 0;
    }
}
