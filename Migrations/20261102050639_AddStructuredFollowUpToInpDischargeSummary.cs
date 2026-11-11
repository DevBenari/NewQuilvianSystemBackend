using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-256 / FIX-EPS-043-02. Rencana kontrol terstruktur pada resume pulang: tanggal
    /// kontrol, poliklinik tujuan, dan dokter tujuan, pada resume beserta tabel revisinya.
    /// </summary>
    /// <remarks>
    /// SELURUHNYA NULLABLE. Resume lama hanya punya teks FollowUpInstruction dan tetap terbaca;
    /// migration tidak mengisi kolom baru. Tabel revisi menyimpan salinan tanpa foreign key,
    /// karena ia catatan riwayat.
    ///
    /// LANGKAH MUNDUR. Down() membuang keenam kolom. Aman selama belum ada resume yang mengisi
    /// rencana kontrol terstruktur; sesudahnya, membuangnya menghapus rencana kontrol yang
    /// tercatat, sedangkan teks FollowUpInstruction tetap utuh.
    /// </remarks>
    public partial class AddStructuredFollowUpToInpDischargeSummary : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FollowUpDate",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummaryRevision",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "FollowUpDate",
                schema: "public",
                table: "InpDischargeSummary",
                type: "date",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_FollowUpDate",
                schema: "public",
                table: "InpDischargeSummary",
                column: "FollowUpDate");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "FollowUpDoctorId");

            migrationBuilder.CreateIndex(
                name: "IX_InpDischargeSummary_FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "FollowUpServiceUnitId");

            migrationBuilder.AddForeignKey(
                name: "FK_InpDischargeSummary_MstDoctor_FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "FollowUpDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_InpDischargeSummary_MstServiceUnit_FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary",
                column: "FollowUpServiceUnitId",
                principalSchema: "public",
                principalTable: "MstServiceUnit",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_InpDischargeSummary_MstDoctor_FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropForeignKey(
                name: "FK_InpDischargeSummary_MstServiceUnit_FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropIndex(
                name: "IX_InpDischargeSummary_FollowUpDate",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropIndex(
                name: "IX_InpDischargeSummary_FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropIndex(
                name: "IX_InpDischargeSummary_FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummaryRevision");

            migrationBuilder.DropColumn(
                name: "FollowUpDate",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropColumn(
                name: "FollowUpDoctorId",
                schema: "public",
                table: "InpDischargeSummary");

            migrationBuilder.DropColumn(
                name: "FollowUpServiceUnitId",
                schema: "public",
                table: "InpDischargeSummary");
        }
    }
}
