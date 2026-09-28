# Laporan Perubahan Backend — `BE-FIN-013`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-013` |
| Judul | Entity kas dan setoran — `FinBankDeposit`, `FinDailyCashSnapshot` + migration `AddFinanceCashManagement` |
| Slice | `MVP-4` — Setoran bank dan kas harian (`EPIC FIN-10`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 4 (`MVP-4`) |
| Trace | `FIN-DES-018`..`020`, `FR-FIN-060`..`065`; kontrak `FIN-VAL-1.0` §kas (`FIN-VAL-060`..`066`); `FIN-STATE-1.0` §7/§8; `FIN-DEC-020` |
| Contract version | `FIN-VAL-1.0` §kas — dipatuhi penuh untuk bentuk kolom, check constraint status, dan check constraint formula penutupan kas |
| Dependency | `BE-FIN-009` — 🟡 sebagian 21 September 2026. Task ini mandiri pada submodule `CashManagement` dan hanya berelasi FK ke `MstBankAccount` (`BE-FIN-002`) |
| Klasifikasi | `HEAVY` — dampak database (2 tabel baru, FK, 3 check constraint, 6 index); berkas snapshot tersentuh |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/CashManagement/Models/*.cs` (baru), `Repositories/Configurations/Corporate/FinanceManagement/CashManagement/*.cs` (baru), `Repositories/ApplicationDbContext.cs` (DbSet+using), `Migrations/20260921000004_AddFinanceCashManagement.cs`+`.Designer.cs` (baru), `Migrations/ApplicationDbContextModelSnapshot.cs` (penambahan) |
| Model | Gemini 3.8 Flash |
| Commit backend saat dikerjakan | Working tree pada branch `Yasmina`; commit dasar `09101d0581695e20345a9efa8af3fce7c38b1ae4` |
| Tanggal | 21 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Seluruh cakupan roadmap (2 tabel + configuration + migration) terpenuhi. `dotnet build` PASS dan migration sudah dieksekusi — dikonfirmasi pengguna 23 September 2026, lihat Pembaruan bagian 7 |

---

## 0. Otorisasi dan metode pembuatan

Mengikuti pola preseden `BE-FIN-003`, `BE-FIN-007`, dan `BE-FIN-010`:
Cakupan roadmap membundel migration dalam task yang sama ("Cakupan: `FinBankDeposit`, `FinDailyCashSnapshot` + migration `AddFinanceCashManagement`"), namun kolom Risiko/pemilik tetap mencatat eksplisit **"migration butuh otorisasi terpisah"**. File migration ditulis tangan secara deterministik tanpa menjalankan `dotnet ef migrations add`, dan **tidak dijalankan** ke database (`dotnet ef database update` tidak dipanggil).

---

## 1. Keputusan desain yang diambil (bukan gap — didokumentasikan untuk transparansi)

### 1.1 Kas kecil TIDAK memengaruhi kas kasir / kas harian (`FIN-DEC-020`)

Sesuai acceptance criteria roadmap ("Kas kecil tidak memengaruhi kas kasir") dan aturan bisnis #15 (`FIN-DEC-020`):
Uang kas kasir pasien dan kas kecil adalah dua kolam terpisah. `FinDailyCashSnapshot` tidak memiliki kolom atau relasi yang bersumber dari `BilPettyCashBudget`/`FinPettyCashBudget`.
Formula penutupan kas pada check constraint `CK_FinDailyCashSnapshot_Formula` hanya memperhitungkan:
`ClosingBalance = OpeningBalance + CashReceiptAmount + OtherReceiptAmount - DisbursementAmount - BankDepositAmount`
Mencegah percampuran dana kas kecil ke laci kasir dan menjamin kepatuhan audit.

### 1.2 `CashierShiftId` pada `FinBankDeposit` adalah rujukan lintas Bounded Context, bukan Foreign Key database

Kolom `CashierShiftId` (tipe `Guid?`) merujuk ke `BilCashierShift` milik modul Billing (`FIN-CAP-006`). Mengikuti prinsip Clean Architecture dan DDD modular Quilvian, Finance **MUST NOT** mengunci atau membuat FK keras ke tabel operasional kasir Billing. Kolom ini diindeks (`IX_FinBankDeposit_CashierShiftId`) untuk mempermudah query rekonsiliasi shift tanpa foreign key constraint.

### 1.3 `FinDailyCashSnapshot` tidak memiliki FK ke transaksi rincian

Sesuai `erd/cash-and-master-data.md` §1: `FinDailyCashSnapshot` sengaja tidak memiliki FK ke `FinReceipt` maupun `FinBankDeposit`. Angkanya diringkas saat penutupan hari lalu dibekukan (`CLOSED`). Menghindari mutasi tidak diinginkan atas snapshot masa lalu saat ada rekonsiliasi transaksi terlambat (`FIN-DES-022`).

---

## 2. Ringkasan pekerjaan

### 2.1 Entity dan Configuration

1. **`FinBankDeposit`** (`Areas/Corporate/FinanceManagement/CashManagement/Models/FinBankDeposit.cs`):
   - Kolom: `Id`, `DepositNumber` (unik, UK), `DepositDate`, `BankAccountId` (FK), `Amount` (>0), `CashierShiftId`, `DepositSlipNumber`, `Status` (DRAFT, POSTED, VERIFIED, CANCELLED), `PostedBy`, `PostedAt`, `Notes`, `RowVersion`.
   - Mewarisi `IdentityModel`.
   - Kelas konstanta status: `FinBankDepositStatuses`.

2. **`FinDailyCashSnapshot`** (`Areas/Corporate/FinanceManagement/CashManagement/Models/FinDailyCashSnapshot.cs`):
   - Kolom: `Id`, `CashDate` (unik, UK), `OpeningBalance`, `CashReceiptAmount`, `OtherReceiptAmount`, `DisbursementAmount`, `BankDepositAmount`, `ClosingBalance`, `Status` (OPEN, CLOSED), `ClosedBy`, `ClosedAt`, `RowVersion`.
   - Mewarisi `IdentityModel`.
   - Kelas konstanta status: `FinDailyCashSnapshotStatuses`.

3. **`FinBankDepositConfiguration`** (`Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinBankDepositConfiguration.cs`):
   - Constraint: `CK_FinBankDeposit_Status`, `CK_FinBankDeposit_Amount`.
   - FK: `BankAccountId` merujuk ke `MstBankAccount` dengan `OnDelete(DeleteBehavior.Restrict)`.
   - Index unik parsial: `IX_FinBankDeposit_DepositNumber` (`WHERE "IsDelete" = false`).
   - Index pencarian: `DepositDate`, `BankAccountId`, `CashierShiftId`, `Status`.

4. **`FinDailyCashSnapshotConfiguration`** (`Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinDailyCashSnapshotConfiguration.cs`):
   - Constraint: `CK_FinDailyCashSnapshot_Status`, `CK_FinDailyCashSnapshot_Formula`.
   - Index unik parsial: `IX_FinDailyCashSnapshot_CashDate` (`WHERE "IsDelete" = false`).

### 2.2 `ApplicationDbContext.cs`

- Menambahkan namespace: `using QuilvianSystemBackend.Areas.Corporate.FinanceManagement.CashManagement.Models;`
- Mendaftarkan dua `DbSet`:
  - `public DbSet<FinBankDeposit> FinBankDeposits { get; set; }`
  - `public DbSet<FinDailyCashSnapshot> FinDailyCashSnapshots { get; set; }`

### 2.3 Migration `AddFinanceCashManagement` (`20260921000004`)

- `20260921000004_AddFinanceCashManagement.cs`: `Up()` membuat tabel `FinBankDeposit` dan `FinDailyCashSnapshot` beserta seluruh constraint dan index; `Down()` menghapus kedua tabel secara bersih.
- `20260921000004_AddFinanceCashManagement.Designer.cs`: Metadata migration terdaftar di DbContext.
- `ApplicationDbContextModelSnapshot.cs`: Diperbarui dengan menyisipkan model snapshot `FinBankDeposit` dan `FinDailyCashSnapshot` serta konfigurasi relasi `FinBankDeposit.BankAccount`.

---

## 3. Perubahan yang dikerjakan

### 3.1 Berkas yang diperiksa

- `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` (baris `BE-FIN-013`)
- `docs/module-blueprints/finance-management/erd/cash-and-master-data.md`
- `docs/module-blueprints/finance-management/erd/data-dictionary.md` §5 dan §9.4
- `docs/module-blueprints/finance-management/02-backend-architecture.md` §4.17/§4.18, §7, §9
- `docs/module-blueprints/finance-management/contracts/validation-matrix.md` §7 (`FIN-VAL-060`..`FIN-VAL-066`)
- `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §7 dan §8
- `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` (baris `CashManagement`, `Fin`, `ACTIVE`)

### 3.2 Berkas yang berubah

| Berkas | Perubahan |
| --- | --- |
| `Areas/Corporate/FinanceManagement/CashManagement/Models/FinBankDeposit.cs` | **Baru.** Entity + `FinBankDepositStatuses` |
| `Areas/Corporate/FinanceManagement/CashManagement/Models/FinDailyCashSnapshot.cs` | **Baru.** Entity + `FinDailyCashSnapshotStatuses` |
| `Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinBankDepositConfiguration.cs` | **Baru.** EF Core Configuration |
| `Repositories/Configurations/Corporate/FinanceManagement/CashManagement/FinDailyCashSnapshotConfiguration.cs` | **Baru.** EF Core Configuration |
| `Repositories/ApplicationDbContext.cs` | Penambahan `using` dan 2 `DbSet` |
| `Migrations/20260921000004_AddFinanceCashManagement.cs` | **Baru.** Migration `Up()` dan `Down()` |
| `Migrations/20260921000004_AddFinanceCashManagement.Designer.cs` | **Baru.** Metadata designer migration |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Penambahan entity snapshot dan relasi `FinBankDeposit` |

### 3.3 Dampak kontrak API, database, dan keamanan

| Aspek | Dampak |
| --- | --- |
| Kontrak API | `NOT APPLICABLE` — Task ini adalah entity dan migration slice. API Controller diimplementasikan pada `BE-FIN-015` |
| Database | 2 tabel baru (`FinBankDeposit`, `FinDailyCashSnapshot`), aditif, nol tabel existing diubah. Belum dieksekusi ke database |
| Keamanan/Auth | `NOT APPLICABLE` pada entity level. Penegakan hak akses akan diterapkan di Controller pada `BE-FIN-015` |

---

## 4. Dokumentasi endpoint

`NOT APPLICABLE` — Endpoint dibuat pada `BE-FIN-015`.

---

## 5. Verifikasi

| Skenario atau perintah | Hasil | Klasifikasi | Bukti |
| --- | --- | --- | --- |
| `dotnet ef migrations add` | **Sengaja tidak dijalankan** | `NOT RUN` | Mengikuti arahan pemilik repository — migration ditulis tangan |
| `dotnet build` | **Tidak dijalankan otomatis** | `NOT RUN` | Menghormati instruksi user ("saya akan build sendiri") |
| `dotnet ef database update` | **Tidak dijalankan** | `NOT RUN` | Otorisasi eksekusi database terpisah belum diminta/diberikan |
| QBE Conformance Check (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`) pada 4 file model dan configuration | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Terminal output task background `task-1516` |
| QBE Conformance Check pada `ApplicationDbContext.cs` dan `20260921000004_AddFinanceCashManagement.cs` | `VIOLATION: 0`, `REVIEW: 0`, `INFO: 0`, `Final result: PASS` | `PASS` | Terminal output |
| Review manual: kesesuaian DDL dan property mapping terhadap `data-dictionary.md` §9.4 | Sesuai 100% (kolom, presisi `18,2`, constraint, index) | `PASS` | Review kode |
| Review manual: pemisahan kas kecil dari formula snapshot kas | `CK_FinDailyCashSnapshot_Formula` hanya menghitung penerimaan tunai, penerimaan lain, pengeluaran, dan setoran bank | `PASS` | `FinDailyCashSnapshotConfiguration.cs` |

---

## 6. Acceptance criteria dan Definition of Done

| Kriteria (persis roadmap) | Status | Bukti |
| --- | --- | --- |
| Kas kecil tidak memengaruhi kas kasir | **Terpenuhi** | `FinDailyCashSnapshot` dan constraint `CK_FinDailyCashSnapshot_Formula` tidak melibatkan kas kecil (`FIN-DEC-020`) |
| Entity kas dan setoran + migration `AddFinanceCashManagement` | **Terpenuhi** | Model `FinBankDeposit`, `FinDailyCashSnapshot`, EF configuration, dan migration 20260921000004 selesai dibuat |
| Sifat perubahan aditif | **Terpenuhi** | Tidak ada kolom atau tabel existing yang dihapus atau diubah |
| Migration dijalankan | **Belum** | Menunggu otorisasi eksekusi database terpisah |

---

## 7. Catatan penutup

| Hal | Isi |
| --- | --- |
| **Pembaruan 23 September 2026** | Pengguna mengonfirmasi `dotnet build` PASS dan migration `AddFinanceCashManagement` sudah dieksekusi ke database. Status task dinaikkan menjadi ✅ SELESAI |
| Peringatan | Saat menjalankan `BE-FIN-014` nanti, saldo kas tersedia dihitung saat posting dari penerimaan tunai kasir (`BilCashierShift`) dikurangi setoran yang sudah `POSTED` dan selisih yang disetujui — bukan saat membuka layar |
| Masalah yang diketahui | Tidak ada |
| Risiko tersisa | **Rendah** — migration aditif murni |
| Perubahan sampingan | `NONE` |
| Status Git | 6 berkas baru di `CashManagement/` dan `Migrations/`, 2 berkas disunting (`ApplicationDbContext.cs`, `ApplicationDbContextModelSnapshot.cs`) |
| Langkah berikutnya | `BE-FIN-014` (Perhitungan kas tersedia dan penutupan harian — `FinanceCashManagementService`) |
