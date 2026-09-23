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
frontend_source_sha: abed49b03
frontend_repo: QuilvianSystemFrontendDev
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
| `FIN-CAP-007` | Kontrak `BilCollectionHandoff` (`FIN-DEC-005`) | Billing (titik sentuh Finance) | Grep `BilCollectionHandoff\|CollectionHandoff` di seluruh backend — **nol hasil** | `Missing` | Perlu dibangun bersama owner Billing sesuai `FIN-DEC-005` | Belum ada draft desain sama sekali, termasuk di blueprint billing-kasir |
| `FIN-CAP-008` | Konsumen Finance untuk `BilArHandoff`/`BilApHandoff` (`FIN-SC-001`) | Finance | Grep `BilArHandoff\|BilApHandoff` di `Areas/Corporate` — **nol hasil** | `Missing` | Tidak ada satu pun service/controller Finance yang membaca handoff Billing | Sesuai ekspektasi — memang belum digarap |
| `FIN-CAP-009` | `FinReceivable`/`FinPayable`/`FinReceipt`/`FinAccountingEventOutbox` (`FIN-SC-002..004`, `007`) | Finance | Grep `FinReceipt\|FinAccountingEventOutbox\|FinReceivable\|FinPayable\|AccountingEventOutbox` di seluruh backend — **nol hasil** | `Missing` | Seluruh entity ini harus dibangun dari nol | Cocok dengan status PROPOSED di `FIN-PRD-V2-0.3` bagian 11 |
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

## 10. Staleness dan impact-scan trigger

Peta ini **stale** dan wajib impact-scan ulang (terbatas pada bagian yang relevan) bila salah
satu terjadi:

- Backend SHA `NewQuilvianSystemBackend`/`Yasmina` bergerak dari `09101d05`.
- Frontend SHA `QuilvianSystemFrontendDev` bergerak dari `abed49b03`.
- `docs/module-blueprints/billing-kasir/blueprint-manifest.md` menaikkan `backend_commit_sha`
  di atas titik yang sudah diverifikasi pass ini (audit ulang `git diff` terhadap folder
  `BillingManagement`/`FinanceManagement`/`AccountingManagement` wajib diulang, bukan
  diasumsikan tetap kosong).
- `docs/module-blueprints/accounting/blueprint-manifest.md` mencatat endpoint Accounting Event
  baru dibangun (ubah status `FIN-CAP-018` dari `Missing`).

## 11. Handoff

Lanjutkan `/grill-me` (Closure pass) untuk klaster AR/AP/Cash Management (`FIN-OQ-002`..`005`
pada `00-interview-decisions.md`), memakai peta ini sebagai dasar sehingga pertanyaan yang
jawabannya sudah ada di source (mis. field apa yang tersedia di `BilArHandoff`) tidak ditanyakan
ulang. `FIN-CQ-01` sebaiknya diajukan sebagai pertanyaan pertama pada Closure pass tersebut,
karena menentukan DI MANA keputusan Cash Management berikutnya harus dicatat.
