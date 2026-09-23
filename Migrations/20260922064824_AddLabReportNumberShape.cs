using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddLabReportNumberShape : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReportNumberLength",
                schema: "public",
                table: "LabDisciplineSetting",
                type: "integer",
                nullable: false,
                defaultValue: 4);

            migrationBuilder.AddColumn<string>(
                name: "ReportNumberSeparator",
                schema: "public",
                table: "LabDisciplineSetting",
                type: "character varying(5)",
                maxLength: 5,
                nullable: true);

            // PENGISIAN SEKALI JALAN bagi ketiga baris yang sudah ada.
            //
            // Tanpa ini, baris lama memakai bawaan '-' dan 4 digit — benar bagi Mikrobiologi,
            // SALAH bagi Patologi Anatomi (26.0919) dan Patologi Klinik (25039254). Dan
            // seeder TIDAK akan memperbaikinya: ia hanya menyisipkan, nol memperbarui, justru
            // supaya nol pernah menimpa pekerjaan kepala instalasi.
            //
            // KENAPA INI BUKAN PELANGGARAN PRINSIP ITU: kedua kolom ini baru lahir pada
            // migration ini juga, sehingga nol mungkin ada manusia yang pernah menyetelnya.
            // Yang diisi adalah kolom yang belum pernah punya nilai, dari bukti yang sama
            // dengan yang dipakai seeder (LAB-EVD-005) — bukan menimpa keputusan siapa pun.
            //
            // Syarat "ReportNumberSeparator IS NULL" ditulis agar migration ini tetap aman
            // bila dijalankan pada database yang seseorang sudah menyetelnya lebih dulu.
            migrationBuilder.Sql(@"
                UPDATE public.""LabDisciplineSetting""
                SET ""ReportNumberSeparator"" = '-', ""ReportNumberLength"" = 4
                WHERE ""Discipline"" = 3 AND ""ReportNumberSeparator"" IS NULL;

                UPDATE public.""LabDisciplineSetting""
                SET ""ReportNumberSeparator"" = '.', ""ReportNumberLength"" = 4
                WHERE ""Discipline"" = 2 AND ""ReportNumberSeparator"" IS NULL;

                UPDATE public.""LabDisciplineSetting""
                SET ""ReportNumberSeparator"" = '', ""ReportNumberLength"" = 6
                WHERE ""Discipline"" = 1 AND ""ReportNumberSeparator"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReportNumberLength",
                schema: "public",
                table: "LabDisciplineSetting");

            migrationBuilder.DropColumn(
                name: "ReportNumberSeparator",
                schema: "public",
                table: "LabDisciplineSetting");
        }
    }
}
