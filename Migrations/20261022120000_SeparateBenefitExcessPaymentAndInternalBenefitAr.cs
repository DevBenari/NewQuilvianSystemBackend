using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Memisahkan tiga aliran Benefit Karyawan RS di ledger:
    /// 1. Pembayaran langsung atas kelebihan plafon (E): target alokasi baru BENEFIT_EXCESS pada BilPaymentAllocation dan
    ///    batas BenefitExcessAuthorizedAmount pada BilSettlement. Dibaca/ditulis hanya oleh jalur kelebihan plafon; semua
    ///    perhitungan sisa tagihan pasien (K) tetap hanya membaca target INVOICE.
    /// 2. Piutang benefit internal vs piutang pihak luar: IsInternalBenefit pada BilArHandoff dan FinReceivable. Backfill
    ///    menandai baris PAYER lama yang debiturnya penjamin berflag internal (IsEmployeeBenefitGuarantor).
    /// 3. Asal piutang karyawan: EmployeeArOrigin (OVER_ALLOWANCE / PATIENT_PORTION / MIXED), kosong untuk baris lama.
    /// Tidak ada data yang dihapus. Down gagal bila sudah ada alokasi BENEFIT_EXCESS (constraint lama tidak mengizinkannya);
    /// baris itu harus dibereskan lebih dulu.
    /// </summary>
    /// <inheritdoc />
    public partial class SeparateBenefitExcessPaymentAndInternalBenefitAr : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BenefitExcessAuthorizedAmount",
                schema: "public",
                table: "BilSettlement",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeArOrigin",
                schema: "public",
                table: "BilArHandoff",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternalBenefit",
                schema: "public",
                table: "BilArHandoff",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "EmployeeArOrigin",
                schema: "public",
                table: "FinReceivable",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInternalBenefit",
                schema: "public",
                table: "FinReceivable",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill: porsi Benefit RS yang sudah ada (PAYER atas penjamin berflag internal) ditandai sebagai piutang internal.
            migrationBuilder.Sql(
                "UPDATE public.\"BilArHandoff\" h SET \"IsInternalBenefit\" = true " +
                "FROM public.\"MstCompanyGuarantor\" c " +
                "WHERE h.\"DebtorType\" = 'PAYER' AND h.\"DebtorReferenceId\" = c.\"Id\" AND c.\"IsEmployeeBenefitGuarantor\" = true;");
            migrationBuilder.Sql(
                "UPDATE public.\"FinReceivable\" r SET \"IsInternalBenefit\" = true " +
                "FROM public.\"MstCompanyGuarantor\" c " +
                "WHERE r.\"DebtorType\" = 'PAYER' AND r.\"DebtorReferenceId\" = c.\"Id\" AND c.\"IsEmployeeBenefitGuarantor\" = true;");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPaymentAllocation_TargetType",
                schema: "public",
                table: "BilPaymentAllocation");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPaymentAllocation_TargetType",
                schema: "public",
                table: "BilPaymentAllocation",
                sql: "\"TargetType\" IN ('INVOICE','BENEFIT_EXCESS')");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilSettlement_BenefitExcess",
                schema: "public",
                table: "BilSettlement",
                sql: "\"BenefitExcessAuthorizedAmount\" IS NULL OR \"BenefitExcessAuthorizedAmount\" >= 0");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_EmployeeArOrigin",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"EmployeeArOrigin\" IS NULL OR (\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"EmployeeArOrigin\" IN ('OVER_ALLOWANCE','PATIENT_PORTION','MIXED'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilArHandoff_InternalBenefit",
                schema: "public",
                table: "BilArHandoff",
                sql: "\"IsInternalBenefit\" = false OR \"DebtorType\" = 'PAYER'");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivable_EmployeeArOrigin",
                schema: "public",
                table: "FinReceivable",
                sql: "\"EmployeeArOrigin\" IS NULL OR (\"DebtorType\" = 'EMPLOYEE_BENEFIT' AND \"EmployeeArOrigin\" IN ('OVER_ALLOWANCE','PATIENT_PORTION','MIXED'))");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivable_InternalBenefit",
                schema: "public",
                table: "FinReceivable",
                sql: "\"IsInternalBenefit\" = false OR \"DebtorType\" = 'PAYER'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivable_InternalBenefit",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivable_EmployeeArOrigin",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_InternalBenefit",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilArHandoff_EmployeeArOrigin",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilSettlement_BenefitExcess",
                schema: "public",
                table: "BilSettlement");

            migrationBuilder.DropCheckConstraint(
                name: "CK_BilPaymentAllocation_TargetType",
                schema: "public",
                table: "BilPaymentAllocation");

            migrationBuilder.AddCheckConstraint(
                name: "CK_BilPaymentAllocation_TargetType",
                schema: "public",
                table: "BilPaymentAllocation",
                sql: "\"TargetType\" = 'INVOICE'");

            migrationBuilder.DropColumn(
                name: "IsInternalBenefit",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropColumn(
                name: "EmployeeArOrigin",
                schema: "public",
                table: "FinReceivable");

            migrationBuilder.DropColumn(
                name: "IsInternalBenefit",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "EmployeeArOrigin",
                schema: "public",
                table: "BilArHandoff");

            migrationBuilder.DropColumn(
                name: "BenefitExcessAuthorizedAmount",
                schema: "public",
                table: "BilSettlement");
        }
    }
}
