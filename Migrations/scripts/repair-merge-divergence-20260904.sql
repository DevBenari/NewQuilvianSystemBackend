-- =====================================================================================
-- Perbaikan divergensi migration setelah merge QuilvianIntegrationBackend ke Ikbal
--
-- MASALAHNYA
--   Database dev sudah menjalankan tujuh migration dari jalur Ikbal yang berkasnya tidak
--   ada lagi di kode setelah merge, karena branch integration menggantinya dengan versi
--   lain. Akibatnya sebagian pekerjaan migration yang tertunda SUDAH terpasang di database
--   dengan nama migration yang berbeda, dan `dotnet ef database update` berhenti dengan
--   "relation already exists".
--
--   Skrip ini menerapkan HANYA bagian yang benar-benar belum ada, lalu mencatat migration
--   yang bersangkutan sebagai sudah diterapkan.
--
-- YANG DIPERIKSA SEBELUM SKRIP INI DITULIS
--   `20260830151340_RepairPostCanonicalIntegration` membuat 5 tabel, 18 indeks, 4 kolom,
--   dan mengganti satu check constraint. Yang diperiksa langsung ke database:
--     - kelima tabel        : SUDAH ADA
--     - 17 dari 18 indeks   : SUDAH ADA
--     - keempat kolom       : BELUM ADA
--     - IX_BilTender_KwitansiNumber : BELUM ADA (menunggu kolomnya)
--     - check constraint    : ADA, tetapi versi lama tanpa RoomChargeAmount
--
--   Jadi yang tersisa hanyalah empat kolom, satu indeks, dan pembaruan check constraint.
--
-- SIFAT
--   Dibungkus transaksi. Setiap pernyataan memakai penjaga IF NOT EXISTS atau setara,
--   sehingga aman dijalankan ulang.
--
-- PRASYARAT
--   Cadangan database sudah diambil. Skrip ini mengubah skema.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f repair-merge-divergence-20260904.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ------------------------------------------------- 1. kolom yang belum terpasang
-- Definisi disalin apa adanya dari `RepairPostCanonicalIntegration`.

ALTER TABLE public."BilTender"
    ADD COLUMN IF NOT EXISTS "CashierReferenceNote" character varying(150) NULL;

ALTER TABLE public."BilTender"
    ADD COLUMN IF NOT EXISTS "KwitansiNumber" character varying(50) NULL;

ALTER TABLE public."BilSettlement"
    ADD COLUMN IF NOT EXISTS "Note" character varying(500) NULL;

ALTER TABLE public."BilCalculationVersion"
    ADD COLUMN IF NOT EXISTS "RoomChargeAmount" numeric(18,2) NOT NULL DEFAULT 0;

-- --------------------------------------------------------- 2. indeks yang hilang
-- Unik dan terfilter, persis seperti pada migration-nya: nomor kwitansi boleh kosong,
-- tetapi bila diisi tidak boleh kembar.

CREATE UNIQUE INDEX IF NOT EXISTS "IX_BilTender_KwitansiNumber"
    ON public."BilTender" ("KwitansiNumber")
    WHERE "KwitansiNumber" IS NOT NULL;

-- ------------------------------------------------- 3. check constraint diperbarui
-- Versi lama tidak menyebut RoomChargeAmount karena kolomnya memang belum ada.

ALTER TABLE public."BilCalculationVersion"
    DROP CONSTRAINT IF EXISTS "CK_BilCalculationVersion_Amounts";

ALTER TABLE public."BilCalculationVersion"
    ADD CONSTRAINT "CK_BilCalculationVersion_Amounts" CHECK (
        "GrossAmount" >= 0 AND "AdministrationFeeAmount" >= 0 AND
        "RoomChargeAmount" >= 0 AND "ItemDiscount" >= 0 AND
        "TotalDiscount" >= 0 AND "TaxAmount" >= 0 AND
        "PatientAmount" >= 0 AND "PrimaryAmount" >= 0 AND
        "ExcessAmount" >= 0 AND "UnresolvedCoverageAmount" >= 0);

-- ------------------------------------- 4. catat migration sebagai sudah diterapkan
-- Seluruh efeknya kini terpasang, sebagian oleh migration lama yang berkasnya hilang
-- dan sisanya oleh skrip ini.

INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
SELECT '20260830151340_RepairPostCanonicalIntegration', '9.0.18'
WHERE NOT EXISTS (
    SELECT 1 FROM "__EFMigrationsHistory"
    WHERE "MigrationId" = '20260830151340_RepairPostCanonicalIntegration'
);

COMMIT;

-- --------------------------------------------------------------------- hasilnya

SELECT
    (SELECT count(*) FROM information_schema.columns
     WHERE table_schema = 'public'
       AND ((table_name = 'BilTender' AND column_name IN ('CashierReferenceNote', 'KwitansiNumber'))
         OR (table_name = 'BilSettlement' AND column_name = 'Note')
         OR (table_name = 'BilCalculationVersion' AND column_name = 'RoomChargeAmount'))
    ) AS kolom_terpasang_dari_4,
    (SELECT count(*) FROM pg_indexes
     WHERE schemaname = 'public' AND indexname = 'IX_BilTender_KwitansiNumber'
    ) AS indeks_terpasang_dari_1,
    (SELECT count(*) FROM pg_constraint
     WHERE conname = 'CK_BilCalculationVersion_Amounts'
       AND pg_get_constraintdef(oid) LIKE '%RoomChargeAmount%'
    ) AS constraint_diperbarui_dari_1;
