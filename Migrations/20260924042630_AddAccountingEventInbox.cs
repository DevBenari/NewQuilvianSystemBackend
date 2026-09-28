using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingEventInbox : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventKind",
                schema: "public",
                table: "AccEventType",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateTable(
                name: "AccAccountingEvent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: true),
                    EventTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SourceTransactionId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    SourceVersion = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "1"),
                    EventOccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    AccountingDate = table.Column<DateTime>(type: "date", nullable: false),
                    DocumentDate = table.Column<DateTime>(type: "date", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CurrencyCode = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "IDR"),
                    EventStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    HoldReasonCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: true),
                    RawPayload = table.Column<string>(type: "text", nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    IgnoreReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccAccountingEvent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccAccountingEvent_AccEventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalSchema: "public",
                        principalTable: "AccEventType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccAccountingEvent_AccJournal_JournalId",
                        column: x => x.JournalId,
                        principalSchema: "public",
                        principalTable: "AccJournal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccAccountingEvent_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccAccountingEventAttempt",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    AttemptNumber = table.Column<int>(type: "integer", nullable: false),
                    AttemptedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsSuccess = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    FailureMessage = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_AccAccountingEventAttempt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccAccountingEventAttempt_AccAccountingEvent_AccountingEven~",
                        column: x => x.AccountingEventId,
                        principalSchema: "public",
                        principalTable: "AccAccountingEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AccAccountingEventComponent",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingEventId = table.Column<Guid>(type: "uuid", nullable: false),
                    ComponentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_AccAccountingEventComponent", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccAccountingEventComponent_AccAccountingEvent_AccountingEv~",
                        column: x => x.AccountingEventId,
                        principalSchema: "public",
                        principalTable: "AccAccountingEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_AccountingDate",
                schema: "public",
                table: "AccAccountingEvent",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_CorrelationId",
                schema: "public",
                table: "AccAccountingEvent",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_EventNumber",
                schema: "public",
                table: "AccAccountingEvent",
                column: "EventNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_EventTypeId",
                schema: "public",
                table: "AccAccountingEvent",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_JournalId",
                schema: "public",
                table: "AccAccountingEvent",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_LegalEntityId_EventStatus",
                schema: "public",
                table: "AccAccountingEvent",
                columns: new[] { "LegalEntityId", "EventStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEvent_SourceModule_SourceTransactionId_EventTy~",
                schema: "public",
                table: "AccAccountingEvent",
                columns: new[] { "SourceModule", "SourceTransactionId", "EventTypeCode", "SourceVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEventAttempt_AccountingEventId_AttemptNumber",
                schema: "public",
                table: "AccAccountingEventAttempt",
                columns: new[] { "AccountingEventId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingEventComponent_AccountingEventId_ComponentCode",
                schema: "public",
                table: "AccAccountingEventComponent",
                columns: new[] { "AccountingEventId", "ComponentCode" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccAccountingEventAttempt",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccAccountingEventComponent",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccAccountingEvent",
                schema: "public");

            migrationBuilder.DropColumn(
                name: "EventKind",
                schema: "public",
                table: "AccEventType");
        }
    }
}
