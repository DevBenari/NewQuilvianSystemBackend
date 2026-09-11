-- =====================================================================================
-- BE-SEC-003B — Perluasan SysAccessPolicy ke exact historical capability set
--
-- STATUS BERKAS INI: RANCANGAN. BELUM DIJALANKAN. BELUM DIVALIDASI TERHADAP DATABASE.
--
--   Bagian 1-3 HANYA MEMBACA (dry run). Aman dijalankan kapan saja.
--   Bagian 4 MENULIS dan sengaja dibungkus blok yang TIDAK akan jalan sampai
--   seseorang mengubahnya secara sadar. Jangan mengubahnya sebelum dry run ditinjau.
--   Bagian 6 adalah rollback.
--
-- PRINSIP YANG MENGIKAT SELURUH BERKAS INI
--
--   1. TIDAK ADA nama Departemen atau Jabatan yang ditulis di sini.
--      TIDAK ADA GUID yang ditulis di sini.
--      Seluruh kepemilikan diturunkan dari baris SysAccessPolicy yang SUDAH ADA.
--      Konsekuensinya: skrip ini benar tanpa perlu tahu isi database sebelumnya,
--      dan tidak dapat mengarang penerima baru.
--
--   2. BEFORE accessible endpoint set = AFTER accessible endpoint set.
--      Setiap identitas baru hanya diberikan kepada pasangan Departemen x Posisi yang
--      hari ini memang memegang identitas lama asalnya. Tidak ada penambahan.
--
--   3. Idempoten. Dijalankan dua kali menghasilkan keadaan yang sama.
--
--   4. Tidak ada baris yang dihapus. Penonaktifan memakai IsActive/IsDelete.
--
-- PRASYARAT MUTLAK — DIKONFIRMASI PENGUKURAN 11 SEPTEMBER 2026
--
--   Pada database development saat ini, SELURUH identitas hasil pemecahan berstatus
--   TIDAK_TERDAFTAR, dan PatientProcedure.Update serta DoctorQueue.Update masih
--   IsActive=true / IsDelete=false. Artinya AccessMenuSeeder BELUM pernah berjalan
--   terhadap database ini pada HEAD.
--
--   Konsekuensinya untuk skrip ini: bagian 4.1 dan 4.2 menyambung ke baris registry
--   identitas BARU. Selama baris itu belum ada, sambungannya kosong dan skrip ini
--   menyisipkan NOL baris. Itu bukan kegagalan senyap — bagian 1.1 memang dirancang
--   menangkapnya — tetapi berarti skrip ini TIDAK DAPAT dijalankan lebih dulu.
--
--   Urutan yang benar dan tidak boleh dibalik:
--
--     1. Aplikasi HEAD start SEKALI dalam jendela pemeliharaan, tanpa traffic pengguna.
--        AccessMenuSeeder membuat baris registry identitas baru dan menutup yang lama.
--     2. Jalankan bagian 1 skrip ini. Seluruh baris wajib berstatus 'ok'.
--     3. Baru Tahap 1, lalu verifikasi, lalu Tahap 2.
--
--   Antara langkah 1 dan selesainya Tahap 1, identitas baru sudah ditegakkan endpoint
--   tetapi belum diberikan kepada siapa pun. Pada jendela itu dokter akan ditolak 403.
--   KARENA ITU langkah 1 sampai 3 wajib berada di dalam satu jendela pemeliharaan yang
--   sama, tertutup dari traffic pengguna.
--
-- CATATAN PENTING TENTANG IDENTITAS YANG SUDAH PENSIUN
--
--   AccessMenuSeeder sudah menutup SysActionAccess milik PatientProcedure.Update dan
--   DoctorQueue.Update (IsActive=false, IsDelete=true). Baris SysAccessPolicy yang
--   menunjuk keduanya MASIH ADA dan flag-nya sendiri masih hidup — yang mati adalah
--   baris registry-nya.
--
--   Karena itu "siapa dulu pemegangnya" WAJIB dibaca dari flag POLICY, bukan dari flag
--   ACTION. Kalau penyaringnya ikut menuntut action aktif, himpunan sumbernya kosong
--   dan seluruh migrasi ini menghasilkan nol baris — gagal senyap.
-- =====================================================================================

\set ON_ERROR_STOP on

-- =====================================================================================
-- BAGIAN 0 — Peta pemecahan (identitas lama -> identitas baru)
--
-- Ini satu-satunya bagian yang ditulis tangan, dan isinya HANYA nama identitas yang
-- berasal dari source code ([AccessPermission] pada controller). Bukan data.
-- =====================================================================================

DROP VIEW IF EXISTS be_sec_003b_peta;
CREATE TEMP VIEW be_sec_003b_peta (resource, action_lama, action_baru) AS
VALUES
    -- PatientProcedure.Update  -> 5 identitas   (identitas lama PENSIUN)
    ('PatientProcedure',   'Update', 'Edit'),
    ('PatientProcedure',   'Update', 'Approve'),
    ('PatientProcedure',   'Update', 'Execute'),
    ('PatientProcedure',   'Update', 'RemoveDraft'),
    ('PatientProcedure',   'Update', 'Cancel'),

    -- PatientProcedure.Create  -> +1 identitas  (identitas lama BERTAHAN, menyempit ke POST /)
    ('PatientProcedure',   'Create', 'Select'),

    -- DoctorQueue.Update       -> 6 identitas   (identitas lama PENSIUN)
    ('DoctorQueue',        'Update', 'Call'),
    ('DoctorQueue',        'Update', 'StartConsultation'),
    ('DoctorQueue',        'Update', 'FinishConsultation'),
    ('DoctorQueue',        'Update', 'Skip'),
    ('DoctorQueue',        'Update', 'NoShow'),
    ('DoctorQueue',        'Update', 'Requeue'),

    -- DoctorConsultation.Update -> +3 identitas (identitas lama BERTAHAN, menjaga PUT /{id})
    ('DoctorConsultation', 'Update', 'WriteSoap'),
    ('DoctorConsultation', 'Update', 'Complete'),
    ('DoctorConsultation', 'Update', 'Cancel'),

    -- PatientAssessment.Update  -> +2 identitas (identitas lama BERTAHAN)
    ('PatientAssessment',  'Update', 'Complete'),
    ('PatientAssessment',  'Update', 'Cancel'),

    -- PatientDiagnosis.Update   -> +3 identitas (identitas lama BERTAHAN)
    ('PatientDiagnosis',   'Update', 'SetPrimary'),
    ('PatientDiagnosis',   'Update', 'Resolve'),
    ('PatientDiagnosis',   'Update', 'Cancel'),

    -- PatientVitalSign.Update   -> +3 identitas (identitas lama BERTAHAN)
    ('PatientVitalSign',   'Update', 'Verify'),
    ('PatientVitalSign',   'Update', 'NotifyDoctor'),
    ('PatientVitalSign',   'Update', 'Cancel');

-- PatientAssessment.Amend SENGAJA TIDAK ada di peta ini. Ia bukan pelestarian dari
-- Update; sumbernya adalah Complete, dan Complete baru terisi oleh migrasi ini juga.
-- Karena itu Amend dikerjakan terpisah pada bagian 2, SESUDAH Complete.

-- =====================================================================================
-- Resolusi identitas -> baris registry.
--
-- Sengaja TIDAK menuntut action aktif: identitas yang sudah pensiun pun harus ketemu,
-- justru merekalah yang policy-nya perlu dilestarikan.
-- =====================================================================================

DROP VIEW IF EXISTS be_sec_003b_identitas;
CREATE TEMP VIEW be_sec_003b_identitas AS
SELECT
    c."ControllerName"  AS resource,
    a."ActionName"      AS action,
    c."Id"              AS controller_access_id,
    a."Id"              AS action_access_id,
    a."IsActive"        AS action_is_active,
    a."IsDelete"        AS action_is_delete
FROM public."SysActionAccess" a
JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId";

-- Pemegang efektif sebuah identitas, dibaca dari FLAG POLICY saja.
DROP VIEW IF EXISTS be_sec_003b_pemegang;
CREATE TEMP VIEW be_sec_003b_pemegang AS
SELECT
    i.resource,
    i.action,
    i.controller_access_id,
    i.action_access_id,
    p."Id"            AS policy_id,
    p."DepartmentId"  AS department_id,
    p."PositionId"    AS position_id
FROM be_sec_003b_identitas i
JOIN public."SysAccessPolicy" p
      ON p."ActionAccessId"     = i.action_access_id
     AND p."ControllerAccessId" = i.controller_access_id
WHERE p."IsAllowed"
  AND p."IsActive"
  AND NOT p."IsDelete";

-- Baris yang SEHARUSNYA ada sesudah migrasi, untuk seluruh peta bagian 0.
DROP VIEW IF EXISTS be_sec_003b_target;
CREATE TEMP VIEW be_sec_003b_target AS
SELECT DISTINCT
    m.resource,
    m.action_lama,
    m.action_baru,
    baru.controller_access_id,
    baru.action_access_id,
    lama.department_id,
    lama.position_id
FROM be_sec_003b_peta m
JOIN be_sec_003b_pemegang lama
      ON lama.resource = m.resource
     AND lama.action   = m.action_lama
JOIN be_sec_003b_identitas baru
      ON baru.resource = m.resource
     AND baru.action   = m.action_baru;

-- =====================================================================================
-- BAGIAN 1 — DRY RUN. Pemeriksaan prasyarat. HANYA MEMBACA.
-- =====================================================================================

\echo ''
\echo '==== 1.1 Apakah setiap identitas pada peta benar-benar terdaftar? ===='
\echo '     Baris mana pun dengan status TIDAK_TERDAFTAR menghentikan migrasi:'
\echo '     berarti aplikasi belum pernah start di HEAD ini, atau namanya salah ketik.'

SELECT DISTINCT
    m.resource,
    m.action_lama,
    m.action_baru,
    CASE WHEN lama.action_access_id IS NULL THEN 'TIDAK_TERDAFTAR' ELSE 'ok' END AS status_lama,
    CASE WHEN baru.action_access_id IS NULL THEN 'TIDAK_TERDAFTAR' ELSE 'ok' END AS status_baru,
    lama.action_is_delete AS lama_sudah_pensiun
FROM be_sec_003b_peta m
LEFT JOIN be_sec_003b_identitas lama ON lama.resource = m.resource AND lama.action = m.action_lama
LEFT JOIN be_sec_003b_identitas baru ON baru.resource = m.resource AND baru.action = m.action_baru
ORDER BY m.resource, m.action_lama, m.action_baru;

\echo ''
\echo '==== 1.2 Pemegang identitas lama — inilah seluruh sumber kepemilikan ===='
\echo '     Kalau bagian ini kosong untuk sebuah identitas lama, tidak ada yang dilestarikan.'

SELECT
    h.resource,
    h.action,
    d."DepartmentName" AS department_name,
    pos."PositionName" AS position_name,
    h.department_id,
    h.position_id,
    (
        SELECT count(DISTINCT uo."UserId")
        FROM public."AspNetUserOrganization" uo
        WHERE uo."DepartmentId" = h.department_id
          AND uo."PositionId"   = h.position_id
          AND uo."IsActive" AND NOT uo."IsDelete" AND NOT uo."IsCancel"
          AND (uo."EffectiveStartDate" IS NULL OR uo."EffectiveStartDate" <= now())
          AND (uo."EffectiveEndDate"   IS NULL OR uo."EffectiveEndDate"   >= now())
    ) AS pengguna_aktif
FROM be_sec_003b_pemegang h
LEFT JOIN public."MstDepartment" d   ON d."Id"   = h.department_id
LEFT JOIN public."MstPosition"   pos ON pos."Id" = h.position_id
WHERE (h.resource, h.action) IN (
        ('PatientProcedure','Update'), ('PatientProcedure','Create'),
        ('DoctorQueue','Update'), ('DoctorConsultation','Update'),
        ('PatientAssessment','Update'), ('PatientDiagnosis','Update'),
        ('PatientVitalSign','Update'))
ORDER BY h.resource, h.action, d."DepartmentName", pos."PositionName";

\echo ''
\echo '==== 1.3 Baris yang AKAN DIBUAT (belum ada) — inti dry run ===='

SELECT
    t.resource,
    t.action_lama || ' -> ' || t.action_baru AS pemecahan,
    d."DepartmentName" AS department_name,
    pos."PositionName" AS position_name,
    t.department_id,
    t.position_id,
    t.controller_access_id,
    t.action_access_id
FROM be_sec_003b_target t
LEFT JOIN public."MstDepartment" d   ON d."Id"   = t.department_id
LEFT JOIN public."MstPosition"   pos ON pos."Id" = t.position_id
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id
)
ORDER BY t.resource, t.action_baru, d."DepartmentName", pos."PositionName";

\echo ''
\echo '==== 1.4 Ringkasan jumlah per identitas baru ===='

SELECT
    t.resource,
    t.action_baru,
    count(*) AS baris_akan_dibuat
FROM be_sec_003b_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id
)
GROUP BY t.resource, t.action_baru
ORDER BY t.resource, t.action_baru;

\echo ''
\echo '==== 1.5 Baris policy yang menggantung pada identitas yang sudah pensiun ===='
\echo '     Kandidat untuk dinonaktifkan pada bagian 4.3 — HANYA sesudah penggantinya dibuat.'

SELECT
    h.resource, h.action, h.policy_id, h.department_id, h.position_id
FROM be_sec_003b_pemegang h
JOIN be_sec_003b_identitas i ON i.resource = h.resource AND i.action = h.action
WHERE i.action_is_delete
ORDER BY h.resource, h.action;

-- =====================================================================================
-- BAGIAN 2 — DRY RUN PatientAssessment.Amend
--
-- Keputusan pemilik sistem: Amend diberikan kepada pemegang efektif
-- PatientAssessment.Complete. Amend adalah FEATURE GATE; penjaga keduanya tetap aturan
-- kepemilikan dokumen (RM-DEC-004) yang tidak dapat dilewati.
--
-- URUTAN ITU PENTING. PatientAssessment.Complete adalah identitas BARU yang baru terisi
-- oleh bagian 4.1 migrasi ini. Bila Amend dihitung sebelum Complete terisi, hasilnya
-- NOL BARIS. Karena itu bagian ini dijalankan SESUDAH bagian 4.1, dan query di bawah
-- memperlihatkan kedua kemungkinan supaya selisihnya terlihat sebelum menulis.
-- =====================================================================================

\echo ''
\echo '==== 2.1 Pemegang PatientAssessment.Complete SEKARANG (sebelum migrasi) ===='
\echo '     Bila kosong, keputusan owner dibaca harfiah menghasilkan NOL baris Amend.'

SELECT count(*) AS pemegang_complete_sekarang
FROM be_sec_003b_pemegang
WHERE resource = 'PatientAssessment' AND action = 'Complete';

\echo ''
\echo '==== 2.2 Pemegang PatientAssessment.Complete SESUDAH migrasi (= pemegang Update) ===='

SELECT
    d."DepartmentName" AS department_name,
    pos."PositionName" AS position_name,
    h.department_id,
    h.position_id
FROM be_sec_003b_pemegang h
LEFT JOIN public."MstDepartment" d   ON d."Id"   = h.department_id
LEFT JOIN public."MstPosition"   pos ON pos."Id" = h.position_id
WHERE h.resource = 'PatientAssessment' AND h.action = 'Update'
ORDER BY d."DepartmentName", pos."PositionName";

-- =====================================================================================
-- BAGIAN 3 — Idempotensi
--
-- Rancangannya satu kalimat: setiap penulisan dijaga NOT EXISTS atas kunci alami
-- (DepartmentId, PositionId, ControllerAccessId, ActionAccessId).
--
-- Jalankan query ini SESUDAH menulis. Hasilnya wajib NOL baris; artinya menjalankan
-- ulang bagian 4 tidak akan membuat satu pun baris tambahan.
-- =====================================================================================

\echo ''
\echo '==== 3.1 Sisa pekerjaan (wajib 0 sesudah migrasi) ===='

SELECT count(*) AS sisa_baris_belum_dibuat
FROM be_sec_003b_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id
);

\echo ''
\echo '==== 3.2 Duplikat kunci alami (wajib 0, sebelum maupun sesudah) ===='

SELECT "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId", count(*) AS jumlah
FROM public."SysAccessPolicy"
GROUP BY 1,2,3,4
HAVING count(*) > 1;

-- =====================================================================================
-- BAGIAN 4 — PENULISAN.
--
-- TIDAK AKAN BERJALAN apa adanya. Untuk menjalankannya, seseorang harus:
--   (a) meninjau seluruh keluaran bagian 1 dan 2 lebih dulu;
--   (b) mengganti :'AKTOR' dengan Guid pengguna yang bertanggung jawab;
--   (c) menghapus baris \echo + ROLLBACK di akhir dan menggantinya dengan COMMIT.
--
-- Seluruh blok berjalan dalam SATU transaksi. Gagal di tengah = tidak ada yang berubah.
-- =====================================================================================

-- \set AKTOR '00000000-0000-0000-0000-000000000000'   -- WAJIB diisi Guid aktor sebenarnya

-- URUTAN WAJIB, ditetapkan pemilik sistem dan TIDAK BOLEH DIBALIK:
--
--   TAHAP 1 : buat baris pengganti          (4.1 + 4.2)   -> COMMIT
--   JEDA    : verifikasi parity (bagian 5), ditinjau manusia
--   TAHAP 2 : baru nonaktifkan policy lama  (4.3)         -> COMMIT
--
-- Kedua tahap sengaja DIPISAH menjadi dua transaksi, bukan satu. Alasannya: bila parity
-- ternyata tidak terbukti, hak lama masih aktif dan tidak ada seorang pun yang kehilangan
-- akses. Menyatukan keduanya dalam satu transaksi membuat penonaktifan ikut ter-commit
-- sebelum siapa pun sempat memeriksa hasilnya — persis urutan yang dilarang.

/*  ---------- TAHAP 1 — LEPAS KOMENTAR HANYA SETELAH DRY RUN DITINJAU ----------

BEGIN;

-- 4.1 Pelestarian: seluruh identitas baru pada peta bagian 0.
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy")
SELECT
    gen_random_uuid(), t.department_id, t.position_id,
    t.controller_access_id, t.action_access_id,
    true, true, false, false, now(), :'AKTOR'::uuid
FROM be_sec_003b_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id);

-- 4.2 PatientAssessment.Amend — SESUDAH 4.1, sehingga Complete sudah terisi.
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy")
SELECT
    gen_random_uuid(), h.department_id, h.position_id,
    amend.controller_access_id, amend.action_access_id,
    true, true, false, false, now(), :'AKTOR'::uuid
FROM be_sec_003b_pemegang h
JOIN be_sec_003b_identitas amend
      ON amend.resource = 'PatientAssessment' AND amend.action = 'Amend'
WHERE h.resource = 'PatientAssessment' AND h.action = 'Complete'
  AND NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = h.department_id
      AND p."PositionId"         = h.position_id
      AND p."ControllerAccessId" = amend.controller_access_id
      AND p."ActionAccessId"     = amend.action_access_id);

-- Ganti baris berikut dengan COMMIT; hanya bila keluaran bagian 1.3 dan 1.4 sudah benar.
ROLLBACK;

    ---------- BATAS TAHAP 1 ---------- */

-- =====================================================================================
-- JEDA WAJIB.
--
-- Jalankan bagian 5 (verifikasi parity) SEKARANG, lalu tinjau hasilnya bersama pemilik
-- sistem. Jangan lanjut ke Tahap 2 sebelum terbukti:
--
--   - jumlah pasangan Departemen x Posisi TIDAK berubah;
--   - tidak ada pasangan yang kehilangan endpoint;
--   - tidak ada pasangan yang memperoleh endpoint di luar peta bagian 0;
--   - bagian 3.1 melaporkan 0 sisa baris.
--
-- Sampai Tahap 2 dijalankan, policy lama MASIH AKTIF. Artinya untuk sementara sebagian
-- pengguna memegang identitas lama DAN identitas barunya sekaligus. Itu disengaja dan
-- aman: tumpang tindih sementara tidak memperluas endpoint yang dapat dijangkau, karena
-- identitas lama sudah tidak dijaga endpoint mana pun (registry-nya ditutup seeder).
-- =====================================================================================

/*  ---------- TAHAP 2 — HANYA SETELAH PARITY TERBUKTI ----------

BEGIN;

-- 4.3 Nonaktifkan policy yang menggantung pada identitas yang sudah pensiun.
--     Dijalankan TERAKHIR dan sebagai transaksi TERSENDIRI.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = false,
    "UpdateDateTime" = now(),
    "UpdateBy" = :'AKTOR'::uuid
FROM be_sec_003b_identitas i
WHERE p."ActionAccessId" = i.action_access_id
  AND i.action_is_delete
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive";

-- Ganti baris berikut dengan COMMIT; hanya bila parity di atas sudah terbukti.
ROLLBACK;

    ---------- BATAS TAHAP 2 ---------- */

-- =====================================================================================
-- BAGIAN 5 — Verifikasi sesudah penulisan
-- =====================================================================================

\echo ''
\echo '==== 5.1 Metrik sesudah ===='

SELECT
    (SELECT count(*) FROM public."SysAccessPolicy")                                AS policy_fisik,
    (SELECT count(*) FROM public."SysAccessPolicy"
      WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete")                         AS policy_efektif,
    (SELECT count(*) FROM public."SysActionAccess"
      WHERE "IsActive" AND NOT "IsDelete")                                         AS action_aktif,
    (SELECT count(*) FROM (
        SELECT DISTINCT "DepartmentId", "PositionId" FROM public."SysAccessPolicy"
        WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete") x)                    AS pasangan_dept_posisi;

\echo ''
\echo '==== 5.2 Parity — tidak boleh ada pasangan yang kehilangan atau memperoleh ===='
\echo '     Jumlah pasangan Departemen x Posisi wajib SAMA dengan sebelum migrasi.'

SELECT
    d."DepartmentName" AS department_name,
    pos."PositionName" AS position_name,
    count(*) AS jumlah_izin_efektif
FROM public."SysAccessPolicy" p
LEFT JOIN public."MstDepartment" d   ON d."Id"   = p."DepartmentId"
LEFT JOIN public."MstPosition"   pos ON pos."Id" = p."PositionId"
WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
GROUP BY d."DepartmentName", pos."PositionName"
ORDER BY d."DepartmentName", pos."PositionName";

-- =====================================================================================
-- BAGIAN 6 — ROLLBACK
--
-- Membalikkan 4.1, 4.2, dan 4.3 tanpa menghapus satu baris pun.
-- Jalankan hanya bila migrasi sudah COMMIT dan harus dibatalkan.
-- =====================================================================================

/*  ---------- ROLLBACK ----------

BEGIN;

-- 6.1 Aktifkan kembali policy identitas lama yang dinonaktifkan 4.3.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = true,
    "UpdateDateTime" = now(),
    "UpdateBy" = :'AKTOR'::uuid
FROM be_sec_003b_identitas i
WHERE p."ActionAccessId" = i.action_access_id
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND NOT p."IsActive"
  AND NOT p."IsDelete";

-- 6.2 Nonaktifkan seluruh baris yang dibuat 4.1 dan 4.2.
--     Dikenali dari identitas barunya, bukan dari waktu pembuatan.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = false,
    "UpdateDateTime" = now(),
    "UpdateBy" = :'AKTOR'::uuid
FROM be_sec_003b_identitas i
WHERE p."ActionAccessId" = i.action_access_id
  AND (i.resource, i.action) IN (
        ('PatientProcedure','Edit'), ('PatientProcedure','Approve'),
        ('PatientProcedure','Execute'), ('PatientProcedure','RemoveDraft'),
        ('PatientProcedure','Cancel'), ('PatientProcedure','Select'),
        ('DoctorQueue','Call'), ('DoctorQueue','StartConsultation'),
        ('DoctorQueue','FinishConsultation'), ('DoctorQueue','Skip'),
        ('DoctorQueue','NoShow'), ('DoctorQueue','Requeue'),
        ('DoctorConsultation','WriteSoap'), ('DoctorConsultation','Complete'),
        ('DoctorConsultation','Cancel'),
        ('PatientAssessment','Complete'), ('PatientAssessment','Cancel'),
        ('PatientAssessment','Amend'),
        ('PatientDiagnosis','SetPrimary'), ('PatientDiagnosis','Resolve'),
        ('PatientDiagnosis','Cancel'),
        ('PatientVitalSign','Verify'), ('PatientVitalSign','NotifyDoctor'),
        ('PatientVitalSign','Cancel'))
  AND p."IsActive";

-- Ganti dengan COMMIT; hanya setelah keluaran bagian 5 kembali ke angka sebelum migrasi.
ROLLBACK;

    ---------- BATAS ROLLBACK ---------- */

\echo ''
\echo '==== SELESAI ===='
