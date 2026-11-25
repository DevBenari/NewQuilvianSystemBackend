-- PAY-BE-FIX-025 (lampiran): audit BACA-SAJA nilai default komponen penggajian terhadap aturan eksklusif baru. Tidak mengubah apa pun.
-- Aturan baru: Fixed hanya Nominal Default; Percentage hanya Persentase Default; Formula hanya Formula; Nominal dan Persentase tidak aktif bersamaan.

-- [1-5] Pengelompokan komponen aktif (belum dihapus) menurut metode dan nilai default
SELECT
  CASE
    WHEN "DefaultAmount" > 0 AND "DefaultPercentage" > 0 THEN
         CASE WHEN "CalculationMethod" IN ('Fixed','Percentage') THEN '3/4. ' || "CalculationMethod" || ' dengan kedua nilai > 0 (KONFLIK)'
              ELSE '5. ' || "CalculationMethod" || ' dengan kedua nilai > 0 (KONFLIK)' END
    WHEN "CalculationMethod" = 'Fixed' AND "DefaultPercentage" = 0 THEN '1. Fixed, persentase 0 (sesuai)'
    WHEN "CalculationMethod" = 'Fixed' AND "DefaultPercentage" > 0 THEN 'Fixed dengan persentase > 0 (KONFLIK)'
    WHEN "CalculationMethod" = 'Percentage' AND "DefaultAmount" = 0 THEN '2. Percentage, nominal 0 (sesuai)'
    WHEN "CalculationMethod" = 'Percentage' AND "DefaultAmount" > 0 THEN 'Percentage dengan nominal > 0 (KONFLIK)'
    WHEN "CalculationMethod" = 'Formula' AND ("DefaultAmount" > 0 OR "DefaultPercentage" > 0) THEN 'Formula dengan nilai default > 0 (KONFLIK)'
    WHEN "CalculationMethod" = 'Formula' THEN 'Formula tanpa nilai default (sesuai)'
    WHEN "DefaultAmount" > 0 OR "DefaultPercentage" > 0 THEN '5. ' || "CalculationMethod" || ' dengan nilai default (kontrak lama dipertahankan)'
    ELSE ''|| "CalculationMethod" || ' tanpa nilai default (sesuai)'
  END AS "Kelompok",
  count(*) AS "Jumlah"
FROM "public"."MstPayrollComponent"
WHERE "IsDelete" = false
GROUP BY 1
ORDER BY 1;

-- Formula tersimpan pada metode non-Formula (ditolak backend baru bila dikirim ulang)
SELECT "PayrollComponentCode", "PayrollComponentName", "CalculationMethod", left("FormulaExpression", 60) AS "Formula"
FROM "public"."MstPayrollComponent"
WHERE "IsDelete" = false AND "CalculationMethod" <> 'Formula' AND btrim(coalesce("FormulaExpression", '')) <> '';

-- [DETAIL] Komponen yang KONFLIK menurut aturan baru, beserta pemakaiannya (6-8). Inilah daftar yang perlu keputusan pengguna berwenang.
WITH konflik AS (
  SELECT c."Id", c."PayrollComponentCode", c."PayrollComponentName", c."CalculationMethod", c."DefaultAmount", c."DefaultPercentage", c."IsActive"
  FROM "public"."MstPayrollComponent" c
  WHERE c."IsDelete" = false AND (
        (c."DefaultAmount" > 0 AND c."DefaultPercentage" > 0)
     OR (c."CalculationMethod" = 'Fixed'      AND c."DefaultPercentage" > 0)
     OR (c."CalculationMethod" = 'Percentage' AND c."DefaultAmount" > 0)
     OR (c."CalculationMethod" = 'Formula'    AND (c."DefaultAmount" > 0 OR c."DefaultPercentage" > 0)))
)
SELECT k."PayrollComponentCode", k."PayrollComponentName", k."CalculationMethod", k."DefaultAmount", k."DefaultPercentage", k."IsActive",
  (SELECT count(*) FROM "public"."MstAllowanceType"        t WHERE t."PayrollComponentId" = k."Id" AND t."IsDelete" = false) AS "JenisTunjangan",
  (SELECT count(*) FROM "public"."MstDeductionType"        t WHERE t."PayrollComponentId" = k."Id" AND t."IsDelete" = false) AS "JenisPotongan",
  (SELECT count(*) FROM "public"."PrlPayrollEmployeeComponent" t WHERE t."PayrollComponentId" = k."Id") AS "BarisPayrollRun",
  (SELECT count(*) FROM "public"."PrlPayrollVariableInput" t WHERE t."PayrollComponentId" = k."Id") AS "PenghasilanVariabel",
  (SELECT count(*) FROM "public"."PrlPayrollAdjustment"    t WHERE t."PayrollComponentId" = k."Id") AS "Penyesuaian",
  (SELECT count(*) FROM "public"."PrlWorkforceAllowance"   t WHERE t."PayrollComponentId" = k."Id") AS "TunjanganPegawai",
  (SELECT count(*) FROM "public"."PrlAllowancePolicy"      t WHERE t."PayrollComponentId" = k."Id") AS "KebijakanTunjangan",
  (SELECT count(*) FROM "public"."TrxEmployeeLoan"         t WHERE t."PayrollComponentId" = k."Id") AS "PinjamanKaryawan",
  (SELECT count(*) FROM "public"."TrxOvertimeRealization"  t WHERE t."PayrollComponentId" = k."Id") AS "RealisasiLembur"
FROM konflik k
ORDER BY k."PayrollComponentName";

-- [6] Pemakaian SELURUH komponen oleh payroll (semua tabel yang mereferensikan komponen) -- untuk menilai dampak sebelum mengubah metode
SELECT c."PayrollComponentCode", c."PayrollComponentName", c."CalculationMethod",
  (SELECT count(*) FROM "public"."PrlPayrollEmployeeComponent" t WHERE t."PayrollComponentId" = c."Id") AS "BarisPayrollRun",
  (SELECT count(*) FROM "public"."PrlPayrollVariableInput" t WHERE t."PayrollComponentId" = c."Id") AS "PenghasilanVariabel",
  (SELECT count(*) FROM "public"."MstAllowanceType" t WHERE t."PayrollComponentId" = c."Id" AND t."IsDelete" = false) AS "JenisTunjangan",
  (SELECT count(*) FROM "public"."MstDeductionType" t WHERE t."PayrollComponentId" = c."Id" AND t."IsDelete" = false) AS "JenisPotongan"
FROM "public"."MstPayrollComponent" c
WHERE c."IsDelete" = false
ORDER BY "BarisPayrollRun" DESC, c."PayrollComponentName";
