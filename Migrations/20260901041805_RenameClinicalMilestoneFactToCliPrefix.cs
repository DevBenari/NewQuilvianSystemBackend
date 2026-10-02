using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// Menormalkan ledger fakta klinis dari prefix legacy <c>Trx</c> ke prefix canonical
    /// <c>Cli</c> milik ClinicalManagement (QBE-NAM-003).
    ///
    /// Migration ini hanya mengganti nama. Tabel, kolom, tipe data, index, constraint, dan
    /// seluruh barisnya tetap sama persis; tidak ada DROP, CREATE, TRUNCATE, DELETE, maupun
    /// penyalinan data (QBE-DB-002). Karena itu jumlah baris sebelum dan sesudah migration
    /// wajib identik, dan <see cref="Down"/> mengembalikan seluruh nama ke bentuk semula
    /// tanpa kehilangan satu baris pun.
    ///
    /// Nama PK dan FK diganti lewat <c>ALTER TABLE ... RENAME CONSTRAINT</c> karena
    /// <c>MigrationBuilder</c> tidak menyediakan operasi rename constraint, dan karena pada
    /// PostgreSQL bentuk itu murni penggantian nama di katalog: PK tidak dibangun ulang dan
    /// FK tidak pernah dilepas sehingga tidak ada jeda tanpa integritas referensial maupun
    /// pemindaian validasi ulang seisi tabel.
    ///
    /// Nama constraint wajib ikut dinormalkan: EF menurunkan nama constraint dari nama tabel,
    /// sehingga membiarkannya sebagai <c>PK_TrxClinicalMilestoneFact</c> akan membuat
    /// migration berikutnya yang menyentuh key ini mencari nama yang tidak ada di database.
    /// </summary>
    public partial class RenameClinicalMilestoneFactToCliPrefix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "TrxClinicalMilestoneFact",
                schema: "public",
                newName: "CliClinicalMilestoneFact",
                newSchema: "public");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"CliClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"PK_TrxClinicalMilestoneFact\" " +
                "TO \"PK_CliClinicalMilestoneFact\";");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"CliClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId\" " +
                "TO \"FK_CliClinicalMilestoneFact_TrxPatientEncounter_EncounterId\";");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_DispatchStatus",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_CliClinicalMilestoneFact_DispatchStatus");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_EncounterId",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_CliClinicalMilestoneFact_EncounterId");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_IdempotencyKey",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_CliClinicalMilestoneFact_IdempotencyKey");

            // Nama index dipotong PostgreSQL pada batas 63 karakter. Karena `Trx` dan `Cli`
            // sama-sama tiga karakter, posisi pemotongannya tidak berubah.
            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_CliClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~");

            migrationBuilder.RenameIndex(
                name: "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_CliClinicalMilestoneFact_SourceContext_SourceAggregateId_So~");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameIndex(
                name: "IX_CliClinicalMilestoneFact_SourceContext_SourceAggregateId_So~",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~");

            migrationBuilder.RenameIndex(
                name: "IX_CliClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~");

            migrationBuilder.RenameIndex(
                name: "IX_CliClinicalMilestoneFact_IdempotencyKey",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_TrxClinicalMilestoneFact_IdempotencyKey");

            migrationBuilder.RenameIndex(
                name: "IX_CliClinicalMilestoneFact_EncounterId",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_TrxClinicalMilestoneFact_EncounterId");

            migrationBuilder.RenameIndex(
                name: "IX_CliClinicalMilestoneFact_DispatchStatus",
                schema: "public",
                table: "CliClinicalMilestoneFact",
                newName: "IX_TrxClinicalMilestoneFact_DispatchStatus");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"CliClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"FK_CliClinicalMilestoneFact_TrxPatientEncounter_EncounterId\" " +
                "TO \"FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId\";");

            migrationBuilder.Sql(
                "ALTER TABLE public.\"CliClinicalMilestoneFact\" " +
                "RENAME CONSTRAINT \"PK_CliClinicalMilestoneFact\" " +
                "TO \"PK_TrxClinicalMilestoneFact\";");

            migrationBuilder.RenameTable(
                name: "CliClinicalMilestoneFact",
                schema: "public",
                newName: "TrxClinicalMilestoneFact",
                newSchema: "public");
        }
    }
}
