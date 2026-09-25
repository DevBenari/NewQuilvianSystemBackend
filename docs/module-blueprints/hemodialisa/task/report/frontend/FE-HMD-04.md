# Laporan Perubahan Frontend — `FE-HMD-04`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-04` |
| Judul | Layar Master Station dan Master Butir Persiapan Overridable (`FE-HMD-09`, `FE-HMD-10`) |
| Slice | `MVP-1` — Master Data Mesin, Station, dan Lembar Kesiapan Unit |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.2 |
| Trace | `FR-HMD-051`, `FE-HMD-09`, `FE-HMD-10`, `HMD-ASM-001`, `HMD-GATE-002`, `03-frontend-architecture.md` Bagian 4.5, 4.6 & 9; `contracts/api-contract.md`; `contracts/state-transition-matrix.md` Bagian 6 |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk tata letak kolom, styling CSS module, dialog modal konfirmasi, dan integrasi komponen base Quilvian |
| Dependency | `FE-HMD-02`, `FE-HMD-03` (selesai), `BE-HMD-04`, `BE-HMD-05` (selesai) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 8, berkas dibuat 10, berkas diubah 2, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/hemodialysis-management/master-data/stations/**`, `src/components/view/health-services/hemodialysis-management/master-data/checklist-items/**`, `src/utils/health-services/hemodialysis-management/hemodialysis-stations-display-utils.js`, `src/utils/health-services/hemodialysis-management/hemodialysis-checklist-items-display-utils.js`, `src/style/health-services/hemodialysis-management/hemodialysis-stations.module.css`, `src/style/health-services/hemodialysis-management/hemodialysis-checklist-items.module.css`, `src/app/health-services/hemodialysis-management/master-data/stations/page.jsx`, `src/app/health-services/hemodialysis-management/master-data/checklist-items/page.jsx`, `tests/unit/hemodialysis-stations-and-checklists.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `f09426938` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis dan build Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute App Router `/health-services/hemodialysis-management/master-data/stations` dan `.../master-data/checklist-items` hanya berupa komponen placeholder statis sederhana.
2. Belum ada antarmuka inventaris station (tempat tidur/kursi dialisis) yang menampilkan nomor/kode station, ruangan, peruntukan isolasi fisik, status kelaikan (`Available`, `Maintenance`, `Blocked`), dan status aktif.
3. Belum ada dialog modal untuk mengubah status kelaikan station yang mewajibkan input alasan operasional.
4. Belum ada antarmuka konfigurasi 12 butir keselamatan persiapan pra-HD yang menampilkan kode, nama butir, kategori, tanda wajib, dan kebijakan boleh dilewati (`IsOverridable`).
5. Sesuai asumsi `HMD-ASM-001` dan `HMD-GATE-002`, seluruh 12 butir checklist keselamatan wajib terkunci secara baku (`IsOverridable: false`) pada awal implementasi dan hanya boleh diubah melalui otorisasi akun tata kelola klinis dengan nomor memo komite klinis / keputusan direktur.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Pengelolaan Master Station Hemodialisa (`FE-HMD-09`)
1. **Pemeriksaan Inventaris Station**: Koordinator Unit HD membuka menu *Hemodialisa > Master Data > Master Station HD*.
2. **Kartu Ringkasan Metrik**: Koordinator melihat ringkasan: Total Station, Tersedia (Available), Dalam Perawatan (Maintenance), Diblokir (Blocked), dan Khusus Isolasi Fisik.
3. **Penyaringan Data**: Pengguna dapat menyaring station berdasarkan ruangan, status kelaikan, atau peruntukan isolasi fisik.
4. **Perubahan Status Kelaikan Station**:
   - Pengguna menekan tombol "Ubah Status" pada baris station.
   - Sistem membuka modal `ChangeStationStatusModal`.
   - Pengguna memilih status baru dan mengisi teks alasan (minimal 5 karakter, contoh: *"Pembersihan dan sterilisasi station pasca sesi"*).
   - Setelah konfirmasi, status station pada tabel dan kartu metrik seketika terbarui.
5. **Perlindungan Privasi Isolasi**: Station yang dilengkapi tekanan negatif (*negative pressure*) atau alokasi isolasi fisik menampilkan lencana netral **"Khusus Isolasi"** tanpa menyebutkan indikasi diagnosis klinis pasien.

### 2.2 Penguncian Baku 12 Butir Checklist Keselamatan Pra-HD (AC-1)
1. Penanggung jawab unit membuka menu *Hemodialisa > Master Data > Master Butir Persiapan*.
2. Sistem memuat daftar 12 butir checklist keselamatan persiapan:
   - `CHK-01`: Informed Consent Hemodialisa
   - `CHK-02`: Pemeriksaan Tanda Vital Pra-HD
   - `CHK-03`: Tinjauan Hasil Laboratorium & Serologi
   - `CHK-04`: Penilaian Berat Badan & Target Ultrafiltrasi
   - `CHK-05`: Pemeriksaan Jalur Akses Vaskular (AV Fistula / Graft / CDL)
   - `CHK-06`: Kelaikan Mesin HD & Sirkulasi Ekstrakorporeal
   - `CHK-07`: Kesesuaian Dialiser & Cairan Dialisat
   - `CHK-08`: Protokol Antikoagulan (Heparin / Bebas Heparin)
   - `CHK-09`: Kesiapan Obat & Suplementasi Intradialitik
   - `CHK-10`: Jalur Darah & Sensor Tekanan Udara Bebas
   - `CHK-11`: Verifikasi Identitas & Gelang Pasien
   - `CHK-12`: Kesiapan Tindakan Darurat & Resusitasi
3. **Verifikasi AC-1**: Pada awal implementasi, seluruh 12 butir checklist menampilkan lencana dan status kebijakan **"Wajib Mutlak (Tidak Boleh Dilewati)"** (`isOverridable: false`).

### 2.3 Perubahan Kebijakan Overridable oleh Admin Tata Kelola Klinis (AC-2)
1. Admin Tata Kelola Klinis (pemegang hak akses `HemodialysisChecklistItem : SetOverridable`) memilih butir `CHK-03` ("Tinjauan Hasil Laboratorium & Serologi").
2. Admin menekan tombol "Izinkan Boleh Dilewati".
3. Sistem menampilkan dialog modal `SetChecklistOverridableModal`:
   - Menampilkan peringatan tata kelola klinis (`HMD-ASM-001` & `HMD-GATE-002`).
   - Admin memasukkan referensi memo komite klinis: *"Sesuai Keputusan Direktur No. 102 tentang Kebijakan Pelayanan Hemodialisa"*.
   - Admin mencentang konfirmasi pernyataan kewenangan klinis.
4. Admin menekan tombol "Tetapkan Boleh Dilewati":
   - Tombol simpan terkunci selama proses transmisi API (`PATCH .../overridable`).
   - Status butir `CHK-03` terbukti berubah menjadi chip hijau bertuliskan **"Boleh Dilewati Dokter"**.
   - Perubahan tersimpan secara transaksional di backend tanpa perlu mengubah source code aplikasi.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 4.5, 4.6 & 9
- `docs/module-blueprints/hemodialisa/contracts/api-contract.md`
- `docs/module-blueprints/hemodialisa/contracts/state-transition-matrix.md` Bagian 6
- `QuilvianSystemFrontendDev/src/lib/services/health-services/hemodialysis-management/hmdResourceService.js`
- `QuilvianSystemFrontendDev/src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js`

### 3.2 Berkas yang Dibuat dan Berubah

| Berkas | Status | Deskripsi Perubahan |
| --- | --- | --- |
| `src/style/health-services/hemodialysis-management/hemodialysis-stations.module.css` | Baru | CSS Module styling untuk halaman master station, form radio status, lencana isolasi netral, dan kartu ringkasan |
| `src/utils/health-services/hemodialysis-management/hemodialysis-stations-display-utils.js` | Baru | Pure utility functions: resolver badge status station (`Available`, `Maintenance`, `Blocked`), label isolasi netral, dan row id generator |
| `src/components/view/health-services/hemodialysis-management/master-data/stations/hemodialysis-stations-table-columns.jsx` | Baru | Konfigurasi 8 kolom tabel master station: No, Kode Station, Nama Station, Ruangan, Peruntukan Isolasi, Status Kelaikan, Status Aktif, dan Aksi |
| `src/components/view/health-services/hemodialysis-management/master-data/stations/modals/change-station-status-modal.jsx` | Baru | Modal ubah status kelaikan operasional station berbasis `ConfirmModal` dengan validasi alasan minimal 5 karakter |
| `src/components/view/health-services/hemodialysis-management/master-data/stations/modals/create-edit-station-modal.jsx` | Baru | Modal pendaftaran dan penyuntingan station dengan form isolasi fisik dan tekanan negatif |
| `src/components/view/health-services/hemodialysis-management/master-data/stations/hemodialysis-stations-view.jsx` | Baru | View utama master station: Redux integration, Hero, SummaryGrid (5 metrik), DataFilter, ClinicalStateBoundary (4 state), dan DataTable + Pagination |
| `src/app/health-services/hemodialysis-management/master-data/stations/page.jsx` | Diubah | Menggantikan placeholder dengan merender `HemodialysisStationsView` secara dinamis di App Router |
| `src/style/health-services/hemodialysis-management/hemodialysis-checklist-items.module.css` | Baru | CSS Module styling untuk halaman master checklist items, lencana wajib, lencana overridable, form memo klinis, dan notice box |
| `src/utils/health-services/hemodialysis-management/hemodialysis-checklist-items-display-utils.js` | Baru | Pure utility functions: resolver label kategori Bahasa Indonesia, resolver status overridable, dan row id generator |
| `src/components/view/health-services/hemodialysis-management/master-data/checklist-items/hemodialysis-checklist-items-table-columns.jsx` | Baru | Konfigurasi 7 kolom tabel master butir checklist: No, Kode, Nama Butir, Kategori, Kewajiban (Badge WAJIB), Kebijakan Boleh Dilewati, dan Aksi |
| `src/components/view/health-services/hemodialysis-management/master-data/checklist-items/modals/set-checklist-overridable-modal.jsx` | Baru | Modal otorisasi perubahan kebijakan overridable berbasis `ConfirmModal`, mewajibkan referensi memo komite klinis dan checkbox credential |
| `src/components/view/health-services/hemodialysis-management/master-data/checklist-items/modals/create-edit-checklist-item-modal.jsx` | Baru | Modal pendaftaran dan penyuntingan butir checklist baru |
| `src/components/view/health-services/hemodialysis-management/master-data/checklist-items/hemodialysis-checklist-items-view.jsx` | Baru | View utama master checklist: Redux integration, Hero, SummaryGrid (5 metrik), DataFilter, ClinicalStateBoundary (4 state), dan DataTable + Pagination |
| `src/app/health-services/hemodialysis-management/master-data/checklist-items/page.jsx` | Diubah | Menggantikan placeholder dengan merender `HemodialysisChecklistItemsView` secara dinamis di App Router |
| `tests/unit/hemodialysis-stations-and-checklists.test.mjs` | Baru | Suite pengujian otomatis mencakup integritas berkas, kolom & utils station/checklist, AC-1 penguncian awal 12 butir, AC-2 transisi overridable via memo klinis, dan 4 state boundary |

---

## 4. Keputusan UI & Reuse (UI Gate: 8 Elemen)

```text
UI GATE CHECKLIST — FE-HMD-04:
1. Frame Halaman (Hero Header): REUSE Base Hero (src/components/features/base-features/hero.jsx)
   - Station: "Master Station Hemodialisa" + "+ Tambah Station"
   - Checklist: "Master Butir Persiapan (Checklist Pra-HD)" + "+ Tambah Butir Checklist"
2. Kartu Ringkasan (Summary Metric): REUSE Base SummaryGrid (src/components/features/base-features/summary-grid.jsx)
   - Station: 5 kartu (Total Station, Tersedia, Dalam Perawatan, Diblokir, Khusus Isolasi)
   - Checklist: 5 kartu (Total Butir, Wajib Mutlak, Boleh Dilewati, Kategori Klinis, Kategori Mesin)
3. Kontrol Pencarian & Filter: REUSE Base DataFilter & FilterSelect
   - Pencarian kata kunci dan dropdown filter terpadu
4. Tabel Data & Paginasi: REUSE Base DataTable & Base Pagination
   - Station: 8 kolom terstruktur
   - Checklist: 7 kolom terstruktur
5. Modal Ubah Status Station: REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
   - Pilihan radio status kelaikan dan textarea alasan
6. Modal Otorisasi Kebijakan Overridable: REUSE Base ConfirmModal
   - Input nomor memo komite klinis dan checkbox credential otorisasi
7. Modal Tambah / Sunting: REUSE Base Modal & BaseButton
   - Formulir terstruktur dengan validasi input
8. State Boundary: REUSE ClinicalStateBoundary (src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx)
   - Loading State, Empty State (Nihil & Filter), Error State (Coba Lagi), Content State
```

---

## 5. State yang Ditangani di Layar

| State Boundary | Pemicu / Kondisi | Tampilan Antarmuka yang Dihasilkan |
| --- | --- | --- |
| **Loading** | `stationsLoading / checklistItemsLoading === true` | Skeleton loader animasi dengan pesan informatif |
| **Empty (Belum Ada Data)** | Data kosong tanpa filter aktif | Ilustrasi kosong dengan pesan informatif dan tombol aksi tambah pertama |
| **Empty (Hasil Filter Kosong)** | Data kosong dengan filter aktif | Ilustrasi kosong dengan pesan penyaring tidak cocok dan tombol atur ulang |
| **Error** | Error state terisi | Pesan kegagalan dari server dan tombol *"Coba Lagi"* |
| **Content (Berisi Data)** | Data tersedia | Tabel data lengkap beserta paginasi dan tombol aksi interaktif |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Unit Test Otomatis (Node.js Test Runner)
Perintah yang dijalankan:
```powershell
node --loader ./tests/support/module-alias-loader.mjs --test tests/unit/hemodialysis-sidebar-navigation.test.mjs tests/unit/hemodialysis-client-and-redux-slice.test.mjs tests/unit/hemodialysis-machine-master.test.mjs tests/unit/hemodialysis-stations-and-checklists.test.mjs
```

Hasil Pengujian:
```text
✔ FE-HMD-02 File Integrity: Seluruh berkas services, slices, constants, dan hook tersedia
✔ FE-HMD-02 AC-1 & Helper: unwrapApiResponse membongkar ApiResponse<T> format camelCase dan PascalCase
✔ FE-HMD-02 AC-1: normalizeHmdError mendeteksi HTTP 423 Locked dan memandu ke Addendum Rekam Medis
✔ FE-HMD-02 Helper: normalizeHmdError mendeteksi HTTP 409 Conflict dan pesan bentrok data
✔ FE-HMD-02 AC-2: Hook status transisi menonaktifkan tombol aksi tidak sah pada sesi InProgress
✔ FE-HMD-02 AC-2: Sesi Finalized berstatus terkunci dan hanya membuka opsi Addendum
✔ FE-HMD-02 Aturan Resep & Mesin: Resep Active tidak dapat diedit; Mesin aktif tidak dapat diblokir
✔ FE-HMD-02 Redux Slice: hemodialysisWorklistSlice mengelola filter dan state kerja
✔ FE-HMD-02 Redux Slice: hemodialysisSessionSlice mitigasi stale state antar pasien via resetSessionState
✔ FE-HMD-02 Redux Slice: hemodialysisMasterSlice mengelola data mesin, station, dan checklist
✔ FE-HMD-03 File Integrity: Seluruh berkas view, kolom, modal, utils, dan rute master mesin tersedia
✔ FE-HMD-03 Tabel: hemodialysis-machines-table-columns.jsx mendefinisikan 8 kolom lengkap dengan aksi
✔ FE-HMD-03 Status Badge & Formatter: Format status kelaikan dan tanggal Indonesia valid
✔ FE-HMD-03 AC-1: Teknisi memilih M-02, mengubah status ke Maintenance dengan alasan, dan status terbarui
✔ FE-HMD-03 AC-2: Layar mendukung 4 state (loading, empty, error, content) secara deterministik
✔ FE-HMD-01 AC-1: menu-items.jsx mendaftarkan grup Hemodialisa dengan 6 submenu dan 4 subitems master data
✔ FE-HMD-01 AC-2: Seluruh 9 rute navigasi Hemodialisa memiliki berkas page.jsx valid tanpa 404
✔ FE-HMD-01 AC-2: Resolver sidebar mengenali seluruh rute Hemodialisa dan menghasilkan path navigasi yang benar
✔ FE-HMD-01 AC-3: Pengguna Koordinator HD melihat seluruh menu Hemodialisa lengkap
✔ FE-HMD-01 AC-3: Pengguna Staf Bangsal tidak melihat menu internal unit (Kesiapan Unit dan Master Data)
✔ FE-HMD-01 AC-3: Filter berbasis permissions menyaring menu sesuai klaim hak akses
✔ FE-HMD-04 File Integrity: Seluruh berkas view, kolom, modal, utils, dan rute station dan checklist tersedia
✔ FE-HMD-04 Station Utils: getStationStatusBadgeProps & getStationIsolationInfo bekerja sesuai standar
✔ FE-HMD-04 Checklist Utils: getChecklistCategoryLabel & getOverridableBadgeProps bekerja sesuai aturan
✔ FE-HMD-04 AC-1: Seluruh 12 butir checklist keselamatan pada awal implementasi berstatus isOverridable: false
✔ FE-HMD-04 AC-2: Perubahan kebijakan IsOverridable pada Tinjauan Serologi dengan memo klinis berhasil memperbarui state Redux
✔ FE-HMD-04 State Boundary: Kedua layar master station dan checklist mengimplementasikan 4 state ClinicalStateBoundary
ℹ tests 27 | pass 27 | fail 0 | cancelled 0 | skipped 0
```

### 6.2 ESLint Linter Verification
Perintah yang dijalankan:
```powershell
node ./node_modules/eslint/bin/eslint.js "src/components/view/health-services/hemodialysis-management/master-data/stations/**/*.jsx" "src/components/view/health-services/hemodialysis-management/master-data/checklist-items/**/*.jsx" "src/utils/health-services/hemodialysis-management/hemodialysis-stations-display-utils.js" "src/utils/health-services/hemodialysis-management/hemodialysis-checklist-items-display-utils.js" "src/app/health-services/hemodialysis-management/master-data/stations/page.jsx" "src/app/health-services/hemodialysis-management/master-data/checklist-items/page.jsx"
```
Hasil: **Exit Code 0 (0 error, 0 warning)**.

---

## 7. Pemenuhan Acceptance Criteria & Definition of Done

### 7.1 Acceptance Criteria
- [x] **AC-1 (Penguncian Checklist di Awal)**: Pada awal implementasi, seluruh 12 butir checklist menampilkan sakelar `IsOverridable` dalam keadaan nonaktif (false / Wajib Mutlak).
- [x] **AC-2 (Perubahan Kebijakan Overridable via Memo Klinis)**: Admin tata kelola klinis mengaktifkan sakelar pada butir "Tinjauan Serologi", memasukkan catatan "Sesuai Keputusan Direktur No. 102", dan menyimpan. Status berubah dan tombol simpan terkunci selama proses transmisi.
- [x] **State Boundary**: Kedua layar mengimplementasikan 4 state `ClinicalStateBoundary` secara deterministik.

### 7.2 Definition of Done Checklist
- [x] Kedua halaman master terpasang pada sub-rute yang benar (`/master-data/stations` dan `/master-data/checklist-items`).
- [x] Form dan modal konfirmasi berfungsi sempurna dengan validasi input.
- [x] Privasi isolasi netral diterapkan tanpa menyebut diagnosis serologi.
- [x] Pengujian unit otomatis lengkap dan lulus 100% (27/27 pass).
- [x] ESLint bersih tanpa error dan tanpa warning.
- [x] Tidak ada commit Git otomatis atau pelanggaran batas wewenang.
