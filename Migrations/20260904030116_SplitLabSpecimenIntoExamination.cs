using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Memindahkan jenis pemeriksaan dan salinan tarif dari wadah ke baris pemeriksaan
    /// (<c>LAB-DEC-024</c>, <c>FR-02.4</c>, <c>FR-02.6</c>, <c>BE-LAB-11</c>).
    ///
    /// Tabel <c>LabExamination</c> sudah dibentuk <c>BE-LAB-09</c> dan sudah diisi kode sejak
    /// <c>BE-LAB-12</c>, sehingga yang tersisa di sini hanya melepas keenam kolom yang tidak
    /// dibaca siapa pun lagi.
    /// </summary>
    public partial class SplitLabSpecimenIntoExamination : Migration
    {
        /// <summary>
        /// Menolak berjalan ketika masih ada baris wadah yang memuat salinan tarif.
        ///
        /// Rencana migration pada <c>02-backend-architecture.md</c> bagian 7 menyebut pemindahan
        /// data lama sebagai <b>prasyarat mutlak</b>: setiap wadah lama wajib lebih dulu menjadi
        /// satu wadah dan satu pemeriksaan, dan baris pemeriksaannya wajib memakai kembali
        /// identitas wadah lama supaya tautan <c>BilChargeLine.SourceItemId</c> tidak putus.
        ///
        /// Penjaga ini menerjemahkan prasyarat itu menjadi kode. Pada basis data yang tabelnya
        /// kosong — sebagaimana jawaban <c>LAB-OPEN-012</c> atas dev pemilik — ia tidak melakukan
        /// apa pun. Pada basis data yang masih berisi, ia menghentikan migration <b>sebelum</b>
        /// satu kolom pun dihapus, sehingga salah jalankan berakhir sebagai penolakan dan bukan
        /// sebagai salinan tarif yang hilang tanpa jejak.
        /// </summary>
        private const string PenjagaDataLama = @"
DO $$
DECLARE jumlah bigint;
BEGIN
    SELECT COUNT(*) INTO jumlah FROM public.""LabSpecimen"";

    IF jumlah > 0 THEN
        RAISE EXCEPTION
            'LAB-OPEN-012: tabel LabSpecimen masih memuat % baris, sehingga keenam kolom salinan tarif tidak dihapus. Pindahkan lebih dahulu setiap baris menjadi baris LabExamination yang MEMAKAI KEMBALI identitas wadah lama. Lihat 02-backend-architecture.md bagian 7 langkah 3.', jumlah;
    END IF;
END $$;
";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(PenjagaDataLama);

            migrationBuilder.DropForeignKey(
                name: "FK_LabSpecimen_MstProcedure_ProcedureId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropIndex(
                name: "IX_LabSpecimen_ProcedureId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "ProcedureCodeSnapshot",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "ProcedureId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "ProcedureNameSnapshot",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "TariffCodeSnapshot",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "TariffId",
                schema: "public",
                table: "LabSpecimen");

            migrationBuilder.DropColumn(
                name: "UnitPriceSnapshot",
                schema: "public",
                table: "LabSpecimen");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ProcedureCodeSnapshot",
                schema: "public",
                table: "LabSpecimen",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProcedureId",
                schema: "public",
                table: "LabSpecimen",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "ProcedureNameSnapshot",
                schema: "public",
                table: "LabSpecimen",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TariffCodeSnapshot",
                schema: "public",
                table: "LabSpecimen",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "TariffId",
                schema: "public",
                table: "LabSpecimen",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "UnitPriceSnapshot",
                schema: "public",
                table: "LabSpecimen",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabSpecimen_ProcedureId",
                schema: "public",
                table: "LabSpecimen",
                column: "ProcedureId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabSpecimen_MstProcedure_ProcedureId",
                schema: "public",
                table: "LabSpecimen",
                column: "ProcedureId",
                principalSchema: "public",
                principalTable: "MstProcedure",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
