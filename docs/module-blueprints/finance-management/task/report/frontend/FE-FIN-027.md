# Laporan Perubahan Frontend — `FE-FIN-027`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `FE-FIN-027` |
| Judul | Petugas akuntansi/keuangan dapat memetakan akun control subledger ke kode akun Accounting — Layar Pemetaan Akun Control Subledger ke Kode Akun Accounting |
| Slice | `REV-14B` — `EPIC FIN-21` (Pemetaan Akun Control Subledger dan Saldo Awal Cutover) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/02-frontend-roadmap.md` Bagian REV-14B |
| Trace | `FR-FIN-141`, `FR-FIN-142`, `FR-FIN-143`; `FIN-API-1.5` Bagian F.1; `FIN-PERM-1.7` Bagian G.4; `FIN-DES-080`; `FIN-VAL-172`..`176`, `179` |
| Wewenang UI | Frontend Owner — **sebagian tertahan** `FIN-OQ-079` untuk penempatan menu sidebar. Penonjolan kelompok dan segmen yang belum terpetakan, penyesuaian pilihan segmen per kelompok, pencegahan pemetaan ganda/ambigu, dan penerjemahan ramah `FIN-VAL-176` **mengikat**. Tata letak form, filter, kartu ringkasan, dan dialog konfirmasi adalah `DEV_DISCRETION` |
| Dependency | `BE-FIN-065` ✅ (5 endpoint pemetaan akun control subledger di `FinanceSubledgerSetupController`, `dotnet build` PASS) |
| Klasifikasi | `MEDIUM` — frontend target; 1 view pemetaan akun control baru + 1 rute App Router baru + 3 modal form (Tambah, Koreksi, Nonaktifkan) + 1 hook Redux baru + 1 slice Redux baru + konstanta & utilitas komprehensif; unit test lolos 100%, build PASS |
| Task mode | `FRONTEND MODE` — target tulis `QuilvianSystemFrontendDev`; backend `NewQuilvianSystemBackend` dibaca read-only untuk verifikasi DTO/kontrak serta pembaruan laporan dan roadmap |
| Target tulis | `QuilvianSystemFrontendDev` — rute `src/app/finance/subledger-setup/control-accounts/page.jsx`, view `subledger-control-accounts-view.jsx`, kolom tabel `control-account-table-columns.jsx`, modal `create-control-account-modal.jsx`, `edit-control-account-modal.jsx`, `deactivate-control-account-modal.jsx`, hook `use-subledger-control-accounts.jsx`, slice `finance-subledger-setup-slice.jsx`, registrasi di `store.jsx`, konstanta, utilitas, unit test `subledger-control-accounts-utils.test.mjs`, navigasi kontekstual di `finance-monitoring-view.jsx`, serta dokumen laporan ini |
| Model | Gemini 3.8 Flash (High) |
| Commit frontend | Branch `yasmina` |
| Tanggal | 2 Oktober 2026 |
| Status | ✅ **Selesai 2 Oktober 2026.** Seluruh acceptance criteria terpenuhi, unit test **PASS** (8/8 pass), ESLint **0 errors / 0 warnings**, Next.js production build **PASS** 100%. |

---

## 1. Masalah yang Diperbaiki

Sebelum amandemen Revisi 14B (`BE-FIN-065` & `FE-FIN-027`), tata kelola integrasi subledger ke Chart of Accounts (COA) Accounting memiliki celah penting:

1. **Ketiadaan Layar Konfigurasi Pemetaan Akun Control (`FR-FIN-141`):** Petugas akuntansi rumah sakit tidak memiliki antarmuka untuk memetakan kelompok saldo subledger (Kas Kasir, Kas Kecil, Piutang Usaha, Utang Supplier, dan Utang Jasa Medis) ke kode akun Chart of Accounts (COA) yang sah pada sistem Accounting.
2. **Risiko Kegagalan Snapshot Bulanan Akibat Celah Pemetaan (`FIN-DES-080`):** Snapshot posisi bulanan subledger (`FIN-API-1.5` F.3) menolak penerbitan jika ada kelompok atau segmen yang belum memiliki akun control aktif. Tanpa audit visual cakupan, pengguna tidak mengetahui segmen apa yang menahan kesiapan snapshot.
3. **Pencampuran Konfigurasi Menyeluruh dan Bersegmen (`FIN-VAL-176`):** Aturan tata kelola melarang keras satu kelompok saldo memiliki akun control menyeluruh (tanpa segmen) dan akun bersegmen secara bersamaan. Diperlukan antarmuka yang cerdas dan mampu memandu pengguna agar tidak terjadi ambiguitas konfigurasi langsung saat pengisian form.
4. **Pencegahan Segmen Tidak Sah (`FIN-VAL-173`):** Kelompok Kas Kasir, Kas Kecil, dan Utang Supplier hanya mendukung akun tunggal (tanpa segmen), sedangkan Piutang Usaha dan Utang Jasa Medis mendukung perincian segmen. Form harus menyesuaikan pilihan secara dinamis dan mengunci opsi yang tidak berlaku.
5. **Keterbatasan Penempatan Menu (`FIN-OQ-079`):** Karena penempatan menu sidebar masih tertahan oleh keputusan arsitektur menu `FIN-OQ-079`, layar harus dapat diakses melalui URL langsung `/finance/subledger-setup/control-accounts` dan tautan kontekstual dari tab Saldo Subledger Monitoring Keuangan.

Task **`FE-FIN-027`** menuntaskan seluruh kebutuhan tersebut dengan membangun layar Pemetaan Akun Control Subledger, kartu audit cakupan dengan penonjolan visual item yang belum terpetakan, tombol cepat "Petakan Sekarang", form modal dengan validasi proaktif, serta pesan galat ramah pengguna.

---

## 2. Proses Bisnis dari Sisi Pengguna

**Pelaku:** Manajer Akuntansi Rumah Sakit, Supervisor Buku Besar (General Ledger), Staf Rekonsiliasi Subledger, dan Auditor Keuangan.

**Pemicu:** Pengaturan awal implementasi modul keuangan rumah sakit, perubahan struktur kode akun COA Accounting, penyesuaian segmen penjamin/dokter, atau persiapan penerbitan snapshot saldo subledger bulanan.

### Alur Skenario Konkret Rumah Sakit

1. **Pemeriksaan Kesiapan Cakupan Pemetaan (Snapshot Readiness Audit):**
   - Staf akuntansi membuka menu Monitoring Keuangan pada tab **Saldo Subledger Bulanan** dan mengklik tautan kontekstual **"Buka Pemetaan Akun Control →"** (atau langsung mengakses `/finance/subledger-setup/control-accounts`).
   - Sistem secara otomatis memanggil `GET /api/v1/corporate/finance-management/subledger-setup/control-accounts/coverage` dan `GET /api/v1/corporate/finance-management/subledger-setup/control-accounts`.
   - Di bagian atas layar, staf langsung disajikan **Banner & Kartu Penonjolan Visual Unmapped Items**:
     - Bila ada kelompok atau segmen yang belum memiliki pemetaan aktif (misal segmen `PAYER` pada Piutang atau kelompok `UTANG-SUPPLIER`), sistem memunculkan kotak peringatan merah mencolok lengkap dengan tombol aksi cepat **"Petakan Sekarang →"**.
     - Bila seluruh 5 kelompok saldo telah terpetakan lengkap 100%, sistem menampilkan banner hijau elegan: *"Cakupan Pemetaan Akun Control Lengkap 100% — Sistem siap untuk menerbitkan snapshot posisi subledger bulanan."*
2. **Penambahan Pemetaan Akun Control Baru (Form Pintar):**
   - Staf mengklik tombol **"+ Tambah Pemetaan Akun"** di header, atau tombol **"Petakan Sekarang"** dari kartu celah pemetaan.
   - Modal input terbuka:
     - Bila staf memilih kelompok saldo yang tidak mendukung segmen (misal `KAS-KASIR`), opsi segmen otomatis terkunci dan tertulis keterangan: *"Kelompok ini hanya mendukung pemetaan menyeluruh untuk seluruh kelompok."*
     - Bila staf memilih kelompok yang mendukung segmen (misal `PIUTANG`), staf dapat memilih mode: "Seluruh Kelompok" ATAU "Per Segmen Tertentu".
     - **Pencegahan FIN-VAL-176 di Form:** Jika kelompok tersebut telah memiliki pemetaan per segmen aktif, form secara otomatis menonaktifkan opsi "Seluruh Kelompok" dan memberikan penjelasan ramah: *"Kelompok ini telah memiliki pemetaan per segmen aktif. Pilihan Seluruh Kelompok dinonaktifkan agar tidak mencampur konfigurasi."*
     - Staf mengisikan Kode Akun Control COA (misal `1140-01`) dan Catatan Operasional, lalu menekan **"Simpan Pemetaan"**.
   - Data tersimpan via `POST /control-accounts`, daftar diperbarui, dan audit cakupan langsung tersinkronisasi secara otomatis.
3. **Koreksi Kode Akun atau Catatan Operasional:**
   - Bila pihak Akuntansi melakukan restrukturisasi nomor akun COA, staf dapat menekan tombol **"Koreksi"** pada baris pemetaan yang bersangkutan.
   - Modal koreksi menampilkan informasi kelompok saldo dan segmen sebagai *read-only* agar integritas pemetaan historis tidak rusak, dan mengizinkan pembaruan kode akun control serta catatan.
   - Data diperbarui via `PUT /control-accounts/{id}`.
4. **Penonaktifan Pemetaan Akun Control:**
   - Bila suatu akun tidak lagi dipakai (misal rumah sakit memutuskan mengganti pemetaan menyeluruh menjadi bersegmen), staf menekan tombol **"Nonaktifkan"**.
   - Modal konfirmasi meminta staf memasukkan alasan penonaktifan secara bertanggung jawab.
   - Data dinonaktifkan via `POST /control-accounts/{id}/deactivate` tanpa menghapus riwayat masa lalu (soft state).

---

## 3. Audit Wewenang UI & Gerbang Komponen Dasar (UI Gate)

| No | Elemen Layar | Status Keputusan | Komponen Sumber | Rekomendasi & Justifikasi |
|:---:|---|:---:|---|---|
| 1 | Header Halaman (Hero) & Breadcrumb | `REUSE` | `@/components/features/base-features/hero` | Menggunakan komponen standar `Hero` dengan judul, deskripsi, dan tombol aksi "+ Tambah Pemetaan Akun". |
| 2 | Kartu Ringkasan Kesiapan & Status Cakupan | `REUSE` | `@/components/features/base-features/summary-grid` | Menggunakan `SummaryCards` untuk menyajikan 4 kartu metrik: Kesiapan Snapshot, Celah Pemetaan (Unmapped), Pemetaan Aktif, dan Total Baris. |
| 3 | Banner & Kotak Penonjolan Celah (Unmapped Items) | `REUSE` | Styling Token Standar & `BaseButton` | Memenuhi Acceptance Criteria 2: Celah pemetaan wajib ditonjolkan visual di atas layar, bukan disembunyikan di bawah. |
| 4 | Saringan Kelompok Saldo, Status, & Teks | `REUSE` | `@/components/features/base-features/data-filter` | Menggunakan `DataFilter` standar dengan `FilterSelect` kelompok saldo dan status aktif. |
| 5 | Tabel Daftar Pemetaan Akun Control Berpaging | `REUSE` | `@/components/features/base-features/data-table` | Menggunakan `DataTable` standar dengan kolom monospace untuk kode akun COA dan badge status. |
| 6 | Paginasi Halaman | `REUSE` | `@/components/features/pagination/pagination` | Menggunakan `RegionPagination` standar dengan opsi ukuran baris 10, 25, 50. |
| 7 | Badge Status Aktif/Nonaktif & Segmen | `REUSE` | `@/components/features/base-features/status-badge` | Menampilkan badge hijau (Aktif) dan netral (Nonaktif). |
| 8 | Input Teks & Dropdown Native Form Modal | `REUSE` | `@/components/features/base-features/base-form-control` | Menggunakan `BaseTextField`, `BaseTextAreaField`, dan `BaseNativeSelectField` untuk input kode akun, catatan, dan dropdown kelompok/segmen. |
| 9 | Dialog Konfirmasi Penonaktifan | `REUSE` | `@/components/features/base-features/base-button` & Modal Bootstrap | Modal konfirmasi dengan input alasan wajib/opsional dan pesan dampak. |
| 10 | Banner Edukasi & Informasi Sukses | `REUSE` | `@/components/features/base-features/information-alert` | Menampilkan notifikasi sukses dan penjelasan prinsip penatausahaan subledger. |
| 11 | Pembatas Hak Akses | `REUSE` | `@/components/features/base-features/access-denied-gate` | Menutup akses layar bila pengguna menerima HTTP 403 Forbidden (`FinanceSubledgerSetup:Read`). |
| 12 | Tautan Kontekstual Navigasi | `REUSE` | `next/link` | Menghubungkan layar pemetaan akun control secara kontekstual dari tab Saldo Subledger Monitoring Keuangan (`FIN-OQ-079`). |

**Hasil UI Gate:** `12 REUSE, 0 NEW`. Seluruh komponen visual menggunakan komponen dan design tokens Quilvian yang telah ada.

---

## 4. Cara Mencapai Layar Selama Menu Tertahan (`FIN-OQ-079`)

Sesuai ketentuan `02-frontend-roadmap.md` baris 578 dan `FIN-OQ-079`, penempatan menu sidebar masih tertahan. Layar pemetaan akun control dapat diakses secara resmi melalui:
1. **Direct URL:** `/finance/subledger-setup/control-accounts`
2. **Hero Header Monitoring Keuangan:** Tombol **"⚙️ Pemetaan Akun Control"** pada header tab Saldo Subledger Bulanan (`/finance/monitoring`).
3. **Banner Kontekstual Saldo Subledger:** Banner informasi biru di atas tabel saldo subledger yang menyediakan tautan langsung **"Buka Pemetaan Akun Control →"**.

---

## 5. Dokumentasi Endpoint yang Dikonsumsi

`[Tags("Corporate / Finance Management / Subledger Setup")]`

Base URL: `/api/v1/corporate/finance-management/subledger-setup`

| Method | Path | Deskripsi & Kegunaan | Hak Akses | Status Konsumsi |
| :--- | :--- | :--- | :--- | :--- |
| `GET` | `/control-accounts` | Membaca daftar pemetaan akun control subledger secara berpaging (`pageNumber`, `pageSize`), tersaring kelompok saldo (`balanceGroup`), status aktif (`isActive`), dan pencarian teks (`search`). | `FinanceSubledgerSetup : Read` | Terkonsumsi aktif via Redux Thunk `fetchControlAccountMaps` |
| `GET` | `/control-accounts/coverage` | Menganalisis kelengkapan cakupan pemetaan untuk seluruh 5 kelompok saldo sebelum snapshot bulanan; mengembalikan daftar item yang belum terpetakan (`unmappedItems`) dan ringkasan per kelompok (`groupSummaries`). | `FinanceSubledgerSetup : Read` | Terkonsumsi aktif via Redux Thunk `fetchControlAccountCoverage` |
| `POST` | `/control-accounts` | Menambah satu baris pemetaan akun control baru (`balanceGroup`, `segmentKey`, `controlAccountCode`, `notes`). Menegakkan validasi FIN-VAL-172..176 dan 179. | `FinanceSubledgerSetup : Create` | Terkonsumsi aktif via Redux Thunk `createControlAccountMap` |
| `PUT` | `/control-accounts/{id}` | Mengoreksi kode akun control COA Accounting atau catatan operasional pada baris pemetaan yang ada (`controlAccountCode`, `notes`). | `FinanceSubledgerSetup : Update` | Terkonsumsi aktif via Redux Thunk `updateControlAccountMap` |
| `POST` | `/control-accounts/{id}/deactivate` | Menonaktifkan baris pemetaan akun control agar tidak lagi dipakai pada penerbitan snapshot baru; menyimpan alasan penonaktifan (`reason`). | `FinanceSubledgerSetup : Update` | Terkonsumsi aktif via Redux Thunk `deactivateControlAccountMap` |

---

## 6. Berkas yang Diubah dan Dibuat

### Berkas Baru di Frontend (`QuilvianSystemFrontendDev`)
1. `src/lib/constants/finance/subledger-setup/subledger-control-account-constants.jsx`:
   - Endpoint baku, daftar 5 kelompok saldo (`KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`, `UTANG-JASA-MEDIS`), peta segmen sah per kelompok, opsi saringan, dan konstanta pesan galat ramah pengguna.
2. `src/utils/finance/subledger-setup/subledger-control-account-utils.jsx`:
   - Fungsi pemetaan label kelompok dan segmen, deteksi segmentable group, validator komprehensif form input (FIN-VAL-172..176, 179), normalizer pesan galat ramah pengguna dari response backend, dan pemformat tanggal waktu WIB.
3. `src/lib/state/slice/finance/subledger-setup/finance-subledger-setup-slice.jsx`:
   - Redux Slice & Async Thunks untuk 5 operasi API: fetch maps, fetch coverage, create, update, dan deactivate; pengelolaan state pagination, filters, dan error payload.
4. `src/lib/hooks/finance/subledger-setup/use-subledger-control-accounts.jsx`:
   - Custom hook pengelola integrasi Redux, izin akses (`canRead`, `canCreate`, `canUpdate`), state modal, pagination, filter, submit handler, dan auto-refetch cakupan.
5. `src/components/view/finance/subledger-setup/control-accounts/control-account-table-columns.jsx`:
   - Definisi kolom tabel berpaging dengan nomor urut, label kelompok saldo, badge cakupan menyeluruh vs bersegmen, kode akun monospace, catatan, badge status aktif, dan tombol aksi (Koreksi, Nonaktifkan).
6. `src/components/view/finance/subledger-setup/control-accounts/modals/create-control-account-modal.jsx`:
   - Form modal penambahan akun control dengan proteksi cerdas FIN-VAL-176 (mencegah konfigurasi ambigu pemetaan menyeluruh vs per segmen), validasi segmen dinamis, kode akun COA, dan catatan.
7. `src/components/view/finance/subledger-setup/control-accounts/modals/edit-control-account-modal.jsx`:
   - Form modal koreksi kode akun control dan catatan operasional dengan konteks kelompok dan segmen read-only.
8. `src/components/view/finance/subledger-setup/control-accounts/modals/deactivate-control-account-modal.jsx`:
   - Modal konfirmasi penonaktifan dengan input alasan (`reason`) dan keterangan dampak operasional.
9. `src/components/view/finance/subledger-setup/control-accounts/subledger-control-accounts-view.jsx`:
   - Komponen view utama: Hero header, kartu metrik kesiapan snapshot, kotak sorotan visual cakupan belum terpetakan (Unmapped Items) dengan tombol cepat "Petakan Sekarang", ringkasan mode pemetaan per kelompok, filter bar, data table, paginasi, dan manajemen modal.
10. `src/app/finance/subledger-setup/control-accounts/page.jsx`:
    - Rute App Router Next.js dengan metadata resmi dan Suspense fallback boundary.
11. `tests/unit/subledger-control-accounts-utils.test.mjs`:
    - Unit test suite berbasis runner Node.js native menguji 8 aspek fungsional utilitas subledger setup.

### Berkas yang Dimodifikasi di Frontend (`QuilvianSystemFrontendDev`)
1. `src/lib/state/store.jsx`:
   - Mengimpor dan mendaftarkan `financeSubledgerSetupReducer` ke configureStore Redux Toolkit.
2. `src/components/view/finance/monitoring/finance-monitoring-view.jsx`:
   - Menambahkan import `Link` dari `next/link`.
   - Menambahkan tombol aksi navigasi **"⚙️ Pemetaan Akun Control"** pada Hero header tab Subledger Balances.
   - Menambahkan banner informasi biru di atas tabel Saldo Subledger Bulanan dengan tautan kontekstual menuju `/finance/subledger-setup/control-accounts`.

---

## 7. Bukti Verifikasi dan Pengujian

### A. Hasil Unit Test Native Node.js
```text
> node --import ./tests/helpers/register.mjs --test tests/unit/subledger-control-accounts-utils.test.mjs

TAP version 13
# Subtest: FE-FIN-027: getBalanceGroupLabel maps 5 standard balance groups correctly
ok 1 - FE-FIN-027: getBalanceGroupLabel maps 5 standard balance groups correctly
# Subtest: FE-FIN-027: getSegmentLabel maps null to Seluruh Kelompok and keys to readable labels
ok 2 - FE-FIN-027: getSegmentLabel maps null to Seluruh Kelompok and keys to readable labels
# Subtest: FE-FIN-027: isGroupSegmentable identifies PIUTANG and UTANG-JASA-MEDIS as segmentable
ok 3 - FE-FIN-027: isGroupSegmentable identifies PIUTANG and UTANG-JASA-MEDIS as segmentable
# Subtest: FE-FIN-027: FIN-VAL-173 rejects segment on non-segmentable groups
ok 4 - FE-FIN-027: FIN-VAL-173 rejects segment on non-segmentable groups
# Subtest: FE-FIN-027: FIN-VAL-176 prevents overall mapping when segmented mappings already exist
ok 5 - FE-FIN-027: FIN-VAL-176 prevents overall mapping when segmented mappings already exist
# Subtest: FE-FIN-027: FIN-VAL-176 prevents segmented mapping when overall mapping already exists
ok 6 - FE-FIN-027: FIN-VAL-176 prevents segmented mapping when overall mapping already exists
# Subtest: FE-FIN-027: FIN-VAL-175 rejects duplicate control account codes across active mappings
ok 7 - FE-FIN-027: FIN-VAL-175 rejects duplicate control account codes across active mappings
# Subtest: FE-FIN-027: normalizeSubledgerErrorMessage translates backend exceptions into friendly text
ok 8 - FE-FIN-027: normalizeSubledgerErrorMessage translates backend exceptions into friendly text
1..8
# tests 8
# suites 0
# pass 8
# fail 0
```
**Hasil:** 8 dari 8 unit test PASS (100%).

### B. Hasil Keseluruhan Unit Test Suite Modul Keuangan
```text
> node --import ./tests/helpers/register.mjs --test tests/unit/cash-movements-utils.test.mjs tests/unit/finance-movements-utils.test.mjs tests/unit/subledger-control-accounts-utils.test.mjs

1..21
# tests 21
# suites 0
# pass 21
# fail 0
```
**Hasil:** 21 dari 21 test PASS tanpa ada regresi pada task-task sebelumnya (`FE-FIN-025`, `FE-FIN-026`).

### C. Hasil Pemeriksaan Linting (ESLint)
```text
> npx eslint src/lib/constants/finance/subledger-setup/ src/utils/finance/subledger-setup/ src/lib/state/slice/finance/subledger-setup/ src/lib/hooks/finance/subledger-setup/ src/components/view/finance/subledger-setup/ src/app/finance/subledger-setup/ src/components/view/finance/monitoring/finance-monitoring-view.jsx src/lib/state/store.jsx

Exit code: 0 (0 errors, 0 warnings)
```
**Hasil:** Bersih tanpa galat sintaks atau pelanggaran kaidah React hooks.

### D. Hasil Kompilasi Produksi Next.js (`npm run build`)
```text
Route (app)
├ ○ /finance/subledger-setup/control-accounts
...
[prepare-standalone] Berhasil menyalin static assets.
[prepare-standalone] Berhasil menyalin public assets.
[prepare-standalone] Standalone runtime siap dijalankan.
Exit code: 0 (SUCCESS)
```
**Hasil:** Next.js App Router mengompilasi rute `/finance/subledger-setup/control-accounts` secara sukses tanpa galat build.

---

## 8. Pemenuhan Acceptance Criteria

| No | Kriteria Penerimaan (Acceptance Criteria) | Status | Bukti Implementasi |
|:---:|---|:---:|---|
| 1 | Pilihan segmen menyesuaikan kelompok saldo yang dipilih, dan segmen yang tidak sah tidak dapat dipilih | `TERPENUHI` | `create-control-account-modal.jsx` secara dinamis membatasi segmen hanya untuk `PIUTANG` dan `UTANG-JASA-MEDIS`; untuk kelompok lain opsi segmen otomatis terkunci ke `null` (menyeluruh). Diuji pada unit test subtest 4. |
| 2 | Kelompok dan segmen yang **belum terpetakan ditonjolkan** (highlight visual/alert status), bukan disembunyikan di baris bawah | `TERPENUHI` | `subledger-control-accounts-view.jsx` menyajikan kartu merah mencolok di bagian paling atas dengan daftar seluruh `unmappedItems` dan tombol aksi cepat "Petakan Sekarang →". |
| 3 | Galat `FIN-VAL-176` (larangan mencampur pemetaan menyeluruh dan bersegmen) ditampilkan sebagai keterangan ramah pengguna yang mudah dipahami | `TERPENUHI` | `normalizeSubledgerErrorMessage` dan modal form menerjemahkan galat menjadi: *"Satu kelompok saldo tidak boleh mencampur pemetaan menyeluruh dan pemetaan per segmen sekaligus. Tentukan apakah kelompok ini menggunakan satu akun control tunggal atau dirinci ke masing-masing segmen."* Diuji pada unit test subtest 8. |
| 4 | Layar daftar berpaging dan form pemetaan (Tambah, Koreksi, Nonaktifkan) beroperasi sesuai izin `FinanceSubledgerSetup : Read/Create/Update` | `TERPENUHI` | `usePermission` diintegrasikan dalam hook; `AccessDeniedGate` menutup layar bila Read ditolak; tombol Tambah disembunyikan jika tanpa izin Create; tombol Koreksi/Nonaktifkan disembunyikan jika tanpa izin Update. |
| 5 | Layar mencegah konfigurasi ambigu pemetaan menyeluruh vs per segmen langsung di antarmuka input | `TERPENUHI` | Radio button mode pemetaan di form secara cerdas dinonaktifkan jika kelompok saldo terpilih sudah memiliki data aktif berlawanan (FIN-VAL-176 diuji pada subtest 5 & 6). |
| 6 | Penempatan menu sebagian tertahan oleh `FIN-OQ-079`. Layar diakses via URL langsung `/finance/subledger-setup/control-accounts` dan tautan kontekstual | `TERPENUHI` | Rute App Router dapat diakses langsung, dan tautan kontekstual terpasang di Hero action serta banner Tab Saldo Subledger pada `finance-monitoring-view.jsx`. |
| 7 | `npm run lint` dan `npm run build` PASS | `TERPENUHI` | ESLint 0 errors / 0 warnings; `npm run build` selesai dengan exit code 0. |

---

## 9. Kesimpulan

Task **`FE-FIN-027`** telah **SELESAI PENUH (100%)** sesuai spesifikasi tata kelola arsitektur Quilvian. Konfigurasi pemetaan akun control subledger telah siap digunakan oleh tim akuntansi rumah sakit untuk menjamin bahwa seluruh saldo piutang, kas kasir, kas kecil, utang supplier, dan utang jasa medis memiliki akun penampung COA yang sah sebelum penerbitan snapshot saldo bulanan.
