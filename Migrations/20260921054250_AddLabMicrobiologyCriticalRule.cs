using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabMicrobiologyCriticalRule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabMicrobiologyCriticalRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrganismId = table.Column<Guid>(type: "uuid", nullable: true),
                    LabAntibioticId = table.Column<Guid>(type: "uuid", nullable: true),
                    SusceptibilityResult = table.Column<int>(type: "integer", nullable: true),
                    RuleNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_LabMicrobiologyCriticalRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabMicrobiologyCriticalRule_LabAntibiotic_LabAntibioticId",
                        column: x => x.LabAntibioticId,
                        principalSchema: "public",
                        principalTable: "LabAntibiotic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabMicrobiologyCriticalRule_LabOrganism_LabOrganismId",
                        column: x => x.LabOrganismId,
                        principalSchema: "public",
                        principalTable: "LabOrganism",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyCriticalRule_IsActive",
                schema: "public",
                table: "LabMicrobiologyCriticalRule",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyCriticalRule_Kombinasi",
                schema: "public",
                table: "LabMicrobiologyCriticalRule",
                columns: new[] { "LabOrganismId", "LabAntibioticId", "SusceptibilityResult" });

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyCriticalRule_LabAntibioticId",
                schema: "public",
                table: "LabMicrobiologyCriticalRule",
                column: "LabAntibioticId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabMicrobiologyCriticalRule",
                schema: "public");
        }
    }
}
