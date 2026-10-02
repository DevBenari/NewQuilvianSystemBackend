using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingPostingRuleMaster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AccJournal_ReversalOfJournalId",
                schema: "public",
                table: "AccJournal");

            migrationBuilder.CreateTable(
                name: "AccEventType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    EventTypeName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    SourceModule = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_AccEventType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccPostingRule",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    EventTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Treatment = table.Column<int>(type: "integer", nullable: false, defaultValue: 2),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
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
                    table.PrimaryKey("PK_AccPostingRule", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccPostingRule_AccEventType_EventTypeId",
                        column: x => x.EventTypeId,
                        principalSchema: "public",
                        principalTable: "AccEventType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccPostingRule_AccJournalType_JournalTypeId",
                        column: x => x.JournalTypeId,
                        principalSchema: "public",
                        principalTable: "AccJournalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccPostingRule_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccPostingRuleLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PostingRuleId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    ComponentCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false, defaultValue: "TOTAL"),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Side = table.Column<int>(type: "integer", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccPostingRuleLine", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccPostingRuleLine_AccChartOfAccount_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccPostingRuleLine_AccPostingRule_PostingRuleId",
                        column: x => x.PostingRuleId,
                        principalSchema: "public",
                        principalTable: "AccPostingRule",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccPostingRuleLine_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_ReversalOfJournalId",
                schema: "public",
                table: "AccJournal",
                column: "ReversalOfJournalId",
                unique: true,
                filter: "\"ReversalOfJournalId\" IS NOT NULL AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccEventType_EventTypeCode",
                schema: "public",
                table: "AccEventType",
                column: "EventTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccEventType_EventTypeName",
                schema: "public",
                table: "AccEventType",
                column: "EventTypeName");

            migrationBuilder.CreateIndex(
                name: "IX_AccEventType_IsActive_IsDelete",
                schema: "public",
                table: "AccEventType",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRule_EventTypeId",
                schema: "public",
                table: "AccPostingRule",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRule_JournalTypeId",
                schema: "public",
                table: "AccPostingRule",
                column: "JournalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRule_LegalEntityId_EventTypeId",
                schema: "public",
                table: "AccPostingRule",
                columns: new[] { "LegalEntityId", "EventTypeId" },
                unique: true,
                filter: "\"IsActive\" = true AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRuleLine_AccountId",
                schema: "public",
                table: "AccPostingRuleLine",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRuleLine_ComponentCode",
                schema: "public",
                table: "AccPostingRuleLine",
                column: "ComponentCode");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRuleLine_CostCenterId",
                schema: "public",
                table: "AccPostingRuleLine",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AccPostingRuleLine_PostingRuleId_LineNumber",
                schema: "public",
                table: "AccPostingRuleLine",
                columns: new[] { "PostingRuleId", "LineNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccPostingRuleLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccPostingRule",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccEventType",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_AccJournal_ReversalOfJournalId",
                schema: "public",
                table: "AccJournal");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_ReversalOfJournalId",
                schema: "public",
                table: "AccJournal",
                column: "ReversalOfJournalId");
        }
    }
}
