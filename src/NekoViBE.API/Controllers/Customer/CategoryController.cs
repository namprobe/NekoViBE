using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Features.Category.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/categories")]
    [ApiExplorerSettings(GroupName = "v1")]
    [SwaggerTag("This API is used for customer-facing product retrieval")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("select-list")]
        [ProducesResponseType(typeof(List<CategorySelectItem>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategorySelectList([FromQuery] string? search)
        {
            var query = new GetCategorySelectListQuery { Search = search };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
