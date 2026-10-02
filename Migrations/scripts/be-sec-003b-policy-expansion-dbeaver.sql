-- =====================================================================================
-- BE-SEC-003B — Perluasan SysAccessPolicy ke exact historical capability set
-- VARIAN DBEAVER. Pasangan resmi be-sec-003b-policy-expansion.sql.
--
-- STATUS BERKAS INI: RANCANGAN. BELUM DIJALANKAN. BELUM DIVALIDASI TERHADAP DATABASE.
--
-- KENAPA BERKAS INI ADA
--
--   Varian psql memakai meta-command psql (penyetelan variabel dan pencetakan baris) serta
--   interpolasi variabel operator bergaya psql. Ketiganya TIDAK dikenal DBeaver. Operator
--   yang tidak memiliki psql karena itu tidak dapat menjalankan varian itu sama sekali.
--   Berkas ini adalah pasangannya: PostgreSQL biasa, dapat dijalankan langsung dari DBeaver.
--
--   SEMANTIK BISNIS KEDUANYA IDENTIK. Yang berbeda hanya cangkang eksekusinya:
--
--     varian psql                     varian DBeaver
--     ----------------------------------------------------------------------------
--     meta-command hentikan-saat-galat (tidak perlu — lihat CARA MENJALANKAN)
--     meta-command cetak-baris         komentar biasa
--     interpolasi variabel operator    current_setting('be_sec_003b.aktor')::uuid
--     (tanpa gerbang aktor)            gerbang aktor eksplisit: bukan nol-GUID,
--                                      dan wajib ada di AspNetUsers
--
--   Peta pemecahan, 24 identitas wajib, penurunan pemegang historis, aturan
--   PatientAssessment.Amend, sasaran Tahap 1, sasaran Tahap 2, sasaran rollback,
--   perlindungan kunci alami, kolom NOT NULL SysAccessPolicy, dan seluruh kardinalitas
--   yang diharapkan SAMA PERSIS dengan varian psql.
--
-- CARA MENJALANKAN
--
--   DBeaver: buka berkas ini, lalu Execute script (Alt+X) — bukan Execute statement.
--   Pastikan "Stop at error" aktif pada pengaturan eksekusi skrip DBeaver; itulah
--   padanan meta-command hentikan-saat-galat pada varian psql.
--
--   SELURUH bagian 0 sampai 3 WAJIB dijalankan lebih dulu DALAM KONEKSI YANG SAMA.
--   View sementara di bawah bersifat per-sesi: bila koneksi DBeaver diputus atau diganti,
--   view-nya hilang dan Tahap 1/2 tidak akan menemukan sasarannya. Bila ragu, jalankan
--   ulang berkas ini dari atas.
--
-- PRINSIP YANG MENGIKAT SELURUH BERKAS INI
--
--   1. TIDAK ADA nama Departemen atau Jabatan yang ditulis di sini.
--      TIDAK ADA GUID Departemen, Posisi, ControllerAccess, ActionAccess, maupun policy.
--      Seluruh kepemilikan diturunkan dari baris SysAccessPolicy yang SUDAH ADA.
--      Satu-satunya GUID yang diketik manusia adalah UUID operator.
--
--   2. BEFORE accessible endpoint set = AFTER accessible endpoint set.
--      Setiap identitas baru hanya diberikan kepada pasangan Departemen x Posisi yang
--      hari ini memang memegang identitas lama asalnya. Tidak ada penambahan.
--
--   3. Idempoten. Dijalankan dua kali menghasilkan keadaan yang sama.
--
--   4. Tidak ada baris yang dihapus. Penonaktifan memakai IsActive/IsDelete.
--
--   5. Kedua tahap tulis berakhir ROLLBACK. Apa adanya, berkas ini TIDAK mengubah apa pun.
--
-- PRASYARAT MUTLAK — DIKONFIRMASI PENGUKURAN 11 SEPTEMBER 2026
--
--   Pada database development saat itu, SELURUH identitas hasil pemecahan berstatus
--   TIDAK_TERDAFTAR, dan PatientProcedure.Update serta DoctorQueue.Update masih
--   IsActive=true / IsDelete=false. Artinya AccessMenuSeeder BELUM pernah berjalan
--   terhadap database ini pada HEAD.
--
--   PEMBARUAN BASELINE SOURCE — 17 SEPTEMBER 2026 (BE-SEC-015)
--
--     Pengukuran 11 September di atas diambil SEBELUM BE-SEC-012, BE-SEC-013, dan
--     BE-SEC-014. Baseline registry KANONIK DARI SOURCE kini:
--
--       Action 1.300 · Resource 340 · Modul 48
--
--     Baseline lama 1.286 / 339 / 48 sudah TIDAK BERLAKU sebagai target. Berkas ini tidak
--     pernah menegaskan angka registry mana pun, sehingga kontraknya tidak berubah — yang
--     berubah hanya jumlah identitas yang akan dibuat seeder pada langkah 1 di bawah:
--     seeder yang sama kini juga mendaftarkan 14 identitas BE-SEC-012/013 dan resource
--     WorkScheduleAssignment. Kedua puluh empat identitas target BE-SEC-003 TIDAK berubah
--     (verifier: 24 / 24).
--
--     KEADAAN DATABASE DEVELOPMENT ADALAH FAKTA RUNTIME TERPISAH. Ia TIDAK dapat
--     disimpulkan dari angka source di atas dan WAJIB DIUKUR ULANG lewat bagian 1
--     sesudah seeder dijalankan. Jangan memperlakukan 1.300 / 340 / 48 sebagai keadaan
--     database.
--
--   Urutan yang benar dan tidak boleh dibalik:
--
--     1. Aplikasi HEAD start SEKALI dalam jendela pemeliharaan, tanpa traffic pengguna.
--        AccessMenuSeeder membuat baris registry identitas baru dan menutup yang lama.
--     2. Jalankan bagian 0 sampai 3 berkas ini. Bagian 1.0b WAJIB berbunyi 24 / 24 dan
--        seluruh baris 1.0 serta 1.1 wajib berstatus 'ok'.
--     3. Baru Tahap 1, lalu verifikasi bagian 5, lalu Tahap 2.
--
--   Antara langkah 1 dan selesainya Tahap 1, identitas baru sudah ditegakkan endpoint
--   tetapi belum diberikan kepada siapa pun. Pada jendela itu dokter akan ditolak 403.
--   KARENA ITU langkah 1 sampai 3 wajib berada di dalam satu jendela pemeliharaan yang
--   sama, tertutup dari traffic pengguna.
--
-- CATATAN PENTING TENTANG IDENTITAS YANG SUDAH PENSIUN
--
--   AccessMenuSeeder menutup SysActionAccess milik PatientProcedure.Update dan
--   DoctorQueue.Update (IsActive=false, IsDelete=true). Baris SysAccessPolicy yang
--   menunjuk keduanya MASIH ADA dan flag-nya sendiri masih hidup — yang mati adalah
--   baris registry-nya.
--
--   Karena itu "siapa dulu pemegangnya" WAJIB dibaca dari flag POLICY, bukan dari flag
--   ACTION. Kalau penyaringnya ikut menuntut action aktif, himpunan sumbernya kosong
--   dan seluruh migrasi ini menghasilkan nol baris — gagal senyap.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 0 — Peta pemecahan (identitas lama -> identitas baru)
--
-- Ini satu-satunya bagian yang ditulis tangan, dan isinya HANYA nama identitas yang
-- berasal dari source code ([AccessPermission] pada controller). Bukan data.
--
-- View dihapus dalam urutan KEBALIKAN ketergantungan supaya berkas ini dapat dijalankan
-- ulang pada sesi DBeaver yang sama tanpa galat dependency.
-- =====================================================================================

DROP VIEW IF EXISTS be_sec_003b_target;
DROP VIEW IF EXISTS be_sec_003b_pemegang;
DROP VIEW IF EXISTS be_sec_003b_identitas;
DROP VIEW IF EXISTS be_sec_003b_wajib;
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
-- Karena itu Amend dikerjakan terpisah pada bagian 4.2, SESUDAH Complete.

-- =====================================================================================
-- BAGIAN 0b — Daftar 24 identitas target BE-SEC-003 yang WAJIB terdaftar dan AKTIF.
--
-- Peta bagian 0 memuat 23 identitas baru. Identitas ke-24, PatientAssessment.Amend,
-- sengaja tidak ada di sana karena sumber kepemilikannya bukan Update melainkan Complete.
-- Tanpa daftar terpisah ini, Amend yang belum terdaftar membuat Tahap 1 menyisipkan 35
-- baris, bukan 36 — TANPA error dan TANPA peringatan.
--
-- Isinya identik dengan tools/authorization-verifier/required-identities.txt.
--
-- CATATAN: berbeda dari be_sec_003b_identitas yang sengaja tidak menuntut action aktif
-- (karena identitas LAMA yang sudah pensiun justru harus ketemu), daftar ini menuntut
-- identitas BARU benar-benar AKTIF.
-- =====================================================================================

CREATE TEMP VIEW be_sec_003b_wajib (resource, action) AS
VALUES
    ('PatientProcedure',   'Select'),
    ('PatientProcedure',   'Edit'),
    ('PatientProcedure',   'Approve'),
    ('PatientProcedure',   'Execute'),
    ('PatientProcedure',   'RemoveDraft'),
    ('PatientProcedure',   'Cancel'),

    ('DoctorQueue',        'Call'),
    ('DoctorQueue',        'StartConsultation'),
    ('DoctorQueue',        'FinishConsultation'),
    ('DoctorQueue',        'Skip'),
    ('DoctorQueue',        'NoShow'),
    ('DoctorQueue',        'Requeue'),

    ('DoctorConsultation', 'WriteSoap'),
    ('DoctorConsultation', 'Complete'),
    ('DoctorConsultation', 'Cancel'),

    ('PatientAssessment',  'Complete'),
    ('PatientAssessment',  'Cancel'),
    ('PatientAssessment',  'Amend'),

    ('PatientDiagnosis',   'SetPrimary'),
    ('PatientDiagnosis',   'Resolve'),
    ('PatientDiagnosis',   'Cancel'),

    ('PatientVitalSign',   'Verify'),
    ('PatientVitalSign',   'NotifyDoctor'),
    ('PatientVitalSign',   'Cancel');

-- =====================================================================================
-- Resolusi identitas -> baris registry.
--
-- Sengaja TIDAK menuntut action aktif: identitas yang sudah pensiun pun harus ketemu,
-- justru merekalah yang policy-nya perlu dilestarikan.
-- =====================================================================================

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

-- 1.0 PRASYARAT MUTLAK: 24 identitas target wajib TERDAFTAR dan AKTIF.
--     Mencakup PatientAssessment.Amend, yang TIDAK tercakup 1.1 maupun 3.1.
--     Kolom status wajib ok untuk SELURUH 24 baris sebelum Tahap 1 dijalankan.
SELECT
    w.resource,
    w.action,
    CASE
        WHEN i.action_access_id IS NULL           THEN 'TIDAK_TERDAFTAR'
        WHEN i.action_is_delete                   THEN 'TERDAFTAR_TAPI_DIHAPUS'
        WHEN NOT i.action_is_active               THEN 'TERDAFTAR_TAPI_NONAKTIF'
        ELSE 'ok'
    END AS status
FROM be_sec_003b_wajib w
LEFT JOIN be_sec_003b_identitas i
       ON i.resource = w.resource AND i.action = w.action
ORDER BY w.resource, w.action;

-- 1.0b Ringkasan prasyarat — wajib berbunyi 24 / 24.
SELECT
    count(*) FILTER (
        WHERE i.action_access_id IS NOT NULL
          AND i.action_is_active
          AND NOT i.action_is_delete
    )                                   AS identitas_siap,
    count(*)                            AS identitas_wajib,
    count(*) FILTER (
        WHERE i.action_access_id IS NULL
           OR NOT i.action_is_active
           OR i.action_is_delete
    )                                   AS belum_siap
FROM be_sec_003b_wajib w
LEFT JOIN be_sec_003b_identitas i
       ON i.resource = w.resource AND i.action = w.action;

-- 1.1 Apakah setiap identitas pada peta benar-benar terdaftar?
--     Baris mana pun dengan status TIDAK_TERDAFTAR menghentikan migrasi:
--     berarti aplikasi belum pernah start di HEAD ini, atau namanya salah ketik.
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

-- 1.2 Pemegang identitas lama — inilah seluruh sumber kepemilikan.
--     Kalau bagian ini kosong untuk sebuah identitas lama, tidak ada yang dilestarikan.
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

-- 1.3 Baris yang AKAN DIBUAT (belum ada) — inti dry run.
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

-- 1.4 Ringkasan jumlah per identitas baru.
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

-- 1.5 SASARAN TAHAP 2 — hanya dua identitas pensiun milik BE-SEC-003B.
--     Predikatnya IDENTIK dengan bagian 4.3a, sehingga daftar di bawah adalah persis baris
--     yang akan dinonaktifkan Tahap 2 — tidak kurang, tidak lebih.
--     Kontrak pemilik: PatientProcedure.Update = 1, DoctorQueue.Update = 3, total 4.

-- 1.5a Rincian sasaran.
--
--     KOREKSI BE-SEC-017. Sebelumnya bagian ini hanya menyaring i.action_is_delete, tanpa
--     membatasi identitasnya. Artinya ia menampilkan SETIAP policy hidup yang menunjuk
--     baris registry yang sudah dihapus — termasuk BillingItemCategory.*, CompanyGuarantor.*,
--     dan identitas pensiun lain yang TIDAK ADA hubungannya dengan BE-SEC-003B. Dry-run
--     terhadap database development 17 September 2026 memang memunculkan baris-baris itu,
--     dan pembacanya wajar menyimpulkan kontrak 4 baris sudah salah. Padahal bagian 4.3a
--     sejak awal sudah dibatasi dua identitas; yang keliru hanya pratinjaunya.
SELECT
    i.resource,
    i.action,
    p."Id"            AS policy_id,
    p."DepartmentId"  AS department_id,
    p."PositionId"    AS position_id,
    d."DepartmentName" AS department_name,
    pos."PositionName" AS position_name
FROM public."SysAccessPolicy" p
JOIN be_sec_003b_identitas i ON i.action_access_id = p."ActionAccessId"
LEFT JOIN public."MstDepartment" d   ON d."Id"   = p."DepartmentId"
LEFT JOIN public."MstPosition"   pos ON pos."Id" = p."PositionId"
WHERE i.action_is_delete
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive"
ORDER BY i.resource, i.action, d."DepartmentName", pos."PositionName";

-- 1.5b Ringkasan kontrak Tahap 2 — wajib 1 + 3 = 4.
--      Angka di bawah DIBACA dari database apa adanya. Bila statusnya BEDA, jangan
--      jalankan Tahap 2 dan laporkan selisihnya kepada pemilik sistem.
SELECT
    count(*) FILTER (WHERE i.resource = 'PatientProcedure' AND i.action = 'Update') AS patientprocedure_update,
    count(*) FILTER (WHERE i.resource = 'DoctorQueue'      AND i.action = 'Update') AS doctorqueue_update,
    count(*)                                                                        AS total,
    CASE
        WHEN count(*) = 4
         AND count(*) FILTER (WHERE i.resource = 'PatientProcedure' AND i.action = 'Update') = 1
         AND count(*) FILTER (WHERE i.resource = 'DoctorQueue'      AND i.action = 'Update') = 3
        THEN 'cocok — kontrak 1 + 3 = 4 terpenuhi'
        ELSE 'BEDA — JANGAN jalankan Tahap 2; gerbang 4.3b akan membatalkan transaksi'
    END AS status
FROM public."SysAccessPolicy" p
JOIN be_sec_003b_identitas i ON i.action_access_id = p."ActionAccessId"
WHERE i.action_is_delete
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive";

-- 1.5c OBSERVASI DI LUAR SCOPE — JANGAN dijadikan sasaran Tahap 2.
--      Identitas pensiun LAIN yang masih dipegang policy hidup. BE-SEC-003B TIDAK
--      menyentuhnya sama sekali. Ditampilkan hanya supaya keberadaannya tercatat dan tidak
--      lagi disalahartikan sebagai sasaran Tahap 2. Pembersihannya, bila memang diinginkan,
--      adalah keputusan pemilik modul masing-masing dan task tersendiri.
SELECT
    i.resource,
    i.action,
    count(*) AS policy_hidup,
    'DI LUAR SCOPE BE-SEC-003B — tidak disentuh' AS catatan
FROM public."SysAccessPolicy" p
JOIN be_sec_003b_identitas i ON i.action_access_id = p."ActionAccessId"
WHERE i.action_is_delete
  AND (i.resource, i.action) NOT IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive"
GROUP BY i.resource, i.action
ORDER BY i.resource, i.action;

-- =====================================================================================
-- BAGIAN 2 — DRY RUN PatientAssessment.Amend
--
-- Keputusan pemilik sistem: Amend diberikan kepada pemegang efektif
-- PatientAssessment.Complete. Amend adalah FEATURE GATE; penjaga keduanya tetap aturan
-- kepemilikan dokumen (RM-DEC-004) yang tidak dapat dilewati.
--
-- URUTAN ITU PENTING. PatientAssessment.Complete adalah identitas BARU yang baru terisi
-- oleh bagian 4.1 migrasi ini. Bila Amend dihitung sebelum Complete terisi, hasilnya
-- NOL BARIS. Karena itu 4.2 dijalankan SESUDAH 4.1.
-- =====================================================================================

-- 2.1 Pemegang PatientAssessment.Complete SEKARANG (sebelum migrasi).
--     Bila kosong, keputusan owner dibaca harfiah menghasilkan NOL baris Amend.
SELECT count(*) AS pemegang_complete_sekarang
FROM be_sec_003b_pemegang
WHERE resource = 'PatientAssessment' AND action = 'Complete';

-- 2.2 Pemegang PatientAssessment.Complete SESUDAH migrasi (= pemegang Update).
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

-- 3.1 Sisa pekerjaan (wajib 0 sesudah migrasi).
SELECT count(*) AS sisa_baris_belum_dibuat
FROM be_sec_003b_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id
);

-- 3.2 Duplikat kunci alami (wajib 0, sebelum maupun sesudah).
SELECT "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId", count(*) AS jumlah
FROM public."SysAccessPolicy"
GROUP BY 1,2,3,4
HAVING count(*) > 1;

-- =====================================================================================
-- BAGIAN 4 — PENULISAN.
--
-- TIDAK AKAN BERJALAN apa adanya. Untuk menjalankannya, seseorang harus:
--   (a) meninjau seluruh keluaran bagian 1 dan 2 lebih dulu;
--   (b) melepas komentar blok Tahap 1;
--   (c) mengisi UUID operator pada 4.0a dengan Guid pengguna yang bertanggung jawab;
--   (d) mengganti ROLLBACK di akhir tahap dengan COMMIT.
--
-- Seluruh blok berjalan dalam SATU transaksi. Gagal di tengah = tidak ada yang berubah.
--
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
-- =====================================================================================

/*  ---------- TAHAP 1 — LEPAS KOMENTAR HANYA SETELAH DRY RUN DITINJAU ----------

BEGIN;

-- 4.0a Parameter transaksi. Nilai bertahan sampai transaksi berakhir (is_local = true),
--      dan terbaca di dalam blok DO maupun DML di bawah.
--
--      >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR YANG SEBENARNYA <<<
SELECT set_config('be_sec_003b.aktor', '00000000-0000-0000-0000-000000000000', true);

-- 4.0b GERBANG OPERATOR — wajib nyata.
DO $$
DECLARE
    aktor_teks text := current_setting('be_sec_003b.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_sec_003b.aktor belum disetel. Jalankan 4.0a lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION
            'UUID operator belum diisi. Ganti nilai be_sec_003b.aktor pada 4.0a dengan Guid '
            'pengguna yang bertanggung jawab atas migrasi ini. Jejak audit tanpa pelaku '
            'tidak dapat dipertanggungjawabkan.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION
            'Operator % tidak ditemukan pada AspNetUsers. Pakai Guid pengguna yang benar-benar ada.',
            aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 4.0c GERBANG PRASYARAT — WAJIB, DAN SENGAJA MENGGAGALKAN TRANSAKSI.
--
-- Menghentikan Tahap 1 bila salah satu dari 24 identitas target belum terdaftar, atau
-- terdaftar tetapi tidak aktif / sudah dihapus. Tanpa gerbang ini, identitas yang hilang
-- hanya membuat JOIN-nya kosong: 4.1/4.2 menyisipkan lebih sedikit baris dari yang
-- diharapkan dan transaksi tetap COMMIT — gagal senyap.
DO $$
DECLARE
    belum_siap  integer;
    rincian     text;
BEGIN
    SELECT count(*),
           string_agg(w.resource || '.' || w.action || ' (' ||
               CASE
                   WHEN i.action_access_id IS NULL THEN 'TIDAK_TERDAFTAR'
                   WHEN i.action_is_delete         THEN 'TERDAFTAR_TAPI_DIHAPUS'
                   ELSE 'TERDAFTAR_TAPI_NONAKTIF'
               END || ')', ', ' ORDER BY w.resource, w.action)
      INTO belum_siap, rincian
      FROM be_sec_003b_wajib w
      LEFT JOIN be_sec_003b_identitas i
             ON i.resource = w.resource AND i.action = w.action
     WHERE i.action_access_id IS NULL
        OR NOT i.action_is_active
        OR i.action_is_delete;

    IF belum_siap > 0 THEN
        RAISE EXCEPTION
            'BE-SEC-003B PRASYARAT GAGAL: % dari 24 identitas target belum siap -> %. '
            'Jalankan aplikasi HEAD sekali dalam jendela pemeliharaan supaya '
            'AccessMenuSeeder membuat baris registry-nya, lalu ulangi bagian 1.',
            belum_siap, rincian;
    END IF;

    RAISE NOTICE 'Gerbang prasyarat lulus: 24 / 24 identitas target terdaftar dan aktif.';
END $$;

-- 4.1 Pelestarian: seluruh identitas baru pada peta bagian 0.
--
--     "UpdateBy", "DeleteBy", dan "CancelBy" WAJIB disebut. Ketiganya uuid NOT NULL pada
--     SysAccessPolicy dan — berbeda dari tabel lain di repository ini — TIDAK memiliki
--     default database (20260516060830_initializeSetup.cs, CreateTable "SysAccessPolicy";
--     SysAccessPolicyConfiguration.cs hanya memberi default pada IsAllowed dan IsActive).
--     Menghilangkannya membuat penyisipan gagal 23502:
--     'null value in column "UpdateBy" violates not-null constraint'.
--
--     Nilainya Guid.Empty, mengikuti konvensi aplikasi untuk baris SysAccessPolicy baru
--     (RoleAccessController.cs): hanya CreateBy yang memuat pelaku.
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy")
SELECT
    gen_random_uuid(), t.department_id, t.position_id,
    t.controller_access_id, t.action_access_id,
    true, true, false, false, now(), current_setting('be_sec_003b.aktor')::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid
FROM be_sec_003b_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id);

-- 4.2 PatientAssessment.Amend — SESUDAH 4.1, sehingga Complete sudah terisi.
--     Kolom audit mengikuti alasan yang sama dengan 4.1 di atas.
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy")
SELECT
    gen_random_uuid(), h.department_id, h.position_id,
    amend.controller_access_id, amend.action_access_id,
    true, true, false, false, now(), current_setting('be_sec_003b.aktor')::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid
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

-- 4.2b VERIFIKASI DI DALAM TRANSAKSI — menutup sisa jalur gagal senyap.
--
-- Gerbang 4.0c membuktikan identitasnya ADA. Blok ini membuktikan barisnya BENAR-BENAR
-- TERBENTUK. Keduanya berbeda: 4.0c memeriksa registry, 4.2b memeriksa hasil penulisan.
--
-- Angka pembandingnya DITURUNKAN dari data, bukan ditulis tangan, sehingga tetap benar
-- bila kepemilikan di database berubah. Nilai yang diharapkan pada database development
-- 11 September 2026 adalah 35 pelestarian + 1 Amend = 36; NOTICE di bawah mencetak
-- angka sebenarnya supaya dapat dibandingkan dengan bagian 1.4.
DO $$
DECLARE
    sisa_peta       integer;
    pemegang_compl  integer;
    baris_amend     integer;
BEGIN
    -- (a) Seluruh baris peta bagian 0 wajib sudah ada.
    SELECT count(*) INTO sisa_peta
      FROM be_sec_003b_target t
     WHERE NOT EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
        WHERE p."DepartmentId"       = t.department_id
          AND p."PositionId"         = t.position_id
          AND p."ControllerAccessId" = t.controller_access_id
          AND p."ActionAccessId"     = t.action_access_id);

    IF sisa_peta > 0 THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 1 GAGAL: % baris peta belum terbentuk sesudah 4.1.',
            sisa_peta;
    END IF;

    -- (b) Amend wajib diterima PERSIS oleh pemegang Complete — tidak kurang, tidak lebih.
    SELECT count(*) INTO pemegang_compl
      FROM be_sec_003b_pemegang
     WHERE resource = 'PatientAssessment' AND action = 'Complete';

    SELECT count(*) INTO baris_amend
      FROM be_sec_003b_pemegang
     WHERE resource = 'PatientAssessment' AND action = 'Amend';

    IF baris_amend <> pemegang_compl THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 1 GAGAL: baris Amend = %, pemegang Complete = %. '
            'Keduanya wajib sama. Selisih berarti 4.2 tidak menyambung ke registry Amend.',
            baris_amend, pemegang_compl;
    END IF;

    RAISE NOTICE 'Tahap 1: seluruh baris peta terbentuk; Amend = % baris (= pemegang Complete). '
                 'Bandingkan total dengan bagian 1.4 — nilai yang diharapkan 36.',
                 baris_amend;
END $$;

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

-- 4.3.0 Parameter transaksi. Tahap 2 adalah transaksi TERSENDIRI, sehingga UUID operator
--       WAJIB disetel ulang di sini — nilai dari Tahap 1 sudah hilang bersama transaksinya.
--
--       >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR YANG SEBENARNYA <<<
SELECT set_config('be_sec_003b.aktor', '00000000-0000-0000-0000-000000000000', true);

-- 4.3.0b GERBANG OPERATOR — wajib nyata.
DO $$
DECLARE
    aktor_teks text := current_setting('be_sec_003b.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_sec_003b.aktor belum disetel. Jalankan 4.3.0 lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION
            'UUID operator belum diisi. Ganti nilai be_sec_003b.aktor pada 4.3.0 dengan Guid '
            'pengguna yang bertanggung jawab atas pencabutan ini.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION 'Operator % tidak ditemukan pada AspNetUsers.', aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 4.3 Nonaktifkan policy yang menggantung pada identitas yang sudah pensiun.
--     Dijalankan TERAKHIR dan sebagai transaksi TERSENDIRI.
--
--     KONTRAK KARDINALITAS YANG DITETAPKAN PEMILIK SISTEM:
--
--       PatientProcedure.Update = 1 policy
--       DoctorQueue.Update      = 3 policy
--       TOTAL                   = 4 policy

-- 4.3a Bekukan sasaran SEBELUM menulis, supaya yang ditegaskan dan yang diubah benar-benar
--      himpunan yang sama. Diturunkan dari kunci bisnis (resource, action) lewat registry.
--      TIDAK ADA GUID policy yang diketik.
CREATE TEMP TABLE be_sec_003b_tahap2_sasaran ON COMMIT DROP AS
SELECT p."Id" AS policy_id, i.resource, i.action
FROM public."SysAccessPolicy" p
JOIN be_sec_003b_identitas i ON i.action_access_id = p."ActionAccessId"
WHERE i.action_is_delete
  AND (i.resource, i.action) IN (('PatientProcedure','Update'), ('DoctorQueue','Update'))
  AND p."IsActive";

-- 4.3b GERBANG KARDINALITAS — TEPAT 4, dengan rincian 1 + 3.
DO $$
DECLARE
    jml    integer;
    jml_pp integer;
    jml_dq integer;
BEGIN
    SELECT count(*),
           count(*) FILTER (WHERE resource = 'PatientProcedure' AND action = 'Update'),
           count(*) FILTER (WHERE resource = 'DoctorQueue'      AND action = 'Update')
      INTO jml, jml_pp, jml_dq
      FROM be_sec_003b_tahap2_sasaran;

    IF jml <> 4 OR jml_pp <> 1 OR jml_dq <> 3 THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 2 DIBATALKAN: sasaran = % baris (PatientProcedure.Update=%, '
            'DoctorQueue.Update=%), diharapkan tepat 4 (1 + 3). Selisih berarti keadaan '
            'database berbeda dari bukti yang mendasari keputusan pemilik. Tinjau bagian 1.5 '
            'lebih dulu; jangan memaksakan skrip ini.',
            jml, jml_pp, jml_dq;
    END IF;

    RAISE NOTICE 'Gerbang Tahap 2 lulus: tepat 4 sasaran (PatientProcedure.Update=1, DoctorQueue.Update=3).';
END $$;

-- 4.3c Penonaktifan, DIBATASI pada himpunan beku 4.3a. Baris yang benar-benar berubah
--      direkam lewat RETURNING supaya jumlahnya dapat ditegaskan tanpa menebak.
CREATE TEMP TABLE be_sec_003b_tahap2_terubah ON COMMIT DROP AS
WITH upd AS (
    UPDATE public."SysAccessPolicy" p
    SET "IsActive"       = false,
        "UpdateDateTime" = now(),
        "UpdateBy"       = current_setting('be_sec_003b.aktor')::uuid
    FROM be_sec_003b_tahap2_sasaran s
    WHERE p."Id" = s.policy_id
      AND p."IsActive"
    RETURNING p."Id" AS policy_id
)
SELECT policy_id FROM upd;

-- 4.3d PENEGASAN SESUDAH TULIS — tepat 4 baris berubah, keempatnya nonaktif, dan tidak
--      ada policy lain yang ikut tersentuh transaksi ini.
DO $$
DECLARE
    terubah     integer;
    masih_aktif integer;
    lain        integer;
BEGIN
    SELECT count(*) INTO terubah FROM be_sec_003b_tahap2_terubah;
    IF terubah <> 4 THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 2 GAGAL: % baris berubah, diharapkan tepat 4. Transaksi dibatalkan.',
            terubah;
    END IF;

    SELECT count(*) INTO masih_aktif
      FROM public."SysAccessPolicy" p
      JOIN be_sec_003b_tahap2_sasaran s ON s.policy_id = p."Id"
     WHERE p."IsActive";
    IF masih_aktif <> 0 THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 2 GAGAL: % sasaran masih aktif sesudah UPDATE. Transaksi dibatalkan.',
            masih_aktif;
    END IF;

    -- now() adalah transaction_timestamp(): setiap baris yang disentuh transaksi ini
    -- memiliki UpdateDateTime yang sama persis. Baris ber-stempel itu yang BUKAN sasaran
    -- berarti ada penulisan di luar scope.
    SELECT count(*) INTO lain
      FROM public."SysAccessPolicy" p
     WHERE p."UpdateDateTime" = now()
       AND NOT EXISTS (SELECT 1 FROM be_sec_003b_tahap2_sasaran s WHERE s.policy_id = p."Id");
    IF lain <> 0 THEN
        RAISE EXCEPTION
            'BE-SEC-003B TAHAP 2 GAGAL: % policy DI LUAR sasaran ikut berubah. Transaksi dibatalkan.',
            lain;
    END IF;

    RAISE NOTICE 'Tahap 2 lulus: tepat 4 policy dinonaktifkan, tidak ada policy lain yang tersentuh.';
END $$;

-- Ganti baris berikut dengan COMMIT; hanya bila parity di atas sudah terbukti.
ROLLBACK;

    ---------- BATAS TAHAP 2 ---------- */

-- =====================================================================================
-- BAGIAN 5 — Verifikasi sesudah penulisan
-- =====================================================================================

-- 5.1 Metrik sesudah.
SELECT
    (SELECT count(*) FROM public."SysAccessPolicy")                                AS policy_fisik,
    (SELECT count(*) FROM public."SysAccessPolicy"
      WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete")                         AS policy_efektif,
    (SELECT count(*) FROM public."SysActionAccess"
      WHERE "IsActive" AND NOT "IsDelete")                                         AS action_aktif,
    (SELECT count(*) FROM (
        SELECT DISTINCT "DepartmentId", "PositionId" FROM public."SysAccessPolicy"
        WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete") x)                    AS pasangan_dept_posisi;

-- 5.2 Parity — tidak boleh ada pasangan yang kehilangan atau memperoleh.
--     Jumlah pasangan Departemen x Posisi wajib SAMA dengan sebelum migrasi.
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

-- Guid yang SAMA dengan yang dipakai saat migrasi.
SELECT set_config('be_sec_003b.aktor', '00000000-0000-0000-0000-000000000000', true);

DO $$
DECLARE
    aktor_teks text := current_setting('be_sec_003b.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_sec_003b.aktor belum disetel.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION 'UUID operator belum diisi.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION 'Operator % tidak ditemukan pada AspNetUsers.', aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 6.1 Aktifkan kembali policy identitas lama yang dinonaktifkan 4.3.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = true,
    "UpdateDateTime" = now(),
    "UpdateBy" = current_setting('be_sec_003b.aktor')::uuid
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
    "UpdateBy" = current_setting('be_sec_003b.aktor')::uuid
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

-- ==== SELESAI ====
