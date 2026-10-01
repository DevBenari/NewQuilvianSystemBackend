using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// <c>BE-RWI-141</c> / keputusan K5 (<c>rencana-kerja/soap/soap.md</c> Rev 2.1) — satu kolom
    /// <c>TrxDoctorConsultation.SourceVitalSignId</c> yang menunjuk baris <c>TrxPatientVitalSign</c>
    /// sumber snapshot tanda vital catatan dokter, beserta index dan FK <c>Restrict</c>.
    /// </summary>
    /// <remarks>
    /// Nullable: catatan lama dan catatan tanpa sumber tetap terbaca apa adanya, tanpa mematikan
    /// layanan. Mundur hanya melepas tautan asal-usul; snapshot nilai tanda vital pada catatan dokter
    /// dan baris ukuran dokter pada deret tetap utuh.
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260930110000_AddDoctorConsultationSourceVitalSign")]
    public partial class AddDoctorConsultationSourceVitalSign : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxDoctorConsultation_SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "SourceVitalSignId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxDoctorConsultation_SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation",
                column: "SourceVitalSignId",
                principalSchema: "public",
                principalTable: "TrxPatientVitalSign",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TrxDoctorConsultation_SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropIndex(
                name: "IX_TrxDoctorConsultation_SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation");

            migrationBuilder.DropColumn(
                name: "SourceVitalSignId",
                schema: "public",
                table: "TrxDoctorConsultation");
        }
    }
}
