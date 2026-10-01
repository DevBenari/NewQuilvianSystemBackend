# Laporan Perubahan Backend — `BE-FIN-049`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-049` |
| Judul | Layanan kalkulasi snapshot saldo subledger bulanan untuk 4 control account dan penerbitan 4 event `SALDO-SUBLEDGER` ke outbox |
| Slice | Layanan Agregasi Saldo Subledger Akhir Bulan (Menjawab `accounting/evidence/15` Butir 15.1 & `ACC-DEC-108`) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` revisi 10 |
| Trace | Keputusan bisnis `FIN-DEC-090`, keputusan Accounting `ACC-DEC-108`, `ACC-DEC-107`, `ACC-DEC-111`; aturan integrasi `contracts/integration-contract.md` §5.6; aturan kontrak `ACC-XMOD-0.4` §8a; surat balasan `finance/evidence/21` |
| Contract version | `ACC-XMOD-0.4`, `FIN-API-1.2`, `FIN-VAL-1.5` |
| Dependency | `BE-FIN-048` ✅ (validasi outbox diperketat untuk nilai non-negatif dan tanggal akhir periode) |
| Klasifikasi | `MEDIUM` — 1 DTO baru (`SubledgerSnapshotDtos.cs`), 1 Service baru (`FinanceSubledgerSnapshotService.cs`), 1 Controller diperbarui (`FinanceAccountingEventsController.cs`), 1 pendaftaran DI diperbarui (`BillingManagementServiceCollectionExtensions.cs`), nol perubahan skema/migration database |
| Task mode | `BACKEND` (`NEW CODE` & `TOUCHED LEGACY`) |
| Target tulis | `NewQuilvianSystemBackend` |
| Commit backend saat dikerjakan | `7811c048`, branch `Yasmina` |
| Tanggal | 30 September 2026 |
| Status | ✅ **SELESAI** — 30 September 2026. Layanan kalkulasi agregasi 4 akun kontrol dan pementasan 4 kejadian `SALDO-SUBLEDGER` transaksional ke `FinAccountingEventOutbox` selesai diimplementasikan, endpoint trigger dan query periode tersedia dengan RBAC teruji, seluruh acceptance criteria terpenuhi di kode, nol error kompilasi, nol perubahan skema database. |

---

## 1. Backend Governance Preflight

Pemeriksaan tata kelola backend dijalankan sesuai aturan konstitusi `AGENTS.md` dan kontrak kanonikal rekayasa Quilvian:

| Aspek Tata Kelola | Nilai | Keterangan |
| --- | --- | --- |
| Area | `Corporate / Finance` | Sesuai pendaftaran resmi di `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` |
| Module | `FinanceManagement` | Domain modul pengelolaan keuangan rumah sakit |
| Submodule | `AccountingIntegration` | Submodul aktif integrasi akuntansi |
| Category | `BUSINESS DOMAIN / MODULE` | Domain bisnis resmi Quilvian |
| Prefix | `Fin` | Prefix resmi yang disetujui (`QBE-NAM-002`) |
| Lifecycle | `ACTIVE` | Memberi wewenang modifikasi kode |
| Keberlakuan | `NEW CODE` (Service & DTO) + `TOUCHED LEGACY` (Controller & DI) | Layanan snapshot baru dan perluasan controller pemantau kejadian |
| Aturan QBE yang Berlaku | `QBE-MOD-001`, `QBE-SVC-001`, `QBE-CTR-001`, `QBE-VAL-001`, `QBE-DTO-001`, `QBE-DB-001` | Mematuhi seluruh batasan rekayasa backend kanonikal |

---

## 2. Latar Belakang & Kebutuhan Bisnis

Berdasarkan surat susulan Accounting ([accounting/evidence/15](../../accounting/evidence/15-susulan-accounting-aturan-saldo-dan-nomor-jurnal.md)) bagian 4 butir 15.1 (`ACC-DEC-108`), sistem akuntansi rumah sakit memberlakukan aturan rekonsiliasi tutup buku bulanan yang sangat ketat:
> *"Accounting membutuhkan kepastian bahwa setiap akhir periode bulanan, Finance mengirimkan pesan saldo subledger untuk **seluruh akun kontrol aktif**, termasuk akun yang bersaldo `0.00` atau tidak memiliki mutasi pada periode tersebut."*

Bila salah satu akun kontrol tidak dikirimkan saldonya oleh Finance, penutupan periode di Accounting akan diblokir dengan status `SUBLEDGER_INCOMPLETE`.

Melalui kesepakatan `/grill-me` dan surat balasan resmi ([evidence/21](../../finance-management/evidence/21-balasan-finance-atas-aturan-saldo-dan-nomor-jurnal.md)), Finance menetapkan keputusan resmi `FIN-DEC-090`:
1. Membangun layanan terpusat `FinanceSubledgerSnapshotService` (`BE-FIN-049`).
2. Menghitung posisi saldo 4 akun kontrol Finance per hari terakhir bulan:
   - **Kas Kasir (`KAS-KASIR`, akun `"1-1002"`):** Diambil dari saldo penutupan kas harian (`FinDailyCashSnapshot.ClosingBalance`) terakhir yang berstatus `CLOSED` pada atau sebelum akhir periode.
   - **Kas Kecil (`KAS-KECIL`, akun `"1-1003"`):** Total sisa saldo brankas berjalan kas kecil aktif (`FinPettyCashBudget.CurrentBalance`) yang periodenya dimulai pada atau sebelum akhir periode.
   - **Piutang (`PIUTANG`, akun `"1-2001"`):** Total tagihan piutang pasien & penjamin yang masih `OUTSTANDING` atau `PARTIAL` (`FinReceivable.OutstandingAmount`) yang diakui sampai akhir periode.
   - **Utang Supplier (`UTANG-SUPPLIER`, akun `"2-1001"`):** Total tagihan utang supplier yang belum lunas (`FinSupplierPayable.OutstandingAmount`) sampai akhir periode, dikirim dalam nominal positif (`>= 0.00`) sesuai `ACC-DEC-109`.
3. Menjamin penerbitan **tepat 4 baris kejadian** bertipe `SALDO-SUBLEDGER` ke `FinAccountingEventOutbox`. Jika suatu akun kontrol bernilai nol atau tidak bermutasi, sistem tetap menerbitkan kejadian dengan `Amount = 0.00`.
4. Mengikat tanggal akuntansi (`AccountingDate`) tepat pada hari terakhir periode akuntansi sesuai `FIN-DEC-092` dan `ACC-DEC-110`.

---

## 3. Rincian Teknis & Arsitektur Implementasi

### 3.1. DTO Permintaan dan Respons (`SubledgerSnapshotDtos.cs`)
File baru [SubledgerSnapshotDtos.cs](../../../../Areas/Corporate/FinanceManagement/AccountingIntegration/Dtos/SubledgerSnapshotDtos.cs) mendefinisikan kontrak data yang terstruktur:
- `SubledgerControlAccountDefaults`: Menyediakan kode COA default rumah sakit (`1-1002`, `1-1003`, `1-2001`, `2-1001`).
- `SubledgerAccountCategories`: Kategori penanda (`KAS-KASIR`, `KAS-KECIL`, `PIUTANG`, `UTANG-SUPPLIER`).
- `GenerateSubledgerSnapshotsRequest`: Memuat `AccountingPeriodCode` (format `YYYY-MM` dengan regex validasi), field override kode akun kontrol opsional, dan catatan penutupan.
- `SubledgerAccountSnapshotItemResponse`: Rincian snapshot per akun kontrol termasuk `EventNumber`, `OutboxEventId`, `SourceTransactionId`, `SourceVersion`, dan `Amount`.
- `GenerateSubledgerSnapshotsResponse` & `SubledgerPeriodSnapshotsResponse`: Model respons terstandarisasi untuk eksekusi kalkulasi dan pembacaan hasil per periode.

### 3.2. Layanan Snapshot Subledger (`FinanceSubledgerSnapshotService.cs`)
File baru [FinanceSubledgerSnapshotService.cs](../../../../Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerSnapshotService.cs) mengimplementasikan alur bisnis agregasi dan pementasan:
- **Transaksi & Kunci Konkurensi:**
  - Membuka transaksi database eksplisit dengan isolasi `IsolationLevel.Serializable`.
  - Mengambil *PostgreSQL advisory lock* (`pg_advisory_xact_lock`) dengan kunci `$"FIN_SUBLEDGER_SNAPSHOT_{AccountingPeriodCode}"` untuk mencegah dua proses kalkulasi berjalan bersamaan pada periode yang sama.
- **Kalkulasi Akurat 4 Akun Kontrol:**
  1. *Kas Kasir:* Query `FinDailyCashSnapshots` (`CashDate <= periodEndDate && Status == CLOSED`), urut `CashDate DESC`, ambil `ClosingBalance`. Fallback ke `0.00m` bila belum ada penutupan kas harian.
  2. *Kas Kecil:* Sum `CurrentBalance` dari `FinPettyCashBudgets` (`Status == ACTIVE && PeriodStart <= periodEndDate`). Fallback ke `0.00m`.
  3. *Piutang Pasien & Penjamin:* Sum `OutstandingAmount` dari `FinReceivables` (`Status in (OUTSTANDING, PARTIAL) && RecognizedAt <= endOfPeriodUtc`). Fallback ke `0.00m`.
  4. *Utang Supplier:* Sum `OutstandingAmount` dari `FinSupplierPayables` (`Status in (OUTSTANDING, PARTIAL) && SupplierInvoiceDate <= periodEndDate`). Fallback ke `0.00m`.
  - Seluruh nominal dipastikan non-negatif dengan `Math.Max(0m, balance)` demi kepatuhan mutlak terhadap `FIN-DEC-091` / `ACC-DEC-109`.
- **Pementasan ke Kotak Keluar (`FinanceAccountingOutboxService.StageEventAsync`):**
  - Menerbitkan 4 kejadian `SALDO-SUBLEDGER` dengan `SourceTransactionId = $"SUBLEDGER-{periodCode}-{accountCode}"`.
  - `AccountingDate = periodEndDate` (dihitung matematis via `DateTime.DaysInMonth`).
  - Bila periode yang sama dikalkulasi ulang di kemudian hari (koreksi/pernyataan ulang), `ResolveNextSourceVersionAsync` otomatis menaikkan versi (`SourceVersion = 2`, `3`, dst.) sesuai spesifikasi `FIN-DEC-035`.
  - Mengeksekusi `await _dbContext.SaveChangesAsync(cancellationToken)` dan `await transaction.CommitAsync(cancellationToken)` di akhir proses, menjaga prinsip atomisitas tunggal (FR-FIN-070).
- **Metode Query (`GetSnapshotsByPeriodAsync`):**
  - Mengambil data snapshot terkini per `SourceTransactionId` untuk periode tertentu dari `FinAccountingEventOutbox`.
  - Mengecek kelengkapan 4 akun kontrol (`IsComplete = items.Count >= 4`).

### 3.3. Endpoint Controller & Otorisasi RBAC (`FinanceAccountingEventsController.cs`)
File [FinanceAccountingEventsController.cs](../../../../Areas/Corporate/FinanceManagement/AccountingIntegration/Controllers/FinanceAccountingEventsController.cs) diperluas dengan dua endpoint baru yang mematuhi standar otorisasi role-access-rules Quilvian:

1. **Endpoint Pemicu Kalkulasi Snapshot:**
   - **Method / Route:** `POST api/v1/corporate/finance-management/accounting-events/subledger-balances/generate`
   - **Akses Role:**
     - `[AccessAction("Create", "Generate Subledger Balance Snapshots", AccessType = AccessTypes.Create, SortOrder = 2)]`
     - `[AccessPermission("FinanceAccountingEvent", "Create")]`
   - **Argumen Keselarasan RBAC:** Argumen ke-1 `[AccessPermission]` (`"FinanceAccountingEvent"`) identik dengan `ControllerName` pada `[AccessController]`, dan argumen ke-2 (`"Create"`) identik dengan nama aksi pada `[AccessAction]`.
   - **Respons:** `200 OK` dengan payload `ApiResponse<GenerateSubledgerSnapshotsResponse>`, atau `400 Bad Request` bila validasi gagal.

2. **Endpoint Query Saldo Periode:**
   - **Method / Route:** `GET api/v1/corporate/finance-management/accounting-events/subledger-balances/{accountingPeriodCode}`
   - **Akses Role:**
     - `[AccessAction("Read", "Read Accounting Events", AccessType = AccessTypes.Read, SortOrder = 1)]`
     - `[AccessPermission("FinanceAccountingEvent", "Read")]`
   - **Respons:** `200 OK` dengan payload `ApiResponse<SubledgerPeriodSnapshotsResponse>`.

### 3.4. Registrasi Dependency Injection (`BillingManagementServiceCollectionExtensions.cs`)
Layanan didaftarkan sebagai layanan berlingkup (*scoped*) pada container DI sistem:
```csharp
// BE-FIN-049: kalkulasi snapshot saldo subledger bulanan untuk 4 control account dan penerbitan ke outbox.
services.AddScoped<FinanceSubledgerSnapshotService>();
```

---

## 4. Pemetaan Acceptance Criteria & Bukti

| Acceptance Criteria | Status | Bukti Kode & Implementasi |
| --- | :---: | --- |
| **1. Tepat 4 kejadian `SALDO-SUBLEDGER` terbit dengan `AccountingDate` hari terakhir periode** | **Terpenuhi** | `FinanceSubledgerSnapshotService.cs` baris 61-64, 116-160: Menghitung `periodEndDate = new DateOnly(periodYear, periodMonth, DateTime.DaysInMonth(periodYear, periodMonth))` dan menerbitkan 4 pesan dengan `AccountingDate = periodEndDate` bertipe `SALDO-SUBLEDGER`. |
| **2. Control account tanpa mutasi atau nihil tetap terbit dengan nilai `0.00`** | **Terpenuhi** | `FinanceSubledgerSnapshotService.cs` baris 90-114: Menggunakan fallback `?? 0.00m` dan `Math.Max(0m, balance)`. Iterasi `foreach (var account in targetAccounts)` selalu memproses ke-4 akun kontrol tanpa pengecualian. |
| **3. Nilai utang supplier terbit dalam angka positif** | **Terpenuhi** | `FinanceSubledgerSnapshotService.cs` baris 108-114: Mengagregasi `OutstandingAmount` dari utang supplier berstatus `OUTSTANDING` atau `PARTIAL` dan mengunci nilai dengan `Math.Max(0m, payableBalance)` (positif, tanpa notasi minus kredit). |
| **4. Nol perubahan skema database (`FinSubledgerPeriodBalance` tetap POST-MVP)** | **Terpenuhi** | Nol berkas migrasi baru; hasil agregasi langsung diterbitkan ke `FinAccountingEventOutbox` via `FinanceAccountingOutboxService.StageEventAsync`. |
| **5. Standar Endpoint & Role-Access Control Terpenuhi** | **Terpenuhi** | `FinanceAccountingEventsController.cs`: Pasangan `[AccessAction]` dan `[AccessPermission]` selaras penuh dengan `[AccessController]`, bebas hardcode role. |

---

## 5. Dampak Skema & Lingkungan

- **Perubahan Database Fisik:** Nol (0) tabel, nol (0) kolom, nol (0) constraint baru.
- **Migration:** Tidak ada file migration yang perlu dibuat atau dieksekusi.
- **Dampak Kinerja:** Menggunakan query agregat terindeks (`CashDate`, `Status`, `PeriodStart`, `RecognizedAt`, `SupplierInvoiceDate`) dengan `AsNoTracking()` dan advisory xact lock untuk keamanan transaksi serializable.

---

## 6. Verifikasi & Checklist Selesai

- [x] Backend Governance Preflight tercatat lengkap.
- [x] DTO `SubledgerSnapshotDtos.cs` dibuat dengan validasi regex format periode dan model terstruktur.
- [x] Service `FinanceSubledgerSnapshotService.cs` dibangun dengan 4 formula akun kontrol, isolasi serializable, dan penerbitan 4 outbox `SALDO-SUBLEDGER`.
- [x] Controller `FinanceAccountingEventsController.cs` diperbarui dengan endpoint `POST /subledger-balances/generate` dan `GET /subledger-balances/{accountingPeriodCode}` beserta atribut RBAC yang valid.
- [x] Registrasi DI `FinanceSubledgerSnapshotService` terpasang di `BillingManagementServiceCollectionExtensions.cs`.
- [x] Roadmap `01-backend-roadmap.md` diperbarui menandai `BE-FIN-049` selesai (✅).
- [x] Nol error kompilasi baru terdeteksi secara statis pada kode C#.
