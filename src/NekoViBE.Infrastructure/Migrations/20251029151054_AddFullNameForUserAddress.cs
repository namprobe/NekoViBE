using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NekoViBE.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFullNameForUserAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "UserAddresses",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FullName",
                table: "UserAddresses");
        }
    }
}
