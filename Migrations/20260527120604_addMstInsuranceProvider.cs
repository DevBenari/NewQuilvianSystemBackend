using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addMstInsuranceProvider : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstInsuranceProvider",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProviderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InsuranceProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InsuranceGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "PrivateInsurance"),
                    ClaimMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Cashless"),
                    ExternalProviderCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsUsingInsuranceTariffBook = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsUsingHospitalTariff = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedEligibilityCheck = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedReferralLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApprovalForProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedApprovalForDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoverageLimitedByPlan = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    PicName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PicPhoneNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PicWhatsAppNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    PicEmail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    OfficeAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    LogoPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    BillingInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClaimInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MstInsuranceProvider", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_ClaimMethod",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "ClaimMethod");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_ExternalProviderCode",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "ExternalProviderCode",
                filter: "\"ExternalProviderCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_InsuranceProviderCode",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "InsuranceProviderCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_InsuranceProviderName",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "InsuranceProviderName");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_IntegrationCode",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_IsActive_IsDelete",
                schema: "public",
                table: "MstInsuranceProvider",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_ProviderType",
                schema: "public",
                table: "MstInsuranceProvider",
                column: "ProviderType");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceProvider_ProviderType_ClaimMethod_IsActive_IsDe~",
                schema: "public",
                table: "MstInsuranceProvider",
                columns: new[] { "ProviderType", "ClaimMethod", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstInsuranceProvider",
                schema: "public");
        }
    }
}
