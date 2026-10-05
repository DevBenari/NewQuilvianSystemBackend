using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Mempercepat daftar Running Invoice (GET /billing/invoices) saat data sudah banyak. Tidak memerlukan
    /// ekstensi PostgreSQL apa pun.
    ///
    /// 1. Kolom BilInvoice.VisitDate: salinan tanggal kunjungan, supaya saringan tanggal tidak lagi memakai CASE
    ///    lintas tabel invoice dan kunjungan. Baris lama diisi dari RegPatientEncounter.EncounterDate; bila
    ///    kunjungannya tidak terbaca dipakai InvoiceDate lalu CreateDateTime (aturan yang sama dengan query lama).
    /// 2. Indeks BilInvoice (Status, CreateDateTime menurun) dan (VisitDate), keduanya parsial IsDelete = false.
    ///
    /// Indeks trigram (pg_trgm) untuk pencarian teks sengaja tidak disertakan karena memerlukan ekstensi yang
    /// hanya bisa dipasang akun superuser. Pencarian teks tetap berjalan, hanya belum dipercepat.
    ///
    /// Catatan operasional:
    /// - CREATE INDEX di sini tidak CONCURRENTLY: penulisan ke BilInvoice tertahan selama indeks dibangun.
    /// - UPDATE pengisian VisitDate menulis ulang seluruh baris BilInvoice satu kali.
    /// </summary>
    public partial class OptimizeRunningInvoiceQueries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "VisitDate",
                schema: "public",
                table: "BilInvoice",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.Sql(
                "UPDATE public.\"BilInvoice\" AS i " +
                "SET \"VisitDate\" = COALESCE(" +
                "(SELECT e.\"EncounterDate\" FROM public.\"RegPatientEncounter\" AS e WHERE e.\"Id\" = i.\"EncounterId\"), " +
                "i.\"InvoiceDate\", " +
                "i.\"CreateDateTime\");");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_Status_CreateDateTime",
                schema: "public",
                table: "BilInvoice",
                columns: new[] { "Status", "CreateDateTime" },
                descending: new[] { false, true },
                filter: "\"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_BilInvoice_VisitDate",
                schema: "public",
                table: "BilInvoice",
                column: "VisitDate",
                filter: "\"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_VisitDate",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropIndex(
                name: "IX_BilInvoice_Status_CreateDateTime",
                schema: "public",
                table: "BilInvoice");

            migrationBuilder.DropColumn(
                name: "VisitDate",
                schema: "public",
                table: "BilInvoice");
        }
    }
}
