using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Queries.GetProduct
{
    public class GetProductQueryHandler : IRequestHandler<GetProductQuery, Result<ProductResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductQueryHandler> _logger;

        public GetProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<ProductResponse>> Handle(GetProductQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.Product>().GetFirstOrDefaultAsync(x => x.Id == query.Id);

                if (entity == null)
                    return Result<ProductResponse>.Failure("Product not found", ErrorCodeEnum.NotFound);

                var primaryImage = entity.ProductImages.FirstOrDefault(x => x.IsPrimary);
                if (primaryImage == null || string.IsNullOrEmpty(primaryImage.ImagePath))
                    _logger.LogWarning("Product with ID {Id} has no primary image", query.Id);
                else
                    _logger.LogInformation("Product with ID {Id} has primary ImagePath: {ImagePath}", query.Id, primaryImage.ImagePath);

                var response = _mapper.Map<ProductResponse>(entity);
                return Result<ProductResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting product with ID {Id}", query.Id);
                return Result<ProductResponse>.Failure("Error getting product", ErrorCodeEnum.InternalError);
            }
        }
    }
}