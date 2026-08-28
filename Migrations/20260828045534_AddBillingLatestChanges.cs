using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBillingLatestChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.AddColumn<string>(
                name: "CashierReferenceNote",
                schema: "public",
                table: "BilTender",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                schema: "public",
                table: "BilSettlement",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion");

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

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilCalculationVersion_Amounts",
                schema: "public",
                table: "BilCalculationVersion",
                sql: "\"GrossAmount\" >= 0 AND \"AdministrationFeeAmount\" >= 0 AND \"ItemDiscount\" >= 0 AND \"TotalDiscount\" >= 0 AND \"TaxAmount\" >= 0 AND \"PatientAmount\" >= 0 AND \"PrimaryAmount\" >= 0 AND \"ExcessAmount\" >= 0 AND \"UnresolvedCoverageAmount\" >= 0");

            migrationBuilder.DropColumn(
                name: "RoomChargeAmount",
                schema: "public",
                table: "BilCalculationVersion");

            migrationBuilder.DropColumn(
                name: "Note",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropColumn(
                name: "CashierReferenceNote",
                schema: "public",
                table: "BilTender");

            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_KwitansiNumber",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "KwitansiNumber",
                schema: "public",
                table: "BilInvoice");
        }
    }
}