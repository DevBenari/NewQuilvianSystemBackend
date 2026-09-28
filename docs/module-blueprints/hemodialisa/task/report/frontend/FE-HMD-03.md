# Laporan Perubahan Frontend — `FE-HMD-03`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-HMD-03` |
| Judul | Layar Master Data Mesin HD dan Riwayat Status Kelaikan Mesin |
| Slice | `MVP-1` — Master Data, Pendaftaran Mesin, dan Station Hemodialisa |
| Roadmap | `docs/module-blueprints/hemodialisa/roadmap/frontend-roadmap.md` bagian 4.2 |
| Trace | `03-frontend-architecture.md` Bagian 8.1 & 9; `02-hospital-domain-architecture.md` Bagian 5.5; `contracts/api-contract.md`; `contracts/state-transition-matrix.md` |
| Contract version | `HMD-CONTRACT-v1` — status `approved` (18 September 2026) |
| Wewenang UI | `DEV_DISCRETION` untuk penataan layout, modal konfirmasi, styling CSS module, dan integrasi komponen base Quilvian |
| Dependency | `FE-HMD-02` (selesai) |
| Klasifikasi | `MEDIUM` — skor 7: repository 0, berkas diperiksa 6, berkas dibuat 6, berkas diubah 2, logika 2, kontrak API 2, database 0, UI 2 |
| Task mode | `FRONTEND` |
| Target tulis | `QuilvianSystemFrontendDev` — `src/components/view/health-services/hemodialysis-management/master-data/machines/**`, `src/utils/health-services/hemodialysis-management/hemodialysis-machines-display-utils.js`, `src/style/health-services/hemodialysis-management/hemodialysis-machines.module.css`, `src/app/health-services/hemodialysis-management/master-data/machines/page.jsx`, `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js`, `tests/unit/hemodialysis-machine-master.test.mjs` |
| Model | Gemini 3.8 Flash |
| Commit frontend saat dikerjakan | `f09426938` pada branch `HamzahV2` |
| Commit backend yang dijadikan rujukan | `89028993` pada branch `MHamzah` |
| Tanggal | 22 September 2026 |
| Status | ✅ Selesai — seluruh acceptance criteria terverifikasi dengan unit test otomatis dan build Next.js sukses |

---

## 1. Keadaan yang Ditemukan di Awal

Sebelum task ini dikerjakan:
1. Rute App Router `/health-services/hemodialysis-management/master-data/machines` hanya berupa berkas placeholder sederhana tanpa antarmuka pengguna fungsional.
2. Belum ada tabel inventaris mesin cuci darah yang menampilkan nomor/kode mesin, merek, serial number, status kelaikan operasional, peruntukan isolasi, dan riwayat kelaikan.
3. Belum ada dialog modal terintegrasi bagi teknisi elektromedis atau perawat koordinator untuk mengubah status kelaikan mesin (`Ready`, `Maintenance`, `Blocked`, `NotEligible`) dengan kewajiban mencantumkan alasan klinis/teknis.
4. Belum ada panel jejak audit kronologis yang menampilkan riwayat perubahan status kelaikan mesin beserta waktu, pengubah, dan alasan perubahan.
5. Belum ada perlindungan privasi data serologi pada daftar mesin: mesin berstatus isolasi berisiko membocorkan label diagnostik infeksius (seperti HBsAg atau Anti-HCV) alih-alih label netral non-stigmatisasi.

---

## 2. Proses Bisnis dari Sisi Pengguna (User Journey)

### 2.1 Alur Pengubahan Status Kelaikan Mesin (AC-1)
1. **Pemeriksaan Daftar Mesin**: Teknisi Elektromedis Rumah Sakit membuka menu *Hemodialisa > Master Data > Master Mesin HD*.
2. **Pemilihan Mesin**: Teknisi melihat inventaris mesin cuci darah di unit HD, menemukan mesin `M-02` (Fresenius 4008S, SN-8726152) yang saat ini berstatus *Ready* (Siap Operasional).
3. **Membuka Dialog Status**: Teknisi menekan tombol aksi "Ubah Status" pada baris mesin `M-02`.
4. **Validasi dan Pengisian Alasan**:
   - Sistem membuka modal `ChangeMachineStatusModal`.
   - Teknisi memilih opsi status baru: `Maintenance` (Dalam Perawatan).
   - Teknisi memasukkan alasan klinis/teknis: *"Penggantian filter ultrafiltrasi berkala"*.
   - Validasi antarmuka mewajibkan alasan minimal 5 karakter sebelum tombol simpan dapat diaktifkan.
5. **Konfirmasi & Pembaruan Data**:
   - Teknisi menekan tombol "Simpan Perubahan".
   - Sistem memanggil `changeHmdMachineStatusAction` ke backend.
   - Status mesin `M-02` pada tabel seketika berubah menjadi chip kuning bertuliskan *Dalam Perawatan*.
   - Kartu ringkasan metrik status di bagian atas layar otomatis memperbarui jumlah mesin dalam perawatan.

### 2.2 Alur Audit Jejak Riwayat Status Mesin (AC-1)
1. Koordinator Unit HD atau Teknisi menekan tombol "Riwayat" pada mesin `M-02`.
2. Sistem membuka modal `MachineStatusHistoryModal` dan memuat jejak audit dari backend.
3. Lini masa kronologis (`ClinicalTimeline`) menampilkan entri terbaru di posisi teratas:
   - Status: *Dalam Perawatan* (`Maintenance`)
   - Alasan: *"Penggantian filter ultrafiltrasi berkala"*
   - Pengubah & Waktu: Teridentifikasi dengan timestamp standar Indonesia (WIB).

### 2.3 Perlindungan Privasi Serologi (03-Frontend-Architecture Bagian 9)
- Mesin yang dialokasikan khusus untuk pasien infeksius (HBsAg / HCV / HIV) ditampilkan dengan lencana netral bertuliskan **"Perlu Isolasi"** (warna oranye) tanpa menyebutkan nama virus atau diagnosis infeksi.
- Hal ini menjaga kerahasiaan medis pasien dan mencegah stigmatisasi di area perawatan terbuka unit hemodialisa.

---

## 3. Perubahan yang Dikerjakan

### 3.1 Berkas yang Diperiksa
- `docs/module-blueprints/hemodialisa/03-frontend-architecture.md` Bagian 8.1 & 9
- `docs/module-blueprints/hemodialisa/02-hospital-domain-architecture.md` Bagian 5.5
- `docs/module-blueprints/hemodialisa/contracts/api-contract.md`
- `docs/module-blueprints/hemodialisa/contracts/state-transition-matrix.md`
- `QuilvianSystemFrontendDev/src/components/features/base-features/data-table.jsx`
- `QuilvianSystemFrontendDev/src/components/features/base-features/hero.jsx`
- `QuilvianSystemFrontendDev/src/components/features/base-features/summary-grid.jsx`
- `QuilvianSystemFrontendDev/src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx`
- `QuilvianSystemFrontendDev/src/components/ui/doctor-clinical-base/ClinicalTimeline.jsx`

### 3.2 Berkas yang Dibuat dan Berubah

| Berkas | Status | Deskripsi Perubahan |
| --- | --- | --- |
| `src/style/health-services/hemodialysis-management/hemodialysis-machines.module.css` | Baru | CSS Module styling untuk halaman master mesin, radio group status kelaikan, form perubahan status, modal layout, dan lencana isolasi netral |
| `src/utils/health-services/hemodialysis-management/hemodialysis-machines-display-utils.js` | Baru | Pure utility functions: format tanggal/waktu Indonesia, resolver badge status kelaikan, dan row id generator |
| `src/components/view/health-services/hemodialysis-management/master-data/machines/hemodialysis-machines-table-columns.jsx` | Baru | Konfigurasi 8 kolom tabel master mesin: No, Kode Mesin, Nama & Merek, Nomor Seri, Status Kelaikan, Peruntukan Isolasi, Terakhir Berubah, dan Aksi |
| `src/components/view/health-services/hemodialysis-management/master-data/machines/modals/change-machine-status-modal.jsx` | Baru | Dialog modal ubah status kelaikan mesin terintegrasi `ConfirmModal` dan hook `useHemodialysisStatusTransition`, validasi alasan minimal 5 karakter |
| `src/components/view/health-services/hemodialysis-management/master-data/machines/modals/machine-status-history-modal.jsx` | Baru | Dialog modal riwayat audit kelaikan mesin menggunakan `ClinicalTimeline` dan `ClinicalTimelineItem` |
| `src/components/view/health-services/hemodialysis-management/master-data/machines/modals/create-edit-machine-modal.jsx` | Baru | Dialog modal pendaftaran dan penyuntingan master mesin baru |
| `src/components/view/health-services/hemodialysis-management/master-data/machines/hemodialysis-machines-view.jsx` | Baru | View utama master mesin: Redux integration, Hero, SummaryGrid (5 kartu status), DataFilter, ClinicalStateBoundary, DataTable + Pagination, dan 3 modals |
| `src/app/health-services/hemodialysis-management/master-data/machines/page.jsx` | Diubah | Menggantikan placeholder dengan merender `HemodialysisMachinesView` secara dinamis di App Router |
| `src/lib/state/slice/health-services/hemodialysis-management/hemodialysisMasterSlice.js` | Diubah | Membersihkan pesan error saat `fetchHmdMachines.fulfilled` berhasil dimuat |
| `tests/support/module-alias-loader.mjs` | Diubah | Mendukung resolusi path relatif tanpa ekstensi untuk Node.js ESM test runner |
| `tests/unit/hemodialysis-machine-master.test.mjs` | Baru | Suite pengujian otomatis mencakup integritas berkas, 8 kolom tabel, badge/formatter, pengujian alur AC-1, dan 4 state boundary AC-2 |

---

## 4. Keputusan UI & Reuse (UI Gate: 8 Elemen)

Sesuai aturan panduan pengembangan frontend Quilvian, setiap elemen antarmuka dievaluasi secara eksplisit dengan prinsip *reuse first*:

```text
UI GATE CHECKLIST — FE-HMD-03:
1. Frame Halaman (Hero Header): REUSE Base Hero (src/components/features/base-features/hero.jsx)
   - Eyebrow: "Pelayanan Kesehatan / Hemodialisa"
   - Title: "Master Mesin Hemodialisa"
   - Action: Tombol "+ Tambah Mesin"
2. Kartu Ringkasan (Summary Metric): REUSE Base SummaryGrid (src/components/features/base-features/summary-grid.jsx)
   - 5 kartu metrik kelaikan: Total Mesin, Siap Operasional, Dalam Perawatan, Diblokir, Tidak Laik Pakai
3. Kontrol Pencarian & Filter: REUSE Base DataFilter & FilterSelect
   - Pencarian kata kunci: Kode, nama, merek, atau serial number
   - Dropdown filter: Status Kelaikan dan Peruntukan Isolasi
4. Tabel Data & Paginasi: REUSE Base DataTable (src/components/features/base-features/data-table.jsx) & Base Pagination
   - Menampilkan 8 kolom terstruktur dengan sticky action column
5. Modal Ubah Status Mesin: REUSE Base ConfirmModal (src/components/features/base-features/confirm-modal.jsx)
   - Varian dinamis (success/warning/danger) sesuai status tujuan
   - Form radio group pilihan status kelaikan dan textarea alasan
6. Modal Riwayat Audit Status: REUSE ClinicalTimeline & ClinicalTimelineItem (src/components/ui/doctor-clinical-base/ClinicalTimeline.jsx)
   - Jejak kronologis terurut dari waktu terbaru
7. Modal Tambah / Edit Mesin: REUSE Base Modal & BaseButton
   - Formulir terstruktur dengan validasi input
8. State Boundary (AC-2): REUSE ClinicalStateBoundary (src/components/ui/doctor-clinical-base/ClinicalStateBoundary.jsx)
   - Loading State: Skeleton loader saat data mesin sedang diambil
   - Empty State: Ilustrasi dan pesan informatif saat belum ada mesin atau filter tidak cocok
   - Error State: Tampilan pesan error dan tombol coba lagi saat koneksi backend bermasalah
   - Content State: Tabel data mesin terisi lengkap
```

---

## 5. State yang Ditangani di Layar (AC-2)

| State Boundary | Pemicu / Kondisi | Tampilan Antarmuka yang Dihasilkan |
| --- | --- | --- |
| **Loading** | `machinesLoading === true` | Skeleton loader animasi dengan pesan *"Memuat data mesin hemodialisa..."* |
| **Empty (Belum Ada Data)** | `filteredMachines.length === 0` tanpa filter aktif | Ilustrasi kosong dengan pesan *"Belum ada data mesin cuci darah yang terdaftar di unit ini"* dan tombol aksi *"+ Tambah Mesin Pertama"* |
| **Empty (Hasil Filter Kosong)** | `filteredMachines.length === 0` dengan filter/pencarian aktif | Ilustrasi kosong dengan pesan *"Penyaring aktif saat ini tidak menghasilkan mesin"* dan tombol aksi *"Atur Ulang Penyaring"* |
| **Error** | `machinesError !== ""` | Tampilan error dengan judul *"Gagal memuat data mesin hemodialisa"*, pesan detail kegagalan, dan tombol *"Coba Lagi"* (`loadMachines`) |
| **Content (Berisi Data)** | `filteredMachines.length > 0` | Tabel 8 kolom lengkap beserta paginasi dan aksi interaktif |

---

## 6. Bukti Verifikasi & Pengujian

### 6.1 Unit Test Otomatis (Node.js Test Runner)
Perintah yang dijalankan:
```powershell
node --loader ./tests/support/module-alias-loader.mjs --test tests/unit/hemodialysis-sidebar-navigation.test.mjs tests/unit/hemodialysis-client-and-redux-slice.test.mjs tests/unit/hemodialysis-machine-master.test.mjs
```

Hasil Pengujian:
```text
✔ FE-HMD-02 File Integrity: Seluruh berkas services, slices, constants, dan hook tersedia (1.89ms)
✔ FE-HMD-02 AC-1 & Helper: unwrapApiResponse membongkar ApiResponse<T> format camelCase dan PascalCase (0.85ms)
✔ FE-HMD-02 AC-1: normalizeHmdError mendeteksi HTTP 423 Locked dan memandu ke Addendum Rekam Medis (0.40ms)
✔ FE-HMD-02 Helper: normalizeHmdError mendeteksi HTTP 409 Conflict dan pesan bentrok data (0.18ms)
✔ FE-HMD-02 AC-2: Hook status transisi menonaktifkan tombol aksi tidak sah pada sesi InProgress (0.34ms)
✔ FE-HMD-02 AC-2: Sesi Finalized berstatus terkunci dan hanya membuka opsi Addendum (0.14ms)
✔ FE-HMD-02 Aturan Resep & Mesin: Resep Active tidak dapat diedit; Mesin aktif tidak dapat diblokir (0.19ms)
✔ FE-HMD-02 Redux Slice: hemodialysisWorklistSlice mengelola filter dan state kerja (2.26ms)
✔ FE-HMD-02 Redux Slice: hemodialysisSessionSlice mitigasi stale state antar pasien via resetSessionState (0.75ms)
✔ FE-HMD-02 Redux Slice: hemodialysisMasterSlice mengelola data mesin, station, dan checklist (0.64ms)
✔ FE-HMD-03 File Integrity: Seluruh berkas view, kolom, modal, utils, dan rute master mesin tersedia (2.06ms)
✔ FE-HMD-03 Tabel: hemodialysis-machines-table-columns.jsx mendefinisikan 8 kolom lengkap dengan aksi (4.86ms)
✔ FE-HMD-03 Status Badge & Formatter: Format status kelaikan dan tanggal Indonesia valid (0.35ms)
✔ FE-HMD-03 AC-1: Teknisi memilih M-02, mengubah status ke Maintenance dengan alasan, dan status terbarui (2.34ms)
✔ FE-HMD-03 AC-2: Layar mendukung 4 state (loading, empty, error, content) secara deterministik (0.57ms)
✔ FE-HMD-01 AC-1: menu-items.jsx mendaftarkan grup Hemodialisa dengan 6 submenu dan 4 subitems master data (7.08ms)
✔ FE-HMD-01 AC-2: Seluruh 9 rute navigasi Hemodialisa memiliki berkas page.jsx valid tanpa 404 (1.66ms)
✔ FE-HMD-01 AC-2: Resolver sidebar mengenali seluruh rute Hemodialisa dan menghasilkan path navigasi yang benar (1.13ms)
✔ FE-HMD-01 AC-3: Pengguna Koordinator HD melihat seluruh menu Hemodialisa lengkap (0.85ms)
✔ FE-HMD-01 AC-3: Pengguna Staf Bangsal tidak melihat menu internal unit (Kesiapan Unit dan Master Data) (0.43ms)
✔ FE-HMD-01 AC-3: Filter berbasis permissions menyaring menu sesuai klaim hak akses (0.47ms)
ℹ tests 21 | pass 21 | fail 0 | cancelled 0 | skipped 0
```

### 6.2 ESLint Linter Verification
Perintah yang dijalankan:
```powershell
node ./node_modules/eslint/bin/eslint.js "src/components/view/health-services/hemodialysis-management/master-data/machines/**/*.jsx" "src/utils/health-services/hemodialysis-management/hemodialysis-machines-display-utils.js" "src/app/health-services/hemodialysis-management/master-data/machines/page.jsx"
```
Hasil: **Exit Code 0 (0 errors, 0 warnings)**.

---

## 7. Pemenuhan Acceptance Criteria & Definition of Done

### 7.1 Acceptance Criteria
- [x] **AC-1 (Alur Ubah Status & Riwayat)**: Teknisi memilih mesin `M-02`, menekan "Ubah Status", memilih status `Maintenance`, mengisi teks alasan `"Penggantian filter ultrafiltrasi berkala"`. Chip status terbukti berubah kuning (*Maintenance*) dan entri riwayat baru tersimpan pada lini masa.
- [x] **AC-2 (Implementasi 4 State Boundary)**: Layar mengimplementasikan `ClinicalStateBoundary` dengan skeleton loader saat memuat, tampilan kosong bila data nihil, pesan error bila gagal terhubung ke backend, serta tabel data saat terisi.

### 7.2 Definition of Done Checklist
- [x] Kode mengacu pada blueprint `03-frontend-architecture.md` Bagian 8.1 dan kontrak `HMD-CONTRACT-v1`.
- [x] Seluruh komponen memanfaatkan base component Quilvian (`Hero`, `SummaryGrid`, `DataFilter`, `DataTable`, `ConfirmModal`, `ClinicalStateBoundary`, `ClinicalTimeline`).
- [x] Lencana netral tanpa stigmatisasi diterapkan pada peruntukan mesin isolasi.
- [x] Pengujian unit otomatis lengkap dan lulus 100%.
- [x] ESLint bersih tanpa error dan tanpa warning.
- [x] Tidak ada commit Git otomatis atau pelanggaran batas wewenang.
