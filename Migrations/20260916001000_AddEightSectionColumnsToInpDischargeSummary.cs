using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-085 / RWI-DEC-112, langkah migration <c>E2</c>. Resume pulang memuat delapan
    /// bagian: tiga isian yang selama ini tidak ada ditambahkan pada resume beserta tabel
    /// revisinya.
    /// </summary>
    /// <remarks>
    /// KETIGANYA NULLABLE, DAN ITU KEPUTUSAN, BUKAN KELALAIAN. Resume pulang yang sudah
    /// ditandatangani adalah rekam medis; ia tidak dapat diisi ulang oleh migration, dan
    /// membuat kolomnya NOT NULL berarti memilih antara dua hal yang sama-sama salah —
    /// mengarang isi klinis, atau menolak seluruh resume lama terbaca. Isi minimal resume masih
    /// berada di bawah gerbang pemilik klinis (RWI-RULE-032, 04-prd-to-mvp.md 22.7 nomor 3),
    /// sehingga kewajiban isian ditetapkan di sana, bukan di sini.
    ///
    /// TIGA KOLOM YANG SAMA PADA DUA TABEL. Tabel revisi menyimpan salinan isi resume sebelum
    /// digantikan. Bila ketiga kolom hanya lahir pada tabel induk, setiap penandatanganan ulang
    /// lewat sesi koreksi akan menghapus Pemeriksaan Penting, Kondisi Saat Pulang, dan Edukasi
    /// versi lama tanpa jejak — persis kebalikan dari gunanya tabel revisi (RWI-DEC-057).
    ///
    /// SENSITIF. Ketiganya memuat keterangan klinis dan tidak boleh masuk payload logger
    /// maupun endpoint daftar mana pun — permission-audit-matrix.md bagian 5.4.
    ///
    /// LANGKAH MUNDUR. Down() membuang keenam kolom. Aman selama ketiganya masih kosong;
    /// begitu ada resume yang terisi, membuangnya menghapus isi rekam medis. Down() memeriksanya
    /// lebih dulu dan GAGAL TERKENDALI beserta jumlah barisnya — roadmap BE-RWI-085 acceptance
    /// criteria 4.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916001000_AddEightSectionColumnsToInpDischargeSummary")]
    public partial class AddEightSectionColumnsToInpDischargeSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImportantFindingsSummary",
                schema: "public",
                table: "InpDischargeSummary",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargeConditionNote",
                schema: "public",
                table: "InpDischargeSummary",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationSummary",
                schema: "public",
                table: "InpDischargeSummary",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ImportantFindingsSummary",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "character varying(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DischargeConditionNote",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EducationSummary",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Batas rollback ditegakkan, bukan sekadar didokumentasikan. Kolom yang sudah
            // terisi adalah isi rekam medis, dan membuangnya tidak dapat dibatalkan.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_terisi integer;
                BEGIN
                    SELECT
                        (SELECT COUNT(*) FROM public.""InpDischargeSummary""
                         WHERE ""ImportantFindingsSummary"" IS NOT NULL
                            OR ""DischargeConditionNote"" IS NOT NULL
                            OR ""EducationSummary"" IS NOT NULL)
                      + (SELECT COUNT(*) FROM public.""InpDischargeSummaryRevision""
                         WHERE ""ImportantFindingsSummary"" IS NOT NULL
                            OR ""DischargeConditionNote"" IS NOT NULL
                            OR ""EducationSummary"" IS NOT NULL)
                    INTO jumlah_terisi;

                    IF jumlah_terisi > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-085: rollback ditolak. % baris resume atau revisi sudah mengisi Pemeriksaan Penting, Kondisi Saat Pulang, atau Edukasi. Membuang kolomnya menghapus isi rekam medis; ekspor lebih dulu bila memang dikehendaki.',
                            jumlah_terisi;
                    END IF;
                END $$;");

            migrationBuilder.DropColumn(
                name: "EducationSummary",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "DischargeConditionNote",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "ImportantFindingsSummary",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "EducationSummary",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropColumn(
                name: "DischargeConditionNote",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropColumn(
                name: "ImportantFindingsSummary",
                schema: "public",
                table: "InpDischargeSummary");
        }
    }
}
