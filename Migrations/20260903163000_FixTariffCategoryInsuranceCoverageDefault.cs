using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class FixTariffCategoryInsuranceCoverageDefault : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bug fix (di luar roadmap, ditemukan lewat laporan pengguna atas badge coverage per
            // item di Menu Pembayaran). Migration 20260902072756_DropTableMstBillingCategory
            // menambahkan kolom IsCoveredByInsuranceDefault ke MstTariffCategory dengan
            // defaultValue: false - berbeda dari model C#-nya sendiri (MstTariffCategory.cs,
            // "= true"). AddColumn dengan default pada tabel yang sudah berisi baris melakukan
            // backfill nilai itu ke SEMUA baris existing, sehingga seluruh kategori tarif saat ini
            // punya IsCoveredByInsuranceDefault = false - membuat SEMUA item invoice apa pun
            // kategorinya selalu dianggap tidak layak coverage sama sekali di
            // BillingCalculationService.BuildCoverageComponents, terlepas dari rule asuransi
            // (MstInsuranceCoverageRule) yang sudah dikonfigurasi dengan benar.
            //
            // Tidak ada bukti satu pun kategori sengaja diset false secara manual - tidak ada API
            // yang bahkan mengekspos field ini untuk diedit. Pengecualian per-item/per-kategori
            // yang sesungguhnya sudah ditangani terpisah lewat MstInsuranceCoverageRule dengan
            // CoverageStatus=NotCovered, bukan lewat mematikan seluruh kategori di sini.
            //
            // Migration ini murni koreksi DATA (bukan perubahan bentuk kolom) - defaultValue
            // kolom di level SQL sengaja TIDAK diubah, karena properti C# "= true" sudah cukup
            // membuat EF menyimpan true untuk kategori baru yang dibuat lewat jalur aplikasi
            // normal (SaveChanges); DEFAULT constraint di SQL hanya relevan untuk INSERT yang
            // melewati EF sama sekali, dan mengubahnya berarti mengedit migration lama yang sudah
            // pernah berjalan - di luar scope perbaikan ini.
            migrationBuilder.Sql(
                "UPDATE \"MstTariffCategory\" SET \"IsCoveredByInsuranceDefault\" = true;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reversibel murni secara teknis - mengembalikan ke state SEBELUM perbaikan ini
            // (yang sesungguhnya buggy: seluruh kategori tidak coverable). Bukan berarti state
            // sebelum ini yang benar; disediakan hanya supaya migration tetap reversible.
            migrationBuilder.Sql(
                "UPDATE \"MstTariffCategory\" SET \"IsCoveredByInsuranceDefault\" = false;");
        }
    }
}
