using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-020: pembayaran keluar, alokasi utang polimorfik, serta potongan dan tambahan transfer
    /// (FIN-DES-015, FIN-DES-026, FIN-DES-027, FR-FIN-050, FR-FIN-051).
    /// FinPaymentAllocation.MedicalServicePayableId disiapkan tanpa foreign key constraint karena
    /// FinMedicalServicePayable belum dibangun (BE-FIN-021, BLOCKED menunggu modul Medical Fee).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinancePayment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinPayment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PaymentType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "SUPPLIER"),
                    PayeeReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    BankAccountId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethod = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "TRANSFER"),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    DeductionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    AdditionAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    NetTransferAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAFT"),
                    ApprovalTier = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    PaidAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinPayment", x => x.Id);
                    table.CheckConstraint("CK_FinPayment_PaymentType", "\"PaymentType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
                    table.CheckConstraint("CK_FinPayment_Status", "\"Status\" IN ('DRAFT','SUBMITTED','APPROVED','PAID','REJECTED','CANCELLED')");
                    table.CheckConstraint("CK_FinPayment_PaymentMethod", "\"PaymentMethod\" IN ('TRANSFER','CASH','CHEQUE')");
                    table.CheckConstraint("CK_FinPayment_Total", "\"TotalAmount\" > 0");
                    table.CheckConstraint("CK_FinPayment_NetTransfer", "\"NetTransferAmount\" = \"TotalAmount\" - \"DeductionAmount\" + \"AdditionAmount\"");
                    table.CheckConstraint("CK_FinPayment_NetTransferNonNegative", "\"NetTransferAmount\" >= 0");
                    table.CheckConstraint("CK_FinPayment_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_FinPayment_FullyAllocatedWhenPaid", "\"Status\" <> 'PAID' OR \"AllocatedAmount\" = \"TotalAmount\"");
                    table.ForeignKey(
                        name: "FK_FinPayment_MstBankAccount_BankAccountId",
                        column: x => x.BankAccountId,
                        principalSchema: "public",
                        principalTable: "MstBankAccount",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPaymentAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    PayableType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    SupplierPayableId = table.Column<Guid>(type: "uuid", nullable: true),
                    MedicalServicePayableId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    IsReversal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ReversalOfAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinPaymentAllocation", x => x.Id);
                    table.CheckConstraint("CK_FinPaymentAllocation_PayableType", "\"PayableType\" IN ('SUPPLIER','MEDICAL_SERVICE')");
                    table.CheckConstraint("CK_FinPaymentAllocation_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinPaymentAllocation_ExactlyOnePayable", "num_nonnulls(\"SupplierPayableId\", \"MedicalServicePayableId\") = 1");
                    table.CheckConstraint("CK_FinPaymentAllocation_PayableTypeMatch", "(\"PayableType\" = 'SUPPLIER' AND \"SupplierPayableId\" IS NOT NULL) OR (\"PayableType\" = 'MEDICAL_SERVICE' AND \"MedicalServicePayableId\" IS NOT NULL)");
                    table.ForeignKey(
                        name: "FK_FinPaymentAllocation_FinPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "public",
                        principalTable: "FinPayment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPaymentAllocation_FinSupplierPayable_SupplierPayableId",
                        column: x => x.SupplierPayableId,
                        principalSchema: "public",
                        principalTable: "FinSupplierPayable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinPaymentAllocation_FinPaymentAllocation_ReversalOfAllocationId",
                        column: x => x.ReversalOfAllocationId,
                        principalSchema: "public",
                        principalTable: "FinPaymentAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinPaymentDeduction",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: false),
                    DeductionType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Direction = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false, defaultValue: "DEDUCTION"),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_FinPaymentDeduction", x => x.Id);
                    table.CheckConstraint("CK_FinPaymentDeduction_Type", "\"DeductionType\" IN ('PPH21','KASBON','PATIENT_DEBT','SITTING_FEE','KSO','IURAN','OTHER')");
                    table.CheckConstraint("CK_FinPaymentDeduction_Direction", "\"Direction\" IN ('DEDUCTION','ADDITION')");
                    table.CheckConstraint("CK_FinPaymentDeduction_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_FinPaymentDeduction_OtherReason", "\"DeductionType\" <> 'OTHER' OR \"Reason\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_FinPaymentDeduction_FinPayment_PaymentId",
                        column: x => x.PaymentId,
                        principalSchema: "public",
                        principalTable: "FinPayment",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Indexes FinPayment
            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_PaymentNumber",
                schema: "public",
                table: "FinPayment",
                column: "PaymentNumber",
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_Status",
                schema: "public",
                table: "FinPayment",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_PaymentType",
                schema: "public",
                table: "FinPayment",
                column: "PaymentType");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_PayeeReferenceId",
                schema: "public",
                table: "FinPayment",
                column: "PayeeReferenceId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_BankAccountId",
                schema: "public",
                table: "FinPayment",
                column: "BankAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_PaidAt",
                schema: "public",
                table: "FinPayment",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_FinPayment_RequestedBy",
                schema: "public",
                table: "FinPayment",
                column: "RequestedBy");

            // Indexes FinPaymentAllocation
            migrationBuilder.CreateIndex(
                name: "IX_FinPaymentAllocation_PaymentId",
                schema: "public",
                table: "FinPaymentAllocation",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPaymentAllocation_SupplierPayableId",
                schema: "public",
                table: "FinPaymentAllocation",
                column: "SupplierPayableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPaymentAllocation_MedicalServicePayableId",
                schema: "public",
                table: "FinPaymentAllocation",
                column: "MedicalServicePayableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinPaymentAllocation_ReversalOfAllocationId",
                schema: "public",
                table: "FinPaymentAllocation",
                column: "ReversalOfAllocationId");

            // Indexes FinPaymentDeduction
            migrationBuilder.CreateIndex(
                name: "IX_FinPaymentDeduction_Payment_Type",
                schema: "public",
                table: "FinPaymentDeduction",
                columns: new[] { "PaymentId", "DeductionType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinPaymentDeduction",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPaymentAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinPayment",
                schema: "public");
        }
    }
}
