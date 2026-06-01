using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changePatientDoctorConsultant : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcedureNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComplicationNote",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FollowUpInstruction",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyProcedure",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromMasterProcedure",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsPackageProcedure",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPrimaryProcedure",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PerformedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PlannedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcedureCategoryNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ProcedureMasterType",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Master");

            migrationBuilder.AddColumn<DateTime>(
                name: "ScheduledAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UnitNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ChiefComplaint",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ClinicalDocumentCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ConsentCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasPrescription",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasProcedure",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "HasSupportingOrder",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "HistoryOfPresentIllness",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MedicalCertificateCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PhysicalExamination",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PrescriptionCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PrescriptionText",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProcedureCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ProcedureText",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SupportingOrderCount",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "SupportingOrderText",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_IsPrimaryProcedure_IsDel~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ConsultationId", "IsPrimaryProcedure", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_ProcedureCodeSnapshot_Is~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ConsultationId", "ProcedureCodeSnapshot", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_IsExecuted_ExecutedAt_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "IsExecuted", "ExecutedAt", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_IsSurgeryRelated_IsPackageProcedure_IsD~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "IsSurgeryRelated", "IsPackageProcedure", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_PerformedAt_PerformedByUserId_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "PerformedAt", "PerformedByUserId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ProcedureCodeSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ProcedureCodeSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ProcedureMasterType_IsFromMasterProcedu~",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "ProcedureMasterType", "IsFromMasterProcedure", "IsPrimaryProcedure", "IsEmergencyProcedure", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_ProcedureNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "ProcedureNameSnapshot");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_DiagnosisCount_ProcedureCount_Prescri~",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "DiagnosisCount", "ProcedureCount", "PrescriptionCount", "SupportingOrderCount", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_FollowUpDate_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "FollowUpDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_HasPrescription_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "HasPrescription", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_HasProcedure_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "HasProcedure", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_HasSupportingOrder_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation",
                columns: new[] { "HasSupportingOrder", "IsDelete" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "PerformedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_IsPrimaryProcedure_IsDel~",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ConsultationId_ProcedureCodeSnapshot_Is~",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_IsExecuted_ExecutedAt_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_IsSurgeryRelated_IsPackageProcedure_IsD~",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_PerformedAt_PerformedByUserId_IsDelete",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ProcedureCodeSnapshot",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ProcedureMasterType_IsFromMasterProcedu~",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_ProcedureNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_DiagnosisCount_ProcedureCount_Prescri~",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_FollowUpDate_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_HasPrescription_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_HasProcedure_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_HasSupportingOrder_IsDelete",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "CompletedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "ComplicationNote",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "FollowUpInstruction",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "IsEmergencyProcedure",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "IsFromMasterProcedure",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "IsPackageProcedure",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "IsPrimaryProcedure",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "IsSurgeryRelated",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "PerformedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "PerformedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "PlannedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "ProcedureCategoryNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "ProcedureMasterType",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "ScheduledAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "StartedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "UnitNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "ChiefComplaint",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "ClinicalDocumentCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "ConsentCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "HasPrescription",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "HasProcedure",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "HasSupportingOrder",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "HistoryOfPresentIllness",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "MedicalCertificateCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "PhysicalExamination",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "PrescriptionCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "PrescriptionText",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "ProcedureCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "ProcedureText",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "SupportingOrderCount",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "SupportingOrderText",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.AlterColumn<string>(
                name: "ProcedureNameSnapshot",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);
        }
    }
}
