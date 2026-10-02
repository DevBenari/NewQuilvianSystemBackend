using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddNursingCarePlanItemRevision : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CliNursingCarePlanItemRevision",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CarePlanItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    ProblemStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    GoalStatement = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    PlannedIntervention = table.Column<string>(type: "text", nullable: true),
                    EvaluationNote = table.Column<string>(type: "text", nullable: true),
                    RevisedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OriginalAuthorEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalAuthoredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_CliNursingCarePlanItemRevision", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlanItemRevision_CliNursingCarePlanItem_CareP~",
                        column: x => x.CarePlanItemId,
                        principalSchema: "public",
                        principalTable: "CliNursingCarePlanItem",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CliNursingCarePlanItemRevision_MstEmployee_OriginalAuthorEm~",
                        column: x => x.OriginalAuthorEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItemRevision_CarePlanItemId",
                schema: "public",
                table: "CliNursingCarePlanItemRevision",
                column: "CarePlanItemId");

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItemRevision_CarePlanItemId_VersionNumber",
                schema: "public",
                table: "CliNursingCarePlanItemRevision",
                columns: new[] { "CarePlanItemId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CliNursingCarePlanItemRevision_OriginalAuthorEmployeeId",
                schema: "public",
                table: "CliNursingCarePlanItemRevision",
                column: "OriginalAuthorEmployeeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CliNursingCarePlanItemRevision",
                schema: "public");
        }
    }
}
