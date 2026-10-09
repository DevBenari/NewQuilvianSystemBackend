using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddOutpatientQueueClassificationAndRenameRegQueue : Migration
    {
        // RJ-DOC-REV-BE-015 / RJ-DOC-REV-BE-016 (Amendment AQ, RJ-DOC-DEC-055..061).
        //
        // 1. LEGACY MIGRATION TrxQueue -> RegQueue (RegistrationManagement, prefix `Reg`,
        //    QBE-NAM-001/003). Scaffolder EF menghasilkan DropTable + CreateTable; diganti
        //    RENAME berbasis katalog Postgres mengikuti preseden
        //    RenameTrxPatientEncounterGuarantorToRegPrefix, sehingga data tetap utuh (QBE-DB-002).
        //    Panjang `TrxQueue` dan `RegQueue` sama (8 karakter), sehingga nama index/constraint
        //    yang terpotong 63 karakter tetap sama dengan yang dihasilkan EF.
        //    Tidak ada identifier lain yang memuat `TrxQueue` sebagai bagian nama (diaudit 8 Okt 2026).
        // 2. Snapshot klasifikasi antrean dan QueueScopeKey + unique index nomor per cakupan.
        // 3. Policy antrean pada MstMembershipTier dan audience pada MstQueueDisplayDevice.

        private const string Peta = @"
                'TrxQueue', 'RegQueue'";

        private const string PetaBalik = @"
                'RegQueue', 'TrxQueue'";

        private static string Skrip(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                -- 1. Nama tabel.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relname = lama AND c.relkind = 'r'
                    ) THEN
                        EXECUTE format('ALTER TABLE public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;

                -- 2. Constraint yang namanya memuat nama tabel lama, termasuk FK milik tabel
                --    klinis yang menunjuk ke antrean. Mengganti nama constraint PK/unique
                --    sekaligus mengganti nama index penopangnya.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT t.relname AS tabel, c.conname AS nama
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;

                -- 3. Sisa index yang tidak ditopang constraint.
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];
                    FOR r IN
                        SELECT c.relname AS nama
                        FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i'
                          AND c.relname LIKE '%' || lama || '%'
                    LOOP
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I',
                                       r.nama, replace(r.nama, lama, baru));
                    END LOOP;
                END LOOP;
            END
            $qbe$;
            """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));

            migrationBuilder.AddColumn<int>(
                name: "QueuePriorityLevelSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "QueueAudienceSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PublicDisplayModeSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<Guid>(
                name: "PatientMembershipIdSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "MembershipTierIdSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MembershipTierCodeSnapshot",
                schema: "public",
                table: "RegQueue",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PriorityReasonCode",
                schema: "public",
                table: "RegQueue",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "QueueScopeKey",
                schema: "public",
                table: "RegQueue",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            // Antrean lama: kunci cakupan = poliklinik, atau service unit bila tanpa poliklinik.
            // Keunikannya sudah dijamin index lama per poliklinik/service unit, sehingga
            // index baru di bawah tidak bentrok dengan data existing.
            migrationBuilder.Sql(
                "UPDATE public.\"RegQueue\" SET \"QueueScopeKey\" = COALESCE(\"ClinicId\", \"ServiceUnitId\");");

            migrationBuilder.CreateIndex(
                name: "IX_RegQueue_QueueDate_ServiceUnitId_QueueScopeKey_QueueNumber",
                schema: "public",
                table: "RegQueue",
                columns: new[] { "QueueDate", "ServiceUnitId", "QueueScopeKey", "QueueNumber" },
                unique: true,
                filter: "\"IsDelete\" = false");

            migrationBuilder.AddColumn<int>(
                name: "QueueAudienceMode",
                schema: "public",
                table: "MstQueueDisplayDevice",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "PublicDisplayMode",
                schema: "public",
                table: "MstMembershipTier",
                type: "integer",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "QueueAudience",
                schema: "public",
                table: "MstMembershipTier",
                type: "integer",
                nullable: false,
                defaultValue: 2);

            // Tier bertipe Regular (TierType = 1) masuk layar Regular; tier lain layar Member.
            // Admin dapat mengubahnya per tier lewat layar Membership Tier.
            migrationBuilder.Sql(
                "UPDATE public.\"MstMembershipTier\" SET \"QueueAudience\" = 1 WHERE \"TierType\" = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "QueueAudience",
                schema: "public",
                table: "MstMembershipTier");

            migrationBuilder.DropColumn(
                name: "PublicDisplayMode",
                schema: "public",
                table: "MstMembershipTier");

            migrationBuilder.DropColumn(
                name: "QueueAudienceMode",
                schema: "public",
                table: "MstQueueDisplayDevice");

            migrationBuilder.DropIndex(
                name: "IX_RegQueue_QueueDate_ServiceUnitId_QueueScopeKey_QueueNumber",
                schema: "public",
                table: "RegQueue");

            migrationBuilder.DropColumn(name: "QueueScopeKey", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "PriorityReasonCode", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "MembershipTierCodeSnapshot", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "MembershipTierIdSnapshot", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "PatientMembershipIdSnapshot", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "PublicDisplayModeSnapshot", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "QueueAudienceSnapshot", schema: "public", table: "RegQueue");
            migrationBuilder.DropColumn(name: "QueuePriorityLevelSnapshot", schema: "public", table: "RegQueue");

            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
