using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductInventory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductInventory.Queries.GetProductInventory
{
    public class GetProductInventoryQueryHandler : IRequestHandler<GetProductInventoryQuery, Result<ProductInventoryResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductInventoryQueryHandler> _logger;

        public GetProductInventoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductInventoryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ProductInventoryResponse>> Handle(GetProductInventoryQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.ProductInventory>().GetFirstOrDefaultAsync(x => x.Id == query.Id);
                if (entity == null)
                {
                    _logger.LogWarning("Product inventory with ID {Id} not found", query.Id);
                    return Result<ProductInventoryResponse>.Failure("Product inventory not found", ErrorCodeEnum.NotFound);
                }

                var response = _mapper.Map<ProductInventoryResponse>(entity);
                return Result<ProductInventoryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product inventory with ID {Id}", query.Id);
                return Result<ProductInventoryResponse>.Failure("Error getting product inventory", ErrorCodeEnum.InternalError);
            }
        }
    }
}
