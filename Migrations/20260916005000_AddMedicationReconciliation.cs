using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-101 / migration R5. Dua tabel baru milik <c>PharmacyManagement</c> — kamus data 0.5
    /// bagian 13.2 dan 13.3: obat bawaan pasien dan riwayat keputusan dokter atasnya
    /// (<c>RWI-DEC-132</c>).
    /// </summary>
    /// <remarks>
    /// Mundur menghapus kedua tabel, tetapi <b>ditolak</b> bila keduanya sudah berisi baris: menghapusnya
    /// berarti menghapus riwayat keputusan terapi obat bawaan pasien (arsitektur 0.5 bagian 11.8).
    /// Jalur pendaftaran obat non-formularium tidak mengubah bentuk <c>MstDrug</c>, sehingga tidak
    /// punya bagian pada migration ini.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916005000_AddMedicationReconciliation")]
    public partial class AddMedicationReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmMedicationReconciliationItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugNameSnapshot = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    IsFormularySnapshot = table.Column<bool>(type: "boolean", nullable: false),
                    Dose = table.Column<decimal>(type: "numeric(12,4)", nullable: true),
                    DoseUnitMeasurementId = table.Column<Guid>(type: "uuid", nullable: true),
                    DrugFormSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    FrequencyText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Route = table.Column<int>(type: "integer", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RecordedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CurrentDecision = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_PhmMedicationReconciliationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_MstEmployee_RecordedByEmplo~",
                        column: x => x.RecordedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_MstMeasurement_DoseUnitMeas~",
                        column: x => x.DoseUnitMeasurementId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationItem_RegPatientEncounter_Encount~",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PhmMedicationReconciliationDecision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReconciliationItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    DecisionType = table.Column<int>(type: "integer", nullable: false),
                    DecisionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DecidedByDoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DecidedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResultPrescriptionId = table.Column<Guid>(type: "uuid", nullable: true),
                    ResultPrescriptionItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    SupersedesDecisionId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_PhmMedicationReconciliationDecision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_AspNetUsers_DecidedByUs~",
                        column: x => x.DecidedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_MstDoctor_DecidedByDoct~",
                        column: x => x.DecidedByDoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_PhmMedicationReconcilia~",
                        column: x => x.ReconciliationItemId,
                        principalSchema: "public",
                        principalTable: "PhmMedicationReconciliationItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_PhmMedicationReconcili~1",
                        column: x => x.SupersedesDecisionId,
                        principalSchema: "public",
                        principalTable: "PhmMedicationReconciliationDecision",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_PhmPrescriptionItem_Res~",
                        column: x => x.ResultPrescriptionItemId,
                        principalSchema: "public",
                        principalTable: "PhmPrescriptionItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmMedicationReconciliationDecision_PhmPrescription_ResultP~",
                        column: x => x.ResultPrescriptionId,
                        principalSchema: "public",
                        principalTable: "PhmPrescription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_CurrentDecision",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "CurrentDecision");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_DoseUnitMeasurementId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "DoseUnitMeasurementId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_DrugId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_EncounterId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_IdempotencyKey",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_InpEpisodeId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_PatientId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_RecordedAt",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_RecordedByEmployeeId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "RecordedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationItem_RecordedByUserId",
                schema: "public",
                table: "PhmMedicationReconciliationItem",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_DecidedByDoctorId",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                column: "DecidedByDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_DecidedByUserId",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                column: "DecidedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_ReconciliationItemId_Se~",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                columns: new[] { "ReconciliationItemId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_ResultPrescriptionId",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                column: "ResultPrescriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_ResultPrescriptionItemId",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                column: "ResultPrescriptionItemId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmMedicationReconciliationDecision_SupersedesDecisionId",
                schema: "public",
                table: "PhmMedicationReconciliationDecision",
                column: "SupersedesDecisionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_baris integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_baris
                    FROM public.""PhmMedicationReconciliationItem"";

                    IF jumlah_baris > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-101: rollback ditolak. % obat bawaan sudah tercatat; menghapus tabel menghapus riwayat keputusan terapi. Ekspor dan minta persetujuan pemilik klinis lebih dulu.',
                            jumlah_baris;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "PhmMedicationReconciliationDecision",
                schema: "public");

            migrationBuilder.DropTable(
                name: "PhmMedicationReconciliationItem",
                schema: "public");
        }
    }
}
