-- =====================================================================================
-- Mengembalikan tabel Laboratorium ke nama `TrxLab...` agar urutan migration dapat diputar
--
-- MASALAHNYA
--   Pada branch integration, urutan sejarahnya adalah:
--     1. tabel lahir bernama `TrxLabSpecimen` dan `TrxLabTransitionHistory`
--     2. `20260903071535_AddLabExamination` membuat foreign key ke `TrxLabSpecimen`
--     3. `20260903094528_RenameLaboratoryTrxTablesToLabPrefix` baru menamainya `Lab...`
--
--   Pada database ini kedua tabel sudah bernama `LabSpecimen` dan `LabTransitionHistory`
--   sejak awal, karena jalur Ikbal membuatnya langsung dengan nama akhir. Akibatnya
--   langkah 2 gagal: foreign key-nya mencari tabel yang namanya sudah berubah.
--
-- KENAPA BEGINI, BUKAN DITANDAI SELESAI
--   Percobaan pertama menandai migration rename sebagai selesai, karena keadaan akhirnya
--   memang sudah tercapai. Itu keliru: `AddLabExamination` berjalan LEBIH DULU daripada
--   rename, dan ia tetap mencari nama lama. Menandai rename selesai tidak menolongnya.
--
--   Yang benar adalah mengembalikan database ke keadaan yang diasumsikan urutan itu, lalu
--   membiarkan EF memainkan keduanya sendiri. Dengan begitu riwayat migration jujur: kedua
--   migration benar-benar dijalankan, dan nama constraint hasil akhirnya persis seperti
--   yang dihasilkan branch integration — bukan hasil tebakan skrip ini.
--
-- SIFAT
--   Dibungkus transaksi, aman diulang, dan hanya menamai ulang. Isi tabel tidak disentuh.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f repair-lab-prefix-replay-20260904.sql
--   lalu: dotnet ef database update
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- Batalkan penandaan yang keliru dari percobaan sebelumnya, supaya EF menjalankan
-- rename-nya sendiri.
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260903094528_RenameLaboratoryTrxTablesToLabPrefix';

DO $$
DECLARE
    r record;
BEGIN
    IF NOT EXISTS (SELECT 1 FROM information_schema.tables
                   WHERE table_schema = 'public' AND table_name = 'LabSpecimen') THEN
        RAISE NOTICE 'LabSpecimen tidak ada; tidak ada yang perlu dikembalikan.';
        RETURN;
    END IF;

    -- Constraint lebih dulu, selagi nama tabelnya masih yang lama, supaya pencarian
    -- katalognya sederhana.
    FOR r IN
        SELECT conname, conrelid::regclass::text AS tabel
        FROM pg_constraint
        WHERE conrelid IN ('public."LabSpecimen"'::regclass,
                           'public."LabTransitionHistory"'::regclass)
          AND conname LIKE '%Lab%'
          AND conname NOT LIKE '%TrxLab%'
    LOOP
        EXECUTE format('ALTER TABLE %s RENAME CONSTRAINT %I TO %I',
            r.tabel, r.conname,
            replace(replace(r.conname, 'LabSpecimen', 'TrxLabSpecimen'),
                    'LabTransitionHistory', 'TrxLabTransitionHistory'));
    END LOOP;

    FOR r IN
        SELECT indexname
        FROM pg_indexes
        WHERE schemaname = 'public'
          AND tablename IN ('LabSpecimen', 'LabTransitionHistory')
          AND indexname LIKE 'IX_Lab%'
    LOOP
        EXECUTE format('ALTER INDEX public.%I RENAME TO %I',
            r.indexname,
            replace(replace(r.indexname, 'IX_LabSpecimen', 'IX_TrxLabSpecimen'),
                    'IX_LabTransitionHistory', 'IX_TrxLabTransitionHistory'));
    END LOOP;

    ALTER TABLE public."LabSpecimen" RENAME TO "TrxLabSpecimen";
    ALTER TABLE public."LabTransitionHistory" RENAME TO "TrxLabTransitionHistory";
END $$;

COMMIT;

SELECT table_name FROM information_schema.tables
WHERE table_schema = 'public' AND table_name LIKE '%LabSpecimen%'
   OR table_schema = 'public' AND table_name LIKE '%LabTransitionHistory%'
ORDER BY 1;
