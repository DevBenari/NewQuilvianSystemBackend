using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-079 / RWI-DEC-130, langkah migration <c>E1</c>. Penugasan dokter mengenal
    /// <b>kenapa</b> ia dibuat, bukan hanya siapa dan dengan peran apa: satu kolom tujuan,
    /// satu check constraint, dan satu index pendukung census "pasien saya".
    /// </summary>
    /// <remarks>
    /// TIGA LANGKAH, DAN URUTANNYA MENGIKAT.
    ///
    ///   1. Tambah kolom "AssignmentPurpose" dengan DEFAULT 0, sehingga baris lama langsung sah.
    ///   2. Isi baris lama menjadi 0 secara eksplisit, lalu pastikan tidak ada nilai di luar
    ///      enum yang tercatat data-dictionary.md bagian 18.1.
    ///   3. Pasang check constraint CK_InpDoctorAssignment_LateDocumentation beserta index
    ///      IX_InpDoctorAssignment_DoctorId_Active.
    ///
    /// KENAPA LANGKAH 3 TIDAK BOLEH NAIK KE ATAS. Check constraint dievaluasi terhadap SELURUH
    /// baris yang sudah ada saat ia dipasang. Bila kolomnya belum terisi nilai yang sah,
    /// pemasangan constraint gagal di tengah jalan dan migration berhenti pada keadaan
    /// setengah jadi: kolom ada, penjaganya tidak.
    ///
    /// PENGISIAN DATA LAMA BUKAN TEBAKAN. Sebelum amandemen ini, tidak ada satu pun jalur tulis
    /// yang dapat melahirkan penugasan singkat penulisan catatan terlambat — endpoint-nya baru
    /// dibuka BE-RWI-080. Seluruh baris lama karena itu benar-benar Regular, dan tidak ada
    /// laporan `unresolved` yang perlu dibuat.
    ///
    /// BATAS LANGKAH MUNDUR — WAJIB DIBACA SEBELUM ROLLBACK. Down() aman dijalankan SELAMA
    /// BELUM ADA SATU PUN BARIS BERTUJUAN 1 (LateDocumentation). Begitu penugasan singkat
    /// pertama tersimpan, membuang kolomnya menghapus satu-satunya pembeda antara dokter jaga
    /// yang benar-benar menjaga shift dan dokter jaga yang hanya diberi jendela menulis — dan
    /// laporan jumlah jaga sejak saat itu menghitung orang yang tidak pernah jaga. Sejak titik
    /// itu pemulihan dilakukan MAJU, bukan mundur. Down() memeriksanya lebih dulu dan GAGAL
    /// TERKENDALI dengan pesan yang menyebut jumlah barisnya, bukan merusak data diam-diam —
    /// roadmap BE-RWI-079 acceptance criteria 4.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916000000_AddAssignmentPurposeToInpDoctorAssignment")]
    public partial class AddAssignmentPurposeToInpDoctorAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Langkah 1 — kolom lahir bersama nilai bawaannya.
            migrationBuilder.AddColumn<int>(
                name: "AssignmentPurpose",
                schema: "public",
                table: "InpDoctorAssignment",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Langkah 2 — baris lama diisi eksplisit, bukan dibiarkan bersandar pada DEFAULT.
            migrationBuilder.Sql(@"
                UPDATE public.""InpDoctorAssignment""
                SET ""AssignmentPurpose"" = 0
                WHERE ""AssignmentPurpose"" IS DISTINCT FROM 0;");

            // Penjaga urutan. Langkah 3 hanya boleh berjalan ketika setiap baris sudah punya
            // tujuan yang dikenal. Bila ada nilai di luar enum, kegagalannya terlihat di sini
            // beserta jumlahnya, bukan sebagai constraint yang gagal dipasang tanpa penjelasan.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_tanpa_tujuan integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_tanpa_tujuan
                    FROM public.""InpDoctorAssignment""
                    WHERE ""AssignmentPurpose"" NOT IN (0, 1);

                    IF jumlah_tanpa_tujuan > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-079: % baris InpDoctorAssignment bertujuan di luar enum yang tercatat. Check constraint tidak dipasang; betulkan kolom AssignmentPurpose lebih dulu.',
                            jumlah_tanpa_tujuan;
                    END IF;
                END $$;");

            // Langkah 3a — INV-INP-12 ditegakkan database, bukan hanya service.
            migrationBuilder.Sql(@"
                ALTER TABLE public.""InpDoctorAssignment""
                ADD CONSTRAINT ""CK_InpDoctorAssignment_LateDocumentation""
                CHECK (""AssignmentPurpose"" <> 1
                    OR (""AssignmentRole"" = 3
                        AND ""EndDateTime"" IS NOT NULL
                        AND ""EndDateTime"" > ""StartDateTime""
                        AND ""HandoverReason"" IS NOT NULL
                        AND length(trim(""HandoverReason"")) > 0));");

            // Langkah 3b — index pendukung census "pasien saya" (NFR-026). Saringannya
            // berangkat dari dokter lalu menilai masa berlaku; index per episode yang sudah ada
            // tidak membantunya.
            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_DoctorId_Active",
                schema: "public",
                table: "InpDoctorAssignment",
                columns: new[] { "DoctorId", "EndDateTime" },
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Batas rollback ditegakkan, bukan sekadar didokumentasikan.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_penugasan_singkat integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_penugasan_singkat
                    FROM public.""InpDoctorAssignment""
                    WHERE ""AssignmentPurpose"" = 1;

                    IF jumlah_penugasan_singkat > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-079: rollback ditolak. % baris penugasan singkat penulisan catatan terlambat sudah tersimpan, dan membuang kolom AssignmentPurpose menghapus satu-satunya pembeda terhadap dokter jaga biasa. Pemulihan harus maju, bukan mundur.',
                            jumlah_penugasan_singkat;
                    END IF;
                END $$;");

            migrationBuilder.DropIndex(
                name: "IX_InpDoctorAssignment_DoctorId_Active",
                schema: "public",
                table: "InpDoctorAssignment");

            migrationBuilder.Sql(@"
                ALTER TABLE public.""InpDoctorAssignment""
                DROP CONSTRAINT IF EXISTS ""CK_InpDoctorAssignment_LateDocumentation"";");

            migrationBuilder.DropColumn(
                name: "AssignmentPurpose",
                schema: "public",
                table: "InpDoctorAssignment");
        }
    }
}
