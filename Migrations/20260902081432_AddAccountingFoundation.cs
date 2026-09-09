using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccAccountingPeriod",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    PeriodCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    FiscalYear = table.Column<int>(type: "integer", nullable: false),
                    PeriodMonth = table.Column<int>(type: "integer", nullable: false),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: false),
                    PeriodStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    ClosedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReopenedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ReopenedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastReasonNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccAccountingPeriod", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccAccountingPeriod_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccChartOfAccount",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountCode = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AccountName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    AccountLevel = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    AccountType = table.Column<int>(type: "integer", nullable: false),
                    NormalBalance = table.Column<int>(type: "integer", nullable: false),
                    IsPostable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    EffectiveStartDate = table.Column<DateTime>(type: "date", nullable: true),
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
                    table.PrimaryKey("PK_AccChartOfAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccChartOfAccount_AccChartOfAccount_ParentAccountId",
                        column: x => x.ParentAccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccChartOfAccount_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccJournalType",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalTypeCode = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    JournalTypeName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumberPrefix = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    RequiresApproval = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    IsSystemType = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_AccJournalType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccNumberSeries",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SequenceKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ScopeKey = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResetPolicy = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CurrentValue = table.Column<long>(type: "bigint", nullable: false),
                    LastAllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AccNumberSeries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AccJournal",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalNumber = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    JournalTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    DocumentDate = table.Column<DateTime>(type: "date", nullable: true),
                    AccountingDate = table.Column<DateTime>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    JournalStatus = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    TotalDebit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    TotalCredit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    SubmittedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReversalOfJournalId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrectionType = table.Column<int>(type: "integer", nullable: true),
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
                    table.PrimaryKey("PK_AccJournal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccJournal_AccAccountingPeriod_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "public",
                        principalTable: "AccAccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccJournal_AccJournalType_JournalTypeId",
                        column: x => x.JournalTypeId,
                        principalSchema: "public",
                        principalTable: "AccJournalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccJournal_AccJournal_ReversalOfJournalId",
                        column: x => x.ReversalOfJournalId,
                        principalSchema: "public",
                        principalTable: "AccJournal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccJournal_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccJournalApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovalAction = table.Column<int>(type: "integer", nullable: false),
                    ActionBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccJournalApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccJournalApproval_AccJournal_JournalId",
                        column: x => x.JournalId,
                        principalSchema: "public",
                        principalTable: "AccJournal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccJournalLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    LineNumber = table.Column<int>(type: "integer", nullable: false),
                    AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostCenterId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    DebitAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CreditAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
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
                    table.PrimaryKey("PK_AccJournalLine", x => x.Id);
                    table.CheckConstraint("CK_AccJournalLine_TepatSatuSisiTerisi", "(\"DebitAmount\" > 0 AND \"CreditAmount\" = 0) OR (\"DebitAmount\" = 0 AND \"CreditAmount\" > 0)");
                    table.ForeignKey(
                        name: "FK_AccJournalLine_AccChartOfAccount_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccJournalLine_AccJournal_JournalId",
                        column: x => x.JournalId,
                        principalSchema: "public",
                        principalTable: "AccJournal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccJournalLine_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingPeriod_FiscalYear",
                schema: "public",
                table: "AccAccountingPeriod",
                column: "FiscalYear");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingPeriod_LegalEntityId_PeriodCode",
                schema: "public",
                table: "AccAccountingPeriod",
                columns: new[] { "LegalEntityId", "PeriodCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingPeriod_PeriodStatus",
                schema: "public",
                table: "AccAccountingPeriod",
                column: "PeriodStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AccChartOfAccount_AccountType",
                schema: "public",
                table: "AccChartOfAccount",
                column: "AccountType");

            migrationBuilder.CreateIndex(
                name: "IX_AccChartOfAccount_LegalEntityId_AccountCode",
                schema: "public",
                table: "AccChartOfAccount",
                columns: new[] { "LegalEntityId", "AccountCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccChartOfAccount_LegalEntityId_AccountName",
                schema: "public",
                table: "AccChartOfAccount",
                columns: new[] { "LegalEntityId", "AccountName" });

            migrationBuilder.CreateIndex(
                name: "IX_AccChartOfAccount_ParentAccountId",
                schema: "public",
                table: "AccChartOfAccount",
                column: "ParentAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_AccountingDate",
                schema: "public",
                table: "AccJournal",
                column: "AccountingDate");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_AccountingPeriodId",
                schema: "public",
                table: "AccJournal",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_JournalStatus",
                schema: "public",
                table: "AccJournal",
                column: "JournalStatus");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_JournalTypeId",
                schema: "public",
                table: "AccJournal",
                column: "JournalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_LegalEntityId_JournalNumber",
                schema: "public",
                table: "AccJournal",
                columns: new[] { "LegalEntityId", "JournalNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournal_ReversalOfJournalId",
                schema: "public",
                table: "AccJournal",
                column: "ReversalOfJournalId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalApproval_ActionBy",
                schema: "public",
                table: "AccJournalApproval",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalApproval_JournalId",
                schema: "public",
                table: "AccJournalApproval",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalLine_AccountId",
                schema: "public",
                table: "AccJournalLine",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalLine_CostCenterId",
                schema: "public",
                table: "AccJournalLine",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalLine_JournalId_LineNumber",
                schema: "public",
                table: "AccJournalLine",
                columns: new[] { "JournalId", "LineNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalType_IsActive_IsDelete",
                schema: "public",
                table: "AccJournalType",
                columns: new[] { "IsActive", "IsDelete" });

            migrationBuilder.CreateIndex(
                name: "IX_AccJournalType_JournalTypeCode",
                schema: "public",
                table: "AccJournalType",
                column: "JournalTypeCode",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccNumberSeries_SequenceKey_ScopeKey",
                schema: "public",
                table: "AccNumberSeries",
                columns: new[] { "SequenceKey", "ScopeKey" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccJournalApproval",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccJournalLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccNumberSeries",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccChartOfAccount",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccJournal",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccAccountingPeriod",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccJournalType",
                schema: "public");
        }
    }
}
