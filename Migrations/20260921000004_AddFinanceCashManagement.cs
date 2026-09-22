using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-013. Kas dan setoran bank milik Finance (FIN-DES-018..020, FR-FIN-060..065).
    /// Dua tabel: FinBankDeposit dan FinDailyCashSnapshot.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceCashManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinBankDeposit",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DepositNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DepositDate = table.Column<DateOnly>(type: "date", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    DepositSlipNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinBankDeposit", x => x.Id);
                    table.CheckConstraint("CK_FinBankDeposit_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinBankDeposit_Status", "\"Status\" IN ('DRAFT','POSTED','VERIFIED','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_FinBankDeposit_MstBankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "public",
                        principalTable: "MstBankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinDailyCashSnapshot",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CashDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OpeningBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CashReceiptAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OtherReceiptAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DisbursementAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BankDepositAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    ClosingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OPEN"),
                    ClosedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ClosedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_FinDailyCashSnapshot", x => x.Id);
                    table.CheckConstraint("CK_FinDailyCashSnapshot_Status", "\"Status\" IN ('OPEN','CLOSED')");
                    // Formula kas harian FIN-DEC-020; kas kecil TIDAK ikut
                    table.CheckConstraint("CK_FinDailyCashSnapshot_Formula",
                        "\"ClosingBalance\" = \"OpeningBalance\" + \"CashReceiptAmount\" + \"OtherReceiptAmount\" - \"DisbursementAmount\" - \"BankDepositAmount\"");
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinBankDeposit_BankAccountId",
                schema: "public",
                table: "FinBankDeposit",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinBankDeposit_CashierShiftId",
                schema: "public",
                table: "FinBankDeposit",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_FinBankDeposit_DepositDate",
                schema: "public",
                table: "FinBankDeposit",
                column: "DepositDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinBankDeposit_DepositNumber",
                schema: "public",
                table: "FinBankDeposit",
                column: "DepositNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinBankDeposit_Status",
                schema: "public",
                table: "FinBankDeposit",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinDailyCashSnapshot_CashDate",
                schema: "public",
                table: "FinDailyCashSnapshot",
                column: "CashDate",
                unique: true,
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinBankDeposit",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinDailyCashSnapshot",
                schema: "public");
        }
    }
}
