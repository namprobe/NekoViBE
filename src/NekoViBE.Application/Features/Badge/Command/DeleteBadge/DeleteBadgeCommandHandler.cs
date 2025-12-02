using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Badge.Command.DeleteBadge
{
    public class DeleteBadgeCommandHandler : IRequestHandler<DeleteBadgeCommand, Result>
    {
        private readonly ILogger<DeleteBadgeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUserService;

        public DeleteBadgeCommandHandler(
            ILogger<DeleteBadgeCommandHandler> logger,
            IUnitOfWork unitOfWork,
            ICurrentUserService currentUserService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(DeleteBadgeCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
                if (!isValid)
                {
                    return Result.Failure("Unauthorized", ErrorCodeEnum.Unauthorized);
                }

                var badge = await _unitOfWork.Repository<Domain.Entities.Badge>().GetByIdAsync(request.Id);
                if (badge == null)
                {
                    return Result.Failure("Badge not found", ErrorCodeEnum.NotFound);
                }

                // Soft delete - set status to Inactive instead of hard delete
                badge.Status = EntityStatusEnum.Inactive;
                badge.UpdatedAt = DateTime.UtcNow;
                badge.UpdatedBy = currentUserId;

                _unitOfWork.Repository<Domain.Entities.Badge>().Update(badge);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return Result.Success("Badge archived successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting badge {BadgeId}", request.Id);
                return Result.Failure("Error deleting badge", ErrorCodeEnum.InternalError);
            }
        }
    }
}
