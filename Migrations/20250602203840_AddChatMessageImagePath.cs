using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LTMS.Migrations
{
    /// <inheritdoc />
    public partial class AddChatMessageImagePath : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Bids_SellerId",
                table: "Bids");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Inventories",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "ChatMessages",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImagePath",
                table: "Bids",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders",
                column: "OrderDate");

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_ProductName",
                table: "Inventories",
                column: "ProductName");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_Title",
                table: "Demands",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_Amount",
                table: "Bids",
                column: "Amount");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SellerId_DemandId",
                table: "Bids",
                columns: new[] { "SellerId", "DemandId" },
                unique: true,
                filter: "[SellerId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Orders_OrderDate",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_ProductName",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Demands_Title",
                table: "Demands");

            migrationBuilder.DropIndex(
                name: "IX_Bids_Amount",
                table: "Bids");

            migrationBuilder.DropIndex(
                name: "IX_Bids_SellerId_DemandId",
                table: "Bids");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ImagePath",
                table: "Bids");

            migrationBuilder.AlterColumn<string>(
                name: "ProductName",
                table: "Inventories",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_Bids_SellerId",
                table: "Bids",
                column: "SellerId");
        }
    }
}
