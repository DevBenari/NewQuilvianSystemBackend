using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class CheckDiffInterToYas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_KwitansiNumber",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.AddColumn<string>(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilTender",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilTender_KwitansiNumber",
                schema: "public",
                table: "BilTender",
                column: "KwitansiNumber",
                unique: true,
                filter: "\"KwitansiNumber\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilTender_KwitansiNumber",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilTender");

            migrationBuilder.AddColumn<string>(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilInvoice",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_KwitansiNumber",
                schema: "public",
                table: "BilInvoice",
                column: "KwitansiNumber",
                unique: true,
                filter: "\"KwitansiNumber\" IS NOT NULL");
        }
    }
}
