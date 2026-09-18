using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-114 / migration K4. MAR milik <c>PharmacyManagement</c> (<c>RWI-DEC-117</c>) — kamus data 0.4
    /// bagian 11.12 s.d. 11.14: dosis, revisi dosis, jam jadwal per frekuensi, dan pengaturan MAR.
    /// Tanpa jam jadwal terkonfigurasi, dosis terjadwal tidak terbentuk — butir tampil beserta pesan
    /// "jadwal belum dikonfigurasi". Mundur ditolak bila sudah ada dosis tercatat.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917102000_AddMedicationAdministration")]
    public partial class AddMedicationAdministration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmMedicationAdministration",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoseSource = table.Column<int>(type: "integer", nullable: false),
                    ScheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoseStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlannedDose = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    PlannedDoseUnitSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActualDose = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    ActualDoseUnitSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ActualRouteSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AdministeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeviationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsHighAlertSnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    DoubleCheckStatus = table.Column<int>(type: "integer", nullable: false),
                    DoubleCheckedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoubleCheckedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    DoubleCheckedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DoubleCheckNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrnIndication = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PrnEvaluationDueAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrnEvaluationNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PrnEvaluatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrnEvaluatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PhmMedicationAdministration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_PrescriptionId",
                        column: x => x.PrescriptionId,
                        principalSchema: "public",
                        principalTable: "PhmPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_PrescriptionItemId",
                        column: x => x.PrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "PhmPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_RecordedByEmployeeId",
                        column: x => x.RecordedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_DoubleCheckedByEmployeeId",
                        column: x => x.DoubleCheckedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_DoubleCheckedByUserId",
                        column: x => x.DoubleCheckedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministration_PrnEvaluatedByUserId",
                        column: x => x.PrnEvaluatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_PhmMedicationAdministration_ScheduledHasTime", "\"DoseSource\" <> 1 OR \"ScheduledAt\" IS NOT NULL");
                    table.CheckConstraint("CK_PhmMedicationAdministration_DoubleCheckerDiffers", "\"DoubleCheckedByUserId\" IS NULL OR \"DoubleCheckedByUserId\" <> \"RecordedByUserId\"");
                });

            migrationBuilder.CreateTable(
                name: "PhmMedicationAdministrationRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdministrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    RevisionKind = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PreviousDoseStatus = table.Column<int>(type: "integer", nullable: false),
                    PreviousActualDose = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    PreviousActualRouteSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PreviousAdministeredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PreviousStatusReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreviousDeviationNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PreviousRecordedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CorrectedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrectedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_PhmMedicationAdministrationRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministrationRevision_AdministrationId",
                        column: x => x.AdministrationId,
                        principalSchema: "public",
                        principalTable: "PhmMedicationAdministration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationAdministrationRevision_CorrectedByUserId",
                        column: x => x.CorrectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmMedicationScheduleTime",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FrequencyCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    SlotNumber = table.Column<int>(type: "integer", nullable: false),
                    TimeOfDay = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
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
                    table.PrimaryKey("PK_PhmMedicationScheduleTime", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmMedicationScheduleTime_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmMedicationAdministrationSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DoseGenerationHorizonHours = table.Column<int>(type: "integer", nullable: false),
                    MissedAfterMinutes = table.Column<int>(type: "integer", nullable: true),
                    PrnEvaluationMinutes = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_PhmMedicationAdministrationSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationAdministration_Number",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "AdministrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationAdministration_Item_ScheduledAt",
                schema: "public",
                table: "PhmMedicationAdministration",
                columns: new[] { "PrescriptionItemId", "ScheduledAt" },
                unique: true,
                filter: "\"DoseSource\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_Episode_ScheduledAt",
                schema: "public",
                table: "PhmMedicationAdministration",
                columns: new[] { "InpEpisodeId", "ScheduledAt" });

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_Episode_Status",
                schema: "public",
                table: "PhmMedicationAdministration",
                columns: new[] { "InpEpisodeId", "DoseStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_Episode_DoubleCheck",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "InpEpisodeId",
                filter: "\"DoubleCheckStatus\" = 1");

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationAdministration_IdempotencyKey",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_PrescriptionId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "PrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_EncounterId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_PatientId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_DrugId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_RecordedByEmployeeId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_RecordedByUserId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_DoubleCheckedByEmployeeId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "DoubleCheckedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_DoubleCheckedByUserId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "DoubleCheckedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministration_PrnEvaluatedByUserId",
                schema: "public",
                table: "PhmMedicationAdministration",
                column: "PrnEvaluatedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationAdministrationRevision_Admin_Number",
                schema: "public",
                table: "PhmMedicationAdministrationRevision",
                columns: new[] { "AdministrationId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationAdministrationRevision_CorrectedByUserId",
                schema: "public",
                table: "PhmMedicationAdministrationRevision",
                column: "CorrectedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationScheduleTime_Code_Unit_Slot",
                schema: "public",
                table: "PhmMedicationScheduleTime",
                columns: new[] { "FrequencyCode", "ServiceUnitId", "SlotNumber" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationScheduleTime_ServiceUnitId",
                schema: "public",
                table: "PhmMedicationScheduleTime",
                column: "ServiceUnitId");

            migrationBuilder.CreateIndex(
                name: "UX_PhmMedicationAdministrationSetting_Active",
                schema: "public",
                table: "PhmMedicationAdministrationSetting",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""PhmMedicationAdministration"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-114: rollback ditolak. Tabel PhmMedicationAdministration berisi % baris; dosis MAR adalah rekam medis pemberian obat dan tidak boleh hilang bersama tabelnya.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "PhmMedicationAdministrationRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmMedicationAdministration",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmMedicationScheduleTime",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmMedicationAdministrationSetting",
                schema: "public");
        }
    }
}
