using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// FIX-EPS-039-03 / RWI-DEC-269 — lima kolom rujukan faskes luar pada episode rawat inap,
    /// semuanya nullable dan additive.
    /// </summary>
    /// <remarks>
    /// Pembuatan otomatis migration ini juga memunculkan perubahan <c>CliTransferHandover</c>
    /// (kolom placement nullable dan indeks unik berfilter). Perubahan itu sudah dikerjakan
    /// <c>20261022130000_MakeTransferHandoverPlacementsNullable</c> yang ditulis manual tanpa
    /// memperbarui snapshot, sehingga operasinya sengaja tidak diulang di sini; hanya snapshot
    /// model yang diselaraskan.
    /// </remarks>
    public partial class AddInpEpisodeExternalReferral : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ExternalReferralDateTime",
                schema: "public",
                table: "InpEpisode",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReferralDiagnosis",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReferralDoctorName",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReferralFacilityName",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExternalReferralNumber",
                schema: "public",
                table: "InpEpisode",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalReferralDateTime",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ExternalReferralDiagnosis",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ExternalReferralDoctorName",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ExternalReferralFacilityName",
                schema: "public",
                table: "InpEpisode");

            migrationBuilder.DropColumn(
                name: "ExternalReferralNumber",
                schema: "public",
                table: "InpEpisode");
        }
    }
}
