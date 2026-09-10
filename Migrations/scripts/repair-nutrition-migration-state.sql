-- =====================================================================================
-- Perbaikan keadaan migration modul Gizi
--
-- MASALAH YANG DIPERBAIKI
--   Sebuah migration Gizi sempat diterapkan ke database, lalu berkasnya dihapus karena
--   isinya belum lengkap. Akibatnya tabel Gz* terlanjur ada di database, sementara EF
--   tidak lagi mengenali migration yang membuatnya. Migration baru pun gagal dengan:
--
--     42P07: relation "GzNutritionOrder" already exists
--
-- YANG DILAKUKAN
--   1. Memastikan seluruh tabel Gz* masih KOSONG.
--   2. Bila kosong: menghapus tabel-tabel itu dan catatan migration yatimnya, sehingga
--      migration yang lengkap dapat berjalan dari awal.
--   3. Bila ada yang berisi: BERHENTI tanpa menghapus apa pun, dan menyebutkan tabel mana.
--
-- PENGAMAN
--   Skrip ini MENOLAK berjalan bila ada satu saja baris data pada tabel Gz*. Tabel-tabel
--   ini baru dibuat dan belum dipakai; bila ternyata sudah berisi, berarti dugaan itu
--   salah dan penghapusan bukan tindakan yang benar.
--
--   Tidak ada tabel di luar Gz* yang disentuh.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d QuilvianNewDevIkbal -f repair-nutrition-migration-state.sql
--   lalu:  dotnet ef database update
-- =====================================================================================

BEGIN;

DO $$
DECLARE
    nama_tabel   text;
    jumlah_baris bigint;
    berisi       text[] := ARRAY[]::text[];
BEGIN
    -- 1. Periksa isi setiap tabel Gz* yang ada.
    FOR nama_tabel IN
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name LIKE 'Gz%'
    LOOP
        EXECUTE format('SELECT COUNT(*) FROM public.%I', nama_tabel) INTO jumlah_baris;
        IF jumlah_baris > 0 THEN
            berisi := array_append(berisi, format('%s (%s baris)', nama_tabel, jumlah_baris));
        END IF;
    END LOOP;

    IF array_length(berisi, 1) > 0 THEN
        RAISE EXCEPTION
            'DIBATALKAN. Tabel Gz* berikut sudah berisi data sehingga tidak dihapus: %. Periksa dulu isinya sebelum melanjutkan.',
            array_to_string(berisi, ', ');
    END IF;

    -- 2. Seluruhnya kosong: aman dihapus.
    FOR nama_tabel IN
        SELECT table_name
        FROM information_schema.tables
        WHERE table_schema = 'public' AND table_name LIKE 'Gz%'
    LOOP
        EXECUTE format('DROP TABLE IF EXISTS public.%I CASCADE', nama_tabel);
        RAISE NOTICE 'Tabel dihapus: %', nama_tabel;
    END LOOP;
END $$;

-- 3. Catatan migration Gizi yang yatim dibuang, supaya EF menjalankan migration lengkap
--    dari awal. Hanya baris migration Gizi yang disentuh.
DELETE FROM "__EFMigrationsHistory"
WHERE "MigrationId" LIKE '%AddNutritionManagement';

COMMIT;

-- =====================================================================================
-- Ringkasan
-- =====================================================================================

SELECT 'Sisa tabel Gz*' AS pemeriksaan, COUNT(*)::text AS nilai
FROM information_schema.tables
WHERE table_schema = 'public' AND table_name LIKE 'Gz%'

UNION ALL

SELECT 'Sisa catatan migration Gizi', COUNT(*)::text
FROM "__EFMigrationsHistory"
WHERE "MigrationId" LIKE '%AddNutritionManagement';
