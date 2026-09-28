# BE-FIN-029 — Model dokumen Purchasing: PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice

- TASK ID: BE-FIN-029
- TASK TYPE: NEW CODE — tujuh entity persisted + tujuh configuration pada submodul `Purchasing` yang baru terdaftar
- COMPLEXITY: MEDIUM
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/Models/**`, `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/**`, `Repositories/ApplicationDbContext.cs` (hanya `DbSet` + `using` baru)

- BACKEND GOVERNANCE PREFLIGHT:
  - Area: `Corporate / Finance`
  - Module: `FinanceManagement`
  - Submodule: `Purchasing` (Pembelian) — terdaftar `docs/engineering/MODULE_OWNERSHIP_PREFIX_REGISTRY.md` oleh `BE-FIN-027` (26 September 2026), lifecycle `ACTIVE`
  - Owner/prefix registry: `Fin` — sudah `ACTIVE` (baris `FinanceManagement / Finance` sejak awal, dipertegas untuk `Purchasing` oleh `BE-FIN-027`)
  - Keberlakuan: `NEW CODE` (tujuh entity operasional pertama pada submodul ini)
  - Status registry: terpenuhi — `QBE-MOD-002`/`QBE-MOD-003` tidak lagi `BLOCKED` untuk `Purchasing` sejak `BE-FIN-027`
  - QBE ID yang berlaku: `QBE-ENT-001` (`IdentityModel`), `QBE-ENT-002` (semantik field), `QBE-NAM-001`/`QBE-NAM-002` (prefix `Fin`, bukan `Trx*`), `QBE-NAM-004` (prefix bukan karangan — dipakai apa adanya dari registry), `QBE-CFG-001` (`IEntityTypeConfiguration<T>` per entity), `QBE-MOD-001` (folder di bawah `Areas/Corporate/FinanceManagement/Purchasing/`), `QBE-CODE-003`/`QBE-CODE-004` (nomor bisnis — **lihat KNOWN ISSUES**, alokasi nomor belum dibangun di task ini)

- FILES INSPECTED:
  - `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs`, `FinSupplierPayableItem.cs` — pola model aggregate root + line item, `IdentityModel`, status sebagai `string` + `static class`
  - `Repositories/Configurations/Corporate/FinanceManagement/Payable/FinSupplierPayableConfiguration.cs` — pola `IEntityTypeConfiguration<T>`, check constraint via `ToTable(..., table => ...)`, konfigurasi kolom audit eksplisit per entity, `HasIndex` dengan filter parsial `IsDelete = false`
  - `Repositories/ApplicationDbContext.cs` — lokasi `using` dan `DbSet` submodul `Payable`/`Receivable`, konfirmasi `builder.ApplyConfigurationsFromAssembly(...)` (baris 1074-an) sehingga configuration baru otomatis terpasang tanpa registrasi manual tambahan
  - `Areas/Administrator/MasterData/Models/MstSupplier.cs` — `PaymentTermDays` sudah ada (`FIN-CAP-027`), dirujuk sebagai FK read-only
  - `docs/module-blueprints/finance-management/erd/data-dictionary.md` bagian C.1-C.7, C.16 (bentuk kolom dan DDL, belum dikoreksi D.2/D.3 — kedua tabel yang dikoreksi revisi 5 **bukan** bagian task ini) dan bagian D (AMENDMENT REVISI 5, dikonfirmasi tidak menyentuh ketujuh tabel task ini)
  - `docs/module-blueprints/finance-management/02-backend-architecture.md` bagian C.1-C.7 (Keputusan arsitektur), C.11 (Yang sengaja tidak dibuat — dasar keputusan `ProductCategory`/`ProductName` teks bebas dan `PurchaseOrderId` wajib pada GR)
  - `docs/module-blueprints/finance-management/roadmap/01-backend-roadmap.md` baris `BE-FIN-029`
  - Pencarian nama class dan nama property `DbSet` di seluruh repository untuk memastikan nol tabrakan nama

- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrderItem.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceipt.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinGoodsReceiptItem.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinInvoiceExchange.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingInvoice.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchasingInvoiceItem.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchaseOrderConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchaseOrderItemConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinGoodsReceiptConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinGoodsReceiptItemConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinInvoiceExchangeConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingInvoiceConfiguration.cs`
  - **Baru** `Repositories/Configurations/Corporate/FinanceManagement/Purchasing/FinPurchasingInvoiceItemConfiguration.cs`
  - `Repositories/ApplicationDbContext.cs` — satu `using` baru + tujuh `DbSet<T>` baru, ditempatkan setelah blok `FinMedicalServicePayable*` mengikuti urutan submodul yang sudah ada; **tidak ada baris lama yang diubah atau dipindah**

- IMPLEMENTATION: Tujuh entity persisted pertama pada submodul `Purchasing`, mengikuti bentuk kolom persis `erd/data-dictionary.md` bagian C.1-C.7 dan pola kode `FinSupplierPayable`/`FinSupplierPayableItem` yang sudah berjalan (aggregate root ber-`RowVersion`, line item tanpa `RowVersion`, status sebagai `string` + `static class …Statuses`, bukan enum `int`).

  Urutan alur: `FinPurchaseOrder` (+ `Item`) → `FinGoodsReceipt` (+ `Item`, **wajib** menunjuk satu PO — `FIN-DES-037`, keputusan `DEV_DISCRETION` yang dicatat eksplisit di `02-backend-architecture.md` C.11 karena tidak ada keputusan yang meminta GR tanpa PO) → `FinInvoiceExchange` (PO dan GR **nullable**, `SetNull` — boleh berdiri sendiri per `FIN-DEC-051`) → `FinPurchasingInvoice` (+ `Item`, relasi satu-ke-satu dengan `FinInvoiceExchange` lewat `HasOne().WithOne().HasForeignKey<FinPurchasingInvoice>()`, menegakkan "tepat satu Purchasing Invoice per Tukar Faktur").

  `FinPurchaseOrder.ApprovalTier` dan `FinPurchasingInvoice.ApprovalTier` disiapkan sebagai kolom `string(10)` yang **diisi service** (`BE-FIN-032`/`034` memanggil `FinanceApprovalTierResolver.Resolve` dari `BE-FIN-028`) — task ini hanya menyediakan kolom dan check constraint `IN ('TIER_1','TIER_2')`, tidak menulis logika pengisiannya.

  `FinPurchasingInvoice.PPNAmount` disiapkan sebagai kolom biasa; kejadian akuntansi `PPN-MASUKAN-PEMBELIAN` yang memakainya adalah cakupan `BE-FIN-034`, bukan task ini.

  Seluruh tujuh configuration mengonfigurasi kolom audit (`CreateDateTime`, `UpdateDateTime`, `DeleteDateTime`, `CancelDateTime`, `IsDelete`, `IsCancel`) secara eksplisit per entity, mengikuti pola `FinSupplierPayableConfiguration` — **bukan** konfigurasi terpusat, karena itu bukan pola yang dipakai submodul Finance manapun saat ini.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Nol. Task ini murni model + configuration; nol controller, nol DTO, nol endpoint. `FIN-API-1.1`/`1.2` grup B.1-B.4 (`Purchasing / Purchase Order`, dst.) tetap **Rencana (belum tersedia)** — dibangun `BE-FIN-032`, `033`, `034`.
- DATABASE IMPACT: **Skema disiapkan, belum diterapkan.** Tujuh tabel baru sesuai `erd/data-dictionary.md` C.1-C.7 dan DDL C.16 (bagian yang tidak dikoreksi AMENDMENT REVISI 5 — `FinSupplierReturnDepositUsage`/`FinReceiptDeduction` yang dikoreksi bukan bagian task ini). **Migration belum dibuat** — itu cakupan `BE-FIN-031`, yang membutuhkan otorisasi terpisah sesuai `AGENTS.md` bagian Keselamatan Database. Nol tabel existing tersentuh; `MstSupplier` dan `FinSupplierPayable` hanya dirujuk sebagai FK, kolomnya tidak diubah.
- SECURITY IMPACT: Nol pada task ini. Belum ada controller/endpoint, sehingga belum ada `[AccessPermission]` yang perlu ditegakkan — itu cakupan `BE-FIN-032`/`034`.
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna ("tampa build automatis") — pengguna akan menjalankan sendiri. **Bukan klaim PASS** |
  | Pencarian nama class tujuh entity baru di seluruh repository | Masing-masing tepat satu definisi, di file yang dimaksud | PASS (review) | `grep -rl "class Fin…\b"` |
  | Pencarian nama property `DbSet` tujuh entity baru | Masing-masing tepat satu, di `ApplicationDbContext.cs` yang baru diedit | PASS (review) | `grep` pasca-edit |
  | Review manual bentuk kolom terhadap `data-dictionary.md` C.1-C.7 | Nama, tipe, wajib/opsional, `MaxLength`, `HasPrecision(18,2)`, default value, dan relasi FK cocok baris demi baris | PASS (review) | Baca ulang ketujuh model dan ketujuh configuration pasca-tulis |
  | Review `DeleteBehavior` per relasi terhadap desain | `FinGoodsReceipt.PurchaseOrderId` → `Restrict` (wajib); `FinInvoiceExchange.PurchaseOrderId`/`GoodsReceiptId` → `SetNull` (opsional); `FinPurchasingInvoice.InvoiceExchangeId` → `Restrict` + relasi satu-ke-satu; sisanya → `Restrict` (default pola submodul lain) | PASS (review) | Sesuai `02-backend-architecture.md` C.1-C.2 |
  | Review check constraint terhadap daftar status di model | `CK_FinPurchaseOrder_Status` (8 nilai), `CK_FinPurchaseOrder_ApprovalTier`, `CK_FinGoodsReceipt_Status`, `CK_FinInvoiceExchange_Status`, `CK_FinPurchasingInvoice_Status` (5 nilai), `CK_FinPurchasingInvoice_ApprovalTier` — seluruhnya cocok persis dengan `…Statuses`/`ApprovalTiers` di model | PASS (review) | Baca silang model ↔ configuration |
  | Konfirmasi `builder.ApplyConfigurationsFromAssembly` mencakup namespace baru | Ya — assembly-wide scan, tidak ada whitelist namespace | PASS (review) | `Repositories/ApplicationDbContext.cs` baris ~1074-an (sebelum penyisipan `DbSet` baru) |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED.**

- WARNINGS: Task ini **belum dikonfirmasi compile**. Risiko yang paling mungkin bila ada kesalahan: nama property navigasi yang salah eja pada salah satu sisi relasi (mis. `WithMany(x => x.Items)` vs `WithMany(x => x.GoodsReceipts)`) — sudah diperiksa manual dua arah untuk seluruh enam relasi FK, tetapi compiler adalah pemeriksa yang sebenarnya untuk hal ini.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — validasi tertunda milik pengguna, sesuai instruksi eksplisit. **Task ini MUST NOT dianggap selesai penuh sampai dikonfirmasi.**
  2. **Migration belum dibuat** (`BE-FIN-031`) — ketujuh tabel ini tidak dapat dipakai sampai migration `AddPurchasingApRumpun` dibuat dan dijalankan, keduanya menunggu otorisasi terpisah.
  3. **Alokasi nomor bisnis** (`PONumber`, `GRNumber`, `ExchangeNumber`, `InvoiceNumber`) belum punya service pembuatnya — task ini hanya menyiapkan kolom `[Required, MaxLength]` dan unique index parsial. Sesuai `QBE-CODE-001`..`006`, alokasinya MUST lewat provider number-series PostgreSQL atomik saat service ditulis (`BE-FIN-032`, `033`, `034`), **bukan** `Count+1`/`Max+1` — dicatat di sini supaya tidak diasumsikan sudah selesai.
  4. `FinGoodsReceiptItem` **tidak** membawa `RowVersion` sendiri (mengikuti pola `FinSupplierPayableItem` — line item tanpa concurrency token sendiri, dinaungi `RowVersion` induknya). Ini konsisten dengan pola existing, dicatat supaya tidak dianggap kelalaian.
- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses dan migration diterapkan lebih dulu, keduanya di luar cakupan/otorisasi task ini.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Repositories/ApplicationDbContext.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/
  ?? Repositories/Configurations/Corporate/FinanceManagement/Purchasing/
  ```

  (Perubahan lain pada `git status --short` keseluruhan repository sudah ada sebelum task ini dimulai — dari `BE-FIN-027`, `BE-FIN-028`, dan pass desain sebelumnya; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** untuk memverifikasi `BE-FIN-028` dan `BE-FIN-029` bersamaan (keduanya belum pernah dikompilasi sejak diedit). Sesudah itu: `BE-FIN-030` (model Retur Pembelian, Deposit Retur, kolom `SourcePurchasingInvoiceId` pada `FinSupplierPayable` — bentuk revisi 5) menjadi task berikutnya yang bergantung pada `BE-FIN-029`; `BE-FIN-038` dan `BE-FIN-041` tetap tidak berprasyarat bila ingin dikerjakan paralel.
