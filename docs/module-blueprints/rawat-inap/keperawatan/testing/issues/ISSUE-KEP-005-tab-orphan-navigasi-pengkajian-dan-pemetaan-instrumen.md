# Laporan Isu: Tab Navigasi Pengkajian Yatim (Orphan Tab) dan Ketiadaan Pemetaan Instrumen Klinis

**ID Isu:** `ISSUE-KEP-005`  
**Modul:** Rawat Inap — Keperawatan (`Inpatient Nursing Workspace`)  
**Submodul:** Navigasi Formulir Pengkajian (`assessment-form-nav.jsx`) & Penangan Instrumen (`assessment-section.jsx`)  
**Tingkat Keparahan:** 🟡 **Sedang / Cacat Konsistensi Navigasi & Pemetaan Instrumen (UX & Routing Gap)**  
**Tanggal Temuan:** 23 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Antigravity (Playwright End-to-End Analysis)  
**Status Isu:** 🟢 **TERATASI (Resolved & Verified)**  

---

## 1. Ringkasan Masalah

Komponen navigasi horizontal pengkajian keperawatan rawat inap (`AssessmentFormNav`) mendefinisikan 7 tab pengkajian:
1. `general` — **Kajian Umum**
2. `fall-risk` — **Resiko Jatuh**
3. `pain` — **Monitoring Nyeri**
4. `education` — **Assement Edukasi** *(catatan: terdapat salah ketik "Assement")*
5. `daily-monitoring` — **Pengawasan Harian Pasien**
6. `initial-eval` — **Evaluasi Awal**
7. `discharge-planning` — **Perencanaan Pulang**

Namun, pada komponen induk perender pengkajian ([`assessment-section.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx#L36-L50)), kamus pemetaan instrumen dan tipe dokumen **hanya mencakup 5 tab**:

```javascript
// Baris 36-50 pada assessment-section.jsx
const TAB_TO_INSTRUMENT_KIND = {
  general: 1, // GeneralNursingAssessmentForm
  "fall-risk": 2, // FallRiskScale
  pain: 3, // PainScale
  education: 4, // EducationAssessmentForm
  "discharge-planning": 5, // DischargePlanningForm
  // KOSONG: 'daily-monitoring' dan 'initial-eval' TIDAK ADA!
};

const TAB_TO_ASSESSMENT_TYPES = {
  general: [0, 1], // 0: Initial, 1: Reassessment
  "fall-risk": [6], // FallRisk
  pain: [7], // PainMonitoring
  education: [8], // EducationAssessment
  "discharge-planning": [3], // DischargePlanning
  // KOSONG: 'daily-monitoring' dan 'initial-eval' TIDAK ADA!
};
```

---

## 2. Dampak Klinis dan Pengalaman Pengguna (UX Impact)

1. **Efek Fallback Menyesatkan Perawat (*Misleading Fallback*):**  
   Ketika perawat menekan tab **"Pengawasan Harian Pasien"** atau **"Evaluasi Awal"**, kode fallback berikut dieksekusi:
   ```javascript
   const currentInstrumentKind = TAB_TO_INSTRUMENT_KIND[currentTab] || 1;
   const validTypes = TAB_TO_ASSESSMENT_TYPES[currentTab] || [0, 1];
   ```
   Akibatnya, sistem merender formulir **Kajian Umum Keperawatan (Kind: 1, Subtipe: 0/1)** alih-alih formulir pengawasan harian atau lembar evaluasi awal!
2. **Kekeliruan Input Rekam Medis:**  
   Perawat yang bermaksud mencatat pengawasan harian dapat tanpa sengaja memperbarui atau membuat draf baru Pengkajian Umum, sehingga menimbulkan duplikasi dokumen yang tidak sesuai tujuan klinis.
3. **Instrumen Yatim di Basis Data:**  
   Di basis data telah tersedia instrumen `CASE_MANAGEMENT_CHECKLIST` (Kind: 6, ID: `c1a1f000-0107-4a01-9b01-000000000008` — Checklist Evaluasi Awal MPP) berstatus disahkan (*Approved*), namun instrumen ini tidak pernah terpanggil oleh tab `initial-eval`.

---

## 3. Analisis Teknis & Lokasi Kode

### A. Lokasi Kode Navigasi:
**Berkas:** [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-form-nav.jsx` (Baris 16–24)](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-form-nav.jsx#L16-L24)

```javascript
const V2_ASSESSMENT_TABS = Object.freeze([
  { key: "general", label: "Kajian Umum" },
  { key: "fall-risk", label: "Resiko Jatuh" },
  { key: "pain", label: "Monitoring Nyeri" },
  { key: "education", label: "Assement Edukasi" }, // Typo label: Assement -> Asesmen
  { key: "daily-monitoring", label: "Pengawasan Harian Pasien" }, // Orphan
  { key: "initial-eval", label: "Evaluasi Awal" }, // Orphan
  { key: "discharge-planning", label: "Perencanaan Pulang" },
]);
```

### B. Bukti Instrumen di Basis Data:
Kueri pada tabel `CliClinicalInstrument`:
```text
('c1a1f000-0107-4a01-9b01-000000000008', 'CASE_MANAGEMENT_CHECKLIST', 'Checklist Evaluasi Awal MPP (draft)', Kind: 6, IsActive: True)
```
Instrumen ini dirancang untuk evaluasi awal Manajer Pelayanan Pasien (MPP / Case Manager), namun belum diintegrasikan ke alur kerja `assessment-section.jsx`.

---

## 4. Langkah-Langkah Reproduksi Masalah

1. Masuk ke Ruang Kerja Keperawatan:
   `http://localhost:3000/health-services/inpatient-management/episodes/c3fe1370-18f0-42fb-8d9f-01449212828e/nursing?section=assessment&tab=daily-monitoring`
2. Perhatikan formulir yang dimuat oleh sistem.
3. **Hasil Temuan:** Judul formulir yang tampil adalah *"Kajian Umum Keperawatan Rawat Inap (draft)"* dengan butir-butir anamnesis awal/kondisi umum, bukan lembar pengawasan harian perawat.

---

## 5. Rekomendasi Solusi (*Proposed Resolution*)

Terdapat dua opsi penataan yang selaras dengan arsitektur Quilvian:

### Opsi 1 (Disarankan untuk Rilis Saat Ini — Scope Inpatient Nurse):
Keluarkan tab `daily-monitoring` dan `initial-eval` dari daftar tab formulir pengkajian keperawatan rawat inap, karena:
1. Pengawasan harian pasien di ruang rawat inap biasanya dicatat melalui **Lini Masa / Catatan Perkembangan Pasien Terintegrasi (CPPT)** dan **Tanda Vital (TTV)**, bukan formulir pengkajian statis.
2. Evaluasi awal MPP merupakan wewenang Manajer Pelayanan Pasien (Case Manager) yang memiliki ruang kerja tersendiri.
3. Koreksi salah ketik label `"Assement Edukasi"` menjadi `"Pengkajian Edukasi"` atau `"Asesmen Edukasi"`.

```javascript
const V2_ASSESSMENT_TABS = Object.freeze([
  { key: "general", label: "Kajian Umum" },
  { key: "fall-risk", label: "Risiko Jatuh" },
  { key: "pain", label: "Monitoring Nyeri" },
  { key: "education", label: "Asesmen Edukasi" },
  { key: "discharge-planning", label: "Perencanaan Pulang" },
]);
```

### Opsi 2 (Jika Ingin Mengakomodasi 7 Tab Penuh):
1. Tambahkan pemetaan untuk `initial-eval`:
   ```javascript
   TAB_TO_INSTRUMENT_KIND["initial-eval"] = 6; // CASE_MANAGEMENT_CHECKLIST
   TAB_TO_ASSESSMENT_TYPES["initial-eval"] = [9]; // CaseManagement
   ```
2. Definisikan instrumen atau komponen khusus untuk `daily-monitoring` (misal: lembar observasi harian).

---

## 6. Realisasi Solusi & Hasil Verifikasi (*Resolution & Evidence*)

Perbaikan telah diterapkan secara terintegrasi:
1. **Penjaga Defensif Tab Non-Instrumen (`assessment-section.jsx`):**  
   Menambahkan verifikasi `const isInstrumentTab = Boolean(TAB_TO_INSTRUMENT_KIND[currentTab]);` dan elemen visual khusus dengan atribut `data-testid="assessment-non-instrument-tab"` jika sub-tab yang dibuka bukan merupakan instrumen formulir langsung. Mencegah terjadinya fallback keliru ke formulir Kajian Umum.
2. **Penyelarasan Rute Ruang Kerja (`nursing-workspace-sections.jsx`):**  
   Pengalihan navigasi telah mengarahkan `daily-monitoring` ke komponen khusus [`DailyMonitoringSection`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/daily-monitoring/daily-monitoring-section.jsx) dan `initial-eval` ke [`InitialEvaluationSection`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/initial-evaluation/initial-evaluation-section.jsx).
3. **Koreksi Typo Label Edukasi:**  
   Label `"Assement Edukasi"` telah diperbaiki menjadi `"Asesmen Edukasi"` pada konstanta [`inpatient-nursing-constants.js`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/lib/constants/health-services/inpatient-management/inpatient-nursing-constants.js) dan [`assessment-form-nav.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-form-nav.jsx).
4. **Verifikasi Pengujian Unit:**  
   Pengujian unit pada `tests/unit/inpatient-clinical-instrument-renderer.test.mjs` (`ISSUE-KEP-005: Konsistensi label Asesmen Edukasi dan penjaga defensif isInstrumentTab`) dinyatakan **PASS (100% Lulus)**.

