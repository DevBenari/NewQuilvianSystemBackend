-- Diagnosa kesiapan modul Operasi
--
-- Sifat: HANYA MEMBACA. Tidak ada INSERT, UPDATE, DELETE, atau DDL di berkas ini.
-- Aman dijalankan terhadap database mana pun, termasuk database yang sedang dipakai.
--
-- Tujuan: menjawab "kenapa saya belum bisa membuat satu kasus operasi" tanpa menebak.
-- Setiap bagian memeriksa satu prasyarat yang ditegakkan
-- OperatingRoomCaseService.CreateAsync dan ValidateReferencesAsync.
--
-- Jalankan seluruhnya, lalu kirimkan hasilnya.

\echo '=== 1. Apakah 13 tabel modul Operasi sudah terbentuk? ==='
SELECT
    CASE WHEN COUNT(*) = 13 THEN 'LENGKAP' ELSE 'KURANG' END AS status,
    COUNT(*)                                                  AS tabel_ditemukan,
    13                                                        AS seharusnya
FROM information_schema.tables
WHERE table_schema = 'public'
  AND table_name LIKE 'Opr%';

SELECT table_name
FROM information_schema.tables
WHERE table_schema = 'public' AND table_name LIKE 'Opr%'
ORDER BY table_name;

\echo ''
\echo '=== 2. Akun mana yang bisa membuat kasus operasi? ==='
-- CreateAsync menolak dengan 403 bila klaim doctor_id kosong.
-- Klaim itu diisi dari AspNetUsers."DoctorId" pada AuthController baris 1893.
SELECT
    u."UserName",
    u."Email",
    CASE WHEN u."DoctorId" IS NULL THEN 'TIDAK BISA - belum tertaut dokter'
         WHEN d."Id" IS NULL       THEN 'TIDAK BISA - dokter tidak ditemukan'
         WHEN d."IsDelete"         THEN 'TIDAK BISA - dokter terhapus'
         WHEN NOT d."IsActive"     THEN 'TIDAK BISA - dokter tidak aktif'
         ELSE 'BISA'
    END                            AS kesimpulan,
    d."FullName"                   AS nama_dokter
FROM "AspNetUsers" u
LEFT JOIN "MstDoctor" d ON d."Id" = u."DoctorId"
ORDER BY (u."DoctorId" IS NULL), u."UserName"
LIMIT 30;

\echo ''
\echo '=== 3. Berapa akun yang sudah tertaut dokter aktif? ==='
SELECT COUNT(*) AS akun_siap_pakai
FROM "AspNetUsers" u
JOIN "MstDoctor" d ON d."Id" = u."DoctorId"
WHERE d."IsActive" AND NOT d."IsDelete";

\echo ''
\echo '=== 4. Adakah tindakan yang memenuhi syarat untuk dioperasikan? ==='
-- ValidateReferencesAsync mensyaratkan TrxPatientProcedure dengan
-- IsSurgeryRelated = true, IsActive = true, IsCancel = false, IsDelete = false,
-- dan EncounterId serta PatientId yang cocok satu sama lain.
SELECT
    COUNT(*) FILTER (WHERE NOT p."IsDelete")                                       AS total_tindakan,
    COUNT(*) FILTER (WHERE NOT p."IsDelete" AND p."IsSurgeryRelated")              AS ditandai_operasi,
    COUNT(*) FILTER (WHERE NOT p."IsDelete" AND p."IsSurgeryRelated"
                       AND p."IsActive" AND NOT p."IsCancel")                      AS memenuhi_syarat
FROM "TrxPatientProcedure" p;

\echo ''
\echo '=== 5. Tindakan yang siap dipilih di form, beserta pasien dan encounter-nya ==='
-- Yang sudah terpakai kasus operasi berjalan sengaja dikecualikan,
-- karena ValidateReferencesAsync menolaknya dengan kode OPR002.
SELECT
    p."Id"          AS patient_procedure_id,
    e."PatientId"   AS patient_id,
    e."Id"          AS encounter_id,
    pt."FullName"   AS nama_pasien
FROM "TrxPatientProcedure" p
JOIN "TrxPatientEncounter" e ON e."Id" = p."EncounterId" AND e."PatientId" = p."PatientId"
JOIN "MstPatient" pt         ON pt."Id" = e."PatientId"
WHERE p."IsSurgeryRelated" AND p."IsActive" AND NOT p."IsCancel" AND NOT p."IsDelete"
  AND NOT e."IsDelete"
  AND NOT EXISTS (
      SELECT 1 FROM "OprCaseProcedure" cp
      JOIN "OprCase" c ON c."Id" = cp."OprCaseId"
      WHERE cp."PatientProcedureId" = p."Id" AND NOT cp."IsDelete" AND NOT c."IsDelete"
        AND c."Status" NOT IN (5, 7)  -- Completed = 5, Cancelled = 7
  )
LIMIT 10;

\echo ''
\echo '=== 6. Sudah ada kasus operasi yang pernah dibuat? ==='
SELECT COUNT(*) AS jumlah_kasus FROM "OprCase" WHERE NOT "IsDelete";
