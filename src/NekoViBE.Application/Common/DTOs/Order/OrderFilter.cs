using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    public class OrderFilter : BasePaginationFilter
    {
        public string? OrderNumber { get; set; }
        public Guid? UserId { get; set; }
        public string? UserEmail { get; set; }
        public string? GuestEmail { get; set; }
        public bool? IsOneClick { get; set; }
        public PaymentStatusEnum? PaymentStatus { get; set; }
        public OrderStatusEnum? OrderStatus { get; set; }
        public decimal? MinAmount { get; set; }
        public decimal? MaxAmount { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool? HasCoupon { get; set; }
        public string? ProductName { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? AnimeSeriesId { get; set; }
    }
    
}
