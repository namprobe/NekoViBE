using MediatR;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.DTOs.UserHomeImage;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserHomeImage.Commands.CreateUserHomeImage;
using NekoViBE.Application.Features.UserHomeImage.Commands.DeleteUserHomeImage;
using NekoViBE.Application.Features.UserHomeImage.Commands.SaveUserHomeImages;
using NekoViBE.Application.Features.UserHomeImage.Commands.UpdateUserHomeImage;
using NekoViBE.Application.Features.UserHomeImage.Queries.GetCurrentUserHomeImages;
using NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImage;
using NekoViBE.Application.Features.UserHomeImage.Queries.GetUserHomeImageList;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    // API/Controllers/Customer/UserHomeImagesController.cs
    [ApiController]
    [Route("api/customer/user-home-images")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Customer_UserHomeImages")]
    [SwaggerTag("This API is used for User Home Image management in Customer")]
    public class UserHomeImagesController : ControllerBase
    {
        private readonly IMediator _mediator;
        public UserHomeImagesController(IMediator mediator) => _mediator = mediator;

        [HttpGet]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(PaginationResult<UserHomeImageItem>), StatusCodes.Status200OK)]
        [SwaggerOperation(Summary = "Lấy danh sách ảnh trang chủ của user", OperationId = "GetUserHomeImageList")]
        public async Task<IActionResult> GetList([FromQuery] UserHomeImageFilter filter)
        {
            var query = new GetUserHomeImageListQuery(filter);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("{id}")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<UserHomeImageResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<UserHomeImageResponse>), StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Lấy chi tiết một lựa chọn ảnh trang chủ", OperationId = "GetUserHomeImage")]
        public async Task<IActionResult> Get(Guid id)
        {
            var query = new GetUserHomeImageQuery(id);
            var result = await _mediator.Send(query);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Thêm ảnh vào trang chủ cá nhân (tối đa 3)", OperationId = "CreateUserHomeImage")]
        public async Task<IActionResult> Create([FromForm] UserHomeImageRequest request)
        {
            var command = new CreateUserHomeImageCommand(request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPut("{id}")]
        [AuthorizeRoles]
        [SwaggerOperation(Summary = "Cập nhật vị trí ảnh trang chủ", OperationId = "UpdateUserHomeImage")]
        public async Task<IActionResult> Update(Guid id, [FromForm] UserHomeImageRequest request)
        {
            var command = new UpdateUserHomeImageCommand(id, request);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpDelete("{id}")]
        [AuthorizeRoles]
        [SwaggerOperation(Summary = "Xóa ảnh khỏi trang chủ cá nhân", OperationId = "DeleteUserHomeImage")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var command = new DeleteUserHomeImageCommand(id);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpPost("save")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Lưu danh sách ảnh trang chủ (tạo mới/cập nhật/xóa)", OperationId = "SaveUserHomeImages")]
        public async Task<IActionResult> Save([FromBody] List<UserHomeImageSaveRequest> requests)
        {
            var command = new SaveUserHomeImagesCommand(requests);
            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result) : StatusCode(result.GetHttpStatusCode(), result);
        }

        [HttpGet("me")]
        [AuthorizeRoles]
        [ProducesResponseType(typeof(Result<List<UserHomeImageItem>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result<List<UserHomeImageItem>>), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Lấy danh sách ảnh trang chủ của user hiện tại",
            Description = "Không cần truyền userId, tự động lấy từ token",
            OperationId = "GetCurrentUserHomeImages")]
        public async Task<IActionResult> GetMyHomeImages()
        {
            var query = new GetCurrentUserHomeImagesQuery();
            var result = await _mediator.Send(query);
            return result.IsSuccess
                ? Ok(result)
                : StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
