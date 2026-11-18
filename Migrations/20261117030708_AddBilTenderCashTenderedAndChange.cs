using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBilTenderCashTenderedAndChange : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CashChangeAmount",
                schema: "public",
                table: "BilTender",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CashTenderedAmount",
                schema: "public",
                table: "BilTender",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilTender_CashBreakdown",
                schema: "public",
                table: "BilTender",
                sql: "(\"CashTenderedAmount\" IS NULL AND \"CashChangeAmount\" IS NULL) OR (\"CashTenderedAmount\" IS NOT NULL AND \"CashChangeAmount\" IS NOT NULL AND \"CashChangeAmount\" >= 0 AND \"CashTenderedAmount\" - \"Amount\" = \"CashChangeAmount\")");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilTender_CashBreakdown",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "CashChangeAmount",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropColumn(
                name: "CashTenderedAmount",
                schema: "public",
                table: "BilTender");
        }
    }
}
