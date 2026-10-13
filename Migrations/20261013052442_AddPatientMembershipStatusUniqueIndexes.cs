using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <summary>
    /// BE-CRM-002 (crm-membership). Membership yang dibuat sebelum aturan siklus hidup berlaku
    /// ditandai Perlu Verifikasi (CRM-DEC-021), lalu index unik per status menggantikan unik
    /// <c>IsPrimary</c> lama. Langkah data harus mendahului index: setelah ditandai tidak ada lagi
    /// baris Aktif (1), Menunggu Pembayaran (7), atau Terjadwal (8), sehingga index pasti terbentuk.
    /// Seluruh SQL data idempoten — menjalankannya ulang tidak menggandakan riwayat.
    /// </summary>
    public partial class AddPatientMembershipStatusUniqueIndexes : Migration
    {
        // Nilai enum: MembershipStatus.Active = 1, NeedsVerification = 9;
        // MembershipRegistrationType.New = 1, LegacyImport = 4; MembershipChangeSource.System = 2.
        private const string LegacyFlagReasonPrefix =
            "Ditandai Perlu Verifikasi saat aturan siklus hidup membership dipasang (CRM-DEC-021).";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. RowVersion baris lama masih nol dari ExtendPatientMembershipLifecycle.
            migrationBuilder.Sql("""
                UPDATE public."MstPatientMembership"
                SET "RowVersion" = gen_random_uuid()
                WHERE "RowVersion" = '00000000-0000-0000-0000-000000000000';
                """);

            // 2. Proyeksi status di pasien pemilik data lama dikosongkan; data lama belum terbukti
            //    dibayar sehingga pasien terbaca Reguler sampai CRM memverifikasi.
            migrationBuilder.Sql("""
                UPDATE public."MstPatient" p
                SET "IsMember" = false,
                    "DefaultMembershipTierId" = NULL,
                    "ActivePatientMembershipId" = NULL,
                    "UpdateDateTime" = now()
                WHERE EXISTS (
                    SELECT 1 FROM public."MstPatientMembership" m
                    WHERE m."PatientId" = p."Id"
                      AND m."IsDelete" = false
                      AND m."RegistrationType" <> 4);
                """);

            // 3. Satu baris riwayat per membership lama; nilai IsActive/IsPrimary lama disimpan di
            //    alasan agar Down dapat memulihkannya.
            migrationBuilder.Sql($"""
                INSERT INTO public."PatMembershipStatusHistory" (
                    "Id", "PatientMembershipId", "FromStatus", "ToStatus", "ActionCode", "Source",
                    "ActorUserId", "OccurredAt", "Reason", "ReferenceId", "OldNotes", "NewNotes",
                    "CreateDateTime", "CreateBy", "UpdateDateTime", "UpdateBy", "DeleteDateTime",
                    "DeleteBy", "CancelDateTime", "CancelBy", "IsCancel", "IsDelete")
                SELECT gen_random_uuid(), m."Id", m."MembershipStatus", 9, 'LEGACY_FLAGGED', 2,
                    NULL, now(),
                    '{LegacyFlagReasonPrefix} Nilai lama: IsActive=' || CASE WHEN m."IsActive" THEN 'true' ELSE 'false' END
                        || ', IsPrimary=' || CASE WHEN m."IsPrimary" THEN 'true' ELSE 'false' END || '.',
                    NULL, NULL, NULL,
                    now(), '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', NULL,
                    '00000000-0000-0000-0000-000000000000', NULL, '00000000-0000-0000-0000-000000000000', false, false
                FROM public."MstPatientMembership" m
                WHERE m."IsDelete" = false
                  AND m."RegistrationType" <> 4;
                """);

            // 4. Tandai data lama. IsActive/IsPrimary menjadi proyeksi kompatibilitas: true hanya bila Aktif.
            migrationBuilder.Sql("""
                UPDATE public."MstPatientMembership"
                SET "MembershipStatus" = 9,
                    "RegistrationType" = 4,
                    "IsActive" = false,
                    "IsPrimary" = false,
                    "RowVersion" = gen_random_uuid(),
                    "UpdateDateTime" = now()
                WHERE "IsDelete" = false
                  AND "RegistrationType" <> 4;
                """);

            migrationBuilder.DropIndex(
                name: "IX_MstPatientMembership_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatientMembership_FeeChargeReferenceId",
                schema: "public",
                table: "MstPatientMembership",
                column: "FeeChargeReferenceId",
                unique: true,
                filter: "\"FeeChargeReferenceId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatientMembership_Patient_Active",
                schema: "public",
                table: "MstPatientMembership",
                column: "PatientId",
                unique: true,
                filter: "\"MembershipStatus\" = 1 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatientMembership_Patient_Pending",
                schema: "public",
                table: "MstPatientMembership",
                column: "PatientId",
                unique: true,
                filter: "\"MembershipStatus\" = 7 AND \"IsDelete\" = false");

            migrationBuilder.CreateIndex(
                name: "UX_MstPatientMembership_Patient_Scheduled",
                schema: "public",
                table: "MstPatientMembership",
                column: "PatientId",
                unique: true,
                filter: "\"MembershipStatus\" = 8 AND \"IsDelete\" = false");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_MstPatientMembership_FeeChargeReferenceId",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "UX_MstPatientMembership_Patient_Active",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "UX_MstPatientMembership_Patient_Pending",
                schema: "public",
                table: "MstPatientMembership");

            migrationBuilder.DropIndex(
                name: "UX_MstPatientMembership_Patient_Scheduled",
                schema: "public",
                table: "MstPatientMembership");

            // Pulihkan hanya data lama yang masih Perlu Verifikasi. Baris yang sudah diakui atau
            // ditolak CRM sesudah migration ini adalah keputusan manusia dan tidak dibalik.
            migrationBuilder.Sql("""
                UPDATE public."MstPatientMembership" m
                SET "MembershipStatus" = h."FromStatus",
                    "RegistrationType" = 1,
                    "IsActive" = h."Reason" LIKE '%IsActive=true%',
                    "IsPrimary" = h."Reason" LIKE '%IsPrimary=true%',
                    "UpdateDateTime" = now()
                FROM public."PatMembershipStatusHistory" h
                WHERE h."PatientMembershipId" = m."Id"
                  AND h."ActionCode" = 'LEGACY_FLAGGED'
                  AND h."FromStatus" IS NOT NULL
                  AND m."MembershipStatus" = 9;
                """);

            migrationBuilder.Sql("""
                DELETE FROM public."PatMembershipStatusHistory" h
                USING public."MstPatientMembership" m
                WHERE h."PatientMembershipId" = m."Id"
                  AND h."ActionCode" = 'LEGACY_FLAGGED'
                  AND m."RegistrationType" = 1;
                """);

            // Proyeksi pasien dihitung ulang dengan aturan lama (IsActive && IsPrimary, JoinDate terbaru).
            migrationBuilder.Sql("""
                UPDATE public."MstPatient" p
                SET "IsMember" = (x."Id" IS NOT NULL),
                    "DefaultMembershipTierId" = x."MembershipTierId",
                    "ActivePatientMembershipId" = x."Id",
                    "UpdateDateTime" = now()
                FROM (
                    SELECT pm."PatientId" AS "OwnerId", latest."Id", latest."MembershipTierId"
                    FROM (SELECT DISTINCT "PatientId" FROM public."MstPatientMembership" WHERE "IsDelete" = false) pm
                    LEFT JOIN LATERAL (
                        SELECT m."Id", m."MembershipTierId"
                        FROM public."MstPatientMembership" m
                        WHERE m."PatientId" = pm."PatientId"
                          AND m."IsActive" = true AND m."IsPrimary" = true AND m."IsDelete" = false
                        ORDER BY m."JoinDate" DESC
                        LIMIT 1) latest ON true
                ) x
                WHERE p."Id" = x."OwnerId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_MstPatientMembership_PatientId_IsPrimary",
                schema: "public",
                table: "MstPatientMembership",
                columns: new[] { "PatientId", "IsPrimary" },
                unique: true,
                filter: "\"IsPrimary\" = true AND \"IsActive\" = true AND \"IsDelete\" = false");
        }
    }
}
