using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Memindahkan kepemilikan ledger fakta milestone klinis ke modul Billing dan menormalkan
    /// namanya dari <c>TrxClinicalMilestoneFact</c> menjadi <c>BilClinicalMilestoneFact</c>.
    ///
    /// Alasannya adalah kepatuhan, bukan selera. QBE-NAM-001 melarang awalan <c>Trx</c> pada
    /// entity operasional, QBE-NAM-002 mewajibkan awalan modul yang terdaftar, dan QBE-MOD-002
    /// mewajibkan entity persisted berada pada modul yang berstatus ACTIVE. Ledger ini dibaca
    /// dan ditulis oleh Billing, sedangkan folder <c>ClinicalBillingIntegration</c> tidak
    /// terdaftar pada registry mana pun. Pemiliknya adalah Billing, sehingga awalannya
    /// <c>Bil</c>.
    ///
    /// Tabel ini sudah pernah diterapkan oleh migration <c>20260824074649</c> dan berisi bukti
    /// serah terima klinis ke Billing. QBE-DB-002 melarang perbaikan penamaan dilakukan dengan
    /// membuang lalu membuat ulang tabel, karena cara itu menghapus bukti. Karena itu seluruh
    /// operasi di sini murni penggantian nama dan tidak menyentuh satu baris pun.
    ///
    /// PostgreSQL tidak ikut mengganti nama constraint maupun index ketika tabelnya diganti
    /// nama. Ketiga hal itu harus diganti nama secara terpisah, jika tidak nama constraint
    /// lama akan tertinggal dan tidak lagi cocok dengan nama yang dibentuk EF dari model.
    /// </summary>
    public partial class RenameClinicalMilestoneFactToBillingOwnership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TrxClinicalMilestoneFact",
                schema: "public",
                newName: "BilClinicalMilestoneFact",
                newSchema: "public");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"BilClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"PK_TrxClinicalMilestoneFact\" " +
                "TO \"PK_BilClinicalMilestoneFact\";");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"BilClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId\" " +
                "TO \"FK_BilClinicalMilestoneFact_TrxPatientEncounter_EncounterId\";");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_DispatchStatus",
                newName: "IX_BilClinicalMilestoneFact_DispatchStatus",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_EncounterId",
                newName: "IX_BilClinicalMilestoneFact_EncounterId",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_IdempotencyKey",
                newName: "IX_BilClinicalMilestoneFact_IdempotencyKey",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            // Nama index dipotong EF pada batas 63 karakter dan ditutup dengan tilde. Karena
            // "Trx" dan "Bil" sama-sama tiga karakter, titik potongnya tidak bergeser.
            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                newName: "IX_BilClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                newName: "IX_BilClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                schema: "public",
                table: "BilClinicalMilestoneFact");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_BilClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                newName: "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_BilClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                newName: "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_BilClinicalMilestoneFact_IdempotencyKey",
                newName: "IX_TrxClinicalMilestoneFact_IdempotencyKey",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_BilClinicalMilestoneFact_EncounterId",
                newName: "IX_TrxClinicalMilestoneFact_EncounterId",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.RenameIndex(
                name: "IX_BilClinicalMilestoneFact_DispatchStatus",
                newName: "IX_TrxClinicalMilestoneFact_DispatchStatus",
                schema: "public",
                table: "BilClinicalMilestoneFact");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"BilClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"FK_BilClinicalMilestoneFact_TrxPatientEncounter_EncounterId\" " +
                "TO \"FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId\";");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"BilClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"PK_BilClinicalMilestoneFact\" " +
                "TO \"PK_TrxClinicalMilestoneFact\";");

            migrationBuilder.RenameTable(
                name: "BilClinicalMilestoneFact",
                schema: "public",
                newName: "TrxClinicalMilestoneFact",
                newSchema: "public");
        }
    }
}
