-- =====================================================================================
-- Hak akses Permintaan Stok Barang/Obat untuk unit peminta dan gudang farmasi
--
-- Modul Farmasi -> Stock Request punya lima aksi yang terbagi menjadi dua peran yang
-- berbeda pekerjaannya:
--
--   SISI PEMINTA (depo yang membutuhkan barang)
--     Read    : melihat riwayat dan detail permintaan
--     Create  : membuat permintaan beserta itemnya
--     Update  : mengubah draft DAN mengirimnya ke gudang
--     Cancel  : membatalkan permintaan sebelum barang diserahkan
--
--   SISI GUDANG (yang menyerahkan)
--     Read    : melihat antrean dan detail permintaan
--     Fulfill : mencatat penyerahan barang, baris per baris
--
--   TIDAK ADA aksi menyetujui maupun menolak. Gudang tidak memutuskan permintaan depo;
--   ia melihat apa yang diminta, lalu mencatat berapa yang diserahkan saat depo mengambil.
--
--   PERSEDIAAN (controller DrugStock, dipakai layar Stok Farmasi dan Kartu Stok)
--     Read    : melihat saldo, batch, kedaluwarsa, dan kartu stok
--     Adjust  : saldo pembuka, penyesuaian, dan pemindahan status seperti karantina
--     Correct : mengoreksi baris kartu stok
--
--   Read persediaan aman diberikan luas karena hanya membaca. Adjust dan Correct sebaiknya
--   dibatasi: keduanya mengubah angka stok tanpa dokumen transaksi, dan itulah jalan yang
--   paling mudah dipakai menutupi selisih. Aturan bisnis menempatkan persetujuan koreksi
--   pada Kepala Farmasi atau Supervisor.
--
-- CARA KERJANYA, DAN KENAPA BUKAN PER ORANG
--   Izin di sistem ini melekat pada pasangan Departemen + Jabatan, bukan pada orang:
--
--     AspNetUserOrganization (UserId -> DepartmentId, PositionId)
--       dipasangkan dengan
--     SysAccessPolicy (DepartmentId, PositionId, ControllerAccessId, ActionAccessId)
--
--   KONSEKUENSI YANG HARUS DISADARI: skrip ini memberi izin kepada JABATANNYA, bukan
--   kepada orang tertentu. Setiap orang yang menempati jabatan itu akan ikut mendapatkan
--   izin ini, sekarang maupun kelak. Periksa dulu siapa saja yang menempatinya.
--
-- YANG HARUS ANDA ISI SENDIRI
--   Nama Departemen dan Jabatan di bawah ini adalah CONTOH, bukan ketetapan. Siapa yang
--   berhak meminta dan siapa yang berhak menyerahkan adalah keputusan instalasi farmasi,
--   bukan keputusan yang boleh disimpulkan dari kode. Ubah keempat nilai di blok
--   parameter sebelum menjalankan.
--
-- CATATAN PENTING TENTANG PEMISAHAN PERAN
--   Aksi Update mencakup "mengubah draft" sekaligus "mengirim ke gudang"; keduanya tidak
--   dapat dipisah tanpa mengubah backend. Bila rumah sakit menghendaki hanya kepala unit
--   yang boleh mengirim sementara stafnya hanya menyusun draft, itu memerlukan aksi baru
--   di controller — beri tahu supaya ditambahkan.
--
--   Sebaiknya sisi peminta dan sisi gudang TIDAK diberikan kepada jabatan yang sama.
--   Petugas yang menyusun permintaan lalu menutup sendiri permintaannya meniadakan
--   gunanya pencatatan penyerahan sebagai bukti serah terima antara dua pihak.
--
-- SIFAT
--   Dijalankan manual, dibungkus transaksi, dan aman diulang.
--   Tidak menghapus atau menonaktifkan izin apa pun yang sudah ada.
--
-- PRASYARAT
--   Backend harus SUDAH dijalankan sekali setelah perubahan aksi terakhir, supaya
--   seluruh aksi terdaftar di SysActionAccess. Skrip ini akan berhenti dengan pesan
--   jelas bila aksinya belum ada.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f grant-stock-request-access.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ------------------------------------------------------------------ parameter
-- UBAH EMPAT NILAI INI sesuai struktur organisasi Anda.

CREATE TEMP TABLE param ON COMMIT DROP AS
SELECT
    'Farmasi'::text          AS requester_department,
    'Apoteker'::text         AS requester_position,
    'Farmasi'::text          AS warehouse_department,
    'Kepala Instalasi'::text AS warehouse_position;

-- --------------------------------------------------------- pemeriksaan awal

DO $$
DECLARE
    v_controller uuid;
    v_missing text;
BEGIN
    SELECT c."Id" INTO v_controller
    FROM "SysControllerAccess" c
    WHERE c."ControllerName" = 'StockRequest' AND NOT c."IsDelete";

    IF v_controller IS NULL THEN
        RAISE EXCEPTION
            'Controller StockRequest belum terdaftar. Jalankan backend sekali lebih dahulu.';
    END IF;

    SELECT string_agg(x.nama, ', ') INTO v_missing
    FROM (VALUES ('Read'), ('Create'), ('Update'), ('Cancel'), ('Fulfill')) AS x(nama)
    WHERE NOT EXISTS (
        SELECT 1 FROM "SysActionAccess" a
        WHERE a."ControllerAccessId" = v_controller
          AND a."ActionName" = x.nama
          AND NOT a."IsDelete"
    );

    IF v_missing IS NOT NULL THEN
        RAISE EXCEPTION
            'Aksi berikut belum terdaftar: %. Restart backend supaya atributnya terbaca.',
            v_missing;
    END IF;
END $$;

-- Memastikan Departemen dan Jabatan yang disebut benar-benar ada, supaya kesalahan
-- ketik tidak berakhir sebagai skrip yang "berhasil" tanpa memberi izin apa pun.
DO $$
DECLARE
    p record;
    v_salah text := '';
BEGIN
    SELECT * INTO p FROM param;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.requester_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen peminta "%s" tidak ditemukan. ', p.requester_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.requester_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan peminta "%s" tidak ditemukan. ', p.requester_position);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.warehouse_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen gudang "%s" tidak ditemukan. ', p.warehouse_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.warehouse_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan gudang "%s" tidak ditemukan. ', p.warehouse_position);
    END IF;

    IF v_salah <> '' THEN
        RAISE EXCEPTION '%', v_salah;
    END IF;
END $$;

-- ------------------------------------------------------------- pemberian izin

WITH ctl AS (
    SELECT "Id" FROM "SysControllerAccess"
    WHERE "ControllerName" = 'StockRequest' AND NOT "IsDelete"
),
sasaran AS (
    -- Sisi peminta
    SELECT d."Id" AS dept, po."Id" AS pos, a."Id" AS act, a."ControllerAccessId" AS ctl_id
    FROM param p
    JOIN "MstDepartment" d ON d."DepartmentName" = p.requester_department AND NOT d."IsDelete"
    JOIN "MstPosition" po  ON po."PositionName" = p.requester_position AND NOT po."IsDelete"
    JOIN ctl ON TRUE
    JOIN "SysActionAccess" a ON a."ControllerAccessId" = ctl."Id" AND NOT a."IsDelete"
    WHERE a."ActionName" IN ('Read', 'Create', 'Update', 'Cancel')

    UNION

    -- Sisi gudang
    SELECT d."Id", po."Id", a."Id", a."ControllerAccessId"
    FROM param p
    JOIN "MstDepartment" d ON d."DepartmentName" = p.warehouse_department AND NOT d."IsDelete"
    JOIN "MstPosition" po  ON po."PositionName" = p.warehouse_position AND NOT po."IsDelete"
    JOIN ctl ON TRUE
    JOIN "SysActionAccess" a ON a."ControllerAccessId" = ctl."Id" AND NOT a."IsDelete"
    WHERE a."ActionName" IN ('Read', 'Fulfill')

    UNION

    -- Persediaan: membaca untuk kedua sisi, mengubah hanya untuk gudang.
    SELECT d."Id", po."Id", a."Id", a."ControllerAccessId"
    FROM param p
    JOIN "MstDepartment" d ON d."DepartmentName" IN (p.requester_department, p.warehouse_department)
                          AND NOT d."IsDelete"
    JOIN "MstPosition" po  ON po."PositionName" IN (p.requester_position, p.warehouse_position)
                          AND NOT po."IsDelete"
    JOIN "SysControllerAccess" sc ON sc."ControllerName" = 'DrugStock' AND NOT sc."IsDelete"
    JOIN "SysActionAccess" a ON a."ControllerAccessId" = sc."Id" AND NOT a."IsDelete"
    WHERE a."ActionName" = 'Read'

    UNION

    SELECT d."Id", po."Id", a."Id", a."ControllerAccessId"
    FROM param p
    JOIN "MstDepartment" d ON d."DepartmentName" = p.warehouse_department AND NOT d."IsDelete"
    JOIN "MstPosition" po  ON po."PositionName" = p.warehouse_position AND NOT po."IsDelete"
    JOIN "SysControllerAccess" sc ON sc."ControllerName" = 'DrugStock' AND NOT sc."IsDelete"
    JOIN "SysActionAccess" a ON a."ControllerAccessId" = sc."Id" AND NOT a."IsDelete"
    WHERE a."ActionName" IN ('Adjust', 'Correct')
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
-- Aman diulang: pasangan yang sudah punya izin dilewati, tidak digandakan.
WHERE NOT EXISTS (
    SELECT 1 FROM "SysAccessPolicy" x
    WHERE x."DepartmentId" = s.dept
      AND x."PositionId" = s.pos
      AND x."ActionAccessId" = s.act
      AND NOT x."IsDelete"
);

-- ------------------------------------------------------------------ hasilnya

SELECT
    c."ControllerName"  AS controller,
    d."DepartmentName" AS departemen,
    po."PositionName"  AS jabatan,
    a."ActionName"     AS aksi,
    p."IsAllowed"      AS diizinkan
FROM "SysAccessPolicy" p
JOIN "SysControllerAccess" c ON c."Id" = p."ControllerAccessId"
JOIN "SysActionAccess" a     ON a."Id" = p."ActionAccessId"
JOIN "MstDepartment" d       ON d."Id" = p."DepartmentId"
JOIN "MstPosition" po        ON po."Id" = p."PositionId"
WHERE c."ControllerName" IN ('StockRequest', 'DrugStock') AND NOT p."IsDelete"
ORDER BY c."ControllerName", d."DepartmentName", po."PositionName", a."SortOrder";

COMMIT;
