using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Modul Finance Saldo Refund Pasien: antrean dan pencairan saldo refund akibat kelebihan
    /// pembayaran deposit pasien (BilRefundableCredit). Menambahkan tabel FinPatientRefundRequest
    /// dengan Foreign Key ke BilRefundableCredit, BilInvoice, dan MstPatient, check constraint
    /// status, nominal, dan aturan maker-checker, serta proteksi rollback (Down) agar tidak
    /// menghapus riwayat transaksi finansial yang telah disetujui atau dibayarkan.
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinancePatientRefundQueue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinPatientRefundRequest",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    RefundableCreditId = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ApprovedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    SubmittedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    PayeeBankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PayeeAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PayeeAccountName = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ExecutionNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FailedReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ExecutedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    IdempotencyKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinPatientRefundRequest", x => x.Id);
                    table.CheckConstraint("CK_FinPatientRefundRequest_Amounts", "\"RequestedAmount\" > 0 AND \"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"RequestedAmount\" AND (\"ApprovedAmount\" IS NULL OR (\"ApprovedAmount\" > 0 AND \"ApprovedAmount\" <= \"RequestedAmount\"))");
                    table.CheckConstraint("CK_FinPatientRefundRequest_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_FinPatientRefundRequest_Status", "\"Status\" IN ('PENDING','APPROVED','REJECTED','CANCELLED','PARTIALLY_PAID','PAID','FAILED')");
                    table.ForeignKey(
                        name: "FK_FinPatientRefundRequest_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPatientRefundRequest_BilRefundableCredit_RefundableCredi~",
                        column: x => x.RefundableCreditId,
                        principalSchema: "public",
                        principalTable: "BilRefundableCredit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPatientRefundRequest_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinPatientRefundRequest_IdempotencyKey",
                schema: "public",
                table: "FinPatientRefundRequest",
                column: "IdempotencyKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinPatientRefundRequest_InvoiceId",
                schema: "public",
                table: "FinPatientRefundRequest",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPatientRefundRequest_PatientId",
                schema: "public",
                table: "FinPatientRefundRequest",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPatientRefundRequest_RefundableCreditId",
                schema: "public",
                table: "FinPatientRefundRequest",
                column: "RefundableCreditId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPatientRefundRequest_Status_SubmittedAt",
                schema: "public",
                table: "FinPatientRefundRequest",
                columns: new[] { "Status", "SubmittedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."FinPatientRefundRequest" WHERE "Status" IN ('APPROVED','PARTIALLY_PAID','PAID') OR "PaidAmount" > 0)
                    THEN
                        RAISE EXCEPTION 'Rollback dibatalkan: terdapat transaksi saldo refund pasien yang sudah disetujui atau dibayarkan.';
                    END IF;
                END $$;
            """);

            migrationBuilder.DropTable(
                name: "FinPatientRefundRequest",
                schema: "public");
        }
    }
}
