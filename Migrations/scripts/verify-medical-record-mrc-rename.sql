-- Pemeriksaan setelah migration `20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix`.
--
-- Skrip ini HANYA MEMBACA. Tidak ada satu pun perintah yang mengubah data maupun skema,
-- sehingga aman dijalankan kapan saja, termasuk berkali-kali.
--
-- Yang dibuktikan: keempat tabel sudah bernama `Mrc*`, tidak ada sisa `Trx*` untuk keempatnya,
-- dan jumlah barisnya sesuai harapan. Rename di PostgreSQL tidak menyentuh isi tabel, tetapi
-- pembuktiannya tetap perlu ada — kontrak menuntut verifikasi row count dan integritas, bukan
-- keyakinan bahwa perintahnya benar.

\echo '=== 1. Tabel: keempatnya harus ada sebagai Mrc*, dan tidak ada lagi yang Trx* ==='

SELECT c.relname AS tabel,
       CASE
           WHEN c.relname LIKE 'Mrc%' THEN 'BENAR — sudah dinormalkan'
           ELSE 'MASIH TRX — migration belum diterapkan'
       END AS keadaan
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public'
  AND c.relkind = 'r'
  AND c.relname IN (
      'TrxClinicalDocumentIntegrity',    'MrcClinicalDocumentIntegrity',
      'TrxClinicalNoteAddendum',         'MrcClinicalNoteAddendum',
      'TrxClinicalNoteAuthorDelegation', 'MrcClinicalNoteAuthorDelegation',
      'TrxMedicalRecordAccessLog',       'MrcAccessLog'
  )
ORDER BY 1;

\echo '=== 2. Jumlah baris. Bandingkan dengan catatan sebelum migration ==='

SELECT 'MrcClinicalDocumentIntegrity'    AS tabel, COUNT(*) AS baris FROM public."MrcClinicalDocumentIntegrity"
UNION ALL
SELECT 'MrcClinicalNoteAddendum',                  COUNT(*) FROM public."MrcClinicalNoteAddendum"
UNION ALL
SELECT 'MrcClinicalNoteAuthorDelegation',          COUNT(*) FROM public."MrcClinicalNoteAuthorDelegation"
UNION ALL
SELECT 'MrcAccessLog',                             COUNT(*) FROM public."MrcAccessLog"
ORDER BY 1;

\echo '=== 3. Sisa nama constraint dan index. Kolom `sisa_trx` harus kosong seluruhnya ==='

SELECT c.relname AS index_atau_constraint,
       CASE WHEN c.relname LIKE '%Trx%' THEN 'SISA TRX' ELSE '' END AS sisa_trx
FROM pg_class c
JOIN pg_namespace n ON n.oid = c.relnamespace
WHERE n.nspname = 'public'
  AND c.relkind = 'i'
  AND (c.relname LIKE '%ClinicalDocumentIntegrity%'
    OR c.relname LIKE '%ClinicalNoteAddendum%'
    OR c.relname LIKE '%ClinicalNoteAuthorDelegation%'
    OR c.relname LIKE '%AccessLog%')
ORDER BY 1;

\echo '=== 4. Foreign key. Yang menunjuk TrxPatientEncounter memang SENGAJA tetap menyebutnya ==='

SELECT t.relname AS tabel, c.conname AS foreign_key
FROM pg_constraint c
JOIN pg_class t ON t.oid = c.conrelid
JOIN pg_namespace n ON n.oid = t.relnamespace
WHERE n.nspname = 'public'
  AND c.contype = 'f'
  AND t.relname IN (
      'MrcClinicalDocumentIntegrity',
      'MrcClinicalNoteAddendum',
      'MrcClinicalNoteAuthorDelegation',
      'MrcAccessLog'
  )
ORDER BY 1, 2;

\echo '=== 5. Riwayat migration. Baris rename harus tercatat ==='

SELECT "MigrationId"
FROM "__EFMigrationsHistory"
WHERE "MigrationId" = '20260831000000_RenameMedicalRecordTrxTablesToMrcPrefix';
