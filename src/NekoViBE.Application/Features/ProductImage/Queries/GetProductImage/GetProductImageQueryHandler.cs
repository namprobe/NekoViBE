using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductImage;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductImage.Queries.GetProductImage
{
    public class GetProductImageQueryHandler : IRequestHandler<GetProductImageQuery, Result<ProductImageResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductImageQueryHandler> _logger;

        public GetProductImageQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductImageQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ProductImageResponse>> Handle(GetProductImageQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.ProductImage>()
                    .GetFirstOrDefaultAsync(x => x.Id == query.Id);

                if (entity == null)
                {
                    _logger.LogWarning("Product image with ID {Id} not found", query.Id);
                    return Result<ProductImageResponse>.Failure(
                        "Product image not found",
                        ErrorCodeEnum.NotFound);
                }

                var response = _mapper.Map<ProductImageResponse>(entity);
                return Result<ProductImageResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product image with ID {Id}", query.Id);
                return Result<ProductImageResponse>.Failure(
                    "Error getting product image",
                    ErrorCodeEnum.InternalError);
            }
        }
    }
}
