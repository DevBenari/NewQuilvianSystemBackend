using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabDisciplineSettingAndReportNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LabReportNumber",
                schema: "public",
                table: "LabOrder",
                type: "character varying(32)",
                maxLength: 32,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabDisciplineSetting",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Discipline = table.Column<int>(type: "integer", nullable: false),
                    ConsultantLabel = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    ConsultantName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    StandingNote = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    ReportNumberPrefix = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_LabDisciplineSetting", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_Discipline_LabReportNumber",
                schema: "public",
                table: "LabOrder",
                columns: new[] { "Discipline", "LabReportNumber" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"LabReportNumber\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_LabDisciplineSetting_Discipline",
                schema: "public",
                table: "LabDisciplineSetting",
                column: "Discipline",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabDisciplineSetting",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_Discipline_LabReportNumber",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "LabReportNumber",
                schema: "public",
                table: "LabOrder");
        }
    }
}
