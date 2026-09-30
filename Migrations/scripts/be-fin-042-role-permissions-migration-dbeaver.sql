-- =====================================================================================
-- BE-FIN-042 — Penyelarasan nama Resource 6 controller legacy Finance ke nama kanonikal
-- (FIN-DEC-078, permission-audit-matrix.md §D.5) — pelestarian SysAccessPolicy lintas rename
-- VARIAN DBEAVER. Pasangan resmi be-fin-042-role-permissions-migration.sql.
--
-- STATUS BERKAS INI: RANCANGAN. BELUM DIJALANKAN. BELUM DIVALIDASI TERHADAP DATABASE.
--   Bagian 1 dan 3 HANYA MEMBACA (dry run). Aman dijalankan kapan saja.
--   Bagian 2 dan 4 MENULIS dan sengaja dibungkus blok komentar yang TIDAK akan jalan
--   sampai seseorang mengubahnya secara sadar. Bagian 5 adalah rollback.
--
-- KENAPA BERKAS INI ADA
--   Varian psql memakai meta-command psql (\set ON_ERROR_STOP on, \echo, :'AKTOR').
--   Sintaks tersebut TIDAK dikenal oleh engine PostgreSQL standar maupun antarmuka DBeaver,
--   sehingga eksekusi di DBeaver akan gagal dengan error:
--     SQL Error [42601]: ERROR: syntax error at or near "on"
--   Berkas ini adalah pasangannya: 100% PostgreSQL murni yang kompatibel penuh dengan DBeaver.
--
--   SEMANTIK BISNIS KEDUANYA IDENTIK:
--     varian psql                     varian DBeaver
--     ----------------------------------------------------------------------------
--     \set ON_ERROR_STOP on            Dikelola via setting DBeaver ("Stop on error")
--     \echo '...'                      Komentar SQL / query info
--     \set AKTOR '...' & :'AKTOR'      SELECT set_config('be_fin_042.aktor', '...', true)
--                                      & current_setting('be_fin_042.aktor', true)::uuid
--                                      + gerbang verifikasi operator di AspNetUsers
--
-- CARA MENJALANKAN DI DBEAVER
--   1. Buka berkas ini di editor SQL DBeaver.
--   2. Jalankan skrip penuh dengan tombol "Execute SQL Script" (Alt+X) — bukan Execute statement (Ctrl+Enter).
--   3. Pastikan opsi "Stop on error" aktif pada pengaturan eksekusi DBeaver.
--   4. SELURUH Bagian 0 sampai Bagian 1 WAJIB dijalankan dalam SATU sesi/koneksi yang sama,
--      karena view sementara (TEMP VIEW) hanya hidup di dalam koneksi tersebut.
--
-- PRASYARAT MUTLAK, URUTAN TIDAK BOLEH DIBALIK
--   1. Deploy source hasil rename 6 controller (BE-FIN-042) ke lingkungan target.
--   2. Jalankan aplikasi HEAD SEKALI dalam jendela pemeliharaan, tanpa traffic pengguna,
--      supaya `AccessMenuSeeder` membuat 6 baris registry BARU dan menutup 6 baris LAMA.
--   3. Jalankan Bagian 1 (dry run) skrip ini. Bagian 1.0 wajib 'hilang = 0' untuk keenam resource
--      baru sebelum Tahap 1 dijalankan.
--   4. Tahap 1 -> COMMIT -> verifikasi parity (Bagian 3) ditinjau manusia -> Tahap 2 -> COMMIT.
--
-- ENAM PASANG RENAME (FIN-DEC-078, §D.5) — RENAME MURNI, ACTION NAME TIDAK BERUBAH:
--   Payment -> FinancePayment | Receipt -> FinanceReceipt | Receivable -> FinanceReceivable
--   SupplierPayable -> FinanceSupplierPayable | BillingIntake -> FinanceBillingIntake
--   AccountingEvents -> FinanceAccountingEvent
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 0 — Peta rename (resource lama -> resource baru) dan view bantu.
-- Satu-satunya bagian yang ditulis tangan; isinya HANYA nama resource dari source code
-- ([AccessController] ControllerName pada 6 controller). Bukan data, bukan GUID.
-- =====================================================================================

DROP VIEW IF EXISTS be_fin_042_target;
DROP VIEW IF EXISTS be_fin_042_pemegang;
DROP VIEW IF EXISTS be_fin_042_identitas;
DROP VIEW IF EXISTS be_fin_042_peta;

CREATE TEMP VIEW be_fin_042_peta (resource_lama, resource_baru) AS
VALUES
    ('Payment',          'FinancePayment'),
    ('Receipt',          'FinanceReceipt'),
    ('Receivable',       'FinanceReceivable'),
    ('SupplierPayable',  'FinanceSupplierPayable'),
    ('BillingIntake',    'FinanceBillingIntake'),
    ('AccountingEvents', 'FinanceAccountingEvent');

CREATE TEMP VIEW be_fin_042_identitas AS
SELECT
    c."ControllerName" AS resource,
    a."ActionName"     AS action,
    c."Id"             AS controller_access_id,
    a."Id"             AS action_access_id,
    a."IsActive"       AS action_is_active,
    a."IsDelete"       AS action_is_delete
FROM public."SysActionAccess" a
JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId";

-- Pemegang efektif identitas LAMA, dibaca dari flag POLICY — bukan flag registry.
-- Identitas lama sudah tertutup (IsDelete) di registry sesudah seeder berjalan, tetapi
-- baris SysAccessPolicy yang menunjuknya masih hidup, dan itulah kepemilikan yang harus
-- dilestarikan (pola identik BE-SEC-003B).
CREATE TEMP VIEW be_fin_042_pemegang AS
SELECT
    i.resource, i.action, i.controller_access_id, i.action_access_id,
    p."Id" AS policy_id, p."DepartmentId" AS department_id, p."PositionId" AS position_id
FROM be_fin_042_identitas i
JOIN public."SysAccessPolicy" p
    ON p."ActionAccessId" = i.action_access_id AND p."ControllerAccessId" = i.controller_access_id
WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete";

-- Baris yang SEHARUSNYA ada sesudah migrasi: pemegang identitas lama disambung ke
-- identitas baru dengan action name PERSIS sama (rename murni, bukan fan-out).
CREATE TEMP VIEW be_fin_042_target AS
SELECT DISTINCT
    m.resource_lama, m.resource_baru, lama.action,
    baru.controller_access_id, baru.action_access_id,
    lama.department_id, lama.position_id
FROM be_fin_042_peta m
JOIN be_fin_042_pemegang lama ON lama.resource = m.resource_lama
JOIN be_fin_042_identitas baru ON baru.resource = m.resource_baru AND baru.action = lama.action;

-- =====================================================================================
-- BAGIAN 1 — DRY RUN. Hanya membaca.
-- =====================================================================================

-- 1.0 Prasyarat: 6 resource baru wajib terdaftar & aktif untuk tiap action lama.
--     Baris "hilang > 0" berarti AccessMenuSeeder belum dijalankan pada HEAD ini,
--     atau nama controller di source tidak persis sama dengan peta Bagian 0.
SELECT
    m.resource_lama, m.resource_baru,
    count(*) FILTER (WHERE baru.action_access_id IS NULL) AS hilang,
    count(*)                                              AS total_action_lama_bermakna
FROM be_fin_042_peta m
JOIN be_fin_042_pemegang lama ON lama.resource = m.resource_lama
LEFT JOIN be_fin_042_identitas baru
       ON baru.resource = m.resource_baru AND baru.action = lama.action
      AND baru.action_is_active AND NOT baru.action_is_delete
GROUP BY m.resource_lama, m.resource_baru
ORDER BY m.resource_lama;

-- 1.1 Pemegang identitas lama, per resource (inilah seluruh sumber kepemilikan).
SELECT h.resource, h.action, d."DepartmentName", pos."PositionName", h.department_id, h.position_id
FROM be_fin_042_pemegang h
LEFT JOIN public."MstDepartment" d   ON d."Id"   = h.department_id
LEFT JOIN public."MstPosition"   pos ON pos."Id" = h.position_id
ORDER BY h.resource, h.action, d."DepartmentName", pos."PositionName";

-- 1.2 Baris yang AKAN DIBUAT Tahap 1 (belum ada).
SELECT
    t.resource_lama, t.resource_baru, t.action,
    d."DepartmentName", pos."PositionName"
FROM be_fin_042_target t
LEFT JOIN public."MstDepartment" d   ON d."Id"   = t.department_id
LEFT JOIN public."MstPosition"   pos ON pos."Id" = t.position_id
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id)
ORDER BY t.resource_baru, t.action, d."DepartmentName";

-- 1.3 Ringkasan jumlah per resource baru.
SELECT t.resource_baru, count(*) AS baris_akan_dibuat
FROM be_fin_042_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id)
GROUP BY t.resource_baru
ORDER BY t.resource_baru;

-- =====================================================================================
-- BAGIAN 2 — TAHAP 1: buat baris pengganti (idempotent).
-- TIDAK AKAN BERJALAN apa adanya — lepas komentar hanya sesudah meninjau Bagian 1.
-- =====================================================================================

/*  ---------- TAHAP 1 — LEPAS KOMENTAR HANYA SETELAH DRY RUN DITINJAU ----------

BEGIN;

-- 2.0a Parameter transaksi. Nilai bertahan sampai transaksi berakhir (is_local = true).
--      >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR / ADMIN PENGESAH MIGRASI <<<
SELECT set_config('be_fin_042.aktor', '00000000-0000-0000-0000-000000000000', true);

-- 2.0b GERBANG OPERATOR — wajib nyata dan terdaftar di AspNetUsers.
DO $$
DECLARE
    aktor_teks text := current_setting('be_fin_042.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_fin_042.aktor belum disetel. Jalankan 2.0a lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION
            'UUID operator belum diisi. Ganti nilai be_fin_042.aktor pada 2.0a dengan Guid '
            'pengguna yang bertanggung jawab atas migrasi ini. Jejak audit tanpa pelaku '
            'tidak dapat dipertanggungjawabkan.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION
            'Operator % tidak ditemukan pada AspNetUsers. Gunakan Guid pengguna yang benar-benar ada.',
            aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 2.0c GERBANG PRASYARAT — menghentikan transaksi bila salah satu resource baru belum
-- terdaftar aktif untuk seluruh action yang dibutuhkan pemegang lama.
DO $$
DECLARE belum_siap integer; rincian text;
BEGIN
    SELECT count(*), string_agg(DISTINCT resource_baru, ', ')
      INTO belum_siap, rincian
      FROM (
        SELECT m.resource_baru
        FROM be_fin_042_peta m
        JOIN be_fin_042_pemegang lama ON lama.resource = m.resource_lama
        LEFT JOIN be_fin_042_identitas baru
               ON baru.resource = m.resource_baru AND baru.action = lama.action
              AND baru.action_is_active AND NOT baru.action_is_delete
        WHERE baru.action_access_id IS NULL
      ) x;

    IF belum_siap > 0 THEN
        RAISE EXCEPTION
            'BE-FIN-042 PRASYARAT GAGAL: resource baru belum lengkap terdaftar/aktif -> %. '
            'Jalankan aplikasi HEAD sekali dalam jendela pemeliharaan supaya AccessMenuSeeder '
            'membuat baris registry-nya, lalu ulangi Bagian 1.', rincian;
    END IF;

    RAISE NOTICE 'Gerbang prasyarat lulus.';
END $$;

-- 2.1 Pelestarian. "UpdateBy"/"DeleteBy"/"CancelBy" WAJIB disebut — uuid NOT NULL tanpa
-- default database pada SysAccessPolicy (lihat SysAccessPolicyConfiguration.cs).
INSERT INTO public."SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy")
SELECT
    gen_random_uuid(), t.department_id, t.position_id,
    t.controller_access_id, t.action_access_id,
    true, true, false, false, now(), current_setting('be_fin_042.aktor', true)::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid
FROM be_fin_042_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id);

-- 2.2 VERIFIKASI DI DALAM TRANSAKSI — seluruh baris target benar-benar terbentuk.
DO $$
DECLARE sisa integer;
BEGIN
    SELECT count(*) INTO sisa
      FROM be_fin_042_target t
     WHERE NOT EXISTS (
        SELECT 1 FROM public."SysAccessPolicy" p
        WHERE p."DepartmentId"       = t.department_id
          AND p."PositionId"         = t.position_id
          AND p."ControllerAccessId" = t.controller_access_id
          AND p."ActionAccessId"     = t.action_access_id);

    IF sisa > 0 THEN
        RAISE EXCEPTION 'BE-FIN-042 TAHAP 1 GAGAL: % baris target belum terbentuk.', sisa;
    END IF;

    RAISE NOTICE 'Tahap 1 lulus: seluruh baris target terbentuk.';
END $$;

-- Ganti baris berikut dengan COMMIT; hanya bila keluaran Bagian 1 sudah ditinjau benar.
ROLLBACK;

    ---------- BATAS TAHAP 1 ---------- */

-- =====================================================================================
-- JEDA WAJIB. Jalankan Bagian 3 (verifikasi parity) SEKARANG, tinjau bersama pemilik
-- sistem, sebelum lanjut ke Tahap 2. Sampai Tahap 2 dijalankan, policy lama MASIH AKTIF —
-- staf memegang identitas lama DAN baru sekaligus untuk sementara. Ini aman: identitas
-- lama sudah tidak dijaga endpoint mana pun (registry-nya ditutup seeder).
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 3 — Verifikasi parity sesudah Tahap 1
-- =====================================================================================

-- 3.1 Sisa target belum terbentuk (wajib 0 sesudah Tahap 1).
SELECT count(*) AS sisa_baris_belum_dibuat
FROM be_fin_042_target t
WHERE NOT EXISTS (
    SELECT 1 FROM public."SysAccessPolicy" p
    WHERE p."DepartmentId"       = t.department_id
      AND p."PositionId"         = t.position_id
      AND p."ControllerAccessId" = t.controller_access_id
      AND p."ActionAccessId"     = t.action_access_id);

-- 3.2 Parity — jumlah pasangan Departemen x Posisi wajib SAMA sebelum/sesudah.
SELECT d."DepartmentName", pos."PositionName", count(*) AS jumlah_izin_efektif
FROM public."SysAccessPolicy" p
LEFT JOIN public."MstDepartment" d   ON d."Id"   = p."DepartmentId"
LEFT JOIN public."MstPosition"   pos ON pos."Id" = p."PositionId"
WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
GROUP BY d."DepartmentName", pos."PositionName"
ORDER BY d."DepartmentName", pos."PositionName";

-- =====================================================================================
-- BAGIAN 4 — TAHAP 2: nonaktifkan policy lama. HANYA setelah parity Bagian 3 terbukti.
-- TIDAK AKAN BERJALAN apa adanya.
-- =====================================================================================

/*  ---------- TAHAP 2 — HANYA SETELAH PARITY TERBUKTI ----------

BEGIN;

-- 4.0a Parameter transaksi.
--      >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR / ADMIN PENGESAH MIGRASI <<<
SELECT set_config('be_fin_042.aktor', '00000000-0000-0000-0000-000000000000', true);

-- 4.0b GERBANG OPERATOR.
DO $$
DECLARE
    aktor_teks text := current_setting('be_fin_042.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_fin_042.aktor belum disetel. Jalankan 4.0a lebih dulu.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION 'UUID operator belum diisi pada 4.0a.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION 'Operator % tidak ditemukan pada AspNetUsers.', aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 4.1 Bekukan sasaran SEBELUM menulis — seluruh policy hidup yang masih menunjuk salah
-- satu dari enam identitas LAMA (bukan hanya yang sudah dilestarikan Tahap 1, supaya
-- policy lama yang sumbernya tidak dikenal Bagian 0 pun tetap tertutup rapat).
CREATE TEMP TABLE be_fin_042_tahap2_sasaran ON COMMIT DROP AS
SELECT p."Id" AS policy_id, i.resource, i.action
FROM public."SysAccessPolicy" p
JOIN be_fin_042_identitas i ON i.action_access_id = p."ActionAccessId"
WHERE i.resource IN (SELECT resource_lama FROM be_fin_042_peta)
  AND p."IsActive";

-- 4.2 Penonaktifan, dibatasi pada himpunan beku 4.1.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = false, "UpdateDateTime" = now(), "UpdateBy" = current_setting('be_fin_042.aktor', true)::uuid
FROM be_fin_042_tahap2_sasaran s
WHERE p."Id" = s.policy_id;

-- 4.3 PENEGASAN SESUDAH TULIS.
DO $$
DECLARE masih_aktif integer;
BEGIN
    SELECT count(*) INTO masih_aktif
      FROM public."SysAccessPolicy" p
      JOIN be_fin_042_tahap2_sasaran s ON s.policy_id = p."Id"
     WHERE p."IsActive";

    IF masih_aktif <> 0 THEN
        RAISE EXCEPTION 'BE-FIN-042 TAHAP 2 GAGAL: % sasaran masih aktif sesudah UPDATE.', masih_aktif;
    END IF;

    RAISE NOTICE 'Tahap 2 lulus: seluruh policy lama dinonaktifkan.';
END $$;

-- Ganti baris berikut dengan COMMIT; hanya bila parity Bagian 3 sudah terbukti.
ROLLBACK;

    ---------- BATAS TAHAP 2 ---------- */

-- =====================================================================================
-- BAGIAN 5 — ROLLBACK (hanya bila migrasi sudah COMMIT dan harus dibatalkan)
-- =====================================================================================

/*  ---------- ROLLBACK ----------

BEGIN;

-- 5.0a Parameter transaksi.
--      >>> GANTI UUID DI BAWAH DENGAN GUID OPERATOR / ADMIN PENGESAH MIGRASI <<<
SELECT set_config('be_fin_042.aktor', '00000000-0000-0000-0000-000000000000', true);

-- 5.0b GERBANG OPERATOR.
DO $$
DECLARE
    aktor_teks text := current_setting('be_fin_042.aktor', true);
    aktor      uuid;
    jml        integer;
BEGIN
    IF aktor_teks IS NULL OR aktor_teks = '' THEN
        RAISE EXCEPTION 'Parameter be_fin_042.aktor belum disetel pada 5.0a.';
    END IF;

    aktor := aktor_teks::uuid;

    IF aktor = '00000000-0000-0000-0000-000000000000'::uuid THEN
        RAISE EXCEPTION 'UUID operator belum diisi pada 5.0a.';
    END IF;

    SELECT count(*) INTO jml FROM public."AspNetUsers" WHERE "Id" = aktor;

    IF jml <> 1 THEN
        RAISE EXCEPTION 'Operator % tidak ditemukan pada AspNetUsers.', aktor;
    END IF;

    RAISE NOTICE 'Operator terverifikasi: %', aktor;
END $$;

-- 5.1 Aktifkan kembali policy identitas lama yang dinonaktifkan Tahap 2.
UPDATE public."SysAccessPolicy" p
SET "IsActive" = true, "UpdateDateTime" = now(), "UpdateBy" = current_setting('be_fin_042.aktor', true)::uuid
FROM be_fin_042_identitas i
WHERE p."ActionAccessId" = i.action_access_id
  AND i.resource IN (SELECT resource_lama FROM be_fin_042_peta)
  AND NOT p."IsActive" AND NOT p."IsDelete";

-- 5.2 Nonaktifkan seluruh baris yang dibuat Tahap 1 (dikenali dari identitas barunya).
UPDATE public."SysAccessPolicy" p
SET "IsActive" = false, "UpdateDateTime" = now(), "UpdateBy" = current_setting('be_fin_042.aktor', true)::uuid
FROM be_fin_042_identitas i
WHERE p."ActionAccessId" = i.action_access_id
  AND i.resource IN (SELECT resource_baru FROM be_fin_042_peta)
  AND p."IsActive";

-- Ganti dengan COMMIT hanya setelah keluaran Bagian 3 kembali ke angka sebelum migrasi.
ROLLBACK;

    ---------- BATAS ROLLBACK ---------- */
