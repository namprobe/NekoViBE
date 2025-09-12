using MediatR;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Queries.GetCategory
{
    public record GetCategoryQuery(Guid Id) : IRequest<Result<CategoryResponse>>;
}
