# BE-FIN-033 — Tukar Faktur (checkpoint dokumen faktur supplier)

- TASK ID: BE-FIN-033
- TASK TYPE: NEW CODE (satu service, satu controller, DTO pada submodul `Purchasing`) — nol perubahan model/schema
- COMPLEXITY: LOW (entity tanpa approval workflow, tanpa akumulasi lintas baris — jauh lebih sederhana dari `BE-FIN-032`)
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/{Services,Controllers,Dtos}/**` (baru) + registrasi DI

- BACKEND GOVERNANCE PREFLIGHT:
  - Area/Module/Submodule: `Corporate / Finance` / `FinanceManagement` / `Purchasing` (`ACTIVE` sejak `BE-FIN-027`)
  - Prefix: `Fin` — dipakai apa adanya, nol prefix baru
  - Keberlakuan: `NEW CODE` — entity sumbernya (`FinInvoiceExchange`, `BE-FIN-029`) dipakai apa adanya, **nol perubahan model**
  - Status registry: terpenuhi, tidak `BLOCKED`
  - QBE ID yang berlaku: `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004`, `QBE-MOD-001`; `QBE-CODE-001..006` **belum terpenuhi** untuk `ExchangeNumber` (gap yang sama seperti `PONumber`/`GRNumber`, `BE-FIN-032`)

- FILES INSPECTED:
  - `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.3 — endpoint, request/response, hak akses
  - `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §B.3 (baris `FinanceInvoiceExchange`) — **selisih yang sama seperti `BE-FIN-032`**: endpoint `cancel` ditulis `Update` pada `api-contract.md`, `Cancel` pada `permission-audit-matrix.md`. Task ini mengikuti `permission-audit-matrix.md` (alasan sama seperti `BE-FIN-032`) — pola berulang, kini dikonfirmasi terjadi pada DUA endpoint cancel berturut-turut, kemungkinan besar berlaku pada seluruh endpoint `cancel` submodul ini (`FinancePurchasingInvoice`, `FinanceSupplierReturn` — belum dibangun, dicatat sebagai peringatan untuk task berikutnya)
  - `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §B.3 — seluruh transisi status Tukar Faktur
  - `docs/module-blueprints/finance-management/04-prd-to-mvp.md` `FR-FIN-082` — contoh konkret: GR 1 November, faktur fisik baru sampai 5 November, estimasi jatuh tempo dihitung dari **5 November** (`ReceivedDate` milik Tukar Faktur sendiri), **bukan** dari tanggal PO maupun tanggal barang diterima. Mengonfirmasi `FinGoodsReceipt.ReceivedDate` dan `FinInvoiceExchange.ReceivedDate` adalah dua tanggal yang secara sengaja berbeda makna
  - `Areas/Corporate/FinanceManagement/Purchasing/Models/FinInvoiceExchange.cs` (`BE-FIN-029`, dibaca ulang) — `PurchaseOrderId`/`GoodsReceiptId` nullable, `Status` (`RECEIVED`/`LINKED_TO_INVOICE`/`CANCELLED`), nol kolom `ApprovalTier`/`RequestedByUserId` (dikonfirmasi: entity ini memang tidak dirancang punya approval workflow — bukan kelalaian)
  - `Areas/Administrator/MasterData/Models/MstSupplier.cs` — `PaymentTermDays` (`int`, default `0`)
  - `docs/module-blueprints/finance-management/task/report/backend/BE-FIN-032.md` — laporan task terakhir submodul yang sama; pola exception (`PurchasingExceptions.cs`, dipakai ulang apa adanya, **nol** exception baru ditambahkan), pola service/controller/DTO, dan KNOWN ISSUES yang berlaku lintas task (`QBE-CODE`, `GET /` daftar belum ada)

- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs` — `GetByIdAsync`, `CreateAsync`, `CancelAsync`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceInvoiceExchangeDtos.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceInvoiceExchangesController.cs` — rincian, create, cancel
  - `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — satu `services.AddScoped<FinanceInvoiceExchangeService>()` disisipkan setelah `FinanceGoodsReceiptService`; **tidak ada baris lain yang diubah**

- IMPLEMENTATION:

  **PO/GR opsional (FIN-DEC-051).** `CreateAsync` menerima `purchaseOrderId`/`goodsReceiptId` sebagai `Guid?` — bila `null`, Tukar Faktur tetap tersimpan berdiri sendiri (tidak ada validasi "salah satu wajib diisi"). Bila diisi, hanya diverifikasi ADA (existence check, bukan gerbang status) — kontrak tidak menuntut PO/GR pada status tertentu untuk direferensikan dari sini, jadi tidak ditambahkan gerbang yang tidak diminta. Satu pengaman konsistensi data DITAMBAHKAN (bukan dari kontrak eksplisit, murni integritas dasar): bila KEDUANYA diisi, `GoodsReceipt.PurchaseOrderId` harus sama dengan `PurchaseOrderId` yang dirujuk — mencegah kombinasi PO+GR yang secara data mustahil valid (`400`, bukan `422`, karena ini kesalahan bentuk request bukan aturan bisnis).

  **EstimatedDueDate (FR-FIN-082, titik paling penting task ini).** Dihitung `ReceivedDate.AddDays(supplier.PaymentTermDays)` — `ReceivedDate` di sini SELALU milik Tukar Faktur yang sedang dibuat, bukan `FinGoodsReceipt.ReceivedDate` (dua kolom bernama sama pada dua entity berbeda, makna berbeda, dikonfirmasi lewat contoh `FR-FIN-082`: barang diterima 1 November, faktur fisik baru sampai 5 November, estimasi dihitung dari 5 November). Cara memastikan nilai kiriman client benar-benar diabaikan bukan lewat "menerima lalu menimpa" — `CreateInvoiceExchangeRequest` **sengaja tidak punya field `EstimatedDueDate` sama sekali**, sehingga tidak ada jalur sama sekali bagi client mengirim nilai ini walau mencoba.

  **Pembatalan.** Hanya dari `RECEIVED`. Pesan ditolak dikhususkan untuk `LINKED_TO_INVOICE` (acceptance criteria eksplisit `BE-FIN-033`: "Tukar Faktur yang sudah `LINKED_TO_INVOICE` tidak dapat dibatalkan") terpisah dari pesan generik status lain, supaya pengguna tahu PERSIS alasannya beda dari sekadar "status salah". Transisi `RECEIVED → LINKED_TO_INVOICE` sendiri adalah **milik `BE-FIN-034`** (dilakukan sistem saat Purchasing Invoice berhasil dibuat) — **tidak dibangun di sini**, konsisten dengan `state-transition-matrix.md` §B.3 yang menulis "Sistem" sebagai pelaku transisi itu, bukan endpoint yang dipanggil user.

  **Nol approval workflow.** Berbeda dari `FinPurchaseOrder`/`FinPurchasingInvoice`, `FinInvoiceExchange` tidak punya `ApprovalTier`/`RequestedByUserId`/`ApprovedByUserId` — dikonfirmasi ini memang desain entity-nya (`BE-FIN-029`), bukan sesuatu yang perlu ditambahkan task ini. `FinanceApprovalTierResolver`/`FinanceApprovalAuthorizationService` (`BE-FIN-028`/`032`) **tidak dipakai** di sini — task ini murni pencatatan checkpoint dan pembatalan sederhana.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Satu controller baru sesuai `FIN-API-1.1` §B.3, **kecuali** `GET /` (daftar berpaging) — gap yang sama seperti `FinancePurchaseOrdersController`/`FinanceGoodsReceiptsController` (`BE-FIN-032`), dicatat bukan dikarang.
- DATABASE IMPACT: **Nol.** `FinInvoiceExchange` sudah ada dari `BE-FIN-029`/`031`. Nol kolom, nol tabel, nol migration baru.
- SECURITY IMPACT: Nol perubahan mekanisme otorisasi baru — memakai `[AccessPermission]` biner yang sudah ada, tanpa gerbang jenjang nominal (entity ini memang tidak punya nilai uang untuk dijenjangkan).
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna. Task ini bertumpuk di atas `BE-FIN-028`..`032` yang juga belum terverifikasi compiler |
  | FIN-DEC-051 (PO/GR boleh kosong) | `CreateAsync` tidak mensyaratkan `purchaseOrderId`/`goodsReceiptId` diisi | PASS (review) | Baca ulang signature dan validasi `CreateAsync` |
  | FR-FIN-082 (`EstimatedDueDate` dari `ReceivedDate` Tukar Faktur + `PaymentTermDays`, bukan dari PO/GR) | `estimatedDueDate = receivedDate.AddDays(supplier.PaymentTermDays)` — `receivedDate` adalah parameter dari request Tukar Faktur ini sendiri, tidak pernah membaca `FinGoodsReceipt.ReceivedDate` | PASS (review) | Baca ulang `CreateAsync`, dicocokkan dengan contoh `FR-FIN-082` (1 & 5 November) |
  | Nilai `EstimatedDueDate` dari client diabaikan | `CreateInvoiceExchangeRequest` tidak punya field ini sama sekali — dicek lewat definisi DTO, bukan lewat logika "terima lalu abaikan" | PASS (review, DESAIN) | Baca ulang `FinanceInvoiceExchangeDtos.cs` |
  | Tukar Faktur `LINKED_TO_INVOICE` tidak dapat dibatalkan | `CancelAsync` melempar `PurchasingValidationException` (422) dengan pesan khusus bila status `LINKED_TO_INVOICE` | PASS (review) | Baca ulang `CancelAsync` |
  | Konsistensi nama Resource/Action `[AccessPermission]` vs `permission-audit-matrix.md` §B.3 | Cocok persis pada 3 endpoint, **kecuali** satu selisih terdokumentasi (`cancel`: `Update` di `api-contract.md` vs `Cancel` di `permission-audit-matrix.md`) — pola SAMA seperti `BE-FIN-032`, kini terjadi dua kali berturut-turut | PASS (review) dengan CATATAN | Perbandingan manual baris-per-baris |
  | `SortOrder`/`ModuleCode` tidak bentrok controller Finance lain | `SortOrder 44` (tertinggi sebelumnya `43`, `FinanceGoodsReceiptsController`); `ModuleCode CORPORATE_FINANCE_MANAGEMENT_INVOICE_EXCHANGE` belum dipakai | PASS (review) | `grep` seluruh `SortOrder =`/`AccessController(` |
  | Pencarian nama service/DTO/controller baru di seluruh repository | Masing-masing tepat satu definisi | PASS (review) | `grep -rn` pasca-tulis |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED.**

- WARNINGS: Task ini bertumpuk di atas `BE-FIN-028`..`032` yang juga belum ter-`build`. **Pola selisih `api-contract.md` vs `permission-audit-matrix.md` pada endpoint `cancel`** kini terjadi konsisten pada DUA submodul berturut-turut (PO, Tukar Faktur) — kemungkinan besar berlaku juga untuk `PurchasingInvoice`/`SupplierReturn` (belum dibangun). Direkomendasikan salah satu dokumen kontrak diperbaiki agar konsisten sebelum `BE-FIN-034`/`035` ditulis, supaya tidak perlu diputuskan berulang per task.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — validasi tertunda milik pengguna.
  2. `ExchangeNumber` masih memakai pola GUID-suffix (`GenerateExchangeNumber`) — bukan provider number-series atomik (`QBE-CODE-001..006`), gap yang sama seperti `PONumber`/`GRNumber` (`BE-FIN-032`), tidak berubah oleh task ini.
  3. `GET /` (daftar berpaging) **tidak dibangun** — gap terbuka yang sama seperti `FinancePurchaseOrdersController`/`FinanceGoodsReceiptsController`.
  4. Transisi `RECEIVED → LINKED_TO_INVOICE` **tidak dibangun di sini** — milik `BE-FIN-034` (Purchasing Invoice), dilakukan sistem saat invoice berhasil dibuat dari Tukar Faktur ini, bukan endpoint pada controller ini.
  5. Selisih dokumentasi `api-contract.md`/`permission-audit-matrix.md` pada action `cancel` — lihat WARNINGS, direkomendasikan diperbaiki di level dokumen kontrak sebelum berulang untuk task ketiga kalinya.

- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses dan migration `BE-FIN-031` diterapkan lebih dulu.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: NONE
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceInvoiceExchangeDtos.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceInvoiceExchangesController.cs
  ```

  (Perubahan lain pada working tree — `BE-FIN-027`..`032` dan seluruh dokumen blueprint AMENDMENT REVISI 4/5 — sudah ada sebelum task ini dimulai; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** mencakup `BE-FIN-028`..`033` sekaligus. `BE-FIN-034` (Purchasing Invoice, menyusun `RECEIVED → LINKED_TO_INVOICE`, menulis `FinSupplierPayable`, dan kejadian PPN Masukan) adalah task berikutnya yang bergantung langsung pada `BE-FIN-033` — direkomendasikan mempertimbangkan perbaikan selisih dokumen kontrak (WARNINGS) sebelum memulainya.

---

## AMENDMENT — Penyelesaian `GET /` berpaging (30 September 2026)

Dipicu temuan `FE-FIN-008` (task frontend, dilaporkan lewat `Langkah berikutnya` laporan itu, bukan
diperbaiki sepihak di sana — lihat AMENDMENT senada pada `BE-FIN-032.md` untuk konteks lengkap PO/GR,
dikerjakan dalam satu commit kerja yang sama dengan pembaruan ini). Kontrak `FIN-API-1.1` §B.3 SUDAH
mendaftarkan bentuk `GET /` (`InvoiceExchangeQuery` → `PagedResult<InvoiceExchangeResponse>`, status
`locked` 25 September 2026) sejak sebelum `BE-FIN-033` ditulis — KNOWN ISSUE #3 di atas murni
implementasi tertunda, diselesaikan langsung lewat `build-module-backend` tanpa mengulang
grill-me/design-business-module.

- FILES CHANGED (tambahan, di atas FILES CHANGED asli):
  - `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceInvoiceExchangeDtos.cs` — tambah `InvoiceExchangeQuery` (filter `SupplierId`/`Status`, persis dua filter yang disebut `api-contract.md`)
  - `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceInvoiceExchangeService.cs` — tambah `GetPagedAsync(InvoiceExchangeQuery, ...)` (pola query/paging persis `FinanceSupplierReturnService.GetDepositsPagedAsync`, referensi terdekat pada submodul yang sama)
  - `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceInvoiceExchangesController.cs` — tambah `[HttpGet] GetList`, `[AccessPermission("FinanceInvoiceExchange", "Read")]` (sama persis dengan `GetById`, konsisten pola `FinanceSupplierReturnsController.GetDeposits`)

- OTORISASI: `GET /` memakai `[AccessPermission]` yang SAMA dengan `GetById` yang sudah ada (`FinanceInvoiceExchange : Read`) — dikonfirmasi sudah terdaftar persis di `permission-audit-matrix.md` baris `GET /purchasing/invoice-exchanges`. Nol resource/action baru diciptakan.

- VALIDATION (pembaruan):

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build QuilvianSystemBackend.csproj` | **PASS** — `0 Error(s)`, `233 Warning(s)` (seluruhnya XML-doc pre-existing di file lain, nol menyentuh file yang diubah task ini) | BUILD-VERIFIED | Dijalankan langsung task ini — pertama kali seluruh solution, termasuk `BE-FIN-028`..`033`, terverifikasi compiler |
  | `GET /` filter `SupplierId`/`Status` persis kontrak | `GetPagedAsync` menerapkan keduanya; nol filter tambahan yang tidak diminta kontrak | PASS (review) | Baca ulang `GetPagedAsync` |
  | `PagedResult<T>` shape | Identik pola `FinanceSupplierReturnService.GetDepositsPagedAsync`/`SupplierReturnsController.GetDeposits` | PASS (review) | Perbandingan langsung kedua implementasi |
  | Permission `GET /` = permission `GetById` yang sudah ada | Dicocokkan persis `permission-audit-matrix.md` baris `GET /purchasing/invoice-exchanges` | PASS (review) | Perbandingan manual |
  | Pencarian nama method/DTO baru di seluruh repository | Tepat satu definisi | PASS (review) | `grep -rn` pasca-tulis |

  **Klasifikasi keseluruhan pembaruan ini: BUILD-VERIFIED + REVIEW. MANUAL TEST tetap NOT FEASIBLE** (butuh migration `BE-FIN-031` diterapkan dan data nyata untuk pagination end-to-end).

- KNOWN ISSUES (pembaruan status, bukan daftar baru):
  1. ~~`dotnet build` belum dijalankan~~ — **RESOLVED** pembaruan ini: `dotnet build` PASS, `0` error.
  2. `ExchangeNumber` pola GUID-suffix — tidak berubah, di luar cakupan tugas ini.
  3. ~~`GET /` (daftar berpaging) tidak dibangun~~ — **RESOLVED** pembaruan ini.
  4. Transisi `RECEIVED → LINKED_TO_INVOICE` — tidak berubah, tetap milik `BE-FIN-034`.
  5. Selisih dokumentasi `api-contract.md`/`permission-audit-matrix.md` pada action `cancel` — tidak berubah, di luar cakupan tugas ini (perbaikan dokumen, bukan implementasi).

- GIT STATUS (pembaruan ini) — lihat blok identik pada `BE-FIN-032.md` AMENDMENT, satu working tree yang sama untuk kedua task ini (`FinanceInvoiceExchangeDtos.cs`, `FinanceInvoiceExchangeService.cs`, `FinanceInvoiceExchangesController.cs` termasuk di dalamnya).

- NEXT RECOMMENDED STEP (pembaruan): `BE-FIN-033` kini nol KNOWN ISSUES yang tersisa milik cakupannya sendiri selain #2 (kosmetik, GUID-suffix) dan #4/#5 (eksplisit di luar cakupan, milik task lain/perbaikan dokumen) — dapat dipertimbangkan naik status setelah `dotnet build` solution penuh dikonfirmasi pengguna sendiri dan migration `BE-FIN-031` diterapkan untuk verifikasi manual.
