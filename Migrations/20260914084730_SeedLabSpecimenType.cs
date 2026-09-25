using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Mengisi data induk jenis specimen dengan tujuh baris baseline dari <c>LAB-EVD-001</c>
    /// (<c>LAB-DEC-040</c>, <c>BE-LAB-20</c>).
    ///
    /// <b>Mengapa pengisiannya lewat migration, bukan hanya seeder.</b> Tabel jenis specimen
    /// yang kosong membuat petugas tidak dapat mencatat satu wadah pun — dan yang tertahan
    /// bukan formulir, melainkan bahan yang sudah terlanjur diambil dari tubuh pasien.
    /// Lingkungan yang menjalankan migration dari awal karena itu langsung dapat dipakai, tanpa
    /// menunggu aplikasi menyala.
    ///
    /// Pengisian memakai <c>ON CONFLICT DO NOTHING</c>: menjalankan ulang migration tidak
    /// menimpa perubahan yang sudah dilakukan kepala instalasi.
    /// </summary>
    public partial class SeedLabSpecimenType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Id ditetapkan tetap, bukan dibangkitkan database, agar baris baseline memiliki
            // identitas yang sama di setiap lingkungan dan dapat dirujuk dengan pasti. Nilainya
            // sama persis dengan yang dipakai LabSpecimenTypeSeeder.
            var specimenTypes = new (string Id, string Code, string Name, bool IsOtherBucket, int Sort)[]
            {
                ("2c7e5d10-0001-4b20-8e11-7c2e1b6f8d01", "BLOOD", "Blood", false, 1),
                ("2c7e5d10-0002-4b20-8e11-7c2e1b6f8d02", "URINE", "Urine", false, 2),
                ("2c7e5d10-0003-4b20-8e11-7c2e1b6f8d03", "BODYFLUID", "Body Fluid", false, 3),
                ("2c7e5d10-0004-4b20-8e11-7c2e1b6f8d04", "SPUTUM", "Sputum", false, 4),
                ("2c7e5d10-0005-4b20-8e11-7c2e1b6f8d05", "PUS", "Pus", false, 5),
                ("2c7e5d10-0006-4b20-8e11-7c2e1b6f8d06", "TISSUE", "Jaringan", false, 6),

                // Satu-satunya baris berpenanda Lainnya. Urutannya sengaja 99 supaya ia selalu
                // berada di bawah pilihan yang lebih tepat, dan index unik parsial menjaga agar
                // tidak pernah ada baris Lainnya kedua yang aktif (VAL-62).
                ("2c7e5d10-0007-4b20-8e11-7c2e1b6f8d07", "OTHER", "Lainnya", true, 99)
            };

            const string emptyGuid = "00000000-0000-0000-0000-000000000000";

            foreach (var specimenType in specimenTypes)
            {
                migrationBuilder.Sql(
                    "INSERT INTO public.\"LabSpecimenType\" " +
                    "(\"Id\", \"SpecimenTypeCode\", \"SpecimenTypeName\", \"Description\", " +
                    "\"IsOtherBucket\", \"IsActive\", \"SortOrder\", " +
                    "\"CreateDateTime\", \"CreateBy\", \"UpdateBy\", \"DeleteBy\", \"CancelBy\", " +
                    "\"IsCancel\", \"IsDelete\") VALUES (" +
                    $"'{specimenType.Id}', '{specimenType.Code}', '{specimenType.Name.Replace("'", "''")}', NULL, " +
                    $"{(specimenType.IsOtherBucket ? "true" : "false")}, true, {specimenType.Sort}, " +
                    // NOW() sudah bertipe timestamptz. Memakai NOW() AT TIME ZONE 'UTC' akan
                    // menghasilkan timestamp tanpa zona waktu yang kemudian ditafsirkan ulang
                    // memakai zona server, sehingga waktunya bergeser.
                    "NOW(), " +
                    $"'{emptyGuid}', '{emptyGuid}', '{emptyGuid}', '{emptyGuid}', " +
                    "false, false) " +
                    "ON CONFLICT DO NOTHING;");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Hanya baris baseline yang dihapus, dikenali dari Id tetapnya. Jenis specimen yang
            // ditambahkan kepala instalasi sendiri tidak ikut terbawa, dan wadah yang sudah
            // menunjuk baris baseline akan menahan penghapusannya lewat foreign key.
            migrationBuilder.Sql(
                "DELETE FROM public.\"LabSpecimenType\" WHERE \"Id\" IN (" +
                "'2c7e5d10-0001-4b20-8e11-7c2e1b6f8d01', " +
                "'2c7e5d10-0002-4b20-8e11-7c2e1b6f8d02', " +
                "'2c7e5d10-0003-4b20-8e11-7c2e1b6f8d03', " +
                "'2c7e5d10-0004-4b20-8e11-7c2e1b6f8d04', " +
                "'2c7e5d10-0005-4b20-8e11-7c2e1b6f8d05', " +
                "'2c7e5d10-0006-4b20-8e11-7c2e1b6f8d06', " +
                "'2c7e5d10-0007-4b20-8e11-7c2e1b6f8d07');");
        }
    }
}
