using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.EventProduct;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.EventProduct.Queries.GetEventProduct
{
    public class GetEventProductQueryHandler : IRequestHandler<GetEventProductQuery, Result<EventProductResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetEventProductQueryHandler> _logger;

        public GetEventProductQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetEventProductQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<EventProductResponse>> Handle(GetEventProductQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.EventProduct>()
                    .GetFirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted);
                if (entity == null)
                    return Result<EventProductResponse>.Failure("Event product not found", ErrorCodeEnum.NotFound);

                var response = _mapper.Map<EventProductResponse>(entity);
                return Result<EventProductResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting event product with ID {Id}", query.Id);
                return Result<EventProductResponse>.Failure("Error getting event product", ErrorCodeEnum.InternalError);
            }
        }
    }
}
