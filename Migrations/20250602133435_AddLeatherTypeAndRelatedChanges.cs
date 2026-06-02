using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace LTMS.Migrations
{
    /// <inheritdoc />
    public partial class AddLeatherTypeAndRelatedChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Category",
                table: "Inventories",
                newName: "UnitOfMeasurement");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceId",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LeatherTypeId",
                table: "Inventories",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LeatherTypeId",
                table: "Demands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Quantity",
                table: "Demands",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "UnitOfMeasurement",
                table: "Demands",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "LeatherTypes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeatherTypes", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "LeatherTypes",
                columns: new[] { "Id", "Category", "Description", "Name" },
                values: new object[,]
                {
                    { -15, "Exotic", "Durable, unique grain pattern with a pearly finish", "Stingray Leather" },
                    { -14, "Exotic", "Smaller scales, often used for wallets and belts", "Lizard Leather" },
                    { -13, "Exotic", "Unique patterns, used in fashion accessories", "Snake (Python) Leather" },
                    { -12, "Exotic", "Distinctive scales, high-end", "Crocodile/Alligator Leather" },
                    { -11, "Exotic", "Characteristic quill pattern, luxurious", "Ostrich Leather" },
                    { -10, "Specialty", "Tanned with chromium salts, softer and more pliable", "Chrome-Tanned Leather" },
                    { -9, "Specialty", "Naturally tanned using plant-based tannins", "Vegetable-Tanned Leather" },
                    { -8, "Specialty", "Treated with waxes and oils for a distressed look", "Pull-Up Leather" },
                    { -7, "Specialty", "High-gloss finish, coated with lacquer or plastic", "Patent Leather" },
                    { -6, "Specialty", "Sanded on the grain side, velvety texture", "Nubuck Leather" },
                    { -5, "Specialty", "Made from the underside of the hide, soft and napped finish", "Suede Leather" },
                    { -4, "Common", "Lower layers of hide, often used for suede or coated leather", "Split Leather" },
                    { -3, "Common", "Treated to remove imperfections, embossed texture", "Corrected-Grain Leather" },
                    { -2, "Common", "Sanded and finished, more uniform than full-grain", "Top-Grain Leather" },
                    { -1, "Common", "Highest quality, natural grain surface", "Full-Grain Leather" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Inventories_LeatherTypeId",
                table: "Inventories",
                column: "LeatherTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Demands_LeatherTypeId",
                table: "Demands",
                column: "LeatherTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Demands_LeatherTypes_LeatherTypeId",
                table: "Demands",
                column: "LeatherTypeId",
                principalTable: "LeatherTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Inventories_LeatherTypes_LeatherTypeId",
                table: "Inventories",
                column: "LeatherTypeId",
                principalTable: "LeatherTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Demands_LeatherTypes_LeatherTypeId",
                table: "Demands");

            migrationBuilder.DropForeignKey(
                name: "FK_Inventories_LeatherTypes_LeatherTypeId",
                table: "Inventories");

            migrationBuilder.DropTable(
                name: "LeatherTypes");

            migrationBuilder.DropIndex(
                name: "IX_Inventories_LeatherTypeId",
                table: "Inventories");

            migrationBuilder.DropIndex(
                name: "IX_Demands_LeatherTypeId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "ReferenceId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "LeatherTypeId",
                table: "Inventories");

            migrationBuilder.DropColumn(
                name: "LeatherTypeId",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "Quantity",
                table: "Demands");

            migrationBuilder.DropColumn(
                name: "UnitOfMeasurement",
                table: "Demands");

            migrationBuilder.RenameColumn(
                name: "UnitOfMeasurement",
                table: "Inventories",
                newName: "Category");
        }
    }
}
