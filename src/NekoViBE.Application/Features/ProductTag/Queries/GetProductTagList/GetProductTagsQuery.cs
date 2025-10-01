using MediatR;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Queries.GetProductTagList
{
    public record GetProductTagsQuery(ProductTagFilter Filter) : IRequest<PaginationResult<ProductTagItem>>;
}
