using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Common;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.ProductTag.Commands.CreateProductTag
{
    public class CreateProductTagHandler : IRequestHandler<CreateProductTagCommand, Result>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateProductTagHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateProductTagHandler(IMapper mapper, IUnitOfWork unitOfWork,
            ILogger<CreateProductTagHandler> logger, ICurrentUserService currentUserService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateProductTagCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Người dùng không hợp lệ hoặc không được xác thực khi tạo ProductTag");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }
                if (!await _currentUserService.HasRoleAsync(RoleEnum.Admin))
                {
                    _logger.LogWarning("Người dùng không có quyền Admin để tạo ProductTag");
                    return Result.Failure("User is not allowed to create ProductTag", ErrorCodeEnum.Forbidden);
                }

                // Kiểm tra sự tồn tại của Product và Tag
                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>().AnyAsync(x => x.Id == command.Request.ProductId);
                if (!productExists)
                {
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);
                }

                var tagExists = await _unitOfWork.Repository<Domain.Entities.Tag>().AnyAsync(x => x.Id == command.Request.TagId);
                if (!tagExists)
                {
                    return Result.Failure("Tag not found", ErrorCodeEnum.NotFound);
                }

                var productTag = _mapper.Map<Domain.Entities.ProductTag>(command.Request);
                productTag.InitializeEntity(userId.Value);

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await _unitOfWork.Repository<Domain.Entities.ProductTag>().AddAsync(productTag);

                    // Ghi log UserAction cho hành động tạo
                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = productTag.Id,
                        EntityName = "ProductTag",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Tạo ProductTag với ProductId: {command.Request.ProductId}, TagId: {command.Request.TagId}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("ProductTag created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo ProductTag");
                return Result.Failure("Error creating ProductTag", ErrorCodeEnum.InternalError);
            }
        }
    }
}
