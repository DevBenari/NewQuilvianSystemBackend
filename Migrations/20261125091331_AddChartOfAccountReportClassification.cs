using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddChartOfAccountReportClassification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentAssetCategory",
                schema: "public",
                table: "AccChartOfAccount",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReportClassification",
                schema: "public",
                table: "AccChartOfAccount",
                type: "integer",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentAssetCategory",
                schema: "public",
                table: "AccChartOfAccount");

            migrationBuilder.DropColumn(
                name: "ReportClassification",
                schema: "public",
                table: "AccChartOfAccount");
        }
    }
}
