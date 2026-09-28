# BE-FIN-032 — Purchase Order dan Tanda Terima Barang, dengan persetujuan berjenjang

- TASK ID: BE-FIN-032
- TASK TYPE: NEW CODE (dua service, dua controller, DTO pada submodul `Purchasing`) + kecil TOUCHED LEGACY (dua role Identity baru dan satu service otorisasi bersama, ditempatkan di submodul `Payable` mengikuti presedens `FinanceApprovalTierResolver`)
- COMPLEXITY: HIGH (akumulasi kuantitas penerimaan lintas GR, rollback status PO otomatis, dan gerbang otorisasi jenjang yang sebelumnya tidak ada infrastrukturnya sama sekali)
- CLASSIFICATION SCORE: NOT SCORED
- MODEL: Claude Sonnet 5
- TASK MODE: BACKEND
- WRITE TARGET: `NewQuilvianSystemBackend` — `Areas/Corporate/FinanceManagement/Purchasing/{Services,Controllers,Dtos}/**` (baru), `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalAuthorizationService.cs` (baru), `Seeders/FinanceApprovalRoleSeeder.cs` (baru), `Program.cs` (registrasi seeder), `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` (registrasi DI)

- BACKEND GOVERNANCE PREFLIGHT:
  - Area/Module/Submodule: `Corporate / Finance` / `FinanceManagement` / `Purchasing` (`ACTIVE` sejak `BE-FIN-027`) untuk service/controller/DTO baru; `Payable` (`ACTIVE`, sudah lama) untuk `FinanceApprovalAuthorizationService`
  - Prefix: `Fin` — dipakai apa adanya, nol prefix baru
  - Keberlakuan: `NEW CODE` untuk seluruh service/controller/DTO Purchasing (entity sumbernya, `FinPurchaseOrder`/`FinPurchaseOrderItem`/`FinGoodsReceipt`/`FinGoodsReceiptItem`, sudah ada dari `BE-FIN-029`, dipakai apa adanya — **nol perubahan model**)
  - Status registry: terpenuhi, tidak `BLOCKED`
  - QBE ID yang berlaku: `QBE-NAM-001`, `QBE-NAM-002`, `QBE-NAM-004` (penamaan service/controller/DTO), `QBE-MOD-001` (submodul sudah terdaftar `BE-FIN-027`); `QBE-CODE-001..006` **belum terpenuhi** untuk `PONumber`/`GRNumber` — lihat KNOWN ISSUES

- FILES INSPECTED:
  - `docs/module-blueprints/finance-management/contracts/api-contract.md` §B.1-B.2 — endpoint, request/response, hak akses per endpoint
  - `docs/module-blueprints/finance-management/contracts/permission-audit-matrix.md` §B.1-B.3 — Resource/Action/string `[AccessPermission]` persis. **Ditemukan selisih dengan api-contract.md**: endpoint `POST .../purchase-orders/{id}/cancel` ditulis hak akses `FinancePurchaseOrder : Update` pada `api-contract.md` §B.1, tetapi `Cancel` pada `permission-audit-matrix.md` §B.2. Task ini mengikuti `permission-audit-matrix.md` (dokumen yang secara eksplisit didedikasikan sebagai sumber string `[AccessPermission]`) — dicatat sebagai temuan, bukan diperbaiki sepihak di kedua dokumen kontrak
  - `docs/module-blueprints/finance-management/contracts/state-transition-matrix.md` §B.1-B.2 — seluruh transisi status PO dan GR, termasuk syarat dan kode kegagalan
  - `docs/module-blueprints/finance-management/contracts/validation-matrix.md` `FIN-VAL-100`..`104` beserta contoh kasus `FIN-VAL-102`
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs` (dibaca penuh, 744 baris) — pola maker-checker (`ApproveAsync`, self-approval saja), pola replace-wholesale item pada `UpdateDraftAsync`, helper `EnsureCurrent`/`Stale`/`BeginTransactionAsync`/`AcquireLockAsync`/`ValidateText`/`GeneratePaymentNumber`, keluarga exception `PaymentBadRequestException`/`PaymentValidationException`/`PaymentConflictException`
  - `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalTierResolver.cs` — ambang `TIER_1`/`TIER_2`, dipakai apa adanya
  - `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs` (dibaca penuh) — pola `[AccessController]`/`[AccessAction]`/`[AccessPermission]`, `Failure`/`IsHandled`, `CurrentUserId`
  - `Areas/Corporate/FinanceManagement/Purchasing/Models/FinPurchaseOrder*.cs`, `FinGoodsReceipt*.cs` (`BE-FIN-029`) — dibaca ulang untuk memastikan **nol** kolom baru diasumsikan; ditemukan **`FinPurchaseOrder` TIDAK punya kolom `RejectionReason`** — lihat KNOWN ISSUES
  - **Audit menyeluruh mekanisme otorisasi jenjang** — `01-existing-capability-map.md` `FIN-CAP-035` (dikonfirmasi: hanya *perhitungan* tier yang sudah berjalan, bukan *penegakan* siapa boleh menyetujui tier apa); `Attributes/AccessPermissionAttribute.cs` dan `Services/Security/AccessPermissionService.cs` (dikonfirmasi: model permission murni `resource:action` biner, nol konsep ambang nominal); pencarian `ApprovalLimit`/`SpendingLimit`/`ApprovalThreshold`/`ApproverLevel` di seluruh backend — nol hasil; pencarian role Identity Finance yang sudah ada — nol hasil (hanya `SuperAdminSeeder`). **Kesimpulan: FIN-VAL-102 belum pernah diimplementasikan di codebase mana pun, termasuk pada `FinancePaymentService.ApproveAsync` yang sudah berstatus selesai (`BE-FIN-020`)** — dikonfirmasi lewat `AskUserQuestion` ke Yasmin, dijawab "Role ASP.NET Identity baru"
  - `Seeders/SuperAdminSeeder.cs` — pola `EnsureRoleAsync`/`RoleManager<ApplicationRole>` untuk seeder role baru
  - `Program.cs` — titik pendaftaran `RunStartupSeederAsync` yang sudah ada untuk ±15 seeder lain
  - `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — titik registrasi DI `FinancePaymentService` dkk (utang teknis existing, diikuti apa adanya sesuai instruksi `BE-FIN-028`)
  - `Areas/HealthServices/BillingManagement/Billing/Controllers/BillingFinancialExceptionsController.cs` — pola pemetaan exception 403 (`StatusCode(403, ApiResponse<object>.Fail(403, ...))`), karena keluarga exception `FinancePaymentService` sendiri tidak punya varian 403

- FILES CHANGED:
  - **Baru** `Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalAuthorizationService.cs` — `FinanceApprovalRoles` (`Supervisor Finance`, `Manajer Finance`) + `FinanceApprovalAuthorizationService.CanApproveAsync(actorUserId, approvalTier)`: Manajer boleh menyetujui TIER_1 maupun TIER_2; Supervisor hanya TIER_1
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Services/PurchasingExceptions.cs` — `PurchasingBadRequestException`/`PurchasingValidationException`/`PurchasingConflictException`/`PurchasingForbiddenException`, dipakai bersama kedua service Purchasing
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchaseOrderService.cs` — `GetByIdAsync`, `CreateAsync`, `UpdateAsync`, `SubmitAsync`, `ApproveAsync`, `RejectAsync`, `CancelAsync`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs` — `GetByIdAsync`, `CreateAsync`, `CancelAsync`, `RecomputePurchaseOrderStatus` (helper bersama)
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchaseOrderDtos.cs`, `FinanceGoodsReceiptDtos.cs`
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchaseOrdersController.cs` — rincian, create, update, submit, approve, reject, cancel
  - **Baru** `Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceGoodsReceiptsController.cs` — rincian, create, cancel
  - **Baru** `Seeders/FinanceApprovalRoleSeeder.cs` — menyiapkan definisi dua role (**bukan** penugasan staf ke role, lihat KNOWN ISSUES)
  - `Program.cs` — satu baris `RunStartupSeederAsync("FinanceApprovalRoleSeeder", ...)` disisipkan setelah `SuperAdminSeeder`, sebelum `AccessMenuSeeder`; **tidak ada baris lain yang diubah**
  - `Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — satu `using` baru + tiga `services.AddScoped<...>()` disisipkan setelah `FinancePaymentService`; **tidak ada baris lain yang diubah**

- IMPLEMENTATION:

  **Gerbang otorisasi jenjang — temuan dan keputusan.** Sebelum menulis `ApproveAsync`, dilakukan audit menyeluruh: apakah ada mekanisme APA PUN di codebase ini yang membedakan penyetuju TIER_1 dari TIER_2? Hasilnya nol — bahkan `FinancePaymentService.ApproveAsync` (`BE-FIN-020`, berstatus ✅ selesai) hanya menolak self-approval, tidak pernah mengecek jenjang penyetuju sama sekali, walau `FIN-VAL-052` (padanan `FIN-VAL-102` di sisi Payment) sudah lama tercatat sebagai kontrak yang berlaku. Model permission (`AccessPermissionAttribute`/`AccessPermissionService`) murni biner `resource:action`, nol konsep ambang nominal. Karena ini gerbang keamanan nyata (bukan detail kosmetik) dan acceptance criteria `BE-FIN-032` eksplisit menuntutnya ("Supervisor menyetujui PO Rp 62.000.000 ditolak 403"), keputusan mekanismenya ditanyakan eksplisit lewat `AskUserQuestion` — dijawab **"Role ASP.NET Identity baru"**. `FinanceApprovalAuthorizationService` dan dua role (`Supervisor Finance`, `Manajer Finance`, nama persis istilah yang sudah dipakai `state-transition-matrix.md`/`validation-matrix.md`) dibangun sebagai konsekuensinya, ditempatkan di `Payable/Services` (bukan `Purchasing`) karena sifatnya benar-benar lintas-fitur — persis alasan `FinanceApprovalTierResolver` (`BE-FIN-028`) juga di sana.

  Role hanya **didefinisikan** oleh `FinanceApprovalRoleSeeder` (dipanggil otomatis saat aplikasi berikutnya kali dijalankan, pola persis 15+ seeder lain di `Program.cs` — bukan tindakan terpisah yang saya jalankan). **Menugaskan staf tertentu ke role ini** (mis. lewat `UserManager.AddToRoleAsync` via layar manajemen pengguna yang sudah ada) adalah tindakan administratif terpisah, di luar cakupan task ini — tanpa penugasan itu, TIDAK ADA seorang pun yang dapat menyetujui PO TIER_2 sampai admin menugaskan minimal satu pengguna ke role `Manajer Finance`.

  **Purchase Order.** `CreateAsync` menegakkan `FIN-VAL-100` (minimal satu baris) dan menghitung `TotalAmount`/`ApprovalTier` awal. `UpdateAsync` (DRAFT saja) memakai pola replace-wholesale persis `FinancePaymentService.UpdateDraftAsync` (`RemoveRange` baris lama, tambah baris baru), menghitung ulang tier. `SubmitAsync` menghitung ulang `ApprovalTier` sekali lagi dari `TotalAmount` final — sesuai kalimat eksplisit `state-transition-matrix.md` §B.1 ("Ajukan ... `ApprovalTier` dihitung dari `TotalAmount`"), bukan sekadar mengandalkan nilai dari `Create`/`Update`. `ApproveAsync` menegakkan `FIN-VAL-101` (self-approval, 422) lalu `FIN-VAL-102` (jenjang, 403, lewat `FinanceApprovalAuthorizationService`). `CancelAsync` hanya dari `DRAFT`/`PENDING_APPROVAL` (persis `state-transition-matrix.md` §B.1) dengan pengaman berlapis `FIN-VAL-103` (walau secara alur PO pada dua status ini tidak mungkin sudah punya GR, karena GR hanya tercatat pasca-`APPROVED`).

  **Tanda Terima Barang.** `CreateAsync` menolak PO berstatus selain `APPROVED`/`PARTIALLY_RECEIVED` (secara eksplisit **mengecualikan** `FULLY_RECEIVED` — `state-transition-matrix.md` §B.2 hanya menyebut dua status itu). Akumulasi `ReceivedQuantity` dihitung dari SELURUH GR aktif (belum dibatalkan) atas PO yang sama, dalam transaksi `Serializable` + `pg_advisory_xact_lock` per PO (mencegah dua GR berjalan bersamaan melampaui sisa kuantitas tanpa saling mendeteksi, `FIN-VAL-104`) — pola identik `FinancePaymentService` untuk pemakaian deposit. Status PO dihitung ulang (`RecomputePurchaseOrderStatus`) dari akumulasi tersebut: nol diterima tetap `APPROVED`, sebagian `PARTIALLY_RECEIVED`, seluruhnya `FULLY_RECEIVED`.

  **Keputusan desain yang tidak eksplisit di kontrak, diambil demi konsistensi**: `CancelAsync` pada GR memanggil ULANG `RecomputePurchaseOrderStatus` dengan akumulasi yang MENGECUALIKAN GR yang baru dibatalkan — supaya status PO otomatis turun kembali (`FULLY_RECEIVED`→`PARTIALLY_RECEIVED` atau `PARTIALLY_RECEIVED`→`APPROVED`) tanpa logika pembalikan terpisah. Kontrak tidak menyebutkan ini secara eksplisit, tapi ini konsekuensi langsung dari invariant "status PO mencerminkan akumulasi GR aktif" yang sudah dinyatakan untuk arah maju (penerimaan) — dicatat di sini secara transparan, bukan diam-diam diasumsikan.

- BLUEPRINT STATUS/EVIDENCE: NOT APPLICABLE
- API CONTRACT IMPACT: Dua controller baru sesuai `FIN-API-1.1` §B.1-B.2, **kecuali** `GET /` (daftar berpaging) pada keduanya — belum ada service-nya, gap yang sama seperti `FinancePaymentsController` (dicatat, bukan dikarang). `POST /{id}/cancel` PO memakai action `Cancel` (ikut `permission-audit-matrix.md`, lihat FILES INSPECTED untuk selisih dengan `api-contract.md`).
- DATABASE IMPACT: **Nol.** Seluruh entity yang dipakai (`FinPurchaseOrder`, `FinPurchaseOrderItem`, `FinGoodsReceipt`, `FinGoodsReceiptItem`) sudah ada dari `BE-FIN-029`/`031`. Dua role Identity baru (`AspNetRoles`) akan tercipta otomatis saat aplikasi berikutnya kali dijalankan lewat `FinanceApprovalRoleSeeder` — ini data seed lewat jalur startup yang sudah berjalan untuk 15+ seeder lain (bukan migration, bukan eksekusi database yang saya jalankan sendiri).
- SECURITY IMPACT: **Signifikan, disengaja.** Dua role Identity baru menentukan siapa boleh menyetujui PO/Purchasing Invoice/Pembayaran bernilai TIER_2 — lihat IMPLEMENTATION untuk alasan lengkap dan KNOWN ISSUES untuk syarat aktivasinya (penugasan staf ke role).
- VISUAL REFERENCE: NOT APPLICABLE
- VALIDATION:

  | Command/check | Hasil | Klasifikasi | Bukti/catatan |
  |---|---|---|---|
  | `dotnet build` | **SENGAJA TIDAK DIJALANKAN** | VALIDASI TERTUNDA | Instruksi eksplisit pengguna — pengguna menjalankan sendiri. Task ini bertumpuk di atas `BE-FIN-028`/`029`/`030`/`031` yang juga belum terverifikasi compiler |
  | FIN-VAL-100 (PO tanpa baris → 400) | `CreateAsync` melempar `PurchasingBadRequestException` bila `items` kosong, dipetakan `400` | PASS (review) | Baca ulang `CreateAsync` + `Failure` switch controller |
  | FIN-VAL-101 (pengaju = penyetuju → 422) | `ApproveAsync` membandingkan `actorUserId == purchaseOrder.RequestedByUserId` sebelum cek jenjang, melempar `PurchasingValidationException` (422) | PASS (review) | Baca ulang urutan pengecekan `ApproveAsync` |
  | FIN-VAL-102 (Supervisor approve TIER_2 → 403) | `FinanceApprovalAuthorizationService.CanApproveAsync` mengembalikan `false` untuk role `Supervisor Finance` saja pada `approvalTier = TIER_2`; `ApproveAsync` melempar `PurchasingForbiddenException`, dipetakan `403` | PASS (review, LOGIKA) — **BELUM PASS end-to-end**: butuh minimal satu pengguna nyata ber-role `Manajer Finance`/`Supervisor Finance` untuk diuji manual, lihat KNOWN ISSUES | Baca ulang `CanApproveAsync` dan `ApproveAsync` |
  | FIN-VAL-103 (PO ber-GR tidak dapat dibatalkan) | `CancelAsync` mengecek `FinGoodsReceipts.AnyAsync(...)` sebelum mengizinkan batal; status PO yang mengizinkan Cancel (`DRAFT`/`PENDING_APPROVAL`) secara alur tidak mungkin sudah punya GR — pengaman berlapis, bukan jalur yang seharusnya tercapai | PASS (review) | Baca ulang `CancelAsync` + `state-transition-matrix.md` §B.1 |
  | FIN-VAL-104 (kuantitas diterima > sisa PO → 422) | `FinanceGoodsReceiptService.CreateAsync` mengakumulasi `ReceivedQuantity` dari seluruh GR aktif per baris PO sebelum menerima baris baru, melempar `PurchasingValidationException` bila melebihi `Quantity` baris | PASS (review) | Baca ulang loop akumulasi `CreateAsync` |
  | GR sebagian/seluruh menggerakkan status PO | `RecomputePurchaseOrderStatus` membandingkan total diterima vs total dipesan seluruh baris, dipanggil setelah `CreateAsync` (maju) dan `CancelAsync` (mundur) | PASS (review) | Baca ulang helper + kedua titik pemanggilannya |
  | Konsistensi nama Resource/Action `[AccessPermission]` vs `permission-audit-matrix.md` §B.2-B.3 | Seluruh 11 endpoint (7 PO + 4 GR/GET) cocok string persis, **kecuali** satu selisih terdokumentasi (`cancel` PO: `Update` di `api-contract.md` vs `Cancel` di `permission-audit-matrix.md` — task ini mengikuti yang kedua) | PASS (review) dengan CATATAN | Perbandingan manual baris-per-baris kedua dokumen |
  | Route/`[Tags]`/`SortOrder` tidak bentrok controller Finance lain | `SortOrder 42`/`43` (tertinggi sebelumnya `41`, `FinancePaymentsController`); `ModuleCode` `CORPORATE_FINANCE_MANAGEMENT_PURCHASE_ORDER`/`..._GOODS_RECEIPT` belum dipakai controller mana pun | PASS (review) | `grep` seluruh `SortOrder =`/`AccessController(` pada `Areas/Corporate/FinanceManagement` |
  | Pencarian nama exception/service/DTO baru di seluruh repository | Masing-masing tepat satu definisi | PASS (review) | `grep -rn` pasca-tulis |

  **Klasifikasi keseluruhan validasi task ini: REVIEW ONLY, bukan BUILD-VERIFIED, bukan diuji dengan pengguna nyata.**

- WARNINGS: Task ini bertumpuk di atas `BE-FIN-028`/`029`/`030`/`031` yang juga belum ter-`build`. **FIN-VAL-102 tidak akan benar-benar tertegakkan sampai minimal satu pengguna ditugaskan ke role `Manajer Finance`** — sebelum itu, SETIAP percobaan approve PO TIER_2 akan selalu ditolak `403` (tidak ada yang berwenang), bukan celah keamanan tapi tetap perlu diketahui sebelum UAT.
- KNOWN ISSUES:
  1. `dotnet build` belum dijalankan — validasi tertunda milik pengguna.
  2. **Penugasan staf ke role `Supervisor Finance`/`Manajer Finance` belum dilakukan** — seeder hanya membuat DEFINISI role. Tanpa penugasan, tidak ada seorang pun yang dapat menyetujui PO/Purchasing Invoice/Pembayaran mana pun (baik TIER_1 maupun TIER_2) setelah task ini berjalan, **termasuk memengaruhi `FinancePaymentService` bila diperbarui kelak untuk memakai `FinanceApprovalAuthorizationService` yang sama** — direkomendasikan penugasan dilakukan segera setelah role tercipta (aplikasi pertama kali start pasca-`dotnet build`).
  3. **`FinancePaymentService.ApproveAsync` (BE-FIN-020) TIDAK diperbarui pada task ini** untuk memakai `FinanceApprovalAuthorizationService` — write target task ini adalah submodul Purchasing, menyentuh `FinancePaymentService` yang sudah berstatus ✅ selesai di luar wewenang task ini. **Konsekuensinya: FIN-VAL-052 (padanan persis FIN-VAL-102 untuk Payment) TETAP belum tertegakkan** setelah task ini — gap yang sudah ada sebelumnya, bukan regresi baru, tapi sekarang codebase punya DUA aturan bisnis identik dengan status tertegakkan berbeda. Direkomendasikan task terpisah menyambungkan `FinancePaymentService` ke `FinanceApprovalAuthorizationService` yang sama.
  4. **`FinPurchaseOrder` tidak punya kolom `RejectionReason`** (ditemukan saat membaca ulang model `BE-FIN-029`) — berbeda dari `FinPayment` yang punya. `RejectAsync` tetap memvalidasi alasan wajib diisi (`FIN-VAL` generik alasan wajib) dan mencatatnya ke audit log (`LoggerService.AuditAsync`), TAPI alasan penolakan TIDAK tersimpan di baris `FinPurchaseOrder` itu sendiri — tidak dapat ditampilkan kembali lewat `GET /{id}` nantinya. Menambah kolom ini butuh migration baru (otorisasi terpisah, di luar cakupan task ini) — direkomendasikan sebagai task susulan kecil.
  5. `PONumber`/`GRNumber` masih memakai pola GUID-suffix (`GeneratePoNumber`/`GenerateGrNumber`, identik `GeneratePaymentNumber` milik `FinancePaymentService`) — bukan `Count+1`/`Max+1` (tidak melanggar `QBE-CODE-001..006`), tapi juga bukan provider number-series atomik yang sebenarnya diminta. Sudah dicatat sejak `BE-FIN-029`/`030`, tidak berubah oleh task ini.
  6. `GET /` (daftar berpaging) untuk PO maupun GR **tidak dibangun** — gap terbuka yang sama seperti `FinancePaymentsController`, dicatat sebagai keterbatasan sengaja bukan tersembunyi.
  7. Aksi "Tutup PO" (`FULLY_RECEIVED` → `CLOSED`, `state-transition-matrix.md` §B.1) **tidak dibangun** — di luar acceptance criteria eksplisit `BE-FIN-032`, menyusul task yang menyertakan siklus Tukar Faktur (`BE-FIN-033`) karena syaratnya "seluruh Tukar Faktur turunannya sudah `LINKED_TO_INVOICE` atau `CANCELLED`".

- STALE EVIDENCE / BLOCKED PHASES: NOT APPLICABLE
- MANUAL TEST: NOT FEASIBLE — memerlukan `dotnet build` sukses, migration `BE-FIN-031` diterapkan, DAN penugasan role (KNOWN ISSUE #2) lebih dulu.
- INCIDENTAL CHANGES: NONE
- INTERRUPTIONS: Satu jeda untuk `AskUserQuestion` (mekanisme otorisasi jenjang) — dijawab sebelum implementasi dimulai, bukan interupsi di tengah penulisan kode.
- GIT STATUS (akhir pekerjaan task ini):

  ```text
  M  Program.cs
  M  Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs
  ?? Areas/Corporate/FinanceManagement/Payable/Services/FinanceApprovalAuthorizationService.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Services/PurchasingExceptions.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinancePurchaseOrderService.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Services/FinanceGoodsReceiptService.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinancePurchaseOrderDtos.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Dtos/FinanceGoodsReceiptDtos.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinancePurchaseOrdersController.cs
  ?? Areas/Corporate/FinanceManagement/Purchasing/Controllers/FinanceGoodsReceiptsController.cs
  ?? Seeders/FinanceApprovalRoleSeeder.cs
  ```

  (Perubahan lain pada working tree — `BE-FIN-027`..`031` dan seluruh dokumen blueprint AMENDMENT REVISI 4/5 — sudah ada sebelum task ini dimulai; bukan hasil task ini.)

- NEXT RECOMMENDED STEP: **Pengguna menjalankan `dotnet build`** mencakup `BE-FIN-028`..`032` sekaligus. Sesudah build sukses dan migration `BE-FIN-031` diterapkan: (1) jalankan aplikasi sekali agar `FinanceApprovalRoleSeeder` membuat dua role, (2) tugaskan minimal satu pengguna ke `Manajer Finance` lewat layar manajemen pengguna yang sudah ada, baru kemudian (3) `BE-FIN-033` (Tukar Faktur, bergantung `BE-FIN-031`) dapat dikerjakan paralel dengan penyambungan `FinancePaymentService` ke `FinanceApprovalAuthorizationService` (KNOWN ISSUE #3, belum ada task ID-nya di roadmap — direkomendasikan ditambahkan).
