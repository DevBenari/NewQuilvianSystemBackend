using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260827030531_AddRoomChargeAmountToBilCalculationVersion")]
    public partial class AddRoomChargeAmountToBilCalculationVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.AddColumn<decimal>(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion",
                sql: "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"RoomChargeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.DropColumn(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion",
                sql: "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");
        }
    }
}
