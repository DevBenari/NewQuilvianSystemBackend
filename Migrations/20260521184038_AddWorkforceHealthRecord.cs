using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class AddWorkforceHealthRecord : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WfpHealthRecord",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkforceProfileId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequirementCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    HealthRecordType = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    RecordDate = table.Column<DateTime>(type: "date", nullable: false),
                    ResultStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ProviderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    ExpiredDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsFitToWork = table.Column<bool>(type: "boolean", nullable: true),
                    FitToWorkRestrictionNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    IsVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    VerifiedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    VerifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    VerificationNote = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    FilePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FileContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_WfpHealthRecord", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WfpHealthRecord_AspNetUsers_VerifiedByUserId",
                        column: x => x.VerifiedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WfpHealthRecord_MstWorkforceProfile_WorkforceProfileId",
                        column: x => x.WorkforceProfileId,
                        principalSchema: "public",
                        principalTable: "MstWorkforceProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_ExpiredDate_IsVerified_IsActive_IsDelete",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "ExpiredDate", "IsVerified", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_HealthRecordType_ExpiredDate_IsActive_IsDel~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "HealthRecordType", "ExpiredDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_VerifiedByUserId",
                schema: "public",
                table: "WfpHealthRecord",
                column: "VerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId",
                schema: "public",
                table: "WfpHealthRecord",
                column: "WorkforceProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId_ExpiredDate_IsActive_IsD~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "WorkforceProfileId", "ExpiredDate", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId_HealthRecordType_RecordD~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "WorkforceProfileId", "HealthRecordType", "RecordDate", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId_HealthRecordType_ResultS~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "WorkforceProfileId", "HealthRecordType", "ResultStatus", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId_IsFitToWork_IsActive_IsD~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "WorkforceProfileId", "IsFitToWork", "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_WfpHealthRecord_WorkforceProfileId_IsVerified_IsActive_IsDe~",
                schema: "public",
                table: "WfpHealthRecord",
                columns: new[] { "WorkforceProfileId", "IsVerified", "IsActive", "IsDelete" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WfpHealthRecord",
                schema: "public");
        }
    }
}
