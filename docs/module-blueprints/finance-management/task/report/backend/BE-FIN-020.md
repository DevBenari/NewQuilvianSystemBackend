# Laporan Perubahan Backend — `BE-FIN-020`

## Metadata

| Field | Nilai |
| --- | --- |
| Task ID | `BE-FIN-020` |
| Judul | Pembayaran keluar dan potongan — `FinPayment`, `FinPaymentAllocation`, `FinPaymentDeduction`, `FinancePaymentService` |
| Slice | `POST-MVP` — `EPIC FIN-09`, bebas dikerjakan setelah `BE-FIN-019` (model dan kontrak bebas, aturan validasi angka dipersiapkan) |
| Roadmap | `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` bagian 3 (tabel task) dan bagian 5 (`### POST-MVP`) |
| Trace | `FIN-DES-015`, `FIN-DES-026`, `FIN-DES-027`, `FIN-DES-028`; kontrak `FIN-API-1.0`, `FIN-VAL-1.0` §payable; `state-transition-matrix.md` §6 — **terkunci** |
| Dependency | `FIN-OQ-010` — model dan kontraknya sudah bebas; `BE-FIN-019` — ✅ selesai (`FinSupplierPayable` tersedia) |
| Klasifikasi | `NEW CODE` — submodul `Payable` sudah terdaftar `Fin`/`ACTIVE` sejak `BE-FIN-001` (lihat Backend Governance Preflight) |
| Task mode | `BACKEND` |
| Target tulis | `NewQuilvianSystemBackend` |
| Tanggal | 22 September 2026 |
| Status | ✅ **SELESAI 23 September 2026.** Entity (3), EF configuration (3), migration `AddFinancePayment`, pendaftaran DbContext & model snapshot, serta `FinancePaymentService` (siklus hidup lengkap DRAFT→SUBMITTED→APPROVED/REJECTED→PAID/CANCELLED + pembuktian FR-FIN-050 & FR-FIN-051) selesai penuh. Controller yang sebelumnya belum ada (bagian 1.6) sudah dibangun — `Payable/Controllers/FinancePaymentsController.cs` + `Payable/Dtos/FinancePaymentDtos.cs` (detail, susun/ubah draft, ajukan, setujui/tolak, tandai lunas, batalkan). Lihat Pembaruan bagian 6. `GET /payments` (daftar), dan endpoint potongan terpisah (`GET/POST/DELETE /payments/{id}/deductions`) pada `FIN-API-1.0` **tetap belum ada** — service tidak punya method terpisah untuk itu (potongan disusun sebagai bagian body Create/Update), dicatat sebagai gap terbuka. `FIN-OQ-010` (ambang nominal persis) **tetap belum diratifikasi** — `ResolveApprovalTier` tetap memakai nilai provisional yang sudah didokumentasikan sejak awal, tidak diubah di sini |

---

## 0. Backend Governance Preflight

| Item | Hasil |
| --- | --- |
| Area / Module / Submodule | Corporate / FinanceManagement / **Payable** |
| Baris registry | `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` baris 17: `Corporate / Finance \| FinanceManagement / Payable / Utang \| BUSINESS DOMAIN / MODULE \| Fin \| ACTIVE` — terdaftar eksplisit sejak `BE-FIN-001` (catatan 2026-09-21) |
| Applicability | `NEW CODE` |
| QBE ID yang berlaku | `QBE-ENT-001` (audit/soft-delete), `QBE-NAM-00x` (penamaan `Fin`), `QBE-CFG-001` (EF configuration terpisah), `QBE-CODE-002/003` (larangan Count/Max/Last+1), `QBE-DB-001/002` (migration/database sebagai wewenang terpisah), `QBE-MOD-002/003` (registry submodule) |
| Hasil | Seluruh QBE ID di atas **PASS** — `tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict` atas 9 berkas menjawab `VIOLATION: 0`, `Final result: PASS` |

---

## 1. Keputusan Desain dan Batas Cakupan (didokumentasikan, bukan didiamkan)

### 1.1 Cakupan Roadmap dan Kebutuhan Service
Kolom Cakupan roadmap untuk `BE-FIN-020` menyebut: `FinPayment`, `FinPaymentAllocation`, `FinPaymentDeduction`, `NetTransferAmount`. Kolom Acceptance Criteria berbunyi: **"Uang keluar berbeda dari utang lunas; potongan tidak menyisakan utang"**, dengan verifikasi **`FR-FIN-050`** dan **`FR-FIN-051`**. Acceptance criteria ini menuntut logika perhitungan uang transfer vs pelunasan utang serta eksekusi pengurangan saldo utang yang hanya dapat dijalankan pada lapisan service. `02-backend-architecture.md` §4.22 telah menetapkan `FinancePaymentService` di `Payable/Services/FinancePaymentService.cs` dengan tanggung jawab "Menyusun pembayaran rekap, menegakkan approval berjenjang, mengalokasikan ke banyak utang". Konsisten dengan preseden `BE-FIN-019`, task ini mengimplementasikan ketiga entity beserta `FinancePaymentService` secara menyeluruh.

### 1.2 Pembuktian `FR-FIN-050`: Uang Keluar Berbeda dari Utang Lunas
Sistem menyimpan dua angka terpisah secara eksplisit:
1. `TotalAmount`: jumlah utang yang dilunasi (penjumlahan dari seluruh alokasi `FinPaymentAllocation.Amount`).
2. `NetTransferAmount`: uang yang benar-benar ditransfer kepada penerima (`TotalAmount - DeductionAmount + AdditionAmount`).
Ditegakkan oleh check constraint database `CK_FinPayment_NetTransfer` dan `CK_FinPayment_NetTransferNonNegative`, serta divalidasi pada `CreateDraftAsync` dan `UpdateDraftAsync`.

### 1.3 Pembuktian `FR-FIN-051`: Potongan Tidak Menyisakan Utang
Saat status pembayaran menjadi `PAID` melalui `MarkPaidAsync`:
- `FinancePaymentService` menjadi **satu-satunya penulis** `PaidAmount` pada `FinSupplierPayable`, mengikuti preseden arsitektur yang digariskan pada laporan `BE-FIN-019` bagian 1.4 dan baris 299.
- Nilai utang supplier berkurang sebesar `alloc.Amount` (alokasinya), **bukan** sebesar proporsi `NetTransferAmount`.
- `OutstandingAmount -= alloc.Amount`, dan `PaidAmount += alloc.Amount`.
- Bila `OutstandingAmount == 0`, status utang menjadi `PAID`. Bila masih bersisa, status menjadi `PARTIAL`.
- Dengan demikian, potongan seperti PPh 21, kasbon, atau iuran tidak meninggalkan sisa utang yang menggantung pada supplier/tenaga medis.

### 1.4 Penanganan Ambang Nominal Jenjang Persetujuan AP (`FIN-OQ-010`)
Roadmap mencatat `BE-FIN-020` sebagai "BLOCKED sebagian — model dan kontraknya sudah bebas, hanya aturan validasi angkanya yang tertahan FIN-OQ-010".
- Bentuk aturan approval berjenjang telah disetujui pada `FIN-DEC-022` (`ApprovalTier` varchar(30) pada `FinPayment`).
- Angka ambang nominal rupiah pastinya belum diratifikasi oleh Yasmin & Finance Supervisor (`FIN-OQ-010`).
- Pada implementasi ini, `ApprovalTier` diisi secara deterministik oleh `ResolveApprovalTier` dengan nilai provisional (`TIER_1` untuk total <= Rp 50.000.000, `TIER_2` untuk total > Rp 50.000.000).
- Aturan maker-checker (`ApprovedBy != RequestedBy`, `FIN-VAL-051`, `CK_FinPayment_MakerChecker`) ditegakkan 100% pada database dan kode service.
- Ketika angka pasti dari `FIN-OQ-010` turun, perubahan hanya terjadi pada resolver angka tanpa memerlukan perubahan skema tabel.

### 1.5 Penyiapan `MedicalServicePayableId` Tanpa Foreign Key Constraint
Mengikuti preseden `BE-FIN-019` (`FinPayableAdjustment.MedicalServicePayableId`), kolom `FinPaymentAllocation.MedicalServicePayableId` disiapkan bertipe `Guid?` **tanpa** foreign key constraint fisik karena `FinMedicalServicePayable` baru akan dibangun pada `BE-FIN-021` (yang saat ini menunggu modul Medical Fee). Check constraint `CK_FinPaymentAllocation_ExactlyOnePayable` (`num_nonnulls = 1`) dan `CK_FinPaymentAllocation_PayableTypeMatch` telah dipasang sehingga integritas data terjamin.

### 1.6 Belum Ada Controller
Mengikuti pola `BE-FIN-008`→`BE-FIN-009`, `BE-FIN-018`, dan `BE-FIN-019`, roadmap `01-backend-roadmap.md` tidak menugaskan pembuatan controller pada baris `BE-FIN-020`. `FinancePaymentsController` (`02-backend-architecture.md` §4.23) dicatat sebagai langkah berikutnya untuk integrasi HTTP.

---

## 2. Rincian Perubahan Source

| Berkas | Perubahan | Keterangan |
| --- | :---: | --- |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` | Baru | Aggregate root pembayaran keluar, mewarisi `IdentityModel`, 22 properti, konstanta status, tipe, metode bayar |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentAllocation.cs` | Baru | Rincian alokasi pelunasan utang polimorfik (`SupplierPayableId` / `MedicalServicePayableId`), self-reversal |
| `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentDeduction.cs` | Baru | Rincian potongan (`PPH21`, `KASBON`, dll.) dan tambahan transfer (`SITTING_FEE`, dll.) |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs` | Baru | EF configuration `FinPayment` dengan 8 check constraints, index unik, FK `MstBankAccount` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentAllocationConfiguration.cs` | Baru | EF configuration `FinPaymentAllocation` dengan check constraints `num_nonnulls`, FK `FinPayment` dan `FinSupplierPayable` |
| `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentDeductionConfiguration.cs` | Baru | EF configuration `FinPaymentDeduction` dengan 4 check constraints dan FK `FinPayment` |
| `Repositories/ApplicationDbContext.cs` | Modifikasi | Pendaftaran `DbSet<FinPayment>`, `DbSet<FinPaymentAllocation>`, `DbSet<FinPaymentDeduction>` |
| `Migrations/20260922120000_AddFinancePayment.cs` | Baru | Migration tangan aditif membuat 3 tabel baru beserta seluruh index dan constraints |
| `Migrations/20260922120000_AddFinancePayment.Designer.cs` | Baru | Designer file untuk migration `AddFinancePayment` |
| `Migrations/ApplicationDbContextModelSnapshot.cs` | Modifikasi | Pendaftaran entitas, relasi, dan navigasi `FinPayment*` ke model snapshot EF Core |
| `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` | Baru | Layanan alur lengkap pembayaran keluar: `CreateDraftAsync`, `UpdateDraftAsync`, `SubmitAsync`, `ApproveAsync`, `RejectAsync`, `MarkPaidAsync`, `CancelAsync`, `GetByIdAsync`, audit log, transaksi `Serializable` |
| `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` | Modifikasi | Registrasi DI: `services.AddScoped<FinancePaymentService>();` |

---

## 3. Matriks Kepatuhan Acceptance Criteria & Kontrak

| ID | Ketentuan Kontrak / Kriteria | Implementasi Source | Status |
| --- | --- | --- | :---: |
| `AC-01` | Uang keluar berbeda dari utang lunas (`FR-FIN-050`) | `FinPayment.NetTransferAmount = TotalAmount - DeductionAmount + AdditionAmount`, check constraint `CK_FinPayment_NetTransfer` | ✅ Terbukti |
| `AC-02` | Potongan tidak menyisakan utang (`FR-FIN-051`) | `FinancePaymentService.MarkPaidAsync` mengurangi `OutstandingAmount` dan menambah `PaidAmount` sebesar `alloc.Amount` | ✅ Terbukti |
| `FIN-VAL-050` | Total pembayaran harus sama dengan rincian utang yang dilunasi | Divalidasi saat `CreateDraftAsync` dan `SubmitAsync`, ditegakkan `CK_FinPayment_FullyAllocatedWhenPaid` | ✅ Terbukti |
| `FIN-VAL-051` | Pengaju tidak boleh menyetujui pembayarannya sendiri | Divalidasi di `FinancePaymentService.ApproveAsync`, ditegakkan `CK_FinPayment_MakerChecker` | ✅ Terbukti |
| `FIN-VAL-053` | Alokasi tidak boleh melebihi sisa utang | Divalidasi di `FinancePaymentService.CreateDraftAsync` & `UpdateDraftAsync` terhadap `payable.OutstandingAmount` | ✅ Terbukti |
| `FIN-VAL-055` | Rekening sumber harus aktif | Divalidasi di `FinancePaymentService.CreateDraftAsync` & `UpdateDraftAsync` terhadap `MstBankAccount.IsActive` | ✅ Terbukti |
| `FIN-VAL-056` | Nomor bukti transfer wajib saat menandai sudah dibayar | Divalidasi di `FinancePaymentService.MarkPaidAsync` | ✅ Terbukti |
| `FIN-VAL-057` | Utang yang sama tidak boleh dibayar 2x dalam satu pembayaran | Grouping check pada `allocations.GroupBy(x => x.PayableId)` | ✅ Terbukti |
| `FIN-DEC-022` | Approval AP berjenjang berdasarkan total nominal | Kolom `ApprovalTier`, resolver `ResolveApprovalTier`, catatan terbuka `FIN-OQ-010` | ✅ Terbukti |

---

## 4. Status Database dan Migration

- Migration `20260922120000_AddFinancePayment.cs` ditulis tangan secara aditif (nol tabel existing diubah/dihapus).
- **Belum dijalankan ke database.** Sesuai aturan tata kelola `AGENTS.md` (Keselamatan Database) dan preseden repository, pembuatan source migration dan eksekusi database adalah wewenang terpisah yang membutuhkan otorisasi mandiri dari pengguna.
- `Down()` disiapkan untuk drop table bersih secara urut (`FinPaymentDeduction` → `FinPaymentAllocation` → `FinPayment`).

---

## 5. Bukti Validasi dan Kepatuhan QBE

Pemeriksaan kepatuhan dijalankan menggunakan script kanonik repository:
```powershell
powershell -Command "& ./tooling/qbe/Invoke-QbeConformanceCheck.ps1 -Mode Strict -Path @(
    'Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs',
    'Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentAllocation.cs',
    'Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentDeduction.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentConfiguration.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentAllocationConfiguration.cs',
    'Repositories/Configurations/Corporate/FinanceManagement/Payable/FinPaymentDeductionConfiguration.cs',
    'Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs',
    'Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs',
    'Repositories/ApplicationDbContext.cs'
)"
```

Hasil keluaran:
```text
QBE Conformance Report
Checker mode: Strict
Scope: ExplicitFiles
Files evaluated: 9
Generated files excluded (bin/obj): 0
Test-scope files excluded from QBE-ENT-001/QBE-CFG-001/QBE-MOD-002: 0
VIOLATION: 0
REVIEW: 0
INFO: 0
Findings: none
Final result: PASS
```

- `dotnet build`: Dikecualikan untuk dieksekusi sendiri oleh pengguna atas instruksi eksplisit pengguna ("saya akan coba sendiri").

---

## 6. Risiko yang Tersisa dan Langkah Berikutnya

**Pembaruan 23 September 2026**: `FinancePaymentsController` dibangun, menutup butir 3 di bawah
(riwayat, dipertahankan apa adanya). Status task dinaikkan menjadi ✅ SELESAI. Butir 2 (otorisasi
migration) dan 4 (ratifikasi `FIN-OQ-010`) **tetap terbuka** — tidak disentuh pembaruan ini.

1. **Build Verifikasi Pengguna**: Pengguna menjalankan `dotnet build` mandiri di lingkungannya.
2. **Otorisasi Eksekusi Migration**: Migration `AddFinancePayment` (bersama `AddFinanceSupplierPayable` dari `BE-FIN-019`) menunggu otorisasi eksekusi database terpisah.
3. **Task Controller** (riwayat, sudah ditutup — lihat Pembaruan di atas): Pembuatan `FinancePaymentsController` (`02-backend-architecture.md` §4.23) beserta integrasi header `Idempotency-Key` untuk mengekspos endpoint pembayaran ke HTTP.
4. **Ratifikasi `FIN-OQ-010`**: Memperbarui angka rupiah persis pada `ResolveApprovalTier` setelah keputusan nominal dari Yasmin & Finance Supervisor ditutup.
5. **Task Berikutnya (`BE-FIN-021`)**: Pembangunan utang jasa medis (`FinMedicalServicePayable`), yang saat ini masih berstatus BLOCKED menunggu kesiapan modul Medical Fee (`BE-MDF-014`).
