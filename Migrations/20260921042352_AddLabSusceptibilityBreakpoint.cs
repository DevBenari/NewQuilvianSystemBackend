using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabSusceptibilityBreakpoint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DiscContentUg",
                schema: "public",
                table: "LabAntibiotic",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LabSusceptibilityBreakpoint",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrganismId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabAntibioticId = table.Column<Guid>(type: "uuid", nullable: false),
                    LowerMm = table.Column<int>(type: "integer", nullable: false),
                    UpperMm = table.Column<int>(type: "integer", nullable: false),
                    GuidelineVersion = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_LabSusceptibilityBreakpoint", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabSusceptibilityBreakpoint_LabAntibiotic_LabAntibioticId",
                        column: x => x.LabAntibioticId,
                        principalSchema: "public",
                        principalTable: "LabAntibiotic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabSusceptibilityBreakpoint_LabOrganism_LabOrganismId",
                        column: x => x.LabOrganismId,
                        principalSchema: "public",
                        principalTable: "LabOrganism",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabSusceptibilityBreakpoint_IsActive",
                schema: "public",
                table: "LabSusceptibilityBreakpoint",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LabSusceptibilityBreakpoint_LabAntibioticId",
                schema: "public",
                table: "LabSusceptibilityBreakpoint",
                column: "LabAntibioticId");

            migrationBuilder.CreateIndex(
                name: "IX_LabSusceptibilityBreakpoint_OrganismId_AntibioticId",
                schema: "public",
                table: "LabSusceptibilityBreakpoint",
                columns: new[] { "LabOrganismId", "LabAntibioticId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabSusceptibilityBreakpoint",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "DiscContentUg",
                schema: "public",
                table: "LabAntibiotic");
        }
    }
}
