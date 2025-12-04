// NekoViBE.Application.Features.PostCategory.Queries.GetSelectList/GetPostCategorySelectListQueryHandler.cs
using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.PostCategory.Queries.GetSelectList;

public class GetPostCategorySelectListQueryHandler : IRequestHandler<GetPostCategorySelectListQuery, List<PostCategorySelectItem>>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetPostCategorySelectListQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PostCategorySelectItem>> Handle(GetPostCategorySelectListQuery request, CancellationToken cancellationToken)
    {
        var query = _unitOfWork.Repository<Domain.Entities.PostCategory>().GetQueryable()
            .Where(x => !x.IsDeleted && x.Status == Domain.Enums.EntityStatusEnum.Active);

        if (!string.IsNullOrEmpty(request.Search))
            query = query.Where(x => x.Name.Contains(request.Search));

        return await query
            .OrderBy(x => x.Name)
            .Select(x => new PostCategorySelectItem { Id = x.Id, Name = x.Name })
            .ToListAsync(cancellationToken);
    }
}