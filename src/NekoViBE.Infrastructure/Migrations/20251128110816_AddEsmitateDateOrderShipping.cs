using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEsmitateDateOrderShipping : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "EstimatedDeliveryDate",
                table: "OrderShippingMethods",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDeliveryDate",
                table: "OrderShippingMethods");
        }
    }
}
