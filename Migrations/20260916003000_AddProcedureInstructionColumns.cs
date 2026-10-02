using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-RWI-097 / migration R7. Menambahkan enam kolom instruksi dokter dan pembatalan otomatis penutupan
    /// pada TrxPatientProcedure, serta melonggarkan ConsultationId menjadi nullable khusus untuk jalur
    /// pesanan rawat inap perawat atas instruksi dokter (disetujui Sukma GP lewat RWI-DEC-152).
    /// </summary>
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260916003000_AddProcedureInstructionColumns")]
    public partial class AddProcedureInstructionColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultationId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<bool>(
                name: "CancelledByEpisodeClosure",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructingDoctorId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_InstructingDoctor_Verification",
                schema: "public",
                table: "TrxPatientProcedure",
                columns: new[] { "InstructingDoctorId", "InstructionVerificationStatus" });

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InstructionVerifiedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TrxPatientProcedure_OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "OrderedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InstructionVerifiedByUserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "OrderedByUserId",
                principalSchema: "public",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrxPatientProcedure_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "TrxPatientProcedure",
                column: "InstructingDoctorId",
                principalSchema: "public",
                principalTable: "MstDoctor",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // PENGAMAN ROLLBACK TIDAK SIMETRIS (BE-RWI-097 / RWI-DEC-152):
            // Rollback ditolak bila sudah ada tindakan tanpa ConsultationId karena mengembalikan
            // kolom menjadi NOT NULL akan merusak integritas basis data.
            migrationBuilder.Sql(@"
                DO $$
                DECLARE jumlah_tanpa_konsultasi integer;
                BEGIN
                    SELECT COUNT(*)
                    INTO jumlah_tanpa_konsultasi
                    FROM public.""TrxPatientProcedure""
                    WHERE ""ConsultationId"" IS NULL;

                    IF jumlah_tanpa_konsultasi > 0 THEN
                        RAISE EXCEPTION
                            'BE-RWI-097 / R7: rollback ditolak. % pesanan tindakan rawat inap tersimpan tanpa ConsultationId; mengembalikan kolom menjadi NOT NULL akan merusak integritas data.',
                            jumlah_tanpa_konsultasi;
                    END IF;
                END $$;");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_AspNetUsers_OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropForeignKey(
                name: "FK_TrxPatientProcedure_MstDoctor_InstructingDoctorId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_InstructingDoctor_Verification",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropIndex(
                name: "IX_TrxPatientProcedure_OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "CancelledByEpisodeClosure",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InstructingDoctorId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InstructionVerificationStatus",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedAt",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "InstructionVerifiedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.DropColumn(
                name: "OrderedByUserId",
                schema: "public",
                table: "TrxPatientProcedure");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConsultationId",
                schema: "public",
                table: "TrxPatientProcedure",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
