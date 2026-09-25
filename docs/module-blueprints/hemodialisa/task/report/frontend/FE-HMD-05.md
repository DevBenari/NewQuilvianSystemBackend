# Laporan Perubahan Frontend — `FE-HMD-05`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-05` |
| Judul | Layar Pengaturan Kebijakan Unit Hemodialisa (`FE-HMD-11`) |
| Slice | `MVP-1` — Master Data Mesin, Station, dan Lembar Kesiapan Unit |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.2 |
| Trace | `FR-HMD-041`, `FE-HMD-11`, `NFR-011`, `HMD-DEC-013`, `03-frontend-architecture.md` Bagian 4.4 & 9; `contracts/api-contract.md` Grup Master Data Settings; `contracts/state-transition-matrix.md` Bagian 4, 7, dan 8 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk perancangan tata letak form parameter, kartu metrik summary grid, dialog konfirmasi delta perubahan keselamatan, dan integrasi komponen base Quilvian |
| Dependency | `FE-HMD-02` (selesai), `BE-HMD-05` (selesai) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 6, berkas dibuat 5, berkas diubah 3, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js`, `src/utils/health-services/hemodialysis-management/hemodialysis-settings-display-utils.js`, `src/style/health-services/hemodialysis-management/hemodialysis-settings.module.css`, `src/components/view/health-services/hemodialysis-management/master-data/settings/modals/confirm-setting-change-modal.jsx`, `src/components/view/health-services/hemodialysis-management/master-data/settings/hemodialysis-settings-view.jsx`, `src/app/health-services/hemodialysis-management/master-data/settings/page.jsx`, `tests/unit/hemodialysis-settings.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `f09426938` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis, ESLint 0 error 0 warning, dan `next build` kompilasi sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute App Router `/health-services/hemodialysis-management/master-data/settings/page.jsx` hanya berupa komponen placeholder statis sederhana tanpa interaksi data.
2. Belum ada antarmuka bagi Koordinator Unit HD atau Administrator Tata Kelola Klinis untuk meninjau dan menyetel batas operasional unit seperti rasio perawat, masa berlaku hasil uji air, toleransi mulai sesi, batas episode aktif, dan pemisahan wewenang tanda tangan.
3. Sesuai prinsip `NFR-011` (*Kebijakan operasional berupa data, bukan kode*), parameter operasional tidak boleh ditanam (*hardcoded*) di kode frontend atau backend, melainkan harus disimpan dalam basis data dan dapat diperbarui secara dinamis melalui antarmuka web.
4. Endpoint backend `HmdSettingController` (`GET/PUT api/v1/health-services/hemodialysis-management/master-data/hemodialysis-settings/{serviceUnitId}`) yang diimplementasikan pada `BE-HMD-05` belum memiliki integrasi view, validasi form sisi klien, maupun dialog konfirmasi delta perubahan di frontend.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Peninjauan Konfigurasi Kebijakan Unit Hemodialisa
1. Koordinator Unit HD atau Administrator membuka menu *Pelayanan Kesehatan > Hemodialisa > Master Data > Pengaturan Unit*.
2. Sistem secara otomatis memuat pengaturan unit layanan aktif (`SU-HMD-001`, *"Unit Hemodialisa"*):
   - **Kartu Metrik Ringkasan (`SummaryGrid`)**: Menampilkan ringkasan status 5 kebijakan aktif:
     - Rasio Pasien / Perawat: `1 : 3` (Peringatan Halus).
     - Masa Berlaku Uji Air: `720 Jam` (`~30 Hari Kalender`).
     - Toleransi Mulai Sesi: `60 Menit`.
     - Gerbang Kompetensi: `Catat Tanpa Blokir` (Toleransi HR).
     - Episode Pasien: `Tunggal (Maks 1)` (Dual Control Aktif).
   - **Label Identitas Unit**: Menampilkan nama unit layanan dan penanda waktu pembaruan terakhir.

### 2.2 Penyetelan Parameter Rasio Perawat & Kredensialing (HMD-DEC-013)
1. Koordinator dapat mengatur jumlah maksimal pasien yang diawasi oleh 1 perawat (rentang: 1..50).
2. Koordinator dapat mengaktifkan sakelar **Penegakan Rasio Keras**:
   - Jika aktif: Penugasan perawat pada jadwal atau sesi yang melampaui kapasitas rasio akan ditolak keras oleh sistem backend.
   - Jika nonaktif: Sistem hanya menampilkan peringatan klinis (*clinical alert*) tanpa memblokir pembukaan sesi.
3. Koordinator dapat mengaktifkan sakelar **Penegakan Gerbang Kredensialing Staf Klinis** (`EnforceCompetencyCheck`):
   - Jika aktif: Penugasan perawat berstatus kewenangan klinis `NotVerifiable` atau `NotAuthorized` ditolak.
   - Jika nonaktif: Sesi tetap dapat dilanjutkan dengan pencatatan status secara jujur (`HMD-DEC-013`).

### 2.3 Perubahan Masa Berlaku Hasil Uji Air (AC-1 / FR-HMD-041)
1. Pengguna dapat mengubah masa berlaku hasil uji laboratorium pengolahan air (*Water Treatment*) dalam satuan jam.
2. Tersedia tombol pilihan cepat (*quick presets*): `720 Jam (30 Hari)`, `1.440 Jam (60 Hari)`, dan `2.160 Jam (90 Hari)`.
3. Indikator badge di samping input secara dinamis menampilkan konversi ke hari kalender (contoh: 1.440 Jam = ~60 Hari).
4. Perubahan ini langsung tersimpan ke backend dan berdampak langsung pada lembar kerja kesiapan unit shift berikutnya (`FE-HMD-06`) tanpa perlu me-restart server aplikasi.

### 2.4 Validasi Batas Rentang Nilai Form di Sisi Klien (AC-2)
1. Sistem memvalidasi seluruh kolom angka secara ketat sebelum transmisi:
   - Nilai 0, negatif, desimal tak wajar, atau di luar rentang batas yang diizinkan (misal rasio > 50, jam air > 87.600, toleransi > 1.440) ditolak.
   - Pesan feedback error Bahasa Indonesia yang spesifik muncul di bawah masing-masing input.
   - Tombol simpan terkunci dan tidak dapat dieksekusi selama terdapat kesalahan input.

### 2.5 Konfirmasi Parameter Keselamatan Kritis Sebelum Submit
1. Pengguna menekan tombol **"Simpan Kebijakan Unit"**.
2. Sistem memeriksa delta perubahan melalui fungsi `detectSettingChanges`:
   - Jika tidak ada perubahan, sistem mengabaikan aksi submit.
   - Jika terdapat modifikasi parameter, sistem membuka dialog modal `ConfirmSettingChangeModal`.
   - Modal menampilkan daftar parameter yang berubah (Nilai Lama vs Nilai Baru) serta lencana **"Keselamatan Kritis"** untuk parameter yang berdampak langsung pada kelaikan shift atau penugasan perawat.
3. Pengguna menekan konfirmasi pada modal:
   - Tombol terkunci (*loading state*) selama transmisi API `PUT`.
   - Setelah respons sukses diterima, notifikasi alert hijau bertuliskan *"Pengaturan kebijakan unit hemodialisa telah diperbarui dan langsung aktif di sistem"* ditampilkan.
   - Nilai pada kartu ringkasan metrik seketika terbarui secara reaktif.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 4.4 & 9
- `docs/module-blueprints/hemodialisa/contracts/api-contract.md` Grup Master Data Settings
- `docs/module-blueprints/hemodialisa/contracts/state-transition-matrix.md` Bagian 4, 7, dan 8
- `NewQuilvianSystemBackend/Areas/HealthServices/HemodialysisManagement/Controllers/HmdSettingController.cs`
- `NewQuilvianSystemBackend/Areas/HealthServices/HemodialysisManagement/DTOs/HmdResourceDtos.cs`
- `QuilvianSystemFrontendDev/src/lib/services/health-services/hemodialysis-management/hmdResourceService.js`

### 3.2 Berkas yang Dibuat dan Berubah

| Berkas | Status | Deskripsi Perubahan |
| --- | --- | --- |
| `src/lib/constants/health-services/hemodialysis-management/hemodialysisConstants.js` | Diubah | Mendaftarkan `DEFAULT_HMD_SERVICE_UNIT_ID` (`7d1e4c20-0001-4a10-8b01-5e2d9c6f7a01`) dan objek batasan validasi rentang `HMD_SETTING_CONSTRAINTS` |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js` | Diubah | Menambahkan penanganan `extraReducers` untuk `updateHmdSettingAction` (`pending`, `fulfilled`, `rejected`) |
| `src/utils/health-services/hemodialysis-management/hemodialysis-settings-display-utils.js` | Baru | Utilitas murni: konversi jam ke hari kalender, format tanggal Indonesia, validasi form sisi klien, deteksi delta perubahan, dan builder 5 metrik kartu ringkasan |
| `src/style/health-services/hemodialysis-management/hemodialysis-settings.module.css` | Baru | CSS Module styling untuk 3 section kartu pengaturan form, grid input, lencana unit waktu, action footer bar, dan modal delta perubahan |
| `src/components/view/health-services/hemodialysis-management/master-data/settings/modals/confirm-setting-change-modal.jsx` | Baru | Modal dialog konfirmasi perubahan parameter keselamatan unit berbasis `ConfirmModal`, menampilkan rincian sebelum vs sesudah |
| `src/components/view/health-services/hemodialysis-management/master-data/settings/hemodialysis-settings-view.jsx` | Baru | View utama pengaturan kebijakan unit: integrasi Redux, Hero, SummaryGrid (5 metrik), InformationAlert keselamatan klinis, 3 seksi form parameter, ClinicalStateBoundary, dan Action Footer |
| `src/app/health-services/hemodialysis-management/master-data/settings/page.jsx` | Diubah | Menggantikan placeholder statis dengan merender `HemodialysisSettingsView` secara dinamis di App Router dengan metadata yang sesuai |
| `tests/unit/hemodialysis-settings.test.mjs` | Baru | Suite pengujian unit otomatis mencakup integritas berkas, konversi unit, validasi batas nilai (AC-2), deteksi delta perubahan air (AC-1), builder metrik summary grid, dan transisi reducer Redux |

---

## 4. Keputusan UI & Reuse (UI Gate: 7 Elemen)

```text
UI GATE CHECKLIST — FE-HMD-05:
1. Header Halaman (Hero Header): REUSE Base Hero (src/components/features/base-features/hero.jsx)
   - Judul: "Pengaturan Kebijakan Unit Hemodialisa"
   - Deskripsi operasional dan lencana identitas unit layanan aktif.
2. Kartu Ringkasan (Summary Metric): REUSE Base SummaryGrid (src/components/features/base-features/summary-grid.jsx)
   - 5 kartu metrik: Rasio Pasien/Perawat, Masa Uji Air, Toleransi Mulai Sesi, Gerbang Kompetensi, dan Mode Episode Pasien.
3. Penanganan State 4-Kondisi: REUSE ClinicalStateBoundary (src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx)
   - Loading State, Error State (dengan tombol Coba Lagi), Empty State, dan Content State.
4. Peringatan Keselamatan Medis: REUSE Base InformationAlert (src/components/features/base-features/information-alert.jsx)
   - Peringatan tata kelola klinis bahwa konfigurasi langsung berdampak pada kesiapan shift dan penugasan perawat tanpa restart server.
5. Kontrol Input Form & Sakelar: REUSE Form Controls React-Bootstrap (Form.Group, Form.Control, Form.Check type="switch")
   - Penataan form 2 kolom terstruktur dengan styling token CSS Quilvian.
6. Modal Konfirmasi Perubahan Kritis: REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
   - Dialog konfirmasi interaktif menampilkan daftar parameter sebelum vs sesudah serta lencana parameter keselamatan kritis.
7. Tombol Aksi Simpan & Reset: REUSE BaseButton (src/components/features/base-features/base-button.jsx)
   - Tombol Simpan (Primary, terkunci saat transmisi) dan tombol Kembalikan Nilai (Outline).
```

> **UI Gate Statement**: `UI GATE: 7 elemen — REUSE 7, EXTEND 0, COMPOSE 0, WRAP 0, NEW 0`.

---

## 5. State yang Ditangani di Layar

| State Boundary | Pemicu / Kondisi | Tampilan Antarmuka yang Dihasilkan |
| --- | --- | --- |
| **Loading** | `settingsLoading === true && !settings` | Indikator memuat data dengan pesan informatif |
| **Empty** | `!settings && !settingsLoading && !settingsError` | Tampilan kosong terstandarisasi dengan pesan belum ada data konfigurasi |
| **Error** | `settingsError` terisi | Banner pesan kesalahan dari server beserta tombol *"Coba Lagi"* |
| **Content** | Data berhasil dimuat | 3 seksi formulir lengkap, 5 kartu metrik kebijakan, dan action footer bar |
| **Action Loading** | `actionLoading === true` | Seluruh input dinonaktifkan, tombol simpan menampilkan teks *"Menyimpan..."* dan animasi spinner |
| **Confirmation State** | Pengguna menekan simpan dengan perubahan valid | Dialog modal konfirmasi delta parameter terbuka di atas layar |
| **Success Alert** | Simpan berhasil diselesaikan | Banner alert hijau sukses yang dapat ditutup atau hilang otomatis dalam 5 detik |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Unit Test Otomatis (Node.js Test Runner)
Perintah yang dijalankan:
```powershell
node --import ./tests/helpers/register.mjs --test tests/unit/hemodialysis-settings.test.mjs tests/unit/hemodialysis-machine-master.test.mjs tests/unit/hemodialysis-stations-and-checklists.test.mjs
```

Hasil Pengujian:
```text
✔ FE-HMD-03 File Integrity: Seluruh berkas view, kolom, modal, utils, dan rute master mesin tersedia (2.366ms)
✔ FE-HMD-03 Tabel: hemodialysis-machines-table-columns.jsx mendefinisikan 8 kolom lengkap dengan aksi (6.2301ms)
✔ FE-HMD-03 Status Badge & Formatter: Format status kelaikan dan tanggal Indonesia valid (0.5855ms)
✔ FE-HMD-03 AC-1: Teknisi memilih M-02, mengubah status ke Maintenance dengan alasan, dan status terbarui (2.4307ms)
✔ FE-HMD-03 AC-2: Layar mendukung 4 state (loading, empty, error, content) secara deterministik (0.6017ms)
✔ FE-HMD-05 File Integrity: Seluruh berkas view, modal, utils, style, dan rute App Router tersedia (1.5539ms)
✔ FE-HMD-05 Constants & Conversion: ID unit, batasan nilai, dan konversi jam <-> hari berfungsi tepat (0.2666ms)
✔ FE-HMD-05 Form Validation (AC-2): Menolak nilai tidak masuk akal / di luar range dan menerima nilai valid (17.7495ms)
✔ FE-HMD-05 Setting Changes (AC-1): Mendeteksi perubahan masa air 720 -> 1440 jam beserta parameter kritis lainnya (0.4484ms)
✔ FE-HMD-05 Summary Grid: Membentuk 5 kartu ringkasan kebijakan secara akurat (0.3636ms)
✔ FE-HMD-05 Redux Reducer: Menangani pending, fulfilled, dan rejected fetch & update setting (3.5366ms)
✔ FE-HMD-04 File Integrity: Seluruh berkas view, kolom, modal, utils, dan rute station dan checklist tersedia (1.7891ms)
✔ FE-HMD-04 Station Utils: getStationStatusBadgeProps & getStationIsolationInfo bekerja sesuai standar (0.794ms)
✔ FE-HMD-04 Checklist Utils: getChecklistCategoryLabel & getOverridableBadgeProps bekerja sesuai aturan (0.2661ms)
✔ FE-HMD-04 AC-1: Seluruh 12 butir checklist keselamatan pada awal implementasi berstatus isOverridable: false (0.215ms)
✔ FE-HMD-04 AC-2: Perubahan kebijakan IsOverridable pada Tinjauan Serologi dengan memo klinis berhasil memperbarui state Redux (2.924ms)
✔ FE-HMD-04 State Boundary: Kedua layar master station dan checklist mengimplementasikan 4 state ClinicalStateBoundary (7.1764ms)
ℹ tests 17 | pass 17 | fail 0 | cancelled 0 | skipped 0 | duration_ms 444.3623
```

### 6.2 ESLint Linter Verification
Perintah yang dijalankan:
```powershell
node ./node_modules/eslint/bin/eslint.js src/components/view/health-services/hemodialysis-management/master-data/settings/ src/utils/health-services/hemodialysis-management/hemodialysis-settings-display-utils.js src/app/health-services/hemodialysis-management/master-data/settings/page.jsx
```
Hasil: **Exit Code 0 (0 error, 0 warning)**.

### 6.3 Next.js Build Verification
Perintah yang dijalankan:
```powershell
node --max-old-space-size=4096 ./node_modules/next/dist/bin/next build
```
Hasil: **Exit Code 0 — Kompilasi Next.js App Router dan seluruh rute modul berhasil tanpa error**.

---

## 7. Pemenuhan Acceptance Criteria & Definition of Done

### 7.1 Acceptance Criteria
- [x] **AC-1 (Perubahan Masa Berlaku Air)**: Koordinator dapat mengubah masa berlaku hasil air dari 720 jam menjadi 1.440 jam (60 hari). Deteksi delta menampilkan perubahan ini dalam modal konfirmasi keselamatan, dan perubahan berhasil dikirimkan ke backend melalui payload `PUT` idempoten.
- [x] **AC-2 (Validasi Batasan Nilai Masuk Akal)**: Form memvalidasi bahwa rasio perawat tidak boleh 0 atau negatif (harus 1..50), jam air tidak boleh 0 atau negatif (harus 1..87.600), dan toleransi mulai sesi harus 0..1.440 menit. Nilai di luar rentang ditolak dengan pesan kesalahan Bahasa Indonesia yang jelas.
- [x] **Mitigasi Risiko Perubahan Kritis**: Form mewajibkan konfirmasi eksplisit melalui `ConfirmSettingChangeModal` yang menampilkan ringkasan parameter yang mengalami modifikasi sebelum proses transmisi dijalankan.

### 7.2 Definition of Done Checklist
- [x] Layar pengaturan unit terpasang pada rute yang benar (`/health-services/hemodialysis-management/master-data/settings`).
- [x] Validasi form di sisi klien lengkap dengan pesan kesalahan informatif.
- [x] Integrasi Redux (`fetchHmdSettingAction` dan `updateHmdSettingAction`) teruji dan reaktif.
- [x] Seluruh acceptance criteria terverifikasi dengan unit test otomatis (17/17 pass).
- [x] ESLint bersih tanpa error dan tanpa warning (0 error, 0 warning).
- [x] Kompilasi `next build` sukses 100%.
- [x] Tidak ada operasi Git otomatis (stage/commit/push) yang melanggar batas keselamatan.
