using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWfpComplianceAlert : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpComplianceAlert",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    SourceEntityName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlertType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    AlertTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AlertMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    DueDate = table.Column<DateTime>(type: "date", nullable: false),
                    AlertStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    SeverityLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IsResolved = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ResolvedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpComplianceAlert", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpComplianceAlert_AspNetUsers_ResolvedByUserId",
                        column: x => x.ResolvedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpComplianceAlert_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "WfpComplianceAlertLog",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ComplianceAlertId = table.Column<Guid>(type: "uuid", nullable: false),
                    LogType = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    OldStatus = table.Column<int>(type: "integer", nullable: true),
                    NewStatus = table.Column<int>(type: "integer", nullable: true),
                    LogMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PerformedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    PerformedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_WfpComplianceAlertLog", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpComplianceAlertLog_AspNetUsers_PerformedByUserId",
                        column: x => x.PerformedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpComplianceAlertLog_WfpComplianceAlert_ComplianceAlertId",
                        column: x => x.ComplianceAlertId,
                        principalSchema: "public",
                        principalTable: "WfpComplianceAlert",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_AlertStatus",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "AlertStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_AlertType",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "AlertType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_DueDate",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_DueDate_SeverityLevel_AlertStatus_IsReso~",
                schema: "public",
                table: "WfpComplianceAlert",
                columns: new[] { "DueDate", "SeverityLevel", "AlertStatus", "IsResolved", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_ResolvedByUserId",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "ResolvedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_SeverityLevel",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "SeverityLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_SourceEntityId",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "SourceEntityId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_SourceEntityName",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "SourceEntityName");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_SourceEntityName_SourceEntityId_AlertTyp~",
                schema: "public",
                table: "WfpComplianceAlert",
                columns: new[] { "SourceEntityName", "SourceEntityId", "AlertType", "DueDate" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_SourceEntityName_SourceEntityId_IsDelete",
                schema: "public",
                table: "WfpComplianceAlert",
                columns: new[] { "SourceEntityName", "SourceEntityId", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_WorkforceProfileId",
                schema: "public",
                table: "WfpComplianceAlert",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_WorkforceProfileId_AlertStatus_IsResolve~",
                schema: "public",
                table: "WfpComplianceAlert",
                columns: new[] { "WorkforceProfileId", "AlertStatus", "IsResolved", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlert_WorkforceProfileId_DueDate_AlertStatus_I~",
                schema: "public",
                table: "WfpComplianceAlert",
                columns: new[] { "WorkforceProfileId", "DueDate", "AlertStatus", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_ComplianceAlertId",
                schema: "public",
                table: "WfpComplianceAlertLog",
                column: "ComplianceAlertId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_ComplianceAlertId_LogType_PerformedAt~",
                schema: "public",
                table: "WfpComplianceAlertLog",
                columns: new[] { "ComplianceAlertId", "LogType", "PerformedAt", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_LogType",
                schema: "public",
                table: "WfpComplianceAlertLog",
                column: "LogType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_PerformedAt",
                schema: "public",
                table: "WfpComplianceAlertLog",
                column: "PerformedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_PerformedByUserId",
                schema: "public",
                table: "WfpComplianceAlertLog",
                column: "PerformedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpComplianceAlertLog_PerformedByUserId_PerformedAt_IsDelete",
                schema: "public",
                table: "WfpComplianceAlertLog",
                columns: new[] { "PerformedByUserId", "PerformedAt", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpComplianceAlertLog",
                schema: "public");

            migrationBuilder.DropTable(
                name: "WfpComplianceAlert",
                schema: "public");
        }
    }
}
