using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RadReport",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadStudyId = table.Column<Guid>(type: "uuid", nullable: false),
                    RadOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReportNumber = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    ReportStatus = table.Column<int>(type: "integer", nullable: false),
                    CurrentVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    FirstReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadReport_RadOrder_RadOrderId",
                        column: x => x.RadOrderId,
                        principalSchema: "public",
                        principalTable: "RadOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadReport_RadStudy_RadStudyId",
                        column: x => x.RadStudyId,
                        principalSchema: "public",
                        principalTable: "RadStudy",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RadReportVersion",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RadReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    VersionNumber = table.Column<int>(type: "integer", nullable: false),
                    PreviousVersionId = table.Column<Guid>(type: "uuid", nullable: true),
                    VersionStatus = table.Column<int>(type: "integer", nullable: false),
                    IsAmendment = table.Column<bool>(type: "boolean", nullable: false),
                    Findings = table.Column<string>(type: "character varying(8000)", maxLength: 8000, nullable: true),
                    Impression = table.Column<string>(type: "character varying(4000)", maxLength: 4000, nullable: false),
                    Recommendation = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    AuthorUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AuthorRoleSnapshot = table.Column<int>(type: "integer", nullable: false),
                    DraftedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ValidatorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ValidatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReleasedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AmendmentReason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RadReportVersion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadReportVersion_RadReportVersion_PreviousVersionId",
                        column: x => x.PreviousVersionId,
                        principalSchema: "public",
                        principalTable: "RadReportVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadReportVersion_RadReport_RadReportId",
                        column: x => x.RadReportId,
                        principalSchema: "public",
                        principalTable: "RadReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RadReport_EncounterId",
                schema: "public",
                table: "RadReport",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReport_RadOrderId",
                schema: "public",
                table: "RadReport",
                column: "RadOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReport_RadStudyId",
                schema: "public",
                table: "RadReport",
                column: "RadStudyId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RadReport_ReportNumber",
                schema: "public",
                table: "RadReport",
                column: "ReportNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_RadReport_ReportStatus",
                schema: "public",
                table: "RadReport",
                column: "ReportStatus");

            migrationBuilder.CreateIndex(
                name: "IX_RadReportVersion_AuthorUserId",
                schema: "public",
                table: "RadReportVersion",
                column: "AuthorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReportVersion_PreviousVersionId",
                schema: "public",
                table: "RadReportVersion",
                column: "PreviousVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReportVersion_RadReportId_VersionNumber",
                schema: "public",
                table: "RadReportVersion",
                columns: new[] { "RadReportId", "VersionNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RadReportVersion",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RadReport",
                schema: "public");
        }
    }
}
