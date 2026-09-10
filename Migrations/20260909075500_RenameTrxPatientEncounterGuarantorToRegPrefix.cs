using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class RenameTrxPatientEncounterGuarantorToRegPrefix : Migration
    {
        // Entity penjamin registrasi dinormalkan ke prefix modul pemiliknya:
        //
        //   TrxPatientEncounterGuarantor -> RegPatientEncounterGuarantor (RegistrationManagement, prefix `Reg`)
        //
        // Entity ini berada di dalam HealthServices/RegistrationManagement. Menurut baris registry
        // `HealthServices | RegistrationManagement / Registration`, prefix yang benar adalah `Reg` (QBE-NAM-001).
        // Kontrak rekayasa backend menyatakan normalisasi belum selesai selama class, berkas,
        // configuration, DbSet, rujukan, dan tabel fisik belum dinormalkan secara konsisten (QBE-NAM-003).
        //
        // Polanya mengikuti preseden migrasi sebelumnya (RenamePharmacyTrxTablesToPhmPrefix):
        // nama constraint dan index dicari dari katalog Postgres, tidak diketik satu per satu.
        // Tidak ada DROP+CREATE di sini (QBE-DB-002); seluruhnya RENAME sehingga data existing tetap terjaga utuh.
        //
        // Panjang nama `TrxPatientEncounterGuarantor` dan `RegPatientEncounterGuarantor` sama-sama 27 karakter,
        // sehingga titik potong 63-karakter Postgres/EF tidak berubah dan tidak membutuhkan langkah penyesuaian nama terpotong.

        private const string Peta = @"
                'TrxPatientEncounterGuarantor', 'RegPatientEncounterGuarantor'";

        private const string PetaBalik = @"
                'RegPatientEncounterGuarantor', 'TrxPatientEncounterGuarantor'";

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
        {
            migrationBuilder.Sql(Skrip(Peta));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
