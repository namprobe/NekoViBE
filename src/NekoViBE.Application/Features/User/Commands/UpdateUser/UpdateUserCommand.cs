using MediatR;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.UpdateUser
{
    public record UpdateUserCommand(Guid Id, UpdateUserRequest Request) : IRequest<Result<UserDTO>>;

}
