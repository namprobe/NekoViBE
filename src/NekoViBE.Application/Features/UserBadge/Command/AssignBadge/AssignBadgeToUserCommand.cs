using MediatR;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.DTOs.UserBadge;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.UserBadge.Command.AssignBadge
{
    //public record AssignBadgeToUserCommand(AssignBadgeToUserRequest Request) : IRequest<Result>;
    public record AssignBadgeToUserCommand(AssignBadgeToUserRequest Request) : IRequest<Result<UserBadgeDto>>;

}
