using MediatR;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Commands.CreateCategory
{
    public record CreateCategoryCommand(CategoryRequest Request) : IRequest<Result>;
}
