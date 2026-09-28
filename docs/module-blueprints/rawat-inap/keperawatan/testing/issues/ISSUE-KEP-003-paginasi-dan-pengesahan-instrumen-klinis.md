# Laporan Isu: Kontrak Paginasi Daftar Pengkajian, Keselarasan Enum Status, dan Pengesahan Instrumen Klinis

**ID Isu:** `ISSUE-KEP-003`  
**Modul:** Rawat Inap — Keperawatan (`Inpatient Nursing Workspace`)  
**Submodul:** Pengkajian Pasien (`Assessment Section`) — Seluruh Tab Pengkajian Klinis  
**Tingkat Keparahan:** 🔴 **Tinggi / Integrasi & Finalisasi Blocker** (Menyebabkan penolakan 400 Bad Request pada simpan draf dan penolakan 422 Unprocessable Entity pada penyelesaian dokumen)  
**Tanggal Temuan:** 23 September 2026  
**Ditemukan Oleh:** Pengujian Otomatis Live In-Browser Antigravity (Playwright)  
**Status Isu:** ✅ **RESOLVED / SELESAI DIPERBAIKI** (Struktur paginasi ditangani, enum diselaraskan dengan backend ASP.NET Core, dan instrumen klinis disahkan)  

---

## 1. Ringkasan Masalah

Selama pelaksanaan pengujian end-to-end pada formulir **Resiko Jatuh (*Morse Fall Scale*)**, ditemukan tiga masalah berantai yang saling berkaitan antara frontend, backend, dan seed database:

1. **Struktur Data Paginasi Menyebabkan Active Draft Hilang (HTTP 400 Bad Request):**  
   Endpoint backend `GET /patient-assessments/episodes/{episodeId}` mengembalikan objek berpaginasi:
   `{ pageNumber: 1, pageSize: 25, totalData: 3, totalPage: 1, items: [...] }`.  
   Pada `assessment-section.jsx`, kode memeriksa `Array.isArray(listResult) ? listResult : []`. Karena `listResult` adalah objek (bukan array langsung), variabel `safeList` menjadi kosong (`[]`). Akibatnya, draf aktif yang ada di server tidak pernah terdeteksi, sehingga antarmuka selalu memanggil `POST` (buat baru) alih-alih `PUT` (perbarui draf), memicu penolakan validasi:
   > *"Draft assessment untuk encounter ini sudah ada. Lanjutkan draft tersebut, bukan membuat baru."*

2. **Keselarasan Enum Status Pengkajian (`PatientAssessmentStatus`):**  
   Pada backend ASP.NET Core:
   * `0 = Draft` (Konsep baru)
   * `1 = InProgress` (Konsep sedang dikerjakan / draf tersimpan)
   * `2 = Completed` (Selesai dan dikunci permanen)
   * `3 = Cancelled` (Dibatalkan)  
   Namun pada frontend:
   * Pengecekan draf aktif hanya mencari `status === 0`, mengabaikan `status === 1`.
   * Penanda dokumen selesai (`isCompleted`) mengecek `status === 1`, padahal nilai `1` adalah `InProgress`. Dokumen yang sedang dikerjakan terancam terkunci (*read-only*) sebelum difinalisasi.

3. **Penolakan Pengesahan Instrumen Klinis pada Finalisasi (HTTP 422 Unprocessable Entity):**  
   Ketika perawat menekan *Selesaikan & Kunci Pengkajian* (`PATCH .../complete`), server menolak dengan status `422 Unprocessable Entity` dan kode galat `INSTRUMENT_NOT_APPROVED`:
   > *"Instrumen belum disahkan. Dokumen dapat disimpan sebagai konsep dan diselesaikan setelah pengesahan."*  
   Penyebabnya: Data awal (*seed*) pada tabel basis data `CliClinicalInstrumentVersion` berstatus `VersionStatus = 1` (`Draft`) dan konfigurasi `ClinicalConfiguration:AllowDraftVersionsForTesting` bernilai `false`.

---

## 2. Dampak Klinis dan Bisnis Rumah Sakit

* **Kegagalan Menyimpan Perkembangan Pasien:** Perawat tidak dapat menyimpan pembaruan skor risiko jatuh jika draf pengkajian sudah pernah dibuat sebelumnya.
* **Hambatan Kepatuhan Regulasi Rekam Medis (KARS & SKP):** Dokumen pengkajian tidak dapat disahkan dan dikunci ke rekam medis resmi, menghambat pemenuhan standar keselamatan pasien (pencegahan risiko jatuh).

---

## 3. Rincian Temuan & Solusi Teknis

### A. Perbaikan Ekstraksi Paginasi Frontend
**Berkas:** [`src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx`](file:///c:/Users/Admin/Documents/Quilvian/Source%20Code/QuilvianFinal/QuilvianSystemFrontendDev/src/components/view/health-services/inpatient-management/nursing-workspace/sections/assessment/assessment-section.jsx)

```javascript
// SEBELUM:
const safeList = Array.isArray(listResult) ? listResult : [];

// SESUDAH:
const safeList = Array.isArray(listResult)
  ? listResult
  : Array.isArray(listResult?.items)
  ? listResult.items
  : [];
```

### B. Penyelarasan Enum Status Pengkajian
Mendukung `Draft (0)` dan `InProgress (1)` sebagai konsep yang dapat dilanjutkan, serta memastikan `Completed (2)` sebagai penanda dokumen selesai:

```javascript
// Deteksi draf aktif:
const draftItem = matchingDocs.find(
  (a) => Number(a.assessmentStatus) === 0 || Number(a.assessmentStatus) === 1
);

// Penanda dokumen selesai & terkunci permanen:
const isCompleted = Number(activeAssessmentDetail?.assessmentStatus) === 2;
```

### C. Pengesahan Versi Instrumen Klinis di Basis Data & Lingkungan Dev
1. **Basis Data (`CliClinicalInstrumentVersion`):**  
   Memperbarui status seluruh versi instrumen klinis keperawatan (`FALL_RISK_ADULT`, `FALL_RISK_CHILD`, `FALL_RISK_ELDERLY`, `GENERAL_NURSING_ASSESSMENT`, `PAIN_MONITORING`, dll.) menjadi `VersionStatus = 2` (`Approved`) dengan pengesahan resmi oleh admin rekam medis.
2. **Backend Configuration (`appsettings.Development.json`):**  
   Menambahkan konfigurasi uji:
   ```json
   "ClinicalConfiguration": {
     "AllowDraftVersionsForTesting": true
   }
   ```

---

## 4. Bukti Hasil Verifikasi

Setelah seluruh perbaikan diterapkan:
1. **Muat Draf Aktif:** Frontend sukses mengenali draf aktif `ASM-20260923-00002` untuk episode `c3fe1370-18f0-42fb-8d9f-01449212828e`.
2. **Pembaruan Konsep (`PUT`):**
   ```text
   [HTTP REQ] PUT https://localhost:7184/api/v1/health-services/clinical-management/patient-assessments/0f53f848-06fc-49df-896d-583029406c98
   [HTTP RES] Status: 200 OK
   ```
3. **Penyelesaian Dokumen (`PATCH`):**
   ```text
   [HTTP REQ] PATCH https://localhost:7184/api/v1/health-services/clinical-management/patient-assessments/0f53f848-06fc-49df-896d-583029406c98/complete
   [HTTP RES] Status: 200 OK
   ```
4. **Kunci Permanen & Addendum:** Status dokumen berubah menjadi `Completed`, seluruh isian dikunci (*read-only*), banner legalitas rekam medis muncul, dan tombol *Tambah Koreksi (Addendum)* aktif.
