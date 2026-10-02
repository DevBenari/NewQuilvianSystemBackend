using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-099 / migration R4. Lima kolom pada <c>PhmPrescriptionItem</c> milik
    /// <c>PharmacyManagement</c> — kamus data 0.5 bagian 13.1: penghentian butir
    /// (<c>IsStopped</c>, <c>StoppedAt</c>, <c>StoppedByUserId</c>, <c>StopReason</c>) dan jenis
    /// dosis (<c>DoseKind</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Maju.</b> Seluruh kolom nullable atau berbawaan, sehingga nol baris lama perlu diisi:
    /// butir lama terbaca aktif (<c>IsStopped = false</c>) dan berdosis tetap (<c>DoseKind = 0</c>).
    /// </para>
    /// <para>
    /// <b>Mundur</b> menghapus kelima kolom — kriteria 5 — tetapi <b>ditolak</b> bila sudah ada butir
    /// yang dihentikan atau berdosis sliding scale. Menghapus kolom itu berarti menghapus jejak
    /// penghentian terapi yang wajib tahan lama (permission-audit-matrix 0.6.0 bagian 9.1).
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916004000_AddPrescriptionItemStopAndDoseKind")]
    public partial class AddPrescriptionItemStopAndDoseKind : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsStopped",
                schema: "public",
                table: "PhmPrescriptionItem",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "StoppedAt",
                schema: "public",
                table: "PhmPrescriptionItem",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StopReason",
                schema: "public",
                table: "PhmPrescriptionItem",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DoseKind",
                schema: "public",
                table: "PhmPrescriptionItem",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionItem_StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem",
                column: "StoppedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_PhmPrescriptionItem_PrescriptionId_IsStopped",
                schema: "public",
                table: "PhmPrescriptionItem",
                columns: new[] { "PrescriptionId", "IsStopped" });

            migrationBuilder.AddForeignKey(
                name: "FK_PhmPrescriptionItem_AspNetUsers_StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem",
                column: "StoppedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_berjejak integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah_berjejak
                    FROM public.""PhmPrescriptionItem""
                    WHERE ""IsStopped"" = true OR ""DoseKind"" <> 0;

                    IF jumlah_berjejak > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-099: rollback ditolak. % butir resep sudah dihentikan atau berdosis sliding scale; menghapus kolomnya menghapus jejak klinis.',
                            jumlah_berjejak;
                    END IF;
                END $$;");

            migrationBuilder.DropForeignKey(
                name: "FK_PhmPrescriptionItem_AspNetUsers_StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropIndex(
                name: "IX_PhmPrescriptionItem_PrescriptionId_IsStopped",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropIndex(
                name: "IX_PhmPrescriptionItem_StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropColumn(
                name: "DoseKind",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropColumn(
                name: "IsStopped",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropColumn(
                name: "StopReason",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropColumn(
                name: "StoppedAt",
                schema: "public",
                table: "PhmPrescriptionItem");

            migrationBuilder.DropColumn(
                name: "StoppedByUserId",
                schema: "public",
                table: "PhmPrescriptionItem");
        }
    }
}
