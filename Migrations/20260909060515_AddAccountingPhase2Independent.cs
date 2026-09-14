using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountingPhase2Independent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsControlAccount",
                schema: "public",
                table: "AccChartOfAccount",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClosingSubmittedAt",
                schema: "public",
                table: "AccAccountingPeriod",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ClosingSubmittedBy",
                schema: "public",
                table: "AccAccountingPeriod",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AccAccountingConfiguration",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    RetainedEarningsAccountId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AccAccountingConfiguration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccAccountingConfiguration_AccChartOfAccount_RetainedEarnin~",
                        column: x => x.RetainedEarningsAccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccAccountingConfiguration_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccPeriodClosingApproval",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionSequence = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<int>(type: "integer", nullable: false),
                    ActionBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ActionAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ActionNote = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_AccPeriodClosingApproval", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccPeriodClosingApproval_AccAccountingPeriod_AccountingPeri~",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "public",
                        principalTable: "AccAccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccRecurringJournalTemplate",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateCode = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TemplateName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JournalTypeId = table.Column<Guid>(type: "uuid", nullable: false),
                    Frequency = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    DayOfMonth = table.Column<int>(type: "integer", nullable: false, defaultValue: 1),
                    StartDate = table.Column<DateTime>(type: "date", nullable: false),
                    EndDate = table.Column<DateTime>(type: "date", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
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
                    table.PrimaryKey("PK_AccRecurringJournalTemplate", x => x.Id);
                    table.CheckConstraint("CK_AccRecurringJournalTemplate_DayOfMonth_1_28", "\"DayOfMonth\" >= 1 AND \"DayOfMonth\" <= 28");
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalTemplate_AccJournalType_JournalTypeId",
                        column: x => x.JournalTypeId,
                        principalSchema: "public",
                        principalTable: "AccJournalType",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalTemplate_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccRecurringJournalRun",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    JournalId = table.Column<Guid>(type: "uuid", nullable: false),
                    GeneratedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
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
                    table.PrimaryKey("PK_AccRecurringJournalRun", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalRun_AccAccountingPeriod_AccountingPeriod~",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "public",
                        principalTable: "AccAccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalRun_AccJournal_JournalId",
                        column: x => x.JournalId,
                        principalSchema: "public",
                        principalTable: "AccJournal",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalRun_AccRecurringJournalTemplate_Template~",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "AccRecurringJournalTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AccRecurringJournalTemplateLine",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AccRecurringJournalTemplateLine", x => x.Id);
                    table.CheckConstraint("CK_AccRecurringJournalTemplateLine_TepatSatuSisiTerisi", "(\"DebitAmount\" > 0 AND \"CreditAmount\" = 0) OR (\"DebitAmount\" = 0 AND \"CreditAmount\" > 0)");
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalTemplateLine_AccChartOfAccount_AccountId",
                        column: x => x.AccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalTemplateLine_AccRecurringJournalTemplate~",
                        column: x => x.TemplateId,
                        principalSchema: "public",
                        principalTable: "AccRecurringJournalTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccRecurringJournalTemplateLine_MstCostCenter_CostCenterId",
                        column: x => x.CostCenterId,
                        principalSchema: "public",
                        principalTable: "MstCostCenter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccChartOfAccount_IsControlAccount",
                schema: "public",
                table: "AccChartOfAccount",
                column: "IsControlAccount");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingConfiguration_LegalEntityId",
                schema: "public",
                table: "AccAccountingConfiguration",
                column: "LegalEntityId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccAccountingConfiguration_RetainedEarningsAccountId",
                schema: "public",
                table: "AccAccountingConfiguration",
                column: "RetainedEarningsAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccPeriodClosingApproval_AccountingPeriodId_ActionSequence",
                schema: "public",
                table: "AccPeriodClosingApproval",
                columns: new[] { "AccountingPeriodId", "ActionSequence" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccPeriodClosingApproval_ActionBy",
                schema: "public",
                table: "AccPeriodClosingApproval",
                column: "ActionBy");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalRun_AccountingPeriodId",
                schema: "public",
                table: "AccRecurringJournalRun",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalRun_JournalId",
                schema: "public",
                table: "AccRecurringJournalRun",
                column: "JournalId");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalRun_TemplateId_AccountingPeriodId",
                schema: "public",
                table: "AccRecurringJournalRun",
                columns: new[] { "TemplateId", "AccountingPeriodId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplate_IsActive",
                schema: "public",
                table: "AccRecurringJournalTemplate",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplate_JournalTypeId",
                schema: "public",
                table: "AccRecurringJournalTemplate",
                column: "JournalTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplate_LegalEntityId_TemplateCode",
                schema: "public",
                table: "AccRecurringJournalTemplate",
                columns: new[] { "LegalEntityId", "TemplateCode" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplateLine_AccountId",
                schema: "public",
                table: "AccRecurringJournalTemplateLine",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplateLine_CostCenterId",
                schema: "public",
                table: "AccRecurringJournalTemplateLine",
                column: "CostCenterId");

            migrationBuilder.CreateIndex(
                name: "IX_AccRecurringJournalTemplateLine_TemplateId_LineNumber",
                schema: "public",
                table: "AccRecurringJournalTemplateLine",
                columns: new[] { "TemplateId", "LineNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccAccountingConfiguration",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccPeriodClosingApproval",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccRecurringJournalRun",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccRecurringJournalTemplateLine",
                schema: "public");

            migrationBuilder.DropTable(
                name: "AccRecurringJournalTemplate",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_AccChartOfAccount_IsControlAccount",
                schema: "public",
                table: "AccChartOfAccount");

            migrationBuilder.DropColumn(
                name: "IsControlAccount",
                schema: "public",
                table: "AccChartOfAccount");

            migrationBuilder.DropColumn(
                name: "ClosingSubmittedAt",
                schema: "public",
                table: "AccAccountingPeriod");

            migrationBuilder.DropColumn(
                name: "ClosingSubmittedBy",
                schema: "public",
                table: "AccAccountingPeriod");
        }
    }
}
