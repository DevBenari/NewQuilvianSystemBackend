-- =====================================================================================
-- Isi awal master gizi
--
-- SIFAT DATA INI: TITIK MULAI, BUKAN KETETAPAN
--
--   Isi berikut disusun dari kelaziman rumah sakit di Indonesia, BUKAN dari ketetapan
--   instalasi gizi Anda. Ia ada supaya alur Gizi dapat dijalankan hari ini juga.
--
--   Sebelum dipakai melayani pasien sungguhan, instalasi gizi WAJIB meninjau:
--     - nama dan cakupan setiap jenis diet;
--     - bentuk makanan yang benar-benar disediakan dapur;
--     - jam makan yang sebenarnya berlaku.
--
--   Nonaktifkan yang tidak dipakai lewat layar Master Gizi; jangan dihapus bila sudah
--   pernah dipakai pada diet pasien, karena riwayat diet akan kehilangan acuannya.
--
-- YANG SENGAJA TIDAK DIISI
--   Master diagnosis gizi (`MstDiagnosis` bertipe NUTRITION). Isinya menunggu
--   `GIZ-OQ-002` yang berstatus BLOCKED_BY_BUSINESS_DECISION. Diagnosis karangan akan
--   terlihat resmi padahal tidak pernah disahkan siapa pun, dan menempel pada rekam
--   medis pasien.
--
-- SIFAT SKRIP
--   Aman diulang: memakai Id tetap dan ON CONFLICT DO NOTHING. Tidak menimpa baris yang
--   sudah disunting admin.
--
-- CARA MENJALANKAN
--   psql -h localhost -U postgres -d <database> -f seed-nutrition-masters.sql
-- =====================================================================================

BEGIN;

CREATE TEMPORARY TABLE _ctx ON COMMIT DROP AS
SELECT
    COALESCE(
        (SELECT "Id" FROM "AspNetUsers" WHERE "NormalizedUserName" = 'SUPERADMIN'),
        '00000000-0000-0000-0000-000000000000'::uuid
    )                        AS actor,
    now() AT TIME ZONE 'utc' AS ts;

-- =====================================================================================
-- 1. Jenis Diet
--
-- IsSpecialDiet menandai diet yang perlu perhatian khusus di dapur, sehingga terlihat
-- lebih dulu pada rekap produksi.
-- =====================================================================================

INSERT INTO "GzDietType" (
    "Id", "DietTypeCode", "DietTypeName", "Description", "IsSpecialDiet",
    "SortOrder", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.code, v.name, v.keterangan, v.khusus, v.urutan, true,
    c.ts, c.actor, c.actor, c.actor, c.actor, false, false
FROM _ctx c, (VALUES
    ('e1000000-0000-4000-8000-000000000001'::uuid, 'DT-BIASA',   'Diet Biasa',
     'Tanpa pembatasan khusus', false, 1),
    ('e1000000-0000-4000-8000-000000000002'::uuid, 'DT-DM',      'Diet Diabetes Melitus',
     'Pembatasan karbohidrat sederhana', true, 2),
    ('e1000000-0000-4000-8000-000000000003'::uuid, 'DT-RG',      'Diet Rendah Garam',
     'Pembatasan natrium', true, 3),
    ('e1000000-0000-4000-8000-000000000004'::uuid, 'DT-RP',      'Diet Rendah Protein',
     'Umumnya untuk gangguan ginjal', true, 4),
    ('e1000000-0000-4000-8000-000000000005'::uuid, 'DT-TKTP',    'Diet Tinggi Kalori Tinggi Protein',
     'Untuk kebutuhan energi dan protein yang meningkat', true, 5),
    ('e1000000-0000-4000-8000-000000000006'::uuid, 'DT-RL',      'Diet Rendah Lemak',
     'Pembatasan lemak', true, 6),
    ('e1000000-0000-4000-8000-000000000007'::uuid, 'DT-RPURIN',  'Diet Rendah Purin',
     'Umumnya untuk asam urat tinggi', true, 7)
) AS v(id, code, name, keterangan, khusus, urutan)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 2. Bentuk Makanan
--
-- Urutannya dari yang paling padat ke paling cair, mengikuti tahapan pemulihan pasien.
-- =====================================================================================

INSERT INTO "GzFoodForm" (
    "Id", "FoodFormCode", "FoodFormName", "Description",
    "SortOrder", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.code, v.name, v.keterangan, v.urutan, true,
    c.ts, c.actor, c.actor, c.actor, c.actor, false, false
FROM _ctx c, (VALUES
    ('e2000000-0000-4000-8000-000000000001'::uuid, 'FF-BIASA',  'Makanan Biasa',
     'Nasi dan lauk seperti biasa', 1),
    ('e2000000-0000-4000-8000-000000000002'::uuid, 'FF-LUNAK',  'Makanan Lunak',
     'Bubur kasar atau nasi tim', 2),
    ('e2000000-0000-4000-8000-000000000003'::uuid, 'FF-SARING', 'Makanan Saring',
     'Bubur saring, tekstur halus', 3),
    ('e2000000-0000-4000-8000-000000000004'::uuid, 'FF-CAIR',   'Makanan Cair',
     'Diberikan lewat gelas atau selang', 4)
) AS v(id, code, name, keterangan, urutan)
ON CONFLICT ("Id") DO NOTHING;

-- =====================================================================================
-- 3. Jadwal Makan
--
-- JAM DI SINI PALING PERLU DIPERIKSA. Jam makan berbeda antar rumah sakit, dan angka
-- ini menentukan pengelompokan produksi serta urutan distribusi.
-- =====================================================================================

INSERT INTO "GzMealSchedule" (
    "Id", "MealScheduleCode", "MealScheduleName", "ServingTime", "IsMainMeal",
    "SortOrder", "IsActive", "CreateDateTime", "CreateBy", "UpdateBy", "DeleteBy",
    "CancelBy", "IsCancel", "IsDelete")
SELECT v.id, v.code, v.name, v.jam, v.utama, v.urutan, true,
    c.ts, c.actor, c.actor, c.actor, c.actor, false, false
FROM _ctx c, (VALUES
    ('e3000000-0000-4000-8000-000000000001'::uuid, 'MS-PAGI',    'Makan Pagi',
     '07:00'::time, true,  1),
    ('e3000000-0000-4000-8000-000000000002'::uuid, 'MS-SNACK1',  'Selingan Pagi',
     '10:00'::time, false, 2),
    ('e3000000-0000-4000-8000-000000000003'::uuid, 'MS-SIANG',   'Makan Siang',
     '12:00'::time, true,  3),
    ('e3000000-0000-4000-8000-000000000004'::uuid, 'MS-SNACK2',  'Selingan Sore',
     '15:00'::time, false, 4),
    ('e3000000-0000-4000-8000-000000000005'::uuid, 'MS-MALAM',   'Makan Malam',
     '18:00'::time, true,  5)
) AS v(id, code, name, jam, utama, urutan)
ON CONFLICT ("Id") DO NOTHING;

COMMIT;

-- =====================================================================================
-- Ringkasan
-- =====================================================================================

SELECT 'Jenis Diet' AS master, COUNT(*)::text AS jumlah FROM "GzDietType" WHERE NOT "IsDelete"
UNION ALL
SELECT 'Bentuk Makanan', COUNT(*)::text FROM "GzFoodForm" WHERE NOT "IsDelete"
UNION ALL
SELECT 'Jadwal Makan', COUNT(*)::text FROM "GzMealSchedule" WHERE NOT "IsDelete"
UNION ALL
SELECT 'Diagnosis Gizi (sengaja kosong, GIZ-OQ-002 BLOCKED)',
       COUNT(*)::text FROM "MstDiagnosis" WHERE "DiagnosisType" = 'NUTRITION' AND NOT "IsDelete";
