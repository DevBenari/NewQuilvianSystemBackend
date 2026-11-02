using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class PersempitIndeksCatatanDokterPerKunjungan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"InpEpisodeId\" IS NULL AND \"ClinicId\" IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"InpEpisodeId\" IS NULL");
        }
    }
}
