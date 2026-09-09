using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-068 / INT-DOK-10. Diagnosis terstruktur boleh lahir dari kajian medis awal:
    /// kolom konteks perawatan ditambahkan, dan nomor konsultasi tidak lagi wajib terisi.
    /// </summary>
    /// <remarks>
    /// Nol baris lama disentuh. Seluruh diagnosis yang sudah ada sudah menyebut nomor
    /// konsultasi, dan melepas kewajiban terisi tidak mengubah satu nilai pun.
    ///
    /// LANGKAH MUNDURNYA TIDAK SIMETRIS - 02-backend-architecture.md bagian 7.3 langkah 11.
    /// Mengembalikan ConsultationId menjadi NOT NULL gagal bila sudah ada diagnosis rawat inap
    /// yang lahir tanpa nomor konsultasi. Urutan mundur yang benar:
    ///   1. kembalikan validasinya lebih dulu supaya tidak ada baris baru;
    ///   2. tangani baris yang telanjur ada bersama pemilik klinis;
    ///   3. baru turunkan migration ini.
    /// Menambah InpEpisodeId sendiri mundur tanpa masalah.
    /// </remarks>
    public partial class AddInpatientEpisodeContextToPatientDiagnosis : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultationId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientDiagnosis_InpEpisodeId_PatientId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                columns: new[] { "InpEpisodeId", "PatientId" });

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientDiagnosis_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientDiagnosis_InpEpisode_InpEpisodeId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientDiagnosis_InpEpisodeId_PatientId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientDiagnosis");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultationId",
                schema: "public",
                table: "TrxPatientDiagnosis",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
