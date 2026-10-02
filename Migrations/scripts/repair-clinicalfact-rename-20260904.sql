-- =====================================================================================
-- Menyelaraskan nama tabel ClinicalMilestoneFact sebelum migration rename dijalankan
--
-- MASALAHNYA
--   Kedua branch mengganti nama tabel yang sama dengan tujuan berbeda:
--     - jalur Ikbal        : TrxClinicalMilestoneFact -> BilClinicalMilestoneFact
--       (migration `20260826101500_RenameClinicalMilestoneFactToBillingOwnership`,
--        berkasnya tidak ada lagi di kode setelah merge)
--     - branch integration : TrxClinicalMilestoneFact -> CliClinicalMilestoneFact
--       (migration `20260901041805_RenameClinicalMilestoneFactToCliPrefix`, masih tertunda)
--
--   Migration yang tertunda mencari tabel bernama `TrxClinicalMilestoneFact`, sedangkan
--   di database namanya sudah `BilClinicalMilestoneFact`. Karena itu ia gagal dengan
--   "relation does not exist".
--
-- YANG DILAKUKAN SKRIP INI
--   Mengembalikan nama tabel beserta indeks dan constraint-nya ke `Trx...`, yaitu keadaan
--   yang diharapkan migration tertunda. Setelah itu `dotnet ef database update` dapat
--   menjalankan rename `Trx -> Cli` secara normal, sehingga riwayat migration tetap jujur:
--   EF benar-benar mengerjakan migration-nya, bukan sekadar ditandai selesai.
--
--   Isi tabel tidak disentuh. Rename tidak memindahkan data.
--
-- SIFAT
--   Dibungkus transaksi dan aman diulang: setiap rename hanya berjalan bila nama lamanya
--   masih ada.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f repair-clinicalfact-rename-20260904.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

DO $$
BEGIN
    IF EXISTS (SELECT 1 FROM information_schema.tables
               WHERE table_schema = 'public' AND table_name = 'BilClinicalMilestoneFact') THEN

        ALTER TABLE public."BilClinicalMilestoneFact"
            RENAME TO "TrxClinicalMilestoneFact";

        ALTER INDEX public."PK_BilClinicalMilestoneFact"
            RENAME TO "PK_TrxClinicalMilestoneFact";

        ALTER TABLE public."TrxClinicalMilestoneFact"
            RENAME CONSTRAINT "FK_BilClinicalMilestoneFact_TrxPatientEncounter_EncounterId"
                           TO "FK_TrxClinicalMilestoneFact_TrxPatientEncounter_EncounterId";

        ALTER INDEX public."IX_BilClinicalMilestoneFact_DispatchStatus"
            RENAME TO "IX_TrxClinicalMilestoneFact_DispatchStatus";

        ALTER INDEX public."IX_BilClinicalMilestoneFact_EncounterId"
            RENAME TO "IX_TrxClinicalMilestoneFact_EncounterId";

        ALTER INDEX public."IX_BilClinicalMilestoneFact_IdempotencyKey"
            RENAME TO "IX_TrxClinicalMilestoneFact_IdempotencyKey";

        -- Dua nama di bawah dipotong PostgreSQL pada 63 karakter; EF menandai pemotongan
        -- itu dengan `~`. Panjang awalannya sama (Bil dan Trx sama-sama tiga huruf),
        -- sehingga titik potongnya tidak berubah.
        ALTER INDEX public."IX_BilClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~"
            RENAME TO "IX_TrxClinicalMilestoneFact_SourceContext_MilestoneFactId_Mile~";

        ALTER INDEX public."IX_BilClinicalMilestoneFact_SourceContext_SourceAggregateId_So~"
            RENAME TO "IX_TrxClinicalMilestoneFact_SourceContext_SourceAggregateId_So~";
    END IF;
END $$;

-- Migration lama yang berkasnya hilang dikeluarkan dari riwayat, karena efeknya baru saja
-- dibatalkan. Membiarkannya akan membuat riwayat mengaku telah melakukan sesuatu yang
-- sudah tidak berlaku lagi.
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260826101500_RenameClinicalMilestoneFactToBillingOwnership';

COMMIT;

SELECT
    (SELECT count(*) FROM information_schema.tables
     WHERE table_schema = 'public' AND table_name = 'TrxClinicalMilestoneFact') AS tabel_trx,
    (SELECT count(*) FROM pg_indexes
     WHERE schemaname = 'public' AND tablename = 'TrxClinicalMilestoneFact') AS indeks;
