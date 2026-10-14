using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddFinanceReceivableInstallmentAndBenefitSettlement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FinBenefitSettlement",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    AccountingPeriodCode = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    DebtorReferenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ItemCount = table.Column<int>(type: "integer", nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DRAF"),
                    PostedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    PostedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AccountingEventId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinBenefitSettlement", x => x.Id);
                    table.CheckConstraint("CK_FinBenefitSettlement_Posted", "(\"Status\" = 'DITERBITKAN' AND \"PostedBy\" IS NOT NULL AND \"PostedAt\" IS NOT NULL) OR \"Status\" <> 'DITERBITKAN'");
                    table.CheckConstraint("CK_FinBenefitSettlement_Status", "\"Status\" IN ('DRAF','DITERBITKAN','DIBATALKAN')");
                    table.CheckConstraint("CK_FinBenefitSettlement_Total", "\"TotalAmount\" > 0 AND \"ItemCount\" > 0");
                    table.ForeignKey(
                        name: "FK_FinBenefitSettlement_FinAccountingEventOutbox_AccountingEve~",
                        column: x => x.AccountingEventId,
                        principalSchema: "public",
                        principalTable: "FinAccountingEventOutbox",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableInstallmentPlan",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentCount = table.Column<int>(type: "integer", nullable: false),
                    InstallmentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAgreedAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FirstDeductionPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "MENUNGGU"),
                    AgreementDocumentPath = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    RequestedBy = table.Column<Guid>(type: "uuid", nullable: false),
                    RequestedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ApprovedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ApprovedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    RejectedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    RejectionReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CancelReason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableInstallmentPlan", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableInstallmentPlan_Amount", "\"InstallmentCount\" > 0 AND \"InstallmentAmount\" > 0");
                    table.CheckConstraint("CK_FinReceivableInstallmentPlan_MakerChecker", "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
                    table.CheckConstraint("CK_FinReceivableInstallmentPlan_Status", "\"Status\" IN ('MENUNGGU','DISETUJUI','DITOLAK','SELESAI','DIBATALKAN')");
                    table.CheckConstraint("CK_FinReceivableInstallmentPlan_Total", "\"TotalAgreedAmount\" = \"InstallmentCount\" * \"InstallmentAmount\"");
                    table.ForeignKey(
                        name: "FK_FinReceivableInstallmentPlan_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinBenefitSettlementItem",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SettlementId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReceivableId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_FinBenefitSettlementItem", x => x.Id);
                    table.CheckConstraint("CK_FinBenefitSettlementItem_Amount", "\"Amount\" > 0");
                    table.ForeignKey(
                        name: "FK_FinBenefitSettlementItem_FinBenefitSettlement_SettlementId",
                        column: x => x.SettlementId,
                        principalSchema: "public",
                        principalTable: "FinBenefitSettlement",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_FinBenefitSettlementItem_FinReceivable_ReceivableId",
                        column: x => x.ReceivableId,
                        principalSchema: "public",
                        principalTable: "FinReceivable",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FinReceivableInstallment",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PlanId = table.Column<Guid>(type: "uuid", nullable: false),
                    InstallmentNumber = table.Column<int>(type: "integer", nullable: false),
                    DeductionPeriod = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    ScheduledAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CarriedOverAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    PaidAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    OutstandingAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false, defaultValue: "DIJADWALKAN"),
                    LastResultAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LastPayrollPeriodId = table.Column<Guid>(type: "uuid", nullable: true),
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
                    table.PrimaryKey("PK_FinReceivableInstallment", x => x.Id);
                    table.CheckConstraint("CK_FinReceivableInstallment_Amount", "\"ScheduledAmount\" > 0 AND \"CarriedOverAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceivableInstallment_Balance", "\"OutstandingAmount\" = \"ScheduledAmount\" + \"CarriedOverAmount\" - \"PaidAmount\" AND \"OutstandingAmount\" >= 0");
                    table.CheckConstraint("CK_FinReceivableInstallment_Status", "\"Status\" IN ('DIJADWALKAN','TERBAYAR_SEBAGIAN','TERBAYAR','TERTUNGGAK','DIBATALKAN')");
                    table.ForeignKey(
                        name: "FK_FinReceivableInstallment_FinReceivableInstallmentPlan_PlanId",
                        column: x => x.PlanId,
                        principalSchema: "public",
                        principalTable: "FinReceivableInstallmentPlan",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlement_AccountingEventId",
                schema: "public",
                table: "FinBenefitSettlement",
                column: "AccountingEventId");

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlement_Period_Debtor_NotCancelled",
                schema: "public",
                table: "FinBenefitSettlement",
                columns: new[] { "AccountingPeriodCode", "DebtorReferenceId" },
                unique: true,
                filter: "\"Status\" <> 'DIBATALKAN'");

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlement_SettlementNumber",
                schema: "public",
                table: "FinBenefitSettlement",
                column: "SettlementNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlement_Status_AccountingPeriodCode",
                schema: "public",
                table: "FinBenefitSettlement",
                columns: new[] { "Status", "AccountingPeriodCode" });

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlementItem_ReceivableId",
                schema: "public",
                table: "FinBenefitSettlementItem",
                column: "ReceivableId");

            migrationBuilder.CreateIndex(
                name: "IX_FinBenefitSettlementItem_SettlementId_ReceivableId",
                schema: "public",
                table: "FinBenefitSettlementItem",
                columns: new[] { "SettlementId", "ReceivableId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallment_DeductionPeriod_Status",
                schema: "public",
                table: "FinReceivableInstallment",
                columns: new[] { "DeductionPeriod", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallment_PlanId_InstallmentNumber",
                schema: "public",
                table: "FinReceivableInstallment",
                columns: new[] { "PlanId", "InstallmentNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentPlan_PlanNumber",
                schema: "public",
                table: "FinReceivableInstallmentPlan",
                column: "PlanNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentPlan_ReceivableId_Active",
                schema: "public",
                table: "FinReceivableInstallmentPlan",
                column: "ReceivableId",
                unique: true,
                filter: "\"Status\" IN ('MENUNGGU','DISETUJUI')");

            migrationBuilder.CreateIndex(
                name: "IX_FinReceivableInstallmentPlan_Status_RequestedAt",
                schema: "public",
                table: "FinReceivableInstallmentPlan",
                columns: new[] { "Status", "RequestedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FinBenefitSettlementItem",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableInstallment",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinBenefitSettlement",
                schema: "public");

            migrationBuilder.DropTable(
                name: "FinReceivableInstallmentPlan",
                schema: "public");
        }
    }
}
