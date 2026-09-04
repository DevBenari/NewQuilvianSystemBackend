using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class RelaxSingleConsultationAndPrescriptionForInpatient : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription",
                column: "ConsultationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsCancel\" = false AND \"InpEpisodeId\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"InpEpisodeId\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPrescription_ConsultationId",
                schema: "public",
                table: "TrxPrescription",
                column: "ConsultationId",
                unique: true,
                filter: "\"IsDelete\" = false AND \"IsCancel\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_EncounterId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "EncounterId",
                unique: true,
                filter: "\"IsDelete\" = false");
        }
    }
}
