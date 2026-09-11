-- =====================================================================================
-- BE-SEC-003 â€” Diagnostik keadaan technical permission pilot Dokter Rawat Jalan
--
-- SKRIP INI HANYA MEMBACA. Tidak ada INSERT, UPDATE, DELETE, DDL, maupun perubahan
-- lain. Aman dijalankan pada database development kapan saja.
--
-- TUJUAN
--   Fase A BE-SEC-003 (pemecahan identitas di source) sudah ada di HEAD. Fase B
--   (perluasan SysAccessPolicy) BELUM dijalankan. Skrip ini mengukur keadaan
--   sebenarnya supaya Fase B tidak dijalankan di atas angka yang kedaluwarsa.
--
--   Baseline lama (2 September 2026) sudah tidak boleh dipakai sebagai target:
--   SysAccessPolicy total 498 / efektif 469, SysActionAccess aktif 1.076. Sejak itu
--   325 commit masuk beserta beberapa skrip grant milik tim lain.
--
-- CARA MENJALANKAN
--   psql -h <host> -p <port> -U <user> -d <database> -f diagnose-be-sec-003-permission-state.sql
--
--   Connection string TIDAK ditulis di sini dengan sengaja. Isikan sendiri saat
--   menjalankan; jangan menempelkannya ke dalam berkas ini.
--
-- CARA MEMBACA HASILNYA
--   Bagian Aâ€“C  : metrik dasar, dibandingkan dengan baseline lama.
--   Bagian D    : apakah dua identitas yang pensiun benar-benar sudah ditutup seeder.
--   Bagian E    : inventaris policy untuk seluruh identitas pilot.
--   Bagian F    : ringkasan mana yang sudah punya hak dan mana yang kosong.
--   Bagian G    : pengguna terdampak.
--
--   Identitas yang muncul di bagian E dengan nol baris policy berarti kemampuannya
--   ADA di registry tetapi BELUM diberikan kepada siapa pun â€” ditolak 403 untuk semua
--   orang kecuali SuperAdmin. Itulah yang Fase B perbaiki.
-- =====================================================================================

-- \echo ''
-- \echo '==== A. Total SysAccessPolicy ===='

SELECT count(*) AS total_policy_fisik
FROM public."SysAccessPolicy";

-- \echo ''
-- \echo '==== B. SysAccessPolicy efektif (IsAllowed AND IsActive AND NOT IsDelete) ===='

SELECT count(*) AS total_policy_efektif
FROM public."SysAccessPolicy"
WHERE "IsAllowed" AND "IsActive" AND NOT "IsDelete";

-- \echo ''
-- \echo '==== B.2 Pasangan Departemen x Posisi yang memegang izin efektif ===='

SELECT count(*) AS total_pasangan_departemen_posisi
FROM (
    SELECT DISTINCT p."DepartmentId", p."PositionId"
    FROM public."SysAccessPolicy" p
    WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
) AS pasangan;

-- \echo ''
-- \echo '==== C. SysActionAccess aktif ===='

SELECT count(*) AS total_action_access_aktif
FROM public."SysActionAccess"
WHERE "IsActive" AND NOT "IsDelete";

-- \echo ''
-- \echo '==== D. Identitas lama yang seharusnya sudah PENSIUN (ditutup seeder) ===='
-- \echo '     Harapan: IsActive = false DAN IsDelete = true untuk keduanya.'
-- \echo '     Bila masih aktif, berarti aplikasi belum pernah start di HEAD ini.'

SELECT
    c."ControllerName"                          AS resource,
    a."ActionName"                              AS action,
    a."Id"                                      AS action_access_id,
    c."Id"                                      AS controller_access_id,
    a."IsActive"                                AS action_is_active,
    a."IsDelete"                                AS action_is_delete,
    (
        SELECT count(*)
        FROM public."SysAccessPolicy" p
        WHERE p."ActionAccessId" = a."Id"
          AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
    )                                           AS policy_efektif_menggantung
FROM public."SysActionAccess" a
JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId"
WHERE (c."ControllerName" = 'PatientProcedure' AND a."ActionName" = 'Update')
   OR (c."ControllerName" = 'DoctorQueue'      AND a."ActionName" = 'Update')
ORDER BY c."ControllerName", a."ActionName";

-- \echo ''
-- \echo '==== E. Inventaris policy untuk seluruh identitas pilot BE-SEC-003 ===='
-- \echo '     Satu baris per (identitas x Departemen x Posisi).'
-- \echo '     Identitas tanpa baris policy muncul di bagian F sebagai "KOSONG".'

WITH identitas(resource, action) AS (
    VALUES
        ('PatientProcedure',    'Select'),
        ('PatientProcedure',    'Create'),
        ('PatientProcedure',    'Edit'),
        ('PatientProcedure',    'Approve'),
        ('PatientProcedure',    'Execute'),
        ('PatientProcedure',    'RemoveDraft'),
        ('PatientProcedure',    'Cancel'),

        ('DoctorQueue',         'Call'),
        ('DoctorQueue',         'StartConsultation'),
        ('DoctorQueue',         'FinishConsultation'),
        ('DoctorQueue',         'Skip'),
        ('DoctorQueue',         'NoShow'),
        ('DoctorQueue',         'Requeue'),

        ('DoctorConsultation',  'Update'),
        ('DoctorConsultation',  'WriteSoap'),
        ('DoctorConsultation',  'Complete'),
        ('DoctorConsultation',  'Cancel'),

        ('PatientAssessment',   'Update'),
        ('PatientAssessment',   'Complete'),
        ('PatientAssessment',   'Cancel'),
        ('PatientAssessment',   'Amend'),

        ('PatientDiagnosis',    'Update'),
        ('PatientDiagnosis',    'SetPrimary'),
        ('PatientDiagnosis',    'Resolve'),
        ('PatientDiagnosis',    'Cancel'),

        ('PatientVitalSign',    'Update'),
        ('PatientVitalSign',    'Verify'),
        ('PatientVitalSign',    'NotifyDoctor'),
        ('PatientVitalSign',    'Cancel')
)
SELECT
    i.resource                                  AS resource,
    i.action                                    AS action,
    c."Id"                                      AS controller_access_id,
    a."Id"                                      AS action_access_id,
    a."IsActive"                                AS action_is_active,
    a."IsDelete"                                AS action_is_delete,
    p."DepartmentId"                            AS department_id,
    d."DepartmentName"                          AS department_name,
    p."PositionId"                              AS position_id,
    pos."PositionName"                          AS position_name,
    p."IsAllowed"                               AS policy_is_allowed,
    p."IsActive"                                AS policy_is_active,
    p."IsDelete"                                AS policy_is_delete,
    (
        SELECT count(DISTINCT uo."UserId")
        FROM public."AspNetUserOrganization" uo
        WHERE uo."DepartmentId" = p."DepartmentId"
          AND uo."PositionId"   = p."PositionId"
          AND uo."IsActive"
          AND NOT uo."IsDelete"
          AND NOT uo."IsCancel"
          AND (uo."EffectiveStartDate" IS NULL OR uo."EffectiveStartDate" <= now())
          AND (uo."EffectiveEndDate"   IS NULL OR uo."EffectiveEndDate"   >= now())
    )                                           AS jumlah_pengguna_terdampak
FROM identitas i
LEFT JOIN public."SysControllerAccess" c
       ON c."ControllerName" = i.resource
LEFT JOIN public."SysActionAccess" a
       ON a."ControllerAccessId" = c."Id"
      AND a."ActionName" = i.action
LEFT JOIN public."SysAccessPolicy" p
       ON p."ActionAccessId" = a."Id"
      AND p."ControllerAccessId" = c."Id"
LEFT JOIN public."MstDepartment" d  ON d."Id"   = p."DepartmentId"
LEFT JOIN public."MstPosition"   pos ON pos."Id" = p."PositionId"
ORDER BY i.resource, i.action, d."DepartmentName", pos."PositionName";

-- \echo ''
-- \echo '==== F. Ringkasan per identitas â€” mana yang sudah punya hak, mana yang KOSONG ===='
-- \echo '     status TIDAK_TERDAFTAR : identitas tidak ada di registry (cek Fase A / seeder)'
-- \echo '     status KOSONG          : terdaftar tetapi nol policy efektif -> 403 untuk semua'
-- \echo '     status ADA             : sudah punya policy efektif'

WITH identitas(resource, action) AS (
    VALUES
        ('PatientProcedure',    'Select'),
        ('PatientProcedure',    'Create'),
        ('PatientProcedure',    'Edit'),
        ('PatientProcedure',    'Approve'),
        ('PatientProcedure',    'Execute'),
        ('PatientProcedure',    'RemoveDraft'),
        ('PatientProcedure',    'Cancel'),

        ('DoctorQueue',         'Call'),
        ('DoctorQueue',         'StartConsultation'),
        ('DoctorQueue',         'FinishConsultation'),
        ('DoctorQueue',         'Skip'),
        ('DoctorQueue',         'NoShow'),
        ('DoctorQueue',         'Requeue'),

        ('DoctorConsultation',  'Update'),
        ('DoctorConsultation',  'WriteSoap'),
        ('DoctorConsultation',  'Complete'),
        ('DoctorConsultation',  'Cancel'),

        ('PatientAssessment',   'Update'),
        ('PatientAssessment',   'Complete'),
        ('PatientAssessment',   'Cancel'),
        ('PatientAssessment',   'Amend'),

        ('PatientDiagnosis',    'Update'),
        ('PatientDiagnosis',    'SetPrimary'),
        ('PatientDiagnosis',    'Resolve'),
        ('PatientDiagnosis',    'Cancel'),

        ('PatientVitalSign',    'Update'),
        ('PatientVitalSign',    'Verify'),
        ('PatientVitalSign',    'NotifyDoctor'),
        ('PatientVitalSign',    'Cancel')
)
SELECT
    i.resource,
    i.action,
    CASE
        WHEN a."Id" IS NULL                       THEN 'TIDAK_TERDAFTAR'
        WHEN NOT a."IsActive" OR a."IsDelete"     THEN 'REGISTRY_DITUTUP'
        WHEN coalesce(pe.jumlah, 0) = 0           THEN 'KOSONG'
        ELSE 'ADA'
    END                                           AS status,
    coalesce(pe.jumlah, 0)                        AS policy_efektif,
    a."IsActive"                                  AS action_is_active,
    a."IsDelete"                                  AS action_is_delete
FROM identitas i
LEFT JOIN public."SysControllerAccess" c
       ON c."ControllerName" = i.resource
LEFT JOIN public."SysActionAccess" a
       ON a."ControllerAccessId" = c."Id"
      AND a."ActionName" = i.action
LEFT JOIN LATERAL (
    SELECT count(*) AS jumlah
    FROM public."SysAccessPolicy" p
    WHERE p."ActionAccessId" = a."Id"
      AND p."ControllerAccessId" = c."Id"
      AND p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
) pe ON true
ORDER BY i.resource, i.action;

-- \echo ''
-- \echo '==== G. Pengguna aktif pada pasangan Departemen x Posisi yang terdampak ===='
-- \echo '     Yaitu pasangan yang memegang salah satu identitas pilot, lama maupun baru.'

WITH pasangan_terdampak AS (
    SELECT DISTINCT p."DepartmentId", p."PositionId"
    FROM public."SysAccessPolicy" p
    JOIN public."SysActionAccess"     a ON a."Id" = p."ActionAccessId"
    JOIN public."SysControllerAccess" c ON c."Id" = a."ControllerAccessId"
    WHERE p."IsAllowed" AND p."IsActive" AND NOT p."IsDelete"
      AND c."ControllerName" IN (
            'PatientProcedure', 'DoctorQueue', 'DoctorConsultation',
            'PatientAssessment', 'PatientDiagnosis', 'PatientVitalSign')
)
SELECT
    d."DepartmentName"                  AS department_name,
    pos."PositionName"                  AS position_name,
    t."DepartmentId"                    AS department_id,
    t."PositionId"                      AS position_id,
    count(DISTINCT uo."UserId")         AS jumlah_pengguna_aktif
FROM pasangan_terdampak t
LEFT JOIN public."MstDepartment" d   ON d."Id"   = t."DepartmentId"
LEFT JOIN public."MstPosition"   pos ON pos."Id" = t."PositionId"
LEFT JOIN public."AspNetUserOrganization" uo
       ON uo."DepartmentId" = t."DepartmentId"
      AND uo."PositionId"   = t."PositionId"
      AND uo."IsActive"
      AND NOT uo."IsDelete"
      AND NOT uo."IsCancel"
      AND (uo."EffectiveStartDate" IS NULL OR uo."EffectiveStartDate" <= now())
      AND (uo."EffectiveEndDate"   IS NULL OR uo."EffectiveEndDate"   >= now())
GROUP BY d."DepartmentName", pos."PositionName", t."DepartmentId", t."PositionId"
ORDER BY d."DepartmentName", pos."PositionName";

-- \echo ''
-- \echo '==== SELESAI â€” tidak ada data yang diubah ===='
