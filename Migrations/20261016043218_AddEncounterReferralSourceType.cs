using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddEncounterReferralSourceType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceDoctorCertificateId",
                schema: "public",
                table: "RegEncounterReferral",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SourceType",
                schema: "public",
                table: "RegEncounterReferral",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.CreateIndex(
                name: "IX_RegEncounterReferral_SourceDoctorCertificateId",
                schema: "public",
                table: "RegEncounterReferral",
                column: "SourceDoctorCertificateId");

            migrationBuilder.AddForeignKey(
                name: "FK_RegEncounterReferral_CliDoctorCertificate_SourceDoctorCerti~",
                schema: "public",
                table: "RegEncounterReferral",
                column: "SourceDoctorCertificateId",
                principalSchema: "public",
                principalTable: "CliDoctorCertificate",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // RJ-REF-REV-005: aturan kelengkapan baru rujukan Rawat Jalan/Laboratorium eksternal
            // (fasilitas + tanggal + unit tujuan). Rujukan lama yang sudah memenuhi aturan baru tidak
            // lagi tertahan "belum lengkap". Rujukan IGD (EncounterType 2) tidak disentuh. Down()
            // tidak mengembalikan flag ini karena nilai lamanya hasil hitung aturan yang dicabut.
            migrationBuilder.Sql(@"
UPDATE public.""RegEncounterReferral"" r
SET ""IsComplete"" = true,
    ""CompletedAt"" = COALESCE(r.""CompletedAt"", now())
FROM public.""RegPatientEncounter"" e
WHERE r.""PatientEncounterId"" = e.""Id""
  AND r.""IsDelete"" = false
  AND r.""IsComplete"" = false
  AND e.""EncounterType"" <> 2
  AND e.""ReferralInstitutionId"" IS NOT NULL
  AND r.""TargetServiceUnitId"" <> '00000000-0000-0000-0000-000000000000';");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RegEncounterReferral_CliDoctorCertificate_SourceDoctorCerti~",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropIndex(
                name: "IX_RegEncounterReferral_SourceDoctorCertificateId",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropColumn(
                name: "SourceDoctorCertificateId",
                schema: "public",
                table: "RegEncounterReferral");

            migrationBuilder.DropColumn(
                name: "SourceType",
                schema: "public",
                table: "RegEncounterReferral");
        }
    }
}
