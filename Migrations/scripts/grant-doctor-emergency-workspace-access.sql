-- =====================================================================================
-- Hak akses Ruang Kerja Dokter IGD (Emergency Doctor Workspace)
--
-- Mengacu pada Matriks Hak Akses Modul IGD:
-- docs/module-blueprints/igd/contracts/permission-audit-matrix.md
-- Bagian 9 (Ruang Kerja Dokter IGD) — disetujui Rizki Gunawan lewat IGD-DEC-230
--
-- Memberikan hak akses kepada Departemen Medis + Jabatan Dokter IGD untuk:
-- 1. DoctorConsultation          : Read, Create, Update, Complete, Cancel, WriteSoap
-- 2. PatientAssessment            : Create, Update, Complete, Amend, Cancel (Read sudah ada)
-- 3. PatientDiagnosis             : Read, Create, Update, SetPrimary, Cancel
-- 4. PatientIntegratedProgressNote: Read, Create, Update
-- 5. Prescription                 : Read, Create, Update, Stop
-- 6. PatientProcedure             : Read, Create, Cancel
-- 7. EmergencyDoctorAssignment    : Read
--
-- SIFAT
--   Dibungkus transaksi, idempoten (NOT EXISTS atas kunci alami), tidak menghapus izin lain.
-- =====================================================================================

BEGIN;

CREATE TEMP TABLE _param_target_igd ON COMMIT DROP AS
SELECT
    '676f2aa7-8089-466b-b8a9-73adf5599626'::uuid AS department_id, -- Medis
    'ae5bb7af-9e65-63ed-c22b-57212203e592'::uuid AS position_id;   -- Dokter IGD

CREATE TEMP TABLE _aksi_dokter_igd ON COMMIT DROP AS
SELECT
    ca."Id" AS controller_id,
    aa."Id" AS action_id,
    ca."ControllerName",
    aa."ActionName"
FROM "SysControllerAccess" ca
JOIN "SysActionAccess" aa ON aa."ControllerAccessId" = ca."Id"
WHERE (
    (ca."ControllerName" = 'DoctorConsultation' AND aa."ActionName" IN ('Read', 'Create', 'Update', 'Complete', 'Cancel', 'WriteSoap')) OR
    (ca."ControllerName" = 'PatientAssessment' AND aa."ActionName" IN ('Read', 'Create', 'Update', 'Complete', 'Amend', 'Cancel')) OR
    (ca."ControllerName" = 'PatientDiagnosis' AND aa."ActionName" IN ('Read', 'Create', 'Update', 'SetPrimary', 'Cancel')) OR
    (ca."ControllerName" = 'PatientIntegratedProgressNote' AND aa."ActionName" IN ('Read', 'Create', 'Update')) OR
    (ca."ControllerName" = 'Prescription' AND aa."ActionName" IN ('Read', 'Create', 'Update', 'Stop')) OR
    (ca."ControllerName" = 'PatientProcedure' AND aa."ActionName" IN ('Read', 'Create', 'Cancel')) OR
    (ca."ControllerName" = 'EmergencyDoctorAssignment' AND aa."ActionName" IN ('Read'))
)
AND ca."IsActive" AND NOT ca."IsDelete"
AND aa."IsActive" AND NOT aa."IsDelete";

INSERT INTO "SysAccessPolicy" (
    "Id", "DepartmentId", "PositionId", "ControllerAccessId", "ActionAccessId",
    "IsAllowed", "IsActive", "IsDelete", "IsCancel", "CreateDateTime", "CreateBy",
    "UpdateBy", "DeleteBy", "CancelBy"
)
SELECT
    gen_random_uuid(),
    t.department_id,
    t.position_id,
    a.controller_id,
    a.action_id,
    true,
    true,
    false,
    false,
    now() AT TIME ZONE 'utc',
    (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid,
    '00000000-0000-0000-0000-000000000000'::uuid
FROM _param_target_igd t
CROSS JOIN _aksi_dokter_igd a
WHERE NOT EXISTS (
    SELECT 1 FROM "SysAccessPolicy" x
    WHERE x."DepartmentId" = t.department_id
      AND x."PositionId" = t.position_id
      AND x."ControllerAccessId" = a.controller_id
      AND x."ActionAccessId" = a.action_id
      AND NOT x."IsDelete"
);

COMMIT;
