using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Tag;
using NekoViBE.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Tag.Queries.GetSelectList
{
    public class GetTagSelectListQueryHandler : IRequestHandler<GetTagSelectListQuery, List<TagSelectItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetTagSelectListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<TagSelectItem>> Handle(GetTagSelectListQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<Domain.Entities.Tag>().GetQueryable();

            if (!string.IsNullOrEmpty(request.Search))
                query = query.Where(x => x.Name.Contains(request.Search));

            return await query
                .Where(x => !x.IsDeleted) // Chỉ lấy các Tag chưa bị xóa
                .OrderBy(x => x.Name)
                .Select(x => new TagSelectItem { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }
    }
}
