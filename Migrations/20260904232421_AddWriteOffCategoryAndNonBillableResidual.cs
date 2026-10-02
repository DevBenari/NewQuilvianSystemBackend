using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWriteOffCategoryAndNonBillableResidual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Category",
                schema: "public",
                table: "BilWriteOffCase",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "PATIENT_AR");

            migrationBuilder.AddColumn<decimal>(
                name: "NonBillableResidualAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_BilWriteOffCase_InvoiceId_Category_Status",
                schema: "public",
                table: "BilWriteOffCase",
                columns: new[] { "InvoiceId", "Category", "Status" },
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilWriteOffCase_InvoiceId_Category_Status",
                schema: "public",
                table: "BilWriteOffCase");

            migrationBuilder.DropColumn(
                name: "Category",
                schema: "public",
                table: "BilWriteOffCase");

            migrationBuilder.DropColumn(
                name: "NonBillableResidualAmount",
                schema: "public",
                table: "BilCalculationVersion");
        }
    }
}
