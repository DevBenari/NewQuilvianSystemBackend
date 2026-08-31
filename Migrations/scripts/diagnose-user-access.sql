-- Diagnosa mengapa sebuah akun ditolak dengan
-- "Anda tidak memiliki akses ke menu atau fitur ini."
--
-- Sifat: HANYA MEMBACA.
--
-- AccessPermissionService.HasAccessAsync meloloskan pengguna tanpa syarat bila
-- keduanya benar:
--   1. barisnya ditemukan dengan IsActive = true, DAN
--   2. ia dikenali sebagai super admin, yaitu punya role SuperAdmin ATAU UserType = 1
-- selama Security:Authorization:EnforceClinicalPolicyForSuperAdmin bernilai false.
--
-- Bila salah satu tidak terpenuhi, pemeriksaan berlanjut ke pemetaan role dan
-- permission, dan akun tanpa pemetaan itu akan ditolak.

SELECT
    u."UserName",
    u."IsActive",
    u."UserType",
    CASE u."UserType" WHEN 1 THEN 'SuperAdmin' WHEN 2 THEN 'Employee'
         WHEN 3 THEN 'PermanentDoctor' WHEN 4 THEN 'GuestDoctor'
         WHEN 5 THEN 'ExternalUser' ELSE 'lainnya' END           AS arti_user_type,
    COALESCE(string_agg(r."Name", ', '), '(tidak punya role)')   AS daftar_role,
    CASE
        WHEN NOT u."IsActive" THEN 'DITOLAK - IsActive false'
        WHEN u."UserType" = 1 THEN 'LOLOS - UserType SuperAdmin'
        WHEN bool_or(r."NormalizedName" = 'SUPERADMIN') THEN 'LOLOS - punya role SuperAdmin'
        ELSE 'DITOLAK - bukan super admin, perlu pemetaan role dan permission'
    END                                                          AS kesimpulan,
    u."WorkforceProfileId",
    u."DoctorId"
FROM "AspNetUsers" u
LEFT JOIN "AspNetUserRoles" ur ON ur."UserId" = u."Id"
LEFT JOIN "AspNetRoles" r      ON r."Id" = ur."RoleId"
WHERE u."NormalizedUserName" IN ('SUPERADMIN', 'OPR.ANESTESI', 'OPR.PERAWAT')
GROUP BY u."Id", u."UserName", u."IsActive", u."UserType",
         u."WorkforceProfileId", u."DoctorId"
ORDER BY u."UserName";
