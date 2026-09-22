using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-107 / migration K1. Instrumen dan formulir klinis berversi milik <c>ClinicalManagement</c>
    /// (<c>RWI-DEC-124</c>, <c>RWI-DEC-136</c>) — kamus data 0.4 bagian 11.4 s.d. 11.6: instrumen, versi, dan
    /// jawaban dokumen. Tabel lahir kosong; versi <c>Draft</c> diisi <c>ClinicalInstrumentDraftSeeder</c>.
    /// Foreign key <c>CaseManagementEvaluationId</c> dipasang migration K3 bersama tabel Evaluasi Awal.
    /// Mundur ditolak bila sudah ada jawaban pengkajian tersimpan.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917100000_AddClinicalInstrument")]
    public partial class AddClinicalInstrument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliClinicalInstrument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Code = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    InstrumentKind = table.Column<int>(type: "integer", nullable: false),
                    TargetMinAgeMonths = table.Column<int>(type: "integer", nullable: true),
                    TargetMaxAgeMonths = table.Column<int>(type: "integer", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_CliClinicalInstrument", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CliClinicalInstrumentVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    InstrumentId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    VersionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DefinitionJson = table.Column<string>(type: "jsonb", nullable: false),
                    DefinitionHash = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    LastModifiedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApprovedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovalNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RetiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RetiredByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    RetireReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_CliClinicalInstrumentVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliClinicalInstrumentVersion_InstrumentId",
                        column: x => x.InstrumentId,
                        principalSchema: "public",
                        principalTable: "CliClinicalInstrument",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliClinicalInstrumentVersion_LastModifiedByUserId",
                        column: x => x.LastModifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliClinicalInstrumentVersion_ApprovedByUserId",
                        column: x => x.ApprovedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliClinicalInstrumentVersion_RetiredByUserId",
                        column: x => x.RetiredByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_CliClinicalInstrumentVersion_ApproverDiffers", "\"ApprovedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"LastModifiedByUserId\"");
                });

            migrationBuilder.CreateTable(
                name: "CliAssessmentInstrumentResponse",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uuid", nullable: true),
                    CaseManagementEvaluationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InstrumentVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DefinitionHashSnapshot = table.Column<string>(type: "character(64)", fixedLength: true, maxLength: 64, nullable: false),
                    ResponsesJson = table.Column<string>(type: "jsonb", nullable: false, defaultValueSql: "'{}'::jsonb"),
                    TotalScore = table.Column<decimal>(type: "numeric(8,2)", nullable: true),
                    BandCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BandLabelSnapshot = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsAlertBand = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ComputedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_CliAssessmentInstrumentResponse", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliAssessmentInstrumentResponse_AssessmentId",
                        column: x => x.AssessmentId,
                        principalSchema: "public",
                        principalTable: "TrxPatientAssessment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliAssessmentInstrumentResponse_InstrumentVersionId",
                        column: x => x.InstrumentVersionId,
                        principalSchema: "public",
                        principalTable: "CliClinicalInstrumentVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_CliAssessmentInstrumentResponse_OneOwner", "num_nonnulls(\"AssessmentId\", \"CaseManagementEvaluationId\") = 1");
                });

            migrationBuilder.CreateIndex(
                name: "UX_CliClinicalInstrument_Code",
                schema: "public",
                table: "CliClinicalInstrument",
                column: "Code",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliClinicalInstrument_Kind",
                schema: "public",
                table: "CliClinicalInstrument",
                column: "InstrumentKind");

            migrationBuilder.CreateIndex(
                name: "UX_CliClinicalInstrumentVersion_Instrument_Version",
                schema: "public",
                table: "CliClinicalInstrumentVersion",
                columns: new[] { "InstrumentId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_CliClinicalInstrumentVersion_Approved",
                schema: "public",
                table: "CliClinicalInstrumentVersion",
                column: "InstrumentId",
                unique: true,
                filter: "\"VersionStatus\" = 2 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliClinicalInstrumentVersion_LastModifiedByUserId",
                schema: "public",
                table: "CliClinicalInstrumentVersion",
                column: "LastModifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliClinicalInstrumentVersion_ApprovedByUserId",
                schema: "public",
                table: "CliClinicalInstrumentVersion",
                column: "ApprovedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliClinicalInstrumentVersion_RetiredByUserId",
                schema: "public",
                table: "CliClinicalInstrumentVersion",
                column: "RetiredByUserId");

            migrationBuilder.CreateIndex(
                name: "UX_CliAssessmentInstrumentResponse_Assessment_Version",
                schema: "public",
                table: "CliAssessmentInstrumentResponse",
                columns: new[] { "AssessmentId", "InstrumentVersionId" },
                unique: true,
                filter: "\"AssessmentId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_CliAssessmentInstrumentResponse_Evaluation_Version",
                schema: "public",
                table: "CliAssessmentInstrumentResponse",
                columns: new[] { "CaseManagementEvaluationId", "InstrumentVersionId" },
                unique: true,
                filter: "\"CaseManagementEvaluationId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_CliAssessmentInstrumentResponse_InstrumentVersionId",
                schema: "public",
                table: "CliAssessmentInstrumentResponse",
                column: "InstrumentVersionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""CliAssessmentInstrumentResponse"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-107: rollback ditolak. Tabel CliAssessmentInstrumentResponse berisi % baris; menghapus tabel menghapus hasil pengkajian pasien beserta versi instrumen yang dipakainya.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "CliAssessmentInstrumentResponse",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliClinicalInstrumentVersion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "CliClinicalInstrument",
                schema: "public");
        }
    }
}
