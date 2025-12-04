using MediatR;
using NekoViBE.Application.Common.DTOs.Product;
using System.Collections.Generic;

namespace NekoViBE.Application.Features.Product.Queries.GetSelectList
{
    public class GetProductSelectListQuery : IRequest<List<ProductSelectItem>>
    {
        public string? Search { get; set; }
    }
}