using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Category.Queries.GetSelectList
{
    public class GetEventSelectListQueryHandler : IRequestHandler<GetCategorySelectListQuery, List<CategorySelectItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetEventSelectListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<CategorySelectItem>> Handle(GetCategorySelectListQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<Domain.Entities.Category>().GetQueryable();

            if (!string.IsNullOrEmpty(request.Search))
                query = query.Where(x => x.Name.Contains(request.Search));

            return await query
                .OrderBy(x => x.Name)
                .Take(50) // giới hạn combobox
                .Select(x => new CategorySelectItem { Id = x.Id, Name = x.Name })
                .ToListAsync(cancellationToken);
        }
    }
}
