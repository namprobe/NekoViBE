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
        private readonly IFileService _fileService;

        public GetCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCategoryQueryHandler> logger, IFileService fileService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _fileService = fileService;
        }

        public async Task<Result<CategoryResponse>> Handle(GetCategoryQuery query, CancellationToken cancellationToken)
        {
            try
            {
                var entity = await _unitOfWork.Repository<Domain.Entities.Category>().GetFirstOrDefaultAsync(x => x.Id == query.Id);

                if (entity == null)
                    return Result<CategoryResponse>.Failure("Category not found", ErrorCodeEnum.NotFound);

                entity.ImagePath = _fileService.GetFileUrl(entity.ImagePath);

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
