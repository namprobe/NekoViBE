using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Commands.DeleteCategory
{
    public record DeleteCategoryCommand(Guid Id) : IRequest<Result>;
}
