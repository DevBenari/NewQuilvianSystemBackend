using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWfpCompetencySkillMatrix : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MstCompetency",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CompetencyName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CompetencyCategory = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_MstCompetency", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MstPositionCompetencyRequirement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PositionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    MinimumLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsCertificationRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsTrainingRequired = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_MstPositionCompetencyRequirement", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MstPositionCompetencyRequirement_MstCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "public",
                        principalTable: "MstCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MstPositionCompetencyRequirement_MstPosition_PositionId",
                        column: x => x.PositionId,
                        principalSchema: "public",
                        principalTable: "MstPosition",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpCompetencyAssessment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    CompetencyId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssessmentDate = table.Column<DateTime>(type: "date", nullable: false),
                    CompetencyLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ResultStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AssessedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_WfpCompetencyAssessment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpCompetencyAssessment_AspNetUsers_AssessedByUserId",
                        column: x => x.AssessedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpCompetencyAssessment_MstCompetency_CompetencyId",
                        column: x => x.CompetencyId,
                        principalSchema: "public",
                        principalTable: "MstCompetency",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpCompetencyAssessment_MstWorkforceProfile_WorkforceProfil~",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompetency_CompetencyCategory",
                schema: "public",
                table: "MstCompetency",
                column: "CompetencyCategory");

            migrationBuilder.CreateIndex(
                name: "IX_MstCompetency_CompetencyCategory_IsActive_IsDelete",
                schema: "public",
                table: "MstCompetency",
                columns: new[] { "CompetencyCategory", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstCompetency_CompetencyCode",
                schema: "public",
                table: "MstCompetency",
                column: "CompetencyCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MstCompetency_CompetencyName",
                schema: "public",
                table: "MstCompetency",
                column: "CompetencyName");

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_CompetencyId",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_CompetencyId_IsActive_IsDe~",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                columns: new[] { "CompetencyId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_MinimumLevel_IsCertificati~",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                columns: new[] { "MinimumLevel", "IsCertificationRequired", "IsTrainingRequired" });

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_PositionId",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_PositionId_CompetencyId",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                columns: new[] { "PositionId", "CompetencyId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_MstPositionCompetencyRequirement_PositionId_IsRequired_IsAc~",
                schema: "public",
                table: "MstPositionCompetencyRequirement",
                columns: new[] { "PositionId", "IsRequired", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_AssessedByUserId",
                schema: "public",
                table: "WfpCompetencyAssessment",
                column: "AssessedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_CompetencyId",
                schema: "public",
                table: "WfpCompetencyAssessment",
                column: "CompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_ExpiredDate_IsActive_IsDelete",
                schema: "public",
                table: "WfpCompetencyAssessment",
                columns: new[] { "ExpiredDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_WorkforceProfileId",
                schema: "public",
                table: "WfpCompetencyAssessment",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_WorkforceProfileId_CompetencyId_Ass~",
                schema: "public",
                table: "WfpCompetencyAssessment",
                columns: new[] { "WorkforceProfileId", "CompetencyId", "AssessmentDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_WorkforceProfileId_CompetencyId_IsA~",
                schema: "public",
                table: "WfpCompetencyAssessment",
                columns: new[] { "WorkforceProfileId", "CompetencyId", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpCompetencyAssessment_WorkforceProfileId_ResultStatus_IsV~",
                schema: "public",
                table: "WfpCompetencyAssessment",
                columns: new[] { "WorkforceProfileId", "ResultStatus", "IsVerified", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MstPositionCompetencyRequirement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpCompetencyAssessment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "MstCompetency",
                schema: "public");
        }
    }
}
