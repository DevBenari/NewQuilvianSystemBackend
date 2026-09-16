using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    public partial class RenameDrugBatchAndOperatingRoomStockSource : Migration
    {
        // Dua entity dinormalkan ke prefix modul pemiliknya:
        //
        //   MstDrugBatch                -> PhmDrugBatch    (PharmacyManagement, prefix `Phm`)
        //   MstOperatingRoomStockSource -> OprStockSource  (OperatingRoomManagement, `Opr`)
        //
        // Keduanya berada di dalam folder modul, bukan di `Areas/HealthServices/MasterData`,
        // sehingga `Resolve-RegistryOwnership` menuntut namanya memakai prefix baris registry
        // yang mencocoki foldernya. Selama masih berawalan `Mst`, QBE-MOD-002 menolak keduanya
        // sebagai entity yang prefiksnya tidak berwenang.
        //
        // Polanya sama dengan RenamePharmacyTrxTablesToPhmPrefix dan preseden Rekam Medis:
        // nama constraint dan index dicari dari katalog Postgres, tidak diketik satu per satu.
        // Seluruhnya RENAME; tidak ada DROP+CREATE (QBE-DB-002), sehingga isi kedua tabel
        // tetap utuh.
        //
        // Berbeda dari campaign `Trx*` -> `Phm*`, di sini langkah 4 DIPERLUKAN.
        // `MstDrugBatch` -> `PhmDrugBatch` memang sama panjang, tetapi
        // `MstOperatingRoomStockSource` -> `OprStockSource` memendek 13 huruf. Satu nama yang
        // semula dipotong Npgsql pada batas 63 karakter — ditandai akhiran `~` — kini muat
        // utuh, sehingga harus disepadankan dengan nama yang diharapkan model EF.

        private const string Peta = @"
                'MstOperatingRoomStockSource', 'OprStockSource',
                'MstDrugBatch', 'PhmDrugBatch'";

        private const string PetaBalik = @"
                'OprStockSource', 'MstOperatingRoomStockSource',
                'PhmDrugBatch', 'MstDrugBatch'";

        // Langkah 4 arah maju: nama yang semula terpotong `~` kini muat utuh.
        private const string RapikanMaju = @"
                'FK_OprStockSource_MstDrugStorageLocation_StorageL~',
                'FK_OprStockSource_MstDrugStorageLocation_StorageLocationId'";

        // Arah balik: dipendekkan lagi supaya penggantian nama tabel dapat mengenalinya.
        private const string RapikanBalik = @"
                'FK_OprStockSource_MstDrugStorageLocation_StorageLocationId',
                'FK_OprStockSource_MstDrugStorageLocation_StorageL~'";

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

        /// <summary>
        /// Langkah 4: memulihkan nama yang semula terpotong batas 63 karakter.
        ///
        /// Dijalankan setelah penggantian nama tabel, karena barulah pada saat itu namanya
        /// berbentuk `FK_OprStockSource_..._StorageL~` dengan awalan yang sudah baru.
        /// </summary>
        private static string SkripRapikan(string peta) => $$"""
            DO $qbe$
            DECLARE
                peta CONSTANT text[] := ARRAY[{{peta}}
                ];
                lama text;
                baru text;
                i int;
                r record;
            BEGIN
                FOR i IN 1 .. array_length(peta, 1) / 2 LOOP
                    lama := peta[i * 2 - 1];
                    baru := peta[i * 2];

                    FOR r IN
                        SELECT t.relname AS tabel
                        FROM pg_constraint c
                        JOIN pg_class t ON t.oid = c.conrelid
                        JOIN pg_namespace n ON n.oid = t.relnamespace
                        WHERE n.nspname = 'public' AND c.conname = lama
                    LOOP
                        EXECUTE format(
                            'ALTER TABLE public.%I RENAME CONSTRAINT %I TO %I',
                            r.tabel, lama, baru);
                    END LOOP;

                    IF EXISTS (
                        SELECT 1 FROM pg_class c
                        JOIN pg_namespace n ON n.oid = c.relnamespace
                        WHERE n.nspname = 'public' AND c.relkind = 'i' AND c.relname = lama
                    ) THEN
                        EXECUTE format('ALTER INDEX public.%I RENAME TO %I', lama, baru);
                    END IF;
                END LOOP;
            END
            $qbe$;
            """;

        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(Skrip(Peta));
            migrationBuilder.Sql(SkripRapikan(RapikanMaju));
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Urutannya dibalik: nama dipendekkan lebih dulu supaya penggantian nama tabel
            // pada langkah berikutnya dapat mengenali polanya.
            migrationBuilder.Sql(SkripRapikan(RapikanBalik));
            migrationBuilder.Sql(Skrip(PetaBalik));
        }
    }
}
