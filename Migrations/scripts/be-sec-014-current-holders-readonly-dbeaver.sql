-- =====================================================================================
-- BE-SEC-014 — Pembacaan keadaan saat ini. SELURUHNYA BACA-SAJA.
--
-- STATUS BERKAS INI: siap dijalankan operator. Tidak ada satu pun perintah tulis.
--   Tidak ada INSERT, UPDATE, DELETE, BEGIN, maupun COMMIT di dalam berkas ini.
--   Aman dijalankan kapan saja, termasuk saat aplikasi sedang melayani traffic.
--
-- TUJUAN
--   1. Memastikan siapa pemegang hak enam resource HR hari ini — TANPA menebak.
--   2. Mendaftar posisi pada Departemen Human Resource supaya pemilik sistem dapat
--      menyatakan secara eksplisit posisi mana yang dihitung sebagai "staff".
--   3. Mengukur registry sebelum dan sesudah AccessMenuSeeder.
--
-- KENAPA BERKAS INI ADA
--   Skrip pemberian hak (BE-SEC-014 bagian 4) menolak menebak. Ia menuntut daftar posisi
--   staff yang disetujui pemilik. Bagian 2 di bawah adalah satu-satunya sumber sah daftar
--   itu: nama posisi yang benar-benar ada di database, bukan yang diasumsikan.
--
-- CARA MEMBACA "EFEKTIF"
--   Sebuah policy benar-benar memberi hak hanya bila SELURUH rantainya hidup. Predikat
--   di bawah disalin dari AccessPermissionService.HasAccessAsync supaya hasil query ini
--   tidak berbeda dari keputusan runtime:
--
--     policy.IsAllowed AND policy.IsActive AND NOT policy.IsDelete
--     AND policy.ControllerAccessId = action.ControllerAccessId
--     AND action.IsActive AND NOT action.IsDelete AND NOT action.IsSystemOnly
--     AND controller.IsActive AND NOT controller.IsDelete AND NOT controller.IsSystemOnly
--
--   Policy yang menunjuk baris registry TERTUTUP tetap ada dan flag-nya sendiri masih
--   hidup — yang mati adalah registry-nya. Itulah "hak tertidur". Bagian 1.3 sengaja
--   dibuat untuk menemukannya, dan karena itu TIDAK menyaring flag action.
-- =====================================================================================

-- =====================================================================================
-- BAGIAN 1 — Pemegang hak enam resource HR
-- =====================================================================================

-- 1.1 Registry: apakah resource dan action-nya sudah terdaftar, dan apakah hidup.
--     Jalankan SEBELUM dan SESUDAH AccessMenuSeeder. Sebelum seeder,
--     WorkScheduleAssignment TIDAK akan muncul sama sekali — itu memang diharapkan.
SELECT
    m."ModuleCode",
    c."ControllerName"                             AS resource,
    a."ActionName"                                 AS action,
    a."AccessType",
    a."IsActive"                                   AS action_aktif,
    a."IsDelete"                                   AS action_dihapus,
    c."IsActive"                                   AS resource_aktif,
    c."IsDelete"                                   AS resource_dihapus,
    CASE
        WHEN a."IsActive" AND NOT a."IsDelete" THEN 'HIDUP'
        ELSE 'TERTUTUP'
    END                                            AS status_registry
FROM public."SysControllerAccess" c
JOIN public."SysApplicationModule" m ON m."Id" = c."ModuleId"
LEFT JOIN public."SysActionAccess" a ON a."ControllerAccessId" = c."Id"
WHERE c."ControllerName" IN (
        'WorkSchedule','Shift','ShiftGroup','ShiftPattern','WorkCalendar','WorkScheduleAssignment')
ORDER BY c."ControllerName", a."ActionName";

-- 1.2 Pemegang EFEKTIF hari ini — hak yang benar-benar dapat dipakai.
SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName"  AS resource,
    a."ActionName"      AS action
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE c."ControllerName" IN (
        'WorkSchedule','Shift','ShiftGroup','ShiftPattern','WorkCalendar','WorkScheduleAssignment')
  AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete"
  AND pol."ControllerAccessId" = a."ControllerAccessId"
  AND a."IsActive" AND NOT a."IsDelete" AND NOT a."IsSystemOnly"
  AND c."IsActive" AND NOT c."IsDelete" AND NOT c."IsSystemOnly"
ORDER BY d."DepartmentName", p2."PositionName", c."ControllerName", a."ActionName";

-- 1.3 HAK TERTIDUR — policy hidup yang menunjuk baris registry TERTUTUP.
--     Sengaja TIDAK menyaring flag action/controller: justru itu yang dicari.
--     Inilah baris yang akan hidup kembali sendiri begitu seeder mengaktifkan
--     ulang identitasnya. Lihat evidence/13.
SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName"  AS resource,
    a."ActionName"      AS action,
    a."IsActive"        AS action_aktif,
    a."IsDelete"        AS action_dihapus,
    pol."Id"            AS policy_id
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE c."ControllerName" IN (
        'WorkSchedule','Shift','ShiftGroup','ShiftPattern','WorkCalendar','WorkScheduleAssignment')
  AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete"
  AND (NOT a."IsActive" OR a."IsDelete" OR NOT c."IsActive" OR c."IsDelete")
ORDER BY d."DepartmentName", p2."PositionName", c."ControllerName", a."ActionName";

-- 1.4 Ringkasan per pasangan Departemen x Posisi.
SELECT
    d."DepartmentName",
    p2."PositionName",
    count(*) FILTER (WHERE a."IsActive" AND NOT a."IsDelete")            AS hak_efektif,
    count(*) FILTER (WHERE NOT a."IsActive" OR a."IsDelete")             AS hak_tertidur,
    string_agg(DISTINCT c."ControllerName" || '.' || a."ActionName", ', '
               ORDER BY c."ControllerName" || '.' || a."ActionName")     AS rincian
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE c."ControllerName" IN (
        'WorkSchedule','Shift','ShiftGroup','ShiftPattern','WorkCalendar','WorkScheduleAssignment')
  AND pol."IsAllowed" AND pol."IsActive" AND NOT pol."IsDelete"
GROUP BY d."DepartmentName", p2."PositionName"
ORDER BY d."DepartmentName", p2."PositionName";

-- =====================================================================================
-- BAGIAN 2 — Posisi pada Departemen Human Resource
--
-- KELUARAN BAGIAN INI WAJIB DITINJAU PEMILIK SISTEM.
--
-- Skrip pemberian hak tidak menebak posisi mana yang "staff". Pemilik memilih dari
-- daftar di bawah, lalu menuliskan nama-nama yang disetujui ke dalam bagian 2 skrip
-- be-sec-014-post-seeder-hr-initial-grants.sql.
--
-- Matriks yang sudah disetujui:
--   Manajer HR      -> Read, Create, Update, Delete
--   Posisi staff HR -> Read, Create saja
-- =====================================================================================

-- 2.1 Seluruh posisi pada Departemen Human Resource, beserta jumlah penggunanya.
SELECT
    d."DepartmentName",
    p2."PositionName",
    p2."PositionCode",
    p2."IsActive"                                     AS posisi_aktif,
    count(DISTINCT o."UserId") FILTER (
        WHERE o."IsActive" AND NOT o."IsDelete" AND NOT o."IsCancel"
    )                                                 AS pengguna_aktif
FROM public."MstPosition" p2
JOIN public."MstDepartment" d ON d."Id" = p2."DepartmentId"
LEFT JOIN public."AspNetUserOrganization" o
       ON o."DepartmentId" = d."Id" AND o."PositionId" = p2."Id"
WHERE d."DepartmentName" ILIKE '%human resource%'
  AND NOT d."IsDelete"
  AND NOT p2."IsDelete"
GROUP BY d."DepartmentName", p2."PositionName", p2."PositionCode", p2."IsActive"
ORDER BY p2."PositionName";

-- 2.2 Nama departemen yang menyerupai "Human Resource" dan "Finance".
--     Dijalankan untuk memastikan nama bisnis yang dipakai skrip lain benar-benar cocok
--     satu baris, bukan nol dan bukan banyak.
SELECT "Id", "DepartmentCode", "DepartmentName", "IsActive", "IsDelete"
FROM public."MstDepartment"
WHERE "DepartmentName" ILIKE '%human resource%'
   OR "DepartmentName" ILIKE '%finance%'
ORDER BY "DepartmentName";

-- 2.3 Posisi manajer pada kedua departemen itu.
SELECT d."DepartmentName", p2."PositionName", p2."PositionCode", p2."IsActive", p2."IsDelete"
FROM public."MstPosition" p2
JOIN public."MstDepartment" d ON d."Id" = p2."DepartmentId"
WHERE (d."DepartmentName" ILIKE '%human resource%' OR d."DepartmentName" ILIKE '%finance%')
  AND p2."PositionName" ILIKE '%manajer%'
ORDER BY d."DepartmentName", p2."PositionName";

-- =====================================================================================
-- BAGIAN 3 — Pengukuran registry kanonik
--
-- Baseline lama 1286 / 339 / 48 sudah TIDAK BERLAKU sejak BE-SEC-012 dan BE-SEC-013.
-- Baseline baru yang diharapkan SESUDAH seeder: 1300 action / 340 resource / 48 modul.
-- =====================================================================================

-- 3.1 Hitungan aktif. Bandingkan dengan baseline baru.
SELECT
    (SELECT count(*) FROM public."SysActionAccess"     WHERE "IsActive" AND NOT "IsDelete") AS action_aktif,
    (SELECT count(*) FROM public."SysControllerAccess" WHERE "IsActive" AND NOT "IsDelete") AS resource_aktif,
    (SELECT count(*) FROM public."SysApplicationModule" WHERE "IsActive" AND NOT "IsDelete") AS modul_aktif,
    (SELECT count(*) FROM public."SysAccessPolicy")                                          AS policy_fisik,
    (SELECT count(*) FROM public."SysAccessPolicy"
      WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete")                                   AS policy_hidup;

-- 3.2 Perbandingan eksplisit terhadap baseline baru. Ketiganya wajib 'cocok'
--     SESUDAH seeder dijalankan.
SELECT
    'action'   AS metrik,
    (SELECT count(*) FROM public."SysActionAccess" WHERE "IsActive" AND NOT "IsDelete") AS terukur,
    1300 AS diharapkan,
    CASE WHEN (SELECT count(*) FROM public."SysActionAccess"
                WHERE "IsActive" AND NOT "IsDelete") = 1300 THEN 'cocok' ELSE 'BEDA' END AS status
UNION ALL
SELECT
    'resource',
    (SELECT count(*) FROM public."SysControllerAccess" WHERE "IsActive" AND NOT "IsDelete"),
    340,
    CASE WHEN (SELECT count(*) FROM public."SysControllerAccess"
                WHERE "IsActive" AND NOT "IsDelete") = 340 THEN 'cocok' ELSE 'BEDA' END
UNION ALL
SELECT
    'modul',
    (SELECT count(*) FROM public."SysApplicationModule" WHERE "IsActive" AND NOT "IsDelete"),
    48,
    CASE WHEN (SELECT count(*) FROM public."SysApplicationModule"
                WHERE "IsActive" AND NOT "IsDelete") = 48 THEN 'cocok' ELSE 'BEDA' END;

-- =====================================================================================
-- BAGIAN 4 — Gerbang pra-seeder untuk skrip pencabutan Finance
--
-- Jalankan tepat sebelum be-sec-014-pre-seeder-revoke-finance-workschedule.sql.
-- Hasilnya WAJIB tepat 2 baris. Bila bukan 2, JANGAN jalankan skrip pencabutan itu —
-- skripnya memang akan membatalkan diri sendiri, tetapi lebih baik diketahui lebih dulu.
-- =====================================================================================

SELECT
    d."DepartmentName",
    p2."PositionName",
    c."ControllerName" AS resource,
    a."ActionName"     AS action,
    pol."IsActive"     AS policy_aktif,
    a."IsActive"       AS action_aktif,
    a."IsDelete"       AS action_dihapus
FROM public."SysAccessPolicy" pol
JOIN public."MstDepartment"        d  ON d."Id"  = pol."DepartmentId"
JOIN public."MstPosition"          p2 ON p2."Id" = pol."PositionId"
JOIN public."SysActionAccess"      a  ON a."Id"  = pol."ActionAccessId"
JOIN public."SysControllerAccess"  c  ON c."Id"  = a."ControllerAccessId"
WHERE d."DepartmentName" = 'Finance'
  AND p2."PositionName"  = 'Manajer Finance'
  AND c."ControllerName" = 'WorkSchedule'
  AND a."ActionName" IN ('Update','Delete')
  AND pol."IsActive" AND NOT pol."IsDelete"
ORDER BY a."ActionName";
