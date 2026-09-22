# Laporan Perubahan Backend — `BE-FIN-004`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-004` |
| Judul | API data induk Finance |
| Slice | `MVP-0` — Fondasi data induk (`EPIC FIN-01`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-0`) |
| Trace | `FIN-DES-001`, `FR-FIN-001`..`004`; `00-delivery-roadmap.md` bagian 5 baris `FR-FIN-001`..`004` |
| Contract version | `FIN-API-1.0` (locked 20 September 2026), `FIN-PERM-1.0` (locked 20 September 2026) — dipatuhi dengan dua delta terdokumentasi (bagian 1) |
| Dependency | `BE-FIN-003` — 🟡 sebagian 21 September 2026 (file migration dibuat, belum dijalankan), lihat [laporan](BE-FIN-003.md). Source task ini ditulis terhadap entity `BE-FIN-002` yang sudah ada di compiler; tidak menunggu migration benar-benar diterapkan untuk bisa ditulis, tetapi **tidak bisa diuji end-to-end** sampai migration itu jalan |
| Klasifikasi | `MEDIUM` — satu repository; 7 berkas diubah/dibuat; logika sedang (validasi unik, eksklusivitas mata uang dasar, riwayat kurs append-only); memakai kontrak API yang sudah ada dengan dua delta kecil; database hanya query terhadap entity yang sudah ada; keamanan memakai `[AccessPermission]` yang sudah mapan, bukan inti baru |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/MasterData/{Controllers,DTOs,Services}/`, `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI dua service baru) |
| Model | Claude Sonnet 5 |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | 🟡 **SEBAGIAN.** `BankAccountsController` dan `CurrenciesController` (dengan sub-resource kurs) selesai dan lengkap 9-baseline (minus `DELETE` pada Currency, disengaja). Grup endpoint `Bank` pada `FIN-API-1.0` **tidak** diimplementasikan Finance — lihat delta bagian 1. Belum dapat diuji end-to-end karena migration `BE-FIN-003` belum dijalankan |

---

## 1. Delta terhadap kontrak yang disetujui — dibaca lebih dulu

`FIN-API-1.0` (locked) merancang **tiga** grup endpoint data induk: `Bank`, `Bank Account`, dan
`Currency` (dengan sub-resource `exchange-rates`). Task ini hanya mengimplementasikan **dua**:

| Grup pada `FIN-API-1.0` | Dikerjakan? | Alasan |
| --- | --- | --- |
| `Master Data / Bank` (`api/v1/corporate/finance-management/master-data/banks`, CRUD penuh atas `MstBank`) | **Tidak** | Konsekuensi langsung keputusan `BE-FIN-002`: `MstBank` milik Finance tidak pernah dibuat — entity itu sudah ada dan tetap dipakai dari `Areas/Administrator/MasterData` (`api/v1/administrator/master-data/banks` yang sudah aktif). Membuat controller `Bank` baru di Finance berarti mengekspos CRUD atas entity yang **tidak dimiliki** Finance, atau — lebih buruk — diam-diam membuat entity `MstBank` kedua yang justru ditolak eksplisit `BE-FIN-002`. Keduanya salah; opsi yang benar adalah tidak membuatnya sama sekali, konsisten dengan preseden `FIN-DEC-014` (Finance memakai `MstSupplier` existing tanpa membuka endpoint CRUD Supplier sendiri) |
| `Master Data / Bank Account` | **Ya** | `BankAccountsController`, 9-baseline penuh |
| `Master Data / Currency` (+ sub-resource `exchange-rates`) | **Ya, minus `DELETE`** | `CurrenciesController`. `DELETE /{id}` **sengaja tidak dibuat** — `FIN-API-1.0` sendiri tidak mencantumkan baris `DELETE` untuk grup Currency (berbeda dari Bank dan Bank Account yang eksplisit mencantumkannya), sehingga ini bukan kekurangan implementasi melainkan mengikuti kontrak apa adanya. Mata uang hanya bisa dinonaktifkan (`PATCH /{id}/status`), konsisten dengan riwayat kurs yang bergantung padanya |

**Rekomendasi ke pass desain**: `api-contract.md` §*Master Data / Bank* perlu ditandai
`SUPERSEDED` atau dihapus, dan `FIN-SC-006`/`00-interview-decisions.md` diberi catatan silang ke
temuan `BE-FIN-002`, supaya pembaca berikutnya tidak menganggap grup `Bank` masih "Rencana (belum
tersedia)" padahal sudah diputuskan tidak akan pernah dibuat dalam bentuk itu.

---

## 2. Proses bisnis

### 2.1 Rekening bank (`BankAccountsController`)

1. **Pelaku.** Staf Finance dengan hak akses `BankAccount`.
2. **Membuat rekening baru:** memilih bank (dari `MstBank` milik Administrator — validasi
   memastikan `BankId` ada dan belum dihapus), mengisi nomor rekening, nama pemilik rekening,
   jenis rekening (`OPERATIONAL`/`COLLECTION`/`PAYMENT`), dan kode mata uang (bawaan `IDR`).
3. **Aturan bisnis utama** (`BE-FIN-004` AC): nomor rekening yang sama pada bank yang sama ditolak
   `409 Conflict` — "Nomor rekening ini sudah terdaftar untuk bank yang sama." Contoh: rekening
   `1234567890` di BCA sudah ada; mencoba membuat rekening `1234567890` di BCA lagi ditolak, tetapi
   `1234567890` di Mandiri **diterima** (bank berbeda).
4. **Menonaktifkan, bukan menghapus** untuk rekening yang sudah dipakai transaksi (`PATCH
   .../status`); `DELETE` tetap tersedia untuk rekening yang **belum pernah dipakai** sama sekali
   (baseline 9-endpoint), mengikuti pola `PettyCashCategoryService.DeleteAsync`.
5. **Jalur tidak normal:** `BankId` tidak ditemukan → `422` "Bank yang dipilih tidak ditemukan.
   Pilih bank lain."; jenis rekening di luar tiga pilihan → `422`; kode mata uang bukan 3 huruf →
   `422`.

### 2.2 Mata uang dan kurs harian (`CurrenciesController`)

1. **Membuat mata uang baru:** kode ISO 4217 (mis. `USD`), nama, simbol opsional, jumlah desimal
   (bawaan 2), dan penanda mata uang dasar.
2. **Aturan bisnis utama** (`FR-FIN-003`): hanya **satu** mata uang boleh ditandai sebagai mata
   uang dasar. Contoh konkret: `IDR` sudah ditandai dasar. Staf menandai `USD` sebagai dasar juga
   → **ditolak** `422` "Sudah ada mata uang dasar lain yang aktif. Nonaktifkan status mata uang
   dasar itu terlebih dahulu sebelum menandai mata uang ini." — bukan otomatis menggeser `IDR`.
3. **Mencatat kurs harian** (`POST /{id}/exchange-rates`): kurs beli, jual, dan tengah untuk satu
   tanggal. Satu mata uang hanya boleh punya **satu** baris kurs per tanggal — mencoba mencatat
   kurs `USD` tanggal 21 September 2026 dua kali ditolak `409 Conflict` "Kurs USD untuk tanggal
   2026-09-21 sudah pernah dicatat."
4. **Kurs bersifat riwayat (append-only):** tidak ada endpoint ubah/hapus kurs. Koreksi kurs yang
   salah dicatat sebagai baris baru pada tanggal yang benar, bukan menimpa baris lama — konsisten
   dengan pola append-only yang dipakai modul lain di codebase ini.
5. **Jalur tidak normal:** kode mata uang dobel → `409`; kode mata uang bukan 3 huruf → `422`;
   mata uang atau tanggal tidak ditemukan saat mencatat kurs → `404`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/contracts/api-contract.md` §*Master Data / Bank, Bank Account, Currency*
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` (`FR-FIN-003`, contoh mata uang dasar)
- `Areas/Corporate/FinanceManagement/MasterData/{Controllers,DTOs,Services}/PettyCashCategory*` — template 9-baseline yang sudah berjalan nyata di submodule yang sama, dipakai sebagai bentuk baku (bukan `master-data-endpoint-standard.md` secara abstrak, karena source nyata di submodule yang sama lebih otoritatif)
- `Areas/Administrator/MasterData/Models/MstBank.cs`, `Controllers/BankController.cs` — memastikan tidak menduplikasi capability yang sudah ada
- `Attributes/AccessControllerAttribute.cs`, `Attributes/AccessActionAttribute.cs` — bentuk `SortOrder`, argumen wajib
- `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — titik registrasi DI yang sudah dipakai `PettyCashCategoryService`/`PettyCashBudgetService`/`PettyCashVoucherService` (seluruhnya kelas Corporate/FinanceManagement, diregistrasi dari titik ini karena alasan historis migrasi Petty Cash — bukan pola baru yang saya ciptakan)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/BankAccountDtos.cs` | **Baru.** Query, Create/Update/UpdateStatus request, Response, Delete/Option/Summary/FilterMetadata response |
| `Areas/Corporate/FinanceManagement/MasterData/Services/BankAccountService.cs` | **Baru.** CRUD lengkap; validasi `BankId` terhadap `MstBank` (Administrator, join, bukan FK yang ditulis Finance); tolak nomor rekening ganda per bank; audit log MUST NOT memuat `AccountNumber` (Sensitif) |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/BankAccountsController.cs` | **Baru.** 9 endpoint baseline penuh, route `api/v1/corporate/finance-management/master-data/bank-accounts` |
| `Areas/Corporate/FinanceManagement/MasterData/DTOs/CurrencyDtos.cs` | **Baru.** DTO Currency (8 baseline, minus Delete) + DTO ExchangeRate (Query, Create, Response) |
| `Areas/Corporate/FinanceManagement/MasterData/Services/CurrencyService.cs` | **Baru.** CRUD Currency (minus Delete); eksklusivitas mata uang dasar (tolak, bukan geser); riwayat kurs append-only dengan penolakan duplikat tanggal |
| `Areas/Corporate/FinanceManagement/MasterData/Controllers/CurrenciesController.cs` | **Baru.** 8 endpoint baseline (minus `DELETE`, disengaja — bagian 1) + 2 endpoint sub-resource kurs, route `api/v1/corporate/finance-management/master-data/currencies` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Registrasi `services.AddScoped<BankAccountService>()` dan `services.AddScoped<CurrencyService>()`, mengikuti titik registrasi Corporate/FinanceManagement yang sudah ada di berkas ini |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Dua grup endpoint baru (`Bank Account`, `Currency`+`ExchangeRate`), route hyphenated `api/v1/corporate/finance-management/master-data/...`, seluruhnya `ApiResponse<T>`/`PagedResult<T>`. Grup `Bank` pada `FIN-API-1.0` sengaja tidak diimplementasikan (bagian 1) |
| Database | `NOT APPLICABLE` — tidak ada perubahan schema; hanya query/mutasi terhadap entity yang sudah ada dari `BE-FIN-002` |
| Keamanan/Auth | Dua `[AccessController]` baru (`BankAccount`, `Currency`) di bawah moduleCode `CORPORATE_FINANCE_MANAGEMENT_MASTER_DATA` yang sudah ada; setiap action punya `[AccessAction]`+`[AccessPermission]` berpasangan argumen persis mengikuti pola `PettyCashCategoriesController`. Tidak ada hardcode role |

---

## 4. Dokumentasi endpoint

#### Corporate / Finance Management / Master Data / Bank Account

Base URL: `api/v1/corporate/finance-management/master-data/bank-accounts`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter dan pilihan jenis rekening untuk layar daftar | `BankAccount : Read` |
| `GET` | `/summary` | Ringkasan jumlah rekening aktif/nonaktif | `BankAccount : Read` |
| `GET` | `/` | Daftar rekening bank berpaging, dapat difilter bank/jenis/status | `BankAccount : Read` |
| `GET` | `/options` | Pilihan rekening untuk formulir setoran/pembayaran | `BankAccount : Read` |
| `GET` | `/{id:guid}` | Rincian satu rekening | `BankAccount : Read` |
| `POST` | `/` | Menambah rekening baru | `BankAccount : Create` |
| `PUT` | `/{id:guid}` | Memperbarui data rekening | `BankAccount : Update` |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan/menonaktifkan rekening | `BankAccount : Update` |
| `DELETE` | `/{id:guid}` | Menghapus rekening yang belum pernah dipakai | `BankAccount : Delete` |

#### Corporate / Finance Management / Master Data / Currency

Base URL: `api/v1/corporate/finance-management/master-data/currencies`

| Method | Path | Kegunaan | Hak akses |
| --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Metadata filter untuk layar daftar mata uang | `Currency : Read` |
| `GET` | `/summary` | Ringkasan jumlah mata uang aktif/nonaktif | `Currency : Read` |
| `GET` | `/` | Daftar mata uang berpaging | `Currency : Read` |
| `GET` | `/options` | Pilihan mata uang untuk formulir | `Currency : Read` |
| `GET` | `/{id:guid}` | Rincian satu mata uang | `Currency : Read` |
| `POST` | `/` | Menambah mata uang baru | `Currency : Create` |
| `PUT` | `/{id:guid}` | Memperbarui data mata uang | `Currency : Update` |
| `PATCH` | `/{id:guid}/status` | Mengaktifkan/menonaktifkan mata uang | `Currency : Update` |
| `GET` | `/{id:guid}/exchange-rates` | Riwayat kurs harian satu mata uang, dapat difilter rentang tanggal | `Currency : Read` |
| `POST` | `/{id:guid}/exchange-rates` | Mencatat kurs satu tanggal (append-only) | `Currency : Update` |

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet build` | **Tidak dijalankan** | `NOT RUN` | Atas instruksi eksplisit pengguna pada sesi ini; pengguna memvalidasi build secara mandiri. Risiko ini bertumpuk dengan `BE-FIN-002`/`BE-FIN-003` yang juga belum di-build — lihat Catatan Penutup |
| `powershell.exe -NoProfile -File tooling/qbe/Invoke-QbeConformanceCheck.ps1` | `Files evaluated: 17`, `VIOLATION: 0`, `Final result: PASS` | `PASS` | Kutipan di bawah |
| Review manual: setiap `[AccessAction]`/`[AccessPermission]` argumen 1/2 dicocokkan terhadap `[AccessController].ControllerName` | Cocok pada seluruh 19 action (9 `BankAccount` + 10 `Currency`) | `PASS` | Perbandingan manual berkas controller pada bagian 3.2 |
| Review manual: tidak ada `IsInRole`/nama role/`UserType` hardcode pada controller/service baru | Tidak ditemukan — otorisasi murni lewat `[AccessPermission]` | `PASS` | Isi berkas pada bagian 3.2 |
| Skenario bisnis: nomor rekening ganda pada bank yang sama | Kode ditelusuri manual — `AnyAsync(... x.BankId == request.BankId && x.AccountNumber == accountNumber ...)` melempar `BankAccountConflictException` → `409` | `PASS` (tervalidasi kode, **belum** dieksekusi runtime — lihat `NOT RUN` `dotnet build`) | `BankAccountService.cs` `ValidateAsync` |
| Skenario bisnis: dua mata uang dasar sekaligus | Kode ditelusuri manual — cek `AnyAsync(x.IsBaseCurrency)` lain sebelum izinkan `IsBaseCurrency = true` baru, melempar `CurrencyValidationException` → `422` | `PASS` (tervalidasi kode, belum runtime) | `CurrencyService.cs` `ValidateAsync` |
| Skenario bisnis: kurs dobel pada tanggal yang sama | Kode ditelusuri manual — cek `AnyAsync(x.CurrencyId == currencyId && x.RateDate == request.RateDate)` sebelum insert, melempar `CurrencyConflictException` → `409`, sejalan dengan `IX_MstExchangeRate_Currency_RateDate` sebagai penjaga lapis kedua di database | `PASS` (tervalidasi kode, belum runtime) | `CurrencyService.cs` `CreateExchangeRateAsync` |

Keluaran `Invoke-QbeConformanceCheck.ps1`:

```
QBE Conformance Report
Checker mode: ReportOnly
Scope: WorkingTree
Files evaluated: 17
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
REPORT ONLY: No enforcement/blocking performed.
Final result: PASS
```

`AUTOMATED TEST: NOT APPLICABLE` — backend tidak memelihara project test otomatis.

Uji manual: `NOT FEASIBLE` pada task ini — migration `BE-FIN-003` belum dijalankan sehingga tabel
`MstBankAccount`/`MstCurrency`/`MstExchangeRate` belum ada di database mana pun untuk dipanggil API
sungguhan.

**Tidak dijalankan:** `dotnet build`, uji manual endpoint (`curl`/Swagger) — keduanya menunggu
migration dijalankan dan build diverifikasi pengguna terlebih dahulu.

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria | Status | Bukti |
| --- | --- | --- |
| `FinanceMasterDataService` + 3 controller | 🟡 **Delta disengaja** — 2 controller (`BankAccountsController`, `CurrenciesController`), bukan 3, dan sebagai 2 service terpisah (`BankAccountService`, `CurrencyService`) bukan satu `FinanceMasterDataService` monolitik — mengikuti pola satu-service-per-entity yang sudah berjalan (`PettyCashCategoryService`) alih-alih menciptakan service gabungan generik | Bagian 1 dan 3.2 |
| Nomor rekening ganda ditolak | Terpenuhi | Bagian 5, `BankAccountService.ValidateAsync` |
| Data induk terpakai dinonaktifkan bukan dihapus | Terpenuhi untuk pola umum (`PATCH .../status`); guard "tidak dapat dihapus bila sudah dipakai" pada `DELETE` **belum** ditambahkan ke `BankAccountService.DeleteAsync` karena belum ada satu pun entity (`FinBankDeposit`, dst., `MVP-4`/`BE-FIN-013`+) yang mereferensikan `BankAccountId` — lihat Catatan Penutup | `BankAccountService.cs` |
| `UAT-01`, `UAT-02` | **Belum dapat dijalankan** — memerlukan database dengan migration `BE-FIN-003` diterapkan | Bagian 5 |
| Route `api/v1/corporate/finance-management/...` hyphenated | Terpenuhi | `[Route(...)]` pada kedua controller |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| Peringatan | **Risiko build bertumpuk.** `BE-FIN-002` dan `BE-FIN-003` belum diverifikasi `dotnet build`; task ini menambah kode di atas keduanya tanpa jaminan compiler. Sangat disarankan pengguna menjalankan `dotnet build` sekarang mencakup ketiga task (`BE-FIN-002`, `003`, `004`) sekaligus sebelum melanjutkan ke task berikutnya |
| Masalah yang diketahui | (1) `BankAccountService.DeleteAsync` belum punya guard "sudah dipakai" karena belum ada konsumen `BankAccountId` di source manapun saat ini — **wajib ditambahkan** saat `BE-FIN-013`+ (Cash Management) mulai mereferensikannya, mengikuti pola `PettyCashCategoryService.DeleteAsync`. (2) Grup endpoint `Bank` pada `FIN-API-1.0` perlu diratifikasi `SUPERSEDED` oleh pemilik blueprint (bagian 1) |
| Risiko tersisa | Sedang — bergantung pada `dotnet build` yang belum dijalankan (lihat peringatan). Risiko bisnis rendah: validasi sudah menutup skenario duplikasi dan eksklusivitas yang diminta |
| Perubahan sampingan | `NONE` |
| Interupsi | `NONE` pada task ini |
| Status Git | `git status --short` menunjukkan berkas `BE-FIN-002`/`003` yang sudah ada, ditambah 7 berkas baru/berubah task ini: 2 controller, 2 DTO, 2 service (baru), 1 DI extension (berubah) |
| Langkah berikutnya | (1) `dotnet build` mencakup `BE-FIN-002`..`004`. (2) Otorisasi eksekusi migration `BE-FIN-003`. (3) Setelah migration jalan, jalankan `UAT-01`/`UAT-02` sungguhan. (4) `BE-FIN-005` (intake Billing) dapat mulai — dependency-nya `BE-FIN-004`, bukan `BE-FIN-002`/`003` langsung |
