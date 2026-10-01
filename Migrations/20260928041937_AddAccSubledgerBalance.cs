using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccSubledgerBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccSubledgerBalance",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    LegalEntityId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountingPeriodId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChartOfAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AsOfDate = table.Column<DateTime>(type: "date", nullable: false),
                    SourceVersionNumber = table.Column<int>(type: "integer", nullable: false),
                    AccountingEventId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_AccSubledgerBalance", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccSubledgerBalance_AccAccountingEvent_AccountingEventId",
                        column: x => x.AccountingEventId,
                        principalSchema: "public",
                        principalTable: "AccAccountingEvent",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccSubledgerBalance_AccAccountingPeriod_AccountingPeriodId",
                        column: x => x.AccountingPeriodId,
                        principalSchema: "public",
                        principalTable: "AccAccountingPeriod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccSubledgerBalance_AccChartOfAccount_ChartOfAccountId",
                        column: x => x.ChartOfAccountId,
                        principalSchema: "public",
                        principalTable: "AccChartOfAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AccSubledgerBalance_MstLegalEntity_LegalEntityId",
                        column: x => x.LegalEntityId,
                        principalSchema: "public",
                        principalTable: "MstLegalEntity",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccSubledgerBalance_AccountingEventId",
                schema: "public",
                table: "AccSubledgerBalance",
                column: "AccountingEventId");

            migrationBuilder.CreateIndex(
                name: "IX_AccSubledgerBalance_AccountingPeriodId",
                schema: "public",
                table: "AccSubledgerBalance",
                column: "AccountingPeriodId");

            migrationBuilder.CreateIndex(
                name: "IX_AccSubledgerBalance_ChartOfAccountId",
                schema: "public",
                table: "AccSubledgerBalance",
                column: "ChartOfAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_AccSubledgerBalance_LegalEntityId_AccountingPeriodId_ChartO~",
                schema: "public",
                table: "AccSubledgerBalance",
                columns: new[] { "LegalEntityId", "AccountingPeriodId", "ChartOfAccountId" },
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccSubledgerBalance",
                schema: "public");
        }
    }
}
