using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabMicrobiologyIsolateAndSusceptibility : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabMicrobiologyIsolate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabExaminationId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrganismId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrganismNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    IsSusceptibilityTested = table.Column<bool>(type: "boolean", nullable: false),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_LabMicrobiologyIsolate", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabMicrobiologyIsolate_LabExamination_LabExaminationId",
                        column: x => x.LabExaminationId,
                        principalSchema: "public",
                        principalTable: "LabExamination",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabMicrobiologyIsolate_LabOrganism_LabOrganismId",
                        column: x => x.LabOrganismId,
                        principalSchema: "public",
                        principalTable: "LabOrganism",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LabIsolateSusceptibility",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabMicrobiologyIsolateId = table.Column<Guid>(type: "uuid", nullable: false),
                    LabAntibioticId = table.Column<Guid>(type: "uuid", nullable: false),
                    AntibioticNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Concentration = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: true),
                    ConcentrationUnitId = table.Column<Guid>(type: "uuid", nullable: true),
                    DiscContentUgSnapshot = table.Column<int>(type: "integer", nullable: true),
                    BreakpointLowerMmSnapshot = table.Column<int>(type: "integer", nullable: true),
                    BreakpointUpperMmSnapshot = table.Column<int>(type: "integer", nullable: true),
                    ZoneDiameterMm = table.Column<int>(type: "integer", nullable: true),
                    ComputedResult = table.Column<int>(type: "integer", nullable: true),
                    Result = table.Column<int>(type: "integer", nullable: false),
                    IsResultOverridden = table.Column<bool>(type: "boolean", nullable: false),
                    ResultOverrideReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Note = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_LabIsolateSusceptibility", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabIsolateSusceptibility_LabAntibiotic_LabAntibioticId",
                        column: x => x.LabAntibioticId,
                        principalSchema: "public",
                        principalTable: "LabAntibiotic",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabIsolateSusceptibility_LabMicrobiologyIsolate_LabMicrobio~",
                        column: x => x.LabMicrobiologyIsolateId,
                        principalSchema: "public",
                        principalTable: "LabMicrobiologyIsolate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabIsolateSusceptibility_MstMeasurement_ConcentrationUnitId",
                        column: x => x.ConcentrationUnitId,
                        principalSchema: "public",
                        principalTable: "MstMeasurement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabIsolateSusceptibility_ConcentrationUnitId",
                schema: "public",
                table: "LabIsolateSusceptibility",
                column: "ConcentrationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_LabIsolateSusceptibility_IsolateId_AntibioticId",
                schema: "public",
                table: "LabIsolateSusceptibility",
                columns: new[] { "LabMicrobiologyIsolateId", "LabAntibioticId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabIsolateSusceptibility_LabAntibioticId",
                schema: "public",
                table: "LabIsolateSusceptibility",
                column: "LabAntibioticId");

            migrationBuilder.CreateIndex(
                name: "IX_LabIsolateSusceptibility_LabMicrobiologyIsolateId",
                schema: "public",
                table: "LabIsolateSusceptibility",
                column: "LabMicrobiologyIsolateId");

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyIsolate_LabExaminationId",
                schema: "public",
                table: "LabMicrobiologyIsolate",
                column: "LabExaminationId");

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyIsolate_LabExaminationId_LabOrganismId",
                schema: "public",
                table: "LabMicrobiologyIsolate",
                columns: new[] { "LabExaminationId", "LabOrganismId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabMicrobiologyIsolate_LabOrganismId",
                schema: "public",
                table: "LabMicrobiologyIsolate",
                column: "LabOrganismId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabIsolateSusceptibility",
                schema: "public");

            migrationBuilder.DropTable(
                name: "LabMicrobiologyIsolate",
                schema: "public");
        }
    }
}
