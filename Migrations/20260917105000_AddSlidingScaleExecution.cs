using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-123 / migration K7. Pelaksanaan sliding scale milik <c>PharmacyManagement</c> (<c>RWI-DEC-147</c>)
    /// — kamus data 0.4 bagian 11.15. Setelah R6 <c>dokter-rawat-inap</c> (order dan rentang), K4 (dosis MAR),
    /// dan K5 (GDS bangsal). Mundur ditolak bila sudah ada pelaksanaan tercatat.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917105000_AddSlidingScaleExecution")]
    public partial class AddSlidingScaleExecution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PhmSlidingScaleExecution",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutionNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    OrderId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderVersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InpEpisodeId = table.Column<Guid>(type: "uuid", nullable: false),
                    BloodGlucoseReadingId = table.Column<Guid>(type: "uuid", nullable: false),
                    GlucoseValueSnapshot = table.Column<decimal>(type: "numeric(7,2)", nullable: false),
                    GlucoseUnitSnapshot = table.Column<int>(type: "integer", nullable: false),
                    MatchedRangeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComputedDoseUnits = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    MedicationAdministrationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsException = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExceptionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReadingCorrectedAfterExecution = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ExecutedByEmployeeId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExecutionStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
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
                    table.PrimaryKey("PK_PhmSlidingScaleExecution", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_OrderId",
                        column: x => x.OrderId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_OrderVersionId",
                        column: x => x.OrderVersionId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleOrderVersion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_InpEpisodeId",
                        column: x => x.InpEpisodeId,
                        principalSchema: "public",
                        principalTable: "InpEpisode",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_BloodGlucoseReadingId",
                        column: x => x.BloodGlucoseReadingId,
                        principalSchema: "public",
                        principalTable: "CliBloodGlucoseReading",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_MatchedRangeId",
                        column: x => x.MatchedRangeId,
                        principalSchema: "public",
                        principalTable: "PhmSlidingScaleRange",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_MedicationAdministrationId",
                        column: x => x.MedicationAdministrationId,
                        principalSchema: "public",
                        principalTable: "PhmMedicationAdministration",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_ExecutedByEmployeeId",
                        column: x => x.ExecutedByEmployeeId,
                        principalSchema: "public",
                        principalTable: "MstEmployee",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PhmSlidingScaleExecution_ExecutedByUserId",
                        column: x => x.ExecutedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_PhmSlidingScaleExecution_ComputedDose", "\"ComputedDoseUnits\" >= 0");
                    table.CheckConstraint("CK_PhmSlidingScaleExecution_ExceptionReason", "\"IsException\" = false OR \"ExceptionReason\" IS NOT NULL");
                });

            migrationBuilder.CreateIndex(
                name: "UX_PhmSlidingScaleExecution_Number",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "ExecutionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_Order_ExecutedAt",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                columns: new[] { "OrderId", "ExecutedAt" });

            migrationBuilder.CreateIndex(
                name: "UX_PhmSlidingScaleExecution_Reading_Recorded",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "BloodGlucoseReadingId",
                unique: true,
                filter: "\"ExecutionStatus\" = 1");

            migrationBuilder.CreateIndex(
                name: "UX_PhmSlidingScaleExecution_MedicationAdministration",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "MedicationAdministrationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_PhmSlidingScaleExecution_IdempotencyKey",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_OrderVersionId",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "OrderVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_InpEpisodeId",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_MatchedRangeId",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "MatchedRangeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_ExecutedByEmployeeId",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "ExecutedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmSlidingScaleExecution_ExecutedByUserId",
                schema: "public",
                table: "PhmSlidingScaleExecution",
                column: "ExecutedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""PhmSlidingScaleExecution"";
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-123: rollback ditolak. Tabel PhmSlidingScaleExecution berisi % baris; pelaksanaan sliding scale adalah jejak perhitungan dosis insulin pasien.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropTable(
                name: "PhmSlidingScaleExecution",
                schema: "public");
        }
    }
}
