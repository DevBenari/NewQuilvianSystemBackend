using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBenefitInsuranceEnrollmentAndDependentEnhancements : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Kolom baru pada TrxEmployeeInsuranceEnrollment (BE-HRD-182)
            migrationBuilder.AddColumn<string>(
                name: "BpjsKesehatanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PrivateInsurancePolicyNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProbationEvaluationId",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TerminationDate",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TerminationReason",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "idx_insurance_enrollment_bpjs_kes",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                column: "BpjsKesehatanNumber");

            migrationBuilder.CreateIndex(
                name: "idx_insurance_enrollment_bpjs_tk",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment",
                column: "BpjsKetenagakerjaanNumber");

            // 2. Kolom baru pada TrxEmployeeBenefitDependent (BE-HRD-181, BE-HRD-183, BE-HRD-184)
            migrationBuilder.AddColumn<string>(
                name: "NationalIdNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FamilyCardNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(16)",
                maxLength: 16,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActiveStudent",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "StudentVerificationLetterPath",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StudentVerificationExpiryDate",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                defaultValue: "Draft");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "TrxEmployeeBenefitDependent",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "idx_insurance_enrollment_bpjs_kes",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropIndex(
                name: "idx_insurance_enrollment_bpjs_tk",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "BpjsKesehatanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "BpjsKetenagakerjaanNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "PrivateInsurancePolicyNumber",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "ProbationEvaluationId",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "TerminationDate",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "TerminationReason",
                schema: "public",
                table: "TrxEmployeeInsuranceEnrollment");

            migrationBuilder.DropColumn(
                name: "NationalIdNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "FamilyCardNumber",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "IsActiveStudent",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "StudentVerificationLetterPath",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "StudentVerificationExpiryDate",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "TrxEmployeeBenefitDependent");
        }
    }
}
