using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ShippingHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ShippingHistories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderShippingMethodId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrackingNumber = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    StatusName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    StatusDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EventTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AdditionalData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CallerIpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ShippingHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ShippingHistories_OrderShippingMethods_OrderShippingMethodId",
                        column: x => x.OrderShippingMethodId,
                        principalTable: "OrderShippingMethods",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ShippingHistories_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingHistories_EventTime",
                table: "ShippingHistories",
                column: "EventTime");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingHistories_OrderId",
                table: "ShippingHistories",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingHistories_OrderId_EventTime",
                table: "ShippingHistories",
                columns: new[] { "OrderId", "EventTime" });

            migrationBuilder.CreateIndex(
                name: "IX_ShippingHistories_OrderShippingMethodId",
                table: "ShippingHistories",
                column: "OrderShippingMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_ShippingHistories_TrackingNumber",
                table: "ShippingHistories",
                column: "TrackingNumber",
                filter: "TrackingNumber IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ShippingHistories");
        }
    }
}
