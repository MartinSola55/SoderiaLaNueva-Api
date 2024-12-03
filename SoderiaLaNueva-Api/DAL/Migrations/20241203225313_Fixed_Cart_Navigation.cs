using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoderiaLaNueva_Api.Migrations
{
    /// <inheritdoc />
    public partial class Fixed_Cart_Navigation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentMethod_Cart_CartId",
                table: "PaymentMethod");

            migrationBuilder.DropIndex(
                name: "IX_PaymentMethod_CartId",
                table: "PaymentMethod");

            migrationBuilder.DropColumn(
                name: "CartId",
                table: "PaymentMethod");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CartId",
                table: "PaymentMethod",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentMethod_CartId",
                table: "PaymentMethod",
                column: "CartId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentMethod_Cart_CartId",
                table: "PaymentMethod",
                column: "CartId",
                principalTable: "Cart",
                principalColumn: "Id");
        }
    }
}
