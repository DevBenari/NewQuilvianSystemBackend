# BE-FIN-034 — Purchasing Invoice: pengakuan utang supplier dan kejadian PPN Masukan

- TASK ID: BE-FIN-034
- TASK TYPE: NEW CODE (service/controller/DTO submodul `Purchasing`) + **TOUCHED LEGACY sensitif** (`FinanceSupplierPayableService`, `BE-FIN-019`, sudah berjalan) + aditif kecil (`FinAccountingEventTypeCodes`, satu konstanta baru)
- COMPLEXITY: HIGH (satu transaksi lintas tiga entity + satu service existing yang harus tetap identik perilakunya untuk jalur lama)
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/{Services,Controllers,Dtos}/**` (baru); `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` (dua parameter opsional ditambah di akhir); `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` (satu konstanta baru); registrasi DI

- BACKEND GOVERNANCE PREFLIGHT:
  - Area/Module/Submodule: `Corporate / Finance` / `FinanceManagement` / `Purchasing` (`ACTIVE`) untuk kode baru; `Payable` (`ACTIVE`, sudah lama) untuk `FinanceSupplierPayableService`; `AccountingIntegration` (`ACTIVE`, sudah lama) untuk konstanta kode kejadian
  - Prefix: `Fin` — dipakai apa adanya
  - Keberlakuan: `NEW CODE` untuk `FinancePurchasingInvoiceService`/Controller/DTO; `TOUCHED LEGACY` untuk `FinanceSupplierPayableService.CreateAsync` (dua parameter opsional ditambah di **akhir** daftar, bukan disisipkan — satu-satunya caller existing, `FinanceSupplierPayablesController`, terverifikasi tidak perlu diubah); aditif murni untuk `FinAccountingEventTypeCodes` (satu `public const string` baru, nol konstanta lama diubah)
  - Status registry: terpenuhi, tidak `BLOCKED`
  - QBE ID yang berlaku: `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-MOD-001` untuk kode baru; `QBE-CFG-002` (SHOULD, perbaikan aman dalam cakupan) untuk perluasan `FinanceSupplierPayableService.CreateAsync`; `QBE-CODE-001..006` **belum terpenuhi** untuk `InvoiceNumber` (gap yang sama seperti `PONumber`/`GRNumber`/`ExchangeNumber`)

- FILES INSPECTED:
  - `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.4 (endpoint Purchasing Invoice — **nol endpoint cancel terdaftar**, lihat catatan pada FinancePurchasingInvoicesController) dan §B.9 (gerbang PPN Masukan — "bukan endpoint terpisah, gerbangnya di level worker")
  - `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §B.4 — `[AccessPermission]` persis lima endpoint yang dibangun (Read, Create, Update, Submit, Approve, Reject — Approve/Reject berbagi action `Approve`)
  - `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §B.4 — seluruh transisi, termasuk baris kunci "APPROVED — (efek samping) — Sistem — Membuat FinSupplierPayable; Tukar Faktur → LINKED_TO_INVOICE; outbox PPN-MASUKAN-PEMBELIAN PENDING — **Seluruhnya satu transaksi, bila gagal status tetap PENDING_APPROVAL**"
  - `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-105`..`109`, `122` beserta catatan eksplisit "`FIN-VAL-122` bukan aturan yang menolak permintaan pengguna — Purchasing Invoice tetap bisa disetujui dan baris outbox tetap ditulis. Yang dicegah hanyalah pengiriman"
  - `docs/module-blueprints/finance-management/contracts/integration-contract.md` §5.8 — **"Amount pada kejadian ini adalah nilai PPN-nya saja, bukan nilai invoice penuh"**; nilai pokok "dicatat lewat kejadian pengakuan utang supplier terpisah" (mengonfirmasi `AP_CREATED` yang sudah ada, bukan kode baru, adalah kejadian pokoknya)
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` (dibaca penuh, 599 baris SEBELUM diedit) — `CreateAsync` menghitung `OriginalAmount` dari jumlah item (`Quantity × UnitPrice`), menulis `AP_CREATED` sebesar `payable.OriginalAmount`, **tidak pernah mengisi `SourcePurchasingInvoiceId`** pada kode sebelum task ini walau kolomnya sudah ada sejak `BE-FIN-030`
  - **Pencarian SELURUH caller `FinanceSupplierPayableService.CreateAsync`** — tepat satu: `FinanceSupplierPayablesController.cs` baris 57, tujuh argumen posisional persis signature lama. Dikonfirmasi: menambah parameter baru di **akhir** (setelah `cancellationToken`, bukan lazim tapi paling aman) dengan nilai bawaan `null` tidak mengubah pemanggilan itu sama sekali
  - `Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceAccountingOutboxService.cs` (dibaca penuh) — `StageEventAsync` TIDAK membuka/commit transaksi sendiri (pemanggil WAJIB sudah di dalam transaksinya sendiri); `DeliveryStatus` bawaan `Pending` kecuali `RequiresFinalization=true` diminta eksplisit; `EventTypeCode` diterima apa adanya (nol validasi terhadap katalog tertutup)
  - `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` — katalog `FinAccountingEventTypeCodes` (17 kode `FIN-DEC-002` + 5 alias V2) dikonfirmasi **belum memuat** kode manapun dari AMENDMENT REVISI 4/5 (kode 25-29) — seluruhnya baru diusulkan lewat evidence letter, belum pernah ditulis ke source sampai task ini
  - `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-032.md`/`033.md` — pola exception/service/controller/DTO submodul yang sama

- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs` — `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `SubmitAsync`, `ApproveAsync` (transaksi utama), `RejectAsync`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs`
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs` — `CreateAsync` bertambah dua parameter opsional di akhir (`Guid? sourcePurchasingInvoiceId = null`, `decimal? accountingEventAmountOverride = null`); payable baru mengisi `SourcePurchasingInvoiceId`; event `AP_CREATED` memakai `accountingEventAmountOverride ?? payable.OriginalAmount`. **Nol baris lain diubah** — `OriginalAmount`/`OutstandingAmount`/duplicate-check/seluruh method lain persis seperti sebelumnya
  - `Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs` — satu `public const string PpnMasukanPembelian = "PPN-MASUKAN-PEMBELIAN";` ditambahkan ke `FinAccountingEventTypeCodes`; **nol konstanta lama diubah**
  - `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — satu `services.AddScoped<FinancePurchasingInvoiceService>()` disisipkan; **tidak ada baris lain yang diubah**

- IMPLEMENTATION:

  **Perluasan `FinanceSupplierPayableService.CreateAsync` — bagian paling berisiko task ini.** Dibaca penuh dulu (599 baris) sebelum satu baris pun diubah. Ditemukan: (a) hanya SATU caller di seluruh repository (`FinanceSupplierPayablesController.cs`, tujuh argumen posisional); (b) `OriginalAmount` dihitung dari jumlah `Amount` seluruh `SupplierPayableItemRequest` — invariant yang secara eksplisit didokumentasikan dalam komentar kelasnya sendiri ("supaya invariant selalu benar dengan sendirinya, bukan divalidasi lalu ditolak"). Dua parameter baru ditambahkan di **akhir** daftar parameter (setelah `cancellationToken`, bukan konvensi lazim tapi paling aman untuk pemanggilan posisional existing) dengan nilai bawaan `null` — pemanggilan lama dari `FinanceSupplierPayablesController` **tidak disentuh sama sekali** dan perilakunya identik: `sourcePurchasingInvoiceId` tetap `null` (kolom `FinSupplierPayable.SourcePurchasingInvoiceId` sudah nullable sejak `BE-FIN-030`), `accountingEventAmountOverride` tetap `null` sehingga `AP_CREATED` tetap ditulis sebesar `payable.OriginalAmount` PENUH — **nol regresi pada jalur input manual**.

  **Kenapa satu item sintetis, bukan menyalin baris invoice apa adanya.** `FinPurchasingInvoice.TotalAmount` adalah hasil `Subtotal − Discount + PPN − DownPayment − OtherDeduction` (`FIN-VAL-107`), sedangkan `FinanceSupplierPayableService.CreateAsync` menghitung `OriginalAmount` HANYA dari jumlah baris item yang dikirim — bila baris `FinPurchasingInvoiceItem` apa adanya yang dikirim, hasilnya adalah `SubtotalAmount` (sebelum diskon/PPN/DP/potongan lain), BUKAN `TotalAmount` yang sebenarnya harus dibayar. Menyalin baris apa adanya akan menghasilkan ledger utang yang salah nilainya bagi invoice manapun yang punya diskon/PPN/DP. Solusinya: SATU baris `SupplierPayableItemRequest` sintetis (`Quantity=1`, `UnitPrice=TotalAmount`, `Description="Purchasing Invoice {InvoiceNumber}"`) — `OriginalAmount` hasilnya TEPAT `TotalAmount`, invariant "jumlah item = OriginalAmount induk" milik service itu tetap benar dengan sendirinya, tanpa mengubah satu baris pun logika internalnya. Rincian per-baris asli tetap dapat dilihat lewat `GET /purchasing-invoices/{id}` (endpoint controller ini sendiri) — tidak hilang, hanya tidak diduplikasi ke ledger `FinSupplierPayable`.

  **Dua kejadian, nol PPN dobel kredit (titik paling penting task ini).** `ApproveAsync` memanggil `_supplierPayableService.CreateAsync(..., accountingEventAmountOverride: TotalAmount − PPNAmount)` — `AP_CREATED` yang dihasilkan bernilai pokok TANPA PPN. Bila `PPNAmount > 0`, satu event `PPN-MASUKAN-PEMBELIAN` DITULIS TERPISAH sebesar `PPNAmount` saja (`integration-contract.md` §5.8: "Amount pada kejadian ini adalah nilai PPN-nya saja"). Jumlah keduanya = `TotalAmount` persis — memenuhi bukti acceptance criteria roadmap ("dua baris untuk satu invoice ber-PPN, jumlah keduanya = TotalAmount"). Bila `PPNAmount = 0`, event kedua **dilewati** — keputusan implementasi (kontrak tidak eksplisit membahas kasus ini), alasannya kejadian bernilai nol bukan fakta akuntansi yang perlu diakui.

  **Satu transaksi (state-transition-matrix.md §B.4).** `ApproveAsync` membuka transaksi `Serializable` + advisory lock per Purchasing Invoice SEBELUM memanggil `_supplierPayableService.CreateAsync` — karena `CreateAsync` memanggil `SaveChangesAsync`-nya sendiri TANPA membuka transaksi sendiri (dikonfirmasi dari pembacaan penuh), panggilan itu otomatis ikut serta dalam transaksi ambient milik `ApproveAsync` selama `ApplicationDbContext`-nya sama (dijamin — keduanya `Scoped` dalam DI scope permintaan yang sama). Sesudahnya, `ApproveAsync` sendiri menulis event PPN (bila ada), mengubah `FinInvoiceExchange.Status` → `LINKED_TO_INVOICE`, mengubah `FinPurchasingInvoice.Status` → `APPROVED`, lalu SATU `SaveChangesAsync` terakhir dan commit. **Exception apa pun di mana pun dalam urutan ini** (termasuk dari dalam `CreateAsync` — `PayableConflictException`/`PayableValidationException`/`PayableBadRequestException`) memicu rollback penuh lewat blok `catch` umum — `FinPurchasingInvoice.Status` **tetap** `PENDING_APPROVAL`, persis seperti yang dituntut kontrak.

  **Gerbang PPN Masukan (`FIN-VAL-122`, `FIN-DEC-056`) — dikonfirmasi tidak menghalangi task ini.** Baris outbox `PPN-MASUKAN-PEMBELIAN` tetap ditulis normal (`DeliveryStatus = PENDING`, bukan `HELD_FOR_FINALIZATION` — `RequiresFinalization` tidak di-set). Gerbangnya murni di level WORKER PENGIRIMAN, yang belum pernah dibangun sama sekali (`EPIC FIN-12`) — sehingga secara alami belum ada yang mengirim kode ini ke Accounting sampai worker itu dibangun DAN eksplisit memeriksa ratifikasi `FIN-OQ-020`, persis pola `FIN-DES-029` untuk kode lain yang pernah menunggu.

  **Cancel sengaja tidak dibangun.** `api-contract.md` §B.4 tidak mendaftarkan endpoint cancel untuk Purchasing Invoice sama sekali (berbeda dari PO/GR/Tukar Faktur yang semuanya punya), walau `state-transition-matrix.md` §B.4 mengizinkan transisi `DRAFT`/`PENDING_APPROVAL` → `CANCELLED`. Acceptance criteria roadmap `BE-FIN-034` juga tidak menyebutnya. Dicatat sebagai kesenjangan dokumen kontrak (bukan `api-contract.md` yang keliru per se — mungkin memang belum diberi endpoint), bukan ditambah sepihak.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Satu controller baru sesuai `FIN-API-1.1` §B.4, **kecuali** `GET /` (daftar) dan `cancel` (lihat IMPLEMENTATION) — keduanya gap terdokumentasi, bukan dikarang.
- DATABASE IMPACT: **Nol.** `FinPurchasingInvoice`/`Item` sudah ada dari `BE-FIN-029`/`031`. `FinSupplierPayable.SourcePurchasingInvoiceId` sudah ada dari `BE-FIN-030`/`031` — task ini yang PERTAMA KALI benar-benar MENGISI kolom itu, tapi nol perubahan skema.
- SECURITY IMPACT: Nol mekanisme otorisasi baru — memakai `FinanceApprovalAuthorizationService` (`BE-FIN-032`) apa adanya, ambang sama dengan PO.
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna. `BE-FIN-028`..`033` terkonfirmasi build sukses (497.3s, 229 warning, 0 error) oleh pengguna sebelum task ini dimulai — task ini menyentuh source BARU yang belum ikut diverifikasi |
  | Satu-satunya caller `FinanceSupplierPayableService.CreateAsync` tidak perlu diubah | `FinanceSupplierPayablesController.cs` baris 57 tetap tujuh argumen posisional, cocok signature baru (dua parameter baru di akhir, bernilai bawaan) | PASS (review) | `grep -rn` seluruh repository sebelum dan sesudah edit |
  | Jalur input manual `FinanceSupplierPayableService` tidak berubah perilaku | `sourcePurchasingInvoiceId`/`accountingEventAmountOverride` default `null` → `SourcePurchasingInvoiceId` tetap `null`, `AP_CREATED` tetap `payable.OriginalAmount` penuh, identik sebelum task ini | PASS (review) | Baca ulang diff `CreateAsync` baris per baris |
  | FIN-VAL-105 (Tukar Faktur sudah punya invoice → 409) | `CreateAsync` query eksplisit `FinPurchasingInvoices.AnyAsync(InvoiceExchangeId == ...)` sebelum insert | PASS (review) | Baca ulang `CreateAsync` |
  | FIN-VAL-106 (Tukar Faktur bukan RECEIVED → 422) | Dicek di `CreateAsync` (create-time) DAN diperiksa ulang di dalam transaksi `ApproveAsync` (defense-in-depth) | PASS (review) | Baca ulang kedua titik |
  | FIN-VAL-107 (rincian tidak seimbang → 422) | `ValidateBalance` membandingkan `Subtotal − Discount + PPN − DownPayment − OtherDeduction` vs `TotalAmount`, dibulatkan 2 desimal, dipanggil di `CreateAsync` DAN `UpdateAsync` | PASS (review) | Baca ulang `ValidateBalance` dan kedua titik pemanggilannya |
  | FIN-VAL-108 (self-approval → 422), FIN-VAL-109 (jenjang → 403) | `ApproveAsync` memanggil urutan self-check lalu `FinanceApprovalAuthorizationService.CanApproveAsync` — pola identik `FinancePurchaseOrderService.ApproveAsync` (`BE-FIN-032`), dipakai ulang bukan ditulis ulang | PASS (review) | Baca ulang `ApproveAsync` berdampingan dengan versi PO |
  | Satu transaksi mencakup payable + outbox PPN + status Tukar Faktur + status invoice | Seluruhnya dalam satu `BeginTransactionAsync(Serializable)` ... `CommitAsync`, satu blok `catch` umum me-rollback semuanya | PASS (review) | Baca ulang alur `ApproveAsync` menyeluruh |
  | Dua kejadian jumlahnya = `TotalAmount` untuk invoice ber-PPN | `AP_CREATED` = `TotalAmount − PPNAmount`; `PPN-MASUKAN-PEMBELIAN` = `PPNAmount` (dilewati bila nol) | PASS (review, ARITMETIKA) | Baca ulang kedua nilai yang dikirim ke `StageEventAsync`/`CreateAsync` |
  | `FIN-VAL-122`/gerbang worker tidak terhalang penulisan outbox | `DeliveryStatus` bawaan `Pending` (bukan `HeldForFinalization`) — dikonfirmasi dari pembacaan `StageEventAsync`, `RequiresFinalization` tidak di-set | PASS (review) | Baca ulang `StageEventAsync` dan pemanggilan di `ApproveAsync` |
  | `SortOrder`/`ModuleCode`/`[AccessPermission]` tidak bentrok controller Finance lain | `SortOrder 45` (tertinggi sebelumnya `44`); string permission cocok `permission-audit-matrix.md` §B.4 persis, TANPA selisih (berbeda dari PO/Tukar Faktur — Purchasing Invoice tidak punya endpoint cancel yang berpotensi selisih) | PASS (review) | `grep` menyeluruh + perbandingan manual |
  | Pencarian nama exception/service/DTO/konstanta baru di seluruh repository | Masing-masing tepat satu definisi | PASS (review) | `grep -rn` pasca-tulis |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED.**

- WARNINGS: Task ini menyentuh `FinanceSupplierPayableService` (`BE-FIN-019`, sudah berjalan lama) — walau perubahan aditif murni dan satu-satunya caller terverifikasi, disarankan `dotnet build` mencakup task ini dijalankan **sebelum** `BE-FIN-035`/`036` (yang juga akan memanggil layanan Purchasing lain) supaya kesalahan kompilasi apa pun pada perluasan ini terdeteksi lebih awal, bukan menumpuk lebih jauh.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan untuk task ini — validasi tertunda milik pengguna.
  2. `InvoiceNumber` masih memakai pola GUID-suffix — gap `QBE-CODE-001..006` yang sama seperti `PONumber`/`GRNumber`/`ExchangeNumber`.
  3. `GET /` (daftar berpaging) dan endpoint `cancel` **tidak dibangun** untuk Purchasing Invoice — lihat IMPLEMENTATION untuk alasan masing-masing (gap kontrak, bukan kelalaian).
  4. `FinPurchasingInvoice` tidak punya kolom `RejectionReason` — pola gap yang sama seperti `FinPurchaseOrder` (`BE-FIN-032`); alasan penolakan tercatat audit log saja.
  5. **Rincian baris asli `FinPurchasingInvoiceItem` TIDAK muncul di `FinSupplierPayableItem`** — ledger utang hanya mencatat satu baris ringkasan (`Quantity=1, UnitPrice=TotalAmount`). Ini keputusan implementasi yang disengaja (lihat IMPLEMENTATION) untuk menjaga `OriginalAmount` benar tanpa mengubah `FinanceSupplierPayableService`; rincian asli tetap dapat dibaca lewat `GET /purchasing-invoices/{id}`, bukan hilang, hanya tidak diduplikasi.
  6. Worker pengiriman kejadian Accounting (termasuk `PPN-MASUKAN-PEMBELIAN`) **belum pernah dibangun** (`EPIC FIN-12`) — `FIN-VAL-122` karenanya belum benar-benar diuji ujung-ke-ujung (tidak ada pengirim untuk diverifikasi melewati baris ini), hanya diverifikasi bahwa PENULISAN baris outbox tidak terhalang apa pun.

- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses dan migration `BE-FIN-031` diterapkan lebih dulu.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Areas/Corporate/FinanceManagement/Payable/Services/FinanceSupplierPayableService.cs
  M  Areas/Corporate/FinanceManagement/AccountingIntegration/Models/FinAccountingEventOutbox.cs
  M  Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchasingInvoiceService.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchasingInvoiceDtos.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchasingInvoicesController.cs
  ```

  (Perubahan lain pada working tree — `BE-FIN-027`..`033` dan seluruh dokumen blueprint AMENDMENT REVISI 4/5 — sudah ada sebelum task ini dimulai; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** mencakup task ini (di atas `BE-FIN-028`..`033` yang sudah terkonfirmasi sukses terpisah). `BE-FIN-035` (Retur Pembelian, bergantung `BE-FIN-034`) adalah task berikutnya di jalur utama; `BE-FIN-038` tetap tidak berprasyarat bila ingin dikerjakan paralel.
