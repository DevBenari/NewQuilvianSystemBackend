# BE-FIN-030 — Model Retur Pembelian, Deposit Retur (bentuk revisi 5), dan asal Purchasing Invoice pada utang supplier

- TASK ID: BE-FIN-030
- TASK TYPE: NEW CODE (empat entity baru pada submodul `Purchasing`) + TOUCHED LEGACY (satu kolom nullable pada `FinSupplierPayable` yang sudah berjalan)
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/Models/**` (baru), `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/**` (baru), `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` (kolom baru), `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` (mapping baru), `Repositories/ApplicationDbContext.cs` (`DbSet` baru)

- BACKEND GOVERNANCE PREFLIGHT:
  - Area/Module/Submodule: `Corporate / Finance` / `FinanceManagement` / `Purchasing` (`ACTIVE` sejak `BE-FIN-027`) untuk empat entity baru; `Payable` (`ACTIVE`, sudah lama) untuk kolom `FinSupplierPayable`
  - Prefix: `Fin` — dipakai apa adanya, tidak ada prefix baru
  - Keberlakuan: `NEW CODE` untuk `FinSupplierReturn`/`Item`/`FinSupplierReturnDeposit`/`Usage`; `TOUCHED LEGACY` untuk `FinSupplierPayable` (kolom nullable ditambahkan pada entity yang sudah berjalan, `BE-FIN-019` ✅)
  - Status registry: terpenuhi, tidak `BLOCKED`
  - QBE ID yang berlaku: `QBE-ENT-001`, `QBE-ENT-002`, `QBE-NAM-002`, `QBE-CFG-001`, `QBE-MOD-001` untuk entity baru; `QBE-CFG-002` (SHOULD, perbaikan configuration aman dalam cakupan) untuk `FinSupplierPayableConfiguration`

- FILES INSPECTED:
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinPayableAdjustment.cs` — pola tambahan class `static …Types`/`…Directions` dalam satu file, pola kolom polimorfik nullable dengan navigasi opsional
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` (sebelum edit) dan `FinSupplierPayableConfiguration.cs` (sebelum edit) — bentuk kolom existing, urutan `HasIndex` di akhir `Configure`
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinPayment.cs` — konfirmasi `FinPayment` adalah target FK `PaymentId` yang benar (aggregate root pembayaran, bukan `FinPaymentAllocation`)
  - `docs/module-blueprints/finance-management/erd/data-dictionary.md` bagian C.8-C.10 (`FinSupplierReturn`/`Item`/`Deposit`, tidak dikoreksi revisi 5), bagian C.11 (**DIGANTIKAN**, ditandai eksplisit di berkas) dan D.2 (bentuk `FinSupplierReturnDepositUsage` yang berlaku — `PaymentId`, `Status`, `ReleasedAt`, `RowVersion`, unique index parsial), bagian D.4(b) untuk DDL
  - `docs/module-blueprints/finance-management/02-backend-architecture.md` bagian C.1-C.2 (`FIN-DES-038`), D.1-D.2 (`FIN-DES-045`, `046` — alasan bentuk revisi 5)
  - `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` `FIN-STATE-1.2` B.5 (Retur Pembelian) dan `FIN-STATE-1.3` C.1-C.2 (revisi 5 — baris pemakaian dan status deposit)
  - `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-029.md` — laporan task sebelumnya, memastikan `FinPurchasingInvoice` (target FK `FinSupplierReturn.PurchasingInvoiceId` dan `FinSupplierPayable.SourcePurchasingInvoiceId`) sudah ada sebagai source, walau belum ter-build
  - Pencarian nama class dan `DbSet` di seluruh repository — nol tabrakan

- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturn.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnItem.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnDeposit.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnDepositUsage.cs` — **bentuk revisi 5**: `PaymentId` (FK ke `FinPayment`), bukan `PurchasingInvoiceId`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnItemConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnDepositConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnDepositUsageConfiguration.cs`
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs` — tambah `using` untuk `Purchasing.Models`, tambah properti `SourcePurchasingInvoiceId` (`Guid?`) dan navigasi `SourcePurchasingInvoice`
  - `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` — tambah `HasOne(...).OnDelete(DeleteBehavior.SetNull)` dan satu `HasIndex` untuk kolom baru; **tidak ada baris lama yang dihapus atau diubah maknanya**
  - `Repositories/ApplicationDbContext.cs` — empat `DbSet<T>` baru ditambahkan setelah blok `BE-FIN-029`; **tidak ada baris lama yang diubah**

- IMPLEMENTATION: Empat entity mengikuti bentuk `erd/data-dictionary.md` C.8-C.10 dan **D.2** (bukan C.11 yang sudah ditandai digantikan). `FinSupplierReturn` (+ `Item`) mencatat retur atas satu `FinPurchasingInvoice` yang sudah `APPROVED` (ditegakkan service, `BE-FIN-035`, bukan check constraint lintas tabel). `FinSupplierReturnDeposit` adalah kredit retur milik supplier, satu-ke-satu dengan `FinSupplierReturn` sumbernya (`HasOne().WithOne().HasForeignKey<FinSupplierReturnDeposit>()`, mengunci "satu retur = satu deposit", `FIN-DEC-047`).

  **Titik paling penting task ini:** `FinSupplierReturnDepositUsage` dibangun langsung dalam **bentuk revisi 5** — `PaymentId` menunjuk `FinPayment` (aggregate root pembayaran keluar, submodul `Payable`), bukan `PurchasingInvoiceId` seperti rancangan awal AMENDMENT REVISI 4 yang sudah ditandai **DIGANTIKAN** di `erd/data-dictionary.md` bagian C.11. Ini konsekuensi `FIN-DEC-057` (Deposit Retur dipakai sebagai sumber dana di tingkat pembayaran, bukan di tingkat invoice) yang digambar `02-backend-architecture.md` bagian D. Kolom `Status` (`RESERVED`/`APPLIED`/`RELEASED`) dan `ReleasedAt` disiapkan sesuai `FIN-DES-046` (reservasi saat `DRAFT`, bukan saat `PAID`) — task ini hanya menyiapkan kolomnya; transisi statusnya ditulis `BE-FIN-036`.

  `FinSupplierPayable.SourcePurchasingInvoiceId` ditambahkan sebagai kolom **nullable**, `FK SetNull` ke `FinPurchasingInvoice`. Jalur input manual `FinSupplierPayableService` (`BE-FIN-019` ✅) **tidak disentuh** — baris yang dibuatnya tetap valid dengan kolom baru bernilai `NULL`, sesuai `FIN-DES-040`.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Nol. Tidak ada controller/endpoint pada task ini.
- DATABASE IMPACT: **Skema disiapkan, belum diterapkan.** Empat tabel baru + satu kolom pada tabel yang sudah berjalan (`FinSupplierPayable`), sesuai rencana migration `data-dictionary.md` D.7: keduanya masuk migration `AddPurchasingApRumpun` (`BE-FIN-031`), yang belum dibuat. Nol tabel existing lain tersentuh. `FinSupplierPayable` sendiri: perubahan aditif murni (kolom nullable, default `NULL`) — baris lama tidak perlu backfill.
- SECURITY IMPACT: Nol pada task ini. Belum ada controller/endpoint/permission.
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna — pengguna menjalankan sendiri. **Bukan klaim PASS**, dan task ini bertumpuk di atas `BE-FIN-029` yang juga belum terverifikasi compiler |
  | Pencarian nama class empat entity baru di seluruh repository | Masing-masing tepat satu definisi | PASS (review) | `grep -rl "class Fin…\b"` |
  | Pencarian nama property `DbSet` empat entity baru | Masing-masing tepat satu | PASS (review) | `grep` pasca-edit |
  | Review `erd/data-dictionary.md` C.11 vs D.2 | Dikonfirmasi C.11 ditandai eksplisit "DIGANTIKAN bagian D.2 (REVISI 5)" pada berkas blueprint; model yang ditulis task ini mengikuti D.2, bukan C.11 | PASS (review) | Baca ulang berkas blueprint sebelum menulis model |
  | Review relasi satu-ke-satu `FinSupplierReturn` ↔ `FinSupplierReturnDeposit` | `HasOne(x => x.SourceReturn).WithOne(x => x.Deposit).HasForeignKey<FinSupplierReturnDeposit>(x => x.SourceReturnId)` — navigasi berlawanan cocok (`FinSupplierReturn.Deposit`, `FinSupplierReturnDeposit.SourceReturn`) | PASS (review) | Baca ulang kedua model + configuration |
  | Review `FinSupplierPayableConfiguration.cs` pasca-edit | Baris lama (`PayableNumber`, unique index existing, dll.) tidak berubah; hanya sisipan baru untuk `SourcePurchasingInvoiceId` | PASS (review) | Diff manual — tidak ada baris existing yang terhapus |
  | Konfirmasi `FinPayment` (bukan `FinPaymentAllocation`) sebagai target FK `PaymentId` | Benar — `FinSupplierReturnDepositUsage.PaymentId` menunjuk `FinPayment`, aggregate root, sesuai `02-backend-architecture.md` D.2 baris "PaymentId — FK ke FinPayment" | PASS (review) | Baca ulang blueprint bagian D.2 sebelum menulis model |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED.**

- WARNINGS: Task ini bertumpuk di atas `BE-FIN-029` yang **juga** belum ter-`build`. Bila `BE-FIN-029` ternyata punya kesalahan kompilasi, task ini otomatis terdampak karena mereferensikan `FinPurchasingInvoice`. Disarankan `dotnet build` dijalankan mencakup ketiganya (`BE-FIN-028`, `029`, `030`) sekaligus.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — validasi tertunda milik pengguna.
  2. Migration belum dibuat (`BE-FIN-031`) — empat tabel baru dan satu kolom baru tidak dapat dipakai sampai migration dibuat dan diterapkan, keduanya menunggu otorisasi terpisah.
  3. `ReturnNumber`, seperti nomor bisnis lain pada rumpun ini, belum punya service alokasinya — menyusul `BE-FIN-035` lewat provider number-series atomik (`QBE-CODE-001..006`), bukan `Count+1`.
  4. Mekanisme **pemakaian** deposit (`ReserveAsync`/`ReleaseAsync`/`MarkAppliedAsync` pada `FinanceSupplierReturnService`, dipanggil `FinancePaymentService`) belum ditulis — task ini hanya menyiapkan tabel dan kolomnya (`FinDES-045`/`046`), logikanya cakupan `BE-FIN-036`.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses dan migration diterapkan lebih dulu.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs
  M  Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs
  M  Repositories/ApplicationDbContext.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturn.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnItem.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnDeposit.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Models/FinSupplierReturnDepositUsage.cs
  ?? Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnConfiguration.cs
  ?? Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnItemConfiguration.cs
  ?? Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnDepositConfiguration.cs
  ?? Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinSupplierReturnDepositUsageConfiguration.cs
  ```

  (Perubahan lain pada `git status --short` keseluruhan repository — `FinancePaymentsController.cs`/`FinPayment.cs`/`FinancePaymentService.cs`/`FinanceApprovalTierResolver.cs` dari `BE-FIN-028`, tujuh berkas `Purchasing` dari `BE-FIN-029` — sudah ada sebelum task ini dimulai; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** mencakup `BE-FIN-028`, `029`, dan `030` sekaligus. Sesudah terkonfirmasi: `BE-FIN-031` (migration `AddPurchasingApRumpun`, mencakup sebelas tabel `BE-FIN-029`/`030` dan kolom `SourcePurchasingInvoiceId`) adalah task berikutnya yang bergantung pada ketiganya, dan membutuhkan otorisasi migration terpisah sebelum dibuat. `BE-FIN-038` dan `041` tetap tidak berprasyarat bila ingin dikerjakan paralel tanpa menunggu `BE-FIN-031`.
