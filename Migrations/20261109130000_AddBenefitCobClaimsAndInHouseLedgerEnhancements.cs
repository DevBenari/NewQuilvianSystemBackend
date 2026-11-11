using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitCobClaimsAndInHouseLedgerEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Kolom tambahan pada TrxBenefitClaim (BE-HRD-188, BE-HRD-189)
            migrationBuilder.AddColumn<string>(
                name: "SepBpjsNumber",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "HospitalName",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalHospitalBill",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrimaryPayerCoveredAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "SecondaryClaimAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<decimal>(
                name: "OutOfPocketExcessAmount",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.00m);

            migrationBuilder.AddColumn<Guid>(
                name: "DependentId",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "SettledAt",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SettlementReference",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "TrxBenefitClaim",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_benefit_claim_sep",
                schema: "public",
                table: "TrxBenefitClaim",
                column: "SepBpjsNumber");

            // 2. Kolom tambahan pada HrdBenefitBalanceMovement (BE-HRD-186, BE-HRD-187)
            migrationBuilder.AddColumn<Guid>(
                name: "WorkforceProfileId",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "BenefitPlanId",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "HrdBenefitBalanceMovement",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_benefit_claim_sep",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SepBpjsNumber",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "HospitalName",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "TotalHospitalBill",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "PrimaryPayerCoveredAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SecondaryClaimAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "OutOfPocketExcessAmount",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "DependentId",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SettledAt",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "SettlementReference",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "TrxBenefitClaim");

            migrationBuilder.DropColumn(
                name: "WorkforceProfileId",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "BenefitPlanId",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                schema: "public",
                table: "HrdBenefitBalanceMovement");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "HrdBenefitBalanceMovement");
        }
    }
}
