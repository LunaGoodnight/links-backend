using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LinksService.Migrations
{
    /// <inheritdoc />
    public partial class AddCategoryTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Create Categories table first
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Order = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Order",
                table: "Categories",
                column: "Order");

            // Step 2: Add CategoryId column to Links table
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "Links",
                type: "int",
                nullable: true);

            // Step 3: Migrate existing category strings to Categories table
            migrationBuilder.Sql(@"
                INSERT INTO Categories (Name, `Order`, CreatedAt, UpdatedAt)
                SELECT DISTINCT Category, 0, UTC_TIMESTAMP(), UTC_TIMESTAMP()
                FROM Links
                WHERE Category IS NOT NULL AND Category != '';
            ");

            // Step 4: Update Links to reference the new CategoryId
            migrationBuilder.Sql(@"
                UPDATE Links l
                INNER JOIN Categories c ON c.Name = l.Category
                SET l.CategoryId = c.Id
                WHERE l.Category IS NOT NULL AND l.Category != '';
            ");

            // Step 5: Drop old index and column
            migrationBuilder.DropIndex(
                name: "IX_Links_Category",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Links");

            // Step 6: Create new index and foreign key
            migrationBuilder.CreateIndex(
                name: "IX_Links_CategoryId",
                table: "Links",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Links_Categories_CategoryId",
                table: "Links",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Links_Categories_CategoryId",
                table: "Links");

            // Re-add Category string column
            migrationBuilder.AddColumn<string>(
                name: "Category",
                table: "Links",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            // Migrate data back to string column
            migrationBuilder.Sql(@"
                UPDATE Links l
                INNER JOIN Categories c ON c.Id = l.CategoryId
                SET l.Category = c.Name
                WHERE l.CategoryId IS NOT NULL;
            ");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropIndex(
                name: "IX_Links_CategoryId",
                table: "Links");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "Links");

            migrationBuilder.CreateIndex(
                name: "IX_Links_Category",
                table: "Links",
                column: "Category");
        }
    }
}
