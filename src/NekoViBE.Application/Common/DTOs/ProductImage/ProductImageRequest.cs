using Microsoft.AspNetCore.Http;
using NekoViBE.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NekoViBE.Application.Common.DTOs.ProductImage
{
    public class ProductImageRequest
    {
        [Required(ErrorMessage = "Product ID is required")]
        public Guid ProductId { get; set; }

        [Required(ErrorMessage = "Image file is required")]
        public IFormFile Image { get; set; } = null!;

        public bool IsPrimary { get; set; } = false;

        public int DisplayOrder { get; set; } = 0;

        public EntityStatusEnum Status { get; set; } = EntityStatusEnum.Active;
    }
}
