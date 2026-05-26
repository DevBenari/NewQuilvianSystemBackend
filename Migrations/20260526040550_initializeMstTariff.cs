using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class initializeMstTariff : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstTariff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TariffCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TariffName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    TariffCategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ClinicId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientClassId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExternalServiceCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ExternalClassCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProviderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsSurgeryRelated = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRoomCharge = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsAdministrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsRegistrationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsConsultationFee = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsPackageTariff = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedDoctor = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsNeedApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    NormalPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: false, defaultValue: 0m),
                    MemberPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    InsurancePrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    CompanyPrice = table.Column<decimal>(type: "numeric(18,2)", nullable: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
                    EffectiveEndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsTaxable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstTariff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstTariff_MstClinic_ClinicId",
                        column: x => x.ClinicId,
                        principalSchema: "public",
                        principalTable: "MstClinic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstTariff_MstPatientClass_PatientClassId",
                        column: x => x.PatientClassId,
                        principalSchema: "public",
                        principalTable: "MstPatientClass",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstTariff_MstServiceUnit_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstTariff_MstTariffCategory_TariffCategoryId",
                        column: x => x.TariffCategoryId,
                        principalSchema: "public",
                        principalTable: "MstTariffCategory",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ClinicId",
                schema: "public",
                table: "MstTariff",
                column: "ClinicId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ExternalClassCode",
                schema: "public",
                table: "MstTariff",
                column: "ExternalClassCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ExternalServiceCode",
                schema: "public",
                table: "MstTariff",
                column: "ExternalServiceCode");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_IsRoomCharge_IsAdministrationFee_IsRegistrationFe~",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "IsRoomCharge", "IsAdministrationFee", "IsRegistrationFee", "IsConsultationFee", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_PatientClassId",
                schema: "public",
                table: "MstTariff",
                column: "PatientClassId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ServiceUnitId",
                schema: "public",
                table: "MstTariff",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_ServiceUnitId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "ServiceUnitId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_TariffCategoryId",
                schema: "public",
                table: "MstTariff",
                column: "TariffCategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_TariffCategoryId_PatientClassId_IsActive_IsDelete",
                schema: "public",
                table: "MstTariff",
                columns: new[] { "TariffCategoryId", "PatientClassId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_TariffCode",
                schema: "public",
                table: "MstTariff",
                column: "TariffCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstTariff_TariffName",
                schema: "public",
                table: "MstTariff",
                column: "TariffName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstTariff",
                schema: "public");
        }
    }
}
