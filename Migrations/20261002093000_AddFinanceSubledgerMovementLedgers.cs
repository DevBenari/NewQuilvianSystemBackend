using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-FIN-058 (FR-FIN-130..132; FIN-DEC-123, 116; FIN-DES-079, 082; DDL R14.1..R14.3).
    /// Membuat tiga skema buku mutasi subledger:
    /// 1. FinReceivableMovement: buku mutasi saldo piutang
    /// 2. FinSupplierPayableMovement: buku mutasi saldo utang supplier
    /// 3. FinCashMovement: buku mutasi kas operasional kasir
    /// Murni tabel baru, belum dieksekusi ke database (menunggu eksekusi oleh Yasmin sesuai FIN-DEC-138).
    /// </summary>
    /// <inheritdoc />
    public partial class AddFinanceSubledgerMovementLedgers : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinReceivableMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FundingSourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FundingSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProofId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpeningItemBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinReceivableMovement", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableMovement_Balance", "\"BalanceAfter\" = \"BalanceBefore\" + \"Amount\"");
                    table.CheckConstraint("CK_FinReceivableMovement_FundingSource", "\"PaymentMethodCode\" IS NULL OR \"FundingSourceType\" IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_FinReceivableMovement_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinSupplierPayableMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierPayableId = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceBefore = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PaymentId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FundingSourceType = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    FundingSourceId = table.Column<Guid>(type: "uuid", nullable: true),
                    ReferenceNumber = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProofId = table.Column<Guid>(type: "uuid", nullable: true),
                    OpeningItemBatchId = table.Column<Guid>(type: "uuid", nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinSupplierPayableMovement", x => x.Id);
                    table.CheckConstraint("CK_FinSupplierPayableMovement_Balance", "\"BalanceAfter\" = \"BalanceBefore\" + \"Amount\"");
                    table.ForeignKey(
                        name: "FK_FinSupplierPayableMovement_FinSupplierPayable_SupplierPayableId",
                        column: x => x.SupplierPayableId,
                        principalSchema: "public",
                        principalTable: "FinSupplierPayable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinCashMovement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MovementType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Direction = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BusinessDate = table.Column<DateOnly>(type: "date", nullable: false),
                    OccurredAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SourceReferenceType = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    SourceReferenceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CashierShiftId = table.Column<Guid>(type: "uuid", nullable: true),
                    PaymentMethodCode = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Notes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: false),
                    CausationId = table.Column<Guid>(type: "uuid", nullable: false),
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
                    table.PrimaryKey("PK_FinCashMovement", x => x.Id);
                    table.CheckConstraint("CK_FinCashMovement_Direction", "\"Direction\" IN ('IN','OUT')");
                    table.CheckConstraint("CK_FinCashMovement_Amount", "\"Amount\" > 0");
                });

            // Index FinReceivableMovement
            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_ReceivableId_BusinessDate",
                schema: "public",
                table: "FinReceivableMovement",
                columns: new[] { "ReceivableId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_BusinessDate",
                schema: "public",
                table: "FinReceivableMovement",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_SourceAllocationId",
                schema: "public",
                table: "FinReceivableMovement",
                column: "SourceAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_PaymentMethodCode",
                schema: "public",
                table: "FinReceivableMovement",
                column: "PaymentMethodCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_OpeningItemBatchId",
                schema: "public",
                table: "FinReceivableMovement",
                column: "OpeningItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_CorrelationId",
                schema: "public",
                table: "FinReceivableMovement",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableMovement_ProofId",
                schema: "public",
                table: "FinReceivableMovement",
                column: "ProofId",
                unique: true,
                filter: "\"ProofId\" IS NOT NULL AND \"IsDelete\" = false");

            // Index FinSupplierPayableMovement
            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_SupplierPayableId_BusinessDate",
                schema: "public",
                table: "FinSupplierPayableMovement",
                columns: new[] { "SupplierPayableId", "BusinessDate" });

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_BusinessDate",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "BusinessDate");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_PaymentId",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "PaymentId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_PaymentAllocationId",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "PaymentAllocationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_PaymentMethodCode",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "PaymentMethodCode");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_OpeningItemBatchId",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "OpeningItemBatchId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_CorrelationId",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_FinSupplierPayableMovement_ProofId",
                schema: "public",
                table: "FinSupplierPayableMovement",
                column: "ProofId",
                unique: true,
                filter: "\"ProofId\" IS NOT NULL AND \"IsDelete\" = false");

            // Index FinCashMovement
            migrationBuilder.CreateIndex(
                name: "IX_FinCashMovement_Source",
                schema: "public",
                table: "FinCashMovement",
                columns: new[] { "SourceReferenceType", "SourceReferenceId", "MovementType" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_FinCashMovement_BusinessDate_Direction",
                schema: "public",
                table: "FinCashMovement",
                columns: new[] { "BusinessDate", "Direction" });

            migrationBuilder.CreateIndex(
                name: "IX_FinCashMovement_CashierShiftId",
                schema: "public",
                table: "FinCashMovement",
                column: "CashierShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_FinCashMovement_CorrelationId",
                schema: "public",
                table: "FinCashMovement",
                column: "CorrelationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinCashMovement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinSupplierPayableMovement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableMovement",
                schema: "public");
        }
    }
}
