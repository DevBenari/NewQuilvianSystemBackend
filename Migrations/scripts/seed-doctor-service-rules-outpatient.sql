-- =====================================================================================
-- RJ-DOC-REV-BE-005 — Aturan layanan dokter rawat jalan (MstDoctorServiceRule)
--
-- TUJUAN
--   Setiap dokter AKTIF yang punya jadwal praktik mendapat satu aturan layanan KONSULTASI
--   untuk setiap pasangan (Unit Layanan, Klinik) pada jadwalnya. Aturan ini yang dibaca
--   Billing (BillingSourceTariffResolver, urutan 1b) untuk menemukan tarif jasa konsultasi.
--
-- RELASI YANG DIISI
--   MstDoctor         <- DoctorId           dokter dari jadwal
--   MstServiceUnit    <- ServiceUnitId      unit dari jadwal
--   MstClinic         <- ClinicId           klinik dari jadwal
--   MstProcedure      <- ProcedureId        tindakan konsultasi bertarif konsultasi:
--                                             Poli Umum / Medical Check Up -> POLI_22377
--                                               "Jasa Konsultasi Medik Umum"
--                                             klinik lain                  -> POLI_26530
--                                               "Jasa Konsultasi Medik Spesialis"
--   MstTariffCategory <- TariffCategoryId   CONSULTATION
--   MstTariff         <- TariffId           SENGAJA KOSONG. Tarif dipilih per kelas pasien
--                                           dari MstTariff ber-IsConsultationFee milik
--                                           tindakan di atas (19 kelas sudah tersedia).
--                                           Mengisi TariffId akan memaksa satu harga untuk
--                                           semua kelas.
--   MstPatientClass   <- PatientClassId     SENGAJA KOSONG, berlaku untuk semua kelas.
--
-- IDEMPOTEN
--   Baris hanya dibuat bila belum ada aturan konsultasi hidup untuk (dokter, unit, klinik).
--   Aman dijalankan berulang. Tidak ada UPDATE, DELETE, maupun TRUNCATE.
--
-- KODE
--   RuleCode = 'DSR-RJ-' || DoctorCode || '-' || ClinicCode (dipotong 50 karakter).
--   Berbeda awalan dari kode layar master ('DSR-RSMMC-') sehingga tidak berbenturan.
-- =====================================================================================

BEGIN;

DO $$
BEGIN
    IF NOT EXISTS (SELECT 1 FROM "MstProcedure" WHERE "ProcedureCode" = 'POLI_22377' AND NOT "IsDelete")
       OR NOT EXISTS (SELECT 1 FROM "MstProcedure" WHERE "ProcedureCode" = 'POLI_26530' AND NOT "IsDelete") THEN
        RAISE EXCEPTION 'RJ-DOC-REV-BE-005: tindakan konsultasi POLI_22377/POLI_26530 tidak ditemukan. Skrip dihentikan.';
    END IF;
    IF NOT EXISTS (SELECT 1 FROM "MstTariffCategory" WHERE "TariffCategoryCode" = 'CONSULTATION' AND NOT "IsDelete") THEN
        RAISE EXCEPTION 'RJ-DOC-REV-BE-005: kategori tarif CONSULTATION tidak ditemukan. Skrip dihentikan.';
    END IF;
END $$;

WITH sched AS (
    SELECT DISTINCT s."DoctorId", s."ServiceUnitId", s."ClinicId"
    FROM "MstDoctorSchedule" s
    JOIN "MstDoctor" d ON d."Id" = s."DoctorId" AND d."IsActive" AND NOT d."IsDelete"
    WHERE NOT s."IsDelete" AND s."ClinicId" IS NOT NULL
),
src AS (
    SELECT
        sc."DoctorId", sc."ServiceUnitId", sc."ClinicId",
        d."DoctorCode", d."FullName", c."ClinicCode", c."ClinicName",
        CASE
            WHEN c."ClinicName" ILIKE '%umum%' OR c."ClinicName" ILIKE '%medical check%'
                THEN (SELECT "Id" FROM "MstProcedure" WHERE "ProcedureCode" = 'POLI_22377' AND NOT "IsDelete" LIMIT 1)
            ELSE (SELECT "Id" FROM "MstProcedure" WHERE "ProcedureCode" = 'POLI_26530' AND NOT "IsDelete" LIMIT 1)
        END AS "ProcedureId"
    FROM sched sc
    JOIN "MstDoctor" d ON d."Id" = sc."DoctorId"
    JOIN "MstClinic" c ON c."Id" = sc."ClinicId"
)
INSERT INTO "MstDoctorServiceRule" (
    "Id", "RuleCode", "RuleName", "RuleType", "DoctorId", "ServiceUnitId", "ClinicId",
    "TariffCategoryId", "TariffId", "ProcedureId", "PatientClassId",
    "IsAllowWalkIn", "IsAllowAppointment", "IsAllowKioskRegistration", "IsAllowTelemedicine",
    "IsNeedReferral", "IsNeedApproval", "IsPrimaryForClinic", "IsDefaultForClinic",
    "DailyQuotaLimit", "PriorityLevel", "RuleStatus", "EffectiveStartDate", "EffectiveEndDate",
    "SortOrder", "Description", "IsActive",
    "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy", "CancelBy", "IsCancel", "IsDelete")
SELECT
    gen_random_uuid(),
    LEFT('DSR-RJ-' || src."DoctorCode" || '-' || src."ClinicCode", 50),
    LEFT('Konsultasi ' || src."FullName" || ' - ' || src."ClinicName", 200),
    2,                                   -- DoctorServiceRuleType.Consultation
    src."DoctorId", src."ServiceUnitId", src."ClinicId",
    (SELECT "Id" FROM "MstTariffCategory" WHERE "TariffCategoryCode" = 'CONSULTATION' AND NOT "IsDelete" LIMIT 1),
    NULL,
    src."ProcedureId",
    NULL,
    true, true, true, false,
    false, false, false, false,
    0, 0, 2,                             -- DoctorServiceRuleStatus.Active
    NULL, NULL,
    0, 'RJ-DOC-REV-BE-005 — aturan konsultasi rawat jalan dari jadwal dokter', true,
    CURRENT_TIMESTAMP, '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000',
    '00000000-0000-0000-0000-000000000000', '00000000-0000-0000-0000-000000000000', false, false
FROM src
WHERE NOT EXISTS (
    SELECT 1 FROM "MstDoctorServiceRule" r
    WHERE r."DoctorId" = src."DoctorId"
      AND r."ServiceUnitId" = src."ServiceUnitId"
      AND r."ClinicId" = src."ClinicId"
      AND r."RuleType" = 2
      AND NOT r."IsDelete")
  AND NOT EXISTS (
    SELECT 1 FROM "MstDoctorServiceRule" r
    WHERE r."RuleCode" = LEFT('DSR-RJ-' || src."DoctorCode" || '-' || src."ClinicCode", 50));

COMMIT;

-- Pemeriksaan (hanya membaca)
SELECT d."FullName", c."ClinicName", p."ProcedureCode", r."RuleCode"
FROM "MstDoctorServiceRule" r
JOIN "MstDoctor" d ON d."Id" = r."DoctorId"
JOIN "MstClinic" c ON c."Id" = r."ClinicId"
LEFT JOIN "MstProcedure" p ON p."Id" = r."ProcedureId"
WHERE r."RuleCode" LIKE 'DSR-RJ-%' AND NOT r."IsDelete"
ORDER BY 1, 2;
