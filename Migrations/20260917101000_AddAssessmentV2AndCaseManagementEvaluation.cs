using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-110 dan BE-RWI-113 / migration K3 — kamus data 0.4 bagian 11.1, 11.2, dan 11.7.
    /// Tiga kolom <c>TrxPatientAssessment</c> (<c>PainAssessmentState</c>, <c>PainReassessmentDueAt</c>,
    /// <c>VitalSignId</c>), satu kolom <c>TrxPatientVitalSign</c> (<c>InpEpisodeId</c>), dan tabel
    /// <c>CliCaseManagementEvaluation</c>. Baris lama tetap terbaca: nyeri <c>NotAssessed</c>, tanpa rujukan
    /// tanda vital, tanda vital tanpa episode. Tanpa mematikan layanan.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917101000_AddAssessmentV2AndCaseManagementEvaluation")]
    public partial class AddAssessmentV2AndCaseManagementEvaluation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliCaseManagementEvaluation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceUnitIdSnapshot = table.Column<Guid>(type: "uuid", nullable: false),
                    EvaluationStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AuthorEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClinicalDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CliCaseManagementEvaluation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_AuthorEmployeeId",
                        column: x => x.AuthorEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_AuthorUserId",
                        column: x => x.AuthorUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_CompletedByUserId",
                        column: x => x.CompletedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliCaseManagementEvaluation_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "UX_CliCaseManagementEvaluation_Number",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "EvaluationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CliCaseManagementEvaluation_Episode_Active",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "InpEpisodeId",
                unique: true,
                filter: "\"EvaluationStatus\" IN (1, 2) AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_CliCaseManagementEvaluation_IdempotencyKey",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_EncounterId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_PatientId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_AuthorEmployeeId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "AuthorEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_AuthorUserId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_CompletedByUserId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "CompletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliCaseManagementEvaluation_CancelledByUserId",
                schema: "public",
                table: "CliCaseManagementEvaluation",
                column: "CancelledByUserId");

            migrationBuilder.AddColumn<int>(
                name: "PainAssessmentState",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "PainReassessmentDueAt",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientVitalSign",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "VitalSignId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_Episode_PainDue",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "InpEpisodeId", "PainReassessmentDueAt" },
                filter: "\"PainReassessmentDueAt\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientVitalSign_InpEpisodeId_ObservationDateTime",
                schema: "public",
                table: "TrxPatientVitalSign",
                columns: new[] { "InpEpisodeId", "ObservationDateTime" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientAssessment_VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "VitalSignId",
                principalSchema: "public",
                principalTable: "TrxPatientVitalSign",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientVitalSign_InpEpisodeId",
                schema: "public",
                table: "TrxPatientVitalSign",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CliAssessmentInstrumentResponse_CaseManagementEvaluationId",
                schema: "public",
                table: "CliAssessmentInstrumentResponse",
                column: "CaseManagementEvaluationId",
                principalSchema: "public",
                principalTable: "CliCaseManagementEvaluation",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""CliCaseManagementEvaluation"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-113: rollback ditolak. Tabel CliCaseManagementEvaluation berisi % baris; menghapus tabel menghapus Evaluasi Awal MPP pasien.', jumlah;
                    END IF;
                END $$;");

            // BE-RWI-110 kriteria 6. Kolom boleh dihapus hanya selama belum berisi data pasien sungguhan.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""TrxPatientAssessment""
                    WHERE ""VitalSignId"" IS NOT NULL OR ""PainAssessmentState"" <> 0 OR ""PainReassessmentDueAt"" IS NOT NULL;
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-110: rollback ditolak. % pengkajian sudah mengisi kolom K3; menghapus kolom menghapus keadaan nyeri dan rujukan tanda vital pasien.', jumlah;
                    END IF;

                    SELECT COUNT(*) INTO jumlah FROM public.""TrxPatientVitalSign"" WHERE ""InpEpisodeId"" IS NOT NULL;
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-110: rollback ditolak. % tanda vital sudah menyimpan episode rawat inap.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropForeignKey(
                name: "FK_CliAssessmentInstrumentResponse_CaseManagementEvaluationId",
                schema: "public",
                table: "CliAssessmentInstrumentResponse");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientVitalSign_InpEpisodeId",
                schema: "public",
                table: "TrxPatientVitalSign");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientAssessment_VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropTable(
                name: "CliCaseManagementEvaluation",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientVitalSign_InpEpisodeId_ObservationDateTime",
                schema: "public",
                table: "TrxPatientVitalSign");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_Episode_PainDue",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientVitalSign");

            migrationBuilder.DropColumn(
                name: "VitalSignId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "PainReassessmentDueAt",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "PainAssessmentState",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
