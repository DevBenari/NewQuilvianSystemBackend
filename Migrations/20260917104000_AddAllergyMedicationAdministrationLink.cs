using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-117 / migration K6 — kamus data 0.4 bagian 11.3. Dua kolom nullable pada <c>TrxPatientAllergy</c>
    /// yang menautkan dugaan reaksi obat ke episode dan ke dosis MAR pemicunya. Setelah K4 karena foreign key
    /// ke <c>PhmMedicationAdministration</c>. Alergi lama tidak tersentuh.
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260917104000_AddAllergyMedicationAdministrationLink")]
    public partial class AddAllergyMedicationAdministrationLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "InpEpisodeId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientAllergy_SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "SourceMedicationAdministrationId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientAllergy_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "InpEpisodeId",
                principalSchema: "public",
                principalTable: "InpEpisode",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientAllergy_SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy",
                column: "SourceMedicationAdministrationId",
                principalSchema: "public",
                principalTable: "PhmMedicationAdministration",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Mundur menghapus tautan dugaan reaksi obat ke dosisnya. Ditolak selama tautan itu sudah dipakai.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah integer;
                BEGIN
                    SELECT COUNT(*) INTO jumlah FROM public.""TrxPatientAllergy""
                    WHERE ""InpEpisodeId"" IS NOT NULL OR ""SourceMedicationAdministrationId"" IS NOT NULL;
                    IF jumlah > 0 THEN
                        RAISE EXCEPTION 'BE-RWI-117: rollback ditolak. % alergi sudah tertaut episode atau dosis MAR; menghapus kolom menghapus asal dugaan reaksi obat.', jumlah;
                    END IF;
                END $$;");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientAllergy_SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientAllergy_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAllergy_SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientAllergy_InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy");

            migrationBuilder.DropColumn(
                name: "SourceMedicationAdministrationId",
                schema: "public",
                table: "TrxPatientAllergy");

            migrationBuilder.DropColumn(
                name: "InpEpisodeId",
                schema: "public",
                table: "TrxPatientAllergy");
        }
    }
}
