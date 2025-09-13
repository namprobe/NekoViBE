using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Commands.DeleteProduct
{
    public record DeleteProductCommand(Guid Id) : IRequest<Result>;
}
