using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Melonggarkan CK_FinReceivableInvoiceBatch_DebtorType dari "DebtorType" = 'PAYER' menjadi IN ('PAYER','EMPLOYEE_BENEFIT')
    /// agar Invoice AR piutang karyawan/dokter dapat disimpan sebagai batch berjenis EMPLOYEE_BENEFIT (nomor, dokumen, dan daftar
    /// Invoice AR memakai tabel yang sama). Hanya memperlonggar constraint: tidak ada data yang diubah, dan semua baris PAYER
    /// yang ada tetap lolos. Down memasang kembali constraint lama dan akan GAGAL bila sudah ada batch EMPLOYEE_BENEFIT;
    /// batch itu harus dibatalkan/dipindahkan lebih dulu.
    /// </summary>
    /// <inheritdoc />
    public partial class AllowEmployeeBenefitInvoiceBatchDebtorType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivableInvoiceBatch_DebtorType",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivableInvoiceBatch_DebtorType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                sql: "\"DebtorType\" IN ('PAYER','EMPLOYEE_BENEFIT')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivableInvoiceBatch_DebtorType",
                schema: "public",
                table: "FinReceivableInvoiceBatch");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivableInvoiceBatch_DebtorType",
                schema: "public",
                table: "FinReceivableInvoiceBatch",
                sql: "\"DebtorType\" = 'PAYER'");
        }
    }
}
