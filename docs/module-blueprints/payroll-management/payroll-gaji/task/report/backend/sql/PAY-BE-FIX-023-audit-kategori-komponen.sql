-- PAY-BE-FIX-023 (lampiran): audit BACA-SAJA kondisi Kategori Komponen Penggajian di database. Tidak mengubah apa pun.
-- Jalankan sebelum memakai formulir Tambah Kategori, agar tahu kategori mana yang sudah ada, bermasalah, atau perlu dibuat.

-- [1] Lima kategori inti: status per kategori (Missing = belum ada; Ready = sesuai; Inactive = ada tetapi nonaktif; GroupMismatch = ada tetapi kelompok teknisnya salah)
WITH katalog(nama, kelompok) AS (
    VALUES ('Gaji Pokok','Earning'), ('Tunjangan','Earning'), ('Insentif','Earning'), ('Potongan','Deduction'), ('Bonus','Earning')
)
SELECT k.nama AS "KategoriInti", k.kelompok AS "KelompokSeharusnya",
       COALESCE(string_agg(c."ComponentCategoryCode" || ' [' || c."ComponentGroup" || CASE WHEN c."IsActive" THEN '' ELSE ', nonaktif' END || ']', '; '), '-') AS "YangAdaDiDatabase",
       CASE
         WHEN count(c."Id") = 0 THEN 'Missing'
         WHEN bool_or(c."ComponentGroup" = k.kelompok AND c."IsActive") THEN 'Ready'
         WHEN bool_or(c."ComponentGroup" = k.kelompok) THEN 'Inactive'
         ELSE 'GroupMismatch'
       END AS "Status"
FROM katalog k
LEFT JOIN "public"."MstPayrollComponentCategory" c
       ON c."IsDelete" = false
      AND lower(regexp_replace(btrim(c."ComponentCategoryName"), '\s+', ' ', 'g')) = lower(k.nama)
GROUP BY k.nama, k.kelompok
ORDER BY k.nama;

-- [2] Nama inti ganda (harus 0 baris)
SELECT lower(regexp_replace(btrim("ComponentCategoryName"), '\s+', ' ', 'g')) AS "NamaDinormalkan", count(*) AS "Jumlah"
FROM "public"."MstPayrollComponentCategory"
WHERE "IsDelete" = false
GROUP BY 1
HAVING count(*) > 1;

-- [3] Kategori LEGACY (di luar lima nama inti) beserta jumlah komponen yang memakainya. Tetap dibaca dan dipakai, tidak ditawarkan untuk komponen baru.
SELECT c."ComponentCategoryCode", c."ComponentCategoryName", c."ComponentGroup", c."IsActive",
       (SELECT count(*) FROM "public"."MstPayrollComponent" p WHERE p."PayrollComponentCategoryId" = c."Id" AND p."IsDelete" = false) AS "JumlahKomponen"
FROM "public"."MstPayrollComponentCategory" c
WHERE c."IsDelete" = false
  AND lower(regexp_replace(btrim(c."ComponentCategoryName"), '\s+', ' ', 'g')) NOT IN ('gaji pokok','tunjangan','insentif','potongan','bonus')
ORDER BY c."ComponentCategoryName";

-- [4] Komponen yang ComponentType-nya TIDAK selaras dengan ComponentGroup kategorinya. Dipertahankan apa adanya oleh backend selama
--     kategorinya tidak diganti; tidak dikonversi otomatis. Putuskan per kasus sebelum menyentuhnya.
SELECT p."PayrollComponentCode", p."PayrollComponentName", p."ComponentType" AS "TipeKomponen",
       c."ComponentCategoryName" AS "Kategori", c."ComponentGroup" AS "KelompokKategori", p."IsActive"
FROM "public"."MstPayrollComponent" p
JOIN "public"."MstPayrollComponentCategory" c ON c."Id" = p."PayrollComponentCategoryId"
WHERE p."IsDelete" = false AND c."IsDelete" = false AND p."ComponentType" <> c."ComponentGroup"
ORDER BY c."ComponentCategoryName", p."PayrollComponentName";

-- [5] Komponen turunan (BaseComponentId) yang tipenya berbeda dari komponen dasarnya. Aturan baru menolak kombinasi ini saat dibuat
--     atau saat relasi/tipe diubah; yang sudah tersimpan tidak diubah.
SELECT d."PayrollComponentCode" AS "Turunan", d."ComponentType" AS "TipeTurunan", b."PayrollComponentCode" AS "Dasar", b."ComponentType" AS "TipeDasar"
FROM "public"."MstPayrollComponent" d
JOIN "public"."MstPayrollComponent" b ON b."Id" = d."BaseComponentId"
WHERE d."IsDelete" = false AND b."IsDelete" = false AND d."ComponentType" <> b."ComponentType";
