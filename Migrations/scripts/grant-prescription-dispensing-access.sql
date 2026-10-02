-- =====================================================================================
-- Hak akses Penyerahan Obat Resep (Prescription Dispensing)
--
-- Controller `PrescriptionDispensing` punya empat aksi, dan pembagiannya mengikuti dua
-- langkah alurnya:
--
--   Read     : melihat rekapitulasi dan histori penyerahan satu resep
--   Prepare  : menyiapkan penyerahan dan MENAHAN stok — saldo belum berkurang
--   Dispense : menyerahkan obat; DI SINI saldo fisik berkurang tepat satu kali
--   Cancel   : membatalkan penyiapan dan melepas tahanan stoknya
--
-- KENAPA PREPARE DAN DISPENSE DIPISAHKAN
--   Keduanya berbeda akibatnya. Prepare hanya menahan: obat belum berpindah, tetapi sudah
--   tidak dapat dijanjikan kepada pasien lain. Dispense yang benar-benar mengurangi saldo
--   dan menyatakan obat sudah sampai ke pasien — dan angka itulah yang kelak dibaca copy
--   resep sebagai bukti berapa yang sudah diserahkan.
--
--   Karena Dispense yang menentukan isi dokumen yang dibawa pasien, ia diberikan kepada
--   apoteker pada contoh di bawah, sementara penyiapannya kepada tenaga teknis kefarmasian.
--
-- CANCEL DIBERIKAN KEPADA KEDUANYA
--   Yang menyiapkan harus dapat membatalkan penyiapannya sendiri ketika pasien menunda atau
--   membatalkan pengambilan; menahannya sampai apoteker sempat menangani hanya mengunci stok
--   tanpa guna. Pembatalan hanya berlaku selama obat belum diserahkan — sesudahnya
--   pengembalian melewati Retur Obat, yang wajib diperiksa apoteker.
--
-- YANG TIDAK DICAKUP
--   Controller `PrescriptionCopy` sengaja TIDAK diberi izin di sini. Format legal dokumennya
--   belum diputuskan (`PHA-DEC-061`, BUSINESS DECISION REQUIRED) dan layarnya belum dibangun;
--   memberi izin atas dokumen yang belum final hanya membuka jalan bagi lembar setengah jadi
--   untuk terbit. Buat skripnya tersendiri setelah formatnya ditetapkan.
--
-- YANG HARUS ANDA ISI SENDIRI
--   Nama Departemen dan Jabatan di bawah adalah CONTOH. Siapa yang berhak menyerahkan obat
--   kepada pasien adalah keputusan instalasi farmasi beserta ketentuan yang berlaku padanya,
--   bukan sesuatu yang boleh disimpulkan dari kode.
--
-- SIFAT
--   Dijalankan manual, dibungkus transaksi, aman diulang, tidak menghapus izin yang ada.
--
-- PRASYARAT
--   Backend sudah dijalankan sekali setelah controller ini ditambahkan.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f grant-prescription-dispensing-access.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ------------------------------------------------------------------ parameter

CREATE TEMP TABLE param ON COMMIT DROP AS
SELECT
    'Farmasi'::text                    AS penyiap_department,
    'Tenaga Teknis Kefarmasian'::text  AS penyiap_position,
    'Farmasi'::text                    AS penyerah_department,
    'Apoteker'::text                   AS penyerah_position;

-- --------------------------------------------------------- pemeriksaan awal

DO $$
DECLARE
    v_missing text;
BEGIN
    SELECT string_agg(x.aksi, ', ' ORDER BY x.aksi) INTO v_missing
    FROM (VALUES ('Read'), ('Prepare'), ('Dispense'), ('Cancel')) AS x(aksi)
    WHERE NOT EXISTS (
        SELECT 1
        FROM "SysControllerAccess" c
        JOIN "SysActionAccess" a ON a."ControllerAccessId" = c."Id" AND NOT a."IsDelete"
        WHERE c."ControllerName" = 'PrescriptionDispensing' AND NOT c."IsDelete"
          AND a."ActionName" = x.aksi
    );

    IF v_missing IS NOT NULL THEN
        RAISE EXCEPTION
            'Aksi PrescriptionDispensing berikut belum terdaftar: %. Jalankan backend sekali lebih dahulu.',
            v_missing;
    END IF;
END $$;

DO $$
DECLARE
    p record;
    v_salah text := '';
BEGIN
    SELECT * INTO p FROM param;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.penyiap_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen penyiap "%s" tidak ditemukan. ', p.penyiap_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.penyiap_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan penyiap "%s" tidak ditemukan. ', p.penyiap_position);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.penyerah_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen penyerah "%s" tidak ditemukan. ', p.penyerah_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.penyerah_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan penyerah "%s" tidak ditemukan. ', p.penyerah_position);
    END IF;

    IF v_salah <> '' THEN
        RAISE EXCEPTION '%', v_salah;
    END IF;
END $$;

-- ------------------------------------------------------------- pemberian izin

WITH rencana(peran, aksi) AS (
    VALUES
        ('penyiap',  'Read'),
        ('penyiap',  'Prepare'),
        ('penyiap',  'Cancel'),
        ('penyerah', 'Read'),
        ('penyerah', 'Dispense'),
        ('penyerah', 'Cancel')
),
sasaran AS (
    SELECT d."Id" AS dept, po."Id" AS pos, c."Id" AS ctl_id, a."Id" AS act
    FROM rencana r
    CROSS JOIN param p
    JOIN "MstDepartment" d
      ON d."DepartmentName" = CASE r.peran WHEN 'penyiap' THEN p.penyiap_department
                                           ELSE p.penyerah_department END
     AND NOT d."IsDelete"
    JOIN "MstPosition" po
      ON po."PositionName" = CASE r.peran WHEN 'penyiap' THEN p.penyiap_position
                                          ELSE p.penyerah_position END
     AND NOT po."IsDelete"
    JOIN "SysControllerAccess" c
      ON c."ControllerName" = 'PrescriptionDispensing' AND NOT c."IsDelete"
    JOIN "SysActionAccess" a
      ON a."ControllerAccessId" = c."Id" AND a."ActionName" = r.aksi AND NOT a."IsDelete"
)
INSERT INTO "SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
    "IsCancel", "IsDelete"
)
SELECT
    gen_random_uuid(), s.dept, s.pos, s.ctl_id, s.act,
    TRUE, TRUE,
    now(), '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    FALSE, FALSE
FROM sasaran s
WHERE NOT EXISTS (
    SELECT 1 FROM "SysAccessPolicy" x
    WHERE x."DepartmentId" = s.dept
      AND x."PositionId" = s.pos
      AND x."ActionAccessId" = s.act
      AND NOT x."IsDelete"
);

COMMIT;

-- ------------------------------------------------------------------ hasilnya

SELECT
    d."DepartmentName" AS departemen,
    po."PositionName"  AS jabatan,
    a."ActionName"     AS aksi,
    p."IsAllowed"      AS diizinkan
FROM "SysAccessPolicy" p
JOIN "SysControllerAccess" c ON c."Id" = p."ControllerAccessId"
JOIN "SysActionAccess" a     ON a."Id" = p."ActionAccessId"
JOIN "MstDepartment" d       ON d."Id" = p."DepartmentId"
JOIN "MstPosition" po        ON po."Id" = p."PositionId"
WHERE c."ControllerName" = 'PrescriptionDispensing' AND NOT p."IsDelete"
ORDER BY po."PositionName", a."SortOrder";
