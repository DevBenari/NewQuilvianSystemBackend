using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddBillCollectionPrescriptionHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "BilCollectionHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentAllocationIds = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    PaymentMethodId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaymentMethodAccountId = table.Column<Guid>(type: "uuid", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    KwitansiNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    ProviderReference = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    ProviderEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceInvoiceStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    TenderStatus = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    HandoffKey = table.Column<Guid>(type: "uuid", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "CREATED"),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
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
                    table.PrimaryKey("PK_BilCollectionHandoff", x => x.Id);
                    table.CheckConstraint("CK_BilCollectionHandoff_Amount", "\"Amount\" > 0");
                    table.CheckConstraint("CK_BilCollectionHandoff_Status", "\"Status\" IN ('CREATED','ACKNOWLEDGED')");
                    table.CheckConstraint("CK_BilCollectionHandoff_TenderStatus", "\"TenderStatus\" IN ('SUCCEEDED','REVERSED')");
                    table.ForeignKey(
                        name: "FK_BilCollectionHandoff_BilCashierShift_CashierShiftId",
                        column: x => x.CashierShiftId,
                        principalSchema: "public",
                        principalTable: "BilCashierShift",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCollectionHandoff_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCollectionHandoff_BilSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "BilSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCollectionHandoff_BilTender_TenderId",
                        column: x => x.TenderId,
                        principalSchema: "public",
                        principalTable: "BilTender",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilCollectionHandoff_MstPaymentMethod_PaymentMethodId",
                        column: x => x.PaymentMethodId,
                        principalSchema: "public",
                        principalTable: "MstPaymentMethod",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilPrescriptionClearanceHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PrescriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FinancialOutcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    ReasonCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    FinancialVersion = table.Column<long>(type: "bigint", nullable: false),
                    EffectiveAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "CREATED"),
                    AcknowledgedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RowVersion = table.Column<Guid>(type: "uuid", nullable: false),
                    CreateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdateDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdateBy = table.Column<Guid>(type: "uuid", nullable: false),
                    DeleteDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeleteBy = table.Column<Guid>(type: "uuid", nullable: false),
                    CancelDateTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CancelBy = table.Column<Guid>(type: "uuid", nullable: false),
                    IsCancel = table.Column<bool>(type: "boolean", nullable: false),
                    IsDelete = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BilPrescriptionClearanceHandoff", x => x.Id);
                    table.CheckConstraint("CK_BilPrescriptionClearanceHandoff_ClearanceStatus", "\"ClearanceStatus\" IN ('CLEARED', 'REVOKED')");
                    table.CheckConstraint("CK_BilPrescriptionClearanceHandoff_FinancialOutcome", "\"FinancialOutcome\" IS NULL OR \"FinancialOutcome\" IN ('PAID', 'INSURANCE_APPROVED', 'PAYMENT_WAIVED')");
                    table.CheckConstraint("CK_BilPrescriptionClearanceHandoff_FinancialVersion", "\"FinancialVersion\" > 0");
                    table.CheckConstraint("CK_BilPrescriptionClearanceHandoff_ReasonCode", "\"ReasonCode\" IN ('INVOICE_SETTLED', 'INVOICE_WRITTEN_OFF', 'PRESCRIPTION_CHARGE_INCREASED', 'PAYMENT_REVERSED', 'WRITE_OFF_REVERSED', 'PAYER_COVERAGE_REVERSED')");
                    table.CheckConstraint("CK_BilPrescriptionClearanceHandoff_Status", "\"Status\" IN ('CREATED', 'ACKNOWLEDGED')");
                    table.ForeignKey(
                        name: "FK_BilPrescriptionClearanceHandoff_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_CashierShiftId",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_HandoffKey",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "HandoffKey",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_Invoice",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_OccurredAt",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "OccurredAt");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_PaymentMethodId",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "PaymentMethodId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_SettlementId",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "SettlementId");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_Status",
                schema: "public",
                table: "BilCollectionHandoff",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_BilCollectionHandoff_Tender_Status",
                schema: "public",
                table: "BilCollectionHandoff",
                columns: new[] { "TenderId", "TenderStatus" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilPrescriptionClearanceHandoff_Invoice",
                schema: "public",
                table: "BilPrescriptionClearanceHandoff",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilPrescriptionClearanceHandoff_Prescription_Version",
                schema: "public",
                table: "BilPrescriptionClearanceHandoff",
                columns: new[] { "PrescriptionId", "FinancialVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilPrescriptionClearanceHandoff_Status",
                schema: "public",
                table: "BilPrescriptionClearanceHandoff",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilCollectionHandoff",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilPrescriptionClearanceHandoff",
                schema: "public");
        }
    }
}
