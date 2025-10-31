using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.User.Commands.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, Result>
    {
        private readonly ILogger<DeleteUserCommandHandler> _logger;
        private readonly UserManager<AppUser> _userManager;
        private readonly ICurrentUserService _currentUserService;

        public DeleteUserCommandHandler(
            ILogger<DeleteUserCommandHandler> logger,
            UserManager<AppUser> userManager,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _userManager = userManager;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var user = await _userManager.FindByIdAsync(request.Id.ToString());
                if (user == null)
                {
                    return Result.Failure("User not found", ErrorCodeEnum.NotFound);
                }

                // Prevent self-deletion
                if (user.Id == currentUserId)
                {
                    return Result.Failure("Cannot delete your own account", ErrorCodeEnum.DatabaseError);
                }

                var result = await _userManager.DeleteAsync(user);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return Result.Failure("Failed to delete user", ErrorCodeEnum.DatabaseError, errors);
                }

                _logger.LogInformation("User {UserId} deleted successfully by {CurrentUser}", request.Id, currentUserId);
                return Result.Success("User deleted successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting user {UserId}", request.Id);
                return Result.Failure("Error deleting user", ErrorCodeEnum.InternalError);
            }
        }
    }
}
