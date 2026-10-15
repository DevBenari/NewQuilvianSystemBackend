using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounterRescheduleHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegEncounterReschedule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    OldEncounterDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    NewEncounterDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    OldDoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewDoctorScheduleId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewDoctorId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldQueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    NewQueueId = table.Column<Guid>(type: "uuid", nullable: true),
                    OldQueueCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    NewQueueCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    RescheduledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RescheduledByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RegEncounterReschedule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegEncounterReschedule_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReschedule_EncounterId_RescheduledAt",
                schema: "public",
                table: "RegEncounterReschedule",
                columns: new[] { "EncounterId", "RescheduledAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegEncounterReschedule",
                schema: "public");
        }
    }
}
