-- PAY-BE-FIX-022 (lampiran): memindahkan Kebijakan Tunjangan Transportasi LAMA (PrlTransportAllowancePolicy, metode FixedMonthly)
-- ke Kebijakan Tunjangan umum (PrlAllowancePolicy + PrlAllowancePolicyGradeRate) agar dapat dipilih di "Tambah Tunjangan".
--
-- SYARAT: DDL PAY-BE-FIX-021 bagian 5 sudah diterapkan (tabel PrlAllowancePolicy dan PrlAllowancePolicyGradeRate ada).
-- SIFAT: aman diulang (kebijakan yang kodenya sudah ada dilewati); tidak mengubah/menghapus kebijakan lama; tidak menyentuh penetapan pegawai.
-- CARA PAKAI: jalankan apa adanya. Skrip berakhir ROLLBACK sehingga Anda dapat meninjau 3 hasil pemeriksaan di bawah. Bila sudah benar,
--             ganti baris "ROLLBACK;" paling bawah dengan "COMMIT;" lalu jalankan ulang.
--
-- YANG DIPINDAHKAN: hanya kebijakan lama yang AKTIF, belum dihapus, belum berakhir, bermetode FixedMonthly, punya SalaryGradeId dan
-- PayrollComponentId, bernominal > 0, dan batas bulanannya 0 (tanpa batas) atau >= nominal; serta komponennya cocok TEPAT satu jenis
-- tunjangan Transportasi aktif di Master Tunjangan. Yang tidak memenuhi syarat muncul di pemeriksaan [2] dengan alasannya.
-- PerAttendance / PerDay / Reimbursement TIDAK dipindahkan (mesin hitung belum menghitungnya).
--
-- PERHATIAN (butuh keputusan HRD): kebijakan umum mencocokkan pegawai berdasarkan Grade Gaji (+ legal entity/fasilitas), TIDAK berdasarkan
-- Struktur Gaji. Kebijakan lama yang dibatasi satu Struktur Gaji akan berlaku untuk semua struktur pada grade yang sama. Kolom
-- "JumlahStrukturAktifPadaGrade" di pemeriksaan [1] menunjukkan apakah ini berisiko (>1 berarti ya).

BEGIN;

CREATE TEMP TABLE "_kandidat_transport" ON COMMIT DROP AS
SELECT
    l."Id"                                         AS "LegacyId",
    l."PolicyCode"                                 AS "LegacyCode",
    l."PolicyName"                                 AS "PolicyName",
    l."SalaryGradeId"                              AS "SalaryGradeId",
    l."SalaryStructureId"                          AS "SalaryStructureId",
    l."PayrollComponentId"                         AS "PayrollComponentId",
    l."LegalEntityId"                              AS "LegalEntityId",
    l."HospitalSiteId"                             AS "HospitalSiteId",
    l."FixedMonthlyAmount"                         AS "Amount",
    NULLIF(l."MonthlyLimitAmount", 0)              AS "MaximumAmount",
    (date_trunc('day', COALESCE(l."EffectiveStartDate", l."CreateDateTime") AT TIME ZONE 'UTC')) AT TIME ZONE 'UTC' AS "StartUtc",
    CASE WHEN l."EffectiveEndDate" IS NULL THEN NULL
         ELSE (date_trunc('day', l."EffectiveEndDate" AT TIME ZONE 'UTC')) AT TIME ZONE 'UTC' END AS "EndUtc",
    t."Id"                                         AS "AllowanceTypeId",
    ('MIG-' || left(l."PolicyCode", 46))           AS "NewCode"
FROM "public"."PrlTransportAllowancePolicy" l
JOIN (
    SELECT "PayrollComponentId", min("Id"::text)::uuid AS "Id", count(*) AS "Jumlah"
    FROM "public"."MstAllowanceType"
    WHERE "IsDelete" = false AND "IsActive" = true AND "IsRecurring" = true AND lower("AllowanceCategory") = 'transport'
      AND "PayrollComponentId" IS NOT NULL
    GROUP BY "PayrollComponentId"
    HAVING count(*) = 1
) t ON t."PayrollComponentId" = l."PayrollComponentId"
WHERE l."IsDelete" = false AND l."IsActive" = true
  AND l."CalculationMethod" = 'FixedMonthly'
  AND l."SalaryGradeId" IS NOT NULL
  AND l."FixedMonthlyAmount" > 0
  AND (l."MonthlyLimitAmount" = 0 OR l."MonthlyLimitAmount" >= l."FixedMonthlyAmount")
  AND (l."EffectiveEndDate" IS NULL OR l."EffectiveEndDate" >= now());

-- [1] Yang AKAN dipindahkan (periksa nominal, grade, dan risiko cakupan struktur)
SELECT k."LegacyCode", k."PolicyName", g."SalaryGradeName", k."Amount", k."MaximumAmount", k."StartUtc", k."EndUtc", k."NewCode",
       (SELECT count(*) FROM "public"."MstSalaryStructure" s
         WHERE s."SalaryGradeId" = k."SalaryGradeId" AND s."IsDelete" = false AND s."IsActive" = true) AS "JumlahStrukturAktifPadaGrade",
       k."SalaryStructureId" IS NOT NULL AS "KebijakanLamaDibatasiStruktur",
       EXISTS (SELECT 1 FROM "public"."PrlAllowancePolicy" p WHERE p."PolicyCode" = k."NewCode" AND p."IsDelete" = false) AS "SudahAdaDilewati"
FROM "_kandidat_transport" k
JOIN "public"."MstSalaryGrade" g ON g."Id" = k."SalaryGradeId"
ORDER BY k."LegacyCode";

-- [2] Kebijakan lama aktif yang TIDAK dipindahkan beserta alasannya
SELECT l."PolicyCode", l."PolicyName", l."CalculationMethod",
       CASE
         WHEN l."CalculationMethod" <> 'FixedMonthly' THEN 'Metode bukan FixedMonthly (belum dihitung mesin)'
         WHEN l."SalaryGradeId" IS NULL THEN 'Belum punya Grade Gaji (pemetaan dari grade kepegawaian belum diputuskan HRD)'
         WHEN l."PayrollComponentId" IS NULL THEN 'Belum punya komponen payroll'
         WHEN l."FixedMonthlyAmount" <= 0 THEN 'Nominal bulanan 0'
         WHEN l."MonthlyLimitAmount" > 0 AND l."MonthlyLimitAmount" < l."FixedMonthlyAmount" THEN 'Batas bulanan lebih kecil dari nominal'
         WHEN l."EffectiveEndDate" IS NOT NULL AND l."EffectiveEndDate" < now() THEN 'Sudah berakhir'
         ELSE 'Komponen tidak cocok tepat satu jenis tunjangan Transportasi aktif di Master Tunjangan'
       END AS "Alasan"
FROM "public"."PrlTransportAllowancePolicy" l
WHERE l."IsDelete" = false AND l."IsActive" = true
  AND NOT EXISTS (SELECT 1 FROM "_kandidat_transport" k WHERE k."LegacyId" = l."Id");

-- Pemindahan
INSERT INTO "public"."PrlAllowancePolicy"
    ("Id", "AllowanceTypeId", "PolicyCode", "PolicyName", "CalculationMethod", "CurrencyCode", "PayrollComponentId",
     "LegalEntityId", "HospitalSiteId", "EffectiveStartDate", "EffectiveEndDate", "Description")
SELECT gen_random_uuid(), k."AllowanceTypeId", k."NewCode", k."PolicyName", 'FixedAmount', 'IDR', k."PayrollComponentId",
       k."LegalEntityId", k."HospitalSiteId", k."StartUtc", k."EndUtc",
       left('Dipindahkan dari kebijakan transportasi lama ' || k."LegacyCode" || ' (' || k."LegacyId"::text || ').', 500)
FROM "_kandidat_transport" k
WHERE NOT EXISTS (SELECT 1 FROM "public"."PrlAllowancePolicy" p WHERE p."PolicyCode" = k."NewCode" AND p."IsDelete" = false);

INSERT INTO "public"."PrlAllowancePolicyGradeRate"
    ("Id", "AllowancePolicyId", "SalaryGradeId", "Amount", "MaximumAmount", "EffectiveStartDate", "EffectiveEndDate")
SELECT gen_random_uuid(), p."Id", k."SalaryGradeId", k."Amount", k."MaximumAmount", p."EffectiveStartDate", p."EffectiveEndDate"
FROM "_kandidat_transport" k
JOIN "public"."PrlAllowancePolicy" p ON p."PolicyCode" = k."NewCode" AND p."IsDelete" = false
WHERE NOT EXISTS (SELECT 1 FROM "public"."PrlAllowancePolicyGradeRate" r
                  WHERE r."AllowancePolicyId" = p."Id" AND r."SalaryGradeId" = k."SalaryGradeId" AND r."IsDelete" = false);

-- [3] Hasil akhir kebijakan umum Transportasi (harus memuat semua baris [1])
SELECT p."PolicyCode", p."PolicyName", p."CalculationMethod", p."IsActive", r."Amount", r."MaximumAmount", g."SalaryGradeName"
FROM "public"."PrlAllowancePolicy" p
JOIN "public"."PrlAllowancePolicyGradeRate" r ON r."AllowancePolicyId" = p."Id" AND r."IsDelete" = false
JOIN "public"."MstSalaryGrade" g ON g."Id" = r."SalaryGradeId"
WHERE p."PolicyCode" LIKE 'MIG-%' AND p."IsDelete" = false
ORDER BY p."PolicyCode";

ROLLBACK; -- ganti dengan COMMIT; setelah [1], [2], [3] ditinjau dan benar
