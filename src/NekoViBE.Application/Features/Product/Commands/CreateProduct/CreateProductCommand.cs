using MediatR;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Models;


namespace NekoViBE.Application.Features.Product.Commands.CreateProduct
{
    public record CreateProductCommand(ProductRequest Request) : IRequest<Result>;

}
