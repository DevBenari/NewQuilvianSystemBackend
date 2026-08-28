using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260827050000_RenameEmergencyTriageDetailToEmgPrefix")]
    public partial class RenameEmergencyTriageDetailToEmgPrefix : Migration
    {
        // Detail triase IGD pindah ke prefix registry `Emg`.
        //
        // Prefix modul ini adalah `Emg` menurut baris registry
        // `HealthServices | EmergencyInstallationManagement / Emergency`, dan entitas ini
        // sudah bertetangga langsung dengan `EmgTriageIndicator` di modul yang sama.
        //
        // Kontrak pasal 54 menyatakan normalisasi `Trx*` belum selesai selama class, berkas,
        // configuration, DbSet, rujukan, dan tabel fisik belum dinormalkan bersama. Migration
        // ini adalah pasangan fisik dari rename di sumber; tanpa ia, kode baru dan basis data
        // lama akan saling meleset.
        //
        // Tujuh entitas `Trx*` IGD lain sengaja dibiarkan: mereka legacy yang tidak tersentuh,
        // dan menormalkan `TrxEmergencyVisit` saja menyentuh 30 berkas. Itu batch tersendiri
        // yang perlu jadwal dan koordinasi, bukan bagian dari perbaikan ini.
        //
        // Nama index dan constraint dicari dari katalog Postgres, tidak diketik satu per satu:
        // nama FK buatan EF sebagian sudah terpotong pada batas 63 karakter, misalnya
        // `FK_TrxEmergencyTriageDetail_TrxEmergencyTriage_EmergencyTriage`. Hasil penggantian
        // selalu lebih pendek daripada aslinya, jadi tidak ada pemotongan baru maupun tabrakan.
        //
        // Pencarian memakai nama tabel penuh `TrxEmergencyTriageDetail`, yang lebih panjang
        // daripada nama induknya `TrxEmergencyTriage`, sehingga constraint milik induk tidak
        // ikut terbawa. Bila kelak induk menyusul di-rename dengan pola yang sama, FK anak
        // yang memuat nama induk justru memang harus ikut berubah.

        private const string Peta = @"
                'TrxEmergencyTriageDetail', 'EmgTriageDetail'";

        private const string PetaBalik = @"
                'EmgTriageDetail', 'TrxEmergencyTriageDetail'";

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
