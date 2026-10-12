using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menambahkan skema domain Ayat Silang (Cross-Entry / Unidentified Payer Receipt) V2:
    /// - FinCrossEntry: Aggregate root penerimaan uang penjamin belum teridentifikasi rincian piutangnya (relasi 1:1 ke FinReceipt).
    /// - FinCrossEntryTransaction: Mutasi append-only riwayat transaksi Ayat Silang (kredit penerimaan awal dan debit alokasi ke piutang AR).
    /// - FinCrossEntryDocument: Metadata dokumen lampiran pendukung Ayat Silang (bukti transfer/rekening koran).
    /// Saldo tersedia (available balance) tidak memiliki kolom sendiri melainkan authoritative dari FinReceipt.UnallocatedAmount.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceCrossEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. FinCrossEntry
            migrationBuilder.CreateTable(
                name: "FinCrossEntry",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrossEntryNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReferenceNumber = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    InsuranceProviderId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceiptId = table.Column<Guid>(type: "uuid", nullable: false),
                    OriginalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "OPEN"),
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
                    table.PrimaryKey("PK_FinCrossEntry", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinCrossEntry_MstInsuranceProvider_InsuranceProviderId",
                        column: x => x.InsuranceProviderId,
                        principalSchema: "public",
                        principalTable: "MstInsuranceProvider",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinCrossEntry_MstBankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "public",
                        principalTable: "MstBankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinCrossEntry_FinReceipt_ReceiptId",
                        column: x => x.ReceiptId,
                        principalSchema: "public",
                        principalTable: "FinReceipt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_FinCrossEntry_OriginalAmount", "\"OriginalAmount\" > 0");
                    table.CheckConstraint("CK_FinCrossEntry_Status", "\"Status\" IN ('OPEN', 'PARTIALLY_USED', 'FULLY_USED', 'CANCELLED')");
                });

            // 2. FinCrossEntryTransaction
            migrationBuilder.CreateTable(
                name: "FinCrossEntryTransaction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrossEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfterTransaction = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReceiptAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReversalOfTransactionId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_FinCrossEntryTransaction", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinCrossEntryTransaction_FinCrossEntry_CrossEntryId",
                        column: x => x.CrossEntryId,
                        principalSchema: "public",
                        principalTable: "FinCrossEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinCrossEntryTransaction_FinReceiptAllocation_ReceiptAllocationId",
                        column: x => x.ReceiptAllocationId,
                        principalSchema: "public",
                        principalTable: "FinReceiptAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinCrossEntryTransaction_FinCrossEntryTransaction_ReversalOfTransactionId",
                        column: x => x.ReversalOfTransactionId,
                        principalSchema: "public",
                        principalTable: "FinCrossEntryTransaction",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_FinCrossEntryTransaction_Direction", "\"Direction\" IN ('CREDIT', 'DEBIT')");
                    table.CheckConstraint("CK_FinCrossEntryTransaction_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinCrossEntryTransaction_Balance", "\"BalanceAfterTransaction\" >= 0");
                    table.CheckConstraint("CK_FinCrossEntryTransaction_Type", "\"TransactionType\" IN ('INITIAL_RECEIPT', 'AR_ALLOCATION', 'AR_ALLOCATION_REVERSAL')");
                });

            // 3. FinCrossEntryDocument
            migrationBuilder.CreateTable(
                name: "FinCrossEntryDocument",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CrossEntryId = table.Column<Guid>(type: "uuid", nullable: false),
                    DocumentName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    OriginalFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
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
                    table.PrimaryKey("PK_FinCrossEntryDocument", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FinCrossEntryDocument_FinCrossEntry_CrossEntryId",
                        column: x => x.CrossEntryId,
                        principalSchema: "public",
                        principalTable: "FinCrossEntry",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.CheckConstraint("CK_FinCrossEntryDocument_FileSize", "\"FileSize\" > 0");
                });

            // Indexes for FinCrossEntry
            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_CrossEntryNumber_Active",
                schema: "public",
                table: "FinCrossEntry",
                column: "CrossEntryNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_ReceiptId_Active",
                schema: "public",
                table: "FinCrossEntry",
                column: "ReceiptId",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_InsuranceProvider_Date",
                schema: "public",
                table: "FinCrossEntry",
                columns: new[] { "InsuranceProviderId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_BankAccount_Date",
                schema: "public",
                table: "FinCrossEntry",
                columns: new[] { "BankAccountId", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_Status_Date",
                schema: "public",
                table: "FinCrossEntry",
                columns: new[] { "Status", "TransactionDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntry_ReferenceNumber",
                schema: "public",
                table: "FinCrossEntry",
                column: "ReferenceNumber");

            // Indexes for FinCrossEntryTransaction
            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntryTransaction_CrossEntry_OccurredAt",
                schema: "public",
                table: "FinCrossEntryTransaction",
                columns: new[] { "CrossEntryId", "OccurredAt" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntryTransaction_ReceiptAllocationId",
                schema: "public",
                table: "FinCrossEntryTransaction",
                column: "ReceiptAllocationId",
                filter: "\"ReceiptAllocationId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntryTransaction_ReversalOfTransactionId",
                schema: "public",
                table: "FinCrossEntryTransaction",
                column: "ReversalOfTransactionId");

            // Indexes for FinCrossEntryDocument
            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntryDocument_CrossEntry_CreateDate",
                schema: "public",
                table: "FinCrossEntryDocument",
                columns: new[] { "CrossEntryId", "CreateDateTime" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCrossEntryDocument_StoredFileName",
                schema: "public",
                table: "FinCrossEntryDocument",
                column: "StoredFileName",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinCrossEntryDocument",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinCrossEntryTransaction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinCrossEntry",
                schema: "public");
        }
    }
}
