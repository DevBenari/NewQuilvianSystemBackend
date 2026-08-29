using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260827040000_RenameEmergencyMasterDataToEmgPrefix")]
    public partial class RenameEmergencyMasterDataToEmgPrefix : Migration
    {
        // Enam tabel master data IGD pindah ke prefix registry `Emg`.
        //
        // Sebabnya kepemilikan, bukan selera. Ketika master data dipindahkan ke dalam
        // `Areas/HealthServices/EmergencyInstallationManagement/MasterData/`, kepemilikannya
        // berpindah ke modul IGD. Prefix `Mst` adalah milik baris registry
        // `Administrator / HealthServices | Master / Reference`, yaitu modul master pusat —
        // bukan modul ini. Sesudah perpindahan itu, folder dan nama saling bertentangan;
        // QBE-MOD-002 menurunkan prefix dari kepemilikan, jadi namanya yang mengikuti.
        //
        // QBE-NAM-003 mewajibkan sumber dan tabel fisik dinormalkan bersama, jadi migration
        // ini adalah pasangan fisik dari rename di sumber.
        //
        // Nama index dan constraint dicari dari katalog, tidak diketik satu per satu. Nama
        // FK buatan EF sebagian sudah terpotong pada batas 63 karakter
        // (`FK_MstEmergencySetting_MstServiceUnit_DefaultEmergencyServiceU`), sehingga
        // mengetiknya dari ingatan berisiko meleset dan menggagalkan migration di tengah
        // jalan. Hasil penggantian selalu lebih pendek daripada aslinya, jadi tidak ada
        // pemotongan baru maupun tabrakan nama.

        private const string Peta = @"
                'MstEmergencyArrivalMode',     'EmgArrivalMode',
                'MstEmergencyCaseType',        'EmgCaseType',
                'MstEmergencyDispositionType', 'EmgDispositionType',
                'MstEmergencySetting',         'EmgSetting',
                'MstEmergencyTriageIndicator', 'EmgTriageIndicator',
                'MstEmergencyTriageLevel',     'EmgTriageLevel'";

        private const string PetaBalik = @"
                'EmgArrivalMode',     'MstEmergencyArrivalMode',
                'EmgCaseType',        'MstEmergencyCaseType',
                'EmgDispositionType', 'MstEmergencyDispositionType',
                'EmgSetting',         'MstEmergencySetting',
                'EmgTriageIndicator', 'MstEmergencyTriageIndicator',
                'EmgTriageLevel',     'MstEmergencyTriageLevel'";

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

                -- 2. Constraint mana pun yang namanya memuat nama tabel lama, termasuk FK
                --    milik tabel Trx* yang menunjuk ke sini. Mengganti nama constraint
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
