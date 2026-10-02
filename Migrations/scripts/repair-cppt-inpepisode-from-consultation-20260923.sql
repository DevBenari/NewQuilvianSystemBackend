-- =====================================================================================
-- Mengisi InpEpisodeId pada CPPT lama yang lahir dari SOAP konsultasi dokter
--
-- MASALAHNYA
--   ISS-07 / BE-RWI-128. Endpoint
--     POST .../patient-integrated-progress-notes/from-consultation/{consultationId}
--   membentuk entitasnya sendiri dan melewatkan pemetaan InpEpisodeId. Catatan tersimpan
--   dengan HTTP 200 dan bernomor, tetapi kolom perawatannya kosong.
--
--   Lini masa satu perawatan
--     GET .../patient-integrated-progress-notes/episodes/{episodeId}
--   menyaring dengan "InpEpisodeId" = episodeId, sehingga catatan itu tersaring keluar:
--   terbit, bernomor, dan tidak pernah terbaca PPA lain pada perawatan yang sama.
--
--   Kode sudah diperbaiki, tetapi perbaikan kode tidak menyentuh baris yang telanjur
--   tersimpan. Skrip ini yang mengurusnya.
--
-- YANG DILAKUKAN SKRIP INI
--   Dua langkah, keduanya hanya mengisi kolom yang MASIH KOSONG:
--
--     Langkah 1 - dari stempel pada konsultasinya.
--       Konsultasi rawat inap distempel perawatannya sejak BE-RWI-043. Catatan mengambil
--       stempel itu, dengan syarat kunjungan catatan dan kunjungan konsultasi sama.
--
--     Langkah 2 - dari kunjungan, untuk konsultasi lama yang belum sempat distempel.
--       Hanya dijalankan bila kunjungan itu menaungi TEPAT SATU perawatan yang belum
--       dihapus. Kunjungan dengan lebih dari satu perawatan sengaja dilewati: menebak
--       perawatan mana yang benar pada rekam medis bukan pekerjaan skrip.
--
--   Yang TIDAK disentuh:
--     - Catatan yang InpEpisodeId-nya sudah terisi. Tidak ada satu nilai pun ditimpa.
--     - Catatan rawat jalan, IGD, dan medical check-up. Kunjungannya tidak punya
--       perawatan rawat inap, sehingga kedua langkah tidak menemukan apa pun untuk diisi.
--     - Catatan di luar SourceModule 'DoctorConsultation'.
--     - Isi klinis catatan. Tidak ada satu pun kolom SOAP, instruksi, atau evaluasi
--       yang diubah, dan tidak ada DELETE maupun TRUNCATE di seluruh berkas ini.
--
-- SIFAT
--   Dibungkus satu transaksi dan aman diulang. Pada eksekusi kedua tidak ada lagi baris
--   yang cocok - syaratnya "InpEpisodeId" IS NULL - sehingga nol baris berubah.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> \
--     -f Migrations/scripts/repair-cppt-inpepisode-from-consultation-20260923.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- Sebelum: berapa catatan dari konsultasi yang perawatannya masih kosong.
SELECT
    'sebelum' AS tahap,
    COUNT(*)  AS cppt_dari_konsultasi_tanpa_perawatan
FROM public."TrxPatientIntegratedProgressNote" n
WHERE n."InpEpisodeId" IS NULL
  AND n."ConsultationId" IS NOT NULL;

-- -------------------------------------------------------------------------------------
-- Langkah 1. Stempel perawatan diambil dari konsultasi asalnya.
-- -------------------------------------------------------------------------------------
WITH terisi AS (
    UPDATE public."TrxPatientIntegratedProgressNote" n
    SET "InpEpisodeId" = c."InpEpisodeId"
    FROM public."TrxDoctorConsultation" c
    WHERE n."ConsultationId" = c."Id"
      AND n."InpEpisodeId" IS NULL
      AND c."InpEpisodeId" IS NOT NULL
      AND c."EncounterId" = n."EncounterId"
    RETURNING n."Id"
)
SELECT 'langkah-1-dari-konsultasi' AS tahap, COUNT(*) AS baris_terisi FROM terisi;

-- -------------------------------------------------------------------------------------
-- Langkah 2. Konsultasi lama tanpa stempel: perawatan diturunkan dari kunjungannya,
-- hanya bila kunjungan itu menaungi tepat satu perawatan.
-- -------------------------------------------------------------------------------------
WITH kunjungan_berperawatan_tunggal AS (
    SELECT e."EncounterId"
    FROM public."InpEpisode" e
    WHERE e."IsDelete" = false
    GROUP BY e."EncounterId"
    HAVING COUNT(*) = 1
),
perawatan_tunggal AS (
    SELECT
        e."EncounterId",
        e."Id" AS episode_id
    FROM public."InpEpisode" e
    JOIN kunjungan_berperawatan_tunggal k ON k."EncounterId" = e."EncounterId"
    WHERE e."IsDelete" = false
),
terisi AS (
    UPDATE public."TrxPatientIntegratedProgressNote" n
    SET "InpEpisodeId" = p.episode_id
    FROM perawatan_tunggal p
    WHERE n."EncounterId" = p."EncounterId"
      AND n."InpEpisodeId" IS NULL
      AND n."ConsultationId" IS NOT NULL
      AND n."SourceModule" = 'DoctorConsultation'
    RETURNING n."Id"
)
SELECT 'langkah-2-dari-kunjungan' AS tahap, COUNT(*) AS baris_terisi FROM terisi;

-- Sesudah: yang tersisa kosong beserta alasannya, supaya sisanya tidak lolos tanpa kabar.
SELECT
    'sesudah' AS tahap,
    COUNT(*) FILTER (
        WHERE n."InpEpisodeId" IS NULL
    ) AS masih_kosong,
    COUNT(*) FILTER (
        WHERE n."InpEpisodeId" IS NULL
          AND NOT EXISTS (
              SELECT 1 FROM public."InpEpisode" e
              WHERE e."EncounterId" = n."EncounterId" AND e."IsDelete" = false
          )
    ) AS kosong_karena_bukan_rawat_inap,
    COUNT(*) FILTER (
        WHERE n."InpEpisodeId" IS NULL
          AND (
              SELECT COUNT(*) FROM public."InpEpisode" e
              WHERE e."EncounterId" = n."EncounterId" AND e."IsDelete" = false
          ) > 1
    ) AS kosong_karena_perawatan_lebih_dari_satu
FROM public."TrxPatientIntegratedProgressNote" n
WHERE n."ConsultationId" IS NOT NULL;

COMMIT;

-- =====================================================================================
-- PEMERIKSAAN SETELAH SKRIP DIJALANKAN
--
--   Kolom "masih_kosong" pada baris 'sesudah' harus habis dijelaskan oleh dua kolom di
--   sebelah kanannya. Bila ada selisih, ada catatan yang kunjungannya menaungi tepat satu
--   perawatan tetapi tetap tidak terisi - itu keadaan yang tidak diduga dan perlu dilihat
--   satu per satu sebelum diisi dengan cara lain.
--
--   Lini masa dapat diperiksa langsung, ganti <episodeId> dengan perawatan yang diuji:
--
--     SELECT "ProgressNoteNumber", "NoteDateTime", "ProfessionType"
--     FROM public."TrxPatientIntegratedProgressNote"
--     WHERE "InpEpisodeId" = '<episodeId>'
--       AND "IsDelete" = false
--       AND "IsCancel" = false
--     ORDER BY "NoteDateTime";
-- =====================================================================================
