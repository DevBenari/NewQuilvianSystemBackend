using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddInpatientBillingIntegrationAndClearanceHandoff : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CalculationType",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "FLAT");

            migrationBuilder.AddColumn<decimal>(
                name: "CapAmount",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Percentage",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                type: "numeric(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BilInpatientClearanceHandoff",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EncounterId = table.Column<Guid>(type: "uuid", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClearanceStatus = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FinancialOutcome = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    OutstandingBalance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPatientResponsibility = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPaidOrAllocated = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReasonCode = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    RevocationReason = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
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
                    table.PrimaryKey("PK_BilInpatientClearanceHandoff", x => x.Id);
                    table.CheckConstraint("CK_BilInpatientClearanceHandoff_ClearanceStatus", "\"ClearanceStatus\" IN ('PENDING', 'BLOCKED', 'CLEARED', 'REVOKED')");
                    table.CheckConstraint("CK_BilInpatientClearanceHandoff_FinancialOutcome", "\"FinancialOutcome\" IS NULL OR \"FinancialOutcome\" IN ('FULLY_PAID', 'INSURANCE_GUARANTEED', 'SETTLED_WITH_DEPOSIT', 'DISCHARGED_WITH_AR')");
                    table.CheckConstraint("CK_BilInpatientClearanceHandoff_FinancialVersion", "\"FinancialVersion\" > 0");
                    table.CheckConstraint("CK_BilInpatientClearanceHandoff_ReasonCode", "\"ReasonCode\" IN ('INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED', 'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED')");
                    table.CheckConstraint("CK_BilInpatientClearanceHandoff_Status", "\"Status\" IN ('CREATED', 'ACKNOWLEDGED')");
                    table.ForeignKey(
                        name: "FK_BilInpatientClearanceHandoff_BilInvoice_InvoiceId",
                        column: x => x.InvoiceId,
                        principalSchema: "public",
                        principalTable: "BilInvoice",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                keyColumn: "Id",
                keyValue: new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b801"),
                columns: new[] { "CalculationType", "CapAmount", "Percentage" },
                values: new object[] { "FLAT", null, null });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                keyColumn: "Id",
                keyValue: new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b802"),
                columns: new[] { "CalculationType", "CapAmount", "Percentage" },
                values: new object[] { "FLAT", null, null });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                keyColumn: "Id",
                keyValue: new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b803"),
                columns: new[] { "CalculationType", "CapAmount", "Percentage" },
                values: new object[] { "FLAT", null, null });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                keyColumn: "Id",
                keyValue: new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b804"),
                columns: new[] { "CalculationType", "CapAmount", "Percentage" },
                values: new object[] { "FLAT", null, null });

            migrationBuilder.InsertData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                columns: new[] { "Id", "Amount", "CalculationType", "CancelBy", "CancelDateTime", "CapAmount", "Code", "Coverable", "CreateBy", "CreateDateTime", "DeleteBy", "DeleteDateTime", "EffectiveFrom", "EffectiveTo", "IsActive", "Name", "OncePerPatientLocalDay", "Percentage", "ReplacementPriority", "ServiceType", "UpdateBy", "UpdateDateTime" },
                values: new object[] { new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b805"), 0m, "PERCENTAGE_WITH_CAP", new Guid("00000000-0000-0000-0000-000000000000"), null, 6000000.00m, "ADM-RANAP-01", false, new Guid("00000000-0000-0000-0000-000000000000"), new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Utc), new Guid("00000000-0000-0000-0000-000000000000"), null, new DateTimeOffset(new DateTime(2026, 9, 24, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), null, true, "Biaya Administrasi Rawat Inap (7% Cap Rp6.000.000)", false, 7.00m, 100, "RANAP", new Guid("00000000-0000-0000-0000-000000000000"), null });

            migrationBuilder.AddCheckConstraint(
                name: "CK_MstAdministrationFeePolicy_CalculationType",
                schema: "public",
                table: "MstAdministrationFeePolicy",
                sql: "\"CalculationType\" IN ('FLAT', 'PERCENTAGE_WITH_CAP')");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientClearanceHandoff_ClearanceStatus",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                column: "ClearanceStatus");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientClearanceHandoff_Encounter_Version",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                columns: new[] { "EncounterId", "FinancialVersion" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientClearanceHandoff_Invoice",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_BilInpatientClearanceHandoff_Status",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BilInpatientClearanceHandoff",
                schema: "public");

            migrationBuilder.DropCheckConstraint(
                name: "CK_MstAdministrationFeePolicy_CalculationType",
                schema: "public",
                table: "MstAdministrationFeePolicy");

            migrationBuilder.DeleteData(
                schema: "public",
                table: "MstAdministrationFeePolicy",
                keyColumn: "Id",
                keyValue: new Guid("7e49ba03-b808-4cff-8e71-735ec8d8b805"));

            migrationBuilder.DropColumn(
                name: "CalculationType",
                schema: "public",
                table: "MstAdministrationFeePolicy");

            migrationBuilder.DropColumn(
                name: "CapAmount",
                schema: "public",
                table: "MstAdministrationFeePolicy");

            migrationBuilder.DropColumn(
                name: "Percentage",
                schema: "public",
                table: "MstAdministrationFeePolicy");
        }
    }
}
