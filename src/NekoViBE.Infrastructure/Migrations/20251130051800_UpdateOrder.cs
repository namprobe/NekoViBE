using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TotalAmount",
                table: "Orders",
                newName: "TotalProductAmount");

            migrationBuilder.RenameColumn(
                name: "ShippingAmount",
                table: "Orders",
                newName: "SubtotalOriginal");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "Orders",
                newName: "SubtotalAfterProductDiscount");

            migrationBuilder.RenameColumn(
                name: "UnitPrice",
                table: "OrderItems",
                newName: "UnitPriceOriginal");

            migrationBuilder.RenameColumn(
                name: "DiscountAmount",
                table: "OrderItems",
                newName: "UnitPriceAfterDiscount");

            migrationBuilder.AddColumn<decimal>(
                name: "CodFee",
                table: "OrderShippingMethods",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "FreeshippingNote",
                table: "OrderShippingMethods",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFee",
                table: "OrderShippingMethods",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsFreeshipping",
                table: "OrderShippingMethods",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "ProviderName",
                table: "OrderShippingMethods",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "OrderShippingMethods",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeActual",
                table: "OrderShippingMethods",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeOriginal",
                table: "OrderShippingMethods",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "CouponDiscountAmount",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProductDiscountAmount",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingDiscountAmount",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeActual",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ShippingFeeOriginal",
                table: "Orders",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "LineTotal",
                table: "OrderItems",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitDiscountAmount",
                table: "OrderItems",
                type: "decimal(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxDiscountCap",
                table: "Coupons",
                type: "decimal(10,2)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrderShippingMethods_ProviderName",
                table: "OrderShippingMethods",
                column: "ProviderName",
                filter: "ProviderName IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_OrderShippingMethods_ProviderName",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "CodFee",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "FreeshippingNote",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "InsuranceFee",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "IsFreeshipping",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "ProviderName",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "ShippingDiscountAmount",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "ShippingFeeActual",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "ShippingFeeOriginal",
                table: "OrderShippingMethods");

            migrationBuilder.DropColumn(
                name: "CouponDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ProductDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingDiscountAmount",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFeeActual",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "ShippingFeeOriginal",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "LineTotal",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "UnitDiscountAmount",
                table: "OrderItems");

            migrationBuilder.DropColumn(
                name: "MaxDiscountCap",
                table: "Coupons");

            migrationBuilder.RenameColumn(
                name: "TotalProductAmount",
                table: "Orders",
                newName: "TotalAmount");

            migrationBuilder.RenameColumn(
                name: "SubtotalOriginal",
                table: "Orders",
                newName: "ShippingAmount");

            migrationBuilder.RenameColumn(
                name: "SubtotalAfterProductDiscount",
                table: "Orders",
                newName: "DiscountAmount");

            migrationBuilder.RenameColumn(
                name: "UnitPriceOriginal",
                table: "OrderItems",
                newName: "UnitPrice");

            migrationBuilder.RenameColumn(
                name: "UnitPriceAfterDiscount",
                table: "OrderItems",
                newName: "DiscountAmount");
        }
    }
}
