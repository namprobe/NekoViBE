using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintUserCoupon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_UserCoupons_UserId_CouponId_Active",
                table: "UserCoupons",
                columns: new[] { "UserId", "CouponId" },
                unique: true,
                filter: "UserId IS NOT NULL AND Status = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_UserCoupons_UserId_CouponId_Active",
                table: "UserCoupons");
        }
    }
}
