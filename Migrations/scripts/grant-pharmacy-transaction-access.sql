-- =====================================================================================
-- Hak akses transaksi Farmasi: Transfer Antar Lokasi, Pemakaian Obat, Retur Obat,
-- dan Etiket Obat
--
-- Skrip ini melengkapi `grant-stock-request-access.sql`, yang hanya mencakup controller
-- StockRequest dan DrugStock. Empat controller di bawah ini dibuat setelahnya dan belum
-- pernah diberi izin, sehingga jabatan non-superadmin tidak akan dapat membukanya.
--
-- APA YANG DIBERIKAN, DAN KENAPA DIBAGI BEGITU
--
--   TransferStok (controller StockTransfer)
--     Read     : melihat daftar dan detail transfer
--     Create   : menyusun draft transfer
--     Update   : mengubah draft dan mengajukannya
--     Approve  : menyetujui/menolak — di sinilah stok direservasi dan batch dialokasikan
--     Issue    : mengeluarkan barang dari lokasi asal (stok asal berkurang)
--     Receive  : menerima di lokasi tujuan (stok tujuan bertambah)
--     Cancel   : membatalkan sebelum barang keluar
--
--     Approve sengaja dipisahkan dari Create/Update. Yang mengajukan transfer tidak
--     seharusnya menyetujui pengajuannya sendiri; persetujuan itulah yang mengunci stok.
--
--     Receive diberikan kepada KEDUA sisi. Lokasi tujuan sebuah transfer bisa depo mana
--     pun di antara empat depo yang punya saldo mandiri (PHA-OQ-019), sehingga izin
--     menerima tidak dapat disempitkan hanya ke satu jabatan tanpa memisahkannya per
--     lokasi — dan sistem izin ini bekerja per Departemen + Jabatan, bukan per lokasi.
--     Bila pemisahan per depo memang dikehendaki, itu perubahan backend, bukan skrip.
--
--   PemakaianObat (controller DrugUsage)
--     Read     : melihat riwayat pemakaian pasien
--     Create   : menyusun draft pemakaian
--     Update   : mengubah draft
--     Record   : mencatat pemakaian — DI SINI stok berkurang dan status menjadi NOT BILLED
--     Cancel   : membatalkan
--
--     Record adalah aksi yang menyentuh stok sekaligus membuka tagihan. Ia diberikan
--     kepada sisi depo karena depo yang menyerahkan obat kepada pasien.
--
--   ReturObat (controller DrugReturn)
--     Read     : melihat daftar dan detail retur
--     Create   : mengajukan retur dari unit
--     Update   : mengubah draft dan mengirimkannya
--     Verify   : memeriksa fisik obat lalu menerima/menolak per baris — DI SINI stok
--                bertambah, dan hanya sebesar jumlah yang benar-benar diterima
--     Cancel   : membatalkan pengajuan
--
--     Verify DIPISAHKAN dari Create/Update dengan sengaja. Unit yang mengembalikan obat
--     tidak boleh sekaligus menyatakan obat itu layak kembali ke stok; pemeriksaan fisik
--     oleh apoteker adalah satu-satunya hal yang menghalangi obat rusak masuk lagi ke
--     stok yang dilayankan.
--
--   EtiketObat (controller PrescriptionLabel)
--     Read     : melihat dan mencetak etiket
--
--     Hanya membaca, tidak mengubah apa pun, jadi aman diberikan kepada kedua sisi.
--
-- CARA KERJANYA
--   Sama seperti skrip stock request: izin melekat pada pasangan Departemen + Jabatan
--   (SysAccessPolicy), bukan pada orang. Setiap orang yang menempati jabatan itu ikut
--   mendapatkannya, sekarang maupun kelak. Periksa dulu siapa saja yang menempatinya.
--
-- YANG HARUS ANDA ISI SENDIRI
--   Nama Departemen dan Jabatan di blok parameter adalah CONTOH. Siapa yang berhak
--   menyetujui transfer dan siapa yang berhak memverifikasi retur adalah keputusan
--   instalasi farmasi, bukan sesuatu yang boleh disimpulkan dari kode.
--
-- SIFAT
--   Dijalankan manual, dibungkus transaksi, aman diulang, dan tidak menghapus atau
--   menonaktifkan izin apa pun yang sudah ada.
--
-- PRASYARAT
--   Backend harus SUDAH dijalankan sekali setelah keempat controller ini ditambahkan,
--   supaya aksinya terdaftar di SysActionAccess. Skrip berhenti dengan pesan jelas bila
--   ada yang belum terdaftar.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f grant-pharmacy-transaction-access.sql
-- =====================================================================================

\set ON_ERROR_STOP on

BEGIN;

-- ------------------------------------------------------------------ parameter
-- UBAH EMPAT NILAI INI sesuai struktur organisasi Anda.
--
--   depo_*   : pelaksana harian — menyusun transfer, mencatat pemakaian, mengajukan retur
--   kepala_* : penyetuju — menyetujui transfer dan memverifikasi retur
--
-- Nilai bawaan di bawah memakai jabatan yang benar-benar ada pada master saat skrip ini
-- ditulis. Pembagiannya mengikuti keputusan bisnis: retur diverifikasi apoteker, sehingga
-- verifikasi tidak diberikan kepada jabatan yang mengajukan returnya.

CREATE TEMP TABLE param ON COMMIT DROP AS
SELECT
    'Farmasi'::text                     AS depo_department,
    'Tenaga Teknis Kefarmasian'::text   AS depo_position,
    'Farmasi'::text                     AS kepala_department,
    'Apoteker'::text                    AS kepala_position;

-- --------------------------------------------------------- pemeriksaan awal

DO $$
DECLARE
    v_missing text;
BEGIN
    SELECT string_agg(format('%s.%s', x.controller, x.aksi), ', ' ORDER BY x.controller, x.aksi)
    INTO v_missing
    FROM (VALUES
        ('StockTransfer', 'Read'), ('StockTransfer', 'Create'), ('StockTransfer', 'Update'),
        ('StockTransfer', 'Approve'), ('StockTransfer', 'Issue'), ('StockTransfer', 'Receive'),
        ('StockTransfer', 'Cancel'),
        ('DrugUsage', 'Read'), ('DrugUsage', 'Create'), ('DrugUsage', 'Update'),
        ('DrugUsage', 'Record'), ('DrugUsage', 'Cancel'),
        ('DrugReturn', 'Read'), ('DrugReturn', 'Create'), ('DrugReturn', 'Update'),
        ('DrugReturn', 'Verify'), ('DrugReturn', 'Cancel'),
        ('PrescriptionLabel', 'Read')
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

-- Memastikan Departemen dan Jabatan yang disebut benar-benar ada, supaya kesalahan ketik
-- tidak berakhir sebagai skrip yang "berhasil" tanpa memberi izin apa pun.
DO $$
DECLARE
    p record;
    v_salah text := '';
BEGIN
    SELECT * INTO p FROM param;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.depo_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen depo "%s" tidak ditemukan. ', p.depo_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.depo_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan depo "%s" tidak ditemukan. ', p.depo_position);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstDepartment"
                   WHERE "DepartmentName" = p.kepala_department AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Departemen kepala "%s" tidak ditemukan. ', p.kepala_department);
    END IF;

    IF NOT EXISTS (SELECT 1 FROM "MstPosition"
                   WHERE "PositionName" = p.kepala_position AND NOT "IsDelete") THEN
        v_salah := v_salah || format('Jabatan kepala "%s" tidak ditemukan. ', p.kepala_position);
    END IF;

    IF v_salah <> '' THEN
        RAISE EXCEPTION '%', v_salah;
    END IF;
END $$;

-- ------------------------------------------------------------- pemberian izin
--
-- Setiap baris `rencana` menyebut satu (jabatan, controller, aksi). Menuliskannya sebagai
-- data, bukan sebagai rangkaian UNION, membuat pembagian perannya dapat dibaca sekali
-- lihat dan diubah tanpa menyentuh SQL-nya.

WITH rencana(peran, controller, aksi) AS (
    VALUES
        -- Transfer: depo menyusun dan menjalankan, kepala menyetujui.
        ('depo',   'StockTransfer', 'Read'),
        ('depo',   'StockTransfer', 'Create'),
        ('depo',   'StockTransfer', 'Update'),
        ('depo',   'StockTransfer', 'Issue'),
        ('depo',   'StockTransfer', 'Receive'),
        ('depo',   'StockTransfer', 'Cancel'),
        ('kepala', 'StockTransfer', 'Read'),
        ('kepala', 'StockTransfer', 'Approve'),
        ('kepala', 'StockTransfer', 'Receive'),
        ('kepala', 'StockTransfer', 'Cancel'),

        -- Pemakaian obat pasien: seluruhnya di depo yang menyerahkan.
        ('depo',   'DrugUsage', 'Read'),
        ('depo',   'DrugUsage', 'Create'),
        ('depo',   'DrugUsage', 'Update'),
        ('depo',   'DrugUsage', 'Record'),
        ('depo',   'DrugUsage', 'Cancel'),
        ('kepala', 'DrugUsage', 'Read'),

        -- Retur: unit mengajukan, kepala memeriksa fisik dan memutuskan.
        ('depo',   'DrugReturn', 'Read'),
        ('depo',   'DrugReturn', 'Create'),
        ('depo',   'DrugReturn', 'Update'),
        ('depo',   'DrugReturn', 'Cancel'),
        ('kepala', 'DrugReturn', 'Read'),
        ('kepala', 'DrugReturn', 'Verify'),

        -- Etiket: hanya membaca.
        ('depo',   'PrescriptionLabel', 'Read'),
        ('kepala', 'PrescriptionLabel', 'Read')
),
sasaran AS (
    SELECT
        d."Id" AS dept,
        po."Id" AS pos,
        c."Id" AS ctl_id,
        a."Id" AS act
    FROM rencana r
    CROSS JOIN param p
    JOIN "MstDepartment" d
      ON d."DepartmentName" = CASE r.peran WHEN 'depo' THEN p.depo_department
                                           ELSE p.kepala_department END
     AND NOT d."IsDelete"
    JOIN "MstPosition" po
      ON po."PositionName" = CASE r.peran WHEN 'depo' THEN p.depo_position
                                          ELSE p.kepala_position END
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
WHERE c."ControllerName" IN ('StockTransfer', 'DrugUsage', 'DrugReturn', 'PrescriptionLabel')
  AND NOT p."IsDelete"
ORDER BY c."ControllerName", d."DepartmentName", po."PositionName", a."SortOrder";

COMMIT;
