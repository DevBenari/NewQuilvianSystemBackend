using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWfpDisciplinaryAction : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpDisciplinaryAction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IncidentDate = table.Column<DateTime>(type: "date", nullable: false),
                    IssuedDate = table.Column<DateTime>(type: "date", nullable: false),
                    SeverityLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    EffectiveUntil = table.Column<DateTime>(type: "date", nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ActionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
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
                    table.PrimaryKey("PK_WfpDisciplinaryAction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpDisciplinaryAction_AspNetUsers_IssuedByUserId",
                        column: x => x.IssuedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpDisciplinaryAction_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_ActionStatus",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "ActionStatus");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_ActionType",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "ActionType");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_EffectiveUntil_ActionStatus_IsActive_~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "EffectiveUntil", "ActionStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_IncidentDate",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "IncidentDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_IssuedByUserId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "IssuedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_IssuedDate",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "IssuedDate");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_SeverityLevel",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "SeverityLevel");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_WorkforceProfileId",
                schema: "public",
                table: "WfpDisciplinaryAction",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_WorkforceProfileId_ActionStatus_IsAct~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "WorkforceProfileId", "ActionStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_WorkforceProfileId_ActionType_IssuedD~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "WorkforceProfileId", "ActionType", "IssuedDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpDisciplinaryAction_WorkforceProfileId_SeverityLevel_Acti~",
                schema: "public",
                table: "WfpDisciplinaryAction",
                columns: new[] { "WorkforceProfileId", "SeverityLevel", "ActionStatus", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpDisciplinaryAction",
                schema: "public");
        }
    }
}
