using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-010. Kotak keluar kejadian Finance -> Accounting (FIN-DES-017..019). Hanya dua
    /// tabel (Outbox, Attempt) — FinSubledgerPeriodBalance BUKAN bagian task ini (menunggu
    /// FIN-OQ-011, task terpisah).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceAccountingOutbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinAccountingEventOutbox",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "Finance"),
                    SourceTransactionId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "1"),
                    EventOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AccountingDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "IDR"),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentsJson = table.Column<string>(type: "text", nullable: true),
                    PayloadJson = table.Column<string>(type: "text", nullable: false),
                    DeliveryStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "PENDING"),
                    HoldReason = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LastAttemptAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastResponseCode = table.Column<int>(type: "integer", nullable: true),
                    AccountingReceiptNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccountingJournalNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinAccountingEventOutbox", x => x.Id);
                    // Kontrak Accounting hanya menerima rupiah (aturan bisnis #7).
                    table.CheckConstraint("CK_FinAccountingEventOutbox_Currency", "\"CurrencyCode\" = 'IDR'");
                    table.CheckConstraint("CK_FinAccountingEventOutbox_DeliveryStatus", "\"DeliveryStatus\" IN ('PENDING','HELD_FOR_FINALIZATION','SENT','ACKNOWLEDGED','HELD','FAILED')");
                });

            migrationBuilder.CreateTable(
                name: "FinAccountingEventAttempt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ResponseCode = table.Column<int>(type: "integer", nullable: true),
                    ResponseBody = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    DurationMs = table.Column<int>(type: "integer", nullable: true),
                    ErrorMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_FinAccountingEventAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinAccountingEventAttempt_FinAccountingEventOutbox_OutboxId",
                        column: x => x.OutboxId,
                        principalSchema: "public",
                        principalTable: "FinAccountingEventOutbox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Lapis anti-dobel pertama (FIN-DES-019).
            migrationBuilder.CreateIndex(
                name: "IX_FinAccountingEventOutbox_EventNumber",
                schema: "public",
                table: "FinAccountingEventOutbox",
                column: "EventNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            // Lapis anti-dobel kedua — kontrak ACC-XMOD-0.2 bagian 5.
            migrationBuilder.CreateIndex(
                name: "IX_FinAccountingEventOutbox_SourceIdentity",
                schema: "public",
                table: "FinAccountingEventOutbox",
                columns: new[] { "SourceModule", "SourceTransactionId", "EventTypeCode", "SourceVersion" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinAccountingEventOutbox_DeliveryStatus",
                schema: "public",
                table: "FinAccountingEventOutbox",
                columns: new[] { "DeliveryStatus", "AccountingDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinAccountingEventAttempt_Outbox_Number",
                schema: "public",
                table: "FinAccountingEventAttempt",
                columns: new[] { "OutboxId", "AttemptNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinAccountingEventAttempt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinAccountingEventOutbox",
                schema: "public");
        }
    }
}
