using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.ProductTag;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.ProductTag.Queries.GetProductTag
{
    public class GetProductTagQueryHandler : IRequestHandler<GetProductTagQuery, Result<ProductTagResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetProductTagQueryHandler> _logger;
        private readonly ICurrentUserService _currentUserService;

        public GetProductTagQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetProductTagQueryHandler> logger, ICurrentUserService currentUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _currentUserService = currentUserService;
        }

        public async Task<Result<ProductTagResponse>> Handle(GetProductTagQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var (isValid, userId) = await _currentUserService.IsUserValidAsync();
                if (!isValid)
                {
                    return Result<ProductTagResponse>.Failure("User is not valid", ErrorCodeEnum.Unauthorized);
                }

                // include Tag navigation property
                var productTag = await _unitOfWork.Repository<Domain.Entities.ProductTag>()
                    .GetFirstOrDefaultAsync(
                        predicate: x => x.Id == request.Id && !x.IsDeleted,
                        pt => pt.Tag  // Include Tag navigation property
                    );

                if (productTag == null)
                {
                    return Result<ProductTagResponse>.Failure("ProductTag not found", ErrorCodeEnum.NotFound);
                }
                var productTagResponse = _mapper.Map<ProductTagResponse>(productTag);
                return Result<ProductTagResponse>.Success(productTagResponse, "ProductTag retrieved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi lấy ProductTag");
                return Result<ProductTagResponse>.Failure("Error retrieving ProductTag", ErrorCodeEnum.InternalError);
            }
        }
    }
}
