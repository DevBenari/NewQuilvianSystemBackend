using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Memperluas nilai HandoffType yang sah pada FinBillingHandoffIntake dari empat menjadi
    /// delapan, supaya empat fakta Billing berikut dapat masuk lewat pintu yang sama:
    /// mutasi deposit pasien, pengakuan kelebihan bayar, pengembalian uang ke pasien, dan
    /// pengesahan selisih kas shift kasir.
    ///
    /// Task BE-FIN-022, keputusan arsitektur FIN-DES-029, keputusan bisnis FIN-DEC-040..044.
    /// Ini SATU-SATUNYA migration untuk seluruh AMENDMENT REVISI 3 blueprint FIN-BP-001:
    /// tidak ada tabel baru dan tidak ada kolom baru, karena tabel intake memang dirancang
    /// untuk diperluas lewat kolom HandoffType (FIN-DES-008).
    ///
    /// Empat nilai lama (AR, AP, COLLECTION, ADJUSTMENT) TIDAK berubah artinya, sehingga tidak
    /// ada baris existing yang perlu diisi ulang maupun dipindah.
    ///
    /// Catatan rollback: Down mengembalikan batas menjadi empat nilai dan karena itu akan GAGAL
    /// bila sudah ada baris memakai salah satu nilai baru. Hapus atau pindahkan baris itu lebih
    /// dulu sebelum memundurkan migrasi ini.
    /// </summary>
    public partial class AlterFinBillingHandoffIntakeHandoffTypeCheck : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinBillingHandoffIntake_HandoffType",
                schema: "public",
                table: "FinBillingHandoffIntake");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinBillingHandoffIntake_HandoffType",
                schema: "public",
                table: "FinBillingHandoffIntake",
                sql: "\"HandoffType\" IN ('AR','AP','COLLECTION','ADJUSTMENT','DEPOSIT_MOVEMENT','REFUNDABLE_CREDIT','REFUND_CASE','CASH_VARIANCE_REVIEW')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinBillingHandoffIntake_HandoffType",
                schema: "public",
                table: "FinBillingHandoffIntake");

            migrationBuilder.AddCheckConstraint(
                name: "CK_FinBillingHandoffIntake_HandoffType",
                schema: "public",
                table: "FinBillingHandoffIntake",
                sql: "\"HandoffType\" IN ('AR','AP','COLLECTION','ADJUSTMENT')");
        }
    }
}
