using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddRadSafetyRuleApprovalLifecycle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // URUTAN PADA MIGRATION INI DISENGAJA DAN TIDAK BOLEH DIBALIK.
            //
            // Rencana migration pada RAD-ARCH-BE-001 bagian 8 menyatakan: isi RuleStatus lebih
            // dulu, baru ubah index. Membaliknya membuat index unik parsial terbentuk di atas
            // kolom yang seluruhnya masih bernilai bawaan.
            //
            // Yang lebih berbahaya lagi: gerbang keselamatan radiologi bersifat fail-closed,
            // dan hanya aturan ber-RuleStatus = 3 (Active) yang dinilai. Bila baris lama
            // dibiarkan pada nilai bawaan, seluruh aturan keselamatan yang selama ini berlaku
            // mendadak berhenti dihitung — dan setiap pemeriksaan radiologi langsung tertolak
            // begitu migration ini dijalankan.
            //
            // Karena itu urutannya: tambah kolom, isi data lama, baru sentuh index.

            migrationBuilder.AddColumn<DateTime>(
                name: "RejectedAt",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RejectedByUserId",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            // Nilai bawaan 1 = RadSafetyRuleStatus.Draft, bukan 0. Nol bukan anggota enum yang
            // sah, dan baris yang tertinggal pada nilai itu akan terbaca sebagai keadaan yang
            // tidak ada artinya. Draft juga fail-closed: aturan berstatus Draft tidak ikut
            // dinilai gerbang keselamatan, sehingga baris yang entah bagaimana lolos dari
            // pengisian di bawah tidak akan pernah meloloskan pemeriksaan secara keliru.
            migrationBuilder.AddColumn<int>(
                name: "RuleStatus",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<DateTime>(
                name: "SubmittedAt",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SubmittedByUserId",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                type: "uuid",
                nullable: true);

            // Pengisian data lama. Aturan yang selama ini berlaku diturunkan dari IsActive,
            // sesuai tabel pada RAD-ARCH-BE-001 bagian 8:
            //
            //   IsActive = true  dan IsDelete = false  -> Active   (3)
            //   IsActive = false                        -> Inactive (4)
            //   IsDelete = true                         -> Inactive (4)
            //
            // Ditulis sebagai satu pernyataan supaya tidak ada baris yang tertinggal di antara
            // dua perintah terpisah.
            migrationBuilder.Sql(@"
                UPDATE public.""MstRadModalitySafetyRule""
                SET ""RuleStatus"" = CASE
                    WHEN ""IsActive"" = true AND ""IsDelete"" = false THEN 3
                    ELSE 4
                END;
            ");

            // Baru setelah seluruh baris terisi, index unik parsial dipindahkan dari IsActive
            // ke RuleStatus.
            migrationBuilder.DropIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequi~",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequi~",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                columns: new[] { "ModalityId", "ProcedureId", "SafetyRequirementId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"RuleStatus\" = 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequi~",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "RejectedAt",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "RejectedByUserId",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "RuleStatus",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "SubmittedAt",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.DropColumn(
                name: "SubmittedByUserId",
                schema: "public",
                table: "MstRadModalitySafetyRule");

            migrationBuilder.CreateIndex(
                name: "IX_MstRadModalitySafetyRule_ModalityId_ProcedureId_SafetyRequi~",
                schema: "public",
                table: "MstRadModalitySafetyRule",
                columns: new[] { "ModalityId", "ProcedureId", "SafetyRequirementId" },
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsActive\" = true");
        }
    }
}
