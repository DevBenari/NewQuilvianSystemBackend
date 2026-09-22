using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-094 / migration R3. Menambahkan jenis catatan CPPT tanpa menebak jenis entri
    /// lama. Nilai lama menjadi Unspecified (0) dan tetap terlihat sebagai data legacy.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916002000_AddCpptNoteKind")]
    public partial class AddCpptNoteKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NoteKind",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientIntegratedProgressNote_Episode_Kind_Time",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote",
                columns: new[] { "InpEpisodeId", "NoteKind", "NoteDateTime" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Jenis selain Unspecified adalah klasifikasi klinis yang lahir setelah migration.
            // Rollback ditolak bila data itu sudah ada agar klasifikasinya tidak hilang diam-diam.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_terklasifikasi integer;
                BEGIN
                    SELECT COUNT(*)
                    INTO jumlah_terklasifikasi
                    FROM public.""TrxPatientIntegratedProgressNote""
                    WHERE ""NoteKind"" <> 0;

                    IF jumlah_terklasifikasi > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-094: rollback ditolak. % entri CPPT sudah memiliki jenis catatan; membuang kolom NoteKind akan menghilangkan klasifikasi klinisnya.',
                            jumlah_terklasifikasi;
                    END IF;
                END $$;");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientIntegratedProgressNote_Episode_Kind_Time",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");

            migrationBuilder.DropColumn(
                name: "NoteKind",
                schema: "public",
                table: "TrxPatientIntegratedProgressNote");
        }
    }
}
