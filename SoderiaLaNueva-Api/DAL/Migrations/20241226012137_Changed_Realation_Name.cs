using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SoderiaLaNueva_Api.Migrations
{
    /// <inheritdoc />
    public partial class Changed_Realation_Name : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Client_Subscription_SubscriptionId",
                table: "Client");

            migrationBuilder.DropIndex(
                name: "IX_Client_SubscriptionId",
                table: "Client");

            migrationBuilder.DropColumn(
                name: "SubscriptionId",
                table: "Client");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SubscriptionId",
                table: "Client",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Client_SubscriptionId",
                table: "Client",
                column: "SubscriptionId");

            migrationBuilder.AddForeignKey(
                name: "FK_Client_Subscription_SubscriptionId",
                table: "Client",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "Id");
        }
    }
}
