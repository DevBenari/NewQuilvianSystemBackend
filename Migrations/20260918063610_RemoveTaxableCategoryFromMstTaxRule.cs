using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveTaxableCategoryFromMstTaxRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstTaxRule_TaxableCategory_EffectiveFrom_EffectiveTo_IsActi~",
                schema: "public",
                table: "MstTaxRule");

            migrationBuilder.DropColumn(
                name: "TaxableCategory",
                schema: "public",
                table: "MstTaxRule");

            migrationBuilder.CreateIndex(
                name: "IX_MstTaxRule_EffectiveFrom_EffectiveTo_IsActive_IsDelete",
                schema: "public",
                table: "MstTaxRule",
                columns: new[] { "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstTaxRule_EffectiveFrom_EffectiveTo_IsActive_IsDelete",
                schema: "public",
                table: "MstTaxRule");

            migrationBuilder.AddColumn<string>(
                name: "TaxableCategory",
                schema: "public",
                table: "MstTaxRule",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_MstTaxRule_TaxableCategory_EffectiveFrom_EffectiveTo_IsActi~",
                schema: "public",
                table: "MstTaxRule",
                columns: new[] { "TaxableCategory", "EffectiveFrom", "EffectiveTo", "IsActive", "IsDelete" });
        }
    }
}

