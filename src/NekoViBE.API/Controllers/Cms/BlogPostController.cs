// File: API/Controllers/Cms/BlogPostController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.BlogPost;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.BlogPost.Commands.CreateBlogPost;
using NekoViBE.Application.Features.BlogPost.Commands.DeleteBlogPost;
using NekoViBE.Application.Features.BlogPost.Commands.UpdateBlogPost;
using NekoViBE.Application.Features.BlogPost.Queries.GetBlogPost;
using NekoViBE.Application.Features.BlogPost.Queries.GetBlogPostList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Cms
{
    [ApiController]
    [Route("api/cms/blog-posts")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("CMS", "CMS_BlogPost")]
    [SwaggerTag("This API is used for Blog Post management in CMS")]
    public class BlogPostController : ControllerBase
    {
        private readonly IMediator _mediator;

        public BlogPostController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<BlogPostItem>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Get blog posts with filter", OperationId = "GetBlogPostList")]
        public async Task<IActionResult> GetList([FromQuery] BlogPostFilter filter)
        {
            var result = await _mediator.Send(new GetBlogPostListQuery(filter));
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Get blog post by ID", OperationId = "GetBlogPost")]
        public async Task<IActionResult> Get(Guid id)
        {
            var result = await _mediator.Send(new GetBlogPostQuery(id));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }

        [HttpPost]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Create new blog post", OperationId = "CreateBlogPost")]
        public async Task<IActionResult> Create([FromForm] BlogPostRequest request)
        {
            var result = await _mediator.Send(new CreateBlogPostCommand(request));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result<BlogPostResponse>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Update blog post", OperationId = "UpdateBlogPost")]
        public async Task<IActionResult> Update(Guid id, [FromForm] BlogPostRequest request)
        {
            var result = await _mediator.Send(new UpdateBlogPostCommand(id, request));
            return result.IsSuccess ? Ok(result) : BadRequest(result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Delete blog post", OperationId = "DeleteBlogPost")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var result = await _mediator.Send(new DeleteBlogPostCommand(id));
            return result.IsSuccess ? Ok(result) : NotFound(result);
        }
    }
}