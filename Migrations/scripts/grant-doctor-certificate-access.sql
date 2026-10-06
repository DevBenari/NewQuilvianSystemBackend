-- =====================================================================================
-- RJ-DOC-REV-BE-004/005 — Hak akses Surat Dokter (DoctorCertificate)
--
-- Controller DoctorCertificate baru (Read, Create, Update, Cancel). Skrip ini memberi
-- keempat aksi kepada pasangan Departemen + Jabatan yang SUDAH memegang aksi bernama sama
-- pada DoctorConsultation — yaitu jabatan yang memang menjalankan konsultasi dokter.
--
-- Izin melekat pada JABATAN, bukan orang. Setiap orang pada jabatan itu ikut mendapatkannya.
-- Jalankan setelah aplikasi pernah start sekali (AccessMenuSeeder membuat baris
-- SysControllerAccess/SysActionAccess untuk DoctorCertificate).
--
-- Idempoten: hanya menambah pasangan yang belum ada. Tidak ada UPDATE, DELETE, TRUNCATE.
-- Admin tetap dapat mencabutnya dari layar Pengaturan -> Manajemen Role -> Akses Role.
-- =====================================================================================

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "SysControllerAccess" WHERE "ControllerName" = 'DoctorCertificate') THEN
        RAISE EXCEPTION 'DoctorCertificate belum terdaftar di SysControllerAccess. Jalankan aplikasi sekali, lalu ulangi.';
    END IF;
END $$;

INSERT INTO "SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy",
    "IsCancel", "IsDelete")
SELECT
    gen_random_uuid(), src."DepartmentId", src."PositionId", tc."Id", ta."Id",
    true, true, CURRENT_TIMESTAMP,
    '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000',
    '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000',
    false, false
FROM "SysAccessPolicy" src
JOIN "SysActionAccess" sa ON sa."Id" = src."ActionAccessId"
JOIN "SysControllerAccess" sc ON sc."Id" = sa."ControllerAccessId" AND sc."ControllerName" = 'DoctorConsultation'
JOIN "SysControllerAccess" tc ON tc."ControllerName" = 'DoctorCertificate'
JOIN "SysActionAccess" ta ON ta."ControllerAccessId" = tc."Id" AND ta."ActionName" = sa."ActionName"
WHERE src."IsAllowed" AND src."IsActive" AND NOT src."IsDelete"
  AND sa."ActionName" IN ('Read', 'Create', 'Update', 'Cancel')
  AND NOT EXISTS (
      SELECT 1 FROM "SysAccessPolicy" x
      WHERE x."ActionAccessId" = ta."Id"
        AND x."DepartmentId" IS NOT DISTINCT FROM src."DepartmentId"
        AND x."PositionId" IS NOT DISTINCT FROM src."PositionId"
        AND NOT x."IsDelete");

COMMIT;

-- Pemeriksaan (hanya membaca)
SELECT a."ActionName", COUNT(*) FILTER (WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete") AS jabatan
FROM "SysControllerAccess" c
JOIN "SysActionAccess" a ON a."ControllerAccessId" = c."Id"
LEFT JOIN "SysAccessPolicy" p ON p."ActionAccessId" = a."Id"
WHERE c."ControllerName" = 'DoctorCertificate'
GROUP BY 1 ORDER BY 1;
