# LAPORAN IMPLEMENTASI FINISHING PENGATURAN PERENCANAAN SDM

**Tanggal Pelaporan:** 26 November 2026  
**Modul:** Sumber Daya Manusia (Human Resource)  
**Menu:** Perencanaan Strategis SDM  
**Submenu:** Pengaturan Perencanaan (`/hr/workforce-planning/settings`)  
**Repositori Backend:** `DevBenari/NewQuilvianSystemBackend` (branch `MHamzah`, referensi awal `b1cb233333edc41474e3867f53c034b886a08c81`)  
**Repositori Frontend:** `DevBenari/QuilvianSystemFrontendDev` (branch `HamzahV2`, referensi awal `7dfae4cdb444ff83dd8fe84c49d7c26de7eb4eec`)  
**Status Tugas:** `BE-HRD-FIN-WFP-001` s.d. `005` & `FE-HRD-FIN-WFP-001` s.d. `007` — **SELESAI & TERVERIFIKASI**

---

## 1. Ringkasan Eksekutif

Penyelesaian (finishing) submenu **Pengaturan Perencanaan** pada modul **Perencanaan Strategis SDM** mengubah layar pengisian tunggal sebelumnya menjadi modul tata kelola konfigurasi rumah sakit yang terstruktur, persisten saat halaman di-refresh (`F5`), aman secara hak akses dan cakupan lokasi, serta informatif bagi pengelola SDM maupun pimpinan rumah sakit.

Sebelum perbaikan ini dilakukan, pengguna berhasil menyimpan konfigurasi dan menerima notifikasi sukses, namun setiap kali halaman dimuat ulang (`F5`), pilihan rumah sakit beserta angka batas hari kembali kosong. Penelusuran kode membuktikan bahwa penyebab utama bukan kegagalan penyimpanan database, melainkan desain tampilan lama yang menginisialisasi state `hospitalSiteId` kosong (`""`) pada browser tanpa halaman daftar maupun rute detail berbasis parameter URL. Pada sisi Backend, sistem juga sebelumnya belum menyediakan endpoint daftar seluruh rumah sakit dengan status kelengkapan dan informasi nama pengguna pembaruan terakhir.

Kini, pengguna melihat ringkasan kelengkapan seluruh rumah sakit, tabel daftar status konfigurasi dengan pencarian/filter/pagination, serta halaman detail/ubah tersendiri di rute `/hr/workforce-planning/settings/[hospitalSiteId]` yang menampilkan parameter aktif, status kelengkapan (`Lengkap`, `Belum Lengkap`, `Belum Diatur`), waktu pembaruan terakhir, nama pengguna pembaru, dan alasan perubahan terakhir.

---

## 2. Daftar Permasalahan Sebelumnya & Root Cause Terverifikasi

### 2.1 Daftar Keluhan UAT Existing

1. Pengaturan berhasil diisi dan menampilkan toast tersimpan, tetapi setelah halaman di-refresh (`F5`), pilihan rumah sakit dan seluruh parameter kembali kosong.
2. Tidak tersedia tabel daftar pengaturan seluruh rumah sakit.
3. Tidak tersedia indikator rumah sakit mana yang sudah lengkap, belum lengkap, atau belum pernah diatur.
4. Tidak tersedia informasi waktu pembaruan terakhir secara jelas pada alur kerja pengguna.
5. Tidak tersedia nama pengguna yang terakhir memperbarui konfigurasi (sebelumnya API hanya mengembalikan GUID `UpdatedBy`).
6. Tidak tersedia tampilan alasan perubahan terakhir sebagai jejak audit kebijakan.
7. Tampilan sebelumnya hanya berupa satu form memanjang selebar halaman.
8. Pengguna tidak mengetahui parameter yang sedang berlaku saat membuka menu.

### 2.2 Root Cause Terverifikasi pada Source Code

| Lapisan | Lokasi Berkas & Baris Awal | Akar Masalah Terverifikasi | Dampak ke Pengguna |
|---|---|---|---|
| **Frontend State** | `workforce-planning-settings-view.jsx:59, 69-74` | State awal `const [hospitalSiteId, setHospitalSiteId] = useState("");` dan `useEffect` mengosongkan `form` jika `!hospitalSiteId`. | Saat halaman di-refresh, `hospitalSiteId` kembali `""`, sehingga `GET .../settings` tidak pernah dipanggil dan seluruh field menjadi kosong. |
| **Frontend UX** | `workforce-planning-settings-view.jsx:150-198` | Tidak ada halaman daftar (`LEFT JOIN`), tidak ada kartu ringkasan, tidak ada rute `[hospitalSiteId]`, dan form menampilkan `changeReason` kosong tanpa menampilkan riwayat alasan terakhir. | Pengelola SDM tidak tahu rumah sakit mana yang belum diatur atau siapa yang terakhir mengubah kebijakan. |
| **Frontend Validation** | `workforce-planning-settings-view.jsx:32-40` + `base-form-control.jsx:120-139` | `min: 1` dan `max: 365` pada `BaseInputField` menyebabkan `normalizeNumberInputValue` memaksa angka `0` menjadi `1` dan `366` menjadi `365` secara diam-diam saat mengetik. | Pengguna bisa mengira angka invalid otomatis terkoreksi tanpa peringatan validasi yang semestinya menolak input `0` dan `366`. |
| **Backend List & DTO** | `HrdWorkforcePlanningSettingService.cs` & `WorkforcePlanningSettingDtos.cs` | Belum tersedia endpoint `GET .../settings/list` (`LEFT JOIN` `MstHospitalSite` dengan `HrdWorkforcePlanningSetting`), `ConfigurationStatus`, serta resolusi `UpdatedByName`. | Frontend tidak bisa menampilkan status kelengkapan menyeluruh maupun nama pengguna pembaru dalam satu panggilan daftar yang efisien. |
| **Backend Guard** | `HrdWorkforcePlanningSettingService.cs:87-180` | Belum memeriksa panjang maksimal `ChangeReason > 500` sebelum menyentuh database, belum menangkap `DbUpdateException` pelanggaran indeks unik saat insert bersamaan untuk status `409`, dan belum menerapkan cakupan rumah sakit per penempatan manajerial. | Risiko galat `500` saat panjang alasan melebihi batas kolom DB atau saat dua sesi membuat konfigurasi pertama secara bersamaan. |

---

## 3. Verifikasi Persistence Database

Verifikasi langsung terhadap basis data PostgreSQL pengujian (`QuilvianNewDevHamzah`) dan SQLite in-memory membuktikan:

1. Tabel existing `public."HrdWorkforcePlanningSetting"` tidak diubah strukturnya (nol tabel baru, nol perubahan migration).
2. Indeks unik terfilter existing `IX_HrdWorkforcePlanningSetting_Site` pada kolom `HospitalSiteId` (`"IsDelete" = false`) menjamin maksimal satu baris konfigurasi aktif per rumah sakit.
3. Pengujian `GET -> PUT -> DbContext Baru GET` di xUnit maupun `PUT -> redirect -> GET /list -> buka detail -> F5 Reload` di live PostgreSQL membuktikan nilai `LongLeaveThresholdDays`, `OverdueRequestThresholdDays`, `ChangeReason`, `UpdatedAt`, `UpdatedBy`, dan `RowVersion` tersimpan permanen serta dapat dibaca kembali dengan nilai identik.

**Contoh Skenario Rumah Sakit Konkret:**  
Misalkan Kepala Bagian SDM rumah sakit induk ingin mengevaluasi kekurangan perawat dan dokter di tiga lokasi fasilitas kesehatan di bawah manajemen yang sama:
- **Metropolitan Medical Centre Hospital (`SITE-MMC-001`)**: sudah mengatur batas cuti panjang `30 hari` dan batas umur permintaan `60 hari` -> status otomatis terbaca **Lengkap** (`Complete`).
- **Metropolitan Diagnostic Centre (`SITE-MDC-001`)**: baru mengisi batas cuti panjang `21 hari` sedangkan batas umur permintaan masih dibiarkan kosong (`null`) -> status otomatis terbaca **Belum Lengkap** (`Incomplete`), tanpa pernah dipalsukan menjadi `0 hari`.
- **Metropolitan Healthcare Services Head Office (`SITE-MHS-001`)**: belum pernah diatur sama sekali -> tetap muncul di daftar lewat `LEFT JOIN` `MstHospitalSite`, dengan parameter `Belum diatur` dan status **Belum Diatur** (`NotConfigured`).

---

## 4. Ringkasan Alur Bisnis End-to-End

Urutan proses dari pembukaan menu sampai penyimpanan dan verifikasi ulang:

1. **Pengecekan Hak Akses**: Sistem memeriksa izin `WorkforcePlanningSetting : Read` (untuk membuka daftar/detail) dan `WorkforcePlanningSetting : Update` (untuk melihat tombol ubah dan menyimpan). Pengguna tanpa izin baca melihat layar `AccessDeniedAlert`.
2. **Pemanggilan Daftar (`GET .../settings/list`)**: Backend melakukan `LEFT JOIN` dari seluruh rumah sakit aktif (`MstHospitalSite`) ke `HrdWorkforcePlanningSetting` dalam cakupan akses pengguna, menghitung ringkasan global (Total Rumah Sakit, Lengkap, Belum Lengkap / Belum Diatur) sebelum pagination/pencarian, dan meresolusi `UpdatedByName` secara batch dari `AspNetUsers`.
3. **Pemfilteran & Pencarian**: Pengguna menyaring daftar menurut kata kunci nama/kode rumah sakit atau status kelengkapan (`Lengkap`, `Belum Lengkap`, `Belum Diatur`) serta mengatur jumlah baris per halaman.
4. **Navigasi ke Halaman Detail/Ubah**: Mengklik baris atau tombol aksi (`Detail`, `Atur`, `Lengkapi`, `Ubah`) memindahkan pengguna ke halaman terpisah `/hr/workforce-planning/settings/[hospitalSiteId]`.
5. **Pemuatan Detail (`GET .../settings?hospitalSiteId=...`)**: Halaman membaca `hospitalSiteId` langsung dari URL. Jika pengguna menekan `F5`, halaman kembali memanggil endpoint dengan parameter URL yang sama sehingga nilai dan metadata tetap utuh.
6. **Validasi & Penyimpanan (`PUT .../settings`)**:
   - Angka batas hari harus `null` (dikosongkan) atau bilangan bulat `1..365`.
   - Alasan perubahan wajib diisi (`1..500` karakter) untuk mencegah perubahan kebijakan tanpa akuntabilitas.
   - `RowVersion` diverifikasi untuk mencegah penimpaan diam-diam bila pengguna lain baru saja menyimpan lebih dulu (`HTTP 409`).
7. **Tindak Lanjut Pasca Simpan**: Jika sukses, pengguna diarahkan kembali ke `/hr/workforce-planning/settings` yang memuat ulang daftar terbaru beserta toast konfirmasi. Jika terjadi konflik `409`, nilai ketikan pengguna di form tetap dipertahankan dan muncul banner dengan tombol **Muat Konfigurasi Terbaru**.

---

## 5. Implementasi Backend & Dokumentasi Endpoint Bergaya Swagger

### 5.1 Perubahan File Backend

1. `Areas/Corporate/HumanResource/WorkforcePlanning/DTOs/WorkforcePlanningSettingDtos.cs`
   - Menambahkan konstanta dan fungsi deterministik `WorkforcePlanningSettingStatuses.Compute(...)` (`Complete`, `Incomplete`, `NotConfigured`).
   - Memperluas `WorkforcePlanningSettingResponse` dengan `HospitalSiteCode`, `ConfigurationStatus`, dan `UpdatedByName`.
   - Menambahkan `WorkforcePlanningSettingListQuery`, `WorkforcePlanningSettingSummaryResponse`, dan `WorkforcePlanningSettingListResponse : PagedResult<WorkforcePlanningSettingResponse>`.
   - Menambahkan `[MaxLength(500)]` pada `UpdateWorkforcePlanningSettingRequest.ChangeReason`.
2. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/IHrdWorkforcePlanningSettingService.cs`
   - Menambahkan kontrak `GetListAsync(...)` serta overload `GetByHospitalSiteIdAsync(hospitalSiteId, currentUserId, cancellationToken)` dengan tetap mempertahankan overload 2 parameter untuk pemanggil internal (`HrdStaffingGapCalculationService`, `HrdHeadcountFulfilmentService`).
3. `Areas/Corporate/HumanResource/WorkforcePlanning/Services/HrdWorkforcePlanningSettingService.cs`
   - Implementasi `GetListAsync` dengan `LEFT JOIN` `MstHospitalSites` + `HrdWorkforcePlanningSettings`, agregasi ringkasan menyeluruh pada SQL sebelum filter/pagination, resolusi `UpdatedByName` dalam satu batch query dari `Users`, dan tanpa N+1 query.
   - Penegakan pembatasan cakupan rumah sakit berdasarkan penempatan manajerial aktif (`WfpOrganizationAssignment` dengan `IsManagerialAssignment = true`) untuk pengguna di luar peran administratif penuh/SuperAdmin.
   - Penanganan `DbUpdateConcurrencyException` dan `DbUpdateException` pelanggaran indeks unik menjadi `HTTP 409 Conflict`.
   - Validasi eksplisit panjang `ChangeReason` maksimal 500 karakter (`HTTP 400`).
4. `Areas/Corporate/HumanResource/WorkforcePlanning/Controllers/HrdWorkforcePlanningSettingController.cs`
   - Menambahkan endpoint `[HttpGet("list")]` dengan anotasi izin yang sama persis (`WorkforcePlanningSetting : Read`).
   - Memperbarui `[HttpGet]` agar meneruskan `GetCurrentUserId()` demi penegakan cakupan akses rumah sakit.
5. `Tests/QuilvianSystemBackend.HumanResourceTests/WorkforcePlanningSettingTests.cs`
   - Menambahkan 16 skenario uji terotomasi xUnit (mencakup seluruh poin `BE-HRD-FIN-WFP-005`).

### 5.2 Spesifikasi Endpoint Bergaya Swagger

**Tag Grup Controller:** `[Tags("Corporate / Human Resource / Workforce Planning / Setting")]`  
**Base Route:** `/api/v1/corporate/human-resource/workforce-planning/settings`

| Method | Path | Deskripsi | Hak Akses | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/list` | Daftar seluruh rumah sakit aktif (`LEFT JOIN`) beserta status kelengkapan, pembaru terakhir, dan ringkasan agregat global. | `WorkforcePlanningSetting : Read` | Query: `search`, `hospitalSiteId`, `configurationStatus`, `pageNumber`, `pageSize` | `200 OK`: `ApiResponse<WorkforcePlanningSettingListResponse>` (`summary`, `pageNumber`, `pageSize`, `totalData`, `totalPage`, `items`) · `400 Bad Request` · `403 Forbidden` |
| `GET` | `/` | Mengambil konfigurasi satu rumah sakit beserta status kelengkapan dan nama pembaru terakhir (`null` bila belum diatur). | `WorkforcePlanningSetting : Read` | Query: `hospitalSiteId` (`Guid`, wajib) | `200 OK`: `ApiResponse<WorkforcePlanningSettingResponse>` · `400 Bad Request` · `403 Forbidden` · `404 Not Found` |
| `PUT` | `/` | Membuat konfigurasi pertama atau memperbarui konfigurasi rumah sakit yang sudah ada. | `WorkforcePlanningSetting : Update` | Body JSON: `UpdateWorkforcePlanningSettingRequest` (`hospitalSiteId`, `longLeaveThresholdDays`, `overdueRequestThresholdDays`, `changeReason`, `rowVersion`) | `200 OK`: `ApiResponse<WorkforcePlanningSettingResponse>` · `400 Bad Request` · `403 Forbidden` · `404 Not Found` · `409 Conflict` |

---

## 6. Implementasi Frontend & Struktur Layar

### 6.1 Perubahan & Penambahan File Frontend

| Jenis | File | Ringkasan Perubahan |
|---|---|---|
| Service | `src/lib/services/hr/workforce-planning-service.js` | Menambahkan `getSettingsList(params, signal)`. |
| Constants | `src/lib/constants/hr/workforce-planning/workforce-planning-constants.js` | Menambahkan `settingsList`, pemetaan badge `WORKFORCE_PLANNING_SETTING_STATUS`, opsi filter status, aturan validasi, flash key, dan helper rute detail. |
| Utils | `src/utils/hr/workforce-planning/workforce-planning-display-utils.js` | Menambahkan `resolveWorkforcePlanningSettingStatus`, `getWorkforcePlanningSettingStatusMeta`, `formatThresholdDays`, `toThresholdFormValue`, `validateWorkforcePlanningSettingInput`, dan `getWorkforcePlanningSettingActionLabel`. |
| Table Columns | `src/components/view/hr/workforce-planning/workforce-planning-settings-table-columns.jsx` | Definisi 7 kolom daftar (`No.`, `Rumah Sakit`, `Batas Cuti Panjang`, `Batas Umur Permintaan`, `Status`, `Terakhir Diperbarui`, `Aksi`) memakai `StatusBadge` dan `BaseButton` dengan lebar pas untuk viewport desktop `>= 1366px`. |
| List View | `src/components/view/hr/workforce-planning/workforce-planning-settings-view.jsx` | Restrukturisasi total halaman daftar: `Hero`, 3 kartu `SummaryGrid`, `InformationAlert`, `DataFilter`, `DataTable` + `RegionPagination`, `ToastStack`, serta `AccessDeniedAlert`. Tidak ada form input inline maupun modal. |
| Detail View | `src/components/view/hr/workforce-planning/workforce-planning-settings-detail-view.jsx` | Halaman detail & ubah mandiri berbasis parameter `hospitalSiteId`: layout 2 kolom responsif (`detailLayoutWithSidePanel`), panel kiri parameter + counter alasan, panel kanan `Informasi Pengaturan` + `Aksi Pengaturan`, penanganan `409 Conflict`, serta mode read-only untuk akun tanpa izin `Update`. |
| Route | `src/app/hr/workforce-planning/settings/[hospitalSiteId]/page.jsx` | Rute Next.js App Router dinamis untuk halaman detail/ubah per rumah sakit (tidak didaftarkan ke menu sidebar). |
| CSS Module | `src/style/hr/workforce-planning/workforce-planning-settings.module.css` | CSS Module khusus memakai token desain `globals.css` (`var(--...)`) untuk tata letak form detail, kotak alasan perubahan terakhir, serta jarak aman di atas footer melayang. |
| Unit Test FE | `tests/unit/workforce-planning-settings.test.mjs` | Suite uji `node:test` untuk konstanta kontrak, fungsi kalkulasi status, validasi input batas hari/alasan, serta guard arsitektur. |

---

## 7. Mekanisme Status Kelengkapan, Refresh, Concurrency, dan Keamanan

1. **Klasifikasi Status Kelengkapan**:
   - Kedua parameter terisi -> `Complete` (UI: badge hijau **Lengkap**).
   - Hanya satu parameter terisi -> `Incomplete` (UI: badge kuning **Belum Lengkap**).
   - Kedua parameter `null` (baik belum punya baris di DB maupun baris berparameter kosong) -> `NotConfigured` (UI: badge netral **Belum Diatur**).
   - Nilai `null` tetap dirender `"Belum diatur"`, tidak pernah diubah menjadi `0`.
2. **Penanganan Refresh Halaman (`F5`)**:
   - Pada `/settings/[hospitalSiteId]`, identitas rumah sakit dibaca langsung dari segmen URL Next.js, bukan dari state kosong atau `localStorage`, lalu di-fetch ke Backend setiap pemuatan/refresh sehingga parameter dan metadata pembaruan tetap tampil akurat.
3. **Optimistic Concurrency & Duplikasi Unik (`HTTP 409`)**:
   - Ketika `PUT` ditolak `409`, nilai ketikan lokal tetap dipertahankan agar pengguna tidak kehilangan draf, disertai `InformationAlert` berisi tombol **Muat Konfigurasi Terbaru**.
4. **Otorisasi & Cakupan Lokasi**:
   - Pengguna tanpa `WorkforcePlanningSetting : Read` (`endang.purwanti@rsmmc.local`) menerima penolakan akses (`AccessDeniedAlert` pada UI dan `403 Forbidden` pada API).
   - Pengguna hanya berizin `Read` (`yudha.hermawan@rsmmc.local`) melihat label aksi **Detail**, seluruh isian nonaktif pada halaman detail, tanpa tombol **Simpan Pengaturan**, dan Backend tetap menolak `PUT` langsung dengan `403`.
   - Pengguna administratif SDM berizin `Read` + `Update` (`hartini.sulistyowati@rsmmc.local`) dapat mengelola konfigurasi lintas rumah sakit dalam cakupan aksesnya. Pengguna dengan penempatan manajerial terikat (`IsManagerialAssignment = true`) otomatis dibatasi hanya ke `HospitalSiteId` yang menjadi cakupannya pada endpoint daftar, detail, maupun update.

---

## 8. Hasil Pengujian Terotomasi (Backend & Frontend)

### 8.1 Pengujian Backend

1. **QBE Conformance Check (`Invoke-QbeConformanceCheck.ps1 -Mode Strict`)** terhadap seluruh 5 file Backend yang diubah/ditambah:
   - `Files evaluated: 5`, `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, **Hasil: `PASS`**.
2. **Kompilasi Aplikasi Utama (`QuilvianSystemBackend.csproj` - Configuration `Release`)**:
   - Sukses mengompilasi `bin\Release\net9.0\QuilvianSystemBackend.dll` dan berjalan normal pada `https://localhost:7184` terhubung ke PostgreSQL `QuilvianNewDevHamzah`. *(Catatan: proyek uji penuh `QuilvianSystemBackend.HumanResourceTests` pada branch `MHamzah` memiliki kendala pre-existing pada file uji modul lain `WorkforcePayrollDataSaveTests.cs` dan `WorkforcePayrollDataContractTests.cs` akibat perubahan modul payroll sebelumnya, serta `TestDatabase.cs` bersama belum menyertakan filter fungsi `btrim` milik konfigurasi payroll; oleh karena itu pengujian dieksekusi lewat runner terisolasi `WfpSettingRunner` terhadap binary Backend hasil kompilasi nyata dengan menyertakan `WorkforcePlanningSettingTests.cs` dan tetap tidak mengubah file modul lain.)*
3. **Suite xUnit `WorkforcePlanningSettingTests` (`WfpSettingRunner.csproj`)**:
   - **Total: 16 test cases, 16 Passed, 0 Failed** — mencakup:
     - `GET .../settings/list` LEFT JOIN & ringkasan global.
     - `GET .../settings?hospitalSiteId=...` untuk RS terkonfigurasi vs belum diatur.
     - `PUT` konfigurasi awal -> `PUT` ubah -> `GET` ulang dari `DbContext` baru.
     - Validasi alasan kosong (`""`, `"   "`) -> `400`.
     - Validasi rentang di luar `1..365` (`0`, `-5`, `366`) -> `400`.
     - Validasi panjang alasan `501` karakter -> `400`.
     - Pencarian, filter status (`Complete`/`Incomplete`/`NotConfigured`), filter RS, & pagination dengan menjaga ringkasan total.
     - Konflik `RowVersion` (`409`) & penegakan indeks unik per rumah sakit.
     - Validasi `PermissionRegistryValidator` & `AccessPermissionService` (`Read`, `Update`, tanpa izin).
     - Cakupan akses rumah sakit bagi akun manajerial terbatas (`403 Forbidden` pada list terfilter, detail, dan update di luar cakupan).
4. **Verifikasi Live HTTP API pada Backend Terpasang (`https://localhost:7184` & PostgreSQL `QuilvianNewDevHamzah`)**:
   - `GET .../settings/list`: `200 OK`, 3 rumah sakit aktif (`SITE-MMC-001` Lengkap, `SITE-MDC-001` & `SITE-MHS-001` Belum Diatur), `UpdatedByName` terselesaikan dari DB.
   - `GET .../settings/list?search=Diagnostic`: `200 OK`, `totalData=1`, `summary.totalHospitalSites=3`.
   - `PUT` batas `366` / `0` / panjang alasan `501` karakter / stale `RowVersion` masing-masing mengembalikan `400` dan `409` sesuai kontrak.
   - Hak akses live: `yudha` `GET list = 200`, `PUT = 403`; `endang` `GET list = 403`.

### 8.2 Pengujian Frontend & Build

| Pemeriksaan | Cakupan | Hasil Terobservasi Setelah Edit Terakhir |
|---|---|---|
| `node check-css-usage.mjs` | 4 berkas JSX yang diubah/dibuat (`settings-view`, `table-columns`, `detail-view`, `[hospitalSiteId]/page`) | `OK — 4 berkas diperiksa, 0 temuan.` |
| `node --test tests/unit/workforce-planning-settings.test.mjs` | Unit test kontrak konstanta, helper status, validator rentang `1..365` & alasan `<= 500`, guard arsitektur | **4 passed, 0 failed** |
| `npm run lint:errors` (`eslint . --quiet`) | Seluruh repositori `QuilvianSystemFrontendDev` | **Exit code 0 (0 ESLint errors)** |
| `npm run build` (`Next.js 16.2.12 Turbopack` + `verify-version-manifest` + `prepare-standalone`) | Kompilasi produksi penuh termasuk `/hr/workforce-planning/settings` dan `/hr/workforce-planning/settings/[hospitalSiteId]` | **Exit code 0**, 942 halaman selesai dibuat, standalone runtime siap. *(Catatan baseline suite penuh `npm run test:unit` sebelum penyesuaian lebar kolom akhir mencatat 5421 pass dan 149 gagal pre-existing pada file uji modul lain seperti `workforce-salary-assignment.test.mjs`, `payroll-*`, `billing-*`, dengan 0 kegagalan pada `workforce-planning-*`.)* |

---

## 9. Hasil Live Browser Testing (Playwright Chromium Headed Mode)

Pengujian langsung di peramban Chromium visual (`headless: false`, `slowMo: 400`) dilaksanakan menggunakan skrip `QuilvianSystemFrontendDev/test-with-agy/scripts/wfp-settings-uat.mjs` pada URL `http://localhost:3000` dengan Backend & PostgreSQL nyata:

- **Ringkasan Eksekusi**: `15/15 skenario PASS`, `0` uncaught `pageErrors`, `0` failed API calls (`>= 500`), `1` pesan console HTTP `409` yang memang dipicu secara sengaja pada `UAT-WFP-011`.
- **Artefak Tangkapan Layar (`QuilvianSystemFrontendDev/test-with-agy/screenshots/wfp-settings/`)**:
  - `01-list-page.png` (`UAT-WFP-001`): Halaman daftar, 3 Summary Cards, Filter, Tabel LEFT JOIN, StatusBadge, dan tombol `Detail` / `Ubah` / `Atur` utuh tanpa terpotong.
  - `02-detail-not-configured.png` (`UAT-WFP-002`): Halaman detail rumah sakit belum diatur (`SITE-MHS-001`) menampilkan field kosong (bukan `0`) dan alert belum memiliki pengaturan.
  - `03-detail-configured.png` (`UAT-WFP-002`): Halaman detail rumah sakit sudah lengkap (`SITE-MMC-001`) menampilkan nilai berlaku (`30` / `60`), `Informasi Pengaturan`, `Alasan Perubahan Terakhir`, dan tanpa tombol Simpan di Hero.
  - `04-after-update-list.png` (`UAT-WFP-003`): Daftar terbarui otomatis setelah PUT (`35 hari`, `75 hari`, `Oleh: Hartini Sulistyowati, S.Psi., M.M.`).
  - `05-after-refresh-persistence.png` (`UAT-WFP-004`): Halaman detail sesudah reload penuh (`page.reload`) mempertahankan pilihan RS, nilai `35`/`75`, dan alasan perubahan terakhir.
  - `06-partial-incomplete.png` (`UAT-WFP-005`): Penyimpanan sebagian (`21` dan kosong) pada `SITE-MDC-001` menghasilkan badge kuning **Belum Lengkap** dan toast konfirmasi.
  - `07-complete-updated.png` (`UAT-WFP-006`): Pelengkapan parameter kedua (`45 hari`) memperbarui Summary Card (`Sudah Lengkap: 2`, `Belum Lengkap / Belum Diatur: 1`).
  - `08-validation-empty-reason.png`, `09-validation-366.png`, `10-validation-0.png` (`UAT-WFP-007`..`009`): Penolakan alasan kosong, angka `366`, dan angka `0` tanpa silent clamping serta menjaga nilai input.
  - `11-search-filter.png` (`UAT-WFP-010`): Filter status `Belum Diatur` + pencarian `"Head Office"` dan tombol reset kembali menampilkan 3 baris.
  - `12-conflict-409.png` (`UAT-WFP-011`): Banner peringatan `HTTP 409` dengan tombol **Muat Konfigurasi Terbaru** sambil mempertahankan input pengguna lokal.
  - `13-readonly-yudha.png` & `14-denied-endang.png` (`UAT-WFP-012`): Peran `Read`-only menonaktifkan ubah/simpan; akun tanpa izin menampilkan `AccessDeniedAlert`.
  - `15-mobile-list.png` & `16-mobile-detail.png` (`UAT-WFP-013`): Tampilan viewport mobile `390x844` bebas horizontal overflow (`scrollWidth <= innerWidth + 4`).
  - `17-regression-gaps.png` & `18-regression-headcount.png` (`UAT-WFP-014`, `015`): Menu **Evaluasi Kebutuhan** dan **Pengajuan Kebutuhan Tenaga** tetap berfungsi normal tanpa regresi.

---

## 10. Pemulihan Data Uji & Daftar Berkas di Luar Ruang Lingkup

- **Pemulihan Konfigurasi Database Uji**: Setelah skenario UAT selesai, konfigurasi `Metropolitan Medical Centre Hospital` (`SITE-MMC-001`) telah dipulihkan via endpoint resmi ke nilai awal (`longLeaveThresholdDays = 30`, `overdueRequestThresholdDays = 60`, alasan `"Penetapan awal parameter perencanaan kebutuhan tenaga kerja untuk simulasi UAT tahun 2027."`), parameter `Metropolitan Diagnostic Centre` (`SITE-MDC-001`) dikembalikan menjadi `null` / `null` (status `NotConfigured`), dan `Metropolitan Healthcare Services Head Office` (`SITE-MHS-001`) tetap bersih tanpa baris pengaturan (`id: null`).
- **Catatan Perubahan Lain Developer**: Perubahan prae-eksisting milik modul lain di repositori Frontend (seperti modul Recruitment dan Home Dashboard) maupun modul lain di repositori Backend tidak disentuh, tidak di-reset, dan tetap dijaga utuh.
