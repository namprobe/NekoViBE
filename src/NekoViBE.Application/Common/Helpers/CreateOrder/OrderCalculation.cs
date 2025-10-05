using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Helpers.CreateOrder
{
    public class OrderCalculation
    {
        public decimal TotalAmount { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal ShippingAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public Dictionary<Guid, decimal> ItemPrices { get; set; } = new();
    }
}
