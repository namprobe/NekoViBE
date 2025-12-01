using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Common.QueryBuilders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace NekoViBE.Application.Features.ProductInventory.Queries.GetProductInventoryList
{
    public class GetProductInventoryListQueryHandler : IRequestHandler<GetProductInventoryListQuery, PaginationResult<ProductInventoryItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductInventoryListQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetProductInventoryListQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetProductInventoryListQueryHandler> logger,
            ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<PaginationResult<ProductInventoryItem>> Handle(GetProductInventoryListQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, _) = await _currentUserService.IsUserValidAsync();
                if (!isValid)
                {
                    return PaginationResult<ProductInventoryItem>.Failure(
                        "User is not valid",
                        ErrorCodeEnum.Unauthorized);
                }

                var predicate = request.Filter.BuildPredicate();
                var orderBy = request.Filter.BuildOrderBy();
                var isAscending = request.Filter.IsAscending ?? false;

                var (items, totalCount) = await _unitOfWork.Repository<Domain.Entities.ProductInventory>().GetPagedAsync(
                    pageNumber: request.Filter.Page,
                    pageSize: request.Filter.PageSize,
                    predicate: predicate,
                    orderBy: orderBy,
                    isAscending: isAscending
                );

                var productInventoryItems = _mapper.Map<List<ProductInventoryItem>>(items);

                    foreach (var dto in productInventoryItems)
                    {
                        var entity = items.First(x => x.Id.ToString() == dto.Id);

                        var importerId = entity.UpdatedBy ?? entity.CreatedBy;

                        var appUser = await _unitOfWork.Repository<Domain.Entities.AppUser>().GetFirstOrDefaultAsync(x => x.Id == importerId);
                        dto.Importer = appUser?.LastName;
                    }

                return PaginationResult<ProductInventoryItem>.Success(
                    productInventoryItems,
                    request.Filter.Page,
                    request.Filter.PageSize,
                    totalCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product inventory list with filter: {@Filter}", request.Filter);
                return PaginationResult<ProductInventoryItem>.Failure(
                    "Error getting product inventory list",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}
