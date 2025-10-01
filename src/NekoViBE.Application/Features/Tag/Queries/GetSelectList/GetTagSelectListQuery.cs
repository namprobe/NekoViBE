using MediatR;
using NekoViBE.Application.Common.DTOs.Tag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Queries.GetSelectList
{
    public class GetTagSelectListQuery : IRequest<List<TagSelectItem>>
    {
        public string? Search { get; set; }
    }
}
