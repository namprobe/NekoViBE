using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserBadge.Command.EquipBadge;
using NekoViBE.Application.Features.UserBadge.Command.ProcessBadgeEligibility;
using NekoViBE.Application.Features.UserBadge.Queries.GetUserBadgeWallet;
using Swashbuckle.AspNetCore.Annotations;

namespace NekoViBE.API.Controllers.Customer
{
    [ApiController]
    [Route("api/customer/badges")]
    [ApiExplorerSettings(GroupName = "v1")]
    [Configurations.Tags("Customer", "Badge")]
    [SwaggerTag("This API is used for customer badge management")]
    public class BadgesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<BadgesController> _logger;

        public BadgesController(IMediator mediator, ILogger<BadgesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Get user's badge wallet
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Get user's badge wallet",
            Description = "Retrieves all badges owned by the current user. Use ?filter=all to include locked badges with progress",
            OperationId = "GetUserBadgeWallet",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> GetMyBadges([FromQuery] string? filter = "unlocked", CancellationToken cancellationToken = default)
        {
            var query = new GetUserBadgeWalletQuery(null, filter);
            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        /// <summary>
        /// Get specific user's badge wallet (for profile viewing)
        /// </summary>
        [HttpGet("{userId}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [SwaggerOperation(
            Summary = "Get user's public badge wallet",
            Description = "Retrieves badges for a specific user (public view, only unlocked badges)",
            OperationId = "GetUserBadgeWalletById",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> GetUserBadges(Guid userId, CancellationToken cancellationToken = default)
        {
            var query = new GetUserBadgeWalletQuery(userId, "unlocked");
            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        /// <summary>
        /// Equip a badge (display on profile)
        /// </summary>
        [HttpPatch("{badgeId}/equip")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Equip a badge",
            Description = "Sets a badge as the active/equipped badge for the current user. Unequips all other badges",
            OperationId = "EquipBadge",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> EquipBadge(Guid badgeId, CancellationToken cancellationToken = default)
        {
            var command = new EquipBadgeCommand(badgeId);
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        /// <summary>
        /// Process badge eligibility for current user
        /// </summary>
        [HttpPost("process")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Process badge eligibility",
            Description = "Checks if the current user qualifies for any new badges and awards them automatically. Returns list of newly awarded badges",
            OperationId = "ProcessBadgeEligibility",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> ProcessBadgeEligibility(CancellationToken cancellationToken = default)
        {
            var command = new ProcessBadgeEligibilityCommand(null);
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        /// <summary>
        /// Process badge eligibility for specific user (Admin/Staff only)
        /// </summary>
        [HttpPost("process/{userId}")]
        [AuthorizeRoles("Admin", "Staff")]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status403Forbidden)]
        [SwaggerOperation(
            Summary = "Process badge eligibility for user",
            Description = "Admin/Staff endpoint to process badge eligibility for a specific user",
            OperationId = "ProcessBadgeEligibilityForUser",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> ProcessBadgeEligibilityForUser(Guid userId, CancellationToken cancellationToken = default)
        {
            var command = new ProcessBadgeEligibilityCommand(userId);
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
