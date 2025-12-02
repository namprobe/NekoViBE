using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.Application.Common.DTOs.PostCategory;
using NekoViBE.Application.Features.PostCategory.Queries.GetSelectList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/post-categorys")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_PostCategory")]
    [SwaggerTag("This API is used for post category management in customer")]
    public class PostCategoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        public PostCategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("select-list")]
        [ProducesResponseType(typeof(List<PostCategorySelectItem>), StatusCodes.Status200OK)]
        [SwaggerOperation(
        Summary = "Get post categories for select list",
        Description = "Used for dropdowns in Customer",
        OperationId = "GetPostCategorySelectList",
        Tags = new[] { "Customer", "Customer_PostCategory" }
    )]
        public async Task<IActionResult> GetPostCategorySelectList([FromQuery] string? search)
        {
            var query = new GetPostCategorySelectListQuery { Search = search };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
    }
}
