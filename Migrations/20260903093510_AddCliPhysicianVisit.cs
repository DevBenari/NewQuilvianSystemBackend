using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCliPhysicianVisit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliPhysicianVisit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PhysicianVisitNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorId = table.Column<Guid>(type: "uuid", nullable: false),
                    VisitDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VisitRole = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    VisitStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ConsultationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProgressNoteId = table.Column<Guid>(type: "uuid", nullable: true),
                    PatientProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Note = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    RecordedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrectsVisitId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_CliPhysicianVisit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_AspNetUsers_CancelledByUserId",
                        column: x => x.CancelledByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_CliPhysicianVisit_CorrectsVisitId",
                        column: x => x.CorrectsVisitId,
                        principalSchema: "public",
                        principalTable: "CliPhysicianVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_InpEpisode_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_MstDoctor_DoctorId",
                        column: x => x.DoctorId,
                        principalSchema: "public",
                        principalTable: "MstDoctor",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_TrxDoctorConsultation_ConsultationId",
                        column: x => x.ConsultationId,
                        principalSchema: "public",
                        principalTable: "TrxDoctorConsultation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_TrxPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "TrxPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_TrxPatientIntegratedProgressNote_Progress~",
                        column: x => x.ProgressNoteId,
                        principalSchema: "public",
                        principalTable: "TrxPatientIntegratedProgressNote",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CliPhysicianVisit_TrxPatientProcedure_PatientProcedureId",
                        column: x => x.PatientProcedureId,
                        principalSchema: "public",
                        principalTable: "TrxPatientProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_CancelledByUserId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "CancelledByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_ConsultationId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "ConsultationId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_CorrectsVisitId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "CorrectsVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_DoctorId_VisitDateTime",
                schema: "public",
                table: "CliPhysicianVisit",
                columns: new[] { "DoctorId", "VisitDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_EncounterId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_IdempotencyKey",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_InpEpisodeId_VisitDateTime",
                schema: "public",
                table: "CliPhysicianVisit",
                columns: new[] { "InpEpisodeId", "VisitDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_PatientId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_PatientProcedureId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "PatientProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_PhysicianVisitNumber",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "PhysicianVisitNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_ProgressNoteId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "ProgressNoteId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_RecordedByUserId",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CliPhysicianVisit_VisitStatus",
                schema: "public",
                table: "CliPhysicianVisit",
                column: "VisitStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxDoctorConsultation_CliPhysicianVisit_PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "PhysicianVisitId",
                principalSchema: "public",
                principalTable: "CliPhysicianVisit",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_CliPhysicianVisit_PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "PhysicianVisitId",
                principalSchema: "public",
                principalTable: "CliPhysicianVisit",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxDoctorConsultation_CliPhysicianVisit_PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_CliPhysicianVisit_PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropTable(
                name: "CliPhysicianVisit",
                schema: "public");
        }
    }
}
