using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-074 / RWI-DEC-099. Penugasan dokter mengenal DPJP, konsulen, dan dokter jaga:
    /// satu kolom ditambahkan, satu index unik berganti filter, satu index pendukung lahir.
    /// </summary>
    /// <remarks>
    /// TIGA LANGKAH, DAN URUTANNYA MENGIKAT.
    ///
    ///   1. Tambah kolom "AssignmentRole" dengan DEFAULT 1, sehingga baris lama langsung sah.
    ///   2. Isi baris lama menjadi 1 secara eksplisit. Nilai bawaan database bukan pengganti
    ///      nilai domain yang eksplisit.
    ///   3. Buang index unik lama, lalu buat index unik baru yang filternya MEMBACA kolom itu
    ///      beserta index pendukung periode.
    ///
    /// KENAPA LANGKAH 3 TIDAK BOLEH NAIK KE ATAS. Antara membuang index lama dan memasang
    /// index baru ada jeda ketika tidak ada index yang menjaga INV-INP-03. Bila kolomnya
    /// belum terisi, filter "AssignmentRole" = 1 tidak dapat dievaluasi dengan benar dan dua
    /// DPJP aktif dapat tersimpan pada jeda itu. Karena itu ketiganya berada di dalam SATU
    /// migration, bukan tiga migration terpisah, dan langkah 3 dijaga pemeriksaan eksplisit
    /// yang menggagalkan migration bila masih ada baris berperan di luar 1, 2, atau 3.
    ///
    /// PENGISIAN DATA LAMA BUKAN TEBAKAN. Sebelum amandemen ini, tabel ini hanya pernah
    /// menyimpan DPJP. Seluruh baris lama karena itu diisi 1 tanpa satu pun baris ambigu, dan
    /// tidak ada laporan `unresolved` yang perlu dibuat.
    ///
    /// BATAS LANGKAH MUNDUR — WAJIB DIBACA SEBELUM ROLLBACK. Down() aman dijalankan SELAMA
    /// BELUM ADA SATU PUN BARIS BERPERAN 2 ATAU 3. Begitu konsulen atau dokter jaga pertama
    /// tersimpan, index lama IX_InpDoctorAssignment_EpisodeId_Active akan menolak baris itu
    /// karena filternya hanya melihat "EndDateTime" IS NULL. Sejak titik itu pemulihan
    /// dilakukan MAJU, bukan mundur. Down() memeriksanya lebih dulu dan GAGAL TERKENDALI
    /// dengan pesan yang menyebut jumlah barisnya, bukan merusak data diam-diam.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260911000000_AddAssignmentRoleToInpDoctorAssignment")]
    public partial class AddAssignmentRoleToInpDoctorAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Langkah 1 — kolom lahir bersama nilai bawaannya.
            migrationBuilder.AddColumn<int>(
                name: "AssignmentRole",
                schema: "public",
                table: "InpDoctorAssignment",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            // Langkah 2 — baris lama diisi eksplisit, bukan dibiarkan bersandar pada DEFAULT.
            migrationBuilder.Sql(@"
                UPDATE public.""InpDoctorAssignment""
                SET ""AssignmentRole"" = 1
                WHERE ""AssignmentRole"" IS DISTINCT FROM 1;");

            // Penjaga urutan. Langkah 3 hanya boleh berjalan ketika setiap baris sudah punya
            // peran yang dikenal. Bila migration ini dijalankan terbalik oleh siapa pun,
            // kegagalannya terlihat di sini, bukan berupa dua DPJP aktif yang lolos diam-diam.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_tanpa_peran integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_tanpa_peran
                    FROM public.""InpDoctorAssignment""
                    WHERE ""AssignmentRole"" NOT IN (1, 2, 3);

                    IF jumlah_tanpa_peran > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-074: % baris InpDoctorAssignment belum punya peran yang dikenal. Index unik baru tidak dipasang; isi kolom AssignmentRole lebih dulu.',
                            jumlah_tanpa_peran;
                    END IF;
                END $$;");

            // Langkah 3 — index unik lama dicabut, index unik baru yang membaca peran dipasang.
            migrationBuilder.DropIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_Active",
                schema: "public",
                table: "InpDoctorAssignment");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_ActiveDpjp",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "EpisodeId",
                unique: true,
                filter: "\"EndDateTime\" IS NULL AND \"AssignmentRole\" = 1");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_Episode_Doctor_Role_Period",
                schema: "public",
                table: "InpDoctorAssignment",
                columns: new[] { "EpisodeId", "DoctorId", "AssignmentRole", "StartDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Batas rollback ditegakkan, bukan sekadar didokumentasikan. Index lama menolak
            // penugasan terbuka kedua apa pun perannya, sehingga memasangnya kembali ketika
            // konsulen atau dokter jaga sudah tersimpan akan gagal di tengah jalan.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_peran_baru integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_peran_baru
                    FROM public.""InpDoctorAssignment""
                    WHERE ""AssignmentRole"" IN (2, 3);

                    IF jumlah_peran_baru > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-074: rollback ditolak. % baris konsulen atau dokter jaga sudah tersimpan, dan index lama IX_InpDoctorAssignment_EpisodeId_Active akan menolaknya. Pemulihan harus maju, bukan mundur.',
                            jumlah_peran_baru;
                    END IF;
                END $$;");

            migrationBuilder.DropIndex(
                name: "IX_InpDoctorAssignment_Episode_Doctor_Role_Period",
                schema: "public",
                table: "InpDoctorAssignment");

            migrationBuilder.DropIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_ActiveDpjp",
                schema: "public",
                table: "InpDoctorAssignment");

            migrationBuilder.CreateIndex(
                name: "IX_InpDoctorAssignment_EpisodeId_Active",
                schema: "public",
                table: "InpDoctorAssignment",
                column: "EpisodeId",
                unique: true,
                filter: "\"EndDateTime\" IS NULL");

            migrationBuilder.DropColumn(
                name: "AssignmentRole",
                schema: "public",
                table: "InpDoctorAssignment");
        }
    }
}
