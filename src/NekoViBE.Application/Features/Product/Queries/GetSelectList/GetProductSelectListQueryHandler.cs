using MediatR;
using Microsoft.EntityFrameworkCore;
using NekoViBE.Application.Common.DTOs.Product;
using NekoViBE.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace NekoViBE.Application.Features.Product.Queries.GetSelectList
{
    public class GetProductSelectListQueryHandler : IRequestHandler<GetProductSelectListQuery, List<ProductSelectItem>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetProductSelectListQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<List<ProductSelectItem>> Handle(GetProductSelectListQuery request, CancellationToken cancellationToken)
        {
            var query = _unitOfWork.Repository<Domain.Entities.Product>().GetQueryable();

            // Lọc theo từ khóa tìm kiếm (nếu có)
            if (!string.IsNullOrEmpty(request.Search))
            {
                query = query.Where(x => x.Name.Contains(request.Search));
            }

            return await query
                .Where(x => !x.IsDeleted) // Chỉ lấy sản phẩm chưa bị xóa
                .OrderBy(x => x.Name)     // Sắp xếp theo tên
                .Select(x => new ProductSelectItem
                {
                    Id = x.Id,
                    Name = x.Name
                    // Mapping thêm các trường khác nếu DTO có
                })
                .ToListAsync(cancellationToken);
        }
    }
}