using MediatR;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Queries.GetTagList
{
    public record GetTagsQuery(TagFilter Filter) : IRequest<PaginationResult<TagItem>>;
}
