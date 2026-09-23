using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-119 / migration K5. Pengawasan Harian milik <c>ClinicalManagement</c> — kamus data 0.4 bagian
    /// 11.8 s.d. 11.11: cairan, GDS bangsal, observasi harian beserta tabel revisinya, dan jam shift.
    /// GDS menyimpan satuan wajib tanpa nilai bawaan (<c>RWI-DEC-148</c>, gate <c>G-25</c>). Setelah K4 karena
    /// entri intake obat menunjuk dosis MAR. Mundur ditolak bila tabel sudah berisi data pasien.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917103000_AddDailyMonitoring")]
    public partial class AddDailyMonitoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliFluidBalanceEntry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Direction = table.Column<int>(type: "integer", nullable: false),
                    SourceCategory = table.Column<int>(type: "integer", nullable: false),
                    SourceDetail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    VolumeMl = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    EntryDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    MedicationAdministrationId = table.Column<Guid>(type: "uuid", nullable: true),
                    EntryStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoseCorrectionFlaggedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_CliFluidBalanceEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_RecordedByEmployeeId",
                        column: x => x.RecordedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntry_MedicationAdministrationId",
                        column: x => x.MedicationAdministrationId,
                        principalSchema: "public",
                        principalTable: "PhmMedicationAdministration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_CliFluidBalanceEntry_Volume", "\"VolumeMl\" > 0 AND \"VolumeMl\" <= 10000");
                    table.CheckConstraint("CK_CliFluidBalanceEntry_MedicationLink", "(\"SourceCategory\" = 5) = (\"MedicationAdministrationId\" IS NOT NULL)");
                });

            migrationBuilder.CreateTable(
                name: "CliFluidBalanceEntryRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousVolumeMl = table.Column<decimal>(type: "numeric(9,2)", nullable: false),
                    PreviousEntryDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    PreviousSourceCategory = table.Column<int>(type: "integer", nullable: false),
                    PreviousSourceDetail = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_CliFluidBalanceEntryRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntryRevision_EntryId",
                        column: x => x.EntryId,
                        principalSchema: "public",
                        principalTable: "CliFluidBalanceEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliFluidBalanceEntryRevision_CorrectedByUserId",
                        column: x => x.CorrectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliBloodGlucoseReading",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    MeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    GlucoseValue = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    GlucoseUnit = table.Column<int>(type: "integer", nullable: false),
                    Method = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliBloodGlucoseReading", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReading_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReading_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReading_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReading_RecordedByEmployeeId",
                        column: x => x.RecordedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReading_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_CliBloodGlucoseReading_Unit", "\"GlucoseUnit\" IN (1, 2)");
                });

            migrationBuilder.CreateTable(
                name: "CliBloodGlucoseReadingRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousValue = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    PreviousUnit = table.Column<int>(type: "integer", nullable: false),
                    PreviousMeasuredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliBloodGlucoseReadingRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReadingRevision_ReadingId",
                        column: x => x.ReadingId,
                        principalSchema: "public",
                        principalTable: "CliBloodGlucoseReading",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliBloodGlucoseReadingRevision_CorrectedByUserId",
                        column: x => x.CorrectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliDailyObservation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DietIntakePercent = table.Column<int>(type: "integer", nullable: true),
                    DietNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    MobilizationLevel = table.Column<int>(type: "integer", nullable: false),
                    AbdominalCircumferenceCm = table.Column<decimal>(type: "numeric(5,1)", nullable: true),
                    IsAgitated = table.Column<bool>(type: "boolean", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliDailyObservation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliDailyObservation_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDailyObservation_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDailyObservation_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDailyObservation_RecordedByEmployeeId",
                        column: x => x.RecordedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDailyObservation_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_CliDailyObservation_DietPercent", "\"DietIntakePercent\" IS NULL OR (\"DietIntakePercent\" >= 0 AND \"DietIntakePercent\" <= 100)");
                    table.CheckConstraint("CK_CliDailyObservation_Abdominal", "\"AbdominalCircumferenceCm\" IS NULL OR (\"AbdominalCircumferenceCm\" >= 20 AND \"AbdominalCircumferenceCm\" <= 250)");
                });

            migrationBuilder.CreateTable(
                name: "CliDailyObservationRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ObservationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RevisionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousValuesJson = table.Column<string>(type: "jsonb", nullable: false),
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
                    table.PrimaryKey("PK_CliDailyObservationRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliDailyObservationRevision_ObservationId",
                        column: x => x.ObservationId,
                        principalSchema: "public",
                        principalTable: "CliDailyObservation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliDailyObservationRevision_CorrectedByUserId",
                        column: x => x.CorrectedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CliNursingShift",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    ShiftCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ShiftName = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time without time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliNursingShift", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliNursingShift_ServiceUnitId",
                        column: x => x.ServiceUnitId,
                        principalSchema: "public",
                        principalTable: "MstServiceUnit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntry_Episode_EntryDateTime",
                schema: "public",
                table: "CliFluidBalanceEntry",
                columns: new[] { "InpEpisodeId", "EntryDateTime" });

            migrationBuilder.CreateIndex(
                name: "UX_CliFluidBalanceEntry_MedicationAdministration_Active",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "MedicationAdministrationId",
                unique: true,
                filter: "\"MedicationAdministrationId\" IS NOT NULL AND \"EntryStatus\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_CliFluidBalanceEntry_IdempotencyKey",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntry_EncounterId",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntry_PatientId",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntry_RecordedByEmployeeId",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntry_RecordedByUserId",
                schema: "public",
                table: "CliFluidBalanceEntry",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CliFluidBalanceEntryRevision_Entry_Number",
                schema: "public",
                table: "CliFluidBalanceEntryRevision",
                columns: new[] { "EntryId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliFluidBalanceEntryRevision_CorrectedByUserId",
                schema: "public",
                table: "CliFluidBalanceEntryRevision",
                column: "CorrectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReading_Episode_MeasuredAt",
                schema: "public",
                table: "CliBloodGlucoseReading",
                columns: new[] { "InpEpisodeId", "MeasuredAt" });

            migrationBuilder.CreateIndex(
                name: "UX_CliBloodGlucoseReading_IdempotencyKey",
                schema: "public",
                table: "CliBloodGlucoseReading",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReading_EncounterId",
                schema: "public",
                table: "CliBloodGlucoseReading",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReading_PatientId",
                schema: "public",
                table: "CliBloodGlucoseReading",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReading_RecordedByEmployeeId",
                schema: "public",
                table: "CliBloodGlucoseReading",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReading_RecordedByUserId",
                schema: "public",
                table: "CliBloodGlucoseReading",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CliBloodGlucoseReadingRevision_Reading_Number",
                schema: "public",
                table: "CliBloodGlucoseReadingRevision",
                columns: new[] { "ReadingId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliBloodGlucoseReadingRevision_CorrectedByUserId",
                schema: "public",
                table: "CliBloodGlucoseReadingRevision",
                column: "CorrectedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservation_Episode_ObservedAt",
                schema: "public",
                table: "CliDailyObservation",
                columns: new[] { "InpEpisodeId", "ObservedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_CliDailyObservation_IdempotencyKey",
                schema: "public",
                table: "CliDailyObservation",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservation_EncounterId",
                schema: "public",
                table: "CliDailyObservation",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservation_PatientId",
                schema: "public",
                table: "CliDailyObservation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservation_RecordedByEmployeeId",
                schema: "public",
                table: "CliDailyObservation",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservation_RecordedByUserId",
                schema: "public",
                table: "CliDailyObservation",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CliDailyObservationRevision_Observation_Number",
                schema: "public",
                table: "CliDailyObservationRevision",
                columns: new[] { "ObservationId", "RevisionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliDailyObservationRevision_CorrectedByUserId",
                schema: "public",
                table: "CliDailyObservationRevision",
                column: "CorrectedByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CliNursingShift_Unit_Code",
                schema: "public",
                table: "CliNursingShift",
                columns: new[] { "ServiceUnitId", "ShiftCode" },
                unique: true,
                filter: "\"IsDelete\" = false")
                .Annotation("Npgsql:NullsDistinct", false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""CliFluidBalanceEntry"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-119: rollback ditolak. Tabel CliFluidBalanceEntry berisi % baris; Pengawasan Harian berisi data pasien sungguhan; ekspor dan persetujuan pemilik klinis wajib lebih dulu.', jumlah;
                    END IF;
                    SELECT COUNT(*) INTO jumlah FROM public.""CliBloodGlucoseReading"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-119: rollback ditolak. Tabel CliBloodGlucoseReading berisi % baris; Pengawasan Harian berisi data pasien sungguhan; ekspor dan persetujuan pemilik klinis wajib lebih dulu.', jumlah;
                    END IF;
                    SELECT COUNT(*) INTO jumlah FROM public.""CliDailyObservation"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-119: rollback ditolak. Tabel CliDailyObservation berisi % baris; Pengawasan Harian berisi data pasien sungguhan; ekspor dan persetujuan pemilik klinis wajib lebih dulu.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "CliFluidBalanceEntryRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliFluidBalanceEntry",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliBloodGlucoseReadingRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliBloodGlucoseReading",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliDailyObservationRevision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliDailyObservation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliNursingShift",
                schema: "public");
        }
    }
}
