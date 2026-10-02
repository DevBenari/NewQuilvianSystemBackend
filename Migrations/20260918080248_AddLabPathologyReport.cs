using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabPathologyReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabPathologyOrderContext",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitialDiagnosis = table.Column<string>(type: "text", nullable: true),
                    RelevantHistory = table.Column<string>(type: "text", nullable: true),
                    LastMenstrualPeriod = table.Column<DateOnly>(type: "date", nullable: true),
                    ClinicalNote = table.Column<string>(type: "text", nullable: true),
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
                    table.PrimaryKey("PK_LabPathologyOrderContext", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPathologyOrderContext_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabPathologyReport",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    FindingStatus = table.Column<int>(type: "integer", nullable: true),
                    AnalystUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    FinalizedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FinalizedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
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
                    table.PrimaryKey("PK_LabPathologyReport", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPathologyReport_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabPathologyReportValue",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPathologyReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabPathologyParameterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ParameterNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: false),
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
                    table.PrimaryKey("PK_LabPathologyReportValue", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabPathologyReportValue_LabPathologyParameter_LabPathologyP~",
                        column: x => x.LabPathologyParameterId,
                        principalSchema: "public",
                        principalTable: "LabPathologyParameter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabPathologyReportValue_LabPathologyReport_LabPathologyRepo~",
                        column: x => x.LabPathologyReportId,
                        principalSchema: "public",
                        principalTable: "LabPathologyReport",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabPathologyOrderContext_LabOrderId",
                schema: "public",
                table: "LabPathologyOrderContext",
                column: "LabOrderId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabPathologyReport_FinalizedAt",
                schema: "public",
                table: "LabPathologyReport",
                column: "FinalizedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LabPathologyReport_LabOrderId",
                schema: "public",
                table: "LabPathologyReport",
                column: "LabOrderId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabPathologyReportValue_LabPathologyParameterId",
                schema: "public",
                table: "LabPathologyReportValue",
                column: "LabPathologyParameterId");

            migrationBuilder.CreateIndex(
                name: "IX_LabPathologyReportValue_ReportId_ParameterId",
                schema: "public",
                table: "LabPathologyReportValue",
                columns: new[] { "LabPathologyReportId", "LabPathologyParameterId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabPathologyOrderContext",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabPathologyReportValue",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabPathologyReport",
                schema: "public");
        }
    }
}
