using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadReportTemplateAndFilmPrint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PrintFilm",
                schema: "public",
                table: "RadOrder",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "RadReportTemplate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ModalityId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: true),
                    Findings = table.Column<string>(type: "text", nullable: true),
                    Impression = table.Column<string>(type: "text", nullable: false),
                    Recommendation = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SortOrder = table.Column<int>(type: "integer", nullable: false),
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
                    table.PrimaryKey("PK_RadReportTemplate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RadReportTemplate_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RadReportTemplate_MstRadModality_ModalityId",
                        column: x => x.ModalityId,
                        principalSchema: "public",
                        principalTable: "MstRadModality",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RadReportTemplate_IsActive_ModalityId_ProcedureId",
                schema: "public",
                table: "RadReportTemplate",
                columns: new[] { "IsActive", "ModalityId", "ProcedureId" });

            migrationBuilder.CreateIndex(
                name: "IX_RadReportTemplate_ModalityId",
                schema: "public",
                table: "RadReportTemplate",
                column: "ModalityId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReportTemplate_ProcedureId",
                schema: "public",
                table: "RadReportTemplate",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_RadReportTemplate_TemplateCode",
                schema: "public",
                table: "RadReportTemplate",
                column: "TemplateCode",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RadReportTemplate",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "PrintFilm",
                schema: "public",
                table: "RadOrder");
        }
    }
}
