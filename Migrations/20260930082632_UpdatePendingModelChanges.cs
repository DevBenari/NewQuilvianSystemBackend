using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePendingModelChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "BahasaDigunakan",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DaftarMasalah",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosaBanding",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiagnosaSaatIni",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EdukasiKepada",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IndikasiTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAsing",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNAbdomen",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNEkstremitas",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNGenital",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNJantung",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNKepala",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNLeher",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNMata",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNMulut",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNParu",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNPunggung",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNTHT",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDBNThorak",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsDaerah",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "JenisHambatan",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(250)",
                maxLength: 250,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KajianUtamaPengkajian",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanAbdomen",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanAnggotaGerak",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanDada",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanEkstremitas",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanGenitalia",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanJantung",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanKepala",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanKulit",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanLainnya",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanLeher",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanMata",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanMulut",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanParuParu",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanPunggung",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanTHT",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanThorak",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeadaanUmum",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Kesadaran",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KeteranganTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NamaTempat",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PemeriksaanPenunjang",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PenyampaianEdukasi",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Program",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatAlergi",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatPenyakitDahulu",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatPenyakitKeluarga",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RiwayatPenyakitSekarang",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StatusLokalis",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Terapi",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "character varying(3000)",
                maxLength: 3000,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglKajian",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "TglTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BahasaDigunakan",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "DaftarMasalah",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "DiagnosaBanding",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "DiagnosaSaatIni",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "EdukasiKepada",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IndikasiTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsAsing",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNAbdomen",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNEkstremitas",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNGenital",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNJantung",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNKepala",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNLeher",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNMata",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNMulut",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNParu",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNPunggung",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNTHT",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDBNThorak",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "IsDaerah",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "JenisHambatan",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KajianUtamaPengkajian",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanAbdomen",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanAnggotaGerak",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanDada",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanEkstremitas",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanGenitalia",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanJantung",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanKepala",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanKulit",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanLainnya",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanLeher",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanMata",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanMulut",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanParuParu",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanPunggung",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanTHT",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanThorak",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeadaanUmum",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "Kesadaran",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "KeteranganTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "NamaTempat",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "PemeriksaanPenunjang",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "PenyampaianEdukasi",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "Program",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "RiwayatAlergi",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "RiwayatPenyakitDahulu",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "RiwayatPenyakitKeluarga",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "RiwayatPenyakitSekarang",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "StatusLokalis",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "Terapi",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "TglKajian",
                schema: "public",
                table: "TrxPatientAssessment");

            migrationBuilder.DropColumn(
                name: "TglTindakLanjut",
                schema: "public",
                table: "TrxPatientAssessment");
        }
    }
}
