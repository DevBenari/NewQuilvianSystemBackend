# Laporan Perubahan Frontend — `FE-RWI-076`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-RWI-076` |
| Judul | Tab Penunjang Medis (`FE-DOK-13`) — landing enam kartu; empat kartu "Integrasi belum tersedia" tanpa permintaan jaringan |
| Slice | Gelombang 2 — `DOK-MVP-FE-V2` |
| Roadmap | `docs/module-blueprints/rawat-inap/dokter-rawat-inap/roadmap/frontend-roadmap-v2.md` kartu `FE-RWI-076` |
| Trace | `FE-DOK-13` (menggantikan `FE-DOK-07`); `03-frontend-architecture.md` §10.4.8; `04-prd-to-mvp.md` §22.10 `DOK-17`, `FR-DOK-110`, `FR-DOK-111`, `UAT-DOK-51`, `UAT-DOK-52`; `RWI-DEC-108`, `RWI-DEC-113` |
| Contract version | `0.6.0` API Lab/Rad |
| Wewenang UI | `skema-tampilan-dokter-rawat-inap.md` §12; `03-frontend-architecture.md` §10.4.8; roadmap v2 kartu `FE-RWI-076` |
| Dependency | `FE-RWI-067` ✅ selesai (kerangka Ruang Kerja Dokter Rawat Inap `FE-DOK-09`) |
| Klasifikasi | `MEDIUM` — Rework Tab Penunjang dari dua seksi lama (`FE-DOK-07`) menjadi struktur landing enam layanan (`FE-DOK-13`), modal detail hasil, dan panel isolasi jaringan untuk empat layanan belum terintegrasi |
| Task mode | `FRONTEND` — backend strict read-only |
| Target tulis | `QuilvianSystemFrontendDev` untuk source; laporan ini dan tautan buktinya pada roadmap serta `requirement-traceability-v2.md` sub-modul yang sama |
| Tanggal | 17 September 2026 |
| Status | ✅ `SELESAI`. Keempat acceptance criteria terverifikasi dengan bukti uji otomatis, isolasi jaringan, build PASS, dan audit eslint |

---

## 1. Keadaan yang ditemukan di awal

Sebelum pengerjaan `FE-RWI-076`, Tab Penunjang pada ruang kerja dokter mengimplementasikan bentuk awal `FE-DOK-07` (dari task `FE-RWI-049`). Tampilan tersebut hanya merender dua seksi vertikal berturut-turut: Laboratorium dan Radiologi.

Keterbatasan yang ada:
1. Tidak ada landing enam kartu layanan penunjang (`RWI-DEC-108`, `RWI-DEC-113`).
2. Empat layanan penunjang rumah sakit lainnya — Gizi, Rehabilitasi Medik, Hemodialisa, dan Bank Darah — sama sekali belum dihadirkan di layar dokter.
3. Belum ada antarmuka "Lihat Detail Hasil" untuk memeriksa rincian parameter laboratorium beserta evaluasi rentang nilai rujukan (nilai Hb dan leukosit bertanda di atas/bawah rujukan sesuai `UAT-DOK-51`) maupun ekspertise radiologi.

---

## 2. Proses bisnis dari sisi pengguna

1. Dokter membuka tab **Penunjang Medis** pada ruang kerja dokter rawat inap (`FE-DOK-09`).
2. Dokter disambut dengan **Landing Enam Layanan Penunjang Medis**:
   - **Laboratorium**: menampilkan total pesanan, jumlah hasil final, ringkasan 2 pesanan terbaru, dan tombol "Buka Layanan".
   - **Radiologi**: menampilkan total pesanan, modalitas, hasil final, ringkasan 2 pesanan terbaru, dan tombol "Buka Layanan".
   - **Gizi / Konsultasi Gizi**: menampilkan konteks pasien, badge "Integrasi belum tersedia", dan tombol "Buka Layanan".
   - **Rehabilitasi Medik**: menampilkan konteks pasien, badge "Integrasi belum tersedia", dan tombol "Buka Layanan".
   - **Hemodialisa**: menampilkan konteks pasien, badge "Integrasi belum tersedia", dan tombol "Buka Layanan".
   - **Bank Darah**: menampilkan konteks pasien, badge "Integrasi belum tersedia", dan tombol "Buka Layanan".
3. Dokter dapat beralih cepat antar-layanan menggunakan navigasi segmen terpadu di bagian atas (`Semua (6 Layanan)`, `Laboratorium`, `Radiologi`, `Gizi`, `Rehab Medik`, `Hemodialisa`, `Bank Darah`).
4. Saat dokter membuka **Laboratorium** atau **Radiologi**:
   - Dokter melihat daftar pesanan lengkap perawatan pasien.
   - Pada baris pesanan, dokter dapat menekan tombol **Lihat Detail Hasil**:
     - Bila berstatus `FINAL`: modal menampilkan tabel parameter uji lengkap (mis. Hemoglobin, Leukosit, Hematokrit, Trombosit) dengan satuan, rentang rujukan normal, dan penanda evaluasi (`Normal`, `Di Atas Rujukan (High)`, `Di Bawah Rujukan (Low)`). Untuk radiologi, modal menampilkan temuan ekspertise dan kesimpulan dokter spesialis radiologi.
     - Bila berstatus `BELUM FINAL`: modal menampilkan peringatan keselamatan klinis bahwa hasil belum sah sebagai dasar keputusan klinis (`SUPPORTING_NOT_FINAL_WARNING`).
   - Dokter yang memiliki wewenang tulis dapat membuat pesanan baru lewat tombol "+ Pesan Lab" atau "+ Pesan Radiologi".
5. Saat dokter membuka salah satu dari empat layanan belum terintegrasi (**Gizi**, **Rehab Medik**, **Hemodialisa**, atau **Bank Darah**):
   - Layar menampilkan konteks pasien aktif (Nama, No. RM, No. Perawatan, Ruang/Bed, DPJP) melalui `ClinicalContextBar`.
   - Layar menampilkan panel informasi tegas **Integrasi Belum Tersedia** (`RWI-DEC-108` / `FR-DOK-111`).
   - **Nol Permintaan Jaringan**: Tidak ada panggilan Axios/fetch apa pun ke backend yang modulnya belum tersedia (`AC-4`).
   - Tidak ada formulir tiruan dan tidak ada tombol penyimpanan palsu.
   - Dokter dapat menekan tombol "← Semua Layanan Penunjang" untuk kembali ke ringkasan landing.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diubah dan dibuat

| Berkas | Status | Perubahan |
| :--- | :---: | :--- |
| `src/lib/constants/health-services/inpatient-management/inpatient-supporting-service-constants.jsx` | Ubah | Menambahkan `SUPPORTING_SERVICE_KEY`, array metadata `SUPPORTING_SERVICES` (6 layanan), konstanta `SUPPORTING_UNAVAILABLE_TEXT`, serta label teks landing dan detail hasil |
| `src/utils/health-services/inpatient-management/inpatient-supporting-service-utils.jsx` | Ubah | Menambahkan utilitas `getSupportingServiceMeta`, `countFinalResults`, `resolveLabParameterResults` (evaluasi nilai rujukan Hb/leukosit), dan `resolveRadReportDetails` |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-landing-grid.jsx` | Baru | Komponen landing 6 kartu layanan penunjang medis dengan ringkasan pesanan/hasil untuk Lab/Rad dan konteks pasien untuk 4 layanan belum terintegrasi |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-unavailable-panel.jsx` | Baru | Komponen panel layanan belum terintegrasi dengan konteks pasien (`ClinicalContextBar`), notifikasi `RWI-DEC-108`, dan jaminan isolasi jaringan 100% |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-result-detail-modal.jsx` | Baru | Modal rincian detail hasil pemeriksaan Lab (tabel parameter uji + rujukan) dan Radiologi (ekspertise + kesimpulan) beserta peringatan hasil belum final |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-order-section.jsx` | Ubah | Menambahkan dukungan prop `onBack` untuk navigasi kembali ke landing serta kolom aksi tombol "Lihat Detail Hasil" pada tabel pesanan Lab dan Radiologi |
| `src/components/view/health-services/inpatient-management/physician-workspace/tabs/supporting-service/supporting-service-tab.jsx` | Ubah | Mengintegrasikan seluruh komponen menjadi layar `FE-DOK-13`: navigasi segmen antarlayanan, tampilan landing, seksi Lab/Rad, panel unavailable, dan modal detail hasil |
| `src/style/health-services/inpatient-management/physician-supporting-service.module.css` | Ubah | Menambahkan token styling modular untuk grid 6 kartu, kartu layanan, panel unavailable, dan modal detail hasil responsif |
| `tests/unit/inpatient-supporting-service-v2.test.mjs` | Baru | 7 unit test memvalidasi AC-1 s.d. AC-4, UAT-DOK-51, isolasi jaringan 0 network call, dan penjagaan invariant `FE-RWI-049` |

### 3.2 Tabel keputusan base component

| Elemen UI | Sumber Komponen | Status | Rationale |
| :--- | :--- | :---: | :--- |
| **Header Tab & Navigasi Cepat** | `ClinicalPageHeader`, `ClinicalSegmentedNav` dari `@/components/ui/doctor-clinical-base` | `REUSE` | Menggunakan navigasi segmen terstandarisasi dokter clinical base tanpa mengubah perilaku default props |
| **Grid 6 Kartu Landing** | `BaseButton`, `ClinicalStatusBadge`, `ClinicalAuditBadge` | `COMPOSE` | Merangkai base button dan badge status yang sudah ada menjadi susunan kartu landing responsif |
| **Seksi Laboratorium & Radiologi** | `SupportingOrderSection` (`ClinicalDataTable`, `ClinicalStateBoundary`, `ClinicalActionGuard`) | `REUSE` | Mempertahankan batas state dan tabel pesanan penunjang dari `FE-RWI-049` |
| **Aksi & Modal "Lihat Detail Hasil"** | `BaseButton` (`variant="outline"`, `size="sm"`), `Modal` Bootstrap, `ClinicalAuditBadge`, `ClinicalDataTable` | `COMPOSE` | Menampilkan parameter rujukan hasil lab dan ekspertise radiologi dengan badge status evaluasi |
| **Panel Layanan Belum Terintegrasi** | `ClinicalContextBar`, `ClinicalAuditBadge`, `BaseButton` | `COMPOSE` | Merangkai bar konteks pasien dan kartu status "Integrasi belum tersedia" tanpa form tiruan |

> **UI GATE**: Seluruh elemen berstatus `REUSE` atau `COMPOSE`. Nol elemen `NEW` atau `EXTEND` yang mengubah default props base component.

---

## 4. Peta acceptance criteria

| AC | Deskripsi | Bukti Implementasi & Verifikasi |
| :--- | :--- | :--- |
| **AC-1** | Landing menampilkan enam kartu | `SUPPORTING_SERVICES` mendefinisikan 6 layanan penunjang; `SupportingLandingGrid` merender 6 kartu modular (`supporting-service-card-*`) pada `activeServiceKey === "all"`. Terverifikasi pada unit test `FE-RWI-076 AC-1` (PASS) |
| **AC-2** | Kartu Laboratorium dan Radiologi menampilkan jumlah pesanan, hasil final baru, dan detail hasil (`FR-DOK-110`) | Kartu menampilkan metrik `${orders.length} order • ${finalCount} hasil final`. Tombol "Lihat Detail Hasil" membuka `SupportingResultDetailModal` menampilkan rincian parameter Hb & leukosit bertanda di atas rujukan (`UAT-DOK-51`) serta ekspertise radiologi. Terverifikasi pada unit test `FE-RWI-076 AC-2 & UAT-DOK-51` (PASS) |
| **AC-3** | Empat kartu lain menampilkan konteks pasien dan "Integrasi belum tersedia" (`FR-DOK-111`) | Kartu Gizi, Rehab Medik, Hemodialisa, dan Bank Darah menampilkan badge "Integrasi belum tersedia". Saat dibuka, `SupportingUnavailablePanel` merender bar konteks pasien (`ClinicalContextBar`) dengan Nama, No. RM, No. Episode, Kamar/Bed, dan DPJP. Terverifikasi pada unit test `FE-RWI-076 AC-3` (PASS) |
| **AC-4** | Membuka keempat kartu itu menghasilkan **nol** permintaan jaringan ke modul terkait | `SupportingUnavailablePanel` murni statis, tidak memiliki `useEffect`, tidak mengimpor Axios, fetch, atau berkas `.service.js`. Membuka kartu murni mengalihkan state tampilan tanpa ada request jaringan. Terverifikasi pada unit test `FE-RWI-076 AC-4` (PASS) |

---

## 5. Hasil pengujian dan verifikasi

### A. Unit Testing (`tests/unit/inpatient-supporting-service-v2.test.mjs`)

```text
✔ FE-RWI-076 AC-1: Landing mendefinisikan tepat enam layanan penunjang medis (1.5499ms)
✔ FE-RWI-076 AC-2: Kartu Lab dan Rad menghitung jumlah pesanan dan hasil final baru (0.242ms)
✔ FE-RWI-076 AC-2 & UAT-DOK-51: Parameter hasil lab menampilkan nilai Hb dan leukosit dengan penanda rujukan (0.4897ms)
✔ FE-RWI-076 AC-2: Detail ekspertise dan temuan radiologi dihasilkan untuk hasil final (0.224ms)
✔ FE-RWI-076 AC-3: Empat kartu lain menampilkan konteks pasien dan 'Integrasi belum tersedia' (4.272ms)
✔ FE-RWI-076 AC-4: Membuka keempat kartu menghasilkan nol permintaan jaringan ke modul terkait (1.1146ms)
✔ FE-RWI-076 Invariant: Invariant FE-RWI-049 tetap terjaga pada seluruh berkas (3.3996ms)
ℹ tests 7
ℹ suites 0
ℹ pass 7
ℹ fail 0
ℹ cancelled 0
ℹ skipped 0
ℹ todo 0
ℹ duration_ms 15.9231
```

### B. Unit Testing Regresi Terkait

- `inpatient-physician-visit-regression.test.mjs`: 6/6 PASS.
- `inpatient-resume-payload.test.mjs`: 3/3 PASS.

### C. Lint Verification (`eslint . --quiet`)

- **Hasil:** Exit code 0, nol error pada seluruh berkas frontend.

### D. Production Build Verification (`next build`)

- **Hasil:** Next.js 16.2.12 (Turbopack) production build sukses dengan exit code 0. Standalone runtime siap dijalankan.

### E. Bukti Panel Jaringan (Network Isolation Verification)

Sesuai kriteria `AC-4` dan Definition of Done:
- Pada berkas `supporting-unavailable-panel.jsx`, dilakukan audit statis dan unit test:
  - `axios`: **Nol import / nol pemanggilan**.
  - `fetch()`: **Nol pemanggilan**.
  - `useEffect`: **Nol hook aktif**.
  - Service backend: **Nol ketergantungan**.
- Ketika pengguna menekan tombol "Buka Layanan" pada kartu Gizi, Rehabilitasi Medik, Hemodialisa, atau Bank Darah, peralihan tampilan terjadi secara lokal melalui state `activeServiceKey` di memori React. Panel Network pada peramban mencatat **0 (nol)** permintaan HTTP yang dikirim ke server.

---

## 6. Risiko dan isu terbuka

| # | Risiko / Isu | Dampak | Status |
| :--- | :--- | :--- | :--- |
| 1 | Integrasi modul Gizi, Rehab Medik, Hemodialisa, dan Bank Darah di masa depan | Saat modul-modul ini siap di backend, kartu-kartu penunjang perlu disambungkan ke endpoint API masing-masing | Terjadwal untuk rilis pasca-penyelarasan sesuai keputusan pemilik `RWI-DEC-108` dan `RWI-DEC-113` |

---

## 7. Dependency backend

Tidak ada dependency backend baru. Endpoint Laboratorium dan Radiologi yang digunakan adalah API `0.6.0` yang sudah berjalan (`BE-RWI-052` / `CAP-015`). Empat modul lainnya tidak memerlukan backend pada rilis ini.
