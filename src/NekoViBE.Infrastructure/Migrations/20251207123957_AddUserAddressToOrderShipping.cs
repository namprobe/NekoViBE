using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserAddressToOrderShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserAddressId",
                table: "OrderShippingMethods",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderShippingMethods_UserAddressId",
                table: "OrderShippingMethods",
                column: "UserAddressId",
                filter: "UserAddressId IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderShippingMethods_UserAddresses_UserAddressId",
                table: "OrderShippingMethods",
                column: "UserAddressId",
                principalTable: "UserAddresses",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrderShippingMethods_UserAddresses_UserAddressId",
                table: "OrderShippingMethods");

            migrationBuilder.DropIndex(
                name: "IX_OrderShippingMethods_UserAddressId",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "UserAddressId",
                table: "OrderShippingMethods");
        }
    }
}
