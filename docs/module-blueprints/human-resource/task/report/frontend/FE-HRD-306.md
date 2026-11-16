# Laporan Penyelesaian Task: FE-HRD-306 Service API Riil, Normalisasi Paging Query & Anti-Fallback Menu 12 (separation-service.js)

## 1. Informasi Task
* **Task ID**: `FE-HRD-306`
* **Slice / MVP**: `S-C5-SEP` (Fondasi Kontrak Service & Komunikasi Backend)
* **Nama Fitur**: Service API Riil, Normalisasi Paging Query, Anti-Fallback Seluruh Menu 12 (`separation-service.js`)
* **Status**: Selesai (`✅ PASS`)
* **Tanggal Selesai**: 2026-11-16
* **Penanggung Jawab**: Frontend Engineering Agent

---

## 2. Ringkasan Eksekutif & Kebutuhan Bisnis Rumah Sakit
Dalam tata kelola operasional rumah sakit, ketepatan pertukaran data kepegawaian antara antarmuka pengguna (frontend) dan server backend (ASP.NET Core) sangat krusial. Pada modul pemisahan kerja (Menu 12: Pemberhentian & Pensiun), data yang dikirim maupun diterima menyangkut hak hukum ketenagakerjaan, kewajiban penyerahan aset medis/IT, serta hak keuangan pesangon pegawai.

Sebelum refactor dilakukan, service adapter memiliki potensi ketidaksesuaian kontrak seperti perbedaan penamaan parameter pagination (`page` vs `pageNumber`), adanya potensi fallback array lokal yang menutupi kondisi data kosong riil, serta penanganan error yang belum terstandarisasi. Melalui task `FE-HRD-306`, seluruh antarmuka service pada `src/lib/services/hr/separation-service.js` telah direfaktor secara tuntas:
1. **Normalisasi Dual Parameter Paging**: Helper `buildQueryString` dan `pageParams` kini secara deterministik mengirimkan parameter `page` dan `pageNumber` beriringan ke backend, menjamin kompatibilitas penuh dengan model binder DTO di seluruh endpoint lifecycle dan employee relations.
2. **Eliminasi Total Mock / Fallback**: Tidak ada lagi baris data fiktif atau fallback array di dalam layer service. Setiap pemanggilan API mengembalikan data asli yang dibungkus dalam envelope `ApiResponse<PagedResult<T>>` atau `ApiResponse<T>`.
3. **Penanganan Error HTTP Semantik**: Service meneruskan error status secara jujur (`400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict`, `422 Unprocessable Entity`, dan `500 Internal Server Error`) dengan mengekstrak pesan deskriptif dari backend agar dapat ditampilkan ke pengguna tanpa simulasi palsu.

---

## 3. Rincian Implementasi Teknis

### 3.1 Berkas yang Dimodifikasi
* `QuilvianSystemFrontendDev/src/lib/services/hr/separation-service.js`

### 3.2 Poin-Poin Perubahan Kode
1. **Normalisasi Query String & Paging**:
   ```javascript
   function buildQueryString(params = {}) {
     const query = new URLSearchParams();
     // Normalisasi page dan pageNumber
     const pageVal = params.pageNumber ?? params.page;
     if (pageVal !== undefined && pageVal !== null) {
       query.set('page', String(pageVal));
       query.set('pageNumber', String(pageVal));
     }
     if (params.pageSize) query.set('pageSize', String(params.pageSize));
     if (params.search) query.set('search', params.search);
     if (params.status) query.set('status', params.status);
     // Filter horizon akhir kontrak
     if (params.expiringInDays !== undefined && params.expiringInDays !== null) {
       query.set('expiringInDays', String(params.expiringInDays));
     }
     const qs = query.toString();
     return qs ? `?${qs}` : '';
   }
   ```
2. **Unwrap Envelope Baku `ApiResponse<T>`**:
   Method service melakukan ekstraksi data yang aman dan deterministik:
   ```javascript
   const response = await api.get(url);
   return response.data?.data ?? response.data;
   ```
3. **Penyelarasan Seluruh Rute Operasional Menu 12**:
   - `getResignations(params)` ➔ `/corporate/human-resource/lifecycle-management/resignation-requests`
   - `getRetirements(params)` ➔ `/corporate/human-resource/lifecycle-management/retirements`
   - `getExpiringContracts(params)` ➔ `/corporate/human-resource/employment-contracts`
   - `getSeparations(params)` ➔ `/hrd/separations` (atau rute Corporate terkait)
   - `getHandovers(params)` ➔ `/corporate/human-resource/lifecycle-management/handovers`
   - `getExitClearanceSummary(params)` & `getExitClearances(params)` ➔ `/corporate/human-resource/lifecycle-management/exit-clearances` serta `/hrd/exit-clearance`
   - `calculateSeverance(payload)` ➔ kalkulasi pesangon server-side melalui engine backend.

---

## 4. Spesifikasi Kontrak Endpoint Terkait

| Method | Endpoint Path | Tags | Deskripsi Fungsi | Parameter Utama | Respon Standar |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/corporate/human-resource/lifecycle-management/resignation-requests` | `[Tags("HR Lifecycle - Resignation")]` | Daftar pengajuan resign terpaginasi | `page`, `pageNumber`, `pageSize`, `status`, `search` | `ApiResponse<PagedResult<ResignationDto>>` |
| `GET` | `/api/corporate/human-resource/lifecycle-management/retirements` | `[Tags("HR Lifecycle - Retirement")]` | Daftar pengajuan pensiun & proyeksi | `page`, `pageNumber`, `pageSize`, `status` | `ApiResponse<PagedResult<RetirementCaseDto>>` |
| `GET` | `/api/corporate/human-resource/employment-contracts` | `[Tags("HR Contracts")]` | Monitoring kontrak jatuh tempo | `page`, `pageSize`, `expiringInDays` | `ApiResponse<PagedResult<EmploymentContractDto>>` |
| `GET` | `/api/hrd/separations` | `[Tags("HR Employee Separation")]` | Rekap kasus pemisahan hubungan kerja | `page`, `pageNumber`, `pageSize`, `separationType` | `ApiResponse<PagedResult<EmployeeSeparationDto>>` |
| `GET` | `/api/corporate/human-resource/lifecycle-management/handovers` | `[Tags("HR Lifecycle - Handover")]` | Daftar checklist serah terima aset/tugas | `page`, `pageSize`, `status` | `ApiResponse<PagedResult<OffboardingHandoverDto>>` |
| `GET` | `/api/hrd/exit-clearance/summary` | `[Tags("HR Exit Clearance")]` | Rekap metrik kliring multi-departemen | `separationId`, `status` | `ApiResponse<ExitClearanceSummaryDto>` |
| `POST` | `/api/hrd/separations/calculate-severance` | `[Tags("HR Employee Separation")]` | Kalkulator pesangon otoritatif backend | `employeeId`, `yearsOfService`, `separationReason` | `ApiResponse<SeveranceCalculationDto>` |

---

## 5. Bukti Verifikasi Mutu Rekayasa
* **Linter CI/CD**: `npx eslint src/lib/services/hr/separation-service.js` menghasilkan **0 error dan 0 warning**.
* **Integrasi Parameter**: Verifikasi runtime menunjukkan query parameter `pageNumber` dan `page` terkirim serentak tanpa saling menimpa.
* **Audit Zero Mock**: Tidak ditemukan variabel `mock`, `dummy`, `Date.now()` sebagai ID transaksi, atau fallback array di seluruh berkas service.
* **Traceability Requirement**: Memenuhi prasyarat fondasi untuk task `FE-HRD-307` s.d. `FE-HRD-312` serta skenario uji `UAT-M12-01` s.d. `UAT-M12-30`.
