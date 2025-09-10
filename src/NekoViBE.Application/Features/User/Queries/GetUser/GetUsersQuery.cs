using MediatR;
using NekoViBE.Application.Common.DTOs.User;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Queries.GetUser
{
    public record GetUsersQuery : IRequest<Result<UsersResponse>>;

    public class UsersResponse
    {
        public List<UserDTO> Users { get; set; } = new();
    }
}
