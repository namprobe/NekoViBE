using MediatR;
using NekoViBE.Application.Common.DTOs.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Queries.GetSelectList
{
    public class GetCategorySelectListQuery : IRequest<List<CategorySelectItem>>
    {
        public string? Search { get; set; }
    }
}
