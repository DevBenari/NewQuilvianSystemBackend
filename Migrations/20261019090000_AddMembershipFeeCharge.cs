using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// crm-membership ke Billing &amp; Kasir: tagihan iuran membership non-kunjungan.
    /// Menambah tabel BilMembershipFeeCharge dan BilMembershipFeeAllocation (tanpa EncounterId), kolom
    /// BilSettlement.MembershipFeeChargeId, serta memperluas check constraint BilSettlement agar menerima
    /// Purpose MEMBERSHIP_FEE dengan tepat satu target (invoice, akun deposit, atau tagihan iuran).
    /// Aditif: tidak ada data yang diubah atau dihapus; baris settlement yang ada tetap memenuhi constraint baru.
    /// </summary>
    /// <inheritdoc />
    public partial class AddMembershipFeeCharge : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_Purpose",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_PurposeTarget",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_Target",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipFeeChargeId",
                schema: "public",
                table: "BilSettlement",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BilMembershipFeeCharge",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChargeNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MembershipId = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<Guid>(type: "uuid", nullable: false),
                    FeeAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    PaymentDueAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    SettledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    MembershipConfirmedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelledAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancellationReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_BilMembershipFeeCharge", x => x.Id);
                    table.CheckConstraint("CK_BilMembershipFeeCharge_FeeAmount", "\"FeeAmount\" > 0");
                    table.CheckConstraint("CK_BilMembershipFeeCharge_PaidAmount", "\"PaidAmount\" >= 0 AND \"PaidAmount\" <= \"FeeAmount\"");
                    table.CheckConstraint("CK_BilMembershipFeeCharge_Status", "\"Status\" IN ('UNPAID','PARTIALLY_PAID','PAID','CANCELLED')");
                    table.ForeignKey(
                        name: "FK_BilMembershipFeeCharge_MstPatient_PatientId",
                        column: x => x.PatientId,
                        principalSchema: "public",
                        principalTable: "MstPatient",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilMembershipFeeCharge_MstPatientMembership_MembershipId",
                        column: x => x.MembershipId,
                        principalSchema: "public",
                        principalTable: "MstPatientMembership",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BilMembershipFeeAllocation",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MembershipFeeChargeId = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    AllocatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ReversesAllocationId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_BilMembershipFeeAllocation", x => x.Id);
                    table.CheckConstraint("CK_BilMembershipFeeAllocation_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_BilMembershipFeeAllocation_BilMembershipFeeAllocation_Rever~",
                        column: x => x.ReversesAllocationId,
                        principalSchema: "public",
                        principalTable: "BilMembershipFeeAllocation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilMembershipFeeAllocation_BilMembershipFeeCharge_Membershi~",
                        column: x => x.MembershipFeeChargeId,
                        principalSchema: "public",
                        principalTable: "BilMembershipFeeCharge",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BilMembershipFeeAllocation_BilSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "BilSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BilSettlement_MembershipFeeChargeId_Status",
                schema: "public",
                table: "BilSettlement",
                columns: new[] { "MembershipFeeChargeId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeCharge_ChargeNumber",
                schema: "public",
                table: "BilMembershipFeeCharge",
                column: "ChargeNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeCharge_MembershipId",
                schema: "public",
                table: "BilMembershipFeeCharge",
                column: "MembershipId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeCharge_PatientId",
                schema: "public",
                table: "BilMembershipFeeCharge",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeCharge_Status_PaymentDueAt",
                schema: "public",
                table: "BilMembershipFeeCharge",
                columns: new[] { "Status", "PaymentDueAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeAllocation_MembershipFeeChargeId_AllocatedAt",
                schema: "public",
                table: "BilMembershipFeeAllocation",
                columns: new[] { "MembershipFeeChargeId", "AllocatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeAllocation_ReversesAllocationId",
                schema: "public",
                table: "BilMembershipFeeAllocation",
                column: "ReversesAllocationId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilMembershipFeeAllocation_SettlementId_AllocatedAt",
                schema: "public",
                table: "BilMembershipFeeAllocation",
                columns: new[] { "SettlementId", "AllocatedAt" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_Purpose",
                schema: "public",
                table: "BilSettlement",
                sql: "\"Purpose\" IN ('DEPOSIT_TOP_UP','INVOICE_PAYMENT','MEMBERSHIP_FEE')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_PurposeTarget",
                schema: "public",
                table: "BilSettlement",
                sql: "((\"Purpose\" = 'INVOICE_PAYMENT' AND \"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL AND \"MembershipFeeChargeId\" IS NULL) OR (\"Purpose\" = 'DEPOSIT_TOP_UP' AND \"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL AND \"MembershipFeeChargeId\" IS NULL) OR (\"Purpose\" = 'MEMBERSHIP_FEE' AND \"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NULL AND \"MembershipFeeChargeId\" IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_Target",
                schema: "public",
                table: "BilSettlement",
                sql: "((\"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL AND \"MembershipFeeChargeId\" IS NULL) OR (\"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL AND \"MembershipFeeChargeId\" IS NULL) OR (\"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NULL AND \"MembershipFeeChargeId\" IS NOT NULL))");

            migrationBuilder.AddForeignKey(
                name: "FK_BilSettlement_BilMembershipFeeCharge_MembershipFeeChargeId",
                schema: "public",
                table: "BilSettlement",
                column: "MembershipFeeChargeId",
                principalSchema: "public",
                principalTable: "BilMembershipFeeCharge",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down hanya aman bila belum ada settlement MEMBERSHIP_FEE: constraint lama menolak baris tersebut.
            migrationBuilder.DropForeignKey(
                name: "FK_BilSettlement_BilMembershipFeeCharge_MembershipFeeChargeId",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropTable(
                name: "BilMembershipFeeAllocation",
                schema: "public");

            migrationBuilder.DropTable(
                name: "BilMembershipFeeCharge",
                schema: "public");

            migrationBuilder.DropIndex(
                name: "IX_BilSettlement_MembershipFeeChargeId_Status",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_Purpose",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_PurposeTarget",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_Target",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropColumn(
                name: "MembershipFeeChargeId",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_Purpose",
                schema: "public",
                table: "BilSettlement",
                sql: "\"Purpose\" IN ('DEPOSIT_TOP_UP','INVOICE_PAYMENT')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_PurposeTarget",
                schema: "public",
                table: "BilSettlement",
                sql: "((\"Purpose\" = 'INVOICE_PAYMENT' AND \"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"Purpose\" = 'DEPOSIT_TOP_UP' AND \"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_Target",
                schema: "public",
                table: "BilSettlement",
                sql: "((\"InvoiceId\" IS NOT NULL AND \"DepositAccountId\" IS NULL) OR (\"InvoiceId\" IS NULL AND \"DepositAccountId\" IS NOT NULL))");
        }
    }
}
