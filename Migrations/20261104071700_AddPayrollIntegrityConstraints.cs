using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddPayrollIntegrityConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_TrxPayrollPayment_LiveInstruction",
                schema: "public",
                table: "TrxPayrollPayment",
                column: "PayrollRunEmployeeId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"PaymentStatus\" IN ('Scheduled', 'Processing', 'Paid', 'PartiallyReversed')");

            migrationBuilder.CreateIndex(
                name: "UX_TrxPayrollRun_RegularPeriod",
                schema: "public",
                table: "TrxPayrollRun",
                column: "PayrollPeriodId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"RunType\" = 'Regular' AND \"RunStatus\" NOT IN ('Cancelled', 'Reversed')");

            migrationBuilder.CreateIndex(
                name: "UX_TrxPayrollVariableInput_MedicalFeeClaim",
                schema: "public",
                table: "TrxPayrollVariableInput",
                column: "SourceId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"SourceType\" = 'MdfServiceFee' AND \"SourceId\" IS NOT NULL AND \"InputStatus\" NOT IN ('Cancelled', 'Rejected')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_TrxPayrollPayment_LiveInstruction",
                schema: "public",
                table: "TrxPayrollPayment");

            migrationBuilder.DropIndex(
                name: "UX_TrxPayrollRun_RegularPeriod",
                schema: "public",
                table: "TrxPayrollRun");

            migrationBuilder.DropIndex(
                name: "UX_TrxPayrollVariableInput_MedicalFeeClaim",
                schema: "public",
                table: "TrxPayrollVariableInput");
        }
    }
}
