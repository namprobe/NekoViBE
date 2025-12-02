using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;


namespace NekoViBE.Application.Features.Badge.Command.CreateBadge
{
    public class CreateBadgeCommandHandler : IRequestHandler<CreateBadgeCommand, Result>
    {
        private readonly ILogger<CreateBadgeCommandHandler> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUserService;
        private readonly IFileService _fileService;


        public CreateBadgeCommandHandler(
            ILogger<CreateBadgeCommandHandler> logger,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ICurrentUserService currentUserService,
            IFileService fileService)
        {
            _logger = logger;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUserService = currentUserService;
            _fileService = fileService;
        }
        public async Task<Result> Handle(CreateBadgeCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create badge");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var badgeRepo = _unitOfWork.Repository<Domain.Entities.Badge>();
                var couponRepo = _unitOfWork.Repository<Domain.Entities.Coupon>();
                

                var entity = _mapper.Map<Domain.Entities.Badge>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                if (command.Request.IconPath != null)
                {
                    var imagePath = await _fileService.UploadFileAsync(command.Request.IconPath, "uploads/badge", cancellationToken);
                    entity.IconPath = imagePath;
                    _logger.LogInformation("ImagePath set to {ImagePath} for badge {Name}", imagePath, entity.Name);
                }
                else
                {
                    _logger.LogWarning("No ImageFile provided for badge {Name}", command.Request.Name);
                }

                // Auto-generate a coupon for this badge
                var badgeCoupon = new Domain.Entities.Coupon
                {
                    Code = $"SYS_BADGE_{Guid.NewGuid():N}".ToUpper(), // Unique code
                    Description = $"Auto-generated coupon for badge: {entity.Name}",
                    DiscountType = DiscountTypeEnum.Percentage,
                    DiscountValue = entity.DiscountPercentage,
                    MaxDiscountCap = null, // No cap for badge discounts
                    MinOrderAmount = 0, // No minimum for badge discounts
                    StartDate = entity.StartDate ?? DateTime.UtcNow,
                    EndDate = entity.EndDate ?? DateTime.UtcNow.AddYears(10), // Default 10 years if not time-limited
                    UsageLimit = null, // Unlimited usage
                    CurrentUsage = 0,
                    IsBadgeCoupon = true, // Mark as badge-generated
                    CreatedBy = userId,
                    CreatedAt = DateTime.UtcNow,
                    Status = EntityStatusEnum.Active
                };

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    
                    // Create the coupon first to get its ID
                    await couponRepo.AddAsync(badgeCoupon);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                    
                    // Link the badge to the coupon
                    entity.LinkedCouponId = badgeCoupon.Id;
                    await badgeRepo.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "Badge",
                        NewValue = JsonSerializer.Serialize(new 
                        { 
                            Badge = command.Request,
                            LinkedCouponId = badgeCoupon.Id,
                            CouponCode = badgeCoupon.Code
                        }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created badge '{command.Request.Name}' with linked coupon '{badgeCoupon.Code}'",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                    
                    _logger.LogInformation("Badge {BadgeId} created with linked coupon {CouponId} (Code: {Code})", 
                        entity.Id, badgeCoupon.Id, badgeCoupon.Code);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Badge created successfully");
            }
            catch (IOException ex)
            {
                _logger.LogError(ex, "Error uploading file for badge");
                return Result.Failure("Error uploading file", ErrorCodeEnum.InternalError);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating badge");
                return Result.Failure("Error creating badge", ErrorCodeEnum.InternalError);
            }
        }
        //public async Task<Result> Handle(CreateBadgeCommand command, CancellationToken cancellationToken)
        //{
        //    try
        //    {
        //        var (isValid, currentUserId, _) = await _currentUserService.ValidateUserWithRolesAsync();
        //        if (!isValid)
        //        {

        //            _logger.LogWarning("Invalid or unauthenticated user attempting to create product");
        //            return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
        //        }

        //        var badge = _mapper.Map<Domain.Entities.Badge>(command.Request);
        //        badge.InitializeEntity(currentUserId);

        //        await _unitOfWork.Repository<Domain.Entities.Badge>().AddAsync(badge);
        //        await _unitOfWork.SaveChangesAsync(cancellationToken);

        //        return Result.Success("Badge created successfully");

        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error creating product with name: {Name}", command.Request.Name);
        //        return Result.Failure("Error creating product", ErrorCodeEnum.InternalError);
        //    }
        //}
    }
}
