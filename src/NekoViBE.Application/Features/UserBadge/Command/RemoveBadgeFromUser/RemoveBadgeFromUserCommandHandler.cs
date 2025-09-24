using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.UserBadge.Command.RemoveBadgeFromUser
{
    public class RemoveBadgeFromUserCommandHandler : IRequestHandler<RemoveBadgeFromUserCommand, Result>
    {
        private readonly ILogger<RemoveBadgeFromUserCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public RemoveBadgeFromUserCommandHandler(
            ILogger<RemoveBadgeFromUserCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(RemoveBadgeFromUserCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var userBadge = await _unitOfWork.Repository<Domain.Entities.UserBadge>().GetByIdAsync(request.Id);
                if (userBadge == null)
                {
                    return Result.Failure("User badge not found", ErrorCodeEnum.NotFound);
                }

                _unitOfWork.Repository<Domain.Entities.UserBadge>().Delete(userBadge);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Badge removed from user successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing badge from user {UserBadgeId}", request.Id);
                return Result.Failure("Error removing badge from user", ErrorCodeEnum.InternalError);
            }
        }
    }
}
