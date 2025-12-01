using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NekoViBE.API.Attributes;
using NekoViBE.Application.Common.Extensions;
using NekoViBE.Application.Common.Models;
using NekoViBE.Application.Features.UserBadge.Command.EquipBadge;
using NekoViBE.Application.Features.UserBadge.Command.ProcessBadgeEligibility;
using NekoViBE.Application.Features.UserBadge.Command.SyncBadgeCoupons;
using NekoViBE.Application.Features.UserBadge.Queries.GetUserBadgeWallet;
using NekoViBE.Application.Features.UserBadge.Queries.GetActiveBadgeCoupon;
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

        /// <summary>
        /// Get the coupon associated with the currently equipped badge
        /// </summary>
        /// <remarks>
        /// This endpoint returns the auto-generated coupon for the user's currently equipped badge.
        /// If no badge is equipped or the badge has no linked coupon, returns null.
        /// 
        /// **Important:** This coupon is automatically applied at checkout when the badge is equipped.
        /// Users cannot collect this coupon manually - it's only available when the badge is equipped.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///         "isSuccess": true,
        ///         "data": {
        ///             "id": "123e4567-e89b-12d3-a456-426614174000",
        ///             "code": "SYS_BADGE_GOLD_VIP",
        ///             "description": "Auto-generated coupon for badge: Gold Member",
        ///             "discountType": 0,
        ///             "discountValue": 5.0,
        ///             "maxDiscountCap": null,
        ///             "minOrderAmount": 0,
        ///             "startDate": "2025-01-01T00:00:00Z",
        ///             "endDate": "2035-01-01T00:00:00Z"
        ///         }
        ///     }
        /// </remarks>
        /// <returns>The badge coupon if available, null otherwise</returns>
        /// <response code="200">Coupon retrieved successfully (may be null)</response>
        /// <response code="401">Not authenticated</response>
        /// <response code="500">Server error</response>
        [HttpGet("active-coupon")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status500InternalServerError)]
        [SwaggerOperation(
            Summary = "Get active badge coupon",
            Description = "Retrieves the coupon linked to the user's currently equipped badge. Returns null if no badge is equipped or badge has no coupon. This coupon is automatically applied at checkout.",
            OperationId = "GetActiveBadgeCoupon",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> GetActiveBadgeCoupon(CancellationToken cancellationToken = default)
        {
            var query = new GetActiveBadgeCouponQuery();
            var result = await _mediator.Send(query, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }

        /// <summary>
        /// Sync badge coupons to UserCoupons table for current user
        /// </summary>
        /// <remarks>
        /// This endpoint ensures all badges owned by the user have their linked coupons in the UserCoupons table.
        /// Useful for fixing any missing badge coupons.
        /// 
        /// **Use this if**:
        /// - You have badges but don't see their coupons in "My Coupons"
        /// - After running the badge coupon seed script
        /// - To manually trigger the auto-collection logic
        /// </remarks>
        /// <returns>Sync result with count of coupons added</returns>
        [HttpPost("sync-coupons")]
        [Authorize]
        [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Result), StatusCodes.Status401Unauthorized)]
        [SwaggerOperation(
            Summary = "Sync badge coupons",
            Description = "Ensures all your badges have their linked coupons in your coupon collection. This fixes any missing badge coupons.",
            OperationId = "SyncBadgeCoupons",
            Tags = new[] { "Customer", "Badge" }
        )]
        public async Task<IActionResult> SyncBadgeCoupons(CancellationToken cancellationToken = default)
        {
            var command = new SyncBadgeCouponsCommand { UserId = null }; // null = current user
            var result = await _mediator.Send(command, cancellationToken);
            return StatusCode(result.GetHttpStatusCode(), result);
        }
    }
}
