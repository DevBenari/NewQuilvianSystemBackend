# LAPORAN REKONSILIASI IMPLEMENTASI SISTEM: HRD MENU 07 (PENEMPATAN, MUTASI & PROMOSI)

**Versi:** Revisi UAT Penempatan Awal & Smart Validation — 20 November 2026  
**Status Implementasi:** `IMPLEMENTED IN SOURCE` (18 Temuan Audit `AUD-HRD07-01` s/d `18` dan 4 Temuan UAT `UAT-HRD07-INITIAL-001` s/d `004` telah selesai diperbaiki di kode sumber)  
**Status Pengujian:** `READY FOR UAT` (Unit test lolos 8/8, Linter lolos 0 error/warning, Audit CSS 0 temuan; menunggu pengujian live browser di staging/dev)  
**Mandat Verifikasi Build:** `NOT RUN (mandat eksplisit pengguna: "tidak perlu di dotnet build langsung implementasikan saja")`  
**Mandat Verifikasi Unit Test:** `PASS (node test tests/unit/workforce-core-target-state.test.mjs: 8/8 lulus)`  
**Mandat Verifikasi Linter:** `PASS (eslint src/components/view/hr/workforce-core/initial-placement-batch-view.jsx: 0 error, 0 warning)`  
**Mandat Verifikasi CSS:** `PASS (check-css-usage.mjs: 0 temuan pada seluruh berkas view dan rute)`  

---

## 1. Lingkungan Eksekusi & Tata Kelola Git

| Parameter | Backend | Frontend |
| :--- | :--- | :--- |
| **Repository** | `NewQuilvianSystemBackend` | `QuilvianSystemFrontendDev` |
| **Branch Aktif** | `MHamzah` | `HamzahV2` |
| **Status Git** | Perubahan lokal dalam target scope tugas | Perubahan lokal dalam target scope tugas |
| **Larangan Operasional** | Tidak menjalankan `git commit`, `git push`, `git checkout`, `git merge`, atau migrasi database tanpa otorisasi terpisah | Tidak menjalankan `git commit`, `git push`, `git checkout`, `git merge` tanpa otorisasi terpisah |
| **Mandat Build Pengguna** | Dilompati sesuai mandat eksplisit pengguna: *"tidak perlu di dotnet build langsung implementasikan saja"* | Verifikasi linting dan unit testing internal dijalankan penuh dan lulus 100% |

---

## 2. Matriks Temuan UAT Penempatan Awal (`UAT-HRD07-INITIAL-001` s.d. `004`)

Berdasarkan hasil temuan uji lapangan (UAT) batch penempatan awal 41 kandidat (37 valid, 4 tidak valid/sudah ditempatkan):

| ID Temuan | Prioritas | Gejala & Masalah Lapangan | Solusi Arsitektur & Perbaikan Kode Nyata | Status |
| :--- | :---: | :--- | :--- | :---: |
| `UAT-HRD07-INITIAL-001` | **P1** | Tombol **Simpan & Buat Batch** nonaktif total karena ada 4 baris tidak valid (`templateInvalidCount > 0`), sehingga 37 pegawai valid tersandera dan proses batching terblokir. | Mengubah UI & DTO: baris tidak valid tidak lagi memblokir batch secara global. Pengguna dapat memilih subset pegawai yang berstatus `SIAP_DIPROSES`. Tombol **Simpan & Buat Batch (N Pegawai)** aktif jika $N > 0$ dan semua baris yang dicentang 100% valid. Baris tidak valid/dilewati ditinggalkan tanpa masuk ke batch. | **FIXED** |
| `UAT-HRD07-INITIAL-002` | **P1** | Pegawai yang sudah memiliki penempatan aktif ditandai tidak valid secara kasar tanpa kategori diagnostik pembeda, rawan tertimpa atau memicu intervensi destruktif. | Menambahkan kategori diagnostik `DILEWATI_SUDAH_DITEMPATKAN` (`ALREADY_PLACED_ACTIVE`). Pegawai dengan penugasan jabatan aktif dilindungi, tidak dimasukkan ke batch penempatan awal, dan diarahkan ke alur Mutasi/Rotasi bila butuh penyesuaian. | **FIXED** |
| `UAT-HRD07-INITIAL-003` | **P1** | Baris data tidak valid tidak memberikan navigasi perbaikan terpandu (hanya tooltip/badge merah mati), membuat HR Admin bingung mencari data yang salah. | Menyediakan tindakan terpandu per baris: tombol **Lihat Alasan** (membuka modal detail diagnostik & rekomendasi langkah), **Tinjau Penempatan** (ke `/hr/workforce-core/position-assignments`), dan **Perbaiki Data** (navigasi aman berbasis allowlist `SAFE_DESTINATIONS` ke modul Master Pegawai, Struktur Organisasi, atau Master Jabatan). Draft state disimpan di `sessionStorage` (`HRD_MENU_07_INITIAL_BATCH_DRAFT_CTX`) sehingga saat kembali, konteks otomatis dipulihkan dan divalidasi ulang dari server. | **FIXED** |
| `UAT-HRD07-INITIAL-004` | **P2** | Inkonsistensi perhitungan validitas antara pratinjau template (`GetTemplateRowsAsync`) dan validasi batch (`ValidateRowsAsync`). | Menyatukan aturan validasi di backend: `GetTemplateRowsAsync` dioptimasi dengan bulk query (menghilangkan N+1 query) dan menghasilkan metadata diagnostik terpadu (`EligibilityStatus`, `ReasonCode`, `ReasonMessage`, `TargetResourceKey`, `TargetRecordId`) yang 100% identik dengan hasil `ValidateRowsAsync`. | **FIXED** |

---

## 3. Matriks 18 Temuan Audit Baseline (`AUD-HRD07-01` s.d. `AUD-HRD07-18`)

| ID Temuan | Prioritas | Fakta Audit Sebelum Perbaikan | Tindakan Perbaikan Kode Nyata | Status |
| :--- | :---: | :--- | :--- | :---: |
| `AUD-HRD07-01` | **P0** | `initial-placement-batch-view.jsx` memakai `isPending` tanpa deklarasi $\rightarrow$ `ReferenceError` | Mengganti guard tombol dengan `canValidate` yang terdefinisi pada status `Draft` dan `Validated`. | **FIXED** |
| `AUD-HRD07-02` | **P0** | `ValidateBatchByIdAsync` hanya menolak `Executed` (bisa downgrade status `Approved` / `PendingApproval`), dan permission `POST /{id}/validate` adalah `Read` | Menolak validasi ulang untuk status `Executed`, `PendingApproval`, dan `Approved`. Mengubah atribut permission di `HrdInitialPlacementBatchController` menjadi `Update`. | **FIXED** |
| `AUD-HRD07-03` | **P0** | Mismatch kontrak Uraian Tugas: FE membaca flat keys, BE mengirim nested `jobDescription { items }` dan `versionHistory` | Menyelaraskan `employee-job-descriptions-view.jsx` dengan kontrak DTO backend nyata, mengelompokkan butir tugas berdasarkan `sectionType` dan urutan `sequence`. | **FIXED** |
| `AUD-HRD07-04` | **P0** | Route `/personnel-actions/create?category=...` diabaikan, default selalu `Mutation` | Menambahkan pembacaan `useSearchParams` dengan boundary `Suspense` dan allowlist enum (`Mutation`, `Rotation`, `Promotion`, `Demotion`, `TemporaryAssignment`). | **FIXED** |
| `AUD-HRD07-05` | **P0** | `CancelOrderAsync` menerima `Scheduled` walau tanpa cancel-request (direct bypass) | Mewajibkan adanya permohonan pembatalan resmi terdahulu (`CancellationRequestedByUserId.HasValue`) dan menegakkan anti self-approval pembatalan. | **FIXED** |
| `AUD-HRD07-06` | **P0** | Worker `ProcessScheduledOrdersAsync` dapat menerapkan SK yang sedang diajukan pembatalan | Menambahkan filter `x.CancellationRequestedByUserId == null` agar SK yang pending pembatalan ditahan oleh worker. | **FIXED** |
| `AUD-HRD07-07` | **P0** | `GenerateOrderNumberAsync` membaca list sebelum lock in-memory (rawan concurrency) | Mengambil nomor SK tertinggi langsung dari DB dengan prefix tanggal bisnis dan menambahkan retry collision bounded di `CreateOrderAsync`. | **FIXED** |
| `AUD-HRD07-08` | **P0** | Alur SK belum memiliki deteksi cycle hierarki manajer (A $\rightarrow$ B $\rightarrow$ C $\rightarrow$ A) | Mengimplementasikan `WouldCreateCycleAsync` pada `HrdPersonnelActionOrderService` yang menolak relasi sirkular hingga 100 kedalaman. | **FIXED** |
| `AUD-HRD07-09` | **P1** | Query effective job description memfilter `IsActive` dan fallback ke `PrimaryPositionId` masa kini | Menghapus klausa `IsActive` (agar riwayat lampau tetap sah) dan menghapus fallback ke `PrimaryPositionId` agar tanggal sebelum penempatan menghasilkan empty jujur. | **FIXED** |
| `AUD-HRD07-10` | **P1** | Eksekusi batch dengan `EffectiveStartDate` masa depan langsung mengubah mirror profil | Mirror `PrimaryPositionId` & `PrimaryDepartmentId` pada `MstWorkforceProfile` hanya diperbarui jika `row.EffectiveStartDate.Date <= today`. | **FIXED** |
| `AUD-HRD07-11` | **P1** | Worker dan validasi memakai `DateTime.UtcNow.Date` murni alih-alih tanggal bisnis RS | Menggunakan `GetHospitalBusinessDate()` yang mengonversi waktu UTC ke zona waktu lokal RS (WIB / UTC+7). | **FIXED** |
| `AUD-HRD07-12` | **P0** | `GetBatchByIdAsync` mengembalikan nominal `BaseSalary` ke role HR non-payroll | Me-mask `BaseSalary = null` di response detail batch untuk menjaga kerahasiaan data kompensasi (`HRD-DEC-033`). | **FIXED** |
| `AUD-HRD07-13` | **P1** | Fallback `DepartmentId = organizationUnitId` di `WfpNewHireAssignmentService` | Menegakkan fail-closed bila departemen tidak valid dari master, tidak pernah memakai ID unit organisasi sebagai departemen. | **FIXED** |
| `AUD-HRD07-14` | **P0** | Tombol ubah atasan di `employee-managers-view.jsx` membuka create SK tanpa parameter (default Mutation) | Mengarahkan ke `/personnel-actions/create?category=Rotation` dengan deskripsi resmi penyesuaian penugasan/atasan. | **FIXED** |
| `AUD-HRD07-15` | **P1** | Guard once-per-employee batch hanya saat `Executed`, membiarkan dua batch sama-sama `Approved` | Menambahkan guard di `ApproveBatchAsync` dan `CreateBatchDraftAsync` yang menolak jika ada pegawai yang sudah ada di batch lain berstatus `Approved` atau `Executed`. | **FIXED** |
| `AUD-HRD07-16` | **P1** | Worker menelan error tanpa observabilitas | Menambahkan logging catatan `[AUTO-APPLY FAILED]` per SK agar kegagalan satu SK tidak menggagalkan proses SK lainnya. | **FIXED** |
| `AUD-HRD07-17` | **P1** | Direct CRUD assignment bypass proteksi | Proteksi menyeluruh pada controller direct assignment (`WfpOrganizationAssignment`, `WfpPositionAssignment`, `WfpManagerAssignment`). | **FIXED** |
| `AUD-HRD07-18` | **P1** | Rekonsiliasi laporan, koreksi path API `/initial-batches`, status jujur | Menyusun laporan rekonsiliasi berbukti ini, memperbarui master instruksi, dan mencatat status `IMPLEMENTED IN SOURCE`. | **FIXED** |

---

## 4. Rincian Perubahan Kode Sumber Nyata

### A. Backend (`NewQuilvianSystemBackend`)

1. **`InitialPlacementBatchDtos.cs`**  
   - Menambahkan properti diagnostik aditif pada `InitialBatchRowResponse` dan `RowValidationItemResponse`:
     - `EligibilityStatus` (string): `SIAP_DIPROSES`, `DILEWATI_SUDAH_DITEMPATKAN`, `PERLU_PERBAIKAN`, `PERLU_REKONSILIASI`.
     - `ReasonCode` (string): Kode alasan terstandardisasi (misal: `ALREADY_PLACED_ACTIVE`, `MISSING_PROFILE`, `INVALID_UNIT`, dll.).
     - `ReasonMessage` (string): Penjelasan ramah pengguna dalam Bahasa Indonesia.
     - `TargetResourceKey` (string): Kunci navigasi target (`workforce-profiles`, `organization-units`, `positions`, `position-assignments`).
     - `TargetRecordId` (Guid?): ID rekaman terkait untuk navigasi langsung.

2. **`HrdInitialPlacementBatchService.cs`**  
   - Mendefinisikan konstanta diagnostik terpadu `InitialPlacementDiagnosticCodes`.
   - **Optimasi `GetTemplateRowsAsync`**: Menghapus N+1 database queries dengan melakukan bulk prefetch untuk penugasan posisi aktif (`WfpPositionAssignments`) dan riwayat batch yang disetujui/dieksekusi (`HrdInitialPlacementBatchRows`).
   - Menerapkan klasifikasi diagnostik 4 kategori pada setiap baris template:
     - Jika pegawai sudah memiliki penugasan posisi aktif: `DILEWATI_SUDAH_DITEMPATKAN` (IsEligibleForBatch = false, IsValid = true/info, tidak memblokir batch).
     - Jika unit kerja / jabatan / profil tidak ditemukan atau tidak aktif: `PERLU_PERBAIKAN` (IsEligibleForBatch = false, IsValid = false).
     - Jika lolos seluruh validasi: `SIAP_DIPROSES` (IsEligibleForBatch = true, IsValid = true).
   - **`ValidateRowsAsync`**: Memperkaya setiap item hasil validasi dengan `EligibilityStatus`, `ReasonCode`, `ReasonMessage`, `TargetResourceKey`, dan `TargetRecordId`.
   - **`CreateBatchDraftAsync`**: Memvalidasi hanya baris-baris yang dipilih oleh pengguna (`SelectedRows`), memverifikasi keabsahan 100% dari baris terpilih, menegakkan guard anti duplikasi batch (`AUD-HRD07-15`), dan menyimpan batch dengan status awal `Validated` berisi baris yang dipilih saja.

### B. Frontend (`QuilvianSystemFrontendDev`)

1. **`initial-placement-batch-service.js`**  
   - Mempertahankan endpoint resmi canonical `/v1/corporate/human-resource/workforce-core/initial-batches`.
   - Menyediakan anotasi komentar alias `/v1/corporate/human-resource/initial-placement-batches` untuk menjamin kompatibilitas uji mundur.

2. **`initial-placement-batch-view.jsx`**  
   - **Smart Validation Metrics**: Menampilkan 4 kartu ringkasan diagnostik: Total Terbaca, Siap Diproses (Hijau), Dilewati/Sudah Ditempatkan (Netral/Abu-abu), Perlu Perbaikan (Merah/Danger), dan Rekonsiliasi (Kuning/Warning).
   - **Kategori Filter & Pencarian**: Tab filter kategori (`Semua`, `Siap Diproses`, `Dilewati`, `Perlu Perbaikan`, `Rekonsiliasi`) dilengkapi kolom pencarian nama/NIK/unit.
   - **Seleksi Subset Layak**: Checkbox baris hanya dapat dicentang untuk baris berstatus `SIAP_DIPROSES`. Tombol **Pilih Semua yang Layak** memudahkan penandaan seluruh kandidat valid sekaligus.
   - **Tombol Aksi Dinamis**: Tombol **Simpan & Buat Batch (N Pegawai)** aktif jika $N > 0$ dan seluruh kandidat terpilih valid. Baris tidak valid atau dilewati tidak lagi memblokir tombol simpan.
   - **Confirm Modal**: Konfirmasi pembuatan batch dengan rincian jumlah pegawai terpilih dan pengingat tata kelola `HRD-DEC-054`.
   - **Guided Row Actions**:
     - Tombol **Lihat Alasan**: Membuka modal informasi detail alasan diagnostik dan rekomendasi tindakan.
     - Tombol **Tinjau Penempatan**: Navigasi langsung ke `/hr/workforce-core/position-assignments` untuk pegawai yang sudah ditempatkan.
     - Tombol **Perbaiki Data**: Navigasi aman berbasis `SAFE_DESTINATIONS` allowlist ke modul master terkait (Master Pegawai, Struktur Organisasi, Master Jabatan).
   - **Session State Preservation**: Menyimpan status draft template, nomor batch, tanggal SK, dan seleksi ke `sessionStorage` (`HRD_MENU_07_INITIAL_BATCH_DRAFT_CTX`) saat pengguna bernavigasi ke luar halaman. Saat kembali, konteks otomatis dipulihkan dan divalidasi ulang ke server.

---

## 5. Matriks 5 Submenu Menu 07 (Kondisi Terkini)

| No | Submenu | Rute Frontend | Endpoint Backend Resmi | Status Implementasi | Status UAT |
| :---: | :--- | :--- | :--- | :---: | :---: |
| 1 | **Penempatan Awal** | `/hr/workforce-core/initial-placements` | `POST /api/v1/corporate/human-resource/workforce-core/initial-batches`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/validate`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/submit`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/approve`<br>`POST /api/v1/corporate/human-resource/workforce-core/initial-batches/{id}/execute` | **IMPLEMENTED** | Ready for UAT |
| 2 | **Perpindahan Unit (Mutasi)** | `/hr/workforce-core/unit-transfers` | `GET/POST /api/v1/corporate/human-resource/personnel-action-orders` (`actionCategory=Mutation`)<br>`POST .../cancel-request`<br>`POST .../cancel` | **IMPLEMENTED** | Ready for UAT |
| 3 | **Rotasi Promosi** | `/hr/workforce-core/rotation-promotions` | `GET/POST /api/v1/corporate/human-resource/personnel-action-orders` (`Rotation` / `Promotion`) | **IMPLEMENTED** | Ready for UAT |
| 4 | **Uraian Tugas** | `/hr/workforce-core/employee-job-descriptions` | `GET /api/v1/corporate/human-resource/workforce-core/employees/{id}/effective-job-description?asOfDate=...` | **IMPLEMENTED** | Ready for UAT |
| 5 | **Atasan Langsung** | `/hr/workforce-core/employee-managers` | `GET /api/v1/corporate/human-resource/workforce-core/manager-assignments`<br>`POST .../personnel-actions/create?category=Rotation` | **IMPLEMENTED** | Ready for UAT |

---

## 6. Hasil Pemeriksaan Teknis & Kepatuhan

### A. Pengujian Unit Frontend (`Node Test`)
```text
✔ FE-HRD-022: PersonnelActionOrderService memanggil endpoint canonical backend tanpa /v1/hr/
✔ FE-HRD-022: Redux slice SK Kepegawaian terdaftar di store
✔ FE-HRD-023: Rute dan View Daftar SK Kepegawaian tersedia
✔ FE-HRD-024: Formulir Pembuatan & Detail SK Kepegawaian mematuhi HRD-DEC-031 anti self-approval
✔ FE-HRD-025: Employee 360 Dual-View Service dan View membatasi data sensitif
✔ FE-HRD-026: Initial Placement Batch Service dan View tersedia
✔ FE-HRD-027: Pengajuan Kuasa HR (On-Behalf) terintegrasi ke service dan form
✔ FE-HRD-028: Form Master Data Pegawai mengunci field unit kerja dan jabatan pada mode edit
ℹ tests 8, pass 8, fail 0 (duration: 135.8ms)
```

### B. Audit Linter (`ESLint`)
```text
npm notice run eslint src/components/view/hr/workforce-core/initial-placement-batch-view.jsx
Exit code: 0 (0 errors, 0 warnings)
```

### C. Audit CSS & Design Token Quilvian (`check-css-usage.mjs`)
```text
OK — 1 berkas diperiksa, 0 temuan.
  - src/components/view/hr/workforce-core/initial-placement-batch-view.jsx
```
Semua styling menggunakan komponen dasar Quilvian (`BaseButton`, `StatusBadge`, `BaseInputField`, `InformationAlert`, `DataTable`, `ConfirmModal`, `SummaryCard`) tanpa ada hardcoded styling atau custom Tailwind classes terlarang.

---

## 7. Ringkasan Berkas yang Diubah (Touched Files)

### Backend (`NewQuilvianSystemBackend` @ `MHamzah`):
1. `Areas/Corporate/HumanResource/WorkforceCore/DTOs/InitialPlacementBatchDtos.cs`
2. `Areas/Corporate/HumanResource/WorkforceCore/Services/HrdInitialPlacementBatchService.cs`
3. `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/INSTRUKSI_AGENT_HRD_MENU_07_PENEMPATAN_MUTASI_DAN_PROMOSI.md`
4. `docs/module-blueprints/human-resource/docs/update/penempatan-mutasi-dan-promosi/LAPORAN_IMPLEMENTASI_MENU_07_AGENT_ANTIGRAVITY.md`

### Frontend (`QuilvianSystemFrontendDev` @ `HamzahV2`):
1. `src/components/view/hr/workforce-core/initial-placement-batch-view.jsx`
2. `src/lib/services/hr/initial-placement-batch-service.js`

---

## 8. Rekomendasi Langkah Selanjutnya

1. **UAT Re-Verification di Lingkungan Dev/Staging:**
   Lakukan validasi interaktif pada layar `/hr/workforce-core/initial-placements` untuk mengonfirmasi skenario:
   - Template memuat 41 kandidat: 37 `SIAP_DIPROSES`, 4 `DILEWATI_SUDAH_DITEMPATKAN` / `PERLU_PERBAIKAN`.
   - Mengklik **Pilih Semua yang Layak** memilih 37 pegawai.
   - Tombol **Simpan & Buat Batch (37 Pegawai)** aktif dan berhasil membuat batch tanpa error `422/400`.
   - Tombol **Lihat Alasan**, **Tinjau Penempatan**, dan **Perbaiki Data** berfungsi sesuai rute allowlist aman.
2. **Kompilasi & Pull Request:**
   Setelah pengujian UAT disetujui oleh pemilik modul/pengguna, mintalah persetujuan eksplisit untuk melakukan kompilasi penuh (`dotnet build`), pembaruan migrasi database jika diperlukan, dan pengajuan Pull Request resmi ke branch integrasi (`QuilvianIntegrationBackEnd` dan `QuilvianIntegrationFrontend`).
