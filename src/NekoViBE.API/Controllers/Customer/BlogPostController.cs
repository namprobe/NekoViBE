using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.BlogPost.Queries.GetBlogPost;
using NekoViBE.Application.Features.BlogPost.Queries.GetBlogPostList;
using NekoViBE.Application.Features.BlogPost.Queries.GetLatestByCategory;
using Swashbuckle.AspNetCore.Annotations;
using NekoViBE.Application.Common.Extensions;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/blog-posts")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_BlogPost")]
    [SwaggerTag("This API is used for Blog Post management in customer")]
    public class BlogPostController : ControllerBase
    {
        private readonly IMediator _mediator;
        public BlogPostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(PaginationResult<BlogPostItem>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get blog posts with filter", OperationId = "GetBlogPostList")]
        public async Task<IActionResult> GetList([FromQuery] BlogPostFilter filter)
        {
            var result = await _mediator.Send(new GetBlogPostListQuery(filter));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get blog post by ID", OperationId = "GetBlogPost")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetBlogPostQuery(id));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpGet("latest-by-category")]
        [ProducesResponseType(typeof(Result<List<BlogPostItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<BlogPostItem>>), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get latest blog post for each category",
            Description = "Returns the most recently created published blog post for each post category",
            OperationId = "GetLatestBlogPostsByCategory",
            Tags = new[] { "Customer", "Customer_BlogPost" }
        )]
        public async Task<IActionResult> GetLatestByCategory()
        {
            var result = await _mediator.Send(new GetLatestBlogPostsByCategoryQuery());
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
