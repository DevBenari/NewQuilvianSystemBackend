using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabExamination : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LabExamination",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LabOrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SpecimenId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcedureCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    ProcedureNameSnapshot = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    TariffId = table.Column<Guid>(type: "uuid", nullable: true),
                    TariffCodeSnapshot = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UnitPriceSnapshot = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    ExaminationStatus = table.Column<int>(type: "integer", nullable: false),
                    ChargeEligibleAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Urgency = table.Column<int>(type: "integer", nullable: false),
                    UrgencyMarkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UrgencyMarkedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    IsDuplo = table.Column<bool>(type: "boolean", nullable: false),
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
                    table.PrimaryKey("PK_LabExamination", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LabExamination_LabOrder_LabOrderId",
                        column: x => x.LabOrderId,
                        principalSchema: "public",
                        principalTable: "LabOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabExamination_MstProcedure_ProcedureId",
                        column: x => x.ProcedureId,
                        principalSchema: "public",
                        principalTable: "MstProcedure",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_LabExamination_TrxLabSpecimen_SpecimenId",
                        column: x => x.SpecimenId,
                        principalSchema: "public",
                        principalTable: "TrxLabSpecimen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ChargeEligibleAt",
                schema: "public",
                table: "LabExamination",
                column: "ChargeEligibleAt");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ExaminationStatus",
                schema: "public",
                table: "LabExamination",
                column: "ExaminationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_LabOrderId",
                schema: "public",
                table: "LabExamination",
                column: "LabOrderId");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_ProcedureId",
                schema: "public",
                table: "LabExamination",
                column: "ProcedureId");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_SpecimenId_ProcedureId",
                schema: "public",
                table: "LabExamination",
                columns: new[] { "SpecimenId", "ProcedureId" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_LabExamination_Urgency",
                schema: "public",
                table: "LabExamination",
                column: "Urgency");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LabExamination",
                schema: "public");
        }
    }
}
