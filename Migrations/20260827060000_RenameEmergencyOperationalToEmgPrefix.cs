using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260827060000_RenameEmergencyOperationalToEmgPrefix")]
    public partial class RenameEmergencyOperationalToEmgPrefix : Migration
    {
        // Tujuh tabel operasional IGD yang tersisa pindah ke prefix registry `Emg`.
        //
        // Dengan ini modul IGD selesai dinormalkan: master data (20260827040000), kepergian
        // (20260827030000), detail triase (20260827050000), dan tujuh entitas ini. Tidak ada
        // lagi nama `Trx*` yang dimiliki modul IGD.
        //
        // Kontrak pasal 54 menuntut class, berkas, configuration, DbSet, rujukan, dan tabel
        // fisik dinormalkan bersama. Migration ini bagian fisiknya.
        //
        // Nama index dan constraint dicari dari katalog Postgres, tidak diketik satu per satu:
        // nama FK buatan EF sebagian sudah terpotong pada batas 63 karakter. Hasil penggantian
        // selalu lebih pendek daripada aslinya, jadi tidak ada pemotongan baru maupun tabrakan.
        //
        // Constraint milik tabel lain yang menunjuk ke sini ikut berganti nama, termasuk milik
        // modul di luar IGD seperti `TrxEmergencyStaffingRequest`. Yang berubah hanya nama
        // constraint-nya, bukan tabel milik modul itu.
        //
        // `TrxEmergencyObservation` adalah awalan dari `TrxEmergencyObservationDetail`. Rename
        // tabel memakai pencocokan persis, jadi tidak bisa tertukar. Untuk constraint dan index
        // yang memakai pencocokan LIKE, kedua urutan pemrosesan menghasilkan nama akhir yang
        // sama karena `replace` mengganti seluruh kemunculan.
        //
        // `TrxEmergencyDepartureLegacyPlacement` sengaja tidak ikut: ia tabel arsip tanpa
        // entity, dan Down migration 20260826090500 bergantung pada namanya.

        private const string Peta = @"
                'TrxEmergencyVisit',             'EmgVisit',
                'TrxEmergencyTriage',            'EmgTriage',
                'TrxEmergencyResuscitation',     'EmgResuscitation',
                'TrxEmergencyObservation',       'EmgObservation',
                'TrxEmergencyObservationDetail', 'EmgObservationDetail',
                'TrxEmergencyProcedureDetail',   'EmgProcedureDetail',
                'TrxEmergencyDisposition',       'EmgDisposition'";

        private const string PetaBalik = @"
                'EmgVisit',             'TrxEmergencyVisit',
                'EmgTriage',            'TrxEmergencyTriage',
                'EmgResuscitation',     'TrxEmergencyResuscitation',
                'EmgObservation',       'TrxEmergencyObservation',
                'EmgObservationDetail', 'TrxEmergencyObservationDetail',
                'EmgProcedureDetail',   'TrxEmergencyProcedureDetail',
                'EmgDisposition',       'TrxEmergencyDisposition'";

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
                -- 1. Nama tabel. Pencocokan persis, bukan LIKE.
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

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama, termasuk FK
                --    milik tabel lain yang menunjuk ke sini. Mengganti nama constraint
                --    PK/unique sekaligus mengganti nama index penopangnya.
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

        protected override void Up(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql(Skrip(Peta));

        protected override void Down(MigrationBuilder migrationBuilder)
            => migrationBuilder.Sql(Skrip(PetaBalik));
    }
}
