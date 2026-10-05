# Finance Management — Existing Capability Map

```yaml
blueprint_id: FIN-BP-001
scope_batas_audit: >
  FIN-SC-001 (Billing Handoff Inbox), FIN-SC-002..004 (AR/Collection/AP — audit hanya sisi
  sumber Billing, karena sisi Finance belum dibangun), FIN-SC-005..006 (Cash Management +
  Finance-owned master), FIN-SC-007 (Accounting Integration Outbox — audit hanya keberadaan
  endpoint penerima di Accounting). Tidak mengaudit ulang internal Billing/Accounting di luar
  titik sentuh yang relevan bagi Finance (FIN-OOS-001..004).
backend_source_sha: 09101d05
backend_repo: NewQuilvianSystemBackend
backend_branch: Yasmina
backend_working_tree: bersih (git status --short kosong saat audit)
last_impact_scan_sha: d6cdfaf9 (25 September 2026) — lihat bagian 9.3. Bagian 1-9.2 di bawah
  TIDAK diaudit ulang menyeluruh; hanya klaster yang disebut eksplisit di 9.3 yang diverifikasi
  ulang pada SHA ini. Field backend_source_sha di atas TETAP baseline audit penuh 20 September
  2026, tidak diubah.
frontend_source_sha: 49b59cfaa
frontend_source_sha_previous: abed49b03
frontend_repo: QuilvianSystemFrontendDev
frontend_branch: yasmina
backend_working_tree_28_sep_2026: >
  TIDAK BERSIH. Lima berkas source berubah dan belum di-commit — implementasi task BE-FIN-036
  (+590/-59). Field backend_working_tree di bawah (bersih) berlaku untuk audit 20 September 2026 dan
  sudah tidak mencerminkan keadaan 28 September 2026. Seluruh bukti bagian 16 ditandai
  @working-tree-2026-09-28 dan MUST ditambatkan ulang ke SHA sesudah pekerjaan itu di-commit.
last_impact_scan_revision_7: >
  cba60cb0 (backend, TIDAK bergerak) dan working tree 28 September 2026 — lihat bagian 16. Pass ini
  mengaudit pekerjaan BE-FIN-036 yang berjalan di working tree, bukan perubahan SHA.
  HASIL: sebelas titik desain FIN-DES-045..047 SESUAI; dua cacat pada pekerjaan itu (alias konstanta
  yang dilarang FIN-DES-051, dan nol test); satu temuan SISTEMIK (enam resource hak akses di kode
  berbeda nama dari kontrak, dan Finance.AP/Finance.AR tidak ada di kontrak sama sekali —
  FIN-CAP-043 `Repair`); satu koreksi rujukan pada dokumen desain revisi 6 (empat kemunculan
  FIN-VAL-123 yang seharusnya FIN-VAL-137, sudah dibetulkan).
  DUA ENTRI BARU: FIN-CAP-042 (pemakaian Deposit Retur, `Extend`), FIN-CAP-043 (penamaan resource
  hak akses, `Repair`). FIN-CAP-037 tetap `Missing` dan permukaan tanpa uji bertambah +590 baris.
  dotnet build TIDAK dijalankan (instruksi pengguna) — audit ini tidak dapat menyatakan kode itu
  dapat dikompilasi.
last_impact_scan_revision_6: >
  cba60cb0 (backend) dan 49b59cfaa (frontend), 28 September 2026 — lihat bagian 15. Pass ini adalah
  IMPACT SCAN TERARAH, bukan audit penuh: yang diverifikasi ulang HANYA entri yang berpotensi stale
  akibat 79 commit backend dan 59 commit frontend, ditambah kontrak as-is endpoint penerima
  Accounting yang kini sudah dapat dibaca langsung.
  TUJUH entri berubah status, DUA klaim lama DIKOREKSI, dan LIMA entri baru ditambahkan
  (FIN-CAP-037..041).
last_impact_scan_bagian_20: >
  5d6bb8bf (backend) dan ae2ed334e (frontend), 4 Oktober 2026 — lihat bagian 20. Pass IMPACT SCAN
  TERARAH atas hasil REV-14A..14E (BE-FIN-058..085, FE-FIN-025..032). Bagian 1-19 TIDAK diaudit ulang
  menyeluruh dan tetap tertambat pada SHA masing-masing.
  LIMA gap bagian 18/19 TERTUTUP (jalur pengiriman ke Accounting, FinReceivable nullable, kebocoran
  pembayaran langsung utang supplier, mekanisme impor, penyimpanan bukti). SATU tetap Missing sesuai
  keputusan: utang jasa medis (FIN-DEC-157).
  TEMUAN PALING MATERIAL: nol kunci konfigurasi Finance di appsettings.json, sehingga unggah bukti
  selalu 503 dan ketiga hosted service mati. Seluruhnya fail-closed, tetapi jalur pembayaran langsung
  BELUM dapat dipakai hari ini.
  SEMBILAN ENTRI BARU: FIN-CAP-058..066. SATU CONFLICT BARU: FIN-CQ-10 (prop DataTable tak dikenal) —
  sudah CLOSED hari yang sama oleh alias pada DataTable, dan ANGKANYA DIKOREKSI di 20.14: 59 pemakaian,
  bukan 191. FIN-OQ-084 juga CLOSED.
  Delapan keputusan 4 Oktober 2026 BELUM tercermin di source — source bukan lagi rujukan perilaku target.
input_decisions: docs/module-blueprints/finance-management/00-interview-decisions.md revisi 1
reused_capability_maps:
  - docs/module-blueprints/billing-kasir/01-existing-capability-map.md (approved, revisi terakhir
    18 September 2026, diaudit pada backend 21b4733). Diverifikasi MASIH BERLAKU untuk seluruh
    entity Bil* yang disentuh dokumen ini: `git diff --stat 29a7a7ad..HEAD -- Areas/HealthServices/BillingManagement
    Areas/Corporate/FinanceManagement Areas/Corporate/AccountingManagement` KOSONG (nol
    perubahan) pada backend SHA 09101d05, dan commit 29a7a7ad (19 September 2026, penulis
    yasmina04/englishrocker07@gmail.com — user yang sama dengan owner Finance) SUDAH memutakhirkan
    billing-kasir/01-existing-capability-map.md sendiri. Tidak perlu audit ulang field Bil* dari
    nol; dikutip langsung di bawah.
  - docs/module-blueprints/accounting/01-existing-capability-map.md (approved) — dikutip HANYA
    untuk titik sentuh (keberadaan endpoint Accounting Event, master EventType/PostingRule),
    BUKAN audit ulang internal Accounting (FIN-OOS-002).
```

## 1. Boundary audit

Klaster kemampuan yang relevan bagi Finance Management dan diperiksa pada pass ini:

- **External Integration** (Billing → Finance, Finance → Accounting) — klaster paling kritis,
  karena seluruh 8 keputusan `FIN-DEC-001`..`008` bergantung padanya.
- **Financial** — subledger AR/AP/Receipt/Cash yang menjadi inti modul.
- **Identity/Master Owner** — `MstPettyCashCategory`, kandidat master Bank/Supplier/Currency.
- **Authorization/Audit** — pola hak akses dan audit trail yang sudah dipakai Petty Cash,
  sebagai preseden untuk AR/AP.

Metode: `rg`/Grep langsung ke source kedua repository, dibaca implementasinya untuk field-level
fact, dibandingkan dengan `git diff` terhadap SHA audit billing-kasir untuk memastikan tidak
stale. **Tidak ada build, tidak ada eksekusi test, tidak ada perubahan source maupun blueprint.**

## 2. Ringkasan eksekutif

1. **Sisi Billing (sumber) sudah lengkap dan stabil** untuk seluruh fakta yang dibutuhkan Finance
   — `BilArHandoff`, `BilApHandoff`, `BilHandoffAdjustment`, `BilTender`, `BilSettlement`,
   `BilPaymentAllocation`, `BilCashierShift` semuanya `Ready to reuse` sebagai sumber baca,
   diverifikasi lewat billing-kasir yang sudah `approved`.
2. **Sisi Finance (konsumen) untuk AR/AP/Collection sepenuhnya `Missing`.** Tidak ada satu pun
   file `Fin{Receivable,Payable,Receipt}*` atau `BilCollectionHandoff` di seluruh repository.
   Ini konsisten dengan status PROPOSED di keempat dokumen owner — bukan temuan baru, tapi
   sekarang terverifikasi langsung dari source.
3. **Petty Cash backend `Ready to reuse` penuh**, sudah dipindah ke
   `Areas/Corporate/FinanceManagement/PettyCash` dan `MasterData` sesuai entri registry
   17 September 2026. **Temuan penting:** keputusan bisnis/arsitektur Petty Cash (siklus hidup
   voucher, status budget, movement type, dsb.) sudah `approved` di blueprint **billing-kasir**
   dengan prefix `PC-DEC-*`/`PC-DES-*` — BUKAN di blueprint `finance-management` ini, walau
   kodenya sekarang tinggal di folder `Corporate/FinanceManagement`. Lihat `FIN-CQ-01` di
   bagian 9.
4. **Petty Cash frontend `Reuse with adapter`, bukan `Missing` dan bukan `Ready to reuse`
   penuh.** Rute kanonik `/finance/petty-cash-budget` dan
   `/finance/master-data/petty-cash-category` sudah ada dan berfungsi, tetapi keduanya memanggil
   hook/Redux slice yang masih hidup di path lama
   `src/lib/state/slice/health-services/billing-management/*`. Voucher (create/return/reversal)
   **belum** punya rute di bawah `/finance/` sama sekali — hanya ada di
   `/health-services/billing-management/petty-cash/vouchers`.
5. **Endpoint penerima Accounting Event benar-benar belum dibangun.** Dikonfirmasi langsung:
   tidak ada controller/route yang cocok pola `accounting-events` di
   `Areas/Corporate/AccountingManagement`. Klaim di `FIN-ACC-XMOD-V2-0.3` bagian 1 ("Accounting
   Event inbox/POST endpoint... not yet built") **cocok dengan source saat ini**.
6. **Satu koreksi dokumentasi ditemukan:** PRD `FIN-PRD-V2-0.3` bagian 9.2 menulis base path
   `api/v1/corporate/financemanagement/...` (tanpa tanda hubung). Source sebenarnya memakai
   `api/v1/corporate/finance-management/...` (dengan tanda hubung). Lihat `FIN-CAP-010`.
7. **Tidak ada Conflict yang memblokir.** Satu Closure Question (`FIN-CQ-01`, kepemilikan
   decision log Petty Cash) perlu disepakati sebelum lanjut ke `/design-business-module`, tapi
   tidak memblokir AR/AP yang memang belum tersentuh blueprint mana pun.

## 3. Capability evidence map

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-001` | Sumber AR dari Billing (`FIN-SC-001`, `FIN-AR-001`) | Billing | `NewQuilvianSystemBackend/Areas/HealthServices/BillingManagement/Billing/Models/BilArHandoff.cs#BilArHandoff@09101d05` — field `InvoiceId`, `FinalizationRecordId`, `DebtorType` (hanya `PAYER`/`PATIENT_GUARANTOR`), `DebtorReferenceId`, `Amount`, `DueDate`, `Status` (`CREATED`/`ACKNOWLEDGED`), `HandoffKey`, `CorrelationId`, `CausationId`, `RowVersion` | `Ready to reuse` | Tidak ada `DebtorType EMPLOYEE_BENEFIT` — perlu `Extend` sesuai `FIN-DEC-006` | Field diverifikasi baca langsung, bukan dikutip dari dokumen owner |
| `FIN-CAP-002` | Sumber AP/doctor dari Billing (`FIN-SC-004`) | Billing | `Areas/HealthServices/BillingManagement/Billing/Models/BilApHandoff.cs#BilApHandoff@09101d05` — `DoctorId`, `Amount`, `ReadinessStatus` (`NOT_READY`/`READY`), `Status`, `HandoffKey`, korelasi | `Ready to reuse` | Tidak ada — bentuk sudah sesuai kebutuhan `FIN-DEC-003` (Opsi B, fee dokter terpisah) | — |
| `FIN-CAP-003` | Koreksi/adjustment atas handoff (`FIN-AR-009`, `FIN-AP-006`) | Billing | `Areas/HealthServices/BillingManagement/Billing/Models/BilHandoffAdjustment.cs#BilHandoffAdjustment@09101d05` — `ArHandoffId`/`ApHandoffId` opsional, `Direction`, `Amount`, `Reason` wajib, korelasi | `Ready to reuse` | — | — |
| `FIN-CAP-004` | Sumber tender/pembayaran kasir (`FIN-BIL-007`) | Billing | `Areas/HealthServices/BillingManagement/Billing/Models/BilTender.cs#BilTender@09101d05` — `SettlementId`, `PaymentMethodId`/`PaymentMethodAccountId`, `Amount`, `Status` (5 nilai termasuk `REVERSED`), `KwitansiNumber`, `CashierShiftId`, `ProviderReference`, korelasi, `IdempotencyKey` | `Ready to reuse` | — | Cocok persis dengan seluruh field yang diminta kontrak `FIN-PRD-V2-0.3` bagian 5.1 |
| `FIN-CAP-005` | Sumber settlement/alokasi (`FIN-COL-02`) | Billing | `Areas/HealthServices/BillingManagement/Billing/Models/BilSettlement.cs`, `BilPaymentAllocation.cs@09101d05`; dikutip dari `docs/module-blueprints/billing-kasir/01-existing-capability-map.md` §1, §21 (approved) | `Ready to reuse` | `BilSettlement` bersifat satu-ke-banyak terhadap invoice (temuan billing-kasir §21) — Finance harus akumulasi lintas settlement, bukan per-settlement | Dikutip dari blueprint lain; tidak diaudit ulang field-per-field pada pass ini |
| `FIN-CAP-006` | Sumber shift kasir untuk rekonsiliasi (`FIN-CASH-010`) | Billing | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashierShift.cs@09101d05`; dikutip billing-kasir §1 (`Missing` di sana per 27 Agustus 2026, sudah `Ready to reuse` di §17 dan seterusnya) | `Ready to reuse` (read-only) | Finance hanya boleh baca — larangan ini sudah eksplisit di `FIN-BRD-V2-0.3` aturan #12 | — |
| `FIN-CAP-007` | Kontrak `BilCollectionHandoff` (`FIN-DEC-005`) | Billing (titik sentuh Finance) | ~~Grep `BilCollectionHandoff\|CollectionHandoff` di seluruh backend — nol hasil~~ **USANG, lihat 9.3.** `Areas/HealthServices/BillingManagement/Billing/Models/BilCollectionHandoff.cs#BilCollectionHandoff@d6cdfaf9` — SUDAH DIBANGUN | `Ready to reuse` (diperbarui 25 September 2026) | — | Menutup `blocking_questions.BILLING-COLLECTION-HANDOFF` pada `blueprint-manifest.md` — lihat 9.3 |
| `FIN-CAP-008` | Konsumen Finance untuk `BilArHandoff`/`BilApHandoff`/`BilCollectionHandoff` (`FIN-SC-001`) | Finance | ~~Grep `BilArHandoff\|BilApHandoff` di `Areas/Corporate` — nol hasil~~ **USANG, lihat 9.3.** `FinanceBillingIntakeService.cs#SyncNewFactsAsync,ProcessAsync@d6cdfaf9` | `Ready to reuse` (diperbarui 25 September 2026) | — | — |
| `FIN-CAP-009` | `FinReceivable`/`FinPayable`/`FinReceipt`/`FinAccountingEventOutbox` (`FIN-SC-002..004`, `007`) | Finance | ~~Grep — nol hasil~~ **USANG, lihat 9.3.** Seluruhnya ADA di `d6cdfaf9`: `Collection/Models/FinReceipt.cs`, `Receivable/Models/FinReceivable.cs`, `Payable/Models/FinSupplierPayable.cs`/`FinMedicalServicePayable.cs`, `AccountingIntegration/Models/FinAccountingEventOutbox.cs` | `Ready to reuse` (diperbarui 25 September 2026) | — | Cakupan mendalam per entity BELUM diaudit pass ini — lihat batas 9.3 |
| `FIN-CAP-010` | Master kategori Petty Cash (`FIN-SC-006`, `FIN-CASH-004`) | Finance | `Areas/Corporate/FinanceManagement/MasterData/Models/MstPettyCashCategory.cs#MstPettyCashCategory@09101d05` (`CategoryCode`, `CategoryName`, `Description`, `IsActive`); controller `Areas/Corporate/FinanceManagement/MasterData/Controllers/PettyCashCategoriesController.cs` — route ganda `api/v1/corporate/finance-management/master-data/petty-cash-categories` (kanonik) dan `api/v1/health-services/billing-management/master-data/petty-cash-categories` (alias kompatibilitas), `[Tags("Corporate / Finance Management / Master Data / Petty Cash Category")]` | `Ready to reuse` | — | Base path kanonik memakai tanda hubung `finance-management`, BUKAN `financemanagement` seperti tertulis di `FIN-PRD-V2-0.3` §9.2 — dokumen owner perlu dikoreksi saat dipakai acuan desain |
| `FIN-CAP-011` | Budget/anggaran Petty Cash per periode (`FIN-SC-005`, `FIN-CASH-005/006`) | Finance | `Areas/Corporate/FinanceManagement/PettyCash/Models/FinPettyCashBudget.cs#FinPettyCashBudget@09101d05` — `PoolCode`, `PeriodStart`/`PeriodEnd`, `BudgetAmount`, `CurrentBalance`, `Status` (`DRAFT`/`ACTIVE`/`CLOSED`, plus legacy `ACTIVE_OLD`/`INACTIVE`), `SupersededByBudgetId`, `RowVersion` | `Ready to reuse` | — | Keputusan bisnis di balik model ini (`PC-DEC-016..027`) ada di blueprint billing-kasir, bukan di sini — lihat `FIN-CQ-01` |
| `FIN-CAP-012` | Ledger mutasi Petty Cash (`FIN-SC-005`, `FIN-CASH-007`) | Finance | `Areas/Corporate/FinanceManagement/PettyCash/Models/FinPettyCashBudgetMovement.cs#FinPettyCashBudgetMovement@09101d05` — `MovementType` (`TOP_UP`/`DISBURSEMENT`/`ADJUSTMENT`/`RETURN`/`REVERSAL`/`CARRY_FORWARD_OUT`/`CARRY_FORWARD_IN`), `BalanceBefore`/`BalanceAfter`, `VoucherId` opsional, `FundingSourceType` (`TRANSFER`/`CASH`, hanya untuk `TOP_UP`), `IdempotencyKey`, `CorrelationId` | `Ready to reuse` | `FundingSourceType`/`TransferReference` belum menunjuk akun bank/kas sumber spesifik — persis gap yang dicatat `FIN-ACC-XMOD-V2-0.3` §9.2 untuk auto-posting | Dasar langsung publikasi kejadian `PETTY-CASH-*` ke Accounting (`FIN-DEC-002`) |
| `FIN-CAP-013` | Endpoint operasional Petty Cash budget (`FIN-CASH-005/006/007`) | Finance | `Areas/Corporate/FinanceManagement/PettyCash/Controllers/PettyCashBudgetController.cs@09101d05` — `GET current/overview/periods/movements`, `POST top-ups/adjustments/periods`, `POST periods/{id}/activate`, `POST periods/{id}/close`; route ganda kanonik+alias sama seperti `FIN-CAP-010` | `Ready to reuse` | — | 11 endpoint, cocok jumlahnya dengan klaim `FIN-PRD-V2-0.3` §9.2 |
| `FIN-CAP-014` | Voucher operasional Petty Cash (`FIN-SC-005`) | Billing (titik sentuh) | `Areas/HealthServices/BillingManagement/PettyCash/Models/BilPettyCashVoucher.cs@09101d05` — tetap di Billing sesuai entri registry 17 September 2026; `CategoryId` menunjuk `MstPettyCashCategory` milik Finance | `Ready to reuse` (read/link) | Finance tidak boleh memiliki tabel ini — hanya menaut lewat `VoucherId` pada movement | Batas modular-monolith sudah eksplisit di registry dan di `FIN-BRD-V2-0.3` §9.3 |
| `FIN-CAP-015` | Frontend Petty Cash kanonik `/finance/*` (`FIN-CASH-04`) | Finance | `QuilvianSystemFrontendDev/src/app/finance/petty-cash-budget/*`, `src/app/finance/master-data/petty-cash-category/*@abed49b03` — halaman nyata (687 baris untuk budget view), BUKAN stub | `Reuse with adapter` | Memanggil hook/slice yang masih hidup di `src/lib/hooks/health-services/billing-management/petty-cash/*` dan `src/lib/state/slice/health-services/billing-management/*` — belum dipindah ke path kanonik Finance | Konsisten dengan rekomendasi PRD: migrasi FE ke rute Corporate adalah kerja tersisa, bukan kerja dari nol |
| `FIN-CAP-016` | Frontend voucher Petty Cash di bawah `/finance/` | Finance | Grep rute `src/app/finance/**` — **tidak ada** voucher; voucher hanya ada di `src/app/health-services/billing-management/petty-cash/vouchers/page.jsx@abed49b03` | `Missing` (untuk rute kanonik), `Ready to reuse` (untuk rute legacy) | Perlu halaman voucher baru di bawah `/finance/petty-cash/vouchers` yang reuse component `voucher-detail-modal.jsx`/`create-voucher-modal.jsx`/`reverse-voucher-modal.jsx` yang sudah ada | Bagian dari `FIN-CASH-04` yang eksplisit disebut "voucher trace" |
| `FIN-CAP-017` | Menu sidebar "Keuangan" | Finance | `src/utils/menu-sidebar/menu-items.jsx@abed49b03` baris ~659-683 — group `corporateFinance` sudah ada, HANYA berisi 2 entri: Master Data > Kategori Petty Cash, dan Anggaran Petty Cash | `Reuse with adapter` | Seluruh menu AR/AP/Bank Deposit/Daily Cash/Accounting Integration Monitor di `FIN-PRD-V2-0.3` §3.1 belum ada barisnya | Struktur group sudah ada, tinggal ditambah item — bukan bikin group baru |
| `FIN-CAP-018` | Endpoint penerima Accounting Event (`FIN-SC-007`, titik sentuh) | Accounting | Grep `accounting-events\|AccountingEvent` di `Areas/Corporate/AccountingManagement` — hanya menemukan `AccountingEventTreatment` (enum konfigurasi Posting Rule), **nol controller/route penerima** | `Missing` (dikonfirmasi ulang, bukan hanya dikutip dari dokumen) | Finance boleh membangun `FinAccountingEventOutbox` (FIN-ACC-01) tanpa endpoint ini tersedia, tapi delivery worker (FIN-ACC-03) menunggu Accounting | Dikutip juga oleh evidence Accounting sendiri (`docs/module-blueprints/accounting/evidence/12-paket-kontrak-kejadian-untuk-finance.md`) |
| `FIN-CAP-019` | Master referensi Accounting (COA/EventType/PostingRule/Period/Journal) | Accounting | Dikutip dari `docs/module-blueprints/accounting/01-existing-capability-map.md` dan `blueprint-manifest.md` (approved, revisi 11) — TIDAK diaudit ulang field-per-field karena `FIN-OOS-002` | `Ready to reuse` (sebagai rujukan baca) | — | Audit internal Accounting adalah wewenang blueprint accounting, bukan blueprint ini |
| `FIN-CAP-020` | Aturan **perhitungan** fee dokter | Medical Fee (belum ada) | `Areas/HealthServices/MasterData/Models/MstDoctorServiceRule.cs@09101d05` ADA, **tetapi isinya bukan aturan perhitungan fee** — 20+ field seluruhnya tentang kelayakan layanan (`IsAllowWalkIn`, `IsAllowAppointment`, `IsNeedReferral`, `DailyQuotaLimit`, `PriorityLevel`), dan enum `DoctorServiceRuleType` berisi jenis layanan (`GeneralService`, `Consultation`, `Procedure`, `Telemedicine`), bukan jenis perhitungan. **Nol field nominal, persentase, atau basis perhitungan** | `Conflict` | Nama entity di source sama persis dengan yang disebut `FIN-PRD-V2-0.3` bagian 8, tetapi maknanya berbeda total | **Dikoreksi 20 September 2026.** Pencatatan awal pass desain keliru menandainya `Ready to reuse` sebagai aturan fee. Lihat bagian 9.2 |
| `FIN-CAP-021` | Hasil fee dokter yang sudah disetujui (`DoctorServiceFee`) dan modul MedicalFeeManagement | Medical Fee | Pencarian `class DoctorServiceFee` dan folder `*MedicalFee*` di seluruh `Areas` — **nol hasil** pada `09101d05` | `Missing` | Seluruh modul Medical Fee belum ada; hanya master aturannya yang ada (`FIN-CAP-020`) | **Ketergantungan eksternal keras.** `FinDoctorPayable` memakai `SourceDoctorServiceFeeId` sebagai kunci idempotensi, sehingga rumpun utang dokter TIDAK DAPAT dijalankan sampai Medical Fee dibangun modul lain. Ini temuan pass desain 20 September 2026 |
| `FIN-CAP-022` | Deposit pasien — akun dan mutasinya (`FIN-DEC-040`/`041`, gerbang G6) | Billing (titik sentuh Finance) | `Areas/HealthServices/BillingManagement/Billing/Models/BilDepositAccount.cs#BilDepositAccount@d6cdfaf9` (`EncounterId`, `AvailableBalance`, status `ACTIVE`/`CLOSED`); `BilDepositMovement.cs#BilDepositMovement@d6cdfaf9` — `MovementType` (`TOP_UP`/`ALLOCATION`/`RELEASE`/`REVERSAL`), `IdempotencyKey`, `CorrelationId`/`CausationId`, `ReversesMovementId`, `CashierShiftId` | `Ready to reuse` (read-only) | — | Ditemukan lewat impact scan 25 September 2026, dipicu koreksi `FIN-DEC-032`/`033`. TIDAK ada di `Repositories/ApplicationDbContext.cs` sebelum baris 604-605 — sudah terdaftar (`DbSet<BilDepositAccount>`, `DbSet<BilDepositMovement>`) |
| `FIN-CAP-023` | Kelebihan bayar pasien — pengakuan dan pengembalian (`FIN-DEC-042`/`041`, gerbang G6) | Billing (titik sentuh Finance) | `BilRefundableCredit.cs#BilRefundableCredit@d6cdfaf9` — `SourceType` (`ALLOCATION_EXCESS`/`SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN`), `OriginalAmount`/`AvailableAmount`, status `AVAILABLE`/`EXHAUSTED`; `BilRefundCase.cs#BilRefundCase@d6cdfaf9` — `RefundCategory` (`"BILLING"`/`"DEPOSITO"`), siklus `SUBMITTED→APPROVED→PARTIALLY_EXECUTED→EXECUTED`/`REJECTED`, `IdempotencyKey` | `Ready to reuse` (read-only) | `FIN-DEC-041` SENGAJA hanya memakai irisan `SourceType = ALLOCATION_EXCESS`; `SourceType SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` belum digali lawan jurnalnya — `FIN-OQ-018` | Ditemukan lewat impact scan 25 September 2026 |
| `FIN-CAP-024` | Selisih kas shift kasir yang sudah disahkan (`FIN-DEC-043`, gerbang G6) | Billing (titik sentuh Finance) | `Areas/HealthServices/BillingManagement/Cashier/Models/BilCashVarianceReview.cs#BilCashVarianceReview@d6cdfaf9` — `ShiftId`, `ReviewerId`, `Variance`, `Resolution`, `Reason`, `ReviewedAt`; dibuat `CashierShiftService.cs` baris 613 saat `BilCashierShift.Status` diset `REVIEWED` | `Ready to reuse` (read-only) | — | **Lebih presisi dari yang dicatat `FIN-DEC-043`:** `SourceTransactionId` kejadian `SELISIH-KAS-SHIFT` sebaiknya memakai `BilCashVarianceReview.Id`, BUKAN `BilCashierShift.Id` — baris inilah yang benar-benar merupakan fakta "selisih disahkan", lengkap dengan `Reason`/`Resolution`/`ReviewerId`. Perlu dikonfirmasi ulang saat `/design-business-module`, bukan mengubah keputusan bisnisnya |
| `FIN-CAP-025` | Layanan intake Billing→Finance yang sudah berjalan (`FIN-SC-001`, dasar `FIN-DEC-030`..`044`) | Finance | `Areas/Corporate/FinanceManagement/BillingIntake/Services/FinanceBillingIntakeService.cs#SyncNewFactsAsync,ProcessAsync@d6cdfaf9` — menarik `BilCollectionHandoff` berstatus `CREATED` (baris 138-140), membuat `FinReceipt` per handoff (baris 359-381), diberi wewenang eksplisit 22 September 2026 (`BE-FIN-016`) | `Ready to reuse` | `RequiresFinalization`/kode `EventTypeCode` yang dipilih (`PENERIMAAN-KASIR` vs kode baru `PENERIMAAN-UANG-MUKA`) BELUM disesuaikan dengan `FIN-DEC-030` — ini pekerjaan `/design-business-module`, bukan gap kemampuan | Pola sync/poll ini adalah titik yang MUST diubah saat `FIN-DEC-030` diimplementasikan: `SourceInvoiceStatus` pada `BilCollectionHandoff` (field yang sudah ada) adalah sumber kebenaran untuk memilih kode kejadian yang benar |

## 4. Kontrak backend as-is — Petty Cash (satu-satunya rumpun Finance yang sudah dibangun)

### Corporate / Finance Management / Master Data / Petty Cash Category

Base URL kanonik: `api/v1/corporate/finance-management/master-data/petty-cash-categories`
Alias kompatibilitas: `api/v1/health-services/billing-management/master-data/petty-cash-categories`

| Method | Path | Kegunaan |
|---|---|---|
| `GET` | `/filters/metadata` | Ambil metadata filter untuk layar daftar kategori |
| `GET` | `/summary` | Ringkasan jumlah kategori aktif/nonaktif |
| `GET` | `/` | Daftar kategori dengan filter/paging |
| `GET` | `/options` | Daftar opsi untuk dropdown (dipakai form voucher) |
| `GET` | `/{id}` | Detail satu kategori |
| `POST` | `/` | Buat kategori baru |
| `PUT` | `/{id}` | Perbarui kategori |
| `PATCH` | `/{id}/status` | Aktifkan/nonaktifkan kategori |
| `DELETE` | `/{id}` | Hapus kategori — ditolak bila sudah dipakai voucher (aturan bisnis, bukan gap teknis) |

### Corporate / Finance Management / Petty Cash / Budget

Base URL kanonik: `api/v1/corporate/finance-management/petty-cash/budget`
Alias kompatibilitas: `api/v1/health-services/billing-management/petty-cash/budget`

| Method | Path | Kegunaan |
|---|---|---|
| `GET` | `/current` | Saldo dan status periode aktif saat ini |
| `GET` | `/overview` | Ringkasan plafon, saldo, total top-up, total pencairan |
| `GET` | `/periods` | Daftar periode dengan filter |
| `GET` | `/movements` | Daftar mutasi dengan filter |
| `POST` | `/top-ups` | Tambah saldo (butuh `Idempotency-Key`) |
| `POST` | `/adjustments` | Koreksi manual saldo (butuh `Idempotency-Key`) |
| `POST` | `/periods` | Buat periode baru berstatus `DRAFT` |
| `POST` | `/periods/{id}/activate` | Aktifkan periode |
| `POST` | `/periods/{id}/close` | Tutup periode, opsional carry-forward |

Seluruh endpoint di atas terverifikasi ADA di source pada `09101d05`, bukan hanya diklaim
dokumen. Belum diverifikasi pada pass ini: isi persis request/response DTO per endpoint (butuh
pembacaan `Dtos/*.cs` terpisah bila dibutuhkan saat desain), dan daftar `[AccessPermission]`
per action.

## 5. Kontrak frontend as-is — Petty Cash

| Rute | Status | Catatan |
|---|---|---|
| `/finance/master-data/petty-cash-category` (+`[slug]`, `/update`, `/create`) | `Reuse with adapter` | Halaman nyata, tapi client component masih mewarisi pola dari versi legacy |
| `/finance/petty-cash-budget` | `Reuse with adapter` | 687 baris, memanggil hook `use-petty-cash-budget`, `use-petty-cash-budget-periods`, `use-petty-cash-overview` yang masih berada di path `billing-management` |
| `/finance/master-data/petty-cash-budget` | `Ready to reuse` | Hanya redirect stub ke `/finance/petty-cash-budget`, untuk kompatibilitas tautan lama |
| `/health-services/billing-management/petty-cash/*` (voucher, budget, kategori) | `Ready to reuse` (legacy) | Implementasi asli lengkap, termasuk seluruh modal (top-up, adjust, close period, create/reverse voucher) |
| Redux slice `petty-cash-budget-slice.jsx`, `petty-cash-voucher-slice.jsx`, `master-data-petty-cash-category-slice.jsx` | `Ready to reuse` | Seluruhnya masih di `src/lib/state/slice/health-services/billing-management/` — dipakai bersama oleh rute lama dan rute kanonik baru |

## 6. Trace journey end-to-end (Billing → Finance → Accounting)

```
BilTender SUCCEEDED (Ready to reuse)
  -> BilPaymentAllocation (Ready to reuse)
  -> BilCollectionHandoff (MISSING — FIN-CAP-007)
       -> konsumen Finance (MISSING — FIN-CAP-008)
            -> FinReceipt (MISSING — FIN-CAP-009)
                 -> FinAccountingEventOutbox (MISSING — FIN-CAP-009)
                      -> endpoint Accounting Event (MISSING — FIN-CAP-018)

BilArHandoff/BilApHandoff CREATED (Ready to reuse)
  -> konsumen Finance (MISSING — FIN-CAP-008)
       -> FinReceivable/FinPayable (MISSING — FIN-CAP-009)

FinPettyCashBudgetMovement (Ready to reuse, backend penuh)
  -> FinAccountingEventOutbox (MISSING)
       -> endpoint Accounting Event (MISSING — FIN-CAP-018)
```

**Bacaan rantai ini:** setiap mata rantai di sisi Billing dan Accounting-sebagai-rujukan sudah
solid dan tidak butuh kerja tambahan untuk *dibaca*. Satu-satunya rumpun yang punya
implementasi Finance nyata (Petty Cash) juga berhenti sebelum Accounting — tidak ada satu pun
baris kode yang menghasilkan kejadian ke Accounting hari ini. Ini konsisten dengan
`FIN-DEC-004`/`FIN-DEC-008`: publikasi memang sengaja ditahan sampai kontrak diratifikasi
(baru terjadi pada pass wawancara ini) dan endpoint Accounting tersedia.

## 7. Fact, inference, dan rekomendasi

**Fact** (terverifikasi langsung dari source, bukan dari dokumen owner):

- `BilArHandoff.DebtorType` hanya punya dua nilai hari ini: `PAYER`, `PATIENT_GUARANTOR`.
- `BillingHandoffStatuses` hanya punya `CREATED`/`ACKNOWLEDGED` — tidak ada status "piutang
  tertagih"/collected (temuan yang sama seperti dicatat billing-kasir §21, `BKC-CQ-01`).
- Tidak ada satu pun entity `Fin{Receivable,Payable,Receipt}*` atau `BilCollectionHandoff` di
  seluruh backend.
- Endpoint penerima Accounting Event belum ada di `Areas/Corporate/AccountingManagement`.
- Base path kanonik Finance memakai tanda hubung (`finance-management`), berbeda dari yang
  tertulis di `FIN-PRD-V2-0.3`.
- Route Petty Cash FE kanonik (`/finance/...`) sudah ada dan real, bukan placeholder.

**Inference** (kesimpulan wajar dari fact di atas, bukan fact itu sendiri):

- Karena `BillingHandoffStatuses` belum punya status "tertagih", `FIN-DEC-005` (kontrak
  `BilCollectionHandoff` terpisah dari `BilArHandoff`) kemungkinan besar TIDAK bisa
  diimplementasikan sebagai perluasan `BilArHandoff` yang sudah ada — perlu tabel baru murni,
  sesuai yang sudah dipilih owner, bukan alternatif "tambah status baru di enum existing".
- Migrasi FE Petty Cash ke rute kanonik kemungkinan besar adalah pekerjaan "ganti import
  hook/slice", bukan "bangun ulang UI", mengingat halaman `/finance/petty-cash-budget` sudah
  687 baris fungsional yang hanya menunjuk ke lokasi hook lama.

**Rekomendasi** (bukan keputusan — tetap wewenang owner saat `/design-business-module`):

- Saat desain AR dimulai, mulai dari perluasan `BilArHandoff` (`FIN-DEC-006`) dan definisi
  `BilCollectionHandoff` (`FIN-DEC-005`) lebih dulu sebagai kontrak Billing, baru desain
  `FinReceivable`/`FinReceipt` di sisi Finance — supaya bentuk konsumen mengikuti bentuk
  sumber yang sudah pasti, bukan sebaliknya.

## 8. Reuse dari blueprint lain — ringkasan keputusan

| Blueprint lain | Yang dikutip | Kenapa tidak diaudit ulang |
|---|---|---|
| `billing-kasir` (`BIL-CASH-001`, approved, revisi 1.3) | Seluruh entity `Bil*` yang jadi sumber Finance | `git diff` 29a7a7ad..HEAD atas folder yang relevan kosong; blueprint itu sendiri sudah diperbarui pemilik yang sama pada commit yang sama dengan audit terakhirnya |
| `accounting` (`ACC-BP-001`, approved, revisi 11) | Keberadaan master EventType/PostingRule/COA/Period/Journal, dan status endpoint Accounting Event | `FIN-OOS-002` — Accounting bukan scope modul ini; endpoint inbox diverifikasi ulang langsung (bukan sekadar dikutip) karena itu titik sentuh kritis |

## 9. Unknown dan closure questions

| ID | Pertanyaan | Kenapa penting | Memblokir |
|---|---|---|---|
| `FIN-CQ-01` | Petty Cash punya kode sumber di `Corporate/FinanceManagement`, tetapi keputusan bisnis/arsitekturnya (`PC-DEC-*`/`PC-DES-*`) tercatat di blueprint **billing-kasir**, bukan di blueprint **finance-management** ini. Apakah Petty Cash TETAP didokumentasikan di billing-kasir (hanya dirujuk dari sini), atau dipindah/diduplikasi ke blueprint finance-management? | Tanpa keputusan ini, perubahan bisnis Petty Cash berikutnya berisiko ditulis dua kali di dua blueprint berbeda dan saling tidak sinkron | `DESIGN` untuk Cash Management (`FIN-SC-005`) — tidak memblokir AR/AP |
| `FIN-CQ-02` | DTO/response shape persis dan daftar `[AccessPermission]` per endpoint Petty Cash belum dibaca detail pada pass ini — cukup untuk memastikan "ada", belum cukup untuk jadi acuan desain endpoint AR/AP yang konsisten | Konsistensi pola response/permission antar rumpun Finance | ~~`DESIGN`~~ — **DITUTUP 20 September 2026** oleh pass `/design-business-module`, yang membaca langsung `PettyCashCategoriesController.cs`, `PettyCashBudgetController.cs`, `PettyCashBudgetDtos.cs`, `PettyCashBudgetService.cs`, dan `FinPettyCashBudgetMovementConfiguration.cs`. Polanya tercatat di `02-backend-architecture.md` bagian 2.2 (`FIN-DES-004`..`007`) |
| `FIN-CQ-03` | Rumpun piutang manfaat karyawan menyentuh HR (identitas pemilik manfaat dan eligibilitas), tetapi `FIN-DEC-006` dan `FIN-DEC-016` keduanya bertanda "butuh konfirmasi Billing + HR" yang belum turun. Apakah kedua owner menyetujui perluasan `BilArHandoff` beserta pembagian tanggung jawab penentuan identitasnya? | Ini satu-satunya titik modul yang requirement-nya belum pernah dinilai pemilik domainnya. Dibuka pada pass desain 20 September 2026 sebagai konsekuensi tercatat dari `DOMAIN_ARCHITECTURE_NOT_RUN` | `DESIGN` untuk `EPIC FIN-04` saja — epic itu sudah ditandai `OPEN DECISION` dan dikeluarkan dari seluruh gelombang pengiriman, sehingga **tidak** memblokir MVP |

**Tidak ada Conflict.** Tidak ditemukan dua sumber bukti yang saling bertentangan pada pass ini.

## 9.1 Pembaruan pada pass desain — 20 September 2026

Pass `/design-business-module` menambahkan dua kemampuan hasil pemeriksaan terarah
(`FIN-CAP-020`, `FIN-CAP-021`), menutup `FIN-CQ-02`, dan membuka `FIN-CQ-03`. Bagian 1 sampai 8
di atas **tidak** diaudit ulang; SHA kedua repository tidak bergerak dari `09101d05` /
`abed49b03`.

Temuan yang paling mengubah rencana: **modul Medical Fee belum ada satu baris pun**
(`FIN-CAP-021`). Karena `FinDoctorPayable` memakai `SourceDoctorServiceFeeId` sebagai kunci
idempotensi, rumpun utang dokter bukan sekadar "ditunda karena prioritas" — ia **tertahan
ketergantungan eksternal keras** dan tidak dapat dimulai sampai modul lain membangunnya.

## 9.2 Conflict — `MstDoctorServiceRule` bukan aturan perhitungan fee

**Ini satu-satunya `Conflict` pada peta ini, dan ditemukan terlambat** — pencatatan pertama pass
desain 20 September 2026 keliru menandainya `Ready to reuse`. Koreksinya dicatat di sini apa
adanya, bukan dihapus.

| Sumber | Yang dinyatakan |
|---|---|
| `FIN-PRD-V2-0.3` bagian 8 | "`MstDoctorServiceRule` — Versioned effective rule, **calculation type/value/base**, role/service context" |
| Source `09101d05` | `MstDoctorServiceRule` berisi `IsAllowWalkIn`, `IsAllowAppointment`, `IsAllowKioskRegistration`, `IsAllowTelemedicine`, `IsNeedReferral`, `IsNeedApproval`, `IsPrimaryForClinic`, `DailyQuotaLimit`, `PriorityLevel`, `EffectiveStartDate`/`EndDate`. Enum `DoctorServiceRuleType` = `GeneralService`, `Consultation`, `Procedure`, `TariffCategory`, `Tariff`, `MedicalCheckup`, `Telemedicine` |

Entity yang ada di source adalah **aturan kelayakan dan penjadwalan layanan dokter** — dokter
mana boleh menerima pasien walk-in, berapa kuota hariannya, apakah perlu rujukan. Ia
**tidak memiliki satu pun** field nominal, persentase, atau basis perhitungan. `TariffId` dan
`TariffCategoryId` di dalamnya menyatakan *cakupan berlakunya aturan*, bukan nilai fee.

Tiga akibat yang MUST diperhatikan:

1. **Aturan perhitungan jasa medis belum ada sama sekali di source.** Modul Medical Fee lebih
   kosong daripada yang tercatat sebelumnya: bukan hanya hasil fee (`DoctorServiceFee`) yang
   belum ada, aturan perhitungannya pun belum ada.
2. **Nama `MstDoctorServiceRule` sudah terpakai untuk konsep lain.** Modul Medical Fee
   **MUST NOT** memakai nama itu untuk aturan perhitungan fee. Memakainya akan menabrak entity
   milik Health Services yang sudah berjalan.
3. **Rencana Finance tidak berubah.** `FinDoctorPayable` tetap bergantung pada
   `DoctorServiceFee` yang sudah disetujui, bukan pada aturan perhitungannya. Rumpun utang
   dokter memang sudah `POST-MVP` dan sudah ditandai tertahan ketergantungan eksternal, jadi
   koreksi ini tidak menggeser satu pun gelombang.

Temuan ini diteruskan ke owner sebagai bahan modul Medical Fee, bukan sebagai pekerjaan Finance.

## 9.3 Impact scan lanjutan — 25 September 2026

**Pemicu:** backend SHA bergerak `09101d05` → `d6cdfaf9` (branch `Yasmina` tetap sama). Pass
`/design-business-module` yang mengikuti Amendment pass kejadian keuangan menemukan drift ini
secara tidak sengaja saat memeriksa dasar `FIN-DEC-032`/`033`, lalu owner meminta verifikasi
formal lewat `trace-existing-capabilities` sebelum blocker `BILLING-COLLECTION-HANDOFF` ditutup.

**Batas audit pass ini** (BUKAN audit penuh ulang seluruh peta): klaster `External Integration`
titik sentuh `BilCollectionHandoff`, dan tiga entity Billing baru yang jadi dasar `FIN-DEC-040`
sampai `FIN-DEC-044` (deposit, kelebihan bayar, selisih kas shift). Rumpun AR/AP/Payable Finance
yang lain (`FinReceivable`, `FinSupplierPayable`, `FinMedicalServicePayable`, `FinPayment`, dsb.)
TERKONFIRMASI ADA (lihat `git diff --stat 09101d05..d6cdfaf9 -- Areas/Corporate/FinanceManagement/`
— 59 berkas berubah, 9062 baris ditambahkan) tetapi TIDAK diaudit field-per-field pada pass ini,
karena di luar scope amendment yang sedang berjalan. Butir ini dicatat sebagai audit yang masih
harus dilakukan sebelum peta ini dipakai sebagai acuan lengkap AR/AP.

### Temuan utama

1. **`blocking_questions.BILLING-COLLECTION-HANDOFF` pada `blueprint-manifest.md` GUGUR.**
   `BilCollectionHandoff` bukan sekadar ada di database — ia sudah **dikonsumsi penuh** oleh
   `FinanceBillingIntakeService` (`SyncNewFactsAsync`/`ProcessAsync`, wewenang `BE-FIN-016`
   22 September 2026) yang membuat `FinReceipt` dari setiap handoff berstatus `CREATED`.
   Bentuknya PERSIS memenuhi permintaan `FIN-DEC-005`: tabel handoff persisted, pola sama dengan
   `BilArHandoff`/`BilApHandoff`, dikunci `TenderId` (lewat `HandoffKey` deterministik). Field
   `SourceInvoiceStatus` yang sudah ada di dalamnya adalah persis sumber kebenaran yang
   dibutuhkan untuk memilih `PENERIMAAN-UANG-MUKA` vs `PENERIMAAN-KASIR` (`FIN-DEC-030`).
2. **`FIN-CAP-007`, `008`, `009` yang dulu `Missing` sekarang `Ready to reuse`.** Rumpun intake
   Billing→Finance sudah berjalan ujung ke ujung sampai `FinReceipt`, bukan lagi rencana kosong.
   Diagram trace journey di bagian 6 dokumen ini (`BilCollectionHandoff MISSING → ...`) **sudah
   usang** dan perlu digambar ulang saat `/design-business-module` berikutnya — tidak digambar
   ulang di sini karena itu keluaran desain, bukan keluaran audit.
3. **Tiga entity baru untuk gerbang G6 ditemukan dan diverifikasi** (`FIN-CAP-022`..`024`):
   deposit pasien, kelebihan bayar, dan selisih kas shift — seluruhnya `Ready to reuse` sebagai
   sumber baca, TIDAK PERLU handoff baru dari Billing. Satu koreksi presisi ditemukan:
   `SELISIH-KAS-SHIFT` sebaiknya memakai `BilCashVarianceReview.Id` (bukan `BilCashierShift.Id`)
   sebagai `SourceTransactionId`, karena baris itulah yang membawa `Reason`/`Resolution`/
   `ReviewerId` — bukan perubahan keputusan bisnis `FIN-DEC-043`, hanya kejelasan bukti untuk
   desain berikutnya.
4. **`FIN-CAP-018` (endpoint penerima Accounting Event) TIDAK BERUBAH — tetap `Missing`.**
   Diverifikasi ulang langsung: sepuluh controller `Areas/Corporate/AccountingManagement/**`
   pada `d6cdfaf9` seluruhnya route `api/v1/corporate/accounting/...` yang sudah ada sebelumnya
   (period, ledger, journal, COA, event-type, posting-rule, reconciliation, recurring-journal,
   year-end-closing) — tidak satu pun cocok pola penerima kejadian. Konsisten dengan pengakuan
   Rizki sendiri di gerbang G1 ("belum ada kodenya").

### Fact tambahan (terverifikasi langsung, bukan dari dokumen)

- `Repositories/ApplicationDbContext.cs` baris 604-611 mendaftarkan `DbSet<BilDepositAccount>`,
  `DbSet<BilDepositMovement>`, `DbSet<BilRefundableCredit>`, `DbSet<BilRefundCase>`,
  `DbSet<BilRefundLine>` — semuanya sudah bisa dibaca lewat `ApplicationDbContext` biasa.
- `Repositories/ApplicationDbContext.cs` baris 623 mendaftarkan `DbSet<BilCashVarianceReview>`.
- `git diff --stat 09101d05..d6cdfaf9` untuk `BilDepositAccount.cs`/`BilDepositMovement.cs`/
  `BilCashierShift.cs` **kosong** — ketiganya sudah ada sejak SEBELUM `09101d05`, hanya belum
  masuk boundary audit capability map yang pertama (di luar `FIN-SC-001`..`007` versi awal).
  `BilCollectionHandoff.cs` (91 baris) dan penyesuaian kecil `BilRefundCase.cs`/
  `BilRefundableCredit.cs` (7 baris) BENAR-BENAR baru sejak `09101d05`.

### Yang TIDAK ditutup pass ini

- Audit field-per-field rumpun AR/AP/Payable Finance yang sudah dibangun (`FinReceivable`,
  `FinSupplierPayable`, dst.) — di luar boundary amendment saat ini, direkomendasikan sebagai
  pass `trace-existing-capabilities` terpisah sebelum blueprint dipakai acuan penuh lagi.
- Update diagram trace journey bagian 6 dan ringkasan eksekutif bagian 2 — keduanya masih
  mencerminkan keadaan `09101d05` dan sengaja dibiarkan sebagai jejak sejarah; pembaca MUST
  membaca 9.3 sebagai koreksi yang berlaku, bukan menganggap bagian 2/6 terkini.

## 10. Staleness dan impact-scan trigger

Peta ini **stale** dan wajib impact-scan ulang (terbatas pada bagian yang relevan) bila salah
satu terjadi:

- Backend SHA `NewQuilvianSystemBackend`/`Yasmina` bergerak dari `d6cdfaf9` (SHA impact scan
  terakhir, bagian 9.3) — bukan lagi `09101d05`.
- Frontend SHA `QuilvianSystemFrontendDev` bergerak dari `abed49b03`.
- `docs/module-blueprints/billing-kasir/blueprint-manifest.md` menaikkan `backend_commit_sha`
  di atas titik yang sudah diverifikasi pass ini (audit ulang `git diff` terhadap folder
  `BillingManagement`/`FinanceManagement`/`AccountingManagement` wajib diulang, bukan
  diasumsikan tetap kosong).
- `docs/module-blueprints/accounting/blueprint-manifest.md` mencatat endpoint Accounting Event
  baru dibangun (ubah status `FIN-CAP-018` dari `Missing`).
- Rumpun AR/AP/Payable Finance (`FinReceivable`, `FinSupplierPayable`, dst.) dipakai sebagai
  acuan desain — audit field-per-field belum dilakukan, lihat "Yang TIDAK ditutup" pada 9.3.

## 11. Handoff

Lanjutkan `/grill-me` (Closure pass) untuk klaster AR/AP/Cash Management (`FIN-OQ-002`..`005`
pada `00-interview-decisions.md`), memakai peta ini sebagai dasar sehingga pertanyaan yang
jawabannya sudah ada di source (mis. field apa yang tersedia di `BilArHandoff`) tidak ditanyakan
ulang. `FIN-CQ-01` sebaiknya diajukan sebagai pertanyaan pertama pada Closure pass tersebut,
karena menentukan DI MANA keputusan Cash Management berikutnya harus dicatat.

---

## 12. Audit rumpun BARU — Purchasing/AP, AR Invoice Agregat, Potongan AR (25 September 2026)

**Pemicu:** evidence eksternal `Keuangan.md` (analisis video sistem rujukan, bukan sumber
otoritatif) memicu `/grill-me` amendment pass yang menghasilkan tiga scope baru — `FIN-SC-008`
(Purchasing/AP siklus penuh), `FIN-SC-009` (AR Invoice Agregat ke Company/Guarantor), `FIN-SC-010`
(Potongan sisi penerimaan piutang) — dan keputusan bisnis `FIN-DEC-045` s.d. `FIN-DEC-055`,
seluruhnya `approved` 25 September 2026.

**Batas audit pass ini:** ketiga scope baru di atas, PLUS verifikasi staleness terhadap SHA
audit sebelumnya (`d6cdfaf9`, bagian 9.3). **BUKAN** audit ulang rumpun AR/AP/Payable yang
sudah dibangun (`FinReceivable`, `FinSupplierPayable`, dst. — bagian 9.3 sudah mencatat ini
sebagai pekerjaan tersisa; pass ini MENUTUP sebagian dari catatan itu sejauh relevan dengan tiga
scope baru, tidak mengaudit seluruhnya field-per-field).

**Verifikasi staleness.** Backend SHA bergerak `d6cdfaf9` → `96bf9746` (45 commit, branch
`Yasmina` tetap sama, fast-forward — bukan diverged). `git diff --stat d6cdfaf9..96bf9746 --
Areas/Corporate/FinanceManagement Areas/Administrator/MasterData/Models/MstSupplier.cs
Areas/HealthServices/BillingManagement` menunjukkan **nol** perubahan pada
`Areas/Corporate/FinanceManagement/Payable/`, `MstSupplier.cs`, maupun area Purchasing manapun
— seluruh 45 commit itu murni pada `BillingPayerEditService.cs`, `BillingRefundService.cs`,
`BillingRefundDtos.cs`, `CashierShiftService.cs`/`Dtos`/`Controller` (kebetulan persis pekerjaan
`BE-BUI-001`/`002` pada blueprint `billing-kasir` revisi 1.6 — tidak bersinggungan dengan audit
ini). **Peta ini TIDAK stale untuk ketiga scope baru.** SHA audit pass ini: `96bf9746`.

### 12.1 Tabel bukti kemampuan

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-026` | Ledger utang supplier existing (`FIN-SC-008`, dasar `FIN-DEC-045`) | Finance | `Areas/Corporate/FinanceManagement/Payable/Models/FinSupplierPayable.cs#FinSupplierPayable@96bf9746` — `PayableNumber`, `SupplierId`→`MstSupplier`, `SupplierInvoiceNumber`+`SupplierInvoiceDate` (penjaga input ganda), `OriginalAmount`/`OutstandingAmount`/`PaidAmount`/`AdjustedAmount` (invariant seimbang, pola sama `FinReceivable`), `PaymentTermDays` disalin saat dibuat, `Status` (`OUTSTANDING`/`PARTIAL`/`PAID`/`CANCELLED`) | `Extend` | Dipicu manual (`SupplierInvoiceNumber` diketik staf). `FIN-DEC-045` mengalihkan pemicunya ke Purchasing Invoice yang sudah disetujui — struktur field TETAP relevan, hanya SUMBER pembuatannya yang berubah dari input manual menjadi turunan otomatis Purchasing Invoice | Invariant `OriginalAmount = OutstandingAmount + PaidAmount + AdjustedAmount` MUST tetap dijaga saat sumbernya berubah |
| `FIN-CAP-027` | Master Supplier untuk kebutuhan Purchasing (`FIN-SC-008`, `Keuangan.md` bagian 9.2) | Administrator (titik sentuh Finance, `FIN-DEC-014`) | `Areas/Administrator/MasterData/Models/MstSupplier.cs#MstSupplier@96bf9746` — SUDAH memiliki `PaymentTermDays`, `LeadTimeDays`, `TaxPercent`, `IsTaxable`, `MinimumPurchaseAmount`, `CreditLimitAmount`, `IsPreferredSupplier`, `IsBlacklisted`+`BlacklistReason` | `Ready to reuse` | **Lebih lengkap dari dugaan awal** — TOP/lead time/PPN/diskon yang disebut `Keuangan.md` SUDAH ADA, bukan gap. Field yang benar-benar belum ada: nomor rekening lebih dari satu (multi-bank), kontak PIC pengadaan terpisah dari kontak umum | Klaim awal (sebelum pass ini) bahwa master ini "belum punya TOP/PPN" **keliru** — dikoreksi di sini berbasis baca langsung |
| `FIN-CAP-028` | Purchase Order, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice (`FIN-SC-008`) | — (belum ada pemilik) | Pencarian `PurchaseOrder\|GoodsReceipt\|TukarFaktur\|PurchasingInvoice\|InvoiceExchange\|Procurement\|Requisition\|VendorInvoice` (case-insensitive) di seluruh `Areas/` — 6 hasil, SELURUHNYA false-positive (enum `DrugStockEnums`, modul Rekrutmen HR `TrxJobRequisition*` — soal lowongan kerja, bukan pengadaan barang) | `Missing` | Empat entity inti rumpun Purchasing/AP (`FIN-DEC-045`, `050`, `051`) — nol baris kode, nol tabel, nol endpoint | Rumpun terbesar pada amendment ini; MUST dirancang penuh dari nol di `/design-business-module` |
| `FIN-CAP-029` | Retur Pembelian & Deposit Retur (`FIN-SC-008`, `FIN-DEC-047`) | — (belum ada pemilik) | Pencarian `Retur.*Pembelian\|PurchaseReturn\|SupplierReturn\|ReturDeposit` di seluruh `Areas/` — nol hasil | `Missing` | Entity kredit lintas-invoice (`FIN-DEC-047`) belum ada. **Pola terdekat untuk dipakai acuan struktur** (bukan reuse data): `BilRefundableCredit` milik Billing (`FIN-CAP-023`) — `SourceType`/`OriginalAmount`/`AvailableAmount`/`Status Available-Exhausted` — arah aliran uang berlawanan (kredit DARI supplier, bukan KE pasien), sehingga MUST jadi entity baru di Finance, bukan tabel yang dipakai ulang | — |
| `FIN-CAP-030` | Dokumen "Lembar Tagihan Penjamin Perusahaan" per-invoice (`FIN-SC-009`, titik sentuh `FIN-DEC-048`) | Billing | `Areas/HealthServices/BillingManagement/Billing/Services/BillingCompanyGuarantorInvoiceDocumentService.cs#GetDocumentAsync@96bf9746` (390 baris) + `BillingCompanyGuarantorInvoiceDtos.cs` — endpoint `GET /invoices/{id}/company-guarantor-invoice-document`, murni baca (`AsNoTracking`), SATU invoice per panggilan, `DocumentNumber = invoice.InvoiceNumber` (TANPA nomor seri baru — CAP-41 milik `billing-kasir`), rincian per baris yang ditanggung penjamin, rute reimbursement (`RouteType SELF/INSURANCE_PROVIDER`) | `Reuse with adapter` | **BUKAN duplikat `FIN-DEC-048`.** Ini render PER-INVOICE (satu pasien, satu kunjungan), sedangkan `FIN-DEC-048` butuh AGREGASI lintas banyak invoice/pasien untuk satu penjamin satu periode. Yang bisa dipakai ulang: struktur data `CompanyGuarantorInvoicePayerResponse` (identitas penjamin, rute reimbursement) sebagai RINCIAN di balik AR Invoice Agregat — persis konsisten dengan `FIN-DEC-048` sendiri ("`BilInvoice` individual TETAP ada... sebagai rincian") | **Wajib dibaca sebelum desain `/design-business-module`** — pola nomor dokumen (pakai nomor invoice apa adanya) TIDAK bisa ditiru untuk AR Invoice Agregat karena satu dokumen menaungi BANYAK invoice; MUST punya nomor seri sendiri |
| `FIN-CAP-031` | Kunci pengelompokan penjamin pada `FinReceivable` (`FIN-SC-009`) | Finance | `Areas/Corporate/FinanceManagement/Receivable/Models/FinReceivable.cs#FinReceivable@96bf9746` — `DebtorType` (`PAYER`/`PATIENT_GUARANTOR`, dari `BillingArDebtorTypes`) + `DebtorReferenceId` (polimorfik, bukan FK — lintas bounded context) | `Ready to reuse` | Mengelompokkan `FinReceivable` yang `DebtorType="PAYER"` + `DebtorReferenceId` sama dalam satu periode SUDAH BISA dilakukan dengan query biasa atas kolom yang sudah ada — **nol perubahan skema `FinReceivable` diperlukan** untuk mengidentifikasi kandidat pengelompokan | Ini kabar baik signifikan: fondasi pengelompokan AR Invoice Agregat sudah tersedia, yang belum ada murni lapisan AGGREGATE ROOT baru (`FIN-CAP-032`) |
| `FIN-CAP-032` | Aggregate root AR Invoice Agregat (`FIN-SC-009`, `FIN-DEC-048`) | — (belum ada pemilik) | Pencarian `Consolidat\|Agregat\|GroupInvoice\|InvoiceGroup\|BatchInvoice\|CompanyInvoice\|GuarantorInvoice` (case-insensitive) — 10 hasil, seluruhnya false-positive KECUALI `BillingCompanyGuarantorInvoiceDocumentService`/`Dtos` (`FIN-CAP-030`, per-invoice, bukan agregat) | `Missing` | Entity header baru (nomor tagihan resmi tersendiri, periode penagihan, `DebtorReferenceId`, koleksi `FinReceivable` yang dinaunginya, status siklus penagihan) belum ada sama sekali | — |
| `FIN-CAP-033` | Pola potongan pembayaran sisi Payable, sebagai acuan struktur (`FIN-SC-010`, `FIN-DEC-049`) | Finance | `Areas/Corporate/FinanceManagement/Payable/Models/FinPaymentDeduction.cs#FinPaymentDeduction@96bf9746` — `PaymentId`→`FinPayment`, `DeductionType` (6 nilai tetap + `OTHER`), `Direction` (`DEDUCTION`/`ADDITION`), `Amount` selalu positif, `Reason` wajib bila `OTHER`, tiga check constraint (`Direction`, `Amount>0`, `Type`) | `Ready to reuse` (sebagai pola struktur, BUKAN data yang sama) | Arah aliran uang berlawanan — `FinPaymentDeduction` mengurangi TRANSFER KELUAR tanpa mengurangi utang lunas (`FIN-DES-028`); entity AR baru (`FIN-DEC-049`/`055`) mengurangi `OutstandingAmount` piutang. Struktur field (`DeductionType`, `Direction`, `Amount`, `Reason`) langsung dapat ditiru; hubungan ke aggregate root (`FinReceipt`/`FinReceiptAllocation`, bukan `FinPayment`) dan invariant-nya (MENGURANGI `OutstandingAmount`, bukan sekadar `NetTransferAmount`) HARUS berbeda | `FIN-DES-028` (potongan Payable TIDAK mengurangi utang) dan `FIN-DEC-055` (potongan AR MENGURANGI piutang) sengaja berlawanan arah — desain MUST NOT menyalin invariant `FinPaymentDeduction` mentah-mentah ke entity baru ini |
| `FIN-CAP-034` | Potongan/withholding tax sisi penerimaan (`FIN-SC-010`) | — (belum ada pemilik) | Pencarian `Withhold\|WithholdingTax\|PPh23\|PPh 23\|TaxWithheld` (case-insensitive) di seluruh `Areas/` — **nol hasil** | `Missing` | Entity baru sepenuhnya — dikonfirmasi genuinely tidak ada precedent data apa pun, hanya precedent struktur (`FIN-CAP-033`) | — |
| `FIN-CAP-035` | Mekanisme approval berjenjang berdasarkan nominal, SUDAH BERJALAN (`FIN-SC-008`, dasar pola untuk `FIN-DEC-050`) | Finance | `Areas/Corporate/FinanceManagement/Payable/Services/FinancePaymentService.cs#ResolveApprovalTier@96bf9746` (baris 649-659) + `FinPayment.ApprovalTier` (`Models/FinPayment.cs` baris 58) + `ApprovalTiers.Tier1`/`Tier2` (konstanta string) | **`Ready to reuse` (pola) — TEMUAN PALING PENTING pass ini** | **Placeholder ambang yang SUDAH DITULIS di kode adalah `<= Rp 50.000.000 → Tier1, > Rp 50.000.000 → Tier2` — PERSIS SAMA dengan angka yang baru disepakati `FIN-DEC-052`.** Komentar kode menyebutnya eksplisit "Placeholder ambang nominal provisional (`FIN-OQ-010`)" — kini `FIN-OQ-010` sudah `closed`, sehingga placeholder ini BUKAN LAGI provisional, melainkan SUDAH BENAR tanpa perlu diubah nilainya. `ResolveApprovalTier(paymentType, totalAmount)` — pola switch dua tier ini langsung dapat ditiru untuk approval PO/Purchasing Invoice (`FIN-DEC-050`), method baru terpisah dengan nominal sumber yang berbeda (nilai PO/Invoice, bukan total pembayaran) | Komentar `FinancePaymentsController.cs` baris 24-28 dan `FinPayment.cs` baris 18 MUST diperbarui saat implementasi — kata "provisional"/"belum diratifikasi" sudah tidak akurat sejak `FIN-DEC-052` |
| `FIN-CAP-036` | Konvensi penamaan `[AccessController]`/route untuk resource Payable baru (`FIN-SC-008`) | Finance | `Areas/Corporate/FinanceManagement/Payable/Controllers/FinancePaymentsController.cs@96bf9746` — `[AccessController("CORPORATE_FINANCE_MANAGEMENT_PAYMENT", ...)]`, route `api/v1/corporate/finance-management/payments`, `[Tags("Corporate / Finance Management / Payment")]` | `Ready to reuse` (pola) | — | Konvensi ini MUST diikuti persis untuk resource Purchasing/AP baru (`PurchaseOrder`, `PurchasingInvoice`, `TukarFaktur`, dst.) — bukan pola baru yang diciptakan |

### 12.2 Fact, inferensi, rekomendasi

**Fact (terverifikasi baca langsung):**
1. Nol baris kode untuk Purchase Order/Goods Receipt/Tukar Faktur/Purchasing Invoice di seluruh backend — dikonfirmasi dua kali (pass `/grill-me` sebelumnya dan pass audit ini, keduanya di SHA berbeda dengan hasil sama).
2. `MstSupplier` sudah punya `PaymentTermDays`, `LeadTimeDays`, `TaxPercent`, `IsTaxable`, `CreditLimitAmount` — klaim sebelumnya bahwa master ini "belum lengkap" untuk kebutuhan Purchasing **keliru**, dikoreksi `FIN-CAP-027`.
3. `FinancePaymentService.ResolveApprovalTier` sudah memakai ambang **Rp 50.000.000** sebagai placeholder — angka yang sama persis dengan `FIN-DEC-052`.
4. `BillingCompanyGuarantorInvoiceDocumentService` ada dan matang (390 baris), tetapi beroperasi PER-INVOICE, bukan agregat lintas invoice.
5. `FinReceivable.DebtorType`+`DebtorReferenceId` sudah menyediakan kunci pengelompokan yang dibutuhkan AR Invoice Agregat, tanpa perlu perubahan skema `FinReceivable`.

**Inferensi:**
1. Karena ambang `FinancePaymentService` sudah Rp 50 juta dan `FIN-DEC-052` menyepakati angka yang sama untuk KEDUA checkpoint (PO/Invoice dan pembayaran), kemungkinan besar angka ini bukan kebetulan — pola pikir "Rp 50 juta sebagai batas wajar keputusan finansial menengah" konsisten di kedua konteks. Ini MEMPERKUAT keyakinan bahwa `FIN-DEC-052` adalah keputusan yang tepat, bukan sekadar angka acak.
2. Karena `BillingCompanyGuarantorInvoiceDocumentService` sengaja TIDAK memberi nomor seri baru pada dokumennya sendiri (memakai nomor invoice apa adanya), dan `FIN-DEC-048` eksplisit menuntut AR Invoice Agregat py nomor RESMI sendiri, kedua entity ini akan hidup berdampingan dengan pola penomoran yang BERBEDA secara sengaja — bukan inkonsistensi yang perlu diseragamkan.

**Rekomendasi:**
1. **Lanjut ke `/design-business-module`** untuk seluruh `FIN-SC-008`/`009`/`010` — tidak ada `Conflict` yang memblokir, seluruh `Missing` sudah dipetakan jelas batasnya, dan beberapa pola reuse berharga sudah ditemukan (`FIN-CAP-030`, `031`, `033`, `035`, `036`).
2. **Prioritaskan pemakaian ulang `ResolveApprovalTier`** (`FIN-CAP-035`) sebagai referensi literal saat merancang approval PO/Purchasing Invoice — pola sudah teruji, tinggal ditiru dengan sumber nominal berbeda.
3. **Desain AR Invoice Agregat MUST eksplisit menyatakan** field mana yang dipakai ulang dari `CompanyGuarantorInvoiceDocumentResponse` (`FIN-CAP-030`) sebagai rincian, supaya implementer tidak membangun ulang logika perhitungan per-item yang sudah ada di Billing.
4. **`FIN-CAP-029`** (Deposit Retur) MUST dirancang sebagai entity Finance baru, TIDAK mencoba memakai ulang tabel `BilRefundableCredit` milik Billing — arah aliran uangnya berlawanan dan kepemilikan datanya berbeda modul.

### 12.3 Yang TIDAK ditutup pass ini

- Audit field-per-field rumpun AR/AP/Payable Finance yang SUDAH dibangun sebelumnya (`FinReceivable`, `FinReceiptAllocation`, `FinMedicalServicePayable`, dst.) — tetap menjadi utang dari bagian 9.3, TIDAK bertambah maupun berkurang oleh pass ini karena di luar tiga scope baru yang diminta.
- Perincian kolom persis untuk empat entity baru Purchasing/AP (`FIN-CAP-028`) — itu keluaran desain (`/design-business-module`), bukan keluaran audit.
- Ratifikasi Accounting atas kode `PPN-MASUKAN-PEMBELIAN` (`FIN-OQ-020`) — surat sudah terkirim (`evidence/06`), menunggu balasan Rizki, di luar wewenang audit kemampuan source code.

## 13. Staleness dan impact-scan trigger — pembaruan 25 September 2026

Selain pemicu pada bagian 10, peta ini (khusus bagian 12) **stale** dan wajib impact-scan ulang bila:

- Backend SHA bergerak dari `96bf9746` (SHA audit bagian 12) DAN perubahan menyentuh
  `Areas/Corporate/FinanceManagement/Payable/`, `Areas/Administrator/MasterData/Models/MstSupplier.cs`,
  atau `Areas/HealthServices/BillingManagement/Billing/Services/BillingCompanyGuarantorInvoiceDocumentService.cs`.
- `FIN-OQ-020` dijawab Rizki dan kode `PPN-MASUKAN-PEMBELIAN` berubah nama/bentuk — bagian 12.1
  baris `FIN-CAP-034` perlu dicatat ulang begitu entity-nya dirancang.
- Modul Purchasing/Procurement mulai dibangun di luar Finance Management (skenario yang
  SUDAH ditolak `FIN-DEC-045`, tetapi bila kelak dipertimbangkan ulang, `FIN-CAP-028` MUST
  diperiksa ulang).

## 14. Handoff

Lanjutkan `/design-business-module` untuk `FIN-SC-008`/`009`/`010`. Bahan yang sudah tersedia
untuk pass itu: 15 keputusan bisnis (`FIN-DEC-045`..`055`, seluruhnya `approved`), 11 entri
kemampuan baru (`FIN-CAP-026`..`036`), dan tiga temuan reuse bernilai tinggi (`FIN-CAP-030`
dokumen per-invoice, `FIN-CAP-031` kunci pengelompokan, `FIN-CAP-035` mekanisme approval
berjenjang yang sudah teruji dengan angka yang persis cocok). Satu gerbang eksternal tersisa
di luar wewenang audit ini: `FIN-OQ-020` (ratifikasi Accounting atas kode PPN Masukan) — TIDAK
memblokir desain arsitektur, hanya memblokir `/plan-module-delivery` untuk rumpun Purchasing/AP
secara spesifik, sesuai `FIN-DEC-046`.

---

## 15. Impact scan terarah — 28 September 2026 (`cba60cb0` / `49b59cfaa`)

**Pemicu.** `/design-business-module` revisi 6 menemukan kedua SHA sudah bergerak jauh dari
manifest, dan mencatat peta ini **stale** pada tiga titik. Pass ini memverifikasinya langsung ke
source.

**Batas audit pass ini.** Hanya entri yang berpotensi stale akibat pergerakan SHA, ditambah satu
hal baru yang kini dapat diaudit dan sebelumnya tidak bisa: **kontrak as-is endpoint penerima
Accounting**. **BUKAN** audit ulang seluruh peta, dan **BUKAN** penutupan utang audit
field-per-field rumpun AR/AP/Payable dari bagian 9.3 dan 12.3 — utang itu tetap terbuka.

**Verifikasi staleness.** Backend `96bf9746` → `cba60cb0`: **79 commit**, `git merge-base
--is-ancestor` mengonfirmasi **fast-forward**, bukan diverged. Frontend `abed49b03` → `49b59cfaa`:
**59 commit**. Berbeda dari dua impact scan sebelumnya yang menemukan nol perubahan di dalam
boundary, pass ini menemukan perubahan **besar dan tepat di dalam boundary**: seluruh rumpun
Purchasing/AP dibangun, layar Finance AR/AP dibangun, dan endpoint penerima Accounting muncul.

### 15.1 Koreksi status entri yang sudah ada

| ID | Status lama | Status baru | Bukti (`path#symbol@cba60cb0`) | Yang berubah |
|---|---|---|---|---|
| `FIN-CAP-018` | `Missing` | **`Ready to reuse`** | `Areas/Corporate/AccountingManagement/AccountingEvent/Controllers/AccountingEventController.cs` + `Services/AccAccountingEventService.cs` + `Services/AccAccountingEventSchedulerHostedService.cs` | Endpoint penerima **sudah ada**, lengkap dengan penjadwal pemrosesan. Kontrak as-is lengkap di 15.3. Sejalan dengan pernyataan Accounting di `evidence/14` bagian 4.4 |
| `FIN-CAP-028` | `Missing` | **`Extend`** | `Areas/Corporate/FinanceManagement/Purchasing/` — 7 model, 5 controller, 5 service, 5 berkas DTO; `Repositories/ApplicationDbContext.cs` baris 689-695; 11 configuration; migration `20260926100000_AddPurchasingApRumpun` | PO, Tanda Terima Barang, Tukar Faktur, Purchasing Invoice **sudah dibangun**. Yang **belum**: endpoint daftar berpaging (`GET /`) pada **kelima** controller, dan **nol** layar frontend |
| `FIN-CAP-029` | `Missing` | **`Extend`** | `Purchasing/Models/FinSupplierReturn.cs`, `FinSupplierReturnItem.cs`, `FinSupplierReturnDeposit.cs`, `FinSupplierReturnDepositUsage.cs`; `Services/FinanceSupplierReturnService.cs` | Retur dan Deposit Retur **sudah dibangun** dengan bentuk REVISI 5 (`PaymentId`, bukan `PurchasingInvoiceId`). Yang **belum**: pemisahan PPN (`FIN-DES-055`), dan `GET /` berpaging |
| `FIN-CAP-032` | `Missing` | **`Missing`** (dikonfirmasi ulang) | Pencarian `FinReceivableInvoiceBatch` di `Areas/` dan `Repositories/` — **nol hasil** | Tidak berubah. AR Invoice Agregat masih nol baris |
| `FIN-CAP-034` | `Missing` | **`Missing`** (dikonfirmasi ulang) | `find Areas -name "FinReceiptDeduction*.cs"` — **nol hasil**; nol `DbSet`, nol configuration | Tidak berubah. Potongan AR masih nol baris — konsisten dengan `BE-FIN-040` yang masih ⛔ |
| `FIN-CAP-026` | `Extend` | **`Extend`** (bukti diperbarui) | `Payable/Models/FinSupplierPayable.cs@cba60cb0` + migration `20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable` | Kolom `SourcePurchasingInvoiceId` **sudah terpasang** beserta migration-nya — pengalihan pemicu dari input manual ke Purchasing Invoice sudah berjalan sebagian |
| `FIN-CAP-035` | `Ready to reuse` (pola) | **`Ready to reuse`** — **dengan koreksi klaim** | `Payable/Services/FinanceApprovalTierResolver.cs@cba60cb0` | Direfaktor menjadi resolver tunggal yang dipakai bersama pembayaran, PO, dan Purchasing Invoice. **Klaim lama DIKOREKSI** — lihat 15.2 butir 2 |

### 15.2 Dua klaim lama yang DIKOREKSI berbasis bukti baru

**1. Komentar di source sendiri sudah tidak akurat.** `Repositories/ApplicationDbContext.cs`
baris 686-688 dan 696-699 masih menulis *"Migration `AddPurchasingApRumpun` (`BE-FIN-031`) belum
dibuat/dijalankan"*, padahal berkas `Migrations/20260926100000_AddPurchasingApRumpun.cs`
**sudah ada**. Ini cacat dokumentasi **di dalam source**, dicatat sebagai fakta — audit ini
**tidak memperbaiki source**.

**2. `FIN-CAP-035` keliru pada nilai batasnya, dan kekeliruan itu sudah diperbaiki kode.**
Peta ini sebelumnya menyatakan placeholder `<= Rp 50.000.000 → Tier1` **"PERSIS SAMA"** dengan
`FIN-DEC-052` sehingga *"SUDAH BENAR tanpa perlu diubah nilainya"*. Bukti di
`FinanceApprovalTierResolver.cs` menunjukkan sebaliknya: keputusan yang diratifikasi menaruh
**tepat Rp 50.000.000 di `TIER_2`**, sedangkan placeholder lama menaruhnya di `TIER_1`.
Komentar resolver menyebutnya eksplisit — *"salah satu nilai batasnya (tepat Rp 50.000.000) tidak
cocok dengan keputusan yang sudah diratifikasi"*. Kode sekarang `< 50_000_000m → TIER_1`, sisanya
`TIER_2`.

Pelajaran yang layak dicatat: dua angka yang "kelihatan sama" berbeda pada **satu nilai batas**,
dan audit sebelumnya menyatakannya cocok tanpa memeriksa operator pembandingnya.

### 15.3 Kontrak backend as-is — endpoint penerima Accounting Event

Ini bagian paling berharga dari pass ini: kontrak yang selama ini hanya dikutip dari dokumen
Accounting, kini dibaca langsung dari source. **Finance adalah pemanggilnya**, sehingga setiap
aturan di bawah menentukan apakah worker pengiriman Finance akan berhasil atau ditolak.

#### Corporate - Accounting - Accounting Event

Base URL: `api/v1/corporate/accounting/accounting-events`

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Menerima satu kejadian keuangan dari Finance | `AccountingEvent : Receive` | `ReceiveAccountingEventRequest` | `ApiResponse<AccountingEventReceiptDto>` — `201` baru, `200` sudah pernah diterima, `400`, `409`, `422` |
| `POST` | `/{id}/retry` | Mencoba ulang kejadian `Gagal`/`Tertahan` | `AccountingEvent : Retry` | — | `ApiResponse<AccountingEventDetailDto>` |
| `POST` | `/{id}/ignore` | Mengabaikan kejadian gagal | `AccountingEvent : Ignore` | — | `ApiResponse<AccountingEventDetailDto>` |
| `GET` | `/`, `/{id}`, ringkasan | Pantauan sisi Accounting | `AccountingEvent : Read` | — | — |

**Bentuk tanda terima** (`AccountingEventReceiptDto`): `AccountingEventId`, `EventNumber`,
`EventStatus`, `JournalNumber?`, `AccountingPeriodCode?`, `HoldReasonCode?`, `ReceivedAt`.

**Aturan penolakan yang MUST dipatuhi Finance** — seluruhnya dibaca dari
`AccAccountingEventService.cs`:

| # | Kondisi | Kode | Pesan/perilaku |
|---:|---|---|---|
| 1 | Salah satu dari 12 field wajib kosong (termasuk `SourceVersion`, `CorrelationId`, `CausationId`) | `400` | "Pesan kejadian tidak lengkap. Bidang berikut wajib diisi: …" |
| 2 | Ada field tambahan yang namanya mengandung penanda identitas pasien | `400` | "Pesan kejadian tidak boleh memuat identitas pasien." Diperiksa lewat `[JsonExtensionData]` |
| 3 | `CurrencyCode` bukan rupiah | `409` | "Sistem akuntansi hanya menerima rupiah." |
| 4 | Komponen tanpa kode, kode > 50 karakter, atau nilai **negatif** | `400` | Nilai komponen `0` **boleh** — yang ditolak hanya negatif |
| 5 | Kode komponen kembar dalam satu pesan | `400` | "Komponen berikut dikirim lebih dari sekali: …" |
| 6 | **`Amount <= 0` pada pesan yang BUKAN pesan saldo** | **`400`** | **"Nilai kejadian harus lebih besar dari nol."** |
| 7 | Pesan membawa `SubledgerBalance` padahal jenisnya terdaftar sebagai jenis biasa | `400` | Rincian saldo hanya untuk pesan saldo |
| 8 | Jenis kejadian terdaftar sebagai **pesan saldo subledger** | **`409`** | **"Pesan saldo subledger belum dapat diterima. Jalurnya dibangun pada `BE-ACC-P2-028`."** |
| 9 | `LegalEntityId` tidak ada atau tidak aktif | `422` | "Badan hukum tidak ditemukan." |
| 10 | Kombinasi `SourceModule`+`SourceTransactionId`+`EventTypeCode`+`SourceVersion` sudah pernah diterima | `200` | "…sudah pernah diterima. Tidak ada jurnal baru yang dibuat." **Idempoten** |
| 11 | Jenis kejadian belum terdaftar sebagai `AccEventType` | `422` + `Tertahan` | `HoldReasonCode = EVENT_TYPE_NOT_REGISTERED` |
| 12 | Aturan posting belum ada / komponen tidak terpetakan / komponen kurang | `Tertahan` | `POSTING_RULE_MISSING`, `COMPONENT_UNMAPPED`, `COMPONENT_MISSING` |

**Batas coba ulang terjadwal:** 3 (`BatasCobaUlangTerjadwal`); penjadwal memproses 100 baris per
gelombang (`AccAccountingEventSchedulerHostedService`).

**`Components` bernilai `null` DITERIMA.** `request.Components ?? new List<…>()` menormalkan null
menjadi daftar kosong, dan satu-satunya penolakan terkait komponen berlaku untuk **pesan saldo**
yang membawa komponen. Artinya `PayloadJson` Finance hari ini — yang selalu memuat
`"Components": null` — **tidak akan ditolak**.

### 15.4 Conflict — empat, dan dua di antaranya memblokir

| ID | Conflict | Bukti | Dampak |
|---|---|---|---|
| `FIN-CQ-04` | **Kejadian bernilai `0` akan DITOLAK penerima.** `FIN-DES-054` merancang `PENUTUPAN-SHIFT-KASIR` dan pembaliknya bernilai `0`, dan mencatatnya sebagai pertanyaan ke Accounting (`FIN-OQ-032`). Source menjawabnya: aturan 6 menolak `Amount <= 0` untuk pesan non-saldo, **dan** aturan 8 menolak pesan saldo dengan `409` karena jalurnya belum dibangun. Tidak ada celah yang lolos | `AccAccountingEventService.cs` baris 1075-1099 | **MEMBLOKIR** `FR-FIN-105`. `FIN-OQ-032` tidak perlu lagi ditanyakan sebagai "apakah diterima" — jawabannya **tidak**, pada implementasi hari ini. Yang perlu diputuskan sekarang: bentuk penanda dirancang ulang, atau Accounting memperluas kotak masuknya |
| `FIN-CQ-05` | **Pelurusan `Components` bukan pemblokir runtime.** `integration-contract.md` bagian 5.10.5 butir 1 menulis akibatnya "Setiap pesan berpotensi ditolak `400`". Source menunjukkan `null` diterima | Idem baris 1043, 1091 | **Tidak memblokir.** Pelurusannya tetap layak dikerjakan agar sejalan dengan permintaan Accounting, tetapi **prioritasnya turun** dari pemblokir menjadi kebersihan kontrak. Klaim di kontrak itu MUST diperbaiki |
| `FIN-CQ-06` | **Label menu yang dibangun tidak mengikuti `FIN-DEC-060`.** Keputusan itu menetapkan submenu **"Pembelian"** beserta label Indonesia ("Faktur Pembelian", "Tagihan Gabungan Penjamin"). Yang dibangun: grup **"Account Payable"** dengan butir **"Purchase Order"**, **"Receiving"**, **"Supplier Invoice"** — bahasa Inggris, tanpa submenu "Pembelian", tanpa butir Tagihan Gabungan | `src/utils/menu-sidebar/menu-items.jsx@49b59cfaa` baris 735-774 | Tidak memblokir backend. MUST diputuskan owner: keputusan `FIN-DEC-060` yang menyesuaikan diri ke yang sudah dibangun, atau menu yang disesuaikan ke keputusan. **Audit tidak memilih** |
| `FIN-CQ-07` | **Butir menu PO dan Receiving mengarah ke layar yang tidak punya sumber data.** Keduanya menunjuk `/finance/payable?tab=po` dan `?tab=receiving`, sementara pencarian di seluruh frontend menemukan **nol** pemanggilan endpoint `purchase-orders`/`goods-receipts`/`purchasing-invoices`/`supplier-returns`. Ditambah: kelima controller itu **tidak punya** `GET /` berpaging, sehingga layar daftar memang belum mungkin dibangun | FE: nol hasil pencarian; BE: `grep -c HttpGet` = 1 per controller (hanya `GET /{id}`) | Petugas dapat mencapai butir menu yang tidak dapat menampilkan data. **MUST** ditutup sebelum rumpun Purchasing dinyatakan selesai — dan urutannya backend lebih dulu (`GET /` berpaging), baru frontend |

### 15.5 Entri kemampuan baru

| ID | Kebutuhan | Pemilik | Bukti (`@cba60cb0` / `@49b59cfaa`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-037` | Verifikasi otomatis untuk kemampuan Finance | Platform/Finance | `Tests/QuilvianSystemBackend.Tests`, `.UnitTests.InMemory`, `.UnitTests.Sqlite`, `.IntegrationTests.Postgres` — **12 berkas test**, dan `grep` atas `FinanceManagement\|FinPurchas\|FinSupplierReturn\|AccountingOutbox\|FinAccountingEvent` menemukan **nol** | **`Missing`** (untuk Finance) | Project test **sudah ada** — klaim lama "project test belum terdeteksi" sudah tidak berlaku. Yang tidak ada: **satu pun** test yang menyentuh Finance | **Risiko tertinggi pass ini.** Seluruh rumpun Purchasing/AP (3.415 baris baru) berjalan tanpa test apa pun, termasuk invariant uang seperti `AvailableAmount >= 0` dan pemisahan porsi deposit pada kejadian pembayaran |
| `FIN-CAP-038` | Layar Finance AR/AP | Finance (frontend) | `src/app/finance/receivable/*` (termasuk `[slug]` rincian, `aging`, `invoice`, `payment`, `report`), `payable/*`, `ap-aging`, `ap-report`, `ar-aging`, `ar-report`, `cash-management`, `payment-ap`, `payment-ar` | `Ready to reuse` | Layar AR/AP nyata sudah ada — naik jauh dari keadaan lama yang hanya Petty Cash. Nol layar Purchasing | Sebagian butir menu belum punya sumber data (`FIN-CQ-07`) |
| `FIN-CAP-039` | Layar pantauan intake Billing dan kejadian Accounting | Finance (frontend) | `src/app/finance/monitoring/page.jsx` + `monitoring-client.jsx`; `src/lib/constants/finance/monitoring/monitoring-constants.jsx` memanggil `/api/v1/corporate/finance-management/billing-intake` **dan** `/accounting-events` | `Ready to reuse` | **Layar yang diandalkan `FIN-DES-056`/`057` untuk menampilkan baris `ERROR` sudah ada** — desain revisi 6 tidak perlu layar baru, sebagaimana diasumsikan `03-frontend-architecture.md` bagian 14.3 | Perlu diperiksa apakah layar itu menampilkan `ErrorMessage` intake, bukan hanya status — belum diaudit sampai tingkat komponen |
| `FIN-CAP-040` | Penyaringan butir menu berdasarkan hak akses | Platform (frontend) | `src/utils/menu-sidebar/permission/filter-menu-items-by-permission.jsx` (baru, 111 baris), `role/filter-menu-items-by-role.jsx` (+25); butir Finance memakai `requiredPermission: { resource: "Finance.AR"/"Finance.AP", action: … }` | `Reuse with adapter` | Resource `Finance.AR`/`Finance.AP` **memang ada** di backend, jadi bukan nama karangan. Tetapi butir PO/Receiving dijaga `Finance.AP:View`, sedangkan endpoint-nya menuntut `FinancePurchaseOrder:*`/`FinanceGoodsReceipt:*` — **granularitasnya berbeda** | Pengguna dapat melihat butir menu lalu ditolak di endpoint. Perlu keputusan: menu mengikuti resource granular, atau backend menerima `Finance.AP` sebagai payung |
| `FIN-CAP-041` | Migration Finance yang sudah ada | Finance | `Migrations/20260922110000_AddFinanceSupplierPayable`, `20260926100000_AddPurchasingApRumpun`, `20260926110000_AddSourcePurchasingInvoiceIdToSupplierPayable`, `20260928120000_AddDepositAppliedAmountToFinPayment` | `Ready to reuse` | **Migration `DepositAppliedAmount` (REVISI 5, urutan 3) sudah dibuat** — lebih maju dari yang dicatat blueprint | Audit ini **tidak** memeriksa apakah migration sudah dijalankan di database mana pun; itu di luar wewenang baca source |

### 15.6 Kontrak as-is — lima endpoint Purchasing yang sudah ada

Konvensi penamaan mengikuti `FIN-CAP-036` dan **tidak** meniru utang teknis `FinancePaymentsController`
(yang memakai singkatan "Payment"); resource memakai nama penuh kontrak.

| Grup `[Tags(...)]` | Base URL | Endpoint yang ADA | Hak akses |
|---|---|---|---|
| `Corporate / Finance Management / Purchasing / Purchase Order` | `api/v1/corporate/finance-management/purchasing/purchase-orders` | `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/approve`, `POST /{id}/reject`, `POST /{id}/cancel` | `FinancePurchaseOrder : Read/Create/Update/Submit/Approve/Cancel` — `reject` sengaja memakai `Approve` |
| `… / Goods Receipt` | `…/purchasing/goods-receipts` | `GET /{id}`, `POST /`, `POST /{id}/cancel` | `FinanceGoodsReceipt : Read/Create/Cancel` |
| `… / Invoice Exchange` | `…/purchasing/invoice-exchanges` | `GET /{id}`, `POST /`, `POST /{id}/cancel` | `FinanceInvoiceExchange : Read/Create/Cancel` |
| `… / Purchasing Invoice` | `…/purchasing/purchasing-invoices` | `GET /{id}`, `POST /`, `PUT /{id}`, `POST /{id}/submit`, `POST /{id}/approve`, `POST /{id}/reject` | `FinancePurchasingInvoice : Read/Create/Update/Submit/Approve` |
| `… / Supplier Return` | `…/purchasing/supplier-returns` | `GET /{id}`, `GET /deposits`, `POST /`, `POST /{id}/confirm`, `POST /{id}/cancel` | `FinanceSupplierReturn : Read/Create/Confirm/Cancel` |

**Gap yang sama pada kelimanya:** tidak ada `GET /` berpaging. Komentar controller menyebutnya
eksplisit sebagai gap terbuka yang "dicatat di laporan task, bukan dikarang di sini" — audit ini
mengonfirmasi gap itu masih terbuka pada `cba60cb0`.

### 15.7 Fact, inferensi, rekomendasi

**Fact (terverifikasi baca langsung):**

1. Endpoint penerima Accounting ada, beserta penjadwal, 12 aturan penolakan, dan perilaku idempoten `200`.
2. `Amount <= 0` ditolak `400`; jalur pesan saldo ditolak `409` karena belum dibangun.
3. `Components` bernilai `null` diterima tanpa masalah.
4. Rumpun Purchasing/AP dibangun penuh di backend: 7 model terdaftar `DbContext`, 11 configuration, 1 migration, 5 controller, 5 service.
5. Kelima controller Purchasing tidak punya endpoint daftar berpaging.
6. `FinReceivableInvoiceBatch` dan `FinReceiptDeduction` tetap nol baris.
7. Project test ada (4 project, 12 berkas), nol test menyentuh Finance.
8. Frontend punya layar AR/AP dan layar pantauan; nol layar Purchasing; nol pemanggilan endpoint Purchasing.
9. Menu Finance punya 19 butir berlabel bahasa Inggris, dijaga `Finance.AR`/`Finance.AP`.
10. Ambang approval kini `< Rp 50.000.000 → TIER_1`, terpusat di satu resolver.
11. Komentar `ApplicationDbContext` masih menyatakan migration Purchasing belum dibuat, padahal sudah ada.

**Inferensi (bukan fakta):**

1. Karena kelima controller Purchasing dibangun tanpa `GET /` **dan** frontend-nya belum ada, urutan pengerjaannya kemungkinan sengaja: perilaku transaksional lebih dulu, daftar dan layar menyusul. Ini konsisten dengan catatan controller sendiri, bukan kelalaian.
2. Karena label menu Inggris dipakai seragam di seluruh grup Finance (bukan hanya AP), kemungkinan pola itu keputusan frontend yang lebih luas daripada modul ini — sehingga `FIN-CQ-06` mungkin bukan milik Finance untuk diselesaikan sendiri.

**Rekomendasi:**

1. **`FIN-CQ-04` MUST diangkat ke owner sekarang**, sebelum `FR-FIN-105` masuk perencanaan. Pilihannya bukan lagi menunggu jawaban Rizki tentang "apakah nilai nol diterima" — buktinya sudah ada, dan jawabannya tidak.
2. **Perbaiki klaim pada `integration-contract.md` bagian 5.10.5 butir 1** (`FIN-CQ-05`) supaya prioritas pekerjaan tidak salah baca.
3. **Tambahkan test untuk invariant uang rumpun Purchasing/AP** sebelum rumpun itu dinyatakan selesai (`FIN-CAP-037`) — ini rekomendasi paling berdampak dari pass ini.
4. **Jadwalkan `GET /` berpaging kelima controller** lebih dulu, baru layar Purchasing (`FIN-CQ-07`).
5. `FIN-CQ-06` dibawa ke owner sebagai pertanyaan label, bukan diperbaiki sepihak.

### 15.8 Yang TIDAK ditutup pass ini

- **Audit field-per-field rumpun AR/AP/Payable** (`FinReceivable`, `FinReceiptAllocation`, `FinMedicalServicePayable`, dst.) — utang dari bagian 9.3 dan 12.3, **tetap terbuka**. Pass ini impact scan terarah, bukan audit penuh.
- **Audit tingkat komponen layar Finance** — keberadaan layar dikonfirmasi, tetapi isi tabel, keadaan kosong/gagal, dan apakah `ErrorMessage` intake benar-benar ditampilkan **belum** diperiksa.
- **Apakah migration sudah dijalankan** di database mana pun — di luar wewenang baca source.
- **Field-per-field 7 model Purchasing** terhadap `erd/data-dictionary.md` C.1-C.16 — hanya keberadaan, pendaftaran, dan kontrak endpoint yang diverifikasi; kecocokan kolom satu per satu belum.

### 15.9 Pemicu impact scan berikutnya

Peta ini (khusus bagian 15) **stale** dan wajib diperiksa ulang bila:

- Backend bergerak dari **`cba60cb0`** dan menyentuh `Areas/Corporate/FinanceManagement/`, `Areas/Corporate/AccountingManagement/AccountingEvent/`, atau `Migrations/`.
- Frontend bergerak dari **`49b59cfaa`** dan menyentuh `src/app/finance/`, `src/utils/menu-sidebar/`, atau `src/lib/constants/finance/`.
- **Accounting mengubah aturan penolakan kotak masuknya** — khususnya bila `Amount = 0` mulai diterima atau jalur pesan saldo (`BE-ACC-P2-028`) selesai dibangun. Keduanya langsung mengubah `FIN-CQ-04`.
- Endpoint `GET /` berpaging Purchasing mulai dibangun, atau layar Purchasing mulai dibangun.

### 15.10 Handoff

Pass ini **tidak** menghasilkan arsitektur target dan **tidak** menyentuh source. Yang dihasilkan:
tujuh koreksi status, dua koreksi klaim, empat `Conflict` (`FIN-CQ-04`..`07`), lima entri baru
(`FIN-CAP-037`..`041`), dan kontrak as-is endpoint penerima Accounting.

**Yang paling mendesak dibawa ke owner:** `FIN-CQ-04` — desain penanda shift tertutup revisi 6
bertumpu pada kejadian bernilai `0`, dan kotak masuk Accounting sebagaimana dibangun akan
menolaknya. Ini mengubah `FIN-OQ-032` dari pertanyaan terbuka menjadi temuan berbukti, dan
menuntut `/grill-me` amendment pass atau `/design-business-module` pass kecil untuk merancang ulang
bentuk penandanya.

---

## 16. Impact scan lanjutan — pekerjaan `BE-FIN-036` di working tree, 28 September 2026

**Pemicu.** Sesudah `/design-business-module` revisi 7 selesai, `git status` menunjukkan **lima
berkas source berubah (+590/−59)** yang **tidak** berasal dari pass desain mana pun. Pemeriksaan
menemukan itu implementasi task `BE-FIN-036`, lengkap dengan laporan task di
`task/report/backend/BE-FIN-036.md` — jadi pekerjaan **terlacak**, bukan liar.

**Batas audit pass ini.** Hanya kelima berkas itu beserta kesesuaiannya terhadap `FIN-DES-045`,
`046`, `047`, `051`, dan `059`, ditambah satu pemeriksaan silang yang muncul dari sana: penamaan
resource hak akses seluruh modul. **BUKAN** audit ulang bagian 1-15.

**Peringatan penting tentang sifat bukti pass ini.** Backend SHA **tidak bergerak** — tetap
`cba60cb0`. Seluruh temuan di bawah berasal dari **working tree yang belum di-commit**, sehingga:

| Hal | Konsekuensinya |
|---|---|
| Bukti tidak dapat ditambatkan ke SHA | Entri di bawah ditandai `@working-tree-2026-09-28` alih-alih `@<sha>`. Ia **MUST** diverifikasi ulang begitu pekerjaan itu di-commit |
| Pekerjaan masih dapat berubah | Status `Extend`/`Repair` di bawah adalah keadaan sesaat, bukan kontrak |
| `backend_working_tree` | **TIDAK lagi bersih** — berbeda dari catatan di kepala dokumen ini yang menyebut bersih pada audit 20 September 2026 |

**Satu hal yang TIDAK dilakukan:** `dotnet build` tidak dijalankan, sesuai instruksi pengguna yang
juga tercatat pada laporan task itu sendiri. Seluruh penilaian di bawah adalah **penelaahan statis**
— audit ini **tidak** dapat menyatakan kode itu dapat dikompilasi.

### 16.1 Kesesuaian implementasi terhadap desain yang sudah disetujui

Diperiksa baris demi baris terhadap `FIN-DES-045`..`047` (`approved` 26 September 2026) dan
`D.9` (daftar invariant revisi 5).

| Yang dirancang | Yang ditemukan di working tree | Nilai |
|---|---|---|
| `NetTransferAmount = TotalAmount − DeductionAmount + AdditionAmount − DepositAppliedAmount` | Diterapkan persis, beserta pesan galat yang menyebut keempat komponennya | **Sesuai** |
| `FIN-VAL-091` hanya berlaku bila `DepositAppliedAmount = 0` | `if (NetTransferAmount == 0m && DepositAppliedAmount == 0m) throw` | **Sesuai** |
| Nomor bukti transfer wajib hanya bila `NetTransferAmount > 0` | `if (payment.NetTransferAmount > 0)` | **Sesuai** (`FIN-VAL-056`) |
| Pelepasan baris `RESERVED` saat `REJECTED` dan `CANCELLED` | `ReleaseReservedByPaymentAsync` dipanggil pada kedua jalur, `DepositAppliedAmount` direset ke 0 dan `NetTransferAmount` dihitung ulang | **Sesuai** (`FIN-DES-046`) |
| Baris `RESERVED` → `APPLIED` saat `PAID` | `MarkAppliedByPaymentAsync` | **Sesuai** |
| `AP_PAYMENT` bernilai `TotalAmount − DepositAppliedAmount`, dilewati bila nol | `apPaymentAmount` dihitung demikian, kejadian hanya di-stage bila > 0 | **Sesuai** (`FIN-DES-047`, `FIN-VAL-131`) |
| Kejadian pemakaian deposit memakai **nama final** `FIN-DEC-066` | `FinAccountingEventTypeCodes.PemakaianKreditReturPembelian` | **Sesuai** — implementasi sudah memakai nama hasil ratifikasi, bukan nama lama |
| Kunci `Serializable` ganda (`FIN_PAYMENT_{id}` + `FIN_RETURN_DEPOSIT_{id}`) | Keduanya diambil pada `AddReturnDepositAsync` | **Sesuai** (`FIN-DES-046`) |
| `AvailableAmount >= 0`, satu-satunya penulis `FinanceSupplierReturnService` | `ReserveAsync` menolak `usedAmount > AvailableAmount`, mengurangi saldo, menandai `EXHAUSTED` saat nol; tidak membuka transaksi sendiri | **Sesuai** |
| Lima aturan validasi `FIN-VAL-123`..`127` | Kelimanya dirujuk eksplisit di komentar dan diterapkan | **Sesuai** |
| Tiga endpoint `GET`/`POST`/`DELETE /payments/{id}/return-deposits` | Ketiganya ada, `DELETE` memakai `ExpectedRowVersion` lewat query — persis seperti `api-contract.md` `C.1` | **Sesuai** |

**Penilaian:** implementasi ini **taat pada desainnya**. Dari sebelas titik yang diperiksa, sebelas
sesuai. Ini temuan yang layak dicatat sebagai hal positif, bukan hanya daftar cacat.

### 16.2 Dua cacat yang ditemukan pada pekerjaan itu

| # | Cacat | Bukti | Tingkat |
|---:|---|---|---|
| 1 | **Alias konstanta yang dilarang `FIN-DES-051`.** `public const string PemakaianDepositRetur = PemakaianKreditReturPembelian;` menghidupkan nama yang sudah dicabut katalog | `FinAccountingEventOutbox.cs@working-tree-2026-09-28` | **Rendah.** Alias menunjuk **nilai yang sama**, jadi risiko kirim ganda yang dicegah aturan itu tidak terjadi. Tetap MUST dihapus — pembersihan, nol perubahan perilaku |
| 2 | **Nol test.** Tidak ada berkas di `Tests/` yang berubah bersama +590 baris ini | `git status` — hanya lima berkas `Areas/` | **Tinggi.** Logika uang (invariant saldo, reservasi/pelepasan, pemisahan porsi kas) masuk tanpa satu pun uji otomatis. `FIN-CAP-037` tetap `Missing`, dan permukaan yang tidak teruji **bertambah** |

### 16.3 Temuan sistemik (`FIN-CQ-08`): nama resource hak akses di kode berbeda dari kontrak

Muncul saat memeriksa hak akses ketiga endpoint baru. Ketiganya memakai
`[AccessPermission("Payment", …)]`, sedangkan `api-contract.md` `C.1` dan
`permission-audit-matrix.md` sama-sama menyebut `FinancePayment`. Pemeriksaan lanjutan menunjukkan
**ini bukan kesalahan `BE-FIN-036`** — seluruh sebelas atribut pada controller itu, termasuk yang
sudah di-commit sejak lama, memakai `Payment`.

Perbandingan lengkap seluruh modul:

| Resource pada kontrak (`FIN-PERM-1.2`) | Resource pada kode | Cocok? |
|---|---|:---:|
| `FinancePayment` | `Payment` | **Tidak** |
| `FinanceReceipt` | `Receipt` | **Tidak** |
| `FinanceReceivable` | `Receivable` | **Tidak** |
| `FinanceSupplierPayable` | `SupplierPayable` | **Tidak** |
| `FinanceBillingIntake` | `BillingIntake` | **Tidak** |
| `FinanceAccountingEvent` | `AccountingEvents` | **Tidak** — prefix **dan** bentuk jamak |
| `FinanceBankDeposit`, `FinanceDailyCash` | sama | Ya |
| `FinanceGoodsReceipt`, `FinanceInvoiceExchange`, `FinancePurchaseOrder`, `FinancePurchasingInvoice`, `FinanceSupplierReturn` | sama | Ya |
| **tidak ada di kontrak** | `Finance.AP`, `Finance.AR` | **Tidak ada padanannya** |
| **tidak ada di kontrak** | `BankAccount`, `Currency`, `PettyCashBudget`, `PettyCashCategory` | **Tidak ada padanannya** |

**Polanya terbaca jelas:** controller **baru** (rumpun Purchasing, dibangun pada `cba60cb0`)
mengikuti konvensi kontrak; controller **lama** (Payment/Receipt/Receivable/SupplierPayable/
BillingIntake/AccountingEvent) memakai nama pendek. Modul ini sedang di tengah migrasi penamaan,
dan kontrak mendokumentasikan **sasaran**, bukan keadaan sekarang.

**Kenapa ini material, bukan sekadar kerapian.** String pada `AccessPermission` adalah string yang
**harus diberikan ke peran** agar endpoint dapat dipanggil. Siapa pun yang menyemai peran dari
`permission-audit-matrix.md` akan memberikan `FinancePayment`, dan **tidak ada** endpoint pembayaran
yang dapat dipanggil dengan itu. Sebaliknya, `Finance.AP`/`Finance.AR` — dua resource yang
**dipakai frontend** untuk menyaring butir menu — tidak ada sama sekali di matriks hak akses yang
sudah disetujui.

Ini memperbesar `FIN-OQ-036` yang dibuka revisi 7: persoalannya bukan hanya granularitas butir
menu, melainkan bahwa dua resource yang diandalkan frontend **tidak pernah masuk kontrak hak
akses**.

### 16.4 Entri kemampuan baru

| ID | Kebutuhan | Pemilik | Bukti | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-042` | Deposit Retur dipakai sebagai sumber dana pembayaran supplier (`FIN-SC-008`, `FIN-DEC-057`) | Finance | `Payable/Services/FinancePaymentService.cs`, `Payable/Controllers/FinancePaymentsController.cs`, `Payable/Dtos/FinancePaymentDtos.cs`, `Purchasing/Services/FinanceSupplierReturnService.cs`, `AccountingIntegration/Models/FinAccountingEventOutbox.cs` — seluruhnya `@working-tree-2026-09-28`, **belum di-commit** | **`Extend`** | Sebelas titik desain sesuai (16.1). Yang tersisa: hapus alias konstanta, tambahkan test, dan jalankan build — build **sengaja belum** dijalankan sesuai instruksi pengguna | Belum dapat dikompilasi-verifikasi. Bukti belum tertambat SHA, sehingga MUST diperiksa ulang sesudah commit |
| `FIN-CAP-043` | Penamaan resource hak akses modul Finance | Finance + Security Owner | Perbandingan lengkap pada 16.3 — enam resource menyimpang, tujuh cocok, enam hanya ada di kode | **`Repair`** | Kode dan kontrak menyebut nama berbeda untuk resource yang sama. Penyemaian peran dari kontrak **tidak akan bekerja** untuk enam resource itu | **Tinggi untuk operasional, nol untuk logika bisnis.** Memperbesar `FIN-OQ-036`; MUST diputuskan bersama Security Owner sebelum peran disemai ke lingkungan mana pun |

`FIN-CAP-037` (verifikasi otomatis Finance) **tetap `Missing`**, dan permukaan tanpa uji bertambah
+590 baris.

### 16.5 Satu cacat pada dokumen blueprint yang ditemukan lewat pemeriksaan silang

Saat mencocokkan aturan validasi yang dirujuk kode dengan matriks validasi, ditemukan **rujukan
nomor yang keliru pada dokumen desain revisi 6** — bukan pada kode:

| Hal | Isi |
|---|---|
| Yang keliru | `02-backend-architecture.md` (3 tempat) dan `contracts/integration-contract.md` (1 tempat) menyebut **`FIN-VAL-123`** sebagai aturan yang menolak potongan AR berjenis `OTHER` |
| Kenyataannya | `FIN-VAL-123` sudah dipakai sejak revisi 5 dengan arti **"Deposit hanya untuk pembayaran supplier"**, dan dipakai begitu oleh kode, laporan task `BE-FIN-036`, serta matriks uji |
| Nomor yang benar | **`FIN-VAL-137`** — itu nomor yang benar-benar ditulis di matriks validasi revisi 6 untuk penolakan `OTHER` |
| Akibat bila dibiarkan | Implementer `FIN-DES-052` mencari `FIN-VAL-123`, menemukan aturan tentang deposit supplier, lalu menyimpulkan desainnya tidak konsisten |
| Tindakan | **Keempat rujukan sudah dibetulkan** pada pass ini menjadi `FIN-VAL-137`. Ini koreksi rujukan, bukan perubahan desain — arti aturannya tidak bergerak sedikit pun |

### 16.6 Fact, inferensi, rekomendasi

**Fact (terverifikasi baca langsung):**

1. Backend SHA tidak bergerak (`cba60cb0`); lima berkas source berubah di working tree (+590/−59).
2. Pekerjaan itu task terlacak `BE-FIN-036`, berstatus 🟡 SEBAGIAN, dengan laporan task lengkap.
3. `dotnet build` **tidak** dijalankan, atas instruksi pengguna; verifikasinya penelaahan statis.
4. Sebelas titik desain `FIN-DES-045`..`047` diterapkan sesuai.
5. Implementasi memakai nama kejadian hasil ratifikasi (`PEMAKAIAN-KREDIT-RETUR-PEMBELIAN`), bukan nama lama.
6. Satu alias konstanta bernilai identik ditulis, bertentangan dengan `FIN-DES-051`.
7. Nol test menyertai pekerjaan itu.
8. Enam resource hak akses di kode berbeda nama dari kontrak; `Finance.AP`/`Finance.AR` tidak ada di kontrak sama sekali.
9. Empat rujukan `FIN-VAL-123` pada dokumen desain revisi 6 keliru; sudah dibetulkan menjadi `FIN-VAL-137`.

**Inferensi (bukan fakta):**

1. Karena controller baru mengikuti konvensi kontrak sementara controller lama tidak, penyimpangan penamaan hak akses kemungkinan **peninggalan sebelum kontrak ditulis**, bukan keputusan sadar untuk menolak kontrak. Bila benar, memperbaikinya adalah migrasi terjadwal, bukan perdebatan konvensi.
2. Karena implementasi `BE-FIN-036` taat pada sebelas titik desain termasuk nama hasil ratifikasi yang baru disepakati hari itu, alur desain → task → implementasi pada modul ini tampak berjalan; alias konstanta lebih mirip kehati-hatian berlebih ("jaga kompatibilitas") daripada kelalaian.

**Rekomendasi:**

1. **`FIN-CAP-043` dibawa ke owner bersama Security Owner sebelum peran disemai ke lingkungan mana pun.** Ini rekomendasi paling mendesak dari pass ini: tanpa keputusan itu, hak akses yang disemai dari kontrak tidak akan bekerja.
2. **Hapus alias `PemakaianDepositRetur`** saat pekerjaan `BE-FIN-036` diselesaikan — satu baris, nol risiko.
3. **Jalankan build dan tambahkan test** untuk invariant uang rumpun ini sebelum `BE-FIN-036` dinyatakan selesai; statusnya memang masih 🟡.
4. **Jalankan impact scan ulang sesudah commit**, karena seluruh bukti pass ini belum tertambat SHA.

### 16.7 Yang TIDAK ditutup pass ini

- Audit field-per-field rumpun AR/AP/Payable — utang dari bagian 9.3, 12.3, dan 15.8; **tetap terbuka**.
- Verifikasi kompilasi kelima berkas `BE-FIN-036` — build sengaja tidak dijalankan.
- Audit tingkat komponen layar Finance — sama seperti bagian 15.8.
- Field-per-field tujuh model Purchasing terhadap kamus data.
- Apakah migration sudah dijalankan di database mana pun.

### 16.8 Pemicu impact scan berikutnya

Selain pemicu bagian 15.9, peta ini (khusus bagian 16) **stale** bila:

- Kelima berkas `BE-FIN-036` **di-commit** — seluruh entri `@working-tree-2026-09-28` MUST ditambatkan ulang ke SHA.
- Alias `PemakaianDepositRetur` dihapus, atau test pertama untuk Finance ditambahkan.
- Keputusan `FIN-CAP-043`/`FIN-OQ-036` turun dan nama resource hak akses diselaraskan.
- `dotnet build` dijalankan dan hasilnya diketahui.

### 16.9 Handoff

Pass ini read-only terhadap source dan tidak menghasilkan arsitektur target. Yang dihasilkan: dua
entri kemampuan baru (`FIN-CAP-042` `Extend`, `FIN-CAP-043` `Repair`), penilaian kesesuaian sebelas
titik desain, dua cacat pada pekerjaan berjalan, satu temuan sistemik hak akses, dan satu koreksi
rujukan pada dokumen desain.

**Yang paling mendesak:** `FIN-CAP-043` / `FIN-CQ-08`. Enam resource hak akses di kode tidak sama
dengan kontrak, dan dua resource yang diandalkan frontend tidak ada di kontrak — sehingga penyemaian
peran dari dokumen yang sudah disetujui **tidak akan menghasilkan akses yang bekerja**. Keputusannya
milik owner bersama Security Owner, dan audit tidak memilihnya.

**Penelusuran ID pass ini:**

| ID | Jenis | Tercatat di |
|---|---|---|
| `FIN-CAP-042` | Kemampuan baru — pemakaian Deposit Retur (`Extend`) | 16.4 |
| `FIN-CAP-043` | Kemampuan baru — penamaan resource hak akses (`Repair`) | 16.3, 16.4 |
| `FIN-CQ-08` | Closure question — keputusan penamaan resource, menuntut Security Owner | 16.3, `blueprint-manifest.md` |
| `FIN-OQ-036` | Diperbesar oleh `FIN-CQ-08` — bukan lagi hanya soal granularitas butir menu | 16.3 |
| `FIN-OQ-034` | Tidak berubah oleh pass ini; suratnya sudah dikirim ke owner Billing lewat `evidence/17` | bagian 15.4, `evidence/17` |
| `FIN-CAP-037` | Tetap `Missing`; permukaan tanpa uji bertambah +590 baris | 16.2, 16.4 |

---

## 17. Impact scan terarah — kesejajaran menu Transaksi A/R & A/P vs `Keuangan.md` (30 September 2026, `831ddb5d` / `e957fbec`)

**Pemicu.** Pemilik modul membandingkan struktur menu mobile pada `Keuangan.md` (bukti video
sistem rujukan, bagian 5.1–5.2) dengan frontend Quilvian saat ini dan bertanya: kenapa menu
setara belum ada, dan apakah backend-nya juga belum ada. Pass ini **bukan** audit ulang bagian
1–16; ia menyandingkan tiap butir menu `Keuangan.md` FIN-AP-001..014 / FIN-AR-001..014 dengan
kode as-is hari ini.

**Verifikasi staleness.** Backend `cba60cb0` → `831ddb5d` (mencakup commit `BE-FIN-036`,
`BE-FIN-037`, dan pekerjaan lain yang tidak diaudit satu-satu). Frontend `49b59cfaa` → `e957fbec`
(mencakup `FE-FIN-014`). Kedua pergerakan **menyentuh langsung** boundary Purchasing/AR/AP —
beberapa koreksi status besar ditemukan (17.1).

### 17.1 Koreksi status akibat pekerjaan yang sudah selesai sejak bagian 15/16

| ID | Status lama | Status baru | Bukti (`@831ddb5d` / `@e957fbec`) | Yang berubah |
|---|---|---|---|---|
| `FIN-CQ-06` (label menu Inggris vs `FIN-DEC-060`) | Terbuka | **Selesai** | `src/utils/menu-sidebar/menu-items.jsx` baris 700-742 — submenu **"Pembelian"** dengan label Indonesia ("Tukar Faktur", "Faktur Pembelian", "Retur Pembelian", "Laporan Pembelian"), komentar inline mengutip `FE-FIN-014`/`FIN-DEC-060` | Menu sudah direlabel sesuai keputusan. `Account Payable`/`Account Receivable` sendiri tetap berlabel Inggris (bukan bagian `FIN-DEC-060`) |
| `FIN-CQ-07` (butir menu PO/Receiving tanpa sumber data) | Terbuka | **Selesai untuk PO/Tukar Faktur/Faktur Pembelian/Retur** | `pathname` menu-items.jsx menunjuk `/finance/purchasing/purchase-orders`, `/invoice-exchanges`, `/purchasing-invoices`, `/supplier-returns` — seluruhnya berkorespondensi dengan controller `FIN-CAP-028` yang sudah ada | **Goods Receipt tetap tidak dapat butir/layar daftar sendiri** — komentar kode baris 705-707 menyebut eksplisit "belum ada halaman daftar GR yang berdiri sendiri", GR hanya tercatat dari alur detail PO |
| `FIN-CAP-028` (gap `GET /` berpaging kelima controller Purchasing) | `Extend` (gap paging) | **`Ready to reuse`** untuk 4 dari 5 | `FinancePurchasingReportsController.cs` (baru, `BE-FIN-037`) menyediakan `/purchasing/reports/summary`, `/invoice-exchanges`, `/due-dates`, `/reconciliation` — keempatnya **daftar berpaging read-only**, menutupi kebutuhan tampilan tanpa perlu `GET /` mentah di kelima controller transaksional | Endpoint `GET /` mentah pada kelima controller transaksional **tetap belum ada** — yang baru adalah jalur laporan terpisah, bukan daftar CRUD |
| `FIN-CAP-032` (AR Invoice Agregat) | `Missing` | **`Ready to reuse`** | `Receivable/Models/FinReceivableInvoiceBatch.cs`, `FinReceivableInvoiceBatchItem.cs`, `Services/FinanceReceivableInvoiceBatchService.cs`, `Controllers/FinanceReceivableInvoiceBatchesController.cs` — dan menu FE "Tagihan Gabungan Penjamin" (`financeReceivableInvoiceBatch`, `pathname: /finance/receivable-invoice-batches`) | Rumpun yang dulu nol baris kini punya model+service+controller+layar. Field-per-field terhadap `FIN-DEC-048` **belum diperiksa pass ini** — hanya keberadaan dan kabel FE↔BE |

### 17.2 Kesejajaran menu `Keuangan.md` (sistem rujukan) vs Quilvian hari ini

**Grup A/P** (`Keuangan.md` 5.1 vs `menu-items.jsx` grup "Pembelian" + "Account Payable"):

| Butir `Keuangan.md` | Padanan Quilvian hari ini | Status |
|---|---|---|
| Pembelian Pesanan (Purchase Order) | "Purchase Order" — `/finance/purchasing/purchase-orders` | **Ada** (BE+FE) |
| Penerima Pesanan (Goods Receipt) | Tidak ada butir/layar daftar sendiri; tercatat inline dari detail PO | **BE ada, FE sebagian** (`FIN-CQ-07` sisa) |
| Retur Produk / Retur Pembelian Supplier | "Retur Pembelian" — `/finance/purchasing/supplier-returns` (Retur + Deposit Retur digabung) | **Ada** (BE+FE) |
| Tukar Faktur | "Tukar Faktur" — `/finance/purchasing/invoice-exchanges` | **Ada** (BE+FE) |
| Purchasing Invoice | "Faktur Pembelian" — `/finance/purchasing/purchasing-invoices` | **Ada** (BE+FE) |
| Rekap Purchasing AP | Bagian dari "Laporan Pembelian" → `GET .../reports/summary` | **Ada** (BE+FE, sejak `BE-FIN-037`) |
| Laporan Tukar Faktur | Bagian dari "Laporan Pembelian" → `GET .../reports/invoice-exchanges` | **Ada** (BE+FE, sejak `BE-FIN-037`) |
| Laporan Jatuh Tempo | Bagian dari "Laporan Pembelian" → `GET .../reports/due-dates` | **Ada** (BE+FE, sejak `BE-FIN-037`) |
| Rekonsiliasi Tagihan | Bagian dari "Laporan Pembelian" → `GET .../reports/reconciliation` | **Ada** (BE+FE) — **lebih lengkap dari sistem rujukan sendiri**, yang menurut `Keuangan.md` FIN-AP-014 "menu tersedia, tetapi layar dan tindakannya belum dibuka" |
| Purchasing Payment | "Supplier Payment" — `/finance/payable/payment` (grup Account Payable, bukan Pembelian — Quilvian memisahkan "proses beli" dari "bayar utang") | **Ada** (BE+FE), beda kelompok menu |
| Laporan Pembayaran AP | "Report AP" — `/finance/payable/report` | **Ada di kode, TAPI tersembunyi permanen** — lihat `FIN-CQ-09` baru (17.3) |
| Utang Usaha (A/P Aging) / Laporan Aging AP | "Aging AP" — `/finance/payable/aging` | **Ada** (BE+FE) |
| Jasa Medis | — | **Di luar scope AR/AP** — kapabilitas ini milik modul terpisah `medical-fee` (blueprint sendiri sudah ada `docs/module-blueprints/medical-fee/`), bukan bagian rumpun Purchasing/AP Finance |

**Grup A/R** (`Keuangan.md` 5.2 vs `menu-items.jsx` grup "Account Receivable" + butir flat):

| Butir `Keuangan.md` | Padanan Quilvian hari ini | Status |
|---|---|---|
| Tagihan/Billing | "Billing / Invoice" — `/finance/receivable/invoice` | **Ada** (BE+FE) |
| Receivable AR/Invoice | "Receivable AR" — `/finance/receivable` | **Ada** (BE+FE) |
| Canceled Invoice / Receivable AR Canceled | Tidak ada butir menu terpisah — pembatalan adalah aksi di dalam layar Receivable AR (`FIN-CAP` pembatalan sudah ada di backend, bukti bagian 9/12), bukan halaman sendiri | **Fungsi ada, menu terpisah tidak ada** — konsisten dengan pola Quilvian (aksi inline), bukan gap |
| Report Canceled Invoice / Receiveable AR Canceled / Report Closed Billing / Report AR Created | Tidak ditemukan sebagai laporan terpisah; kemungkinan tercakup "Report AR" generik | **Belum diverifikasi granular** — ditandai closure question 17.4 |
| Report Receiveable AR / Report Payment AR / Report AR | "Report AR" — `/finance/receivable/report` | **Ada di kode, TAPI tersembunyi permanen** — `FIN-CQ-09` (17.3) |
| Settlement AR | "Settlement AR" — `/finance/receivable?status=SETTLED` (filter, bukan layar terpisah) | **Ada** (BE+FE) |
| Laporan Aging AR / Umur Piutang (A/R Aging) | "Aging AR" — `/finance/receivable/aging` | **Ada** (BE+FE) |
| Piutang Korporat/Penjamin | "Tagihan Gabungan Penjamin" — `/finance/receivable-invoice-batches` (`FIN-CAP-032`, baru berubah status 17.1) | **Ada** (BE+FE) — belum diverifikasi field-per-field |
| Manajemen Klaim | Pencarian `Klaim\|Claim` di `Areas/Corporate/FinanceManagement` dan `src/app/finance` — **nol hasil genuine** (satu-satunya match adalah komentar "klaim hak akses" di kode permission, bukan fitur bisnis) | **`Missing`** — nol baris kode di BE maupun FE |
| Pemutihan Piutang | Pencarian `Pemutihan\|WriteOff` di seluruh `Areas/` dan `src/app/finance` — **nol hasil** | **`Missing`** — nol baris kode di BE maupun FE, konsisten dengan `Keuangan.md` sendiri yang mencatat bukti fitur ini "masih terbatas" bahkan di sistem rujukan |
| Ayat Silang | Pencarian `Ayat.?Silang\|OffsettingEntry\|CrossEntry` di seluruh `NewQuilvianSystemBackend` (`.cs`) — **nol hasil** | **`Missing`** — konsep ini **tidak muncul sama sekali** di `Keuangan.md` (bukan salah satu dari FIN-AR-001..014), sehingga kemungkinan bukan istilah dari sistem rujukan yang sama; MUST diklarifikasi ke owner apa maksud istilah ini dan dari sumber mana asalnya sebelum dianggap gap |
| Piutang Tagihan | Kemungkinan sinonim "Receivable AR" | **Ada** (asumsi penamaan, belum dikonfirmasi owner) |

### 17.3 Conflict baru — `FIN-CQ-09`

| ID | Conflict | Bukti | Dampak |
|---|---|---|---|
| `FIN-CQ-09` | **Butir menu "Report AR" dan "Report AP" tidak akan pernah bisa diakses siapa pun.** Backend menjaga `GET .../receivable/report` dan `GET .../payable/report` dengan permission action `"View"`, tetapi frontend menyaring butir menunya dengan `requiredPermission: { action: "View" }` — ini **sudah konsisten**, jadi bukan itu masalahnya. Komentar kode sendiri (`menu-items.jsx` baris 783-786, 830-833) menulis eksplisit: backend mendaftarkan action `"Report"` yang **tidak pernah dipetakan** ke permission apa pun yang bisa digrant admin, sehingga meskipun menunya tampil, endpoint di baliknya secara struktural tidak bisa diberi akses pada peran manapun | `menu-items.jsx` baris 780-786 (AR), 826-833 (AP), keduanya menandai diri sebagai `FIN-CQ-09` di dalam kode | Dua dari sekian laporan yang diminta `Keuangan.md` (Report AR/Report Receiveable AR/Report Payment AR di sisi AR; Laporan Pembayaran AP di sisi AP) secara teknis "ada" tapi **operasional tidak bisa dipakai user manapun**. Ini ditandai sendiri di kode — bukan temuan baru murni, tetapi belum tercatat sebagai entri `Conflict` formal di peta ini sampai pass ini |

### 17.4 Yang TIDAK ditutup pass ini / closure questions untuk `/grill-me`

- **Istilah "Ayat Silang"** — tidak ditemukan di `Keuangan.md` maupun source manapun. **MUST ditanyakan ke owner**: apa definisi bisnisnya, dan apakah berasal dari sistem rujukan yang sama atau sumber lain? Pass ini tidak mengarang definisi.
- **Granularitas "Report AR" generik** vs empat butir terpisah `Keuangan.md` (Report Canceled Invoice, Report Receiveable AR, Report Payment AR, Report Closed Billing, Report AR Created) — belum diverifikasi apakah satu layar "Report AR" sudah menyatukan kelimanya lewat filter, atau sebagian benar-benar belum ada. **MUST diperiksa di tingkat komponen**, di luar wewenang audit source read-only tingkat ini.
- **`Manajemen Klaim` dan `Pemutihan Piutang`** dikonfirmasi `Missing` total (BE dan FE). **MUST diputuskan owner**: apakah keduanya masuk scope Finance AR (sejalan `FIN-DEC-045..055`), atau ditunda sebagai scope terpisah — belum ada keputusan bisnis (`FIN-DEC-XXX`) yang mencakup keduanya secara eksplisit sejauh dokumen ini.
- **Field-per-field `FinReceivableInvoiceBatch`** terhadap `FIN-DEC-048` — hanya keberadaan yang diverifikasi pass ini, bukan kesesuaian kolom.
- **`FIN-CQ-09`** perlu dibawa ke owner bersama Security/Permission — sama sifatnya dengan `FIN-CQ-08` (bagian 16.3): kontrak hak akses dan kode sudah menyimpang duluan sebelum pass ini menandainya.
- Audit field-per-field rumpun AR/AP/Payable yang sudah lama jadi utang (bagian 9.3, 12.3, 15.8, 16.7) **tetap terbuka**, tidak bertambah atau berkurang oleh pass ini.

### 17.5 Handoff

**Jawaban langsung untuk pertanyaan pemicu:** sebagian besar menu `Keuangan.md` **sudah ada** di
Quilvian, backend maupun frontend, dan sebagian dibangun sangat baru (`BE-FIN-037`, `FE-FIN-014`
— bergerak sejak audit bagian 15/16). Gap yang **genuinely** `Missing` di kedua sisi hanya tiga:
**Ayat Silang** (istilah asing, bahkan tidak ada di `Keuangan.md`), **Manajemen Klaim**, dan
**Pemutihan Piutang** (keduanya memang dicatat `Keuangan.md` sendiri sebagai bukti lemah di
sistem rujukan). Selebihnya adalah soal **pengelompokan menu berbeda** (mis. Purchasing Payment
pindah ke grup "Account Payable", bukan "Pembelian"), **aksi inline vs halaman terpisah** (Cancel
Invoice), atau **cacat aksesibilitas menu yang sudah ada** (`FIN-CQ-09`), bukan ketiadaan kapabilitas.

Belum ada `/design-business-module` yang perlu dijalankan untuk sebagian besar butir — pekerjaan
tersisa adalah perbaikan (`FIN-CQ-09`, GR belum berlayar sendiri) dan tiga closure question di atas
untuk `Manajemen Klaim`/`Pemutihan Piutang`/`Ayat Silang`.

---

## 18. Impact scan terarah — kesiapan source untuk `FIN-DEC-111`..`117` (1 Oktober 2026, `7f8c3014` / `85578363b`)

**Pemicu.** Amandemen decision log "Penutupan gap Finance atas balasan Accounting `evidence/16`"
membuka `FIN-OQ-046` (apakah saldo per tanggal bisa dihitung tanpa tabel baru) dan `FIN-OQ-050`
(seberapa luas pola tanggal UTC). Pass ini **terbatas** pada dua pertanyaan itu ditambah apa yang
ditemukan di jalan. Bagian 1–17 tidak diaudit ulang; backend sudah bergerak dari `831ddb5d`, jadi
status di bagian itu yang menyentuh `AccountingIntegration` dan `BillingIntake` dianggap **stale**
sampai diaudit ulang. Frontend hanya dipindai pada tiga kata kunci (hasil di 18.6).

### 18.1 Temuan utama: Finance belum punya jalur pengiriman ke Accounting sama sekali

| ID | Kebutuhan | Pemilik | Bukti (`@7f8c3014`) | Status | Gap | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-050` | Pengiriman otomatis baris outbox ke Accounting | Finance | `Program.cs` baris 940–953 mendaftarkan 11 hosted service; **tidak satu pun milik Finance**. Tidak ada folder `Workers` di `FinanceManagement`. Grep `HttpClient`/pengirim pada `AccountingIntegration/Services` kosong. Roadmap `00-delivery-roadmap.md` baris 40 dan 235: `EPIC FIN-12` **ditunda atas keputusan pemilik** ("karena system saya belum perlu itu") | **`Missing`** (ditunda sengaja) | Seluruh baris `FinAccountingEventOutbox` tinggal `PENDING` selamanya. Gerbang worker `FIN-DES-059` / `FIN-OQ-035` yang dirujuk surat Finance 16 **tidak punya kode** untuk digerbang | Menyentuh **setiap janji "G4 siap" dan "snapshot otomatis"** di `evidence/15`, `21`, dan balasan Accounting `16` |
| `FIN-CAP-051` | Penjadwal snapshot saldo tanggal 1 pukul 00.05 WIB | Finance | `FinanceSubledgerSnapshotService` terdaftar `AddScoped` (`BillingManagementServiceCollectionExtensions.cs:99`); satu-satunya pemanggil `FinanceAccountingEventsController` `POST subledger-balances/generate` | **`Missing`** | Hanya bisa dipicu manual dari layar (`generate-subledger-snapshots-modal.jsx`). Pola yang ada: hosted service di dalam blok `runBackgroundJobs` (`AccAccountingEventSchedulerHostedService`, `LeaveAccrualSchedulerHostedService`, dst.) | Janji `FIN-DEC-092` tidak terpenuhi |
| `FIN-CAP-052` | Pemicu penanda shift | Finance | `FinanceBillingIntakeController` `POST cashier-shift-closure-markers/sync` (baris 80) — satu-satunya pemanggil `SyncCashierShiftClosureMarkersAsync` | **`Missing`** pemicu otomatis | Penanda hanya terbit bila seseorang menekan sinkronisasi | Accounting tidak akan pernah melihat shift tertutup tanpa tindakan manual |

### 18.2 `FIN-OQ-050` — luas pola tanggal UTC (dijawab)

| Hal | Hasil |
|---|---|
| Titik yang menghitung `AccountingDate` dari `UtcNow`/`UtcDateTime` | **20 titik** di lima service: `FinanceReceivableService` (344, 527, 663, 757), `FinanceSupplierPayableService` (253), `FinancePaymentService` (613, 629), `FinanceBillingIntakeService` (489, 664, 715, 743, 825, 906, 990, 1111, 1141), `FinanceReceiptService` (134, 207, 640, 753) |
| Titik yang aman (tanggal berasal dari dokumen) | `FinanceSupplierPayableService:130` (`SupplierInvoiceDate`), `FinancePurchasingInvoiceService:280`, `FinanceSupplierReturnService:188,207` (variabel `accountingDate`, asalnya **belum dibaca** — Unknown) |
| Helper WIB yang sudah ada | `Helpers/AppDateTimeHelper.cs` (`Asia/Jakarta`, `OperationalDate()`, `OperationalDateToUtc()`); pola serupa di `AdministrationFeePolicyService`, `NumberSeriesAllocator`. **Tidak satu pun dipakai oleh Finance.** Catatan: helper berada dalam namespace bersarang ganda (`QuilvianSystemBackend.Helpers.QuilvianSystemBackend.Helpers`) dan **belum punya** konversi `DateTimeOffset → DateOnly` WIB |
| Status | **`Reuse with adapter`** — zona waktunya ada, tetapi perlu satu metode baru dan 20 titik diubah |

Kasus berbahaya yang ikut ditemukan: `FinanceReceivableService` memakai `DateTime.UtcNow` untuk
tanggal pengakuan, dan snapshot piutang memakai `endOfPeriodUtc` berzona nol. Keduanya memotong
bulan tujuh jam lebih awal dari yang dilihat staf.

### 18.3 `FIN-OQ-046` — saldo per tanggal (dijawab sebagian)

| Akun | Data historis yang ada | Status | Catatan |
|---|---|---|---|
| Kas Kasir | `FinDailyCashSnapshot` per `CashDate` dengan `ClosingBalance` | **`Reuse with adapter`** | **Sumbernya baris rekap harian Finance (`OPEN`/`CLOSED`), bukan status `BilCashierShift`.** Surat Finance 21 bagian 3.2 menulis "shift `CLOSED`/`REVIEWED`" — salah baca kode. Keduanya dua hal berbeda; mana yang jadi dasar Kas Kasir adalah **keputusan terbuka** (`FIN-OQ-052`) |
| Kas Kecil | `FinPettyCashBudgetMovement.OccurredAt` + `BalanceAfter` | **`Reuse with adapter`** | Saldo per tanggal = `BalanceAfter` gerakan terakhir sampai akhir periode. Kode snapshot sekarang memakai `CurrentBalance` dan filter `Status = Active` **saat ini**, bukan saat itu |
| Piutang | `FinReceivable.OriginalAmount`, `RecognizedAt`; `FinReceiptAllocation.AllocatedAt` (+`IsReversal`); `FinReceivableAdjustment.ApprovedAt`; `FinReceivableWriteOff.ApprovedAt` | **`Extend`** | Dapat dihitung ulang: asli − alokasi bersih − penyesuaian disetujui − penghapusan disetujui, masing-masing sampai akhir periode. **Tanpa tabel baru.** Belum terbukti: apakah `FinReceiptDeduction` mengurangi `OutstandingAmount` terpisah dari alokasi (Unknown, `FIN-OQ-053`) |
| Utang supplier | `FinPayment.PaidAt`/`ApprovedAt` (induk dari `FinPaymentAllocation`, yang **tidak punya tanggal sendiri**); `FinPayableAdjustment.ApprovedAt`; `FinSupplierReturnDepositUsage.UsedAt`/`ReleasedAt` | **`Extend`** | Join ke induk pembayaran. `OutstandingAmount` dikurangi saat pembayaran **disetujui** (`FinancePaymentService:575`), bukan saat `PaidAt` — tanggal mana yang berlaku untuk posisi per tanggal belum diputuskan (Unknown) |

**Kesimpulan `FIN-OQ-046`:** posisi per tanggal **layak tanpa tabel baru** untuk Piutang, Utang, dan
Kas Kecil, dengan syarat dua Unknown di atas ditutup. Kas Kasir butuh keputusan sumber lebih dulu.

### 18.4 Bukti untuk `FIN-DEC-111`, `113`, `115`, `117`

| ID | Kebutuhan | Bukti | Status | Gap |
|---|---|---|---|---|
| `FIN-CAP-053` | Nomor shift dan metode bayar pada kejadian penerimaan | `FinReceipt.CashierShiftId`, `PaymentMethodId`, `PaymentMethodAccountId` sudah tersimpan (`FinanceReceiptService.cs:105-110`); `AccountingOutboxEventRequest` tidak membawanya; Accounting `ReceiveAccountingEventRequest` punya `AdditionalFields` (`JsonExtensionData`) sehingga kolom tambahan **tidak ditolak** | **`Extend`** | Tambah ke `AccountingOutboxEventRequest` dan `BuildPayloadJson`. **Conflict kecil:** baris pembalik menyalin `CashierShiftId` dari handoff **pembalikan** (`FinanceReceiptService.cs:165-180`), bukan dari kuitansi asli — contoh pada decision log yang menyebut "shift yang sama" **belum tentu benar** (`FIN-OQ-054`) |
| `FIN-CAP-054` | Pemetaan kelompok saldo → kode akun control | Hanya konstanta `SubledgerControlAccountDefaults` dan override opsional per permintaan (`CashierControlAccountCode` dst.) pada `GenerateSubledgerSnapshotsRequest`. Tidak ada tabel atau konfigurasi | **`Missing`** | Satu kode per kelompok, bukan daftar. `FIN-OQ-051` (migration) tetap berlaku |
| `FIN-CAP-055` | Deteksi shift terbuka | `BilCashierShift.Status` punya **tujuh** nilai: `OPEN`, `HANDED_OVER`, `CLOSED`, `CLOSED_WITH_VARIANCE`, `REVIEWED`, `REOPENED`, `PERLU_TINDAK_LANJUT`. Sinkronisasi sekarang hanya membaca `CLOSED`/`REVIEWED`/`REOPENED` (`FinanceBillingIntakeService.cs:1049-1054`) | **`Extend`** | `FIN-DEC-115` menyebut "shift terbuka" tanpa mendefinisikan apakah `HANDED_OVER` ikut. Definisi harus diputuskan (`FIN-OQ-055`) |
| `FIN-CAP-056` | Pengirim saldo utang honor dokter oleh Medical Fee (`FIN-DEC-117`) | `FinMedicalServicePayable` **ada di area Finance** (`Payable/Models`), bukan di Medical Fee. Kelas `MdfServiceFee`/`MdfFinanceHandoff` yang dirujuk **tidak ditemukan** di source. `BE-FIN-021` tertulis `BLOCKED` pada komentar model | **`Conflict`** dengan asumsi `FIN-DEC-117` | Premis "data honor dokter milik Medical Fee" tidak cocok dengan source: tabel utangnya milik Finance, dan Medical Fee belum ada. Opsi A `FIN-DEC-117` belum punya pelaksana (`FIN-OQ-056`) |

### 18.5 Koreksi atas pernyataan sebelumnya

| Pernyataan | Koreksi |
|---|---|
| Sesi `/grill-me` menyebut piutang yang lebih bayar akan terkirim `0.00` oleh `Math.Max` | **Belum terbukti dapat terjadi.** `CK_FinReceivable_Balance` menjaga `OutstandingAmount ≥ 0`, dan `FinancePaymentService:571` menolak alokasi melebihi sisa utang. Kelebihan bayar menjadi saldo tak teralokasi (`FinReceipt.UnallocatedAmount`), bukan piutang negatif. Pemotongan ke nol praktis **tidak aktif** untuk Piutang dan Utang; yang paling mungkin negatif adalah Kas Kasir (`ClosingBalance`). `FIN-DEC-112` tetap sah sebagai aturan, tetapi contohnya salah (`FIN-OQ-057`) |
| `evidence/21` bagian 3.2: Kas Kasir dari "shift `CLOSED`/`REVIEWED`" | Kode memakai `FinDailyCashSnapshot` berstatus `CLOSED`. Lihat 18.3 |

### 18.6 Frontend (terbatas)

`generate-subledger-snapshots-modal.jsx`, `finance-monitoring-view.jsx`,
`use-finance-subledger-balances.jsx`, dan `finance-monitoring-slice.jsx` mengonsumsi
`POST subledger-balances/generate` dan `GET subledger-balances/{period}`. Tidak ada tombol atau layar
untuk sinkronisasi penanda shift. Tidak ada konsumen untuk status "pernyataan ulang". Status:
**`Extend`**, bila `FIN-DEC-114` dan `FIN-DEC-115` dikerjakan. Kontrak respons belum dibandingkan
field per field.

### 18.7 Open question baru

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-052` | Kas Kasir bersumber dari rekap harian Finance (`FinDailyCashSnapshot`) atau dari status shift Billing? | Yasmin | `IMPLEMENTATION` `FIN-DEC-114` |
| `FIN-OQ-053` | Apakah `FinReceiptDeduction` mengurangi piutang terpisah dari alokasi? | Audit `FinanceReceiptService` baris 580–760 | `IMPLEMENTATION` `FIN-DEC-114` |
| `FIN-OQ-054` | Shift mana yang dipakai kuitansi pembalik: shift saat pembalikan terjadi atau shift kuitansi asli? | Yasmin, lalu Accounting | `IMPLEMENTATION` `FIN-DEC-111` |
| `FIN-OQ-055` | "Shift terbuka" mencakup status mana (`OPEN`, `HANDED_OVER`)? | Yasmin | `IMPLEMENTATION` `FIN-DEC-115` |
| `FIN-OQ-056` | Siapa pelaksana pengirim saldo honor dokter bila Medical Fee belum ada dan tabelnya milik Finance? | Yasmin | `LATER SLICE` `FIN-DEC-117` |
| `FIN-OQ-057` | Skenario nyata saldo negatif yang tersisa setelah invarian di 18.5? | Yasmin | Tidak memblokir `FIN-DEC-112` |
| `FIN-OQ-058` | **Apakah `EPIC FIN-12` (worker pengiriman) dibuka kembali?** Tanpa ini, `FIN-DEC-111`..`115` tidak pernah sampai ke Accounting | Yasmin | **Seluruh G4**; mengubah janji `evidence/15`, `21` |

### 18.8 Pemicu impact scan berikutnya

Ulangi bagian ini bila: backend bergerak dari `7f8c3014` pada `AccountingIntegration`,
`BillingIntake`, `Collection`, `Receivable`, atau `Payable`; `Program.cs` mendaftarkan hosted
service Finance; `AppDateTimeHelper` diubah; atau Accounting mengubah `ReceiveAccountingEventRequest`.

### 18.9 Koreksi atas 18.3 (ditemukan pada closure pass yang sama, 1 Oktober 2026)

**Dicabut:** kesimpulan "posisi per tanggal layak tanpa tabel baru" untuk **Piutang**.

| Hal | Bukti | Dampak |
|---|---|---|
| Pembayaran langsung piutang mengurangi `OutstandingAmount` **tanpa** `FinReceipt`/`FinReceiptAllocation` | `FinanceReceivableService.RecordPaymentAsync`, baris 621–701; jejak hanya baris outbox `PENERIMAAN-PIUTANG` dan `AuditAsync("RecordPayment")` | Rumus "asli − alokasi − penyesuaian − penghapusan" **melewatkan** pembayaran langsung. Status Piutang berubah dari `Extend` menjadi `Missing` (buku mutasi) |
| Potongan (`FinReceiptDeduction`) mengurangi piutang terpisah dari alokasi | `FinanceReceiptService.cs:611` memanggil `ApplyAllocationAsync` sendiri | `FIN-OQ-053` **CLOSED**; potongan harus menjadi mutasi tersendiri |
| Penghapusan langsung **punya** baris | `FinanceReceivableService.DirectWriteOffAsync`, baris 729–749 | Aman |
| Utang supplier dan utang jasa medis | `FinancePaymentService.cs:575` mengurangi saldo saat pembayaran **disetujui**; `FinPaymentAllocation` tanpa tanggal | Status tetap `Extend` dengan Unknown; dibuka `FIN-OQ-059` |

Status `FIN-CAP-050` menjadi **`Missing` → disetujui dibangun** (`FIN-DEC-118`); `FIN-CAP-056` (honor dokter)
tidak lagi `Conflict` karena `FIN-DEC-122` mengikuti source. `FIN-CAP-057` baru: **buku mutasi Piutang dan
Utang supplier** — `Missing`, disetujui dibangun (`FIN-DEC-123`), migration menunggu konfirmasi terpisah.

---

## 19. Impact scan terarah — `FIN-OQ-059`, `FIN-OQ-062`, dan kesiapan migrasi `FIN-DEC-129` (1 Oktober 2026, `7f8c3014` / `0b54fdce6`)

**Pemicu.** Closure pass decision log (`FIN-DEC-118`..`129`) menyisakan dua pertanyaan fakta source dan
dua pertanyaan tambahan dari `FIN-DEC-129`. Pass ini **terbatas** pada keempatnya.

**Staleness.** Backend tidak bergerak (`7f8c3014`, sama dengan bagian 18). **Frontend bergerak dari
`85578363b` ke `0b54fdce6`**; pass ini tidak mengaudit ulang frontend, sehingga temuan 18.6 dianggap
stale sampai dipindai ulang.

### 19.1 `FIN-OQ-059` — jalur ubah saldo utang tanpa riwayat (dijawab)

| Jalur | Bukti (`@7f8c3014`) | Menulis riwayat bertanggal? | Status |
|---|---|---|---|
| Pembayaran utang supplier lewat dokumen pembayaran | `FinancePaymentService.cs:575` | **Ya**: induk `FinPayment` (`ApprovedAt`, `PaidAt`) dan `FinPaymentAllocation`. Alokasi tidak punya tanggal sendiri | `Reuse with adapter` |
| **Pembayaran langsung utang supplier** | `FinanceSupplierPayableService.RecordDirectPaymentAsync`, baris 207–274 | **Tidak.** Mengurangi `OutstandingAmount` dan menambah `PaidAmount`; hanya meninggalkan satu baris outbox `PEMBAYARAN-HUTANG-SUPPLIER` dan satu catatan audit. Parameter `bankAccountId`, `paymentMethod`, dan `notes` **diterima tetapi tidak disimpan** | **`Repair`** |
| Penyesuaian utang | `FinanceSupplierPayableService.cs:450,455`; `FinPayableAdjustment.ApprovedAt` | Ya | `Reuse with adapter` |
| Pembuatan utang | `FinanceSupplierPayableService.cs:99-139` | Ya (`SupplierInvoiceDate`) | `Reuse with adapter` |
| Utang jasa medis (`FinMedicalServicePayable`) | Satu-satunya rujukan di `FinancePaymentService.cs:103,112`: `MedicalServicePayableId = null`, komentar "belum aktif — modul Medical Fee belum ada" | **Tidak ada penulis sama sekali** | **`Missing`** |
| Kredit retur supplier yang dipakai melunasi utang | `FinSupplierReturnDepositUsage` (`UsedAt`, `ReleasedAt`), `FinPayment.DepositAppliedAmount` | Sebagian | **`Unknown`** — jalur persisnya belum ditelusuri |

**Jawaban `FIN-OQ-059`: ya, ada satu jalur yang bocor — pembayaran langsung utang supplier.** Bentuknya
sama persis dengan pembayaran langsung piutang (F11). Pembayaran langsung piutang dan utang
sama-sama membuang metode, bank, dan catatan, sehingga `FIN-DEC-126` (metode dan bukti **MUST**
disimpan) hanya menyentuh piutang; **utang supplier belum punya keputusan setara** (`FIN-OQ-067`).

### 19.2 `FIN-OQ-062` — bukti pembayaran (dijawab)

| Hal | Bukti | Status |
|---|---|---|
| `FinReceivableDocument` | `Receivable/Models/FinReceivableDocument.cs`: melacak **kelengkapan berkas klaim** (`DocumentType`, `DocumentNumber`, `IsReceived`, `Notes`), terikat `ReceivableId`, **tanpa kolom berkas**, tanpa konsumen selain properti navigasi | **Tidak cocok dipakai ulang** — tujuannya klaim ke penjamin, bukan bukti bayar, dan secara komentar sendiri melarang dikaitkan dengan pengakuan nilai |
| Bukti pembayaran Finance | `FinReceipt` hanya punya `ProviderReference`/`ProviderEventId` dari Billing; pembayaran langsung tidak menyimpan `referenceNumber` maupun `notes` | **`Missing`** |
| Pola penyimpanan berkas | Unggah `IFormFile`, akar penyimpanan dari konfigurasi `FileStorage:UploadRootPath`, `UseStaticFiles` (`Program.cs:1391-1419`). Kelas yang ada **semuanya milik HR**: `WorkflowFileStorageService`, `WfpCertificationFileStorageService`, `LeaveRequestAttachmentService`. **Tidak ada layanan penyimpanan bersama** | **`Reuse with adapter`** hanya sebagai **pola**; memakai kelas HR langsung melintasi batas modul |

### 19.3 Kesiapan migrasi `FIN-DEC-129`

| Pertanyaan | Hasil | Status |
|---|---|---|
| Bisakah jalur pengakuan piutang dilewati untuk item migrasi? | Pengakuan terjadi **di dalam** jalur intake Billing (`FinanceBillingIntakeService.cs:437-494`) dan terikat `BilArHandoff`. Migrasi **tidak boleh** memakai jalur itu (tidak ada handoff Billing). Jadi "melewati" berarti **jalur pembuatan baru**, bukan mengubah intake | **`Extend`** |
| Apakah `FinReceivable` dapat menampung item tanpa Billing? | `SourceHandoffKey`, `SourceHandoffId`, `InvoiceId` adalah `Guid` **tidak nullable** dan `SourceHandoffKey` punya indeks unik (`FinReceivableConfiguration.cs:46`). Item migrasi tidak punya satu pun | **`Repair`**: perlu kolom nullable, penanda migrasi, dan perubahan constraint (migration) |
| Apakah pembuatan utang supplier dapat dibuat tanpa kejadian? | `FinanceSupplierPayableService` membuat `PENGAKUAN-HUTANG-SUPPLIER` **tanpa syarat** (baris 125–135) dan nilai kejadian harus lebih dari nol. Tidak ada parameter untuk menahannya | **`Extend`**: perlu parameter eksplisit; `SupplierId` wajib ada di master supplier |
| Mekanisme impor yang bisa dipakai ulang? | Tidak ada layanan impor di `Areas`, tidak ada paket spreadsheet di `.csproj`, tidak ada konsumen unggah berkas di Finance | **`Missing`** |
| Pencocokan dengan Accounting | Accounting sudah punya rekonsiliasi subledger pada tutup periode (`AccPeriodClosingService`, `HitungRekonsiliasiSubledgerAsync`, kode `SUBLEDGER_RECONCILIATION`) | `Reuse with adapter` — sisi Accounting; cara Finance membuktikan total migrasi tetap harus dirancang |

### 19.4 Open question baru dan efek ke yang lama

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-059` | **CLOSED** (19.1) | — | — |
| `FIN-OQ-062` | **CLOSED** (19.2) — tidak memakai ulang `FinReceivableDocument` | — | — |
| `FIN-OQ-067` | Pembayaran langsung **utang supplier** punya bentuk yang sama dengan piutang: apakah `FIN-DEC-126` (tunai/non-tunai satu mekanisme, metode dan bukti disimpan) berlaku juga untuk utang? | Yasmin | `IMPLEMENTATION` `FIN-DEC-123` bagian Utang |
| `FIN-OQ-068` | Penyimpanan bukti pembayaran: memakai pola unggah HR dengan layanan baru milik Finance, atau menunggu layanan berkas bersama dari Platform? | Yasmin; Platform bila dibutuhkan | `IMPLEMENTATION` `FIN-DEC-126` |
| `FIN-OQ-069` | Apakah piutang sewa non-pasien lama (`FinNonPatientReceivable`) ikut dimigrasikan? `FIN-DEC-129` hanya menyebut piutang dan utang supplier | Yasmin | `LATER SLICE` |
| `FIN-OQ-070` | Dari mana asal data migrasi (sistem lama, spreadsheet, atau input manual) dan siapa yang menyiapkannya? Menentukan bentuk impor `FIN-OQ-066` | Yasmin | `DESIGN` `FIN-DEC-129` |

### 19.5 Dampak ke keputusan yang sudah ada

1. `FIN-DEC-123`: buku mutasi **MUST** mencakup `RecordDirectPaymentAsync` utang supplier, bukan hanya piutang.
2. `FIN-DEC-129`: butuh migration untuk membuat `SourceHandoffKey`, `SourceHandoffId`, dan `InvoiceId` nullable dengan constraint "diisi bila bukan migrasi". Itu **bukan** perubahan kecil pada tabel yang sudah dipakai; masuk `FIN-OQ-051`.
3. `FIN-DEC-126`: tidak boleh memakai `FinReceivableDocument`; bukti perlu tempat penyimpanan baru.

### 19.6 Pemicu impact scan berikutnya

Ulangi bagian 19 bila backend bergerak dari `7f8c3014` pada `Receivable`, `Payable`, `Collection`, atau
`BillingIntake`; bila layanan penyimpanan berkas bersama muncul di Platform; atau bila `Program.cs`
mendaftarkan hosted service Finance.


---

## 20. Impact scan terarah — hasil `REV-14A`..`14E` (`BE-FIN-058`..`085`, `FE-FIN-025`..`032`), 4 Oktober 2026 (`5d6bb8bf` / `ae2ed334e`)

**Pemicu.** Ketiga pemicu bagian 19.6 terpenuhi sekaligus: backend bergerak pada `Receivable`, `Payable`,
dan `BillingIntake`; layanan penyimpanan berkas milik Finance muncul; dan `Program.cs` kini mendaftarkan
tiga hosted service Finance. Pass ini juga dipicu permintaan owner sesudah `/grill-me` 4 Oktober 2026
(`FIN-DEC-141`..`160`), supaya desain berikutnya berpijak pada keadaan source yang benar.

**Staleness dan baseline.** Baseline yang berlaku adalah bagian 19 (backend `7f8c3014`, frontend
`0b54fdce6`), **bukan** `backend_source_sha` di kepala dokumen. Sejak itu backend bergerak **36 commit**
(81 berkas di bawah `Areas/Corporate/FinanceManagement`, +8.127/−292) dan frontend bergerak dengan
**80 berkas Finance** berubah. Bagian 1–19 di bawah **tidak** diaudit ulang menyeluruh; hanya klaster
yang disebut eksplisit di sini yang diverifikasi pada SHA ini.

**Keadaan working tree saat audit.** Backend: satu berkas dokumentasi berubah dan belum di-commit
(`docs/module-blueprints/finance-management/00-interview-decisions.md` — hasil `/grill-me` pass yang sama;
**nol** source aplikasi). Frontend: **bersih**. Seluruh source `BE-FIN-058`..`085` dan `FE-FIN-025`..`032`
sudah ter-commit, sehingga bukti di bawah tertambat pada SHA, bukan pada working tree.

**Batas audit pass ini.** Klaster **Financial** (buku mutasi subledger, pemetaan akun control, saldo awal
cutover, batch migrasi tagihan lama, bukti transaksi, master ambang), **External Integration** (permukaan
posisi/selisih/snapshot dan tiga hosted service), dan **Authorization/Audit** (empat resource hak akses
baru), ditambah konsumen frontend-nya. **Tidak** mengaudit ulang internal Billing maupun Accounting
(`FIN-OOS-001`..`004`), dan **tidak** memverifikasi keadaan basis data (lihat 20.6).

### 20.1 Lima gap bagian 18/19 yang kini TERTUTUP

| Gap lama | Status lama | Bukti sekarang (`@5d6bb8bf`) | Status baru |
|---|---|---|---|
| Finance belum punya jalur pengiriman ke Accounting sama sekali (18.1) | `Missing` | `Program.cs:975,981,987` mendaftarkan `FinanceAccountingDispatchWorker`, `FinanceSubledgerSnapshotSchedulerHostedService`, `FinanceCashierShiftMarkerSchedulerHostedService` | **`Ready to reuse` dengan catatan 20.4** — terdaftar, tetapi dibangun **mati** |
| `FinReceivable` tidak dapat menampung item tanpa Billing; `SourceHandoffKey`/`SourceHandoffId`/`InvoiceId` tidak nullable (19.3) | `Repair` | `Receivable/Models/FinReceivable.cs:29,32,34` — ketiganya kini `Guid?` (`BE-FIN-079`) | **`Ready to reuse`** |
| Pembayaran langsung utang supplier tidak menulis riwayat bertanggal (19.1) | `Missing` | `FinanceSupplierPayableService.cs:151,374,629` dan `FinancePaymentService.cs:598` memanggil `RecordSupplierPayableMovementAsync`; penulis tunggalnya `FinanceSubledgerMovementService.cs:205` | **`Ready to reuse`** |
| Tidak ada mekanisme impor yang dapat dipakai ulang (19.3) | `Missing` | `IOpeningItemFileReader` + `CsvOpeningItemFileReader` terdaftar (`BillingManagementServiceCollectionExtensions.cs:127`); dua templat nyata ada di `Storage/templates/finance/` | **`Extend`** — jalur CSV berjalan, pembaca XLSX belum ada (`FIN-DEC-149`) |
| Bukti pembayaran tidak punya tempat penyimpanan (19.2) | `Missing` | `FinanceTransactionProofService` terdaftar (baris 124); tiga endpoint pada `FinanceTransactionProofsController` | **`Reuse with adapter`** — ada, tetapi **tidak dapat dipakai** sebelum konfigurasi diisi (20.4) |

### 20.2 Satu gap bagian 19 yang TETAP terbuka

| Gap | Bukti | Status |
|---|---|---|
| Utang jasa medis (`FinMedicalServicePayable`) tanpa penulis sama sekali | Pencarian `FinMedicalServicePayables.Add` dan `new FinMedicalServicePayable` di seluruh source **di luar** `Migrations/`: **nol hasil**. Snapshot tetap mengirimnya `0,00` | **`Missing`** — tidak berubah sejak 19.1 |

Ini **sesuai keputusan**, bukan cacat: `FIN-DEC-157` (4 Oktober 2026) menetapkan utang jasa medis lama
**tidak** dimigrasikan pada rilis ini dan saldo awalnya tetap nol.

### 20.3 Kepemilikan backend — wiring, persistence, dan hak akses

Seluruh pemeriksaan "kemampuan ini benar-benar hidup" dijawab bukti, bukan keberadaan berkas.

| Hal yang diperiksa | Hasil | Bukti |
|---|---|---|
| Delapan tabel baru terdaftar pada `ApplicationDbContext` | **Ya, delapan-delapannya** | `ApplicationDbContext.cs:653,666,686,694,695,698,703,710` |
| Delapan layanan baru terdaftar DI (`AddScoped`) | **Ya** | `BillingManagementServiceCollectionExtensions.cs:95,104,106,108,110,124,127,129` |
| Empat resource hak akses baru dipasang pada controller | **Ya**: `FinanceSubledgerSetup` (10 titik), `FinanceOpeningItemBatch` (9), `FinanceTransactionProof` (3), `MstDirectPaymentThreshold` (2) | Keempat controller terkait |
| Lima migration Finance baru ada di `Migrations/` | **Ya** | `AddFinanceSubledgerMovementLedgers`, `AddFinanceSubledgerSetup`, `AddFinanceTransactionProofAndDirectPaymentThreshold`, `AddFinanceOpeningItemMigration`, `RelaxFinCashMovementAmountForZeroOpeningBalance` |
| Templat berkas migrasi nyata, bukan hanya jalur di kode | **Ya, dua berkas** | `Storage/templates/finance/opening-item-receivable.csv`, `opening-item-supplier-payable.csv` |

**Catatan pola yang perlu diketahui, bukan cacat baru.** Seluruh layanan Finance didaftarkan di
`Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs` — berkas
registrasi milik **Billing**, bukan Finance. Pola ini sudah berlaku sejak sebelum pass ini dan diikuti apa
adanya oleh `BE-FIN-058`..`085`. Akibatnya: orang yang mencari pendaftaran layanan Finance tidak akan
menemukannya di folder Finance. Dicatat sebagai `FIN-CAP-060`, bukan diperbaiki di audit ini.

### 20.4 Temuan paling material — nol konfigurasi Finance di `appsettings.json`

**Fakta.** Pencarian `"Finance` pada `appsettings.json` memulangkan **nol hasil**. Tidak ada blok
`FinanceManagement`, tidak ada blok `Finance:AccountingDispatch`, `Finance:SubledgerSnapshotScheduler`,
maupun `Finance:CashierShiftMarkerScheduler`.

**Akibat nyata, dan seluruhnya fail-closed (berperilaku aman, bukan diam-diam salah):**

| Kemampuan | Perilaku hari ini | Bukti |
|---|---|---|
| Unggah bukti pembayaran (`POST /transaction-proofs`) | **Selalu ditolak `503`** beserta pesan *"batas ukuran berkas belum dikonfigurasi"* | `FinanceTransactionProofService.cs:137-141` |
| Worker pengiriman baris outbox ke Accounting | **Mati.** `Enabled = false` adalah nilai bawaan dan tidak ada konfigurasi yang menghidupkannya | `Program.cs:969-975` |
| Penjadwal snapshot saldo subledger harian | **Mati**, alasan sama | `Program.cs:976-981` |
| Penjadwal penanda shift kasir | **Mati**, alasan sama | `Program.cs:982-987` |

**Artinya untuk layar yang sudah dibangun.** `FE-FIN-030` (pembayaran langsung berkontrol) memakai bukti
pembayaran yang **wajib**. Karena unggah bukti selalu `503`, jalur pembayaran langsung piutang dan utang
supplier **belum dapat dipakai sama sekali** hari ini — bukan karena layarnya salah, melainkan karena satu
nilai konfigurasi belum diisi. Layar sudah menangani `503` sebagai keadaan nyata (tombol dinonaktifkan
beserta arahan menghubungi administrator), sehingga petugas melihat pesan yang benar, bukan galat teknis.

**Hubungannya dengan keputusan 4 Oktober 2026.** `FIN-DEC-156` baru menetapkan batas ukuran **10 MB**
(10.485.760 byte). Nilai itu **belum** masuk `appsettings.json`. Jadi temuan ini bukan keputusan yang
hilang, melainkan **langkah serah terima yang belum dikerjakan**, dan sudah tercatat sebagai prasyarat
go-live pada decision log.

**Contoh konkret.** Petugas AR membuka layar pembayaran langsung piutang hari ini, memilih metode transfer,
lalu memilih berkas kuitansi `.jpg` berukuran 2 MB. Sistem menolak dengan *"Unggah bukti belum dapat
dipakai: batas ukuran berkas belum dikonfigurasi. Hubungi administrator."* Setelah administrator menuliskan
`FinanceManagement:TransactionProof:MaxFileSizeBytes` bernilai `10485760`, berkas yang sama diterima dan
pembayaran dapat dicatat.

### 20.5 Konsumen frontend — dapat dicapai, tetapi belum ada di menu

| Hal yang diperiksa | Hasil | Bukti |
|---|---|---|
| Rute baru benar-benar ada | **Lima rute**: `subledger-setup/control-accounts`, `subledger-setup/opening-balances`, `subledger-setup/opening-item-batches`, `master-data/direct-payment-threshold`, `cash-management/movements` | `src/app/finance/**` |
| Reducer baru terdaftar pada store | **Empat-empatnya** | `src/lib/state/store.jsx:408,413,414,415` |
| Endpoint unggah ulang batch (`BE-FIN-085`) sudah dikonsumsi | **Ya**, di lima berkas (utilitas, slice, hook, view, panel rincian) | `rg "reupload" src` |
| Test unit Finance | **Delapan berkas**; empat yang terbaru memuat 22, 11, 12, dan 26 test | `tests/unit/` |
| **Butir menu untuk kelima layar baru** | **Nol.** Pencarian `subledger`, `threshold`, `opening-item`, `cash-management/movements` pada berkas menu memulangkan nol hasil | `src/utils/menu-sidebar/menu-items.jsx` (2.129 baris) |

**Artinya.** Kelima layar hanya dapat dicapai lewat alamat langsung dan tautan silang antar layar. Ini
**sesuai** keadaan `FIN-OQ-079` saat layar dibangun. Namun `FIN-OQ-079` sudah **ditutup** `FIN-DEC-148` dan
`FIN-DEC-160` (4 Oktober 2026): submenu "Cutover & Subledger" beserta urutan butirnya sudah diputuskan, dan
Ambang masuk submenu Master Data. Jadi sekarang ada **selisih antara keputusan dan source**: keputusannya
ada, pemasangannya belum. Submenu Master Data yang menjadi rumah bagi Ambang memang sudah ada
(`menu-items.jsx:697,703,709` — Kategori Kas Kecil, Mata Uang, Rekening Bank), sehingga penempatannya tidak
menuntut struktur baru.

### 20.6 Yang TIDAK dapat dibuktikan audit ini

| Hal | Alasan | Status |
|---|---|---|
| Apakah kelima migration Finance sudah diterapkan ke basis data | Audit ini membaca source, bukan basis data. Owner melaporkan `dotnet build` dan penerapan migrasi **berhasil** (3–4 Oktober 2026); laporan itu dicatat apa adanya dan **bukan** hasil pengamatan audit | **`Unknown`** terhadap keadaan basis data |
| Apakah `dotnet ef migrations has-pending-model-changes` bersih | Belum dilaporkan. Penting karena migration `RelaxFinCashMovementAmountForZeroOpeningBalance` beserta snapshot-nya **ditulis tangan**, bukan dihasilkan `dotnet ef` | **`Unknown`** |
| Apakah jalur XLSX menghasilkan baris terurai yang identik dengan CSV | Pembaca XLSX belum ada (`FIN-DEC-149`), sehingga kriteria penerimaan paritas format belum dapat diuji | **`Missing`** — sesuai keputusan |
| Perilaku runtime layar `FE-FIN-025`..`032` | Audit ini tidak menjalankan aplikasi | Di luar batas audit |

### 20.7 Conflict — satu, dan sifatnya lintas modul

#### `FIN-CQ-10` — prop `DataTable` yang tidak dikenal, dan 15 layar Finance kehilangan kalimat keadaan kosongnya

> **ANGKA PADA BAGIAN INI DIKOREKSI 20.14.** Hitungan "191 pemakaian app-wide / 34 di Finance" di bawah
> **terlalu besar**: ia menghitung pemakaian pada komponen lain yang memang memiliki prop tersebut.
> Angka yang benar adalah **59 pemakaian** pada `DataTable` (Finance 27, health-services 32). Conflict ini
> juga sudah **CLOSED** oleh alias pada `DataTable`. Baca 20.14 lebih dulu.

**Fakta.** Komponen bersama `DataTable` hanya membaca `rowKey`, `emptyTitle`, dan `emptyDescription`
(`data-table.jsx:135,138`). Sementara itu layar-layar memberinya `getRowId`, `emptyText`, dan
`emptyMessage` — nama yang **tidak dikenal komponen** dan karena itu diabaikan tanpa peringatan apa pun.

**Luasnya, dan ini penting untuk menentukan pemiliknya:**

| Lingkup | Jumlah pemakaian prop tak dikenal |
|---|---|
| `view/finance` | **34** (20 berkas) |
| `view/health-services` | 126 |
| `view/corporate` | 6 |
| `view/administrator` | 5 |
| **Total seluruh aplikasi** | **191** |

**Akibat yang terlihat pengguna, diukur per berkas — bukan disamaratakan.** Dari 20 berkas Finance yang
memakai prop tak dikenal, **lima** ternyata **juga** memberikan `emptyTitle` yang benar, sehingga kalimat
keadaan kosongnya tetap tampil dan prop tak dikenal itu hanya menjadi kode mati:

| Berkas | Keadaan |
|---|---|
| `cash-movements-view.jsx`, `finance-cash-management-view.jsx`, `finance-monitoring-view.jsx`, `finance-receivable-detail-view.jsx`, `goods-receipt-view.jsx` | **Aman.** `emptyTitle` diberikan; kalimatnya tampil |
| 15 berkas sisanya (antara lain `finance-ap-aging-view.jsx`, `finance-ar-report-view.jsx`, `non-patient-receivable-view.jsx`, `subledger-control-accounts-view.jsx`, `purchase-order-form-view.jsx`) | **Terdampak.** Tidak memberikan `emptyTitle` sama sekali, sehingga pengguna selalu membaca kalimat bawaan *"Data tidak ditemukan."* |

Khususnya untuk acceptance criteria `FE-FIN-025`/`026` yang menuntut keadaan kosong berbunyi "buku mutasi
baru berjalan sejak tanggal cutover": kriteria itu **tetap terpenuhi**, karena `cash-movements-view.jsx`
memberikan kalimat itu lewat `emptyTitle` (baris 177). Prop `emptyMessage` di sebelahnya yang mati.

**Akibat kedua, pada kunci baris.** `getRowId` yang diabaikan (7 pemakaian) membuat kunci baris jatuh ke
bawaan `item.id || item.Id || item.key || item.code || row-<indeks>`. Untuk tabel yang barisnya dikenali
lewat kolom lain, kuncinya menjadi nomor urut, dan itu dapat membuat React salah memasangkan baris ketika
urutannya berubah.

**Rincian pemakaian di Finance:** `emptyMessage=` 19 kali, `emptyText=` 8 kali, `getRowId=` 7 kali.

**Pemilik dan batas.** Karena 157 dari 191 pemakaian berada **di luar** Finance, bentuk akhir kontrak
komponen `DataTable` adalah urusan pemilik komponen bersama (Platform/UI), **bukan** Finance. Yang menjadi
milik Finance adalah 34 pemakaian di dalam `view/finance`, dan yang benar-benar merugikan pengguna adalah
15 berkas yang tidak memberikan `emptyTitle`. Satu tabel sudah diselaraskan saat `FE-FIN-029` (tabel
snapshot saldo subledger).

**Status:** `Repair` untuk sisi Finance; titik sentuh untuk sisi komponen bersama. **Tidak** diperbaiki pada
audit ini — audit tidak pernah menyentuh source.

### 20.8 Entri kemampuan baru

| ID | Kebutuhan | Pemilik | Bukti (`repo/path#symbol@SHA`) | Status | Gap/adapter | Risiko |
|---|---|---|---|---|---|---|
| `FIN-CAP-058` | Buku mutasi bertanggal untuk piutang, utang supplier, dan kas | Finance | `backend/Areas/Corporate/FinanceManagement/AccountingIntegration/Services/FinanceSubledgerMovementService.cs#RecordSupplierPayableMovementAsync@5d6bb8bf` | `Ready to reuse` | — | Rendah. Penulis tunggal; keempat pemanggil lewat satu pintu |
| `FIN-CAP-059` | Pemetaan akun control dan saldo awal cutover | Finance | `backend/.../AccountingIntegration/Controllers/FinanceSubledgerSetupController.cs@5d6bb8bf`; `frontend/src/app/finance/subledger-setup/**@ae2ed334e` | `Ready to reuse` | Butir menu belum dipasang (20.5) | Rendah |
| `FIN-CAP-060` | Pendaftaran DI layanan Finance | Platform/Billing | `backend/Areas/HealthServices/BillingManagement/Billing/BillingManagementServiceCollectionExtensions.cs:95-129@5d6bb8bf` | `Reuse with adapter` | Layanan Finance didaftarkan di berkas milik Billing | Rendah, tetapi menyulitkan penelusuran |
| `FIN-CAP-061` | Batch migrasi tagihan lama lewat berkas | Finance | `backend/.../FinanceOpeningItemBatchService.cs@5d6bb8bf`; `backend/Storage/templates/finance/*.csv`; `frontend/src/app/finance/subledger-setup/opening-item-batches@ae2ed334e` | `Extend` | Pembaca XLSX belum ada (`FIN-DEC-149`); batas 10.000 baris (`FIN-DEC-155`) belum ditegakkan di kode | Sedang. Paritas dua format belum dapat diuji |
| `FIN-CAP-062` | Penyimpanan bukti pembayaran milik Finance | Finance | `backend/.../Collection/Services/FinanceTransactionProofService.cs@5d6bb8bf` | `Reuse with adapter` | **Tidak dapat dipakai** sampai `MaxFileSizeBytes` diisi (20.4) | **Tinggi.** Menahan seluruh jalur pembayaran langsung |
| `FIN-CAP-063` | Master ambang pembayaran langsung | Finance | `backend/.../MasterData/Services/DirectPaymentThresholdService.cs@5d6bb8bf`; `frontend/src/app/finance/master-data/direct-payment-threshold@ae2ed334e` | `Extend` | Belum ada penanda versi (`FIN-DEC-146`); kolom tanggal berlaku masih ada (`FIN-DEC-153`); nama pengubah belum dikirim (`FIN-DEC-151`) | Sedang. Perubahan bersamaan masih saling menimpa |
| `FIN-CAP-064` | Jalur pengiriman Finance ke Accounting beserta penjadwalnya | Finance | `backend/Program.cs:975,981,987@5d6bb8bf` | `Ready to reuse` | Ketiganya **mati** tanpa konfigurasi (20.4); pengaktifan menunggu G3 | Sedang |
| `FIN-CAP-065` | Permukaan baca posisi saldo dan selisih kas | Finance | `backend/.../Services/FinanceSubledgerBalanceCalculator.cs@5d6bb8bf`; `frontend/src/components/view/finance/monitoring/subledger-balances@ae2ed334e` | `Extend` | Periode tanpa rekap kas harian masih memulangkan nol dan selisih palsu (`FIN-DEC-152` belum dikerjakan) | Sedang. Angka nol dapat disangka saldo oleh pembaca API lain |
| `FIN-CAP-066` | Keadaan kosong tabel pada layar Finance | Finance | `frontend/src/components/view/finance/**` (34 pemakaian prop tak dikenal pada 20 berkas)`@ae2ed334e` | `Repair` | 15 berkas tidak memberikan `emptyTitle`, sehingga keadaan kosongnya selalu berbunyi "Data tidak ditemukan" (20.7) | Sedang. Tidak menyentuh acceptance criteria `FE-FIN-025`/`026` — keduanya tetap terpenuhi lewat `emptyTitle` |

### 20.9 Fakta, inferensi, dan rekomendasi

**Fakta** (dapat diperiksa ulang pada SHA di atas): seluruh isi 20.1–20.8 bertanda bukti berkas dan baris.

**Inferensi** (kesimpulan audit, bukan bacaan langsung):

1. Jalur pembayaran langsung piutang dan utang supplier **belum dapat dipakai pengguna** hari ini, karena
   bukti pembayaran wajib sedangkan unggahnya selalu `503`. Ini inferensi dari dua fakta — bukti wajib, dan
   konfigurasi kosong — bukan dari pengamatan aplikasi berjalan.
2. Delapan keputusan 4 Oktober 2026 (`FIN-DEC-145`, `146`, `147`, `150`, `151`, `152`, `153`, `155`) **belum
   tercermin di source**. Itu wajar: keputusannya lahir **sesudah** source dibangun. Tetapi berarti source
   hari ini **tidak** lagi menjadi rujukan perilaku target.
3. Menu adalah satu-satunya hal yang menahan kelima layar baru dari dapat ditemukan petugas; semua jalur
   data di belakangnya sudah hidup.

**Rekomendasi** (bukan keputusan, dan bukan pekerjaan audit ini):

1. Isi `FinanceManagement:TransactionProof:MaxFileSizeBytes` bernilai `10485760` sebelum apa pun yang lain —
   satu nilai ini membuka seluruh jalur pembayaran langsung.
2. Minta hasil `dotnet ef migrations has-pending-model-changes` sebelum pass desain, karena satu migration
   ditulis tangan.
3. Perbaiki 15 layar `FIN-CAP-066` dengan memberi `emptyTitle`; jangan menunggu pemilik komponen bersama,
   karena perbaikan per layar tidak menyentuh 157 pemakaian modul lain.
4. Bawa selisih keputusan-vs-source (inferensi 2) ke `/design-business-module`, lalu
   `/plan-module-delivery`, supaya penyesuaiannya menjadi task bernomor — termasuk `FIN-CAP-066` yang
   menyentuh acceptance criteria task yang sudah ditandai selesai.

### 20.10 Yang TIDAK ditutup pass ini

| Hal | Alasan |
|---|---|
| Internal Billing dan Accounting | `FIN-OOS-001`..`004`; hanya titik sentuh yang dibaca |
| Keadaan basis data dan hasil kompilasi | Di luar kemampuan audit source (20.6) |
| Bentuk akhir kontrak komponen `DataTable` | 157 dari 191 pemakaian di luar Finance; milik pemilik komponen bersama |
| Bagian 1–19 dokumen ini | Tidak diaudit ulang menyeluruh; tetap tertambat pada SHA masing-masing |
| `FIN-CQ-08` dan `FIN-CQ-09` | Tidak diperiksa ulang pada pass ini; status terakhirnya tetap berlaku |

### 20.11 Open question baru

| ID | Pertanyaan | Pemilik | Memblokir |
|---|---|---|---|
| `FIN-OQ-084` | Apakah 15 layar Finance yang keadaan kosongnya berbunyi "Data tidak ditemukan" diperbaiki satu per satu dengan memberi `emptyTitle`, atau pemilik komponen bersama lebih dulu diminta menerima nama prop lama sebagai alias supaya 157 pemakaian modul lain ikut terbantu sekaligus? | Yasmin untuk sisi Finance; pemilik komponen bersama untuk aliasnya | `IMPLEMENTATION` perbaikan `FIN-CAP-066`. Tidak memblokir `DESIGN` |

### 20.12 Pemicu impact scan berikutnya

Ulangi bagian 20 bila salah satu terjadi:

1. `appsettings.json` mulai memuat blok `FinanceManagement` atau `Finance:*` — keempat baris 20.4 berubah
   artinya seketika.
2. Butir menu kelima layar baru dipasang (`FIN-DEC-148`, `160`).
3. Keputusan `FIN-DEC-145`..`156` mulai dikerjakan di source — terutama penanda versi ambang, penghapusan
   kolom tanggal berlaku, dan penanda *belum ada rekap* pada selisih kas.
4. Paket pembaca XLSX ditambahkan pada `QuilvianSystemBackend.csproj` (`FIN-OQ-081`).
5. Salah satu dari tiga hosted service Finance dihidupkan.
6. Penulis `FinMedicalServicePayable` muncul (membuka kembali `FIN-DEC-157`).

### 20.13 Handoff

| Field | Nilai |
|---|---|
| Blueprint | `FIN-BP-001`, revisi 15, status `approved` |
| Decision ID yang relevan | `FIN-DEC-111`..`140` (dasar pembangunan), `FIN-DEC-141`..`160` (keputusan 3–4 Oktober 2026 yang belum tercermin di source) |
| Contract version yang berlaku | `FIN-API-1.5`/`1.6`, `FIN-STATE-1.6`, `FIN-VAL-1.7`/`1.8`, `FIN-PERM-1.7`/`1.8` |
| Backend SHA pass ini | `5d6bb8bf` (branch `Yasmina`); working tree memuat satu berkas dokumentasi belum di-commit, nol source aplikasi |
| Frontend SHA pass ini | `ae2ed334e` (branch `yasmina`); working tree bersih |
| Masukan | `00-interview-decisions.md` sampai `FIN-DEC-160`; laporan task `BE-FIN-058`..`085`, `FE-FIN-025`..`032` |
| Entri baru | `FIN-CAP-058`..`066` |
| Conflict baru | `FIN-CQ-10` |
| Open question baru | `FIN-OQ-084` |
| Status pass | Selesai. Nol source aplikasi disentuh, nol perbaikan dilakukan |


### 20.14 Addendum — koreksi angka `FIN-CQ-10` dan penutupannya, 4 Oktober 2026

**Mengapa addendum ini ada.** Angka pada 20.7 **salah terlalu besar**, dan koreksinya ditulis di sini
alih-alih menghapus angka lamanya. Bagian 20.7 menghitung setiap pemakaian `getRowId`, `emptyText`, dan
`emptyMessage` di seluruh aplikasi sebagai cacat `DataTable`. Pemeriksaan ulang yang mengatribusikan setiap
pemakaian ke **komponen induknya** menunjukkan sebagian besar bukan cacat sama sekali.

#### Angka yang benar

| Komponen induk | Pemakaian | Apakah cacat? |
|---|---:|---|
| `DataTable` | **59** | **Ya** — ketiga nama itu tidak pernah dibaca `DataTable` |
| `ResourceFilterSelect` | 48 | Tidak. `emptyText` memang propnya |
| `FilterSelect` | 18 | Tidak. `emptyText` memang propnya |
| `BaseSelectField` | 12 | Tidak. `emptyText` diteruskannya ke select |
| `SummaryGrid` | 5 | Tidak. `emptyText` memang propnya |
| Komponen lain (`ClinicalDataTable`, `CompactTable`, `EmergencyAssessmentSection`, dll.) | 49 | Tidak diperiksa — kontrak milik modul masing-masing |
| **Total yang semula disebut "191 cacat"** | 191 | **Hanya 59 yang benar-benar cacat** |

Sebaran 59 cacat `DataTable` itu:

| Modul | Pemakaian | Berkas |
|---|---:|---:|
| `view/finance` | 27 | 15 |
| `view/health-services` | 32 | 19 |

Jadi tiga pernyataan pada 20.7 **dikoreksi**: bukan "191 pemakaian", melainkan 59; bukan "34 di Finance",
melainkan 27; dan bukan "84 berkas milik owner lain", melainkan 19 berkas (seluruhnya health-services).

#### Keputusan owner dan perbaikan yang dikerjakan

Owner memilih (4 Oktober 2026) **menambahkan alias pada komponen `DataTable` bersama**, bukan memperbaiki
layar Finance satu per satu, dengan alasan satu perubahan menolong seluruh pemakaian sekaligus. Alasan yang
disampaikan owner semula adalah "supaya integrasi Finance ke modul lain lancar dan tidak ada error"; itu
**dikoreksi dan dicatat apa adanya**: prop yang tidak dikenal React **tidak** menimbulkan error dan **tidak**
menyentuh integrasi antar modul — `DataTable` tidak meneruskan prop sisa, sehingga nama asing hanya dibuang
diam-diam. Dampaknya murni tampilan. Keputusan owner tetap berlaku atas dasar daya angkatnya.

**Bentuk perbaikannya** (`frontend/src/components/features/base-features/data-table.jsx`):

| Hal | Isi |
|---|---|
| Alias diterima | `getRowId` → `rowKey`; `emptyText` dan `emptyMessage` → satu kalimat keadaan kosong |
| Urutan prioritas | Nama kanonik **selalu menang**. Alias hanya dipakai ketika nama kanoniknya tidak diberikan |
| Bila judul dan kalimat alias diberikan bersamaan | Kalimat alias menjadi **keterangan** (`emptyDescription`), sehingga keduanya tampil, bukan saling menimpa |
| Nilai bawaan | Dilepas dari destructuring, supaya "tidak diberikan" dapat dibedakan dari "diberikan bernilai bawaan" |
| Penjagaan | Satu test regresi ditambahkan pada `tests/unit/base-components-regression.test.mjs`, yang menambatkan **urutan prioritasnya**. Bila urutannya terbalik, 307 pemakaian yang sudah benar akan berubah diam-diam — kerusakan yang lebih besar daripada cacat yang diperbaiki |

**Verifikasi yang sudah dilakukan** (pemeriksa berkas, **bukan** `npm run test:unit`):

| Pemeriksaan | Hasil |
|---|---|
| Sembilan pernyataan test cocok dengan source | `PASS` |
| Simulasi logika penyelesaian alias pada lima keadaan | `PASS` — termasuk keadaan terpenting: layar yang hanya memberi `emptyTitle` (307 pemakaian) **nol berubah** |
| `npm run lint:errors`, `npm run test:unit`, `npm run build` | **NOT RUN** — dijalankan manual oleh owner |

**Batas yang perlu diketahui.** Komponen `DataTable` adalah komponen bersama, dan 32 dari 59 pemakaian yang
kini tertolong berada di `view/health-services` — milik owner lain. Perubahan ini **membuat kalimat keadaan
kosong mereka mulai tampil**, padahal kalimat itu belum pernah ditinjau siapa pun karena selama ini tidak
pernah muncul. Owner Finance memilih ini dengan sadar. **MUST** disampaikan kepada owner health-services.

#### Dampak ke entri dan pertanyaan

| ID | Status baru |
|---|---|
| `FIN-CQ-10` | **CLOSED** oleh alias pada `DataTable`, 4 Oktober 2026. Angkanya dikoreksi menjadi 59 pemakaian |
| `FIN-CAP-066` | Status berubah dari `Repair` menjadi **`Ready to reuse`** untuk sisi Finance. Nol layar Finance perlu disunting; 27 pemakaiannya kini terbaca komponen |
| `FIN-OQ-084` | **CLOSED** oleh keputusan owner: alias di komponen bersama, bukan perbaikan per layar |
| Pemicu impact scan | Bertambah: ulangi bila alias `DataTable` dicabut, atau bila owner health-services menolak perubahan tampilan ini |
