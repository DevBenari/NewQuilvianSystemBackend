-- ==============================================================================
-- Quilvian Hospital System - Seeder Idempotent Master Kebijakan Deposit (UAT/Dev)
-- Modul: Rawat Inap / Billing Management
-- Target Tabel: public."MstDepositPolicy"
-- Skenario: Pasien Tunai / Umum Rawat Inap dengan Minimum Deposit Rp10.000.000
-- Reference: FIX-EPS-007-03 / UAT-ADM-003
-- ==============================================================================

-- Update kebijakan umum yang sudah ada di database (DEP-RANAP-UMUM)
UPDATE public."MstDepositPolicy"
SET "MinimumAmount" = 10000000.00,
    "FollowUpIntervalDays" = 3,
    "IsActive" = true,
    "Description" = 'Kebijakan deposit minimum Rp10.000.000 untuk pasien umum/tunai rawat inap (UAT).'
WHERE "Code" = 'DEP-RANAP-UMUM';

-- Jika belum ada baris sama sekali, sisipkan baris default DEP-POL-UMUM
INSERT INTO public."MstDepositPolicy" (
    "Id",
    "Code",
    "Name",
    "GuarantorId",
    "PatientClassId",
    "IsRequired",
    "MinimumAmount",
    "FollowUpIntervalDays",
    "Description",
    "EffectiveFrom",
    "EffectiveTo",
    "IsActive",
    "CreateDateTime",
    "CreateBy",
    "IsDelete",
    "IsCancel"
)
SELECT
    'd1e43e52-19e3-4d7a-8d19-bb193a778ef2',
    'DEP-POL-UMUM',
    'Kebijakan Deposit Pasien Tunai / Umum',
    NULL,
    NULL,
    true,
    10000000.00,
    3,
    'Kebijakan deposit minimum Rp10.000.000 untuk pasien umum/tunai rawat inap (UAT).',
    '2026-01-01 00:00:00+00',
    NULL,
    true,
    NOW(),
    '00000000-0000-0000-0000-000000000001',
    false,
    false
WHERE NOT EXISTS (
    SELECT 1 FROM public."MstDepositPolicy" WHERE "Code" IN ('DEP-RANAP-UMUM', 'DEP-POL-UMUM')
);

-- Verifikasi data yang tersimpan
SELECT "Id", "Code", "Name", "GuarantorId", "PatientClassId", "MinimumAmount", "FollowUpIntervalDays", "IsActive"
FROM public."MstDepositPolicy"
WHERE "Code" IN ('DEP-RANAP-UMUM', 'DEP-POL-UMUM');

