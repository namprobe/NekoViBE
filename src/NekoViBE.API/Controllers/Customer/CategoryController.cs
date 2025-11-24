using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.Category;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.Category.Queries.GetCategoryList;
using NekoViBE.Application.Features.Category.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/categories")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_Categories")]
    [SwaggerTag("This API is used for Category management in customer")]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(PaginationResult<CategoryItem>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get all categories with pagination and filtering",
            Description = "This API retrieves a paginated list of categories with filtering options",
            OperationId = "GetCategoryList",
            Tags = new[] { "Customer", "Customer_Category" }
        )]
        public async Task<IActionResult> GetCategoryList([FromQuery] CategoryFilter filter)
        {
            var query = new GetCategoryListQuery(filter);
            var result = await _mediator.Send(query);
            return StatusCode(result.GetHttpStatusCode(), result);
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
