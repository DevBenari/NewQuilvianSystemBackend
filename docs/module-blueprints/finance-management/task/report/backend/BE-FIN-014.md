# Laporan Perubahan Backend — `BE-FIN-014`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-014` |
| Judul | Perhitungan kas tersedia dan penutupan harian — `FinanceCashManagementService` |
| Slice | `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 2.1, bagian 3 (tabel task baris `BE-FIN-014`), dan bagian 4 (`MVP-4`) |
| Trace | `FIN-DES-018`, `FIN-DES-021`, `FIN-DES-022`; `FR-FIN-060`..`065`; kontrak `FIN-STATE-1.0` §7/§8; `FIN-VAL-1.0` §kas (`FIN-VAL-060`..`066`); `FIN-DEC-017`, `FIN-DEC-020` |
| Contract version | `FIN-STATE-1.0`, `FIN-VAL-1.0` §kas — dipatuhi penuh untuk penghitungan saldo kas tersedia saat posting, penolakan draf saat penutupan hari, dan pembekuan angka tertutup |
| Dependency | `BE-FIN-013` (Entity, configuration, dan migration `AddFinanceCashManagement` selesai dibuat 21 September 2026) |
| Klasifikasi | `CORE BUSINESS LOGIC` — implementasi transaksi kas, kalkulasi saldo kasir lintas-bounded-context, transaksi `Serializable`, advisory locking PostgreSQL, penutupan harian |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/CashManagement/DTOs/FinanceCashManagementDtos.cs` (baru), `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs` (baru), `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI) |
| Model | Gemini 3.8 Flash |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Seluruh cakupan roadmap (`FinanceCashManagementService`) terpenuhi. `dotnet build` PASS, migration diterapkan, endpoint diuji langsung — dikonfirmasi pengguna 23 September 2026, lihat Pembaruan bagian 7 |

---

## 0. Otorisasi dan metode pembuatan

Sesuai roadmap `01-backend-roadmap.md` bagian 2.1 dan otorisasi implementasi task:
1. `MVP-4` diizinkan mendahului `MVP-2` karena `BilCollectionHandoff` belum tersedia dari tim Billing.
2. `FinanceCashManagementService` menghitung kas tersedia langsung dari kas kasir (`FIN-CAP-006`, `BilCashierShift` read-only) dan **TIDAK** membaca `FinReceipt`.
3. Mengikuti preseden pengerjaan `BE-FIN-008`..`BE-FIN-013`, file service dan DTO ditulis secara clean, modular, dan `dotnet build` diserahkan kepada pengguna sesuai preferensi kerja yang telah disepakati.

---

## 1. Keputusan desain yang diambil (bukan gap — didokumentasikan untuk transparansi)

### 1.1 Formula kas kasir dibaca langsung dari `BilCashierShift` (`FIN-CAP-006`, `FIN-DEC-017`)

Sesuai catatan urutan `MVP-4` di roadmap §2.1:
- `FinanceCashManagementService` membaca `BilCashierShift` milik Billing secara read-only tanpa membuat relasi foreign key keras.
- Logika pengakuan kas kasir (`GetShiftCash`):
  - Bila shift memiliki kas fisik (`PhysicalCash > 0`), nilai tersebut digunakan (mencakup selisih/variance yang telah disahkan saat shift berstatus `REVIEWED`).
  - Bila shift masih berstatus `OPEN` atau kas fisik belum diinput, nilai `SystemCash` yang digunakan.
- Saldo kas tersedia dihitung: total kas kasir tanggal/shift terkait dikurangi seluruh setoran bank `FinBankDeposit` yang sudah berstatus `POSTED` atau `VERIFIED`.

### 1.2 Kas kecil terpisah penuh dari kas kasir (`FIN-DEC-020`, `FR-FIN-063`, `UAT-16`)

Sesuai aturan bisnis #15 (`FIN-DEC-020`):
- `FinanceCashManagementService` sama sekali tidak membaca maupun memodifikasi tabel kas kecil (`BilPettyCashBudget`/`FinPettyCashBudget`).
- Mutasi atau pencairan kas kecil tidak akan mengubah saldo kas tersedia maupun penutupan kas harian.

### 1.3 Transaksi `IsolationLevel.Serializable` dan Advisory Lock PostgreSQL (`FIN-DES-021`, `UAT-14`)

Sesuai DoD #7 dan requirement `FR-FIN-062` / `FIN-DES-021`:
- Perhitungan saldo kas yang tersedia dilakukan di dalam transaksi `IsolationLevel.Serializable`.
- Ditegakkan PostgreSQL advisory lock (`pg_advisory_xact_lock(hashtext('FIN_CASH_{Date}'))`) saat pemanggilan `PostBankDepositAsync` dan `CloseDailyCashAsync`.
- Menjamin dua petugas yang memposting setoran bersamaan tidak dapat menyebabkan saldo kas menjadi negatif atau terjadi double spending (`UAT-14`).

### 1.4 Pembekuan Kas Harian Tertutup (`FIN-DES-022`, `FR-FIN-065`)

- Ketika kas harian satu tanggal ditutup (`Status == CLOSED`), angkanya dibekukan.
- Setiap upaya membuat atau memposting setoran pada tanggal yang sudah ditutup akan ditolak dengan galat `422` dan pesan persis: `"Kas tanggal ini sudah ditutup dan tidak dapat diubah."` (`FIN-VAL-063`).
- Setiap upaya membatalkan setoran yang sudah `POSTED` pada tanggal yang sudah ditutup akan ditolak `422` dengan pesan: `"Kas harian tanggal ini sudah ditutup. Setoran yang sudah diposting tidak dapat dibatalkan."`.

---

## 2. Ringkasan pekerjaan

### 2.1 DTOs (`Areas/Corporate/FinanceManagement/CashManagement/DTOs/FinanceCashManagementDtos.cs`)

Menyediakan kontrak tipe data untuk Bank Deposit dan Daily Cash:
- `BankDepositQuery`: parameter paginasi, pencarian, dan penyaringan rekening bank, tanggal, status.
- `BankDepositResponse`: representasi lengkap setoran bank beserta rincian rekening, slip setor, shift kasir, dan audit trail.
- `CreateBankDepositRequest`: pembuatan draf setoran baru.
- `PostBankDepositRequest`: posting setoran dengan `ExpectedRowVersion`.
- `VerifyBankDepositRequest`: verifikasi rekening koran dengan `ExpectedRowVersion`.
- `CancelBankDepositRequest`: pembatalan setoran dengan `ExpectedRowVersion` dan alasan.
- `AvailableBalanceQuery` & `AvailableCashBalanceResponse`: kueri dan respons saldo kas tersedia.
- `DailyCashQuery` & `DailyCashResponse`: kueri riwayat kas harian dan representasi snapshot kas harian.
- `DailyCashBreakdownResponse` & `DailyCashShiftBreakdownItem`: rincian penelusuran kas harian ke shift kasir penyusunnya dan setoran bank terkait.
- `CloseDailyCashRequest`: masukan penutupan hari.

### 2.2 Service (`Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs`)

Mengimplementasikan seluruh alur operasional Cash Management:
- `GetBankDepositsPagedAsync`: pembacaan daftar setoran dengan paging dan eager loading rekening bank.
- `GetBankDepositByIdAsync`: rincian satu setoran bank beserta informasi shift kasir.
- `GetAvailableCashBalanceAsync`: kalkulasi saldo kas yang masih dapat disetor.
- `CreateBankDepositAsync`: inisialisasi draf setoran (`DRAFT`) dengan nomor `DEP-YYYYMMDD-XXXX`.
- `PostBankDepositAsync`: eksekusi transaksi `Serializable` dengan advisory lock, validasi ulang saldo kas saat itu (`FR-FIN-062`), penolakan jika melebihi kas tersedia (`FIN-VAL-060`), dan pembaruan status ke `POSTED`.
- `VerifyBankDepositAsync`: penandaan kesesuaian dengan rekening koran (`POSTED` -> `VERIFIED`).
- `CancelBankDepositAsync`: pembatalan setoran (`CANCELLED`) dengan pengecekan integritas penutupan kas.
- `GetCurrentDailyCashAsync`: pembacaan posisi kas berjalan live hari ini.
- `GetDailyCashPagedAsync`: riwayat penutupan kas per tanggal.
- `GetDailyCashBreakdownAsync`: penelusuran angka kas ke shift kasir dan setoran bank.
- `CloseDailyCashAsync`: penutupan kas harian (`OPEN` -> `CLOSED`), penolakan bila ada setoran `DRAFT` tertunda (`FIN-VAL-064`), verifikasi saldo awal dari hari sebelumnya (`FIN-VAL-066`), kalkulasi formula penutupan kas (`CK_FinDailyCashSnapshot_Formula`), dan pembekuan angka (`FR-FIN-065`).
- Kelas galat bisnis kanonik: `CashBadRequestException` (400), `CashValidationException` (422), `CashConflictException` (409).

### 2.3 Registrasi DI (`Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs`)

- Mendaftarkan `services.AddScoped<FinanceCashManagementService>();` pada kumpulan extension DI modul Billing & Finance.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (§2.1, §3 baris `BE-FIN-014`, §4 `MVP-4`, §7 DoD)
- `docs/module-blueprints/finance-management/02-backend-architecture.md` (§2.3, §4.22, §5, §7, §9)
- `docs/module-blueprints/finance-management/04-prd-to-mvp.md` (`EPIC FIN-10`, `FR-FIN-060`..`065`, `UAT-13`..`UAT-16`)
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` (§7, §8)
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` (`FIN-VAL-060`..`FIN-VAL-066`)
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris `CashManagement`, `Fin`, `ACTIVE`)
- `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs` (`FIN-CAP-006`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/CashManagement/DTOs/FinanceCashManagementDtos.cs` | **Baru.** 13 DTO model data pertukaran CashManagement |
| `Areas/Corporate/FinanceManagement/CashManagement/Services/FinanceCashManagementService.cs` | **Baru.** Service transaksi kas harian dan setoran bank |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Penambahan `using` dan registrasi scoped `FinanceCashManagementService` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | Mempersiapkan service layer lengkap untuk API Controller `FinanceBankDepositsController` dan `FinanceDailyCashController` yang akan dibangun pada `BE-FIN-015` |
| Database | Tidak ada migration baru. Memanfaatkan tabel `FinBankDeposit`, `FinDailyCashSnapshot`, dan `BilCashierShift` yang sudah ada di skema |
| Keamanan/Auth | Diintegrasikan dengan context user aktor via `actorUserId` pada seluruh metode mutasi kas, dengan pencatatan jejak audit komprehensif |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` untuk task service slice `BE-FIN-014`. Seluruh API Controller (`FinanceBankDepositsController` dan `FinanceDailyCashController`) akan dibangun dan diekspos pada task `BE-FIN-015`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| QBE Conformance Check (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`) pada 3 berkas | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Output task background `task-167` |
| Review manual: Formula kas kasir membaca `BilCashierShift` (`FIN-CAP-006`) tanpa `FinReceipt` | Sesuai mandat roadmap §2.1 | `PASS` | `FinanceCashManagementService.GetShiftCash` |
| Review manual: Penegakan `IsolationLevel.Serializable` dan advisory lock pada mutasi kas | Ditegakkan pada `PostBankDepositAsync` dan `CloseDailyCashAsync` | `PASS` | `BeginTransactionAsync`, `AcquireLockAsync` |
| Review manual: Pesan validasi persis kontrak `FIN-VAL-060` ("Nominal setoran melebihi kas yang tersedia...") | Sesuai 100% | `PASS` | `PostBankDepositAsync` |
| Review manual: Penolakan penutupan kas bila ada setoran `DRAFT` (`FIN-VAL-064`) | Sesuai 100% | `PASS` | `CloseDailyCashAsync` |
| Review manual: Pemisahan kas kecil dari kolam kas kasir (`FIN-DEC-020`, `FR-FIN-063`) | Sesuai 100%, nol referensi ke Petty Cash | `PASS` | Kode `FinanceCashManagementService` |
| `dotnet build` | **Tidak dijalankan otomatis** | `NOT RUN` | Menghormati preferensi user ("saya akan build sendiri") |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Setoran melebihi kas ditolak | **Terpenuhi** | `PostBankDepositAsync` memvalidasi saldo tersedia saat posting; menolak `deposit.Amount > availableBalance` (`FIN-VAL-060`, `UAT-13`) |
| Setoran sebagian diterima | **Terpenuhi** | Setoran `Amount <= availableBalance` diterima, saldo tersisa diperbarui (`FIN-VAL-061`) |
| Saldo dihitung saat posting | **Terpenuhi** | `PostBankDepositAsync` menghitung ulang saldo kas di dalam transaksi `Serializable` dengan advisory lock (`FR-FIN-062`, `FIN-DES-021`, `UAT-14`) |
| Angka tertutup dibekukan | **Terpenuhi** | `CloseDailyCashAsync` mengunci status menjadi `CLOSED`. Mutasi/setoran pada tanggal tertutup ditolak (`FIN-VAL-063`, `FR-FIN-065`) |
| Penutupan hari menolak setoran belum terposting | **Terpenuhi** | `CloseDailyCashAsync` memeriksa jumlah setoran `DRAFT` dan menolak jika `draftDepositsCount > 0` (`FIN-VAL-064`, `UAT-15`) |
| Kas kecil tidak memengaruhi kas kasir | **Terpenuhi** | Tidak ada pembacaan kas kecil pada formula kas harian (`FR-FIN-063`, `UAT-16`) |
| `Serializable` | **Terpenuhi** | Seluruh transaksi pengubah saldo kas (`PostBankDepositAsync`, `CloseDailyCashAsync`) dibuka dengan `IsolationLevel.Serializable` (`DoD #7`) |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi `dotnet build` PASS, migration diterapkan, dan endpoint diuji langsung dengan hasil sesuai ekspektasi. Status task dinaikkan menjadi ✅ SELESAI |
| Peringatan | Saat membangun `BE-FIN-015` (API Controller), kedua controller (`FinanceBankDepositsController` dan `FinanceDailyCashController`) akan mengonsumsi service ini secara langsung |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | **Rendah** — QBE Conformance PASS |
| Perubahan sampingan | `NONE` |
| Status Git | 2 berkas baru di `CashManagement/` (`FinanceCashManagementDtos.cs`, `FinanceCashManagementService.cs`), 1 berkas registrasi DI disunting (`BillingManagementServiceCollectionExtensions.cs`) |
| Langkah berikutnya | `BE-FIN-015` (API setoran dan kas harian — `FinanceBankDepositsController`, `FinanceDailyCashController`) |
