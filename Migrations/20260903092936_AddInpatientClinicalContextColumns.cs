using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInpatientClinicalContextColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerificationDueAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "VerifiedAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VerifiedByUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssessmentType",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClinicalDateTime",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_IdempotencyKey",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "IdempotencyKey",
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_InpEpisodeId_PerformedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "InpEpisodeId", "PerformedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "PhysicianVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_InpEpisodeId_NoteDateTime",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "InpEpisodeId", "NoteDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerificationDueAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "VerificationDueAt",
                filter: "\"VerificationStatus\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerificationStatus",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "VerificationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAssessment_InpEpisodeId_AssessmentType",
                schema: "public",
                table: "TrxPatientAssessment",
                columns: new[] { "InpEpisodeId", "AssessmentType" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_InpEpisodeId_ClinicalDateTime",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "InpEpisodeId", "ClinicalDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "PhysicianVisitId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxDoctorConsultation_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientAssessment_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientIntegratedProgressNote_AspNetUsers_VerifiedByUser~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "VerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientIntegratedProgressNote_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxDoctorConsultation_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientAssessment_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientIntegratedProgressNote_AspNetUsers_VerifiedByUser~",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientIntegratedProgressNote_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_IdempotencyKey",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_InpEpisodeId_PerformedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientIntegratedProgressNote_InpEpisodeId_NoteDateTime",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerificationDueAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerificationStatus",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientIntegratedProgressNote_VerifiedByUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAssessment_InpEpisodeId_AssessmentType",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_InpEpisodeId_ClinicalDateTime",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "IdempotencyKey",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "PhysicianVisitId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "VerificationDueAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "VerifiedAt",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "VerifiedByUserId",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "AssessmentType",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "ClinicalDateTime",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "PhysicianVisitId",
                schema: "public",
                table: "TrxDoctorConsultation");
        }
    }
}
