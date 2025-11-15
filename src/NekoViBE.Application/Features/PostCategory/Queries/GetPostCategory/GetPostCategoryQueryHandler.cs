// NekoViBE.Application.Features.PostCategory.Queries.GetPostCategory/GetPostCategoryQueryHandler.cs
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Enums;
using NekoViBE.Application.Common.Interfaces;
using NekoViBE.Application.Common.Models;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetPostCategory;

public class GetPostCategoryQueryHandler : IRequestHandler<GetPostCategoryQuery, Result<PostCategoryResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<GetPostCategoryQueryHandler> _logger;

    public GetPostCategoryQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetPostCategoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<Result<PostCategoryResponse>> Handle(GetPostCategoryQuery query, CancellationToken cancellationToken)
    {
        try
        {
            var entity = await _unitOfWork.Repository<Domain.Entities.PostCategory>()
                .GetFirstOrDefaultAsync(x => x.Id == query.Id && !x.IsDeleted);

            if (entity == null)
                return Result<PostCategoryResponse>.Failure("Post category not found", ErrorCodeEnum.NotFound);

            var response = _mapper.Map<PostCategoryResponse>(entity);
            return Result<PostCategoryResponse>.Success(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting post category with ID {Id}", query.Id);
            return Result<PostCategoryResponse>.Failure("Error getting post category", ErrorCodeEnum.InternalError);
        }
    }
}