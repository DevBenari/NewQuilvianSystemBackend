using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addMasterMstProcedureMstDrugCategoryMstDrugMstInsuranceCoverageRule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstDrugCategory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCategoryCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugCategoryName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    DrugGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DrugCategoryType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    IsAntibiotic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNarcotic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPsychotropic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighAlert = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsChronicDiseaseDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVaccine = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsumable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstDrugCategory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstProcedure",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ProcedureName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ProcedureGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProcedureCategoryName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProcedureType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    DefaultTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDoctorAction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNursingAction = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsSurgery = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRadiology = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTherapy = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedDoctor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForOutpatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForInpatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsAvailableForEmergency = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ExternalProcedureCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ClinicalNoteTemplate = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstProcedure", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstProcedure_MstTariff_DefaultTariffId",
                        column: x => x.DefaultTariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDrug",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefaultTariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DrugName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    GenericName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    BrandName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ManufacturerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DrugForm = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Strength = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BaseUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DispenseUnit = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Route = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsFormulary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsGeneric = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAntibiotic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNarcotic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPsychotropic = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsHighAlert = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsChronicDiseaseDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsVaccine = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsumable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedPrescription = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsCoveredByInsuranceDefault = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    DefaultPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    InsurancePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MemberPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CompanyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    Indication = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Contraindication = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SideEffect = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WarningPrecaution = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DosageInformation = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    DrugInteraction = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    AdministrationInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    StorageInstruction = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PregnancyCategory = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LactationNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    PediatricNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    GeriatricNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ExternalDrugCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IntegrationCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BpomRegistrationNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NationalDrugCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstDrug", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDrug_MstDrugCategory_DrugCategoryId",
                        column: x => x.DrugCategoryId,
                        principalSchema: "public",
                        principalTable: "MstDrugCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDrug_MstTariff_DefaultTariffId",
                        column: x => x.DefaultTariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstInsuranceCoverageRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    RuleCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RuleName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ItemType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    BenefitPlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PatientClassName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CoverageStatus = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Covered"),
                    CoveragePercent = table.Column<decimal>(type: "numeric(5,2)", nullable: false, defaultValue: 100m),
                    MaxCoverageAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CoPaymentPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    CoPaymentAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    IsCovered = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsExcluded = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedGuaranteeLetter = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAllowExcessPaymentByPatient = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MaxQuantityPerVisit = table.Column<int>(type: "integer", nullable: true),
                    MaxQuantityPerMonth = table.Column<int>(type: "integer", nullable: true),
                    MaxAmountPerVisit = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    MaxAmountPerMonth = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    ApprovalInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    BillingInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstInsuranceCoverageRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstDrugCategory_DrugCategoryId",
                        column: x => x.DrugCategoryId,
                        principalSchema: "public",
                        principalTable: "MstDrugCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstInsuranceProvider_InsuranceProv~",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceCoverageRule_MstTariffCategory_TariffCategoryId",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_BpomRegistrationNumber",
                schema: "public",
                table: "MstDrug",
                column: "BpomRegistrationNumber",
                filter: "\"BpomRegistrationNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_BrandName",
                schema: "public",
                table: "MstDrug",
                column: "BrandName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DefaultTariffId",
                schema: "public",
                table: "MstDrug",
                column: "DefaultTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugCategoryId",
                schema: "public",
                table: "MstDrug",
                column: "DrugCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugCategoryId_IsFormulary_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "DrugCategoryId", "IsFormulary", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugCode",
                schema: "public",
                table: "MstDrug",
                column: "DrugCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_DrugName",
                schema: "public",
                table: "MstDrug",
                column: "DrugName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_ExternalDrugCode",
                schema: "public",
                table: "MstDrug",
                column: "ExternalDrugCode",
                filter: "\"ExternalDrugCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_GenericName",
                schema: "public",
                table: "MstDrug",
                column: "GenericName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IntegrationCode",
                schema: "public",
                table: "MstDrug",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsAntibiotic_IsNarcotic_IsPsychotropic_IsHighAlert_~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsAntibiotic", "IsNarcotic", "IsPsychotropic", "IsHighAlert", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsCoveredByInsuranceDefault_IsNeedApproval_IsActive~",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrug_IsNeedPrescription_IsActive_IsDelete",
                schema: "public",
                table: "MstDrug",
                columns: new[] { "IsNeedPrescription", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_DrugCategoryCode",
                schema: "public",
                table: "MstDrugCategory",
                column: "DrugCategoryCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_DrugCategoryName",
                schema: "public",
                table: "MstDrugCategory",
                column: "DrugCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_DrugCategoryType",
                schema: "public",
                table: "MstDrugCategory",
                column: "DrugCategoryType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_DrugGroupName",
                schema: "public",
                table: "MstDrugCategory",
                column: "DrugGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_IsAntibiotic_IsNarcotic_IsPsychotropic_IsHi~",
                schema: "public",
                table: "MstDrugCategory",
                columns: new[] { "IsAntibiotic", "IsNarcotic", "IsPsychotropic", "IsHighAlert", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDrugCategory_IsCoveredByInsuranceDefault_IsActive_IsDele~",
                schema: "public",
                table: "MstDrugCategory",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_BenefitPlanCode",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "BenefitPlanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_DrugCategoryId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "DrugCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_DrugId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_EffectiveStartDate_EffectiveEndDat~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_BenefitPlanCod~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "BenefitPlanCode", "ItemType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_CoverageStatus~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "CoverageStatus", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_InsuranceProviderId_ItemType_IsAct~",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                columns: new[] { "InsuranceProviderId", "ItemType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_ItemType",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "ItemType");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_ProcedureId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_RuleCode",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "RuleCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceCoverageRule_TariffId",
                schema: "public",
                table: "MstInsuranceCoverageRule",
                column: "TariffId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_DefaultTariffId",
                schema: "public",
                table: "MstProcedure",
                column: "DefaultTariffId");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ExternalProcedureCode",
                schema: "public",
                table: "MstProcedure",
                column: "ExternalProcedureCode",
                filter: "\"ExternalProcedureCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_IntegrationCode",
                schema: "public",
                table: "MstProcedure",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_IsAvailableForOutpatient_IsAvailableForInpatie~",
                schema: "public",
                table: "MstProcedure",
                columns: new[] { "IsAvailableForOutpatient", "IsAvailableForInpatient", "IsAvailableForEmergency", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_IsCoveredByInsuranceDefault_IsNeedApproval_IsA~",
                schema: "public",
                table: "MstProcedure",
                columns: new[] { "IsCoveredByInsuranceDefault", "IsNeedApproval", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_IsDoctorAction_IsNursingAction_IsSurgery_IsLab~",
                schema: "public",
                table: "MstProcedure",
                columns: new[] { "IsDoctorAction", "IsNursingAction", "IsSurgery", "IsLaboratory", "IsRadiology", "IsTherapy", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureCategoryName",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureCategoryName");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureCode",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureGroupName",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureGroupName");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureName",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureName");

            migrationBuilder.CreateIndex(
                name: "IX_MstProcedure_ProcedureType",
                schema: "public",
                table: "MstProcedure",
                column: "ProcedureType");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstInsuranceCoverageRule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrug",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstProcedure",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDrugCategory",
                schema: "public");
        }
    }
}
