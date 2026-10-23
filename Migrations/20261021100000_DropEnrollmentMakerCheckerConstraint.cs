using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menonaktifkan sementara aturan maker-checker enrollment benefit pegawai (FIN-VAL-271): pengaju boleh menyetujui
    /// pengajuannya sendiri sampai ada keputusan bisnis tentang persetujuan berjenjang. Hanya men-drop
    /// CK_TrxEmployeeBenefitEnrollment_MakerChecker; tidak ada data yang diubah. Down memasang kembali constraint dan akan
    /// GAGAL bila sudah ada baris dengan ApprovedByUserId = SubmittedByUserId; baris itu harus dibereskan lebih dulu.
    /// </summary>
    /// <inheritdoc />
    public partial class DropEnrollmentMakerCheckerConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_MakerChecker",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddCheckConstraint(
                name: "CK_TrxEmployeeBenefitEnrollment_MakerChecker",
                schema: "public",
                table: "TrxEmployeeBenefitEnrollment",
                sql: "\"ApprovedByUserId\" IS NULL OR \"SubmittedByUserId\" IS NULL OR \"ApprovedByUserId\" <> \"SubmittedByUserId\"");
        }
    }
}
