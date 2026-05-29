using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class addMstInsuranceTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstInsuranceTariff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    InsuranceTariffCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    InsuranceTariffName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    ExternalServiceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalClassCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BenefitPlanCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BenefitPlanName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    PatientClassName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ContractPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    HospitalPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiscountAmount = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    DiscountPercent = table.Column<decimal>(type: "numeric(5,2)", nullable: true),
                    IsUsingContractPrice = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSurgeryRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRoomCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsDrug = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsumable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsProcedure = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsLaboratory = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRadiology = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    SortOrder = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    BillingInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    ClaimInstruction = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
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
                    table.PrimaryKey("PK_MstInsuranceTariff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstInsuranceProvider_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstTariff_TariffId",
                        column: x => x.TariffId,
                        principalSchema: "public",
                        principalTable: "MstTariff",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstInsuranceTariff_MstTariffCategory_TariffCategoryId",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_BenefitPlanCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "BenefitPlanCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_DrugId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_EffectiveStartDate_EffectiveEndDate_IsAc~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "EffectiveStartDate", "EffectiveEndDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ExternalClassCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ExternalClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ExternalServiceCode",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ExternalServiceCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "InsuranceProviderId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_DrugId_PatientClassI~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "DrugId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_InsuranceTariffCode_~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "InsuranceTariffCode", "ExternalClassCode", "BenefitPlanCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_PatientClassId_IsAct~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_InsuranceProviderId_ProcedureId_PatientC~",
                schema: "public",
                table: "MstInsuranceTariff",
                columns: new[] { "InsuranceProviderId", "ProcedureId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_PatientClassId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_ProcedureId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_TariffCategoryId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstInsuranceTariff_TariffId",
                schema: "public",
                table: "MstInsuranceTariff",
                column: "TariffId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstInsuranceTariff",
                schema: "public");
        }
    }
}
