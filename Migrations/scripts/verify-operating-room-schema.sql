-- Verifikasi skema modul Operasi setelah migration diterapkan.
-- Seluruh perintah di berkas ini hanya membaca; tidak ada yang mengubah data.
--
-- Cara menjalankan:
--   psql -h <host> -p <port> -U <user> -d <database> -f verify-operating-room-schema.sql
--
-- Tiga pemeriksaan di bawah menutup celah bukti yang ditinggalkan pengujian otomatis.
-- Pengujian tersebut memakai database dalam memori, dan provider itu tidak menegakkan
-- index bersyarat, tipe jsonb, maupun perilaku concurrency token.

\echo '=== 1. Migration tercatat? (harus 1 baris) ==='
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260821060256_AddOperatingRoomFoundation';

\echo ''
\echo '=== 2. Jumlah tabel modul Operasi (harus 13) ==='
SELECT count(*) AS jumlah_tabel_opr
FROM information_schema.tables
WHERE table_schema = 'public' AND table_name LIKE 'Opr%';

\echo ''
\echo '=== 3. Filtered unique index pada OprSchedule (harus ada, memuat IsCurrent) ==='
-- Index ini yang mencegah satu kasus operasi punya dua jadwal aktif sekaligus.
-- Bila baris di bawah kosong, aturan tersebut TIDAK ditegakkan database.
SELECT indexname, indexdef
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename = 'OprSchedule'
  AND indexdef ILIKE '%IsCurrent%';

\echo ''
\echo '=== 4. Kolom jsonb (kedua baris harus bertipe jsonb, bukan text) ==='
SELECT table_name, column_name, data_type
FROM information_schema.columns
WHERE table_schema = 'public'
  AND (
    (table_name = 'OprSafetyChecklist' AND column_name = 'ItemsJson') OR
    (table_name = 'OprRecovery'        AND column_name = 'ObservationJson')
  )
ORDER BY table_name;

\echo ''
\echo '=== 5. Unique index penting lainnya ==='
SELECT tablename, indexname
FROM pg_indexes
WHERE schemaname = 'public'
  AND tablename IN ('OprCase', 'OprTeamMember', 'OprSafetyChecklist',
                    'OprExecutionRecord', 'OprAnesthesiaRecord',
                    'OprRecovery', 'OprHandover', 'OprIntegrationDelivery')
  AND indexdef ILIKE '%UNIQUE%'
ORDER BY tablename, indexname;

\echo ''
\echo '=== Selesai. Yang belum bisa diperiksa lewat SQL ==='
\echo 'Concurrency token "Version" adalah perilaku runtime, bukan bentuk skema.'
\echo 'Mengujinya perlu dua sesi mengubah kasus yang sama; sesi kedua harus dijawab OPR012.'
