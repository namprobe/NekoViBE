using MediatR;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest<Result>;

}
