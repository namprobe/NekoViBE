using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Commands.DeleteProductInventory
{
    public record DeleteProductInventoryCommand(Guid Id) : IRequest<Result>;
}
