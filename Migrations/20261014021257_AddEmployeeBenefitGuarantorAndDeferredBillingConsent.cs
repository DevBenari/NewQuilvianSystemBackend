using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeeBenefitGuarantorAndDeferredBillingConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmployeeBenefitGuarantor",
                schema: "public",
                table: "MstCompanyGuarantor",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "BenefitRelationship",
                schema: "public",
                table: "FinReceivable",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeferredConsentGivenBy",
                schema: "public",
                table: "BilArHandoff",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DeferredPatientAmount",
                schema: "public",
                table: "BilArHandoff",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "UX_MstCompanyGuarantor_EmployeeBenefit",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "IsEmployeeBenefitGuarantor",
                unique: true,
                filter: "\"IsEmployeeBenefitGuarantor\" = true AND \"IsDelete\" = false");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_Consent",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"DeferredPatientAmount\" = 0 OR \"BenefitRelationship\" = 'SELF' OR \"DeferredConsentGivenBy\" IS NOT NULL");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_Deferred",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"DeferredPatientAmount\" >= 0 AND \"DeferredPatientAmount\" <= \"Amount\" AND (\"DebtorType\" = 'EMPLOYEE_BENEFIT' OR \"DeferredPatientAmount\" = 0)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_MstCompanyGuarantor_EmployeeBenefit",
                schema: "public",
                table: "MstCompanyGuarantor");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_Consent",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_Deferred",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "IsEmployeeBenefitGuarantor",
                schema: "public",
                table: "MstCompanyGuarantor");

            migrationBuilder.DropColumn(
                name: "DeferredConsentGivenBy",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "DeferredPatientAmount",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.AlterColumn<string>(
                name: "BenefitRelationship",
                schema: "public",
                table: "FinReceivable",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);
        }
    }
}
