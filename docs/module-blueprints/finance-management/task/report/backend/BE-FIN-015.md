# Laporan Perubahan Backend — `BE-FIN-015`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-015` |
| Judul | API setoran dan kas harian — `FinanceBankDepositsController`, `FinanceDailyCashController` |
| Slice | `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task baris `BE-FIN-015`) dan bagian 4 (`MVP-4`) |
| Trace | `FIN-DES-018`, `FIN-DES-021`, `FIN-DES-022`; `FR-FIN-060`..`065`; kontrak `FIN-API-1.0`, `FIN-PERM-1.0`, `FIN-VAL-1.0` §kas (`FIN-VAL-060`..`066`) |
| Contract version | `FIN-API-1.0`, `FIN-PERM-1.0` — dipatuhi penuh untuk struktur route kanonik, pembungkus `ApiResponse<T>`, nama permission, dan kode status HTTP |
| Dependency | `BE-FIN-014` (Layanan transaksi `FinanceCashManagementService` dan DTOs selesai 21 September 2026) |
| Klasifikasi | `API CONTROLLER LAYER` — pembangunan antarmuka REST API, proteksi `[Authorize]`, deklarasi `[AccessController]`, `[AccessAction]`, `[AccessPermission]`, dan Swagger documentation tags |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceBankDepositsController.cs` (baru), `Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceDailyCashController.cs` (baru) |
| Model | Gemini 3.8 Flash |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN — controller API selesai dibangun penuh dan QBE PASS; menunggu verifikasi build dan pengujian runtime pengguna.** Seluruh cakupan roadmap (2 controller) terpenuhi |

---

## 0. Otorisasi dan metode pembuatan

Sesuai roadmap `01-backend-roadmap.md` baris `BE-FIN-015` dan otorisasi implementasi:
1. Pembangunan dua controller API (`FinanceBankDepositsController` dan `FinanceDailyCashController`) membungkus service `FinanceCashManagementService` yang telah diselesaikan pada `BE-FIN-014`.
2. Kedua controller menerapkan prinsip DDD dan clean architecture tanpa menyentuh `ApplicationDbContext` secara langsung (QBE-CODE-002 compliant).
3. Validasi statis QBE dijalankan via `tooling/qbe/Invoke-QbeConformanceCheck.ps1` (`Final result: PASS`). Eksekusi `dotnet build` diserahkan kepada pengguna sesuai preferensi kerja yang telah disepakati.

---

## 1. Keputusan desain yang diambil (bukan gap — didokumentasikan untuk transparansi)

### 1.1 Struktur Route Kanonik Hyphenated Tanpa Alias

- Mengikuti kontrak `FIN-API-1.0` dan arsitektur backend §4.23:
  - `api/v1/corporate/finance-management/bank-deposits`
  - `api/v1/corporate/finance-management/daily-cash`
- Route warisan/alias `health-services/...` sengaja **tidak dibuat** untuk menjaga konsistensi namespace Corporate Finance.

### 1.2 Kesesuaian Ketat `role-access-rules.md`

Sesuai aturan konstitusi hak akses role:
- Argumen ke-1 `[AccessPermission]` identik dengan `ControllerName` pada `[AccessController]`:
  - `FinanceBankDepositsController` -> `ControllerName = "FinanceBankDeposit"` -> `[AccessPermission("FinanceBankDeposit", ...)]`.
  - `FinanceDailyCashController` -> `ControllerName = "FinanceDailyCash"` -> `[AccessPermission("FinanceDailyCash", ...)]`.
- Argumen ke-2 `[AccessPermission]` identik dengan argumen ke-1 `[AccessAction]` pada action method yang sama (`Read`, `Create`, `Post`, `Verify`, `Cancel`, `Close`).
- Nilai `AccessType` menggunakan konstanta `AccessTypes.Read`, `Create`, atau `Update` agar dapat muncul dan dicentang pada layar Pengaturan -> Akses Role.

### 1.3 Penanganan Galat & Pembungkus Kanonik

- Seluruh endpoint mengembalikan pembungkus seragam `ApiResponse<T>`.
- Pemetaan galat bisnis kanonik:
  - `KeyNotFoundException` -> HTTP 404
  - `CashConflictException` -> HTTP 409
  - `CashValidationException` -> HTTP 422
  - `CashBadRequestException` -> HTTP 400

---

## 2. Ringkasan pekerjaan

### 2.1 `FinanceBankDepositsController.cs` (7 Endpoints)

1. `GET /`: Daftar setoran bank terpaginasi dengan penyaringan rekening, status, tanggal, dan pencarian nomor setoran/slip.
2. `GET /{id:guid}`: Rincian satu setoran bank beserta nama rekening bank dan nomor shift kasir terkait.
3. `GET /available-balance`: Kalkulasi saldo kas kasir yang masih tersedia untuk disetor pada tanggal atau shift tertentu.
4. `POST /`: Membuat draf setoran baru (`Status = DRAFT`) dengan nomor `DEP-YYYYMMDD-XXXX`.
5. `POST /{id:guid}/post`: Memposting setoran bank (`Status = POSTED`), melakukan validasi ulang kas tersedia saat transaksi berjalan di dalam isolasi serializable (`FIN-VAL-060`, `FR-FIN-062`).
6. `POST /{id:guid}/verify`: Menandai setoran bank sudah sesuai dengan rekening koran (`Status = VERIFIED`).
7. `POST /{id:guid}/cancel`: Membatalkan setoran bank (`Status = CANCELLED`), menolak jika kas harian tanggal terkait sudah ditutup.

### 2.2 `FinanceDailyCashController.cs` (4 Endpoints)

1. `GET /current`: Membaca posisi kas berjalan hari ini.
2. `GET /`: Riwayat posisi kas per tanggal.
3. `GET /{cashDate}/breakdown`: Rincian penelusuran angka kas satu tanggal ke shift-shift kasir penyusun dan setoran bank yang telah diposting.
4. `POST /{cashDate}/close`: Menutup kas harian (`Status = CLOSED`), menolak jika masih ada setoran berstatus `DRAFT` pada tanggal tersebut (`FIN-VAL-064`), memvalidasi saldo awal dari hari sebelumnya (`FIN-VAL-066`), dan membekukan seluruh angka kas (`FR-FIN-065`).

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (baris `BE-FIN-015`)
- `docs/module-blueprints/finance-management/contracts/api-contract.md` (bagian Bank Deposit & Daily Cash)
- `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §7 (Kas)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-060`..`FIN-VAL-066`)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §4.23

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceBankDepositsController.cs` | **Baru.** 7 REST endpoint setoran bank |
| `Areas/Corporate/FinanceManagement/CashManagement/Controllers/FinanceDailyCashController.cs` | **Baru.** 4 REST endpoint kas harian |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | 11 endpoint baru di bawah rute kanonik `api/v1/corporate/finance-management/` selesai dibangun sesuai `FIN-API-1.0` |
| Database | `NOT APPLICABLE` — Tidak ada migrasi/perubahan skema database |
| Keamanan/Auth | Dilindungi `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]` dengan identitas aktor dari klaim JWT |

---

## 4. Dokumentasi endpoint

### 4.1 Corporate / Finance Management / Bank Deposit

Tag Swagger: `[Tags("Corporate / Finance Management / Bank Deposit")]`  
Base URL: `api/v1/corporate/finance-management/bank-deposits`

| Method | Path | Deskripsi | Auth / Permission | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/` | Daftar setoran bank terpaginasi | `[Authorize]`<br>`FinanceBankDeposit:Read` | `[FromQuery] BankDepositQuery` | `ApiResponse<PagedResult<BankDepositResponse>>` (200) |
| `GET` | `/{id:guid}` | Rincian satu setoran bank | `[Authorize]`<br>`FinanceBankDeposit:Read` | Route parameter `id` | `ApiResponse<BankDepositResponse>` (200), 404 |
| `GET` | `/available-balance` | Pengecekan saldo kas yang masih boleh disetor | `[Authorize]`<br>`FinanceBankDeposit:Read` | `[FromQuery] AvailableBalanceQuery` | `ApiResponse<AvailableCashBalanceResponse>` (200), 404 |
| `POST` | `/` | Membuat draf setoran baru | `[Authorize]`<br>`FinanceBankDeposit:Create` | `[FromBody] CreateBankDepositRequest` | `ApiResponse<BankDepositResponse>` (200), 400, 404, 422 |
| `POST` | `/{id:guid}/post` | Memposting setoran bank (re-evaluasi kas tersedia) | `[Authorize]`<br>`FinanceBankDeposit:Post` | `[FromBody] PostBankDepositRequest` | `ApiResponse<BankDepositResponse>` (200), 400, 404, 409, 422 |
| `POST` | `/{id:guid}/verify` | Verifikasi kesesuaian dengan rekening koran | `[Authorize]`<br>`FinanceBankDeposit:Verify` | `[FromBody] VerifyBankDepositRequest` | `ApiResponse<BankDepositResponse>` (200), 404, 409, 422 |
| `POST` | `/{id:guid}/cancel` | Membatalkan setoran bank yang belum ditutup harinya | `[Authorize]`<br>`FinanceBankDeposit:Cancel` | `[FromBody] CancelBankDepositRequest` | `ApiResponse<BankDepositResponse>` (200), 404, 409, 422 |

### 4.2 Corporate / Finance Management / Daily Cash

Tag Swagger: `[Tags("Corporate / Finance Management / Daily Cash")]`  
Base URL: `api/v1/corporate/finance-management/daily-cash`

| Method | Path | Deskripsi | Auth / Permission | Request | Response |
|---|---|---|---|---|---|
| `GET` | `/current` | Posisi kas berjalan hari ini | `[Authorize]`<br>`FinanceDailyCash:Read` | — | `ApiResponse<DailyCashResponse>` (200) |
| `GET` | `/` | Riwayat posisi kas per tanggal terpaginasi | `[Authorize]`<br>`FinanceDailyCash:Read` | `[FromQuery] DailyCashQuery` | `ApiResponse<PagedResult<DailyCashResponse>>` (200) |
| `GET` | `/{cashDate}/breakdown` | Rincian penyusun angka kas per shift & setoran | `[Authorize]`<br>`FinanceDailyCash:Read` | Route parameter `cashDate` (DateOnly) | `ApiResponse<DailyCashBreakdownResponse>` (200) |
| `POST` | `/{cashDate}/close` | Menutup kas harian dan membekukan angka | `[Authorize]`<br>`FinanceDailyCash:Close` | `[FromBody] CloseDailyCashRequest` | `ApiResponse<DailyCashResponse>` (200), 400, 409, 422 |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Conformance Check (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`) pada kedua controller | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Output CLI PowerShell |
| Review manual: Keselarasan string `[AccessPermission]` dengan `ControllerName` pada `[AccessController]` | `FinanceBankDeposit` dan `FinanceDailyCash` sama persis | `PASS` | `role-access-rules.md` §7 |
| Review manual: Seluruh endpoint dibungkus `ApiResponse<T>` | 11 dari 11 endpoint memakai `ApiResponse<T>` | `PASS` | Review kode controller |
| Review manual: Penolakan penutupan kas bila ada setoran belum diposting (`FIN-VAL-064`) | Diteruskan ke service `CloseDailyCashAsync`, ditangkap dan dipetakan ke 422 | `PASS` | `FinanceDailyCashController.cs` |
| `dotnet build` | **Tidak dijalankan otomatis** | `NOT RUN` | Menghormati preferensi user ("saya akan build sendiri") |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Penutupan hari menolak setoran belum terposting | **Terpenuhi** | `FinanceDailyCashController.Close` memanggil `CloseDailyCashAsync` yang menolak bila ada setoran `DRAFT` dengan galat `422` (`FIN-VAL-064`) |
| Pembungkus `ApiResponse<T>` | **Terpenuhi** | Seluruh respons sukses dan gagal dibungkus `ApiResponse<T>` |
| Route kanonik hyphenated | **Terpenuhi** | `api/v1/corporate/finance-management/bank-deposits` dan `daily-cash` |
| Perlindungan otorisasi | **Terpenuhi** | Seluruh aksi dilindungi `[Authorize]`, `[AccessController]`, `[AccessAction]`, `[AccessPermission]` |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | Dengan selesainya `BE-FIN-015`, seluruh gelombang `MVP-4` (Setoran bank dan kas harian — `BE-FIN-013`, `BE-FIN-014`, `BE-FIN-015`) telah selesai secara struktural di backend |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | **Rendah** — QBE Conformance PASS |
| Perubahan sampingan | `NONE` |
| Status Git | 2 berkas baru di `CashManagement/Controllers/` (`FinanceBankDepositsController.cs`, `FinanceDailyCashController.cs`) |
| Langkah berikutnya | Menunggu verifikasi build pengguna; koordinasi kesiapan tim Billing untuk `MVP-2` (`BE-FIN-016` / `BilCollectionHandoff`) |
