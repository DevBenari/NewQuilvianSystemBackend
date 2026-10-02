-- =====================================================================================
-- GLOBAL REGISTRY DRIFT — ekspor keadaan registry database saat ini
--
-- SATU statement. SELECT saja. Tidak ada \echo, tidak ada perintah khusus psql,
-- tidak ada INSERT/UPDATE/DELETE/DDL. Aman dijalankan di DBeaver: sorot seluruh
-- berkas lalu Execute SQL Statement (Ctrl+Enter), atau Execute script (Alt+X).
--
-- TUJUAN
--   Menyediakan sisi DATABASE untuk dibandingkan dengan sisi SOURCE yang diturunkan
--   PermissionRegistryDescriptor.BuildFromAssembly. Perbandingannya menentukan apa saja
--   yang AKAN dilakukan AccessMenuSeeder ketika HEAD pertama kali start terhadap
--   database ini.
--
--   Yang paling penting adalah baris berkategori DB_ONLY_ACTIVE_WITH_EFFECTIVE_POLICY:
--   registry yang hidup, punya hak efektif, tetapi TIDAK lagi dideklarasikan source.
--   Seeder akan menutupnya, dan hak yang menunjuknya mati seketika.
--
-- CARA EKSPOR DARI DBEAVER
--   Klik kanan pada hasil -> Export resultset -> CSV, atau Advanced Copy -> Copy as
--   Markdown. Sertakan header kolom.
--
-- CATATAN BACA
--   registry_state  : keadaan baris registry itu sendiri
--   policy_efektif  : jumlah SysAccessPolicy hidup yang menunjuk baris ini
--   pasangan_dp     : jumlah pasangan Departemen x Posisi yang memegangnya
--   pengguna_aktif  : jumlah pengguna aktif pada pasangan-pasangan tersebut
--   risiko_penutupan: seberapa besar ruginya bila seeder menutup baris ini
-- =====================================================================================

SELECT
    m."ModuleCode"                                   AS module_code,
    m."ModuleName"                                   AS module_name,
    c."AreaName"                                     AS area_name,
    c."Id"                                           AS controller_access_id,
    c."ControllerName"                               AS resource,
    c."IsActive"                                     AS controller_is_active,
    c."IsDelete"                                     AS controller_is_delete,
    c."IsSystemOnly"                                 AS controller_is_system_only,
    a."Id"                                           AS action_access_id,
    a."ActionName"                                   AS action,
    a."DisplayName"                                  AS action_display_name,
    a."AccessType"                                   AS access_type,
    a."IsActive"                                     AS action_is_active,
    a."IsDelete"                                     AS action_is_delete,
    a."IsSystemOnly"                                 AS action_is_system_only,
    a."VisibleInRoleAccess"                          AS action_visible_in_role_access,

    -- Kunci runtime yang dicari HasAccessAsync. Inilah yang dibandingkan dengan source.
    c."ControllerName" || '.' || a."ActionName"      AS runtime_key,

    CASE
        WHEN c."IsDelete" OR NOT c."IsActive" THEN 'CONTROLLER_CLOSED'
        WHEN a."IsDelete" OR NOT a."IsActive" THEN 'ACTION_CLOSED'
        ELSE 'OPEN'
    END                                              AS registry_state,

    coalesce(pol.policy_efektif, 0)                  AS policy_efektif,
    coalesce(pol.policy_fisik, 0)                    AS policy_fisik,
    coalesce(pol.pasangan_dp, 0)                     AS pasangan_dp,
    coalesce(usr.pengguna_aktif, 0)                  AS pengguna_aktif,

    -- Seberapa mahal bila seeder menutup baris ini karena source tidak lagi
    -- mendeklarasikannya. Dibaca bersama hasil perbandingan sisi source.
    CASE
        WHEN (c."IsDelete" OR NOT c."IsActive" OR a."IsDelete" OR NOT a."IsActive")
            THEN 'SUDAH_TERTUTUP'
        WHEN coalesce(usr.pengguna_aktif, 0) > 0
            THEN 'TINGGI_ADA_PENGGUNA_AKTIF'
        WHEN coalesce(pol.policy_efektif, 0) > 0
            THEN 'SEDANG_ADA_HAK_TANPA_PENGGUNA'
        ELSE 'RENDAH_TANPA_HAK'
    END                                              AS risiko_penutupan,

    -- Daftar pemegangnya, supaya dampaknya terbaca tanpa query kedua.
    pol.pemegang                                     AS pemegang_departemen_posisi

FROM public."SysActionAccess" a
JOIN public."SysControllerAccess" c
      ON c."Id" = a."ControllerAccessId"
LEFT JOIN public."SysApplicationModule" m
      ON m."Id" = c."ModuleId"

LEFT JOIN LATERAL (
    SELECT
        count(*)                                                   AS policy_fisik,
        count(*) FILTER (
            WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
        )                                                          AS policy_efektif,
        count(DISTINCT (p."DepartmentId", p."PositionId")) FILTER (
            WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
        )                                                          AS pasangan_dp,
        string_agg(
            DISTINCT coalesce(d."DepartmentName", '?') || ' x ' || coalesce(po."PositionName", '?'),
            '; '
        ) FILTER (
            WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
        )                                                          AS pemegang
    FROM public."SysAccessPolicy" p
    LEFT JOIN public."MstDepartment" d  ON d."Id"  = p."DepartmentId"
    LEFT JOIN public."MstPosition"   po ON po."Id" = p."PositionId"
    WHERE p."ActionAccessId"     = a."Id"
      AND p."ControllerAccessId" = c."Id"
) pol ON true

LEFT JOIN LATERAL (
    SELECT count(DISTINCT uo."UserId") AS pengguna_aktif
    FROM public."SysAccessPolicy" p
    JOIN public."AspNetUserOrganization" uo
          ON uo."DepartmentId" = p."DepartmentId"
         AND uo."PositionId"   = p."PositionId"
    WHERE p."ActionAccessId"     = a."Id"
      AND p."ControllerAccessId" = c."Id"
      AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
      AND uo."IsActive" AND NOT uo."IsDelete" AND NOT uo."IsCancel"
      AND (uo."EffectiveStartDate" IS NULL OR uo."EffectiveStartDate" <= now())
      AND (uo."EffectiveEndDate"   IS NULL OR uo."EffectiveEndDate"   >= now())
) usr ON true

ORDER BY
    -- Yang paling berisiko ditutup muncul lebih dulu.
    CASE
        WHEN (c."IsDelete" OR NOT c."IsActive" OR a."IsDelete" OR NOT a."IsActive") THEN 3
        WHEN coalesce(usr.pengguna_aktif, 0) > 0 THEN 0
        WHEN coalesce(pol.policy_efektif, 0) > 0 THEN 1
        ELSE 2
    END,
    c."ControllerName",
    a."ActionName";
