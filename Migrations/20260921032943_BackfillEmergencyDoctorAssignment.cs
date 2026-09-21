using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    /// <remarks>
    /// BE-IGD-048, IGD-DEC-136. Melonggarkan AssignedByUserId menjadi nullable (bagian EF), lalu
    /// mengisi riwayat penugasan dokter untuk kunjungan IGD lama (bagian SQL manual).
    ///
    /// Baris hasil pengisian ini dikenali dari tiga hal sekaligus: AssignmentReason berisi
    /// marker di bawah, AssignedByUserId NULL (pelaku historis tidak dapat dibuktikan), dan
    /// CreateBy Guid.Empty (cap sistem). Down() hanya menghapus baris seperti itu, dan hanya
    /// yang belum pernah ditutup pengalihan dokter nyata (EffectiveTo masih NULL).
    /// </remarks>
    public partial class BackfillEmergencyDoctorAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedByUserId",
                schema: "public",
                table: "EmgDoctorAssignment",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            // Wajib sesudah AlterColumn: kolom harus sudah boleh NULL. EffectiveFrom adalah
            // historical fallback (waktu kedatangan pasien), BUKAN waktu penetapan dokter yang
            // terbukti. NOT EXISTS memakai EffectiveTo IS NULL tanpa menyaring IsDelete, persis
            // seperti unique bersyarat IX_EmgDoctorAssignment_EmergencyVisitId_Active, sehingga
            // pernyataan ini idempoten dan tidak dapat melanggar index itu.
            migrationBuilder.Sql("""
                INSERT INTO public."EmgDoctorAssignment" (
                    "Id", "EmergencyVisitId", "DoctorId", "EffectiveFrom", "EffectiveTo",
                    "AssignedByUserId", "AssignmentReason",
                    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
                    "IsCancel", "IsDelete")
                SELECT
                    gen_random_uuid(), v."Id", e."DoctorId",
                    v."ArrivalDateTime", NULL::timestamptz,
                    NULL::uuid, 'Data historis - pengisian BE-IGD-048',
                    NOW(),
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    '00000000-0000-0000-0000-000000000000'::uuid,
                    FALSE, FALSE
                FROM public."EmgVisit" v
                JOIN public."RegPatientEncounter" e ON e."Id" = v."EncounterId"
                WHERE v."EncounterId" IS NOT NULL
                  AND e."DoctorId" IS NOT NULL
                  AND NOT v."IsDelete"
                  AND NOT e."IsDelete"
                  AND NOT EXISTS (
                      SELECT 1 FROM public."EmgDoctorAssignment" a
                      WHERE a."EmergencyVisitId" = v."Id" AND a."EffectiveTo" IS NULL);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Guard A. Baris hasil BE-IGD-048 yang EffectiveTo-nya sudah terisi telah ditutup
            // pengalihan dokter nyata, jadi sudah menjadi riwayat klinis yang dipakai. Rollback
            // berhenti; riwayat itu tidak boleh dihapus.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (
                        SELECT 1 FROM public."EmgDoctorAssignment"
                        WHERE "AssignedByUserId" IS NULL
                          AND "AssignmentReason" = 'Data historis - pengisian BE-IGD-048'
                          AND "CreateBy" = '00000000-0000-0000-0000-000000000000'::uuid
                          AND "EffectiveTo" IS NOT NULL) THEN
                        RAISE EXCEPTION 'Down BE-IGD-048 dihentikan: ada penugasan hasil pengisian data lama yang sudah ditutup (EffectiveTo terisi) oleh pengalihan dokter nyata. Riwayat itu tidak boleh dihapus.';
                    END IF;
                END $$;
                """);

            // Hanya baris BE-IGD-048 yang masih persis seperti saat disisipkan.
            migrationBuilder.Sql("""
                DELETE FROM public."EmgDoctorAssignment"
                WHERE "AssignedByUserId" IS NULL
                  AND "AssignmentReason" = 'Data historis - pengisian BE-IGD-048'
                  AND "CreateBy" = '00000000-0000-0000-0000-000000000000'::uuid
                  AND "EffectiveTo" IS NULL;
                """);

            // Guard C. Wajib sebelum AlterColumn NOT NULL di bawah. Tanpanya EF mengisi NULL
            // yang tersisa dengan GUID kosong, yang bukan pengguna, dan kegagalan baru muncul
            // di foreign key.
            migrationBuilder.Sql("""
                DO $$
                BEGIN
                    IF EXISTS (SELECT 1 FROM public."EmgDoctorAssignment" WHERE "AssignedByUserId" IS NULL) THEN
                        RAISE EXCEPTION 'Down BE-IGD-048 dihentikan: masih ada baris dengan AssignedByUserId NULL yang bukan hasil pengisian data lama BE-IGD-048.';
                    END IF;
                END $$;
                """);

            migrationBuilder.AlterColumn<Guid>(
                name: "AssignedByUserId",
                schema: "public",
                table: "EmgDoctorAssignment",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
