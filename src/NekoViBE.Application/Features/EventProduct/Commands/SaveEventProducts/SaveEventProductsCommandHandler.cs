// SaveEventProductsCommandHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System.Text.Json;

namespace NekoViBE.Application.Features.Event.Commands.SaveEventProducts
{
    public class SaveEventProductsCommandHandler : IRequestHandler<SaveEventProductsCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<SaveEventProductsCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public SaveEventProductsCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<SaveEventProductsCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(SaveEventProductsCommand command, CancellationToken ct)
        {
            try
            {
                // 1. Kiểm tra quyền (admin)
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();

                // 2. Kiểm tra Event có tồn tại
                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .AnyAsync(e => e.Id == command.EventId && !e.IsDeleted);

                if (!eventExists)
                    return Result.Failure($"Event with ID {command.EventId} not found", ErrorCodeEnum.NotFound);

                // 3. Kiểm tra từng ProductId tồn tại + lấy Price để tính DiscountPrice sau
                var productIds = command.Requests.Select(r => r.ProductId).Distinct().ToList();
                var products = new List<Domain.Entities.Product>();
                foreach (var id in productIds)
                {
                    var product = await _unitOfWork.Repository<Domain.Entities.Product>()
                        .GetFirstOrDefaultAsync(p => p.Id == id && !p.IsDeleted);

                    if (product == null)
                    {
                        return Result.Failure($"Product with ID {id} not found", ErrorCodeEnum.NotFound);
                    }
                    products.Add(product);
                }

                var notFoundIds = productIds.Except(products.Select(p => p.Id)).ToList();
                if (notFoundIds.Any())
                    return Result.Failure($"Products not found: {string.Join(", ", notFoundIds)}", ErrorCodeEnum.NotFound);

                // 4. Bắt đầu transaction
                await _unitOfWork.BeginTransactionAsync(ct);

                // Lấy danh sách EventProduct hiện tại
                var currentEventProducts = await _unitOfWork.Repository<Domain.Entities.EventProduct>()
                    .GetQueryable()
                    .Where(ep => ep.EventId == command.EventId && !ep.IsDeleted)
                    .ToListAsync(ct);

                // Xóa cũ + ghi log UserAction
                if (currentEventProducts.Any())
                {
                    foreach (var ep in currentEventProducts)
                    {
                        var deleteAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.Delete,
                            EntityId = ep.Id,
                            EntityName = nameof(EventProduct),
                            OldValue = JsonSerializer.Serialize(new { ep.ProductId, ep.IsFeatured, ep.DiscountPercentage }),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Event reset: Removed product {ep.ProductId} from event",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(deleteAction);
                    }

                    _unitOfWork.Repository<Domain.Entities.EventProduct>().DeleteRange(currentEventProducts);
                }

                // Tạo mới + cập nhật DiscountPrice của Product
                foreach (var request in command.Requests)
                {
                    var product = products.First(p => p.Id == request.ProductId);

                    var newEventProduct = new Domain.Entities.EventProduct
                    {
                        EventId = command.EventId,
                        ProductId = request.ProductId,
                        IsFeatured = request.IsFeatured,
                        DiscountPercentage = request.DiscountPercentage,
                        CreatedBy = userId,
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };

                    await _unitOfWork.Repository<Domain.Entities.EventProduct>().AddAsync(newEventProduct);

                    // Tính và cập nhật DiscountPrice của Product
                    if (request.DiscountPercentage > 0)
                    {
                        product.DiscountPrice = product.Price * request.DiscountPercentage / 100;
                    }
                    else
                    {
                        product.DiscountPrice = null; // không giảm giá
                    }

                    // Ghi log thay đổi DiscountPrice
                    var updateAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = product.Id,
                        EntityName = nameof(Product),
                        OldValue = JsonSerializer.Serialize(new { OldDiscountPrice = product.DiscountPrice }),
                        NewValue = JsonSerializer.Serialize(new { NewDiscountPrice = product.DiscountPrice }),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Event save: Updated product {product.Id} discount to {request.DiscountPercentage}% → DiscountPrice = {product.DiscountPrice}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(updateAction);
                }

                // Commit
                await _unitOfWork.CommitTransactionAsync(ct);

                return Result.Success("Event products saved successfully and discount prices updated");
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(ct);
                _logger.LogError(ex, "Error saving EventProducts for Event {EventId}", command.EventId);
                return Result.Failure("Error saving event products", ErrorCodeEnum.InternalError);
            }
        }
    }
}