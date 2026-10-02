using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyEncounterReconciliation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EmgEncounterReconciliationRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    CountK1 = table.Column<int>(type: "integer", nullable: false),
                    CountK1Outpatient = table.Column<int>(type: "integer", nullable: false),
                    CountK2 = table.Column<int>(type: "integer", nullable: false),
                    CountK3 = table.Column<int>(type: "integer", nullable: false),
                    CountK4 = table.Column<int>(type: "integer", nullable: false),
                    ExecutedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    ExecutedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReversedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReverseReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_EmgEncounterReconciliationRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmgEncounterReconciliationRun_AspNetUsers_ExecutedByUserId",
                        column: x => x.ExecutedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgEncounterReconciliationRun_AspNetUsers_ReversedByUserId",
                        column: x => x.ReversedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "EmgEncounterReconciliationItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    RunId = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    EmergencyVisitId = table.Column<Guid>(type: "uuid", nullable: false),
                    Class = table.Column<int>(type: "integer", nullable: false),
                    StatusBefore = table.Column<int>(type: "integer", nullable: false),
                    StatusAfter = table.Column<int>(type: "integer", nullable: false),
                    CompletedAtBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAtAfter = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsReversed = table.Column<bool>(type: "boolean", nullable: false),
                    ReverseSkipReason = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
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
                    table.PrimaryKey("PK_EmgEncounterReconciliationItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmgEncounterReconciliationItem_EmgEncounterReconciliationRu~",
                        column: x => x.RunId,
                        principalSchema: "public",
                        principalTable: "EmgEncounterReconciliationRun",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmgEncounterReconciliationItem_EmgVisit_EmergencyVisitId",
                        column: x => x.EmergencyVisitId,
                        principalSchema: "public",
                        principalTable: "EmgVisit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_EmgEncounterReconciliationItem_RegPatientEncounter_Encounte~",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationItem_EmergencyVisitId",
                schema: "public",
                table: "EmgEncounterReconciliationItem",
                column: "EmergencyVisitId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationItem_EncounterId",
                schema: "public",
                table: "EmgEncounterReconciliationItem",
                column: "EncounterId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationItem_RunId_EncounterId",
                schema: "public",
                table: "EmgEncounterReconciliationItem",
                columns: new[] { "RunId", "EncounterId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationRun_ExecutedAt",
                schema: "public",
                table: "EmgEncounterReconciliationRun",
                column: "ExecutedAt");

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationRun_ExecutedByUserId",
                schema: "public",
                table: "EmgEncounterReconciliationRun",
                column: "ExecutedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationRun_ReversedByUserId",
                schema: "public",
                table: "EmgEncounterReconciliationRun",
                column: "ReversedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmgEncounterReconciliationRun_RunNumber",
                schema: "public",
                table: "EmgEncounterReconciliationRun",
                column: "RunNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgEncounterReconciliationRun") THEN
                        RAISE EXCEPTION 'Down BE-IGD-052 dihentikan: ada run rekonsiliasi yang sudah tercatat. Menghapus tabel ini menghilangkan jejak encounter mana yang ditutup beserta nilai sebelumnya, sehingga run tidak dapat dibalik lagi.';
                    END IF;
                END $$;
                """);

            migrationBuilder.DropTable(
                name: "EmgEncounterReconciliationItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "EmgEncounterReconciliationRun",
                schema: "public");
        }
    }
}
