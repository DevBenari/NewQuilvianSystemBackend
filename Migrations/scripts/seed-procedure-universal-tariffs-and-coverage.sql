-- ==============================================================================
-- FIX-KEP-013-01: SEED MASTER TARIF UNIVERSAL & ATURAN COVERAGE TINDAKAN RAWAT INAP
-- Target Database: QuilvianNewDevHamzah / QuilvianNewDevMaster
-- Dibuat berdasarkan: PLAN-REPAIR-KEP-013 (ISSUE-KEP-013)
-- Tanggal: 2026-10-20
-- ==============================================================================

BEGIN;

-- 1. Pastikan Kategori Tarif Umum untuk Tindakan Medis Tersedia
DO $$
DECLARE
    v_category_id UUID;
BEGIN
    SELECT "Id" INTO v_category_id 
    FROM public."MstTariffCategory" 
    WHERE "IsProcedure" = true OR "TariffCategoryCode" = 'TC-PROC' OR "TariffCategoryName" ILIKE '%Tindakan%' 
    LIMIT 1;

    IF v_category_id IS NULL THEN
        SELECT "Id" INTO v_category_id FROM public."MstTariffCategory" WHERE "IsDelete" = false LIMIT 1;
    END IF;

    IF v_category_id IS NULL THEN
        v_category_id := gen_random_uuid();
        INSERT INTO public."MstTariffCategory" (
            "Id", "TariffCategoryCode", "TariffCategoryName", "TariffGroupName", 
            "IsRegistrationFee", "IsAdministrationFee", "IsConsultationFee", "IsRoomCharge",
            "IsProcedure", "IsLaboratory", "IsRadiology", "IsPharmacy", "IsSurgery", "IsPackage",
            "IsCoveredByInsuranceDefault", "SortOrder",
            "IsActive", "IsDelete", "IsCancel",
            "CreateBy", "CreateDateTime", "UpdateBy", "DeleteBy", "CancelBy"
        ) VALUES (
            v_category_id, 'TC-PROC', 'Tarif Tindakan Medis', 'Tindakan',
            false, false, false, false,
            true, false, false, false, false, false,
            true, 1,
            true, false, false,
            '00000000-0000-0000-0000-000000000000'::UUID, NOW(),
            '00000000-0000-0000-0000-000000000000'::UUID,
            '00000000-0000-0000-0000-000000000000'::UUID,
            '00000000-0000-0000-0000-000000000000'::UUID
        );
    END IF;
END $$;

-- 2. Tambahkan Master Tarif Universal (Berlaku Semua Kelas: PatientClassId = NULL) untuk Seluruh Tindakan Aktif
INSERT INTO public."MstTariff" (
    "Id",
    "TariffCode",
    "TariffName",
    "TariffCategoryId",
    "ProcedureId",
    "DrugId",
    "MedicalEquipmentId",
    "PatientClassId",
    "ServiceUnitId",
    "ClinicId",
    "SurgeryComponentType",
    "ChargeBasis",
    "ChargeRounding",
    "IsSurgeryRelated",
    "IsRoomCharge",
    "IsAdministrationFee",
    "IsRegistrationFee",
    "IsConsultationFee",
    "IsPackageTariff",
    "IsNeedDoctor",
    "IsNeedApproval",
    "IsTaxable",
    "SortOrder",
    "NormalPrice",
    "EffectiveStartDate",
    "EffectiveEndDate",
    "IsActive",
    "IsDelete",
    "IsCancel",
    "CreateBy",
    "CreateDateTime",
    "UpdateBy",
    "DeleteBy",
    "CancelBy"
)
SELECT 
    gen_random_uuid() AS "Id",
    'TRF-' || p."ProcedureCode" AS "TariffCode",
    'Tarif ' || p."ProcedureName" AS "TariffName",
    COALESCE(
        (SELECT "Id" FROM public."MstTariffCategory" WHERE "IsProcedure" = true OR "TariffCategoryCode" = 'TC-PROC' OR "TariffCategoryName" ILIKE '%Tindakan%' LIMIT 1),
        (SELECT "Id" FROM public."MstTariffCategory" WHERE "IsDelete" = false LIMIT 1)
    ) AS "TariffCategoryId",
    p."Id" AS "ProcedureId",
    NULL::UUID AS "DrugId",
    NULL::UUID AS "MedicalEquipmentId",
    NULL::UUID AS "PatientClassId",   -- NULL = Universal di seluruh kelas kamar rawat inap
    NULL::UUID AS "ServiceUnitId",    -- NULL = Universal di seluruh unit
    NULL::UUID AS "ClinicId",         -- NULL = Universal di seluruh klinik
    0 AS "SurgeryComponentType",
    1 AS "ChargeBasis",              -- PerService
    1 AS "ChargeRounding",
    false AS "IsSurgeryRelated",
    false AS "IsRoomCharge",
    false AS "IsAdministrationFee",
    false AS "IsRegistrationFee",
    false AS "IsConsultationFee",
    false AS "IsPackageTariff",
    true AS "IsNeedDoctor",
    false AS "IsNeedApproval",
    false AS "IsTaxable",
    1 AS "SortOrder",
    -- Skema tarif dummy proporsional berdasarkan kata kunci nama tindakan
    CASE 
        WHEN p."ProcedureName" ILIKE '%Defibrilasi%' OR p."ProcedureName" ILIKE '%Kardioversi%' THEN 350000.00
        WHEN p."ProcedureName" ILIKE '%Ekstraksi%' OR p."ProcedureName" ILIKE '%Bedah%' THEN 200000.00
        WHEN p."ProcedureName" ILIKE '%Jahitan%' OR p."ProcedureName" ILIKE '%Hecting%' THEN 100000.00
        WHEN p."ProcedureName" ILIKE '%Injeksi%' THEN 35000.00
        WHEN p."ProcedureName" ILIKE '%Kateter%' OR p."ProcedureName" ILIKE '%Infus%' THEN 50000.00
        ELSE 75000.00
    END AS "NormalPrice",
    '2026-01-01 00:00:00+00'::TIMESTAMPTZ AS "EffectiveStartDate",
    '2030-12-31 23:59:59+00'::TIMESTAMPTZ AS "EffectiveEndDate",
    true AS "IsActive",
    false AS "IsDelete",
    false AS "IsCancel",
    '00000000-0000-0000-0000-000000000000'::UUID AS "CreateBy",
    NOW() AS "CreateDateTime",
    '00000000-0000-0000-0000-000000000000'::UUID AS "UpdateBy",
    '00000000-0000-0000-0000-000000000000'::UUID AS "DeleteBy",
    '00000000-0000-0000-0000-000000000000'::UUID AS "CancelBy"
FROM public."MstProcedure" p
WHERE p."IsDelete" = false 
  AND p."IsActive" = true 
  AND p."IsLaboratory" = false 
  AND p."IsRadiology" = false
  -- Jangan duplikasi jika tarif universal untuk tindakan tersebut sudah ada
  AND NOT EXISTS (
      SELECT 1 FROM public."MstTariff" t 
      WHERE t."ProcedureId" = p."Id" 
        AND t."PatientClassId" IS NULL 
        AND t."IsDelete" = false
  );

-- 3. Tambahkan Aturan Tanggungan Universal untuk Penjamin Perusahaan (MstCompanyGuarantorCoverageRule)
-- Menanggung seluruh tindakan (ItemType = 'Procedure') sebesar 100%
INSERT INTO public."MstCompanyGuarantorCoverageRule" (
    "Id",
    "CompanyGuarantorId",
    "RuleCode",
    "RuleName",
    "ItemType",
    "ProcedureId",
    "CoverageStatus",
    "CoveragePercent",
    "IsNeedApproval",
    "IsNeedGuaranteeLetter",
    "IsAllowExcessPaymentByPatient",
    "Priority",
    "SortOrder",
    "IsActive",
    "IsDelete",
    "IsCancel",
    "CreateBy",
    "CreateDateTime",
    "UpdateBy",
    "DeleteBy",
    "CancelBy"
)
SELECT 
    gen_random_uuid(),
    cg."Id",
    'RULE-COMP-PROC-' || SUBSTRING(REPLACE(cg."CompanyGuarantorCode", '-', '') FROM 1 FOR 10),
    'Tanggungan Tindakan Medis ' || cg."CompanyGuarantorName",
    'Procedure',
    NULL::UUID, -- Menjamin seluruh tindakan
    'Covered',
    100.00,
    false,
    false,
    true,
    1,
    1,
    true,
    false,
    false,
    '00000000-0000-0000-0000-000000000000'::UUID,
    NOW(),
    '00000000-0000-0000-0000-000000000000'::UUID,
    '00000000-0000-0000-0000-000000000000'::UUID,
    '00000000-0000-0000-0000-000000000000'::UUID
FROM public."MstCompanyGuarantor" cg
WHERE cg."IsDelete" = false AND cg."IsActive" = true
  AND NOT EXISTS (
      SELECT 1 FROM public."MstCompanyGuarantorCoverageRule" r 
      WHERE r."CompanyGuarantorId" = cg."Id" 
        AND r."ItemType" = 'Procedure' 
        AND r."ProcedureId" IS NULL 
        AND r."IsDelete" = false
  );

-- 4. Tambahkan Aturan Tanggungan Universal untuk Asuransi / BPJS (MstInsuranceCoverageRule)
INSERT INTO public."MstInsuranceCoverageRule" (
    "Id",
    "InsuranceProviderId",
    "RuleCode",
    "RuleName",
    "ItemType",
    "ProcedureId",
    "CoverageStatus",
    "CoveragePercent",
    "IsNeedApproval",
    "IsNeedGuaranteeLetter",
    "IsAllowExcessPaymentByPatient",
    "Priority",
    "SortOrder",
    "IsActive",
    "IsDelete",
    "IsCancel",
    "CreateBy",
    "CreateDateTime",
    "UpdateBy",
    "DeleteBy",
    "CancelBy"
)
SELECT 
    gen_random_uuid(),
    ip."Id",
    'RULE-INS-PROC-' || SUBSTRING(REPLACE(ip."InsuranceProviderCode", '-', '') FROM 1 FOR 10),
    'Tanggungan Tindakan Medis ' || ip."InsuranceProviderName",
    'Procedure',
    NULL::UUID, -- Menjamin seluruh tindakan
    'Covered',
    100.00,
    false,
    false,
    true,
    1,
    1,
    true,
    false,
    false,
    '00000000-0000-0000-0000-000000000000'::UUID,
    NOW(),
    '00000000-0000-0000-0000-000000000000'::UUID,
    '00000000-0000-0000-0000-000000000000'::UUID,
    '00000000-0000-0000-0000-000000000000'::UUID
FROM public."MstInsuranceProvider" ip
WHERE ip."IsDelete" = false AND ip."IsActive" = true
  AND NOT EXISTS (
      SELECT 1 FROM public."MstInsuranceCoverageRule" r 
      WHERE r."InsuranceProviderId" = ip."Id" 
        AND r."ItemType" = 'Procedure' 
        AND r."ProcedureId" IS NULL 
        AND r."IsDelete" = false
  );

COMMIT;
