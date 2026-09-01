-- =====================================================================================
-- Hak akses modul Operasi untuk dokter dan perawat
--
-- Memberi izin modul Operasi kepada Departemen dan Jabatan yang ditempati dr. Rendy
-- Pangalila sebagai dokter, dan Vina Aprilianti sebagai perawat.
--
-- CARA KERJANYA, DAN KENAPA BUKAN PER ORANG
--   Izin di sistem ini melekat pada pasangan Departemen + Jabatan, bukan pada orang:
--
--     AspNetUserOrganization (UserId -> DepartmentId, PositionId)
--       dipasangkan dengan
--     SysAccessPolicy (DepartmentId, PositionId, ControllerAccessId, ActionAccessId)
--
--   KONSEKUENSI YANG HARUS DISADARI: skrip ini memberi izin kepada JABATANNYA, bukan
--   kepada Rendy dan Vina sebagai pribadi. Setiap orang lain yang menempati jabatan yang
--   sama akan ikut mendapatkan izin ini, sekarang maupun kelak. Itu memang perilaku yang
--   diinginkan agar tidak perlu mengatur izin satu per satu, tetapi periksa dulu siapa
--   saja yang menempati jabatan itu sebelum menjalankan skrip ini.
--
-- BATAS IZIN
--   Dokter  : seluruh aksi modul Operasi.
--   Perawat : membaca kasus, mengisi persiapan dan checklist, mencatat material dan
--             recovery. TIDAK diberi: membuat, mengubah, dan membatalkan permintaan
--             operasi, serta mengubah jadwal.
--
--   Pembagian ini SEMENTARA dan disusun dari alur yang ada di kode, bukan dari ketetapan
--   rumah sakit. Batas sesungguhnya milik pemilik proses kamar operasi dan perlu ditinjau.
--
--   Aturan klinis tetap berlaku terpisah dan tidak dilonggarkan skrip ini: hanya dokter
--   bedah utama yang boleh memulai operasi, dan sign-off tetap hanya diterima dari
--   pemegang peran itu di tim.
--
-- SIFAT
--   Dijalankan manual, dibungkus transaksi, dan aman diulang.
--   Tidak menghapus atau menonaktifkan izin apa pun yang sudah ada.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <nama_database> -f grant-operating-room-access.sql
-- =====================================================================================

BEGIN;

-- ---------------------------------------------------------------- prasyarat
DO $$
DECLARE
    jml_aksi integer;
BEGIN
    SELECT COUNT(*) INTO jml_aksi
    FROM "SysActionAccess" aa
    JOIN "SysControllerAccess" ca ON ca."Id" = aa."ControllerAccessId"
    WHERE ca."ControllerName" LIKE 'OperatingRoom%'
      AND ca."IsActive" AND NOT ca."IsDelete"
      AND aa."IsActive" AND NOT aa."IsDelete";

    IF jml_aksi = 0 THEN
        RAISE EXCEPTION
            'Controller modul Operasi belum terdaftar di SysControllerAccess. Jalankan aplikasi satu kali agar AccessMenuSeeder mendaftarkannya, lalu ulangi skrip ini.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM "AspNetUsers" u
        JOIN "AspNetUserOrganization" o ON o."UserId" = u."Id"
             AND o."IsActive" AND NOT o."IsDelete"
        WHERE u."UserName" = 'rendi@admin.com') THEN
        RAISE EXCEPTION 'Akun rendi@admin.com tidak punya penempatan Departemen/Jabatan yang aktif.';
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM "AspNetUsers" u
        JOIN "AspNetUserOrganization" o ON o."UserId" = u."Id"
             AND o."IsActive" AND NOT o."IsDelete"
        WHERE u."UserName" = 'vina.aprilianti@rsmmc.local') THEN
        RAISE EXCEPTION 'Akun vina.aprilianti@rsmmc.local tidak punya penempatan Departemen/Jabatan yang aktif.';
    END IF;
END $$;

-- ---------------------------------------------------------------- aksi yang diberikan
-- Daftar aksi modul Operasi yang hidup, dipisah menurut siapa yang boleh memakainya.
CREATE TEMPORARY TABLE _aksi ON COMMIT DROP AS
SELECT
    ca."Id"            AS controller_id,
    aa."Id"            AS action_id,
    ca."ControllerName",
    aa."ActionName",
    -- Perawat tidak diberi kendali atas permintaan dan jadwal operasi. Keduanya
    -- kewenangan dokter, dan memberikannya kepada perawat mengaburkan siapa yang
    -- bertanggung jawab atas keputusan operasi.
    (ca."ControllerName" NOT IN ('OperatingRoomCase', 'OperatingRoomSchedule')
     OR aa."ActionName" = 'Read')                       AS boleh_perawat
FROM "SysControllerAccess" ca
JOIN "SysActionAccess" aa ON aa."ControllerAccessId" = ca."Id"
WHERE ca."ControllerName" LIKE 'OperatingRoom%'
  AND ca."IsActive" AND NOT ca."IsDelete" AND NOT ca."IsSystemOnly"
  AND aa."IsActive" AND NOT aa."IsDelete" AND NOT aa."IsSystemOnly";

-- ---------------------------------------------------------------- penempatan sasaran
CREATE TEMPORARY TABLE _penempatan ON COMMIT DROP AS
SELECT DISTINCT
    o."DepartmentId",
    o."PositionId",
    CASE WHEN u."UserName" = 'rendi@admin.com' THEN 'dokter' ELSE 'perawat' END AS peran
FROM "AspNetUsers" u
JOIN "AspNetUserOrganization" o ON o."UserId" = u."Id"
     AND o."IsActive" AND NOT o."IsDelete"
WHERE u."UserName" IN ('rendi@admin.com', 'vina.aprilianti@rsmmc.local');

-- ---------------------------------------------------------------- pemberian izin
INSERT INTO "SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT
    gen_random_uuid(), p."DepartmentId", p."PositionId", a.controller_id, a.action_id,
    true, true, now() AT TIME ZONE 'utc',
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
    false, false
FROM _penempatan p
JOIN _aksi a ON (p.peran = 'dokter' OR a.boleh_perawat)
WHERE NOT EXISTS (
    SELECT 1 FROM "SysAccessPolicy" x
    WHERE x."DepartmentId"       = p."DepartmentId"
      AND x."PositionId"         = p."PositionId"
      AND x."ControllerAccessId" = a.controller_id
      AND x."ActionAccessId"     = a.action_id
      AND NOT x."IsDelete");

-- =====================================================================================
-- Penautan akun ke data tenaga
--
-- Izin saja belum cukup. Modul Operasi membaca dua tautan pada akun untuk hal berbeda:
--
--   DoctorId           dipakai saat MEMBUAT permintaan operasi (klaim doctor_id)
--   WorkforceProfileId dipakai setiap tindakan klinis untuk memeriksa keanggotaan tim
--
-- Keduanya diisi dari data tenaga yang SUDAH ADA, dicocokkan dengan nama pada akun.
-- Bila tidak ada yang cocok, kolomnya dibiarkan kosong dan ringkasan di bawah akan
-- menyebutkannya — data tenaga orang sungguhan tidak dikarang oleh skrip ini.
--
-- Kolom yang sudah terisi tidak pernah ditimpa.
-- =====================================================================================

-- Rendy Pangalila sebagai dokter.
UPDATE "AspNetUsers" u
SET "DoctorId" = d."Id"
FROM "MstDoctor" d
WHERE u."UserName" = 'rendi@admin.com'
  AND u."DoctorId" IS NULL
  AND d."IsActive" AND NOT d."IsDelete"
  AND (d."FullName" ILIKE '%rend%pangalila%');

-- Profil tenaga dokter diambil dari baris dokter yang baru saja ditautkan, supaya
-- keduanya pasti menunjuk orang yang sama.
UPDATE "AspNetUsers" u
SET "WorkforceProfileId" = d."WorkforceProfileId"
FROM "MstDoctor" d
WHERE u."UserName" = 'rendi@admin.com'
  AND u."WorkforceProfileId" IS NULL
  AND d."Id" = u."DoctorId"
  AND d."WorkforceProfileId" IS NOT NULL;

-- Vina Aprilianti sebagai perawat. Profil tenaganya diambil lewat baris pegawainya.
UPDATE "AspNetUsers" u
SET "WorkforceProfileId" = e."WorkforceProfileId"
FROM "MstEmployee" e
WHERE u."UserName" = 'vina.aprilianti@rsmmc.local'
  AND u."WorkforceProfileId" IS NULL
  AND e."IsActive" AND NOT e."IsDelete"
  AND e."FullName" ILIKE '%vina%aprilianti%';

-- Bila baris pegawainya tidak ketemu, coba langsung ke profil tenaga.
UPDATE "AspNetUsers" u
SET "WorkforceProfileId" = w."Id"
FROM "MstWorkforceProfile" w
WHERE u."UserName" = 'vina.aprilianti@rsmmc.local'
  AND u."WorkforceProfileId" IS NULL
  AND w."IsActive" AND NOT w."IsDelete"
  AND w."DisplayName" ILIKE '%vina%aprilianti%';

COMMIT;

-- =====================================================================================
-- Ringkasan
-- =====================================================================================

SELECT
    u."DisplayName",
    u."UserType",
    CASE WHEN u."DoctorId" IS NULL
         THEN 'BELUM tertaut dokter, tidak dapat membuat permintaan operasi'
         ELSE 'tertaut dokter' END                                   AS status_dokter,
    CASE WHEN u."WorkforceProfileId" IS NULL
         THEN 'BELUM tertaut tenaga, tidak dapat memberi sign-off'
         ELSE 'tertaut tenaga' END                                   AS status_tenaga,
    COUNT(DISTINCT p."Id")                                           AS izin_operasi
FROM "AspNetUsers" u
JOIN "AspNetUserOrganization" o ON o."UserId" = u."Id"
     AND o."IsActive" AND NOT o."IsDelete"
LEFT JOIN "SysAccessPolicy" p ON p."DepartmentId" = o."DepartmentId"
     AND p."PositionId" = o."PositionId"
     AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
LEFT JOIN "SysControllerAccess" ca ON ca."Id" = p."ControllerAccessId"
     AND ca."ControllerName" LIKE 'OperatingRoom%'
WHERE u."UserName" IN ('rendi@admin.com', 'vina.aprilianti@rsmmc.local')
  AND (p."Id" IS NULL OR ca."Id" IS NOT NULL)
GROUP BY u."Id", u."DisplayName", u."UserType", u."DoctorId", u."WorkforceProfileId";

-- Kandidat penautan, hanya ditampilkan bila masih ada yang belum tertaut. Pakai daftar
-- ini untuk menautkan secara manual bila pencocokan nama di atas tidak menemukan apa pun,
-- misalnya karena ejaan namanya berbeda:
--
--   UPDATE "AspNetUsers" SET "DoctorId" = '<Id dari daftar>'
--   WHERE "UserName" = 'rendi@admin.com';
--
--   UPDATE "AspNetUsers" SET "WorkforceProfileId" = '<Id dari daftar>'
--   WHERE "UserName" = 'vina.aprilianti@rsmmc.local';

SELECT 'MstDoctor untuk Rendi' AS kandidat, d."Id"::text, d."FullName", d."DoctorCode"
FROM "MstDoctor" d
WHERE d."IsActive" AND NOT d."IsDelete"
  AND (d."FullName" ILIKE '%rend%' OR d."FullName" ILIKE '%pangalila%')
  AND EXISTS (SELECT 1 FROM "AspNetUsers" u
              WHERE u."UserName" = 'rendi@admin.com' AND u."DoctorId" IS NULL)

UNION ALL

SELECT 'MstWorkforceProfile untuk Vina', w."Id"::text, w."DisplayName", w."ProfileCode"
FROM "MstWorkforceProfile" w
WHERE w."IsActive" AND NOT w."IsDelete"
  AND (w."DisplayName" ILIKE '%vina%' OR w."DisplayName" ILIKE '%aprilianti%')
  AND EXISTS (SELECT 1 FROM "AspNetUsers" u
              WHERE u."UserName" = 'vina.aprilianti@rsmmc.local'
                AND u."WorkforceProfileId" IS NULL);
