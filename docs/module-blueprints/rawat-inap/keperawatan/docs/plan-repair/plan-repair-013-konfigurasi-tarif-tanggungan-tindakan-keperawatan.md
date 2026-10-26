# PLAN-REPAIR-013 — Konfigurasi Master Tarif Tindakan Universal, Status Tanggungan Penjamin, dan Eliminasi Label Tagihan Kasir pada Ruang Kerja Keperawatan

```yaml
plan_id: PLAN-REPAIR-KEP-013
issue: ../issue/issue-013-konfigurasi-tarif-tanggungan-tindakan-keperawatan.md
status_rencana: SELESAI
tanggal_rencana: "2026-10-20"
diputuskan_oleh: "Pengguna / Pemilik Kebutuhan"
tanggal_keputusan: "2026-10-20"
basis_source_backend: "6e0f2b1b (MHamzah)"
basis_source_frontend: "a491b05a (HamzahV2)"
```

---

## 1. Register Status Pengerjaan

| ID perbaikan | Menutup | Ringkasan | Area | Prioritas | Task ID | Status | Bukti |
| --- | --- | --- | :---: | :---: | :---: | :---: | --- |
| `FIX-KEP-013-01` | `ISS-KEP-013-01` | Script SQL Seed Master Tarif Universal (`MstTariff`) dan Aturan Tanggungan Penjamin (`MstCompanyGuarantorCoverageRule` & `MstInsuranceCoverageRule`) | Data (PostgreSQL) | 1 | — | ✅ SELESAI | `Migrations/scripts/seed-procedure-universal-tariffs-and-coverage.sql` |
| `FIX-KEP-013-02` | `ISS-KEP-013-01` | *Decoupling* penentuan status tanggungan dari tarif di backend `InpAncillaryOrderAdapter.cs` | Backend | 2 | — | ✅ SELESAI | `InpAncillaryOrderAdapter.cs:64-88` |
| `FIX-KEP-013-03` | `ISS-KEP-013-02` | Penyesuaian lebar kolom STATUS tabel katalog dan penambahan style `whiteSpace: "nowrap"` pada lencana | Frontend | 2 | — | ✅ SELESAI | `nursing-procedure-order-panel.jsx:251-286`, `procedure-form-panel.jsx:274-314` |
| `FIX-KEP-013-04` | `ISS-KEP-013-03` | Penghilangan teks label *"perkiraan — tagihan final di kasir"* dari tampilan harga tindakan | Frontend + Backend | 1 | — | ✅ SELESAI | `inpatient-coverage-utils.js:1, 23-26, 48-52`, `InpAncillaryOrderAdapter.cs:77` |

**Ringkasan: 4 dari 4 perbaikan selesai (Implementasi kode dan berkas seed selesai; eksekusi seed SQL ke PostgreSQL dan dotnet build dilakukan mandiri oleh pengguna).**

---

## 2. Solusi Terpilih per Temuan

### ISS-KEP-013-01 — Master Data Tarif dan Aturan Tanggungan Tindakan

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Buat data dummy universal (`PatientClassId = NULL`, `ServiceUnitId = NULL`, `ClinicId = NULL`) di `MstTariff` untuk seluruh tindakan aktif, ditambah aturan 100% Ditanggung di `MstCompanyGuarantorCoverageRule` dan `MstInsuranceCoverageRule`. | Cepat, otomatis berlaku untuk seluruh kelas rawat inap (Kelas 1, 2, 3, VIP) tanpa duplikasi baris, langsung mengaktifkan status "Ditanggung". | Data bersifat buatan/dummy (sesuai persetujuan pengembang untuk testing). |
| B | Masukkan tarif secara manual per kelas satu per satu via UI Master Data. | Presisi per kelas kamar. | Sangat memakan waktu, rawan terlewat, dan menghambat pengujian fitur klinis. |

**Solusi terpilih: Opsi A.**
Sesuai persetujuan eksplisit pengguna pada 20-10-2026, pengisian data buatan universal adalah cara paling efektif untuk membuka blokir alur pengujian tindakan rawat inap.

---

### ISS-KEP-013-02 — Lencana Status Terlipat Vertikal

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Sesuaikan proporsi kolom tabel (`STATUS` menjadi 18%, `TARIF` 22%) dan pasang style `whiteSpace: "nowrap"` pada kontainer lencana status di `nursing-procedure-order-panel.jsx` dan `procedure-form-panel.jsx`. | Lencana tampil horizontal rapi, baris tabel tidak membesar vertikal secara berlebihan. | Menggeser sedikit lebar kolom tarif. |
| B | Singkat teks label badge menjadi "Belum Ada". | Teks sangat pendek. | Kurang deskriptif bagi perawat. |

**Solusi terpilih: Opsi A.**
Menjaga konsistensi standar desain antarmuka Quilvian tanpa memotong arti pesan status.

---

### ISS-KEP-013-03 — Keterangan "perkiraan — tagihan final di kasir"

| Opsi | Cara | Kelebihan | Kekurangan |
| --- | --- | --- | --- |
| **A (Terpilih)** | Hapus konstanta `ESTIMATE_PRICE_LABEL` dari fungsi pemformatan harga (`describeCoveragePrice` dan `describeCoverageTotal`) di `inpatient-coverage-utils.js` serta bersihkan `PriceLabel` di backend adapter. | Memenuhi 100% aturan pelapor; tampilan tarif klinis bersih dan murni berupa nominal rupiah saja. | Tidak ada penegasan teks kasir (namun dokter/perawat memang tidak memerlukannya). |
| B | Pindahkan tulisan ke tooltip / icon info kecil. | Penjelasan tetap tersimpan di hover. | Bertentangan dengan arahan pelapor yang meminta kata-kata tersebut dihilangkan sepenuhnya. |

**Solusi terpilih: Opsi A.**
Mengikuti instruksi mutlak pelapor (Aturan Pelapor).

---

## 3. Skema Tampilan Sebelum → Sesudah

### SEBELUM (Kondisi Bermasalah saat ini)

```text
+------------------------------------------------------------------------------------------------------+
| Form Tindakan Medis - Rawat Inap                                                 [ 0 Tindakan ]      |
+------------------------------------------------------------------------------------------------------+
| Daftar Tindakan Medis                                                          43 tindakan tersedia  |
| [ Cari tindakan medis berdasarkan kode atau nama..                                                 ] |
|------------------------------------------------------------------------------------------------------|
| KODE           | NAMA TINDAKAN             | TARIF                         | STATUS       | AKSI     |
|----------------|---------------------------|-------------------------------|--------------|----------|
| PR-RSMMC-00025 | Angkat Jahitan            | tarif belum tersedia          | Status       | [ + ]    |
|                | Bedah Minor               | KELAS II                      | Tangg        |          |
|                |                           |                               | ungan        |          |
|                |                           |                               | Belum        |          |
|                |                           |                               | Tersed       |          |
|                |                           |                               | ia           |          |
|----------------|---------------------------|-------------------------------|--------------|----------|
| PR-RSMMC-00019 | Defibrilasi / Kardioversi | tarif belum tersedia          | (sama pecah) | [ + ]    |
|                | Gawat Darurat             | KELAS II                      |              |          |
+------------------------------------------------------------------------------------------------------+
```

### SESUDAH (Setelah Perbaikan Data & UI)

```text
+------------------------------------------------------------------------------------------------------+
| Form Tindakan Medis - Rawat Inap                                                 [ 0 Tindakan ]      |
+------------------------------------------------------------------------------------------------------+
| Daftar Tindakan Medis                                                          43 tindakan tersedia  |
| [ Cari tindakan medis berdasarkan kode atau nama..                                                 ] |
|------------------------------------------------------------------------------------------------------|
| KODE           | NAMA TINDAKAN             | TARIF                         | STATUS       | AKSI     |
|----------------|---------------------------|-------------------------------|--------------|----------|
| PR-RSMMC-00025 | Angkat Jahitan            | Rp 100.000                    | [Ditanggung] | [ + ]    |
|                | Bedah Minor               | KELAS II                      |              |          |
|----------------|---------------------------|-------------------------------|--------------|----------|
| PR-RSMMC-00019 | Defibrilasi / Kardioversi | Rp 350.000                    | [Ditanggung] | [ + ]    |
|                | Gawat Darurat             | KELAS II                      |              |          |
|----------------|---------------------------|-------------------------------|--------------|----------|
| PR-RSMMC-00027 | Ekstraksi Kuku            | Rp 200.000                    | [Ditanggung] | [ + ]    |
|                | Bedah Minor               | KELAS II                      |              |          |
+------------------------------------------------------------------------------------------------------+
```

#### Tabel Komponen Tampilan SESUDAH:
| Wilayah | Isi | Sumber Data | Komponen |
| --- | --- | --- | --- |
| Kolom TARIF | Nominal Rupiah (misal: `Rp 100.000`) tanpa embel-embel kasir, di bawahnya nama kelas kamar (`KELAS II`) | `coverageFor(item).estimatedUnitPrice` via `describeCoveragePrice` | Text div + small badge kelas |
| Kolom STATUS | Badge hijau solid bertuliskan `"Ditanggung"` (atau abu-abu `"Tidak Di-cover"`) dalam 1 baris mendatar | `coverageFor(item).isCovered` & `label` | `<StatusBadge status="active" label="Ditanggung" />` |
| Kolom AKSI | Tombol biru kotak dengan ikon tambah putih | Trigger form konfigurasi di kolom kanan | `<BaseButton>` |

---

## 4. Rincian Perbaikan

### FIX-KEP-013-01: Script SQL Seed Master Data Universal (PostgreSQL)

Jalankan script SQL berikut pada database target development (`QuilvianNewDevHamzah`):

```sql
-- ==============================================================================
-- FIX-KEP-013-01: SEED MASTER TARIF UNIVERSAL & ATURAN COVERAGE TINDAKAN RAWAT INAP
-- Target Database: QuilvianNewDevHamzah
-- ==============================================================================

BEGIN;

-- 1. Pastikan Kategori Tarif Umum untuk Tindakan Medis Tersedia
DO $$
DECLARE
    v_category_id UUID;
BEGIN
    SELECT "Id" INTO v_category_id FROM public."MstTariffCategory" WHERE "CategoryCode" = 'TC-PROC' OR "CategoryName" ILIKE '%Tindakan%' LIMIT 1;
    IF v_category_id IS NULL THEN
        v_category_id := gen_random_uuid();
        INSERT INTO public."MstTariffCategory" (
            "Id", "CategoryCode", "CategoryName", "Description", "IsActive", "IsDelete", "CreateBy", "CreateDateTime"
        ) VALUES (
            v_category_id, 'TC-PROC', 'Tarif Tindakan Medis', 'Kategori tarif umum untuk tindakan medis dan keperawatan', true, false, 'SYSTEM_SEEDED', NOW()
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
    "NormalPrice",
    "EffectiveStartDate",
    "EffectiveEndDate",
    "IsActive",
    "IsDelete",
    "CreateBy",
    "CreateDateTime"
)
SELECT 
    gen_random_uuid() AS "Id",
    'TRF-' || p."ProcedureCode" AS "TariffCode",
    'Tarif ' || p."ProcedureName" AS "TariffName",
    (SELECT "Id" FROM public."MstTariffCategory" WHERE "CategoryCode" = 'TC-PROC' OR "CategoryName" ILIKE '%Tindakan%' LIMIT 1) AS "TariffCategoryId",
    p."Id" AS "ProcedureId",
    NULL::UUID AS "DrugId",
    NULL::UUID AS "MedicalEquipmentId",
    NULL::UUID AS "PatientClassId",   -- NULL = Universal di seluruh kelas kamar rawat inap
    NULL::UUID AS "ServiceUnitId",    -- NULL = Universal di seluruh unit
    NULL::UUID AS "ClinicId",         -- NULL = Universal di seluruh klinik
    0 AS "SurgeryComponentType",
    1 AS "ChargeBasis",              -- PerService
    1 AS "ChargeRounding",
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
    'SYSTEM_SEEDED' AS "CreateBy",
    NOW() AS "CreateDateTime"
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
    "IsActive",
    "IsDelete",
    "CreateBy",
    "CreateDateTime"
)
SELECT 
    gen_random_uuid(),
    cg."Id",
    'RULE-COMP-PROC-' || SUBSTRING(cg."CompanyGuarantorCode" FROM 1 FOR 10),
    'Tanggungan Tindakan Medis ' || cg."CompanyGuarantorName",
    'Procedure',
    NULL::UUID, -- Menjamin seluruh tindakan
    'Covered',
    100.00,
    false,
    false,
    true,
    true,
    false,
    'SYSTEM_SEEDED',
    NOW()
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
    "IsActive",
    "IsDelete",
    "CreateBy",
    "CreateDateTime"
)
SELECT 
    gen_random_uuid(),
    ip."Id",
    'RULE-INS-PROC-' || SUBSTRING(ip."InsuranceProviderCode" FROM 1 FOR 10),
    'Tanggungan Tindakan Medis ' || ip."InsuranceProviderName",
    'Procedure',
    NULL::UUID, -- Menjamin seluruh tindakan
    'Covered',
    100.00,
    false,
    false,
    true,
    true,
    false,
    'SYSTEM_SEEDED',
    NOW()
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
```

---

### FIX-KEP-013-02: Backend Decoupling Status Tanggungan dari Tarif

Pada berkas [`Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs#L64):
* Jangan menahan penetapan `item.IsCovered` hanya ketika `result.TariffId.HasValue`.
* Jika resolver menghasilkan status cover yang valid (`result.IsCoverageApplicable`), isi `item.IsCovered = result.IsCovered` dan pasang label "Ditanggung" atau "Tidak Di-cover" terlepas dari ketersediaan tarif nominal.

---

### FIX-KEP-013-03: Perbaikan CSS dan Lebar Kolom STATUS di Frontend

1. Di [`nursing-procedure-order-panel.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/procedure/nursing-procedure-order-panel.jsx) dan [`procedure-form-panel.jsx`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/physician-workspace/tabs/procedure/procedure-form-panel.jsx):
   * Ubah header tabel:
     ```jsx
     <th style={{ width: "22%" }}>TARIF</th>
     <th style={{ width: "18%" }}>STATUS</th>
     ```
   * Bungkus cell status dengan kontainer inline atau `span`:
     ```jsx
     <td style={{ whiteSpace: "nowrap" }}>
       <StatusBadge status={coverageFor(item)?.isCovered === true ? "active" : "info"} label={typeof coverageFor(item)?.isCovered === "boolean" ? coverageFor(item).label : "Status tanggungan belum tersedia"} />
     </td>
     ```

---

### FIX-KEP-013-04: Eliminasi Label "perkiraan — tagihan final di kasir"

1. Di [`src/utils/health-services/inpatient-management/inpatient-coverage-utils.js`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/utils/health-services/inpatient-management/inpatient-coverage-utils.js):
   * Kosongkan teks: `export const ESTIMATE_PRICE_LABEL = "";`
   * Pada fungsi `describeCoveragePrice`:
     ```javascript
     const price = item.priceStatus === "AVAILABLE" && item.estimatedUnitPrice !== null && Number.isFinite(item.estimatedUnitPrice)
       ? `Rp ${item.estimatedUnitPrice.toLocaleString("id-ID")}`
       : "tarif belum tersedia";
     ```
   * Pada fungsi `describeCoverageTotal`:
     ```javascript
     const price = summary.availableCount ? `Rp ${summary.total.toLocaleString("id-ID")}` : "tarif belum tersedia";
     return `${price}${summary.unavailableCount ? ` · ${summary.unavailableCount} pemeriksaan tanpa tarif` : ""}`;
     ```
2. Di backend [`InpAncillaryOrderAdapter.cs:77`](file:///C:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/Areas/HealthServices/InPatientManagement/Services/InpAncillaryOrderAdapter.cs#L77):
   * Hilangkan penetapan string: `item.PriceLabel = null;`

---

## 5. Urutan Pengerjaan

```text
FIX-KEP-013-01 (Eksekusi SQL Seed di Database PostgreSQL)
  │
  ├─► FIX-KEP-013-04 (Eliminasi Label Kasir di Frontend & Backend)
  │
  ├─► FIX-KEP-013-03 (Perlebar Kolom STATUS & Nowrap di Frontend)
  │
  └─► FIX-KEP-013-02 (Decoupling Evaluasi Tanggungan di Backend Adapter)
```

---

## 6. Keputusan Pemilik yang Dibutuhkan

Seluruh keputusan telah disetujui secara resmi oleh pengguna pada prompt tanggal 20-10-2026:
1. `K-01`: Pengisian data buatan/dummy universal pada tabel tarif dasar (`MstTariff`) dan aturan tanggungan penjamin &rarr; **DISETUJUI**.
2. `K-02`: Perapihan lebar kolom STATUS dan pencegahan text wrapping pada badge &rarr; **DISETUJUI**.
3. `K-03`: Penghapusan kalimat/catatan *"perkiraan — tagihan final di kasir"* dari ranah dokter dan keperawatan &rarr; **DISETUJUI (Aturan Pelapor)**.
4. `K-04`: Pemilihan Opsi A untuk eliminasi teks *"Perlu persetujuan penjamin"* dari label klinis dan pemurnian kolom TARIF agar hanya memuat nominal harga tanpa duplikasi status &rarr; **DISETUJUI**.

---

## 7. Dokumen Hulu yang Ikut Direvisi

* [`docs/module-blueprints/rawat-inap/keperawatan/contracts/`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/keperawatan/contracts/): Menyesuaikan teks spesifikasi tampilan tarif tanpa keterangan label kasir.
* [`docs/module-blueprints/rawat-inap/dokter-rawat-inap/docs/plan-repair/plan-repair-005-tindakan-tarif-tanggungan-gagal-simpan.md`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/NewQuilvianSystemBackend/docs/module-blueprints/rawat-inap/dokter-rawat-inap/docs/plan-repair/plan-repair-005-tindakan-tarif-tanggungan-gagal-simpan.md): Dihubungkan sebagai register penyelesaian paritas dokter dan perawat.

---

## 8. Verifikasi Menyeluruh

Skenario pengujian setelah perbaikan diaplikasikan:
1. **Verifikasi Tampilan Tabel Katalog**:
   * Buka menu Dokumentasi Asuhan &rarr; Tindakan pada Ruang Kerja Keperawatan.
   * Pastikan 43 tindakan medis menampilkan harga berupa nominal rupiah murni (contoh: `Rp 55.000` di atas teks kelas `ODC` atau `KELAS II`).
   * Pastikan tidak ada lagi teks *"perkiraan — tagihan final di kasir"*.
   * Pastikan tidak ada lagi tulisan *"Perlu persetujuan penjamin"* baik pada kolom TARIF maupun pada lencana kolom STATUS.
   * Pastikan badge kolom STATUS menampilkan lencana hijau `"Ditanggung"` secara mendatar tanpa terlipat vertikal.
2. **Verifikasi Keranjang Staging**:
   * Klik tombol `[ + ]` pada salah satu tindakan.
   * Konfigurasi di form sebelah kanan dan tekan Tambah Tindakan.
   * Pastikan di keranjang "Tindakan yang Dipilih", subtotal dan total harga hanya menampilkan angka rupiah tanpa catatan kasir.
3. **Verifikasi Simpan Pesanan**:
   * Klik Simpan Pesanan Tindakan.
   * Pastikan order berhasil tersimpan (status HTTP 200/201) tanpa galat tipe pembayaran atau ketiadaan tarif.

---

## 9. Riwayat

| Tanggal | Versi | Penulis | Keterangan |
| --- | :---: | --- | --- |
| 20-10-2026 | 1.0.0 | AI Agent (`diagnose-module-issue`) | Dokumen rencana perbaikan diterbitkan dengan status DISETUJUI sesuai instruksi dan persetujuan pengguna. |
| 20-10-2026 | 1.1.0 | AI Agent (`diagnose-module-issue`) | Opsi A diterapkan: eliminasi "Perlu persetujuan penjamin", pemurnian fungsi kolom tarif `describeTariffOnly`. |
