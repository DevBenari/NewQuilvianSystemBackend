using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menghapus CK_FinReceivableInstallmentPlan_MakerChecker ("ApprovedBy" IS NULL OR "ApprovedBy" &lt;&gt; "RequestedBy") agar
    /// pengajuan perjanjian angsuran dapat disetujui oleh akun yang sama dengan pengaju (SuperAdmin, atau akun yang diberi izin
    /// HrdInstallmentPlanApproval : SelfApprove). Database CHECK tidak dapat mengetahui izin dinamis pengguna, jadi aturan
    /// maker-checker kini ditegakkan service (FinanceReceivableInstallmentPlanService.EnsureCanDecideAsync) dan izin
    /// Approve/Reject/SelfApprove pada controller. ApprovedBy tetap UserId pemutus yang sebenarnya. Tidak ada data yang diubah.
    /// Down memasang kembali constraint dan akan GAGAL bila sudah ada baris dengan ApprovedBy = RequestedBy; baris itu harus
    /// dibereskan lebih dulu.
    /// </summary>
    /// <inheritdoc />
    public partial class DropInstallmentPlanMakerCheckerConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_FinReceivableInstallmentPlan_MakerChecker",
                schema: "public",
                table: "FinReceivableInstallmentPlan");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_FinReceivableInstallmentPlan_MakerChecker",
                schema: "public",
                table: "FinReceivableInstallmentPlan",
                sql: "\"ApprovedBy\" IS NULL OR \"ApprovedBy\" <> \"RequestedBy\"");
        }
    }
}
