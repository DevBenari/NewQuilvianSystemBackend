using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class RenameTrxPatientEncounterAndPrescriptionToCanonicalPrefix : Migration
    {
        // Entity transaksi dinormalkan ke prefix modul pemiliknya:
        //
        //   TrxPatientEncounter -> RegPatientEncounter (RegistrationManagement, prefix `Reg`)
        //   TrxPrescription     -> PhmPrescription     (PharmacyManagement, prefix `Phm`)
        //
        // Panjang nama `TrxPatientEncounter` (19) dan `RegPatientEncounter` (19) sama.
        // Panjang nama `TrxPrescription` (15) dan `PhmPrescription` (15) sama.
        // Tidak ada perubahan panjang string untuk batas identifier 63 karakter Postgres.

        private const string Peta = @"
                'TrxPatientEncounter', 'RegPatientEncounter',
                'TrxPrescription', 'PhmPrescription'";

        private const string PetaBalik = @"
                'RegPatientEncounter', 'TrxPatientEncounter',
                'PhmPrescription', 'TrxPrescription'";

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
            migrationBuilder.RenameTable(
                name: "TrxPatientEncounter",
                schema: "public",
                newName: "RegPatientEncounter");

            migrationBuilder.RenameTable(
                name: "TrxPrescription",
                schema: "public",
                newName: "PhmPrescription");

            migrationBuilder.Sql(Skrip(Peta));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "RegPatientEncounter",
                schema: "public",
                newName: "TrxPatientEncounter");

            migrationBuilder.RenameTable(
                name: "PhmPrescription",
                schema: "public",
                newName: "TrxPrescription");

            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
