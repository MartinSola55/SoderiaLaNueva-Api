using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoderiaLaNueva_Api.Migrations
{
    /// <inheritdoc />
    public partial class change_address_nameNumber_to_houseNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameNumber",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "HouseNumber",
                table: "Address",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HouseNumber",
                table: "Address");

            migrationBuilder.AddColumn<string>(
                name: "NameNumber",
                table: "Address",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}
