using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductImage
{
    public class ProductImageFilter
    {
        public Guid? ProductId { get; set; }
        public bool? IsPrimary { get; set; }
        public EntityStatusEnum? Status { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
