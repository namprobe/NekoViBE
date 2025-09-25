using NekoViBE.Application.Common.DTOs.Order;
using NekoViBE.Application.Common.Helpers.CreateOrder;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.Interfaces
{
    public interface IOrderService
    {
        Task<ServiceResult<Order>> CreateOrderAsync(CreateOrderRequest request, Guid? userId, CancellationToken cancellationToken);
    }
}
