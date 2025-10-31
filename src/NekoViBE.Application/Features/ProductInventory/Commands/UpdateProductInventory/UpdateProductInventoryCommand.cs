using MediatR;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Commands.UpdateProductInventory
{
    public record UpdateProductInventoryCommand(Guid Id, ProductInventoryRequest Request) : IRequest<Result>;
}
