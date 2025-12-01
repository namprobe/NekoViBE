using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBadgeCouponLinking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsBadgeCoupon",
                table: "Coupons",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "LinkedCouponId",
                table: "Badges",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Badges_LinkedCouponId",
                table: "Badges",
                column: "LinkedCouponId",
                unique: true,
                filter: "[LinkedCouponId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Badges_Coupons_LinkedCouponId",
                table: "Badges",
                column: "LinkedCouponId",
                principalTable: "Coupons",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Badges_Coupons_LinkedCouponId",
                table: "Badges");

            migrationBuilder.DropIndex(
                name: "IX_Badges_LinkedCouponId",
                table: "Badges");

            migrationBuilder.DropColumn(
                name: "IsBadgeCoupon",
                table: "Coupons");

            migrationBuilder.DropColumn(
                name: "LinkedCouponId",
                table: "Badges");
        }
    }
}
