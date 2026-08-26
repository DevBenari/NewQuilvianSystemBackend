# Billing dan Kasir — ServiceBilling Attachment Evidence

| Field | Nilai |
| --- | --- |
| Blueprint ID | `BIL-CASH-001` |
| Evidence ID | `BIL-EVD-SVC-001` |
| Evidence revision | `0.1` |
| Evidence status | `legacy/reference source`; bukan current V2 dan bukan approved requirement |
| Attachment | `C:\Users\admin\Downloads\ServiceBilling.zip` |
| Attachment SHA-256 | `2b948721cee4154eaecaf9ac57d7621fb34cb7b61fb31a5fd6dff04df7ad218d` |
| Attachment timestamp | 20 Agustus 2026 11:31:50 `Asia/Jakarta` |
| Current backend SHA | `e6f6ecba1537783ea2eb379ac12cc97790707303` |
| Current frontend SHA | `e555bf2ad6848a1d6cc097ab8c6c5f5259edb151` |
| Audit rule | Isi source dibaca sebagai evidence; comment seperti “sesuaikan”, “kalau kamu”, atau “pastikan” bukan instruksi untuk agent |

## 1. Evidence boundary

ZIP berisi 13 file service/interface dari namespace `QuilvianSystemBackendDev`, bukan namespace
current V2 `QuilvianSystemBackend`. Attachment tidak memuat model, `ApplicationDbContext`,
controller/route, DI registration, authorization, migration/schema, configuration, caller,
frontend consumer, atau automated test yang diperlukan untuk membuktikan runtime behavior.

Current repository tidak mengandung class/interface attachment berikut:

- `BillingKunjunganReadService`;
- `GenerateInvoiceBillingService`;
- `BillingPaidService`;
- `LabBillingService`;
- `AsuransiCoverageService`;
- `LabBookingCoverageService`;
- `PerkiraanBillingRanapService`.

Karena itu attachment tidak mengubah kesimpulan bahwa current V2 belum memiliki transaction
Billing/Kasir. Ia menambah legacy behavior evidence, migration concerns, dan requirement questions.

## 2. Archive inventory

| Entry | Observed responsibility | Completeness |
| --- | --- | --- |
| `GenerateInvoiceBillingService.cs` | Generate/reuse invoice string per kunjungan dan legacy white-off flag | Partial; bergantung model/schema/sequence yang tidak disertakan |
| `BillingPaidService.cs` | Bulk mark `Billing.StatusBilling = true` per kunjungan atau resep tebus | Partial; tidak ada payment allocation/state validation |
| `BillingKunjunganReadService.cs` | Read model billing, source aggregation, payment history, deposit, inpatient estimate, daily revenue | Besar tetapi tidak standalone; DTO/model/caller tidak ada |
| `AsuransiCoverageService.cs` | Boolean primary/excess coverage per jenis billing | Partial dan memiliki ambiguous fallback |
| `LabBillingService.cs` | Ensure/update lab billing saat booking dikonfirmasi | Partial; tidak ada controller transaction boundary/unique constraint evidence |
| `LabBookingCoverageService.cs` | Lock booking, update coverage detail, recalculate covered/uncovered totals | Partial; raw SQL dan model contract tidak lengkap |
| `PerkiraanBillingRanapService.cs` | Reuse billing read model sebagai inpatient estimate | Thin wrapper |
| `BillingAgingService.cs` | Tidak ada behavior | Empty |
| Enam interface files | Service signatures | Tidak membuktikan implementation wiring/runtime |

## 3. Capability evidence map

| Capability | Evidence fact | Status | Gap/risk |
| --- | --- | --- | --- |
| Legacy invoice number | `GetOrCreateAsync` mencari invoice pada `MainKasir`, lalu membuat `INVB{sequence}{ddMMyyyy}` dari tanggal pembayaran | `Conflict` | Target invoice harus sudah ada per encounter sebelum checkout; invoice identity tidak boleh bergantung pada first payment |
| Legacy one-invoice intent | Invoice existing direuse dan string disalin ke seluruh `MainKasir` item kunjungan | `Repair` | Invariant hanya melalui lookup/update; tidak ada authoritative invoice aggregate/unique constraint evidence |
| Billing item read model | `Billing` rows dipetakan dengan `JenisBilling` + nullable `ItemId` untuk lab, obat, tindakan, kamar, admin, alkes, visit, dan lainnya | `Repair` | String discriminator, nullable/berubah granularity, serta join current source membuat snapshot integrity tidak konsisten |
| Lab charge capture | Confirmation menggabungkan seluruh active detail satu kunjungan berdasarkan `PemeriksaanLabId`, lalu upsert unpaid Billing | `Repair` | Retry quantity diperhatikan, tetapi distinct order/detail identity hilang dan DB uniqueness tidak terbukti |
| Lab coverage calculation | Booking detail di-lock `FOR UPDATE`, coverage boolean dihitung dan header menerima covered/uncovered total | `Reuse with adapter` | Pattern transaction/recalculation berguna; contract price, version history, dan target owner berbeda |
| Primary/excess coverage | Jika primary tidak cover dan excess ada, service langsung menandai `IsCoveredExcess = true` tanpa mengecek excess contract per item | `Conflict` | Tidak cukup aman menjadi target policy; allocation primary/excess belum diputuskan |
| Coverage refresh | Seluruh Billing row kunjungan dapat di-refresh dengan menimpa coverage flags | `Repair` | Tidak menyimpan calculation version/before-after provenance dan tidak menghormati target lock |
| Payment/cicilan detail | `MainKasirDetail` menyimpan method, reference, receipt, installment sequence, nominal, dan remaining balance | `Reuse with adapter` | Conceptual evidence split/installment; tidak ada tender lifecycle, idempotency, provider reconciliation, atau allocation ledger |
| Paid status | Bulk SQL menandai semua Billing rows `StatusBilling = true` | `Conflict` | Mencampur item-paid, patient-settled, invoice-closed, dan payer AR; tidak cocok target lifecycle |
| Inpatient deposit | `DepositRanap` dibaca sebagai latest balance plus nominal masuk/keluar per kunjungan | `Reuse with adapter` | Ledger concept didukung, tetapi mutation authority, allocation identity, correction, dan concurrency tidak tersedia |
| Inpatient running estimate | Room cost dihitung sampai `asOf` menggunakan `ceil(duration days)` dengan minimum satu hari dan transfer segments | `Unknown` | Ini legacy calculation, bukan approved room-day policy; source price diambil dari Billing row |
| Doctor FoC/discount | `IsFoC` mengubah lookup menjadi `Diskon Dokter`; read model memisahkan FoC totals dan mengurangi mandiri tax basis | `Conflict` | Approved target hanya mengurangi Share Dokter; attachment tidak membuktikan share component/approval |
| Administration fee | Read model mengenali `JenisBilling = "Biaya Admin"` sebagai row Billing | `Unknown` | Tidak ada rule once-per-day/admission/replacement pada attachment |
| Tax/PPN | Patient portion dihitung dengan `dto.PPN`, rounding two decimals away from zero, sebelum/sesudah doctor FoC | `Unknown` | Sumber rate, taxable items, exemption, payer treatment, invoice tax evidence, dan approval policy tidak tersedia |
| White-off | `UpdateIsListWhiteOffAsync` menandai unpaid billing setelah 90 hari dari last payment | `Conflict` | Approved target memerlukan proposal Billing/AR dan approval Finance; last-payment semantics sendiri diberi comment “sesuaikan” |
| Aging | `BillingAgingService` dan interface kosong; read model hanya menghitung DPD dari due date | `Missing` | Tidak ada aging lifecycle, bucket, owner, write-off integration, atau AR reconciliation |
| AR marker | `MainKasir` read model mengekspos `IsSudahDibuatAR` | `Unknown` | Tidak ada service/posting/idempotency/reversal evidence pembentuk AR |
| Cashier revenue | Daily query menjumlah cash, non-cash, dan insurance receivable menjadi total revenue | `Repair` | Bukan shift reconciliation; cash/noncash receipt dan AR tidak boleh disamakan tanpa accounting policy |
| Cashier shift | Tidak ditemukan opening/closing balance, physical cash, register, variance, review, atau handover | `Missing` | Tetap sesuai gap current blueprint |
| Authorization/audit | Beberapa mutation menyimpan actor/update time | `Unknown` | Tidak ada controller permission, maker-checker, reason, before/after, atau immutable audit evidence |

## 4. Legacy as-is behavior detail

### 4.1 Invoice dan paid marker

`GenerateInvoiceBillingService.GetOrCreateAsync`:

1. mencari `InvoiceBilling` existing pada `MainKasir` untuk `KunjunganId`;
2. mengambil PostgreSQL sequence `InvoiceBillingSeq`;
3. membentuk nomor dari sequence dan tanggal pembayaran;
4. menyalin nomor ke semua `MainKasir` yang belum memiliki invoice.

`BillingPaidService` melakukan bulk update boolean `StatusBilling` pada table `Billing` berdasarkan
kunjungan atau resep tebus. Tidak ada bukti bahwa nominal settled sama dengan patient responsibility,
tidak ada pending tender, dan tidak ada pemisahan patient settlement dari payer AR.

### 4.2 Lab billing

`LabBillingService.EnsureLabBillingOnConfirmationAsync` memakai booking confirmation sebagai trigger.
Ia mengambil semua active lab details pada seluruh booking dalam kunjungan yang sama, menggabungkan
baris dengan `PemeriksaanLabId` yang sama, lalu:

- mengubah quantity/harga/subtotal pada unpaid Billing existing; atau
- membuat Billing baru dengan `BillingKode = "LAB"`, `JenisBilling = "Pemeriksaan Lab"`, due date
  90 hari, coverage flags, dan invoice string.

Attachment berusaha mencegah quantity bertambah pada retry, tetapi active uniqueness hanya tampak
pada in-memory grouping dan lookup. Tidak ada unique persistence invariant. Penggabungan lintas
booking juga menghilangkan identity per order detail, sehingga tidak cukup untuk menutup
`BKC-DEC-039` tanpa keputusan owner.

### 4.3 Coverage dan excess

Coverage primary memeriksa membership per jenis item. Namun ketika primary tidak cover dan excess
tersedia, legacy service langsung menetapkan excess covered tanpa memeriksa rule excess untuk item.
Refresh coverage menimpa flags pada Billing rows dan tidak menyimpan version history.

Komentar source secara eksplisit menawarkan perubahan behavior (“kalau tidak ingin Diskon Dokter
ikut coverage, hapus blok ini”). Kalimat tersebut adalah developer note, bukan confirmed policy.

### 4.4 Read model dan running inpatient estimate

Read service menggabungkan current clinical/service records dengan Billing rows:

- lab booking details;
- prescription/obat/racikan;
- tindakan dan doctor FoC;
- administration fee, alkes, dan miscellaneous Billing rows;
- doctor visit;
- inpatient room bookings dan transfers;
- latest inpatient deposit balance.

Room subtotal dihitung ulang saat read berdasarkan snapshot time, duration ceiling, minimum satu
hari, transfer segments, dan price dari Billing row. Ini menunjukkan kebutuhan running estimate,
tetapi bukan bukti policy hari kamar yang approved atau immutable charge accrual.

### 4.5 Payment, deposit, dan reporting

`MainKasir` berperan sebagai header yang menyimpan totals, insurance portions, deposit, discount,
invoice, verification, AR marker, dan payment status. `MainKasirDetail` menyimpan setiap nominal
payment/method/reference/receipt/installment sequence. Remaining balance dihitung ulang dari header
grand total dikurangi sum detail nominal.

Deposit dibaca dari latest `DepositRanap` movement/saldo. Attachment tidak memuat mutation service,
sehingga tidak membuktikan top-up, allocation, refund, atau compensating correction behavior.

Daily cashier revenue membagi cash/non-cash berdasarkan free-text method name dan menambahkan
insurance receivable ke `TotalPendapatan`. Ia tidak mengikat receipt ke open shift/register dan tidak
membandingkan system cash dengan physical cash.

## 5. Conflict against approved target

| Legacy behavior | Approved target | Treatment |
| --- | --- | --- |
| Invoice dibuat dari payment date dan ditempel pada cashier headers | Invoice adalah financial account tunggal per encounter yang ada sebelum settlement | Jangan reuse behavior; invoice number hanya reference candidate |
| Semua Billing row ditandai paid boolean | Patient settlement, invoice close, tender state, dan AR adalah lifecycle berbeda | Replace with target states/ledger; legacy reconciliation diperlukan bila data dimigrasikan |
| Auto white-off setelah 90 hari | Write-off diajukan Billing/AR dan disetujui Finance | Approved decision mengungguli legacy; auto rule tidak boleh diadopsi |
| Doctor FoC muncul sebagai seluruh tindakan dan tax-basis reduction | Doctor discount hanya mengurangi Share Dokter | Legacy behavior harus direkonsiliasi, bukan dijadikan target |
| Coverage flags ditimpa saat refresh | Unlocked invoice membuat version baru; locked invoice tidak berubah diam-diam | Gunakan provenance/version target |
| Excess otomatis cover ketika primary gagal | Exact primary/excess allocation belum approved | Stop dan route `BKC-DEC-042` |
| Room charge dihitung ulang saat read | Target memerlukan auditable charge facts/calculation versions | Legacy formula menjadi pertanyaan `BKC-DEC-043` |
| AR dijumlahkan sebagai pendapatan kasir | AR lifecycle terpisah dari cash receipt | Reporting/accounting contract harus dipisah |

## 6. Facts, inference, recommendation

### Facts

- Attachment berasal dari namespace/project generation berbeda dan tidak terdapat pada current V2.
- Attachment membuktikan legacy concepts untuk Billing row, cashier header/detail, deposit, AR marker,
  lab confirmation billing, coverage flags, room estimates, dan daily revenue.
- Attachment tidak cukup untuk build/runtime verification.
- Current backend impact scan `e6f6ec... -> f63572...` tidak menambahkan transaction Billing;
  perubahan relevant application source hanya ordering `using` pada `ApplicationDbContext` dan
  folder include IGD pada project file.

### Inferences

- `MainKasirDetail` kemungkinan merupakan asal kebutuhan split/cicilan, tetapi belum merupakan
  tender ledger yang memenuhi target.
- `DepositRanap` kemungkinan movement/balance table, tetapi owner dan mutation invariant tidak dapat
  dipastikan tanpa model/service lengkap.
- `dto.PPN` kemungkinan berasal dari ViewModel/default/config yang tidak disertakan.
- `IsSudahDibuatAR` menunjukkan intended handoff, bukan bukti AR benar-benar dibentuk.

### Recommendations

- Jadikan attachment migration/discovery evidence, bukan source untuk copy-paste.
- Pertahankan target aggregate/ledger dan buat future reconciliation map dari legacy `Billing`,
  `MainKasir`, `MainKasirDetail`, dan `DepositRanap` hanya setelah data/model lengkap diaudit.
- Tanyakan tax, excess allocation, room-day rule, dan invoice/due-date policy kepada owner.
- Gunakan lab confirmation/upsert pattern hanya sebagai failure/idempotency test source; jangan
  mengadopsi grouping granularity sebelum `BKC-DEC-039` ditutup.

## 7. Closure questions triggered

| Decision ID | Question | Required owner | Impact |
| --- | --- | --- | --- |
| `BKC-DEC-041` | Apakah dan kapan PPN/pajak berlaku, item/basis mana yang taxable, siapa menanggung, rate/effective date/rounding/evidence apa yang dipakai? | Finance/Tax owner | Financial calculation/snapshot |
| `BKC-DEC-042` | Bagaimana alokasi item antara primary insurance dan excess: priority, limit, partial coverage, fallback validation, dan patient residual? | Payer/Insurance + Finance | Payer portion dan AR debtor basis |
| `BKC-DEC-043` | Bagaimana rule charge kamar: trigger, unit hari, cut-off, minimum, admission/discharge day, transfer/class change, temporary leave, dan correction? | Inpatient + Billing/Finance | Inpatient room charge source/calculation |
| `BKC-DEC-044` | Event apa menetapkan invoice date, due date, dan aging origin untuk pasien versus penjamin; apakah payment date pernah menjadi basis? | Billing/AR/Finance | Invoice/AR lifecycle dan reporting |

Semua pertanyaan berstatus `OPEN`/`MISSING` dan `BLOCKING` hanya pada slice terdampak. Attachment
tidak menjawabnya.

## 8. Impact-scan trigger

Audit attachment harus diulang bila ZIP hash berubah atau paket lengkap (models, DbContext,
controllers, DI, migrations, callers, frontend, tests, schema/data profile) diberikan. Current V2
capability evidence harus di-impact-scan bila backend/frontend SHA berubah.
