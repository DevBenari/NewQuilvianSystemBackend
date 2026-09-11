using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RelaxSingleConsultationAndPrescriptionForInpatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription",
                column: "ConsultationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsCancel\" = false AND \"InpEpisodeId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"InpEpisodeId\" IS NULL");
        }

        /// <inheritdoc />
        /// <remarks>
        /// <para>
        /// <b>Rollback ini tidak selalu mungkin, dan itu memang benar.</b> Arah mundur
        /// membangun ulang unique index versi ketat, yaitu dunia tempat satu kunjungan hanya
        /// boleh punya satu catatan dan satu resep aktif. Begitu <c>BE-RWI-043</c> dipakai dan
        /// seorang dokter menuliskan catatan kedua pada satu perawatan rawat inap, baris itu
        /// melanggar aturan lama - dan PostgreSQL akan menolak membangun index-nya.
        /// </para>
        /// <para>
        /// Terbukti pada uji maju-mundur 5 September 2026 terhadap PostgreSQL 15.15: dua
        /// catatan rawat inap disisipkan, rollback ditolak <c>23505 could not create unique
        /// index</c>; baris uji dihapus, rollback lulus. Tanpa penjagaan di bawah, yang muncul
        /// hanyalah <c>Detail redacted as it may contain sensitive data</c> - benar, tetapi
        /// tidak memberi tahu siapa pun apa yang harus dikerjakan.
        /// </para>
        /// <para>
        /// Karena itu penjagaan ini <b>tidak</b> menghapus, menggabungkan, atau membatalkan
        /// baris klinis mana pun untuk memuluskan rollback. Menghapus catatan dokter demi
        /// menyenangkan sebuah index adalah kehilangan rekam medis. Yang dilakukan adalah
        /// berhenti lebih awal dengan kalimat yang menyebut jumlah dan nama tabelnya, sehingga
        /// keputusannya kembali ke manusia yang berwenang.
        /// </para>
        /// </remarks>
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
DO $$
DECLARE
    kunjungan_bentrok integer;
    konsultasi_bentrok integer;
BEGIN
    SELECT count(*) INTO kunjungan_bentrok FROM (
        SELECT ""EncounterId""
        FROM public.""TrxDoctorConsultation""
        WHERE ""IsDelete"" = false
        GROUP BY ""EncounterId""
        HAVING count(*) > 1
    ) t;

    SELECT count(*) INTO konsultasi_bentrok FROM (
        SELECT ""ConsultationId""
        FROM public.""TrxPrescription""
        WHERE ""IsDelete"" = false AND ""IsCancel"" = false
        GROUP BY ""ConsultationId""
        HAVING count(*) > 1
    ) t;

    IF kunjungan_bentrok > 0 OR konsultasi_bentrok > 0 THEN
        RAISE EXCEPTION
            'Rollback BE-RWI-043 dihentikan: % kunjungan memiliki lebih dari satu catatan dokter dan % konsultasi memiliki lebih dari satu resep aktif. Arah mundur membangun ulang unique index versi ketat, sehingga baris-baris itu akan ditolak. Tidak ada catatan klinis yang dihapus otomatis. Selesaikan lebih dulu bersama pemilik ClinicalManagement dan PharmacyManagement - batalkan, gabungkan, atau pindahkan baris yang berlebih - lalu jalankan rollback ini kembali.',
            kunjungan_bentrok, konsultasi_bentrok;
    END IF;
END $$;
");

            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription",
                column: "ConsultationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsCancel\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
