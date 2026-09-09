-- =====================================================================================
-- Hak akses integrasi Operasi → Farmasi
--
-- Skrip ini HANYA mencakup dua titik akses yang dibutuhkan integrasi persediaan:
--
--   OperatingRoomStockSource
--     Read     : melihat pemetaan kamar operasi → depo farmasi
--     Update   : menetapkan depo sumber stok sebuah kamar
--
--   OperatingRoomIntegration
--     Dispatch : membukukan pemakaian material operasi ke kartu stok Farmasi
--
-- YANG TIDAK DICAKUP
--   Controller Modul Operasi lainnya — kasus, jadwal, persiapan, pelaksanaan, material,
--   recovery, dan laporan — belum pernah memiliki skrip pemberian izin sama sekali.
--   Skrip ini tidak menutup kekurangan itu; ia hanya membuat integrasi persediaan dapat
--   dipakai jabatan non-superadmin. Modul Operasi masih perlu skrip izinnya sendiri.
--
-- SIAPA YANG SEHARUSNYA MEMEGANGNYA
--   Pemetaan depo adalah keputusan konfigurasi yang menentukan stok siapa yang berkurang.
--   Salah petakan berarti stok berkurang di depo yang keliru, dan itu baru terlihat saat
--   stok opname. Karena itu Update sebaiknya dipegang jabatan yang mengelola persediaan,
--   bukan setiap petugas kamar operasi.
--
--   Dispatch memotong stok. Ia diberikan kepada pelaksana kamar operasi karena merekalah
--   yang tahu kapan pemakaian sudah selesai dicatat.
--
-- SIFAT
--   Dijalankan manual, dibungkus transaksi, aman diulang, tidak menghapus izin yang ada.
--
-- PRASYARAT
--   Backend sudah dijalankan sekali setelah kedua controller ditambahkan.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f grant-operating-room-inventory-access.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ------------------------------------------------------------------ parameter
-- UBAH EMPAT NILAI INI sesuai struktur organisasi Anda.
--
--   pelaksana_* : petugas kamar operasi yang membukukan pemakaian
--   pengelola_* : pengelola persediaan yang menetapkan pemetaan depo

CREATE TEMP TABLE param ON COMMIT DROP AS
SELECT
    'Farmasi'::text                    AS pelaksana_department,
    'Tenaga Teknis Kefarmasian'::text  AS pelaksana_position,
    'Farmasi'::text                    AS pengelola_department,
    'Apoteker'::text                   AS pengelola_position;

-- --------------------------------------------------------- pemeriksaan awal

DO $$
DECLARE
    v_missing text;
BEGIN
    SELECT string_agg(format('%s.%s', x.controller, x.aksi), ', ' ORDER BY x.controller, x.aksi)
    INTO v_missing
    FROM (VALUES
        ('OperatingRoomStockSource', 'Read'),
        ('OperatingRoomStockSource', 'Update'),
        ('OperatingRoomIntegration', 'Dispatch')
    ) AS x(controller, aksi)
    WHERE NOT EXISTS (
        SELECT 1
        FROM "SysControllerAccess" c
        JOIN "SysActionAccess" a ON a."ControllerAccessId" = c."Id" AND NOT a."IsDelete"
        WHERE c."ControllerName" = x.controller AND NOT c."IsDelete"
          AND a."ActionName" = x.aksi
    );

    IF v_missing IS NOT NULL THEN
        RAISE EXCEPTION
            'Aksi berikut belum terdaftar: %. Jalankan backend sekali supaya atributnya terbaca.',
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
                   WHERE "DepartmentName" = p.pelaksana_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen pelaksana "%s" tidak ditemukan. ', p.pelaksana_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.pelaksana_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan pelaksana "%s" tidak ditemukan. ', p.pelaksana_position);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.pengelola_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen pengelola "%s" tidak ditemukan. ', p.pengelola_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.pengelola_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan pengelola "%s" tidak ditemukan. ', p.pengelola_position);
    END IF;

    IF v_salah <> '' THEN
        RAISE EXCEPTION '%', v_salah;
    END IF;
END $$;

-- ------------------------------------------------------------- pemberian izin

WITH rencana(peran, controller, aksi) AS (
    VALUES
        -- Pelaksana membukukan, dan perlu melihat pemetaan untuk tahu depo mana yang dipakai.
        ('pelaksana', 'OperatingRoomStockSource', 'Read'),
        ('pelaksana', 'OperatingRoomIntegration', 'Dispatch'),

        -- Pengelola persediaan yang menetapkan pemetaannya.
        ('pengelola', 'OperatingRoomStockSource', 'Read'),
        ('pengelola', 'OperatingRoomStockSource', 'Update'),
        ('pengelola', 'OperatingRoomIntegration', 'Dispatch')
),
sasaran AS (
    SELECT d."Id" AS dept, po."Id" AS pos, c."Id" AS ctl_id, a."Id" AS act
    FROM rencana r
    CROSS JOIN param p
    JOIN "MstDepartment" d
      ON d."DepartmentName" = CASE r.peran WHEN 'pelaksana' THEN p.pelaksana_department
                                           ELSE p.pengelola_department END
     AND NOT d."IsDelete"
    JOIN "MstPosition" po
      ON po."PositionName" = CASE r.peran WHEN 'pelaksana' THEN p.pelaksana_position
                                          ELSE p.pengelola_position END
     AND NOT po."IsDelete"
    JOIN "SysControllerAccess" c
      ON c."ControllerName" = r.controller AND NOT c."IsDelete"
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

-- ------------------------------------------------------------------ hasilnya

SELECT
    c."ControllerName" AS controller,
    d."DepartmentName" AS departemen,
    po."PositionName"  AS jabatan,
    a."ActionName"     AS aksi,
    p."IsAllowed"      AS diizinkan
FROM "SysAccessPolicy" p
JOIN "SysControllerAccess" c ON c."Id" = p."ControllerAccessId"
JOIN "SysActionAccess" a     ON a."Id" = p."ActionAccessId"
JOIN "MstDepartment" d       ON d."Id" = p."DepartmentId"
JOIN "MstPosition" po        ON po."Id" = p."PositionId"
WHERE c."ControllerName" IN ('OperatingRoomStockSource', 'OperatingRoomIntegration')
  AND NOT p."IsDelete"
ORDER BY c."ControllerName", d."DepartmentName", po."PositionName", a."SortOrder";

COMMIT;
