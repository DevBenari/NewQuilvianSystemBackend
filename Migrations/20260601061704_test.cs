using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class test : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "DiagnosisName",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(250)",
                oldMaxLength: 250);

            migrationBuilder.AddColumn<Guid>(
                name: "DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisMasterType",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "ICD10");

            migrationBuilder.AddColumn<string>(
                name: "DifferentialDiagnosisNote",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IcdVersion",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFromMasterDiagnosis",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "OnsetDate",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SupportingFindingNote",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_ConsultationId_DiagnosisId_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "ConsultationId", "DiagnosisId", "IsDelete" },
                filter: "\"DiagnosisId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisCode",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "DiagnosisCode");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisMasterType_IcdVersion_IsFromMa~",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "DiagnosisMasterType", "IcdVersion", "IsFromMasterDiagnosis", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisName",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "DiagnosisName");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_IcdVersion",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "IcdVersion");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_IsPrimary_IsConfirmed_IsChronic_IsNewCa~",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "IsPrimary", "IsConfirmed", "IsChronic", "IsNewCase", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_PatientId_DiagnosisId_DiagnosisStatus_I~",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "PatientId", "DiagnosisId", "DiagnosisStatus", "IsDelete" },
                filter: "\"DiagnosisId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientDiagnosis_MstDiagnosis_DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "DiagnosisId",
                principalSchema: "public",
                principalTable: "MstDiagnosis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientDiagnosis_MstDiagnosis_DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_ConsultationId_DiagnosisId_IsDelete",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisCode",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisMasterType_IcdVersion_IsFromMa~",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_DiagnosisName",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_IcdVersion",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_IsPrimary_IsConfirmed_IsChronic_IsNewCa~",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_PatientId_DiagnosisId_DiagnosisStatus_I~",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisMasterType",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "DifferentialDiagnosisNote",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "IcdVersion",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsFromMasterDiagnosis",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "OnsetDate",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "SupportingFindingNote",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.AlterColumn<string>(
                name: "DiagnosisName",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "character varying(250)",
                maxLength: 250,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500);
        }
    }
}
