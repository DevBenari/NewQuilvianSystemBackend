using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-104 / migration R8. Empat kolom instruksi pada <c>LabOrder</c> (milik
    /// <c>LaboratoryManagement</c>) dan <c>RadOrder</c> (milik <c>RadiologyManagement</c>) — kamus data
    /// 0.5 bagian 13.11. Persetujuan pemilik kedua modul: Yoga Aji, <c>RWI-DEC-153</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Maju</b> tanpa mematikan layanan: seluruh kolom nullable atau berbawaan <c>NotRequired</c>,
    /// sehingga pesanan lama dan pesanan poliklinik/IGD tidak berubah artinya.
    /// </para>
    /// <para>
    /// <b>Mundur</b> menghapus kolom <b>selama bernilai bawaan</b> (kriteria 4). Bila sudah ada pesanan
    /// yang membawa pemberi instruksi atau status verifikasi, rollback ditolak.
    /// </para>
    /// </remarks>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916007000_AddLabRadInstructionColumns")]
    public partial class AddLabRadInstructionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "InstructingDoctorId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "LabOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "LabOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_InstructingDoctorId_InstructionVerificationStatus",
                schema: "public",
                table: "LabOrder",
                columns: new[] { "InstructingDoctorId", "InstructionVerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_LabOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder",
                column: "InstructionVerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LabOrder_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "LabOrder",
                column: "InstructingDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructingDoctorId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "RadOrder",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "RadOrder",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_InstructingDoctorId_InstructionVerificationStatus",
                schema: "public",
                table: "RadOrder",
                columns: new[] { "InstructingDoctorId", "InstructionVerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_RadOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder",
                column: "InstructionVerifiedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RadOrder_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "RadOrder",
                column: "InstructingDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_berinstruksi integer;
                BEGIN
                    SELECT
                        (SELECT COUNT(*) FROM public.""LabOrder"" WHERE ""InstructingDoctorId"" IS NOT NULL OR ""InstructionVerificationStatus"" <> 0)
                      + (SELECT COUNT(*) FROM public.""RadOrder"" WHERE ""InstructingDoctorId"" IS NOT NULL OR ""InstructionVerificationStatus"" <> 0)
                    INTO jumlah_berinstruksi;

                    IF jumlah_berinstruksi > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-104: rollback ditolak. % pesanan laboratorium/radiologi sudah membawa pemberi instruksi atau status verifikasi; kolomnya hanya boleh dihapus selama bernilai bawaan.',
                            jumlah_berinstruksi;
                    END IF;
                END $$;");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_LabOrder_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_InstructingDoctorId_InstructionVerificationStatus",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropIndex(
                name: "IX_LabOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "InstructingDoctorId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "LabOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RadOrder_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropForeignKey(
                name: "FK_RadOrder_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_InstructingDoctorId_InstructionVerificationStatus",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropIndex(
                name: "IX_RadOrder_InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "InstructingDoctorId",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "RadOrder");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "RadOrder");
        }
    }
}
