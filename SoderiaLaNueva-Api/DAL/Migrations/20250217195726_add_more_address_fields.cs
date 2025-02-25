using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoderiaLaNueva_Api.Migrations
{
    /// <inheritdoc />
    public partial class add_more_address_fields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Address",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Address",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Address",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "CityDistrict",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "County",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Neighbourhood",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Postcode",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Road",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Suburb",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Town",
                table: "Address",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Village",
                table: "Address",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CityDistrict",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "County",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Neighbourhood",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Postcode",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Road",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Suburb",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Town",
                table: "Address");

            migrationBuilder.DropColumn(
                name: "Village",
                table: "Address");

            migrationBuilder.AlterColumn<string>(
                name: "State",
                table: "Address",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Country",
                table: "Address",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "City",
                table: "Address",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }
    }
}
