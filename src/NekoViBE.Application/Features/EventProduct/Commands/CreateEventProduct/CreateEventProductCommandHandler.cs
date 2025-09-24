using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
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

namespace NekoViBE.Application.Features.EventProduct.Commands.CreateEventProduct
{
    public class CreateEventProductCommandHandler : IRequestHandler<CreateEventProductCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateEventProductCommandHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public CreateEventProductCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<CreateEventProductCommandHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(CreateEventProductCommand command, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid || userId == null)
                {
                    _logger.LogWarning("Invalid or unauthenticated user attempting to create event product");
                    return Result.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                var eventExists = await _unitOfWork.Repository<Domain.Entities.Event>()
                    .AnyAsync(x => x.Id == command.Request.EventId && !x.IsDeleted);
                if (!eventExists)
                    return Result.Failure("Event not found", ErrorCodeEnum.NotFound);

                var productExists = await _unitOfWork.Repository<Domain.Entities.Product>()
                    .AnyAsync(x => x.Id == command.Request.ProductId && !x.IsDeleted);
                if (!productExists)
                    return Result.Failure("Product not found", ErrorCodeEnum.NotFound);

                var entityExists = await _unitOfWork.Repository<Domain.Entities.EventProduct>()
                    .AnyAsync(x => x.EventId == command.Request.EventId && x.ProductId == command.Request.ProductId && !x.IsDeleted);
                if (entityExists)
                    return Result.Failure("Event product already exists", ErrorCodeEnum.ResourceConflict);

                var entity = _mapper.Map<Domain.Entities.EventProduct>(command.Request);
                entity.CreatedBy = userId;
                entity.CreatedAt = DateTime.UtcNow;
                entity.Status = EntityStatusEnum.Active;

                try
                {
                    await _unitOfWork.BeginTransactionAsync(cancellationToken);
                    await _unitOfWork.Repository<Domain.Entities.EventProduct>().AddAsync(entity);

                    var userAction = new UserAction
                    {
                        UserId = userId.Value,
                        Action = UserActionEnum.Create,
                        EntityId = entity.Id,
                        EntityName = "EventProduct",
                        NewValue = JsonSerializer.Serialize(command.Request),
                        IPAddress = _currentUserService.IPAddress ?? "Unknown",
                        ActionDetail = $"Created event product for EventId: {command.Request.EventId}, ProductId: {command.Request.ProductId}",
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

                return Result.Success("Event product created successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating event product");
                return Result.Failure("Error creating event product", ErrorCodeEnum.InternalError);
            }
        }
    }
}
