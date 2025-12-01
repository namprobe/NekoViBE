using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveProvinceDistrictWardEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserAddresses_Districts_DistrictGuid",
                table: "UserAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddresses_Provinces_ProvinceGuid",
                table: "UserAddresses");

            migrationBuilder.DropForeignKey(
                name: "FK_UserAddresses_Wards_WardGuid",
                table: "UserAddresses");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "Districts");

            migrationBuilder.DropTable(
                name: "Provinces");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_DistrictGuid",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_ProvinceGuid",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_WardGuid",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "DistrictGuid",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "ProvinceGuid",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "WardGuid",
                table: "UserAddresses");

            migrationBuilder.AddColumn<int>(
                name: "DistrictId",
                table: "UserAddresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DistrictName",
                table: "UserAddresses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProvinceId",
                table: "UserAddresses",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProvinceName",
                table: "UserAddresses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardCode",
                table: "UserAddresses",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "WardName",
                table: "UserAddresses",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_DistrictId",
                table: "UserAddresses",
                column: "DistrictId",
                filter: "DistrictId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_ProvinceId",
                table: "UserAddresses",
                column: "ProvinceId",
                filter: "ProvinceId IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_WardCode",
                table: "UserAddresses",
                column: "WardCode",
                filter: "WardCode IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_DistrictId",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_ProvinceId",
                table: "UserAddresses");

            migrationBuilder.DropIndex(
                name: "IX_UserAddresses_WardCode",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "DistrictId",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "DistrictName",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "ProvinceId",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "ProvinceName",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "WardCode",
                table: "UserAddresses");

            migrationBuilder.DropColumn(
                name: "WardName",
                table: "UserAddresses");

            migrationBuilder.AddColumn<Guid>(
                name: "DistrictGuid",
                table: "UserAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProvinceGuid",
                table: "UserAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "WardGuid",
                table: "UserAddresses",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Provinces",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CanUpdateCod = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    CountryId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GHNStatus = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false),
                    NameExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    ProvinceName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RegionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Provinces", x => x.Id);
                    table.UniqueConstraint("AK_Provinces_ProvinceId", x => x.ProvinceId);
                });

            migrationBuilder.CreateTable(
                name: "Districts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProvinceId = table.Column<int>(type: "int", nullable: false),
                    CanUpdateCod = table.Column<bool>(type: "bit", nullable: false),
                    Code = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    DistrictName = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    GHNStatus = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsEnable = table.Column<bool>(type: "bit", nullable: false),
                    NameExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupportType = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Districts", x => x.Id);
                    table.UniqueConstraint("AK_Districts_DistrictId", x => x.DistrictId);
                    table.ForeignKey(
                        name: "FK_Districts_Provinces_ProvinceId",
                        column: x => x.ProvinceId,
                        principalTable: "Provinces",
                        principalColumn: "ProvinceId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DistrictId = table.Column<int>(type: "int", nullable: false),
                    CanUpdateCod = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime", nullable: true, defaultValueSql: "(getdate())"),
                    CreatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeletedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    DeletedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    GHNStatus = table.Column<int>(type: "int", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    NameExtension = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    SupportType = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime", nullable: true),
                    UpdatedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    WardCode = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    WardName = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Wards_Districts_DistrictId",
                        column: x => x.DistrictId,
                        principalTable: "Districts",
                        principalColumn: "DistrictId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_DistrictGuid",
                table: "UserAddresses",
                column: "DistrictGuid",
                filter: "DistrictGuid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_ProvinceGuid",
                table: "UserAddresses",
                column: "ProvinceGuid",
                filter: "ProvinceGuid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserAddresses_WardGuid",
                table: "UserAddresses",
                column: "WardGuid",
                filter: "WardGuid IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictId",
                table: "Districts",
                column: "DistrictId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Districts_DistrictName",
                table: "Districts",
                column: "DistrictName");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_GHNStatus",
                table: "Districts",
                column: "GHNStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Districts_ProvinceId",
                table: "Districts",
                column: "ProvinceId");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Code",
                table: "Provinces",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_ProvinceId",
                table: "Provinces",
                column: "ProvinceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_ProvinceName",
                table: "Provinces",
                column: "ProvinceName");

            migrationBuilder.CreateIndex(
                name: "IX_Provinces_Status",
                table: "Provinces",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_DistrictId",
                table: "Wards",
                column: "DistrictId");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_GHNStatus",
                table: "Wards",
                column: "GHNStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Wards_WardCode",
                table: "Wards",
                column: "WardCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Wards_WardName",
                table: "Wards",
                column: "WardName");

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddresses_Districts_DistrictGuid",
                table: "UserAddresses",
                column: "DistrictGuid",
                principalTable: "Districts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddresses_Provinces_ProvinceGuid",
                table: "UserAddresses",
                column: "ProvinceGuid",
                principalTable: "Provinces",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserAddresses_Wards_WardGuid",
                table: "UserAddresses",
                column: "WardGuid",
                principalTable: "Wards",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
