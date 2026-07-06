using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class changeColumnDiagnosis : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisChapter_ChapterCode",
                schema: "public",
                table: "MstDiagnosisChapter");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion",
                schema: "public",
                table: "MstDiagnosisChapter");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_DiagnosisCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_DiagnosisType",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_ExternalDiagnosisCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_GenderRestriction_MinimumAgeYear_MaximumAgeYear",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IcdVersion",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IntegrationCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IsBillable_IsPrimaryDiagnosisAllowed_IsSeconda~",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IsChronicDisease_IsInfectiousDisease_IsExterna~",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IsPregnancyRelated_IsMentalHealthRelated_IsRar~",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_ShortName",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "MstDiagnosisChapter");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstDiagnosisChapter");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisCategoryName",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisGroupName",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "DiagnosisNameEnglish",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "ExternalDiagnosisCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "GenderRestriction",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IntegrationCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsBillable",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsChronicDisease",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsExternalCause",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsInfectiousDisease",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsMentalHealthRelated",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsPregnancyRelated",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "IsRareDisease",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "MaximumAgeYear",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "MinimumAgeYear",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "ShortName",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.DropColumn(
                name: "SortOrder",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.CreateTable(
                name: "MstDiagnosisDrugRecommendation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrugId = table.Column<Guid>(type: "uuid", nullable: false),
                    RecommendationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Supportive"),
                    IndicationText = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DoseText = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    Route = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Frequency = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DurationText = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CautionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstDiagnosisDrugRecommendation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDiagnosisDrugRecommendation_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDiagnosisDrugRecommendation_MstDrug_DrugId",
                        column: x => x.DrugId,
                        principalSchema: "public",
                        principalTable: "MstDrug",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDiagnosisEducationRecommendation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    EducationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "General"),
                    EducationTitle = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    EducationText = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
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
                    table.PrimaryKey("PK_MstDiagnosisEducationRecommendation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDiagnosisEducationRecommendation_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MstDiagnosisProcedureRecommendation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DiagnosisId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecommendationType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "Procedure"),
                    RecommendationName = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    InstructionText = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_MstDiagnosisProcedureRecommendation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstDiagnosisProcedureRecommendation_MstDiagnosis_DiagnosisId",
                        column: x => x.DiagnosisId,
                        principalSchema: "public",
                        principalTable: "MstDiagnosis",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstDiagnosisProcedureRecommendation_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion_ChapterCode",
                schema: "public",
                table: "MstDiagnosisChapter",
                columns: new[] { "IcdVersion", "ChapterCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IcdVersion_DiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IcdVersion", "DiagnosisCode" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DiagnosisId_RecommendationTy~",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DiagnosisId", "RecommendationType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                column: "DrugId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisDrugRecommendation_DrugId_IsActive_IsDelete",
                schema: "public",
                table: "MstDiagnosisDrugRecommendation",
                columns: new[] { "DrugId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_DiagnosisId",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_DiagnosisId_EducationTy~",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                columns: new[] { "DiagnosisId", "EducationType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisEducationRecommendation_EducationTitle",
                schema: "public",
                table: "MstDiagnosisEducationRecommendation",
                column: "EducationTitle");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_DiagnosisId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "DiagnosisId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_DiagnosisId_Recommendat~",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                columns: new[] { "DiagnosisId", "RecommendationType", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_ProcedureId",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisProcedureRecommendation_RecommendationName",
                schema: "public",
                table: "MstDiagnosisProcedureRecommendation",
                column: "RecommendationName");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstDiagnosisDrugRecommendation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDiagnosisEducationRecommendation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstDiagnosisProcedureRecommendation",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion_ChapterCode",
                schema: "public",
                table: "MstDiagnosisChapter");

            migrationBuilder.DropIndex(
                name: "IX_MstDiagnosis_IcdVersion_DiagnosisCode",
                schema: "public",
                table: "MstDiagnosis");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "MstDiagnosisChapter",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstDiagnosisChapter",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisCategoryName",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisGroupName",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosisNameEnglish",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalDiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GenderRestriction",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IntegrationCode",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBillable",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsChronicDisease",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsExternalCause",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsInfectiousDisease",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsMentalHealthRelated",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPregnancyRelated",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsRareDisease",
                schema: "public",
                table: "MstDiagnosis",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "MaximumAgeYear",
                schema: "public",
                table: "MstDiagnosis",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MinimumAgeYear",
                schema: "public",
                table: "MstDiagnosis",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShortName",
                schema: "public",
                table: "MstDiagnosis",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SortOrder",
                schema: "public",
                table: "MstDiagnosis",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_ChapterCode",
                schema: "public",
                table: "MstDiagnosisChapter",
                column: "ChapterCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosisChapter_IcdVersion",
                schema: "public",
                table: "MstDiagnosisChapter",
                column: "IcdVersion");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_DiagnosisType",
                schema: "public",
                table: "MstDiagnosis",
                column: "DiagnosisType");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ExternalDiagnosisCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "ExternalDiagnosisCode",
                filter: "\"ExternalDiagnosisCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_GenderRestriction_MinimumAgeYear_MaximumAgeYear",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "GenderRestriction", "MinimumAgeYear", "MaximumAgeYear" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IcdVersion",
                schema: "public",
                table: "MstDiagnosis",
                column: "IcdVersion");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IntegrationCode",
                schema: "public",
                table: "MstDiagnosis",
                column: "IntegrationCode",
                filter: "\"IntegrationCode\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsBillable_IsPrimaryDiagnosisAllowed_IsSeconda~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsBillable", "IsPrimaryDiagnosisAllowed", "IsSecondaryDiagnosisAllowed", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsChronicDisease_IsInfectiousDisease_IsExterna~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsChronicDisease", "IsInfectiousDisease", "IsExternalCause", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_IsPregnancyRelated_IsMentalHealthRelated_IsRar~",
                schema: "public",
                table: "MstDiagnosis",
                columns: new[] { "IsPregnancyRelated", "IsMentalHealthRelated", "IsRareDisease", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstDiagnosis_ShortName",
                schema: "public",
                table: "MstDiagnosis",
                column: "ShortName");
        }
    }
}
