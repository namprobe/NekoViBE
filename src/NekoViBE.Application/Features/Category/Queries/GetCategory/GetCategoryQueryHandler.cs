using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Queries.GetCategory
{
    public class GetCategoryQueryHandler : IRequestHandler<GetCategoryQuery, Result<CategoryResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCategoryQueryHandler> _logger;

        public GetCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCategoryQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.Category>().GetFirstOrDefaultAsync(x => x.Id == query.Id);

                if (entity == null)
                    return Result<CategoryResponse>.Failure("Category not found", ErrorCodeEnum.NotFound);

                if (string.IsNullOrEmpty(entity.ImagePath))
                    _logger.LogWarning("Category with ID {Id} has no ImagePath", query.Id);
                else
                    _logger.LogInformation("Category with ID {Id} has ImagePath: {ImagePath}", query.Id, entity.ImagePath);

                var response = _mapper.Map<CategoryResponse>(entity);
                return Result<CategoryResponse>.Success(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting category with ID {Id}", query.Id);
                return Result<CategoryResponse>.Failure("Error getting category", ErrorCodeEnum.InternalError);
            }
        }
    }
}
