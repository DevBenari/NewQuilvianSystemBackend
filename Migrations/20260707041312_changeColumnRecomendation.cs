using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnRecomendation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_RecommendationTy~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DraftFromLiterature");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DraftFromLiterature");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "DraftFromLiterature");

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_DiagnosisId_ReviewStatu~",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                columns: new[] { "DiagnosisId", "ReviewStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "MstDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_DiagnosisId_ReviewStatu~",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                columns: new[] { "DiagnosisId", "ReviewStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                column: "MstDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                column: "ReviewStatus");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_DrugId_Recommend~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DiagnosisId", "DrugId", "RecommendationType", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_ReviewStatus_IsA~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DiagnosisId", "ReviewStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId_ReviewStatus_IsActive~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DrugId", "ReviewStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                column: "MstDiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                column: "ReviewStatus");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDiagnosisDrugRecommendation_MstDiagnosis_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                column: "MstDiagnosisId",
                principalSchema: "public",
                principalTable: "MstDiagnosis",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDiagnosisEducationRecommendation_MstDiagnosis_MstDiagnos~",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                column: "MstDiagnosisId",
                principalSchema: "public",
                principalTable: "MstDiagnosis",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MstDiagnosisProcedureRecommendation_MstDiagnosis_MstDiagnos~",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "MstDiagnosisId",
                principalSchema: "public",
                principalTable: "MstDiagnosis",
                principalColumn: "Id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MstDiagnosisDrugRecommendation_MstDiagnosis_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDiagnosisEducationRecommendation_MstDiagnosis_MstDiagnos~",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropForeignKey(
                name: "FK_MstDiagnosisProcedureRecommendation_MstDiagnosis_MstDiagnos~",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_DiagnosisId_ReviewStatu~",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisEducationRecommendation_DiagnosisId_ReviewStatu~",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisEducationRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisEducationRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_DrugId_Recommend~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_ReviewStatus_IsA~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId_ReviewStatus_IsActive~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisDrugRecommendation_ReviewStatus",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedAt",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivatedByUserId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ActivationNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "MstDiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedAt",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceNote",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceTitle",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceUrl",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.DropColumn(
                name: "SourceYear",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_RecommendationTy~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DiagnosisId", "RecommendationType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DrugId", "IsActive", "IsDelete" });
        }
    }
}
