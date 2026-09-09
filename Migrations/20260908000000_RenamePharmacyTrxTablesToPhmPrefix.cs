using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using QuilvianSystemBackend.Repositories;

#nullable disable

namespace QuilvianSystemBackend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260908000000_RenamePharmacyTrxTablesToPhmPrefix")]
    public partial class RenamePharmacyTrxTablesToPhmPrefix : Migration
    {
        // Lima belas entitas operasional Farmasi pindah ke prefix registry `Phm`.
        //
        // Prefix modul ini adalah `Phm` menurut baris registry
        // `HealthServices | PharmacyManagement / Pharmacy`. Kontrak menyatakan normalisasi
        // `Trx*` belum selesai selama class, berkas, configuration, DbSet, rujukan, dan tabel
        // fisik belum dinormalkan bersama (QBE-NAM-003). Migration ini pasangan fisik dari
        // rename di sumber; tanpa ia, kode baru dan basis data lama akan saling meleset.
        //
        // Polanya mengikuti `RenameMedicalRecordTrxTablesToMrcPrefix`: nama constraint dan
        // index dicari dari katalog Postgres, tidak diketik satu per satu. Sebagian nama
        // buatan EF sudah terpotong pada batas 63 karakter, sehingga mengetiknya manual
        // mengundang salah ketik yang baru ketahuan saat migration dijalankan.
        //
        // Tidak ada DROP+CREATE di sini (QBE-DB-002). Seluruhnya RENAME.
        //
        // TIDAK ADA langkah "rapikan nama terpotong" seperti pada preseden Rekam Medis.
        // Preseden itu membutuhkannya karena penggantian keempatnya memendekkan nama 13 huruf,
        // sehingga dua nama yang semula terpotong menjadi muat utuh dan harus disepadankan
        // dengan harapan model EF. Di sini `Trx` dan `Phm` sama-sama tiga huruf: panjang setiap
        // nama tidak berubah, dan titik potong 63-karakter milik EF jatuh persis di tempat yang
        // sama. Diperiksa sebelum migration ini ditulis — 12 objek berada di 61-63 karakter,
        // 9 di antaranya tepat 63, dan seluruhnya tetap 63 setelah penggantian.
        //
        // `MstDrugBatch` TIDAK ikut: ia master/reference, prefix `Mst`-nya sudah benar menurut
        // kontrak (`Mst*` tidak deprecated), dan kepemilikannya sudah berada di Farmasi.
        // Mengikuti perlakuan yang sama atas `MstLabRejectionReason` di modul Laboratorium.
        // Nama FK yang memuatnya karena itu tetap menyebut `MstDrugBatch`.
        //
        // `MstOperatingRoomStockSource` juga tidak: ia milik modul Operasi, dan
        // menormalkannya menjadi `OprStockSource` adalah campaign tersendiri.
        //
        // Urutan pasangan di bawah tidak mengikat. Foreign key Postgres mengikat OID tabel,
        // bukan namanya, sehingga penggantian nama induk tidak pernah memutus FK anaknya.
        // Urutannya disusun anak-ke-induk semata agar dapat dibaca dan dicocokkan manusia.

        private const string Peta = @"
                'TrxDrugUsageAllocation', 'PhmDrugUsageAllocation',
                'TrxDrugUsageItem', 'PhmDrugUsageItem',
                'TrxDrugUsage', 'PhmDrugUsage',
                'TrxDrugReturnHistory', 'PhmDrugReturnHistory',
                'TrxDrugReturnItem', 'PhmDrugReturnItem',
                'TrxDrugReturn', 'PhmDrugReturn',
                'TrxStockTransferAllocation', 'PhmStockTransferAllocation',
                'TrxStockTransferHistory', 'PhmStockTransferHistory',
                'TrxStockTransferItem', 'PhmStockTransferItem',
                'TrxStockTransfer', 'PhmStockTransfer',
                'TrxStockRequestHistory', 'PhmStockRequestHistory',
                'TrxStockRequestItem', 'PhmStockRequestItem',
                'TrxStockRequest', 'PhmStockRequest',
                'TrxDrugStockMutation', 'PhmDrugStockMutation',
                'TrxDrugStockBalance', 'PhmDrugStockBalance'";

        private const string PetaBalik = @"
                'PhmDrugUsageAllocation', 'TrxDrugUsageAllocation',
                'PhmDrugUsageItem', 'TrxDrugUsageItem',
                'PhmDrugUsage', 'TrxDrugUsage',
                'PhmDrugReturnHistory', 'TrxDrugReturnHistory',
                'PhmDrugReturnItem', 'TrxDrugReturnItem',
                'PhmDrugReturn', 'TrxDrugReturn',
                'PhmStockTransferAllocation', 'TrxStockTransferAllocation',
                'PhmStockTransferHistory', 'TrxStockTransferHistory',
                'PhmStockTransferItem', 'TrxStockTransferItem',
                'PhmStockTransfer', 'TrxStockTransfer',
                'PhmStockRequestHistory', 'TrxStockRequestHistory',
                'PhmStockRequestItem', 'TrxStockRequestItem',
                'PhmStockRequest', 'TrxStockRequest',
                'PhmDrugStockMutation', 'TrxDrugStockMutation',
                'PhmDrugStockBalance', 'TrxDrugStockBalance'";

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
