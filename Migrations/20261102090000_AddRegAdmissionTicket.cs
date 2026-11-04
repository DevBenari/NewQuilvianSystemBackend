using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// rawat-jalan Cetak Antrean Admisi v1.1 — tiket antrean admisi anonim dari kiosk beserta
    /// riwayatnya. Tidak menyentuh <c>RegQueue</c>: nomor antrean poliklinik internal tetap
    /// dibentuk mekanisme existing saat kunjungan disimpan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddRegAdmissionTicket : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RegAdmissionTicket",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ServiceDate = table.Column<DateTime>(type: "date", nullable: false),
                    QueueGroupCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    QueueGroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Prefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    SequenceNumber = table.Column<int>(type: "integer", nullable: false),
                    DisplayNumber = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IssuedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IssuedByUserId = table.Column<Guid>(type: "uuid", nullable: false),
                    IdempotencyKey = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PrintStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    PrintAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastPrintedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PrintFailureReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CounterCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    CounterName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CalledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CalledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CallAttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ServiceStartedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceStartedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    ServiceEndedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ServiceEndedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkipCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    SkippedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SkippedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    SkipReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelledByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: true),
                    CompletionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    RegistrationLinkedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_RegAdmissionTicket", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegAdmissionTicket_RegPatientEncounter_EncounterId",
                        column: x => x.EncounterId,
                        principalSchema: "public",
                        principalTable: "RegPatientEncounter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RegAdmissionTicketHistory",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AdmissionTicketId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FromStatus = table.Column<int>(type: "integer", nullable: true),
                    ToStatus = table.Column<int>(type: "integer", nullable: false),
                    ActorUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CounterCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: true),
                    OccurredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_RegAdmissionTicketHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegAdmissionTicketHistory_RegAdmissionTicket_AdmissionTicke~",
                        column: x => x.AdmissionTicketId,
                        principalSchema: "public",
                        principalTable: "RegAdmissionTicket",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RegAdmissionTicket_EncounterId",
                schema: "public",
                table: "RegAdmissionTicket",
                column: "EncounterId",
                unique: true,
                filter: "\"EncounterId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RegAdmissionTicket_ServiceDate_QueueGroupCode_Status",
                schema: "public",
                table: "RegAdmissionTicket",
                columns: new[] { "ServiceDate", "QueueGroupCode", "Status" });

            migrationBuilder.CreateIndex(
                name: "UX_RegAdmissionTicket_Issuer_IdempotencyKey",
                schema: "public",
                table: "RegAdmissionTicket",
                columns: new[] { "IssuedByUserId", "IdempotencyKey" },
                unique: true,
                filter: "\"IdempotencyKey\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_RegAdmissionTicket_ServiceDate_Group_Sequence",
                schema: "public",
                table: "RegAdmissionTicket",
                columns: new[] { "ServiceDate", "QueueGroupCode", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegAdmissionTicketHistory_AdmissionTicketId_OccurredAt",
                schema: "public",
                table: "RegAdmissionTicketHistory",
                columns: new[] { "AdmissionTicketId", "OccurredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegAdmissionTicketHistory",
                schema: "public");

            migrationBuilder.DropTable(
                name: "RegAdmissionTicket",
                schema: "public");
        }
    }
}
