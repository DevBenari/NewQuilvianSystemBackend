# Laporan Perubahan Backend — `BE-MDF-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-MDF-004` |
| Judul | API dan Service daftar peran pelaksana jasa medis |
| Slice | `MVP-0` (`EPIC MDF-01`, `MDF-02`) |
| Roadmap | `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` bagian 3 |
| Trace | `MF-DEC-016`, `MDF-DES-003`, `MDF-DES-011`; `MDF-API-1.0` §2; `MDF-PERM-1.0` §1–2; `MDF-VAL-1.0` §1; `erd/data-dictionary.md` bagian 1 |
| Contract version | `MDF-API-1.0`, `MDF-PERM-1.0`, `MDF-VAL-1.0`, seluruhnya terkunci 20 September 2026 |
| Dependency | `BE-MDF-003` — 🟡 sebagian: migration aditif telah dibuat pada source |
| Klasifikasi | `STANDARD` — DTOs, Service, Controller dengan 9 endpoint master data, registrasi DI pada `Program.cs` |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Model | Antigravity |
| Commit backend saat dikerjakan | `548785ac` (branch `Yasmina`) |
| Tanggal | 26 Oktober 2026 |
| Status | `SEBAGIAN` — implementasi kode DTO, Service, Controller, dan registrasi DI selesai lengkap dengan pemetaan seluruh acceptance criteria, namun `dotnet build` dan uji runtime `T-01`..`T-06` belum dijalankan atas permintaan pemilik |

### Backend Governance Preflight

| Butir | Nilai |
| --- | --- |
| Area | `HealthServices` |
| Module | `MedicalFeeManagement` |
| Submodule | `MasterData` |
| Pemilik/prefix pada registry | Baris `Mdf` (ACTIVE) dan `Mst` (ACTIVE) pada `MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Keberlakuan | `NEW CODE` |
| Status registry | `ACTIVE` |
| QBE ID yang berlaku | `QBE-SVC-001` (Service layer memisahkan akses DbContext dari Controller), `QBE-API-001` (Response standar `ApiResponse<T>`, hyphenated route), `QBE-PERM-001` (`[AccessController]`, `[AccessAction]`, `[AccessPermission]`, AccessTypes valid, argumen cocok persis), `QBE-NAM-001` (Penamaan konsisten), `QBE-MOD-001`, `QBE-MOD-002`, `QBE-MOD-003` |
| QBE ID yang tidak berlaku | `QBE-DB-*` — tidak ada migrasi atau modifikasi skema basis data pada task ini |
| Governance terbaca | `AGENTS.md`, `CLAUDE.md`, kontrak engineering dan registry di `docs/engineering/`, `TASK_RULES.md`, `REPORT_TEMPLATE.md`, `status-task-roadmap.md` — seluruhnya terbaca |
| Klasifikasi capability | Master Data — 9 endpoint baseline sesuai `master-data-endpoint-standard.md` |

---

## 1. Masalah yang diperbaiki

Sebelum task ini dikerjakan, entitas master `MstMedicalFeeRole` (dari task `BE-MDF-002`) dan berkas migrasi skemanya (dari task `BE-MDF-003`) telah tersedia, tetapi sistem belum memiliki lapisan API dan Service untuk mengelola data peran tersebut.

Tanpa implementasi API ini:
1. Tidak ada endpoint HTTP bagi antarmuka pengguna (Frontend `FE-MDF-005`) untuk menampilkan tabel daftar peran, memfilter peran aktif/utama, maupun melihat statistik ringkasan.
2. Tidak ada endpoint dropdown ringan (`GET /options`) yang dapat dikonsumsi oleh form kesepakatan tarif sharing (`BE-MDF-006` / `FE-MDF-001`) untuk memilih peran pelaksana tindakan.
3. Aturan bisnis matriks validasi `MDF-VAL-1.0` §1 belum ditegakkan di layer service, meliputi:
   - Validasi format dan keunikan kode peran (`MDF_ROLE_CODE_DUPLICATE`).
   - Validasi pemetaan ke enum tim kamar operasi `OprTeamRole` (`MDF_ROLE_MAPPING_DUPLICATE`).
   - Proteksi aturan 1.6: peran yang sudah dipakai pada baris aturan tarif sharing (`MdfSharingRule`) dilarang dihapus (`MDF_ROLE_IN_USE`) dan wajib dinonaktifkan sebagai gantinya.

Contoh konkret skenario rumah sakit:
Petugas Manajemen Pelayanan Medis / Komite Medik menambahkan peran baru `OPERATOR` (Operator Utama Bedah) dan menautkannya ke padanan peran tim kamar operasi `PrimarySurgeon`. Sistem secara otomatis memastikan tidak ada peran lain yang memetakan `PrimarySurgeon` ganda. Ketika peran `OPERATOR` tersebut telah digunakan dalam kesepakatan pembagian jasa dokter spesialis bedah, tombol hapus pada peran tersebut akan ditolak oleh sistem dengan pesan yang jelas bahwa peran sedang aktif digunakan, sehingga data historis pembagian jasa medis tetap aman dan peran hanya boleh dinonaktifkan jika tidak digunakan lagi di masa depan.

---

## 2. Proses bisnis

| Aspek | Isi |
| --- | --- |
| Tujuan | Menyediakan lapisan Service dan REST Controller untuk pengelolaan data master peran pelaksana jasa medis (`MstMedicalFeeRole`) dengan standar master data Quilvian |
| Pelaku | Staf Administrasi Medis / Pengelola Jasa Medis (memiliki hak akses `MedicalFeeRoles`) |
| Pemicu | Kebutuhan konfigurasi kamus peran sebelum menyusun aturan tarif sharing pada `MVP-0` |

Alur kerja dan invarian bisnis yang ditegakkan:

1. **Pengambilan Metadata & Ringkasan Halaman Index**:
   - `GET /filters/metadata`: Mengembalikan konfigurasi awal filter, opsi pengurutan, daftar opsi kamar operasi (`OprTeamRole`), dan metadata field form untuk merender form create/update secara dinamis dan konsisten.
   - `GET /summary`: Mengembalikan ringkasan statistik master peran (total data, peran aktif, nonaktif, peran utama, dan peran yang terpetakan ke kamar operasi).
2. **Pencarian dan Paginasi Tabel Utama**:
   - `GET /`: Mendukung pencarian teks pada kode, nama, deskripsi, atau pemetaan kamar operasi; filter status aktif (`isActive`), filter peran utama (`isPrimaryRole`), dan filter kepemilikan pemetaan (`hasOprTeamRoleMapping`). Hasil dikembalikan dalam bungkus standar `ApiResponse<PagedResult<MedicalFeeRoleResponse>>`.
3. **Penyediaan Dropdown Ringan (Options Feed)**:
   - `GET /options`: Mengembalikan daftar opsi ringkas berlabel format `"RoleCode — RoleName"` untuk digunakan pada komponen combobox form aturan tarif sharing. Secara default hanya menampilkan peran yang aktif (`onlyActive=true`).
4. **Pembuatan Peran Baru (`POST /`)**:
   - `RoleCode` dinormalisasi menjadi huruf besar tanpa spasi. Format wajib 2–30 karakter alfanumerik atau garis bawah (aturan 1.1).
   - Memeriksa keunikan `RoleCode` terhadap data yang belum dihapus. Jika duplikat, menolak dengan HTTP `409 Conflict` dan kode error `MDF_ROLE_CODE_DUPLICATE` (aturan 1.3).
   - Memvalidasi `OprTeamRoleMapping` bila diisi: wajib bernilai salah satu dari enum `OprTeamRole` (aturan 1.4: `PrimarySurgeon`, `AssistantSurgeon`, `Anesthesiologist`, `ScrubNurse`, `CirculatingNurse`, `Other`). Jika tidak valid, menolak dengan HTTP `400 Bad Request`.
   - Memeriksa keunikan pemetaan: satu nilai `OprTeamRole` maksimal dipetakan ke tepat satu peran (aturan 1.5). Jika duplikat, menolak dengan HTTP `409 Conflict` dan kode error `MDF_ROLE_MAPPING_DUPLICATE`.
5. **Pembaruan Peran (`PUT /{id}`)**:
   - Melakukan validasi yang sama dengan create, dengan mengecualikan ID data yang sedang diedit.
   - Mencatat waktu dan user pengubah pada audit trail (`UpdateDateTime`, `UpdateBy`).
6. **Perubahan Status Aktif (`PATCH /{id}/activation` & `PATCH /{id}/status`)**:
   - Menyediakan endpoint perubahan cepat status aktif/nonaktif tanpa mengirim seluruh payload data. Mendukung rute `/activation` sesuai kontrak `MDF-API-1.0` dan alias rute `/status` sesuai standar baseline master data.
7. **Penghapusan Peran (`DELETE /{id}`)**:
   - Memeriksa ketergantungan relasi: jika peran telah dirujuk oleh baris aturan tarif sharing (`MdfSharingRule`) yang belum dihapus, penghapusan ditolak dengan HTTP `422 Unprocessable Entity` dan kode error `MDF_ROLE_IN_USE` (aturan 1.6).
   - Jika belum pernah dipakai baris tarif, data dihapus secara lunak (soft delete: `IsDelete = true`, `IsActive = false`, mengisi `DeleteDateTime` dan `DeleteBy`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

| Berkas | Untuk apa |
| --- | --- |
| `docs/module-blueprints/medical-fee/roadmap/01-backend-roadmap.md` baris 75 | Memastikan acceptance criteria dan batasan task `BE-MDF-004` |
| `docs/module-blueprints/medical-fee/contracts/api-contract.md` §2 | Route kanonik, HTTP method, payload, dan response code `MDF-API-1.0` |
| `docs/module-blueprints/medical-fee/contracts/permission-audit-matrix.md` §1–2 | Resource name `MedicalFeeRoles`, pemetaan aksi `View`/`Manage`, dan role access rules |
| `docs/module-blueprints/medical-fee/contracts/validation-matrix.md` §1 | Aturan validasi 1.1–1.7 untuk peran pelaksana |
| `docs/module-blueprints/medical-fee/testing/acceptance-test-matrix.md` §1 | Verifikasi kriteria pengujian `T-01`..`T-06` |
| `Areas/HealthServices/OperatingRoomManagement/Enums/OperatingRoomEnums.cs` | Nilai valid enum `OprTeamRole` |
| `Areas/HealthServices/MasterData/Services/BloodBankReasonService.cs` | Pola implementasi service master data Quilvian |
| `Program.cs` | Pendaftaran dependency injection service |

### 3.2 Berkas yang dibuat dan diubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/DTOs/MedicalFeeRoleDtos.cs` | **Baru.** DTO request (`CreateMedicalFeeRoleRequest`, `UpdateMedicalFeeRoleRequest`, `ToggleMedicalFeeRoleActivationRequest`, `MedicalFeeRolePagedQuery`), DTO response (`MedicalFeeRoleResponse`, `MedicalFeeRoleOptionResponse`, `MedicalFeeRoleSummaryResponse`, metadata filter), dan wrapper hasil service `MedicalFeeServiceResult<T>` |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Services/MedicalFeeRoleService.cs` | **Baru.** Lapisan service yang menangani seluruh operasi query, ringkasan, opsi dropdown, metadata filter, operasi create/update dengan validasi duplikasi, toggle aktivasi, dan soft delete dengan proteksi aturan 1.6 (`MDF_ROLE_IN_USE`) |
| `Areas/HealthServices/MedicalFeeManagement/MasterData/Controllers/MedicalFeeRolesController.cs` | **Baru.** Controller ASP.NET Core dengan 9 endpoint baseline, atribut otorisasi `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, pencatatan log audit via `LoggerService`, dan pembungkus `ApiResponse<T>` |
| `Program.cs` | **Diperbarui.** Menambahkan using namespace dan registrasi DI `builder.Services.AddScoped<MedicalFeeRoleService>()` di bawah blok Medical Fee Management |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Memenuhi seluruh spesifikasi `MDF-API-1.0` §2 dan melengkapi 9 endpoint baseline master data sesuai standar Quilvian (`/filters/metadata`, `/summary`, `/options`, `/status`) |
| Database | `NOT APPLICABLE` — Tidak ada migrasi atau modifikasi skema tabel baru |
| Keamanan/Auth | Resource `MedicalFeeRoles` terpasang dengan atribut `[AccessController]`, `[AccessAction]`, dan `[AccessPermission]` yang simetris huruf demi huruf, menggunakan hanya `AccessTypes.Read`, `Create`, `Update`, dan `Delete` tanpa hardcode role |

### 3.4 Selisih terhadap blueprint (Delta Kontrak)

| Butir | Blueprint Awal (`MDF-API-1.0`) | Yang Dikerjakan | Alasan |
| --- | --- | --- | --- |
| Endpoint Baseline Master Data | Hanya mencantumkan 6 endpoint (`GET`, `GET /{id}`, `POST`, `PUT`, `PATCH /activation`, `DELETE`) | Menambahkan 3 endpoint baseline: `GET /filters/metadata`, `GET /summary`, `GET /options`, serta alias `PATCH /{id}/status` | Sesuai panduan `master-data-endpoint-standard.md`, sembilan endpoint baseline adalah standar wajib seluruh master data agar frontend dapat merender filter dan form secara seragam |

---

## 4. Dokumentasi endpoint bergaya Swagger

Tag grup Swagger: `[Tags("Health Services / Medical Fee Management / Master Data / Fee Role")]`
Base Path: `/api/v1/health-services/medical-fee-management/master-data/fee-roles`

| Method | Path | Deskripsi | Auth / Permission | Request Body | Response Success | Error Codes |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Mengambil konfigurasi filter, opsi sort, enum `OprTeamRole`, dan form field metadata | `MedicalFeeRoles` : `Read` | — | `200` `ApiResponse<MedicalFeeRoleFilterMetadataResponse>` | `401`, `403` |
| `GET` | `/summary` | Mengambil ringkasan statistik peran (total, aktif, nonaktif, utama, pemetaan OK) | `MedicalFeeRoles` : `Read` | — | `200` `ApiResponse<MedicalFeeRoleSummaryResponse>` | `401`, `403` |
| `GET` | `/` | Mengambil daftar peran jasa medis dengan filter dan pagination | `MedicalFeeRoles` : `Read` | Query string: `search`, `isActive`, `isPrimaryRole`, `hasOprTeamRoleMapping`, `sortBy`, `sortDirection`, `pageNumber`, `pageSize` | `200` `ApiResponse<PagedResult<MedicalFeeRoleResponse>>` | `401`, `403` |
| `GET` | `/options` | Mengambil opsi ringan peran untuk dropdown select pada form aturan tarif | `MedicalFeeRoles` : `Read` | Query string: `onlyActive` (default `true`), `search` | `200` `ApiResponse<List<MedicalFeeRoleOptionResponse>>` | `401`, `403` |
| `GET` | `/{id}` | Mengambil detail satu peran pelaksana jasa medis | `MedicalFeeRoles` : `Read` | Path param: `id` (`Guid`) | `200` `ApiResponse<MedicalFeeRoleResponse>` | `401`, `403`, `404` |
| `POST` | `/` | Menambahkan peran pelaksana jasa medis baru | `MedicalFeeRoles` : `Create` | `CreateMedicalFeeRoleRequest` (JSON) | `201` `ApiResponse<MedicalFeeRoleResponse>` | `400`, `401`, `403`, `409` (`MDF_ROLE_CODE_DUPLICATE`, `MDF_ROLE_MAPPING_DUPLICATE`) |
| `PUT` | `/{id}` | Mengubah data peran pelaksana jasa medis | `MedicalFeeRoles` : `Update` | Path param: `id` (`Guid`), `UpdateMedicalFeeRoleRequest` (JSON) | `200` `ApiResponse<MedicalFeeRoleResponse>` | `400`, `401`, `403`, `404`, `409` |
| `PATCH` | `/{id}/activation` | Mengubah status aktif peran (rute kontrak `MDF-API-1.0`) | `MedicalFeeRoles` : `Update` | Path param: `id` (`Guid`), `ToggleMedicalFeeRoleActivationRequest` (JSON) | `200` `ApiResponse<MedicalFeeRoleResponse>` | `400`, `401`, `403`, `404` |
| `PATCH` | `/{id}/status` | Mengubah status aktif peran (rute standar master data) | `MedicalFeeRoles` : `Update` | Path param: `id` (`Guid`), `ToggleMedicalFeeRoleActivationRequest` (JSON) | `200` `ApiResponse<MedicalFeeRoleResponse>` | `400`, `401`, `403`, `404` |
| `DELETE` | `/{id}` | Menghapus peran secara soft delete (ditolak bila dipakai baris tarif) | `MedicalFeeRoles` : `Delete` | Path param: `id` (`Guid`) | `200` `ApiResponse<bool>` | `401`, `403`, `404`, `422` (`MDF_ROLE_IN_USE`) |

---

## 5. Verifikasi

| Skenario atau pemeriksaan | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet restore` / `dotnet build` | Tidak dijalankan | `NOT RUN` | Instruksi eksplisit pemilik: **tanpa `dotnet build` otomatis** |
| Checker `Invoke-QbeConformanceCheck.ps1` | Tidak dijalankan | `NOT RUN` | `pwsh` tidak terpasang pada lingkungan eksekusi |
| Review diff dan cakupan perubahan | Tepat 3 berkas baru dan 1 berkas `Program.cs` dimodifikasi | `PASS` | `git status --short` |
| Pemeriksaan kesesuaian atribut AccessPermission | `ControllerName` pada `[AccessController]` cocok persis dengan argumen ke-1 `[AccessPermission]` ("MedicalFeeRoles"); argumen ke-2 cocok dengan `[AccessAction]` ("Read", "Create", "Update", "Delete") | `PASS` | `MedicalFeeRolesController.cs` |
| Penegakan aturan bisnis 1.1-1.5 di layer service | Regex kode peran, keunikan kode (`MDF_ROLE_CODE_DUPLICATE`), validasi enum `OprTeamRole`, dan keunikan pemetaan (`MDF_ROLE_MAPPING_DUPLICATE`) | `PASS` | `MedicalFeeRoleService.cs` metode `CreateAsync` dan `UpdateAsync` |
| Penegakan aturan bisnis 1.6 di layer service | Pengecekan relasi ke `_dbContext.MdfSharingRules` sebelum delete; ditolak 422 dengan kode `MDF_ROLE_IN_USE` | `PASS` | `MedicalFeeRoleService.cs` metode `DeleteAsync` |
| Pembungkus respon standar | Seluruh endpoint controller membungkus output dengan `ApiResponse<T>` dan paging dengan `PagedResult<T>` | `PASS` | `MedicalFeeRolesController.cs` |
| Pemeriksaan karakter baris (Line endings) | LF murni, UTF-8 tanpa BOM, nol karakter `\r` | `PASS` | `grep -c $'\r'` bernilai 0 pada seluruh berkas baru dan yang dimodifikasi |
| Skenario uji fungsional `T-01`..`T-06` | Tidak dijalankan | `NOT RUN` | Menuntut runtime server dan database aktif; backend tidak memelihara project test otomatis |

AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| Acceptance: "Peran terpakai dinonaktifkan, bukan dihapus" | Terpetakan ke source | `MedicalFeeRoleService.cs` — `DeleteAsync` memeriksa `_dbContext.MdfSharingRules.AnyAsync(r => r.RoleId == id && !r.IsDelete)`. Bila ada, mengembalikan gagal 422 dengan pesan instruksi untuk menonaktifkan peran sebagai gantinya |
| Kontrak API: Route `api/v1/health-services/medical-fee-management/master-data/fee-roles` | Terpenuhi | `[Route("api/v1/health-services/medical-fee-management/master-data/fee-roles")]` pada `MedicalFeeRolesController.cs` |
| Kontrak API: Pembungkus `ApiResponse<T>` | Terpenuhi | Mengembalikan `ApiResponse<T>.Ok` atau `ApiResponse<object>.Fail` pada seluruh action |
| Kontrak Izin: `[AccessPermission]` terpasang | Terpenuhi | Ditegakkan pada 9 endpoint dengan format `[AccessPermission("MedicalFeeRoles", "<Action>")]` |
| Pendaftaran Dependency Injection | Terpenuhi | `builder.Services.AddScoped<MedicalFeeRoleService>()` terdaftar pada `Program.cs` |
| Validasi build | **Belum terpenuhi** | `NOT RUN` atas instruksi eksplisit pemilik (tanpa build otomatis) |

Catatan: Status task ditandai sebagai `🟡 SEBAGIAN` karena implementasi kode telah lengkap dan memenuhi acceptance criteria pada source, namun kompilasi `dotnet build` dan verifikasi skenario uji `T-01`..`T-06` belum dijalankan sesuai instruksi batasan lingkungan.

---

## 7. Catatan penutup dan Langkah Selanjutnya

1. **Kesiapan Konsumsi Frontend**:
   API peran jasa medis telah siap dikonsumsi oleh task frontend `FE-MDF-005` (Pengelolaan daftar peran) setelah backend dikompilasi dan dijalankan.
2. **Task Berikutnya**:
   - `BE-MDF-006`: Pembuatan layanan kesepakatan tarif berversi (`MedicalFeeSharingService` dan `MedicalFeeSharingAgreementsController`).
   - `BE-MDF-007`: Perancangan entity periode, hasil jasa, rincian, koreksi, dan daftar layanan belum terhitung (`MdfFeePeriod`, `MdfServiceFee`, `MdfServiceFeeDetail`, `MdfServiceFeeAdjustment`, `MdfUnresolvedService`).

