using MediatR;
using NekoViBE.Application.Common.DTOs.Badge;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Badge.Command.UpdateBadge
{
    public record UpdateBadgeCommand(Guid Id, UpdateBadgeRequest Request) : IRequest<Result>;
    
}
