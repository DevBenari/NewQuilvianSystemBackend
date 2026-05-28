using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addInsuranceAndCompanyGuarantor : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstCompanyGuarantor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    CompanyGuarantorName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompanyGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GuarantorType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Corporate"),
                    BillingMethod = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Invoice"),
                    ExternalCompanyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ContractNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    ContractEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsUsingCompanyTariffBook = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsUsingHospitalTariff = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedEmployeeVerification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedApprovalForProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedApprovalForDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoverageLimitedByEmployeeGrade = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreditLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CurrentOutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    PaymentDueDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
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
                    table.PrimaryKey("PK_MstCompanyGuarantor", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientInsurance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    PolicyNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CardNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MemberNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PlanName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ClassName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    HolderRelationship = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastEligibilityCheckAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEligibilityReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EligibilityNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AnnualLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RemainingLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CoPaymentPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedReferralLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CardImagePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstPatientInsurance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientInsurance_MstInsuranceProvider_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatientInsurance_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstPatientCompanyGuarantor",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompanyGuarantorId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmployeeNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    EmployeeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    DepartmentName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PositionName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    GradeLevel = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ClassName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsEligible = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastEligibilityCheckAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastEligibilityReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    EligibilityNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    AnnualLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    RemainingLimitAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CoPaymentPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedEmployeeVerification = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    GuaranteeDocumentPath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstPatientCompanyGuarantor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPatientCompanyGuarantor_MstCompanyGuarantor_CompanyGuara~",
                        column: x => x.CompanyGuarantorId,
                        principalSchema: "public",
                        principalTable: "MstCompanyGuarantor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPatientCompanyGuarantor_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_BillingMethod",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "BillingMethod");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_CompanyGroupName",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "CompanyGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_CompanyGuarantorCode",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "CompanyGuarantorCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_CompanyGuarantorName",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "CompanyGuarantorName");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_ContractStartDate_ContractEndDate_IsAct~",
                schema: "public",
                table: "MstCompanyGuarantor",
                columns: new[] { "ContractStartDate", "ContractEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_ExternalCompanyCode",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "ExternalCompanyCode",
                filter: "\"ExternalCompanyCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_GuarantorType",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "GuarantorType");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_GuarantorType_BillingMethod_IsActive_Is~",
                schema: "public",
                table: "MstCompanyGuarantor",
                columns: new[] { "GuarantorType", "BillingMethod", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_IntegrationCode",
                schema: "public",
                table: "MstCompanyGuarantor",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompanyGuarantor_IsActive_IsDelete",
                schema: "public",
                table: "MstCompanyGuarantor",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_BenefitPlanCode_ClassName_IsActi~",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "BenefitPlanCode", "ClassName", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_CompanyGuarantorId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "CompanyGuarantorId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_CompanyGuarantorId_IsEligible_Is~",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "CompanyGuarantorId", "IsEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_EffectiveStartDate_EffectiveEndD~",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_EmployeeNumber",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "EmployeeNumber",
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_PatientId",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_PatientId_CompanyGuarantorId_Emp~",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "PatientId", "CompanyGuarantorId", "EmployeeNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_PatientId_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "PatientId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientCompanyGuarantor_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientCompanyGuarantor",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_CardNumber",
                schema: "public",
                table: "MstPatientInsurance",
                column: "CardNumber",
                filter: "\"CardNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_EffectiveStartDate_EffectiveEndDate_IsA~",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_InsuranceProviderId",
                schema: "public",
                table: "MstPatientInsurance",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_InsuranceProviderId_IsEligible_IsActive~",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "InsuranceProviderId", "IsEligible", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_MemberNumber",
                schema: "public",
                table: "MstPatientInsurance",
                column: "MemberNumber",
                filter: "\"MemberNumber\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PatientId",
                schema: "public",
                table: "MstPatientInsurance",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PatientId_InsuranceProviderId_PolicyNum~",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "PatientId", "InsuranceProviderId", "PolicyNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PatientId_IsActive_IsDelete",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "PatientId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientInsurance",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientInsurance_PolicyNumber",
                schema: "public",
                table: "MstPatientInsurance",
                column: "PolicyNumber",
                filter: "\"IsDelete\" = false");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPatientCompanyGuarantor",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstPatientInsurance",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCompanyGuarantor",
                schema: "public");
        }
    }
}
