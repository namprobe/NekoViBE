using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Domain.Entities;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Commands.UpdateEventProduct
{
    public class UpdateEventProductCommandHandler : IRequestHandler<UpdateEventProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateEventProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public UpdateEventProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<UpdateEventProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(UpdateEventProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to update event product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var repo = _unitOfWork.Repository<Domain.Entities.EventProduct>();
                var entity = await repo.GetFirstOrDefaultAsync(x => x.Id == command.Id && !x.IsDeleted);
                if (entity == null)
                    return Result.Failure("Event product not found", ErrorCodeEnum.NotFound);

                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .AnyAsync(x => x.Id == command.Request.EventId && !x.IsDeleted);
                if (!eventExists)
                    return Result.Failure("Event not found", ErrorCodeEnum.NotFound);

                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>()
                    .AnyAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (!productExists)
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                var entityExists = await repo.AnyAsync(x => x.EventId == command.Request.EventId && x.ProductId == command.Request.ProductId && x.Id != command.Id && !x.IsDeleted);
                if (entityExists)
                    return Result.Failure("Event product already exists", ErrorCodeEnum.ResourceConflict);

                var oldValue = JsonSerializer.Serialize(_mapper.Map<EventProductRequest>(entity));
                var oldStatus = entity.Status;

                _mapper.Map(command.Request, entity);
                entity.UpdatedBy = userId;
                entity.UpdatedAt = DateTime.UtcNow;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    repo.Update(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Update,
                        EntityId = entity.Id,
                        EntityName = "EventProduct",
                        OldValue = oldValue,
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Updated event product for EventId: {command.Request.EventId}, ProductId: {command.Request.ProductId}",
                        CreatedAt = DateTime.UtcNow,
                        Status = EntityStatusEnum.Active
                    };
                    await _unitOfWork.Repository<UserAction>().AddAsync(userAction);

                    if (oldStatus != EntityStatusEnum.Active)
                    {
                        var statusChangeAction = new UserAction
                        {
                            UserId = userId.Value,
                            Action = UserActionEnum.StatusChange,
                            EntityId = entity.Id,
                            EntityName = "EventProduct",
                            OldValue = oldStatus.ToString(),
                            NewValue = EntityStatusEnum.Active.ToString(),
                            IPAddress = _currentUserService.IPAddress ?? "Unknown",
                            ActionDetail = $"Changed status of event product from {oldStatus} to {EntityStatusEnum.Active}",
                            CreatedAt = DateTime.UtcNow,
                            Status = EntityStatusEnum.Active
                        };
                        await _unitOfWork.Repository<UserAction>().AddAsync(statusChangeAction);
                    }

                    await _unitOfWork.CommitTransactionAsync(cancellationToken);
                }
                catch
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    throw;
                }

                return Result.Success("Event product updated successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating event product with ID: {Id}", command.Id);
                return Result.Failure("Error updating event product", ErrorCodeEnum.InternalError);
            }
        }
    }
}
