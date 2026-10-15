using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixClearanceReasonCodeReevaluatedByCashier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInpatientClearanceHandoff_ReasonCode",
                schema: "public",
                table: "BilInpatientClearanceHandoff");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInpatientClearanceHandoff_ReasonCode",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                sql: "\"ReasonCode\" IN ('INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED', 'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED', 'REEVALUATED_BY_CASHIER')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_BilInpatientClearanceHandoff_ReasonCode",
                schema: "public",
                table: "BilInpatientClearanceHandoff");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilInpatientClearanceHandoff_ReasonCode",
                schema: "public",
                table: "BilInpatientClearanceHandoff",
                sql: "\"ReasonCode\" IN ('INVOICE_SETTLED', 'GUARANTOR_APPROVED', 'DISCHARGE_ORDER_INITIATED', 'LATE_CHARGE_POSTED', 'PAYMENT_REVERSED', 'CORRECTION_APPLIED')");
        }
    }
}
