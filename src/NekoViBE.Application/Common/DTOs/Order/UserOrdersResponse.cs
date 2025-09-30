using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.Order
{
    public class UserOrdersResponse
    {
        public List<OrderDto> Orders { get; set; } = new();
    }
}
