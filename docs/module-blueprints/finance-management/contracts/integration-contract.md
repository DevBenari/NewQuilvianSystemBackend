# Kontrak Integrasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.1` |
| `last_changed_in` | `FIN-INTEGRATION-1.1` — amendment 25 September 2026 |
| Status | `approved` dan `locked` — revisi 1.1 disetujui dan dikunci owner 25 September 2026 |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-25 |
| Input revision | `00-interview-decisions.md` — `FIN-DEC-001`..`044` (`FIN-DEC-030`..`044` ditambahkan 25 September 2026) |
| Kontrak eksternal yang diikuti | **`ACC-XMOD-0.3`** milik Accounting — naik dari `0.2`, diratifikasi sisi Finance lewat `FIN-DEC-039` |
| Backend SHA yang diverifikasi | `d6cdfaf9` (impact scan 25 September 2026, `01-existing-capability-map.md` bagian 9.3) |
| Dampak kompatibilitas | **Satu perubahan perilaku** (penahanan pra-finalisasi dihapus, bagian 5.5) dan **tujuh kode kejadian baru** (bagian 5.4). Sisanya aditif. Tidak ada field yang dihapus maupun diganti nama |

**Yang berubah pada revisi 1.1 — ringkas:**

| Bagian | Perubahan | Dasar |
|---|---|---|
| 2 | `BilCollectionHandoff` **sudah dibangun** dan sudah dikonsumsi Finance, bukan lagi permintaan | `FIN-CAP-007`, `FIN-CAP-025` |
| 2a (baru) | Empat fakta Billing tambahan yang kini dikonsumsi: mutasi deposit, kelebihan bayar, kasus pengembalian, pengesahan selisih kas | `FIN-DEC-040`..`044`, `FIN-CAP-022`..`024` |
| 5.1 | Syarat akun layanan ditetapkan (bukan `SuperAdmin`, Departemen+Jabatan `Receive`-only, terikat badan hukum) | `FIN-DEC-036` |
| 5.2 | Objek `SubledgerBalance` ditambahkan sebagai bagian opsional kedua | `FIN-DEC-035` |
| 5.3 | Nomor jurnal pada balasan `201` menjadi **opsional** | `FIN-DEC-039` |
| 5.4 | Katalog naik dari **17 menjadi 24** kode | `FIN-DEC-031`, `034`, `035`, `040`..`044` |
| 5.5 | **Penahanan pra-finalisasi DIHAPUS**, diganti pemilihan kode berdasarkan status tagihan sumber | `FIN-DEC-030` |
| 5.6 | Bentuk saldo subledger final, memakai amplop yang sama | `FIN-DEC-035` |
| 5.7 | Cutover diikat enam gerbang milik Accounting; tanggal 1 Oktober 2026 dibatalkan | `FIN-DEC-037` |

Finance punya **dua batas integrasi**: menerima dari Billing dan Medical Fee, lalu menerbitkan
ke Accounting. Tidak ada arah ketiga.

---

## 1. Prinsip yang mengikat seluruh integrasi

| Prinsip | Ketentuan | Dasar |
|---|---|---|
| Arah | Satu arah masuk, satu arah keluar. Finance **MUST NOT** menulis ke tabel Billing, Medical Fee, maupun Accounting | `FIN-OOS-001`..`004` |
| Sumber nilai | Finance menyalin nilai dari sumber, **MUST NOT** menghitung ulang | Aturan bisnis #1 dan #2 |
| Idempotensi | Setiap fakta masuk punya kunci unik; kiriman ulang tidak melahirkan baris kedua | `FIN-DES-006`, `009`, `010` |
| Pengakuan | Menerima fakta ≠ menjurnal. Jurnal adalah pekerjaan Accounting | Aturan bisnis #4 |
| Data pasien | **MUST NOT** menyeberang ke Accounting | Aturan bisnis #6, `ACC-DEC-056` |

---

## 2. Billing → Finance: kontrak fakta penerimaan (`BilCollectionHandoff`)

Ini kontrak yang diminta Finance kepada owner Billing sesuai `FIN-DEC-005`. **Tabelnya milik
Billing**; Finance hanya menetapkan isi minimumnya (`FIN-DES-023`).

### 2.1 Bentuk yang diminta

| Field | Tipe | Wajib | Kegunaan bagi Finance |
|---|---|:---:|---|
| `TenderId` | `Guid` | Ya | Kunci idempotensi. Satu tender berhasil = satu penerimaan Finance |
| `SettlementId` | `Guid` | Ya | Telusur ke penyelesaian pembayaran Billing |
| `InvoiceId` | `Guid` | Ya | Telusur ke tagihan |
| `PaymentAllocationIds` | `Guid[]` | Ya bila ada alokasi | Membuktikan berapa uang yang benar-benar mengurangi tagihan |
| `PaymentMethodId` | `Guid` | Ya | Membedakan tunai dan non-tunai |
| `PaymentMethodAccountId` | `Guid` | Tidak | Rekening/kanal non-tunai |
| `Amount` | `decimal(18,2)` | Ya | Nominal; **disalin apa adanya** |
| `KwitansiNumber` | `string(50)` | Ya | Bukti yang dipegang pasien |
| `CashierShiftId` | `Guid` | Ya untuk tunai | Dasar rekonsiliasi shift |
| `ProviderReference` | `string(150)` | Ya untuk non-tunai | Telusur ke penyedia pembayaran |
| `ProviderEventId` | `string(100)` | Ya bila ada | Idempotensi sisi penyedia |
| `OccurredAt` | `DateTimeOffset` | Ya | Waktu uang benar-benar diterima |
| `SourceInvoiceStatus` | `string(30)` | Ya | **Penentu KODE KEJADIAN mana yang terbit** — lihat bagian 5.5. Sampai revisi 1.0 field ini dipakai untuk memutuskan tahan atau kirim; sejak `FIN-DEC-030` tidak ada lagi penahanan, dan field yang sama dipakai memilih antara `PENERIMAAN-UANG-MUKA` dan `PENERIMAAN-KASIR` |
| `TenderStatus` | `string(30)` | Ya | `SUCCEEDED` atau `REVERSED` |
| `HandoffKey` | `Guid` | Ya | Kunci idempotensi handoff |
| `CorrelationId` | `Guid` | Ya | Rantai telusur ujung ke ujung |
| `CausationId` | `Guid` | Ya | Tindakan penyebab |
| `Status` | `string(30)` | Ya | `CREATED` / `ACKNOWLEDGED`, mengikuti pola `BilArHandoff` |

### 2.2 Ketentuan perilaku

| Hal | Ketentuan |
|---|---|
| Kapan dibuat | Saat tender berstatus `SUCCEEDED`, **tidak menunggu** tagihan difinalisasi |
| Pembalikan | Tender `REVERSED` membuat **baris handoff baru**, bukan mengubah baris lama |
| ACK | Finance menandai `ACKNOWLEDGED` setelah penerimaan berhasil dibuat |
| Kegagalan | Bila Finance gagal mengolah, baris tetap `CREATED` dan dicoba ulang. Billing **MUST NOT** menghapusnya |
| Billing → Accounting | **Dilarang.** Billing tidak pernah mengirim kejadian ke Accounting langsung |

### 2.3 Riwayat kesepakatan

| Hal | Status |
|---|---|
| Mekanisme transport (tabel handoff atau outbox) | **Disepakati** 21 September 2026 — tabel handoff persisted (`BilCollectionHandoff`), sesuai usulan Finance (`FIN-DEC-005`, `BKC-DEC-106`) |
| Nama tabel dan kolom persisnya | Wewenang Billing — `BilCollectionHandoff` (`BKC-DES-037`), field persis sesuai bagian 2.1 di atas |
| Eksekusi task Billing | **Selesai** 22 September 2026 — `BilConsumerHandoffService.PublishForTenderAsync` (`BE-BKC-069`) menerbitkan baris sesuai kontrak ini, dipasang di `BillingSettlementService.ReconcileTenderAsync`. Konsumsi sisi Finance dibangun `BE-FIN-016` |

---

## 2a. Billing → Finance: empat fakta uang muka, deposit, dan selisih kas

**Ditambahkan revisi 1.1.** Keempat fakta ini sudah ada di Billing sejak sebelum `09101d05`
tetapi baru relevan bagi Finance setelah `FIN-DEC-030`..`044`. **Tidak ada kontrak baru yang
diminta dari Billing** — keempatnya sudah terdaftar di `ApplicationDbContext` dan Finance
membacanya langsung, pola yang sama seperti `BilCashierShift` (aturan bisnis #9: Finance
membaca dan merekonsiliasi, tidak pernah menghitung ulang dan tidak pernah menulis).

| Fakta Billing | Yang dibaca Finance | Menjadi kejadian | Kunci idempotensi |
|---|---|---|---|
| `BilDepositMovement` tipe `TOP_UP` | Uang muka/deposit masuk | `PENERIMAAN-UANG-MUKA` | `IdempotencyKey` milik Billing |
| `BilDepositMovement` tipe `ALLOCATION` | Uang muka dipakai melunasi tagihan | `PEMAKAIAN-UANG-MUKA-DEPOSIT` | `IdempotencyKey` milik Billing |
| `BilDepositMovement` tipe `RELEASE` | Deposit dikembalikan tunai | `PENGEMBALIAN-UANG-MUKA` | `IdempotencyKey` milik Billing |
| `BilDepositMovement` tipe `REVERSAL` | Pembalikan mutasi deposit | Mengikuti kode mutasi yang dibalik — lihat bagian 5.4 | `IdempotencyKey` milik Billing |
| `BilRefundableCredit` `SourceType = ALLOCATION_EXCESS` berstatus `AVAILABLE` | Kelebihan bayar diakui | `PENGAKUAN-KELEBIHAN-BAYAR` | `Id` (sumber tidak punya kunci sendiri — lihat catatan di bawah) |
| `BilRefundCase` berstatus `EXECUTED`, `RefundableCredit.SourceType = ALLOCATION_EXCESS` | Kelebihan bayar dikembalikan tunai | `PENGEMBALIAN-UANG-MUKA` | `IdempotencyKey` milik Billing |
| `BilCashVarianceReview` (dibuat saat `BilCashierShift.Status` menjadi `REVIEWED`) | Selisih kas shift disahkan | `SELISIH-KAS-SHIFT` | `Id` (sumber tidak punya kunci sendiri) |

**Catatan kunci idempotensi.** `BilDepositMovement` dan `BilRefundCase` sama-sama punya
`IdempotencyKey` sendiri, jadi dipakai apa adanya. `BilRefundableCredit` dan
`BilCashVarianceReview` **tidak punya**, sehingga Finance memakai `Id` baris itu sebagai kunci.
Ini aman karena `Id` bersifat tetap dan unik; yang dijaga adalah jangan sampai satu baris
melahirkan dua kejadian, dan `Id` cukup untuk itu.

**Yang TIDAK masuk cakupan revisi ini, dan sengaja:**

| Fakta | Alasan |
|---|---|
| `BilRefundCase` dengan `RefundableCredit.SourceType = SETTLEMENT` | Lawan jurnalnya belum digali — berpotensi koreksi piutang/pendapatan, bukan penarikan Uang Muka Pasien (`FIN-OQ-018`) |
| `BilRefundCase` dengan `RefundableCredit.SourceType = REFERRED_OUTPATIENT_ADMIN` | Sama seperti di atas (`FIN-OQ-018`) |
| `BilRefundLine` | Rincian baris pengembalian; Finance hanya butuh nilai total per kasus, tidak memecah per baris (`FIN-DEC-038`: seluruh kejadian `Components = TOTAL`) |
| `BilDepositAccount.AvailableBalance` | Saldo berjalan milik Billing. Finance **MUST NOT** memakainya sebagai nilai kejadian — nilai kejadian selalu dari mutasi, bukan dari saldo |

**Jalur masuknya lewat tabel yang sudah ada.** Keempat fakta ini masuk lewat
`FinBillingHandoffIntake` yang sudah berdiri (`FIN-DES-008`), dengan penambahan nilai
`HandoffType` — bukan tabel intake baru. Rinciannya di `02-backend-architecture.md` bagian
AMENDMENT REVISI 3.

**Keempat fakta ini TIDAK terlihat di layar pemantauan "Surat ke Modul Konsumen" milik
Billing.** Layar itu (`BilConsumerHandoffService.GetPendingHandoffsAsync`) hanya memantau surat
yang punya kolom status handoff sendiri (`BilCollectionHandoff`, `BilPrescriptionClearanceHandoff`,
handoff rawat inap) — sumber yang ditandai `CREATED` lalu diakui `ACKNOWLEDGED` oleh konsumennya.
`BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, dan `BilCashVarianceReview` **tidak
punya kolom status seperti itu**, dan Finance dilarang menulisnya (`FIN-STATE-1.1` bagian 1: nol
ACK, status akhir langsung `CONSUMED`). Konsekuensinya: bila sinkronisasi keempat fakta ini macet
di sisi Finance, layar Billing itu **tidak akan menunjukkan apa pun** — pemantauannya wajib
dilakukan dari sisi Finance sendiri (rute `FE-FIN-006`, membaca baris `FinBillingHandoffIntake`
berstatus `NEW`/`ERROR`), bukan dari layar konsumen Billing.

---

## 3. Billing → Finance: fakta AR, AP, dan koreksi (sudah ada)

Ketiga tabel ini **sudah ada di source** pada `09101d05` dan tidak perlu dibangun.

| Sumber | Kunci idempotensi | Menjadi apa di Finance | Status kontrak |
|---|---|---|---|
| `BilArHandoff` | `HandoffKey` | `FinReceivable` | Sudah ada; **perlu perluasan** untuk manfaat karyawan |
| `BilApHandoff` | `HandoffKey` | Rujukan kesiapan pada `FinDoctorPayable`; **bukan** sumber nilai | Sudah ada, dipakai apa adanya |
| `BilHandoffAdjustment` | `Id` | `FinReceivableAdjustment` | Sudah ada, dipakai apa adanya |

### 3.1 Perluasan `BilArHandoff` yang diminta

Sesuai `FIN-DEC-006`. Keadaan sekarang diverifikasi langsung dari source: `BillingArDebtorTypes`
hanya berisi `PATIENT_GUARANTOR` dan `PAYER`.

| Perubahan | Bentuk | Sifat |
|---|---|---|
| Nilai `DebtorType` baru | `EMPLOYEE_BENEFIT` | Aditif — konsumen lama tidak rusak |
| Kolom baru | `BenefitOwnerId` (`Guid?`) | Nullable — baris lama tetap sah |
| Kolom baru | `BenefitRelationship` (`string(30)?`) | Nullable — `SELF`, `SPOUSE`, `CHILD`, dan seterusnya |

**Siapa yang mengisi.** Registrasi/Billing, berdasarkan hubungan pasien dengan karyawan dan
eligibilitas yang berlaku saat pelayanan. Finance **MUST NOT** menentukan ulang identitas ini;
ketidaksesuaian dikembalikan lewat alur koreksi (`FIN-DEC-016`).

**Status.** Menunggu konfirmasi owner Billing **dan** owner HR. Sampai keduanya turun, rumpun
ini `OPEN DECISION` dan tidak masuk gelombang pengiriman mana pun.

---

## 4. Medical Fee → Finance: fee dokter yang sudah disetujui

| Hal | Ketentuan | Dasar |
|---|---|---|
| Apa yang diterima | `DoctorServiceFee` yang berstatus **sudah disetujui** | `FIN-DEC-003` Opsi B |
| Kunci idempotensi | `SourceDoctorServiceFeeId` | Satu fee disetujui = satu utang |
| Apa yang **tidak** diterima | Fee yang masih dihitung, diverifikasi, atau belum disetujui | `FIN-VAL-043` |
| Peran `BilApHandoff` | Rujukan kesiapan saja — **bukan** sumber nilai | `FIN-DEC-003` |
| Perhitungan ulang | **Dilarang.** Finance menyalin nilai fee | Aturan bisnis #1 |

**Mengapa ini penting.** Kalau Finance membentuk utang dokter dari `BilApHandoff.Amount`,
liabilitas dokter akan diakui sebelum fee-nya diverifikasi. Dan bila aturan posting Accounting
juga memuat komponen `JASA_MEDIS` pada kejadian piutang, jasa medis akan terbukukan **dua
kali**. `FIN-DEC-003` Opsi B menutup keduanya: kejadian piutang hanya memuat piutang dan
pendapatan, sedangkan jasa medis terbit terpisah lewat `PENGAKUAN-HUTANG-DOKTER` setelah fee
disetujui.

**Konsekuensi untuk Accounting:** contoh aturan posting yang memuat `JASA_MEDIS` di dalam
`PENGAKUAN-PIUTANG` **MUST** disesuaikan agar tidak berbenturan.

---

## 5. Finance → Accounting: kejadian keuangan

Mengikuti `ACC-XMOD-0.2` apa adanya (`FIN-DEC-001`). Yang ditulis di sini adalah **kewajiban
sisi Finance**, bukan penulisan ulang kontrak Accounting.

### 5.1 Tujuan kirim

| Hal | Nilai |
|---|---|
| Pintu masuk | `POST api/v1/corporate/accounting/accounting-events`, satu kejadian per permintaan |
| Mata uang | `IDR` saja |
| Status saat ini | **Endpoint belum dibangun.** Diverifikasi ulang langsung ke source `d6cdfaf9`: sepuluh controller `Areas/Corporate/AccountingManagement/**` seluruhnya route `api/v1/corporate/accounting/...` yang sudah ada (period, ledger, journal, COA, event-type, posting-rule, reconciliation, recurring-journal, year-end-closing) — tidak satu pun penerima kejadian. Konsisten dengan gerbang `G1` milik Accounting |

**Syarat akun layanan** (`FIN-DEC-036`, revisi 1.1). Ketiga syarat berikut adalah syarat keras
Accounting, bukan pilihan desain Finance:

| # | Syarat | Akibat bila dilanggar |
|---:|---|---|
| 1 | Satu akun khusus untuk Finance — bukan akun manusia, **bukan `SuperAdmin`** | Pengiriman memakai akun manusia membuat jejak audit Accounting tidak dapat dibedakan dari tindakan orang |
| 2 | Akun itu **wajib** punya penugasan Departemen + Jabatan khusus yang hanya diberi hak `AccountingEvent : Receive` | **Tanpa penugasan organisasi, setiap kiriman pasti `403`** — hak akses Accounting diberikan per Departemen + Jabatan, bukan per akun |
| 3 | Akun itu berhak atas badan hukum yang dikirimi kejadian | Kiriman ke badan hukum yang bukan haknya ditolak |

**Mekanisme kredensialnya (JWT Bearer berumur pendek atau mekanisme auth existing) masih
terbuka** — wewenang Platform bersama Finance dan Accounting, dilacak `FIN-OQ-016`. Token
berumur pendek adalah preferensi Accounting, bukan syarat.

### 5.2 Dua belas field wajib

| Field | Sumber di Finance |
|---|---|
| `EventNumber` | Dibuat `FinAccountingEventOutbox` |
| `EventTypeCode` | Salah satu dari **24** kode pada bagian 5.4 |
| `SourceModule` | Selalu `Finance` |
| `SourceTransactionId` | Nomor piutang, penerimaan, utang, pembayaran, atau mutasi kas kecil |
| `SourceVersion` | Dinaikkan saat koreksi, **tidak pernah dipakai ulang** |
| `EventOccurredAt` | Waktu kejadian bisnis sebenarnya |
| `AccountingDate` | Tanggal pembukuan menurut aturan Finance |
| `Amount` | Nilai yang diakui Finance |
| `CurrencyCode` | `IDR` |
| `LegalEntityId` | Rujukan badan hukum |
| `CorrelationId` | Diwarisi dari rantai Billing → Finance |
| `CausationId` | Tindakan penyebab |

**Dua bagian opsional** (yang kedua ditambahkan revisi 1.1):

1. `Components` — daftar `{ ComponentCode, Amount }`. Tanpa itu, seluruh nilai dianggap komponen
   `TOTAL`. **Untuk rilis ini seluruh kejadian Finance memakai `TOTAL`** dan tidak memecah
   komponen apa pun (`FIN-DEC-038`).
2. `SubledgerBalance` — **hanya** untuk kejadian `SALDO-SUBLEDGER`. Berisi dua field:

| Field di dalam `SubledgerBalance` | Tipe | Wajib | Keterangan |
|---|---|:---:|---|
| `AccountingPeriodCode` | `string`, maks **7** | Ya | Bentuk `YYYY-MM`, contoh `2026-11`. Kode periode Accounting memang 7 karakter — bukan 20 seperti draf Finance yang lama |
| `ControlAccountCode` | `string`, maks 50 | Ya | Kode akun kontrol milik Accounting |

**Aturan nilai khusus pesan saldo.** Untuk `SALDO-SUBLEDGER`, `Amount` di amplop dipakai sebagai
nilai saldonya dan **boleh nol atau negatif**; `AccountingDate` di amplop berarti tanggal
cut-off. Ini sengaja tidak memakai field berdiri sendiri, supaya anti-ganda, koreksi lewat
`SourceVersion`, dan penelusuran tidak perlu aturan baru (`FIN-DEC-035`).

### 5.3 Kewajiban Finance atas balasan

| Kode | Arti | Yang dilakukan Finance |
|---|---|---|
| `201` | Kejadian baru diterima | Tandai `ACKNOWLEDGED`, **simpan nomor jurnal bila ada** — lihat catatan di bawah |
| `200` | Sudah pernah diterima | Perlakukan sebagai berhasil. **MUST NOT** membuat kejadian baru |
| `400` | Ada isian tidak sah | Tandai `FAILED`. Perbaiki data sumber, lalu kirim ulang **baris yang sama** |
| `403` | Akun layanan tidak berhak | Tandai `FAILED`, beri tahu operasional. **MUST NOT** dicoba ulang membabi buta |
| `409` | Mata uang bukan rupiah | Tandai `FAILED`. Kiriman ulang pasti ditolak lagi |
| `422` | Aturan posting belum ada | Tandai `HELD`. **MUST NOT** mengirim kejadian baru — Accounting yang melengkapi aturannya |

**Nomor jurnal pada `201` bersifat opsional** (`FIN-DEC-039`, revisi 1.1). Accounting menjurnal
seketika di dalam request, tetapi bila ada gangguan teknis **setelah** kejadian Finance tersimpan,
Accounting tetap menjawab `201` dengan `JournalNumber` kosong lalu mencoba ulang sendiri.

> **Contoh.** Finance mengirim `EVT-300`, dijawab `201` dengan `JournalNumber` kosong. Dua menit
> kemudian jurnal `JU/2026/11/00017` terbentuk di Accounting. Accounting **tidak** mengabari
> Finance, karena Accounting tidak pernah menerbitkan kejadian balik. Bila Finance memang butuh
> nomor itu, Finance mengirim ulang `EVT-300`: jawabannya `200` beserta nomor jurnalnya, tanpa
> jurnal baru.

Akibatnya: `ACKNOWLEDGED` tetap ditandai walaupun `AccountingJournalNumber` kosong. Kolom kosong
pada baris berstatus `ACKNOWLEDGED` **bukan** tanda kegagalan dan **MUST NOT** memicu percobaan
ulang otomatis.

Empat nilai `HoldReasonCode` yang menyertai balasan `422` — `EVENT_TYPE_NOT_REGISTERED`,
`POSTING_RULE_MISSING`, `COMPONENT_UNMAPPED`, `COMPONENT_MISSING` — seluruhnya berarti tertahan.
Finance menandai `HELD`, menyimpan kodenya di `HoldReason`, dan **tidak** mengirim kejadian baru.

### 5.4 Katalog 24 jenis kejadian

Tujuh belas kode pertama **sudah diratifikasi Accounting apa adanya** pada 24 September 2026
(`ACC-DEC-083`, dicatat `FIN-DEC-039`). Tujuh kode terakhir adalah usulan sisi Finance dari
`FIN-DEC-031`, `034`, `035`, `040`..`044` yang **masih menunggu ratifikasi balik Accounting**
(`FIN-OQ-017`) — sudah dikirim lewat `evidence/04` dan dikoreksi `evidence/05`.

| Kode | Dipicu oleh |
|---|---|
| `PENGAKUAN-PIUTANG` | Piutang diakui dari fakta AR Billing |
| `PENERIMAAN-PIUTANG` | Uang diterima untuk piutang yang **sudah** ada |
| `PENYESUAIAN-PIUTANG` | Koreksi piutang disetujui |
| `PEMUTIHAN-PIUTANG` | Penghapusan piutang disetujui |
| `PENGAKUAN-HUTANG-SUPPLIER` | Utang supplier diinput |
| `PEMBAYARAN-HUTANG-SUPPLIER` | Pembayaran supplier ditandai sudah dibayar |
| `PENGAKUAN-HUTANG-DOKTER` | Fee dokter yang sudah disetujui menjadi utang |
| `PEMBAYARAN-HUTANG-DOKTER` | Pembayaran dokter ditandai sudah dibayar |
| `PENYESUAIAN-HUTANG` | Koreksi utang disetujui |
| `SETORAN-BANK` | Setoran bank diposting |
| `PETTY-CASH-TOP-UP` | Penambahan saldo kas kecil |
| `PETTY-CASH-DISBURSEMENT` | Pencairan kas kecil |
| `PETTY-CASH-RETURN` | Pengembalian sisa kas kecil |
| `PETTY-CASH-REVERSAL` | Pembalikan pencairan kas kecil |
| `PETTY-CASH-ADJUSTMENT` | Koreksi saldo kas kecil |
| `PENERIMAAN-KASIR` | Penerimaan dari tender Billing **sebelum/tanpa** piutang |
| `PEMBALIKAN-PENERIMAAN-KASIR` | Pembalikan penerimaan kasir |

**Tujuh kode tambahan — revisi 1.1.** Kolom lawan jurnal ditulis sebagai *usulan* Finance;
aturan posting tetap wewenang Accounting.

| Kode | Dipicu oleh | Lawan jurnal yang diusulkan | Dasar |
|---|---|---|---|
| `SALDO-SUBLEDGER` | Finance menutup periode | **Bukan jurnal** — hanya dicocokkan dengan buku besar | `FIN-DEC-035` |
| `PENERIMAAN-UANG-MUKA` | (a) penerimaan saat tagihan sumber masih `OPEN`; (b) `BilDepositMovement` `TOP_UP` | Debit Kas, Kredit Uang Muka Pasien | `FIN-DEC-031` |
| `PEMAKAIAN-UANG-MUKA-DEPOSIT` | `BilDepositMovement` `ALLOCATION` | Debit Uang Muka Pasien, Kredit Piutang — **tidak ada kas bergerak** | `FIN-DEC-040` |
| `PENGEMBALIAN-UANG-MUKA` | (a) `BilDepositMovement` `RELEASE`; (b) `BilRefundCase` `EXECUTED` dengan sumber `ALLOCATION_EXCESS` | Debit Uang Muka Pasien, **Kredit Kas** — kas benar-benar keluar | `FIN-DEC-041` |
| `PENGAKUAN-KELEBIHAN-BAYAR` | `BilRefundableCredit` `ALLOCATION_EXCESS` berstatus `AVAILABLE` | Debit lawan jurnal penerimaan asli (Piutang/Pendapatan), Kredit Uang Muka Pasien — **tidak menyentuh kas** | `FIN-DEC-042` |
| `SELISIH-KAS-SHIFT` | `BilCashVarianceReview` terbentuk (shift menjadi `REVIEWED`) | Debit/Kredit akun selisih kas (suspense). `Amount` **boleh negatif** untuk kekurangan kas | `FIN-DEC-034`, `043` |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Tender yang melahirkan `PENERIMAAN-UANG-MUKA` di-reverse Billing | Debit Uang Muka Pasien, Kredit Kas | `FIN-DEC-044` |

**Tiga pasang kode yang paling mudah tertukar, dan akibatnya bila tertukar:**

| Pasangan | Bedanya | Bila tertukar |
|---|---|---|
| `PENERIMAAN-KASIR` vs `PENERIMAAN-UANG-MUKA` | Yang pertama untuk penerimaan yang benar-benar final; yang kedua untuk uang yang masih menjadi kewajiban ke pasien | Uang titipan pasien terbukukan sebagai pendapatan. Buku besar tetap seimbang, salahnya baru ketahuan saat pemeriksaan |
| `PEMAKAIAN-UANG-MUKA-DEPOSIT` vs `PENGEMBALIAN-UANG-MUKA` | Yang pertama mengurangi piutang; yang kedua mengeluarkan kas | Kas buku besar tidak cocok dengan kas fisik, atau piutang berkurang padahal tagihannya belum dibayar |
| `PEMBALIKAN-PENERIMAAN-KASIR` vs `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Mengikuti kode penerimaan **aslinya**, bukan status tagihan saat pembalikan | Pembalikan mendebit akun yang tidak pernah dikredit saat penerimaan |

**Aturan pemilihan kode pembalikan** (`FIN-DEC-044`). Kode pembalikan **MUST** diturunkan dari
`EventTypeCode` penerimaan asli yang tersimpan di kotak keluar, **MUST NOT** dihitung ulang dari
status tagihan saat pembalikan terjadi — status itu bisa saja sudah berubah dari `OPEN` menjadi
`FINAL` di antara keduanya.

**Beda `PENERIMAAN-KASIR` dan `PENERIMAAN-PIUTANG`.** Keduanya sama-sama uang masuk, tetapi
lawan jurnalnya berbeda. `PENERIMAAN-KASIR` terbit saat pasien membayar di kasir dan belum ada
piutang Finance sama sekali. `PENERIMAAN-PIUTANG` terbit saat penjamin melunasi piutang yang
sudah diakui sebelumnya. Menyamakan keduanya akan membuat piutang berkurang dua kali atau tidak
berkurang sama sekali.

### 5.5 Pemilihan kode penerimaan berdasarkan status tagihan sumber

**Menggantikan seluruh isi bagian 5.5 revisi 1.0** ("Penahanan sebelum tagihan final", wujud
teknis `FIN-DEC-004`). `FIN-DEC-004` sudah `superseded` oleh `FIN-DEC-030`: penerimaan sebelum
tagihan final **tidak lagi ditahan**, melainkan terbit segera dengan kode yang berbeda.

| Keadaan | Penerimaan di Finance | Kode kejadian | Status pengiriman |
|---|---|---|---|
| Tender berhasil, tagihan masih `OPEN` | Dibuat, terlihat penuh | `PENERIMAAN-UANG-MUKA` | `PENDING` — **langsung siap dikirim** |
| Tagihan kemudian `FINAL` | Tidak berubah | `PEMAKAIAN-UANG-MUKA-DEPOSIT` terbit **sebagai kejadian baru** saat piutang dilunasi dari uang muka | `PENDING` |
| Tender berhasil, tagihan sudah `FINAL` | Dibuat | `PENERIMAAN-KASIR` | `PENDING` |
| Tender `REVERSED`, penerimaan asli `PENERIMAAN-UANG-MUKA` | Baris pembalik dibuat | `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | `PENDING` |
| Tender `REVERSED`, penerimaan asli `PENERIMAAN-KASIR` | Baris pembalik dibuat | `PEMBALIKAN-PENERIMAAN-KASIR` | `PENDING` |

**Kenapa penahanan dihapus.** Uangnya nyata dan sudah ada di kasir sejak diterima, jadi menahan
jurnalnya tidak menahan risikonya.

> **Contoh berangka.** Pasien rawat inap membayar Rp 20.000.000 pada 25 November, pulang
> 5 Desember dengan tagihan Rp 32.000.000.
>
> | Tanggal | Kejadian | Nilai | Akibat di buku besar Accounting |
> |---|---|---|---|
> | 25 November | `PENERIMAAN-UANG-MUKA` | Rp 20.000.000 | Debit Kas, Kredit Uang Muka Pasien |
> | 5 Desember | `PENGAKUAN-PIUTANG` | Rp 32.000.000 | Debit Piutang, Kredit Pendapatan |
> | 5 Desember | `PEMAKAIAN-UANG-MUKA-DEPOSIT` | Rp 20.000.000 | Debit Uang Muka Pasien, Kredit Piutang — sisa piutang Rp 12.000.000 |
>
> Dengan cara lama (ditahan), pada tutup buku November kas di buku besar kurang Rp 20.000.000
> dari kas di laci kasir, dan bila saldo subledger kas Finance menghitung uang itu, penutupan
> November tertahan karena toleransi selisih Accounting nol.

**Status `HELD_FOR_FINALIZATION` tidak dihapus dari basis data, tetapi tidak lagi dihasilkan.**
Nilainya tetap sah pada check constraint supaya baris warisan (bila ada) tidak menjadi tidak
valid. Penanganan baris warisan ada di rencana migration `02-backend-architecture.md`.

### 5.6 Saldo subledger per periode

**Bentuk final** (`FIN-DEC-035`, menggantikan draf lima field berdiri sendiri pada `FIN-DEC-023`
yang kini `superseded`). Accounting menolak bentuk berdiri sendiri; Finance menerima penggantinya
apa adanya.

| Hal | Ketentuan |
|---|---|
| Bentuk pesan | Amplop dua belas field yang **sama** seperti kejadian lain, ditambah objek `SubledgerBalance` (bagian 5.2) |
| Nilai saldo | Memakai `Amount` di amplop — **boleh nol atau negatif** khusus pesan saldo |
| Tanggal cut-off | Memakai `AccountingDate` di amplop |
| Periode | `SubledgerBalance.AccountingPeriodCode`, `string` maks 7, bentuk `YYYY-MM` |
| Akun kontrol | `SubledgerBalance.ControlAccountCode`, `string` maks 50 |
| Kardinalitas | Satu saldo per akun kontrol per periode |
| Koreksi | Naikkan `SourceVersion`. **Khusus pesan saldo, `SourceVersion` wajib bilangan bulat positif** (`1`, `2`, `3`, …) karena versi tertinggi yang berlaku — pesan berversi lebih rendah yang datang terlambat tidak menimpa koreksi |
| Bukan jurnal | Pesan saldo **tidak pernah** menjadi jurnal; ia hanya dicocokkan dengan buku besar saat tutup bulan |
| Periode tertutup | Pesan saldo untuk periode yang sudah ditutup Accounting diterima tetapi tidak mengubah angka periode itu |
| Kapan dikirim | Saat Finance menutup periode, sebelum Accounting menutup bukunya |

### 5.7 Titik mulai pengiriman

Sesuai `FIN-DEC-008`:

| Hal | Ketentuan |
|---|---|
| Transaksi yang dikirim | Hanya yang terjadi **setelah** tanggal go-live integrasi |
| Transaksi sebelum tanggal itu | **Tidak** dikirim, **tidak** direkonstruksi dari histori Finance |
| Saldo sebelum cutover | Diinput Accounting sebagai saldo awal manual, satu kali |
| Tanggal pastinya | **Diikat enam gerbang milik Accounting** (`FIN-DEC-037`, revisi 1.1). Cutover jatuh pada tanggal 1 pukul 00.00 WIB di awal periode akuntansi pertama setelah keenamnya lolos |

**Enam gerbang cutover** (milik Accounting, `ACC-DEC-089`/`090`; dicatat di sini supaya Finance
tahu apa yang ditunggu). Keadaan per 24 September 2026 menurut surat Accounting:

| Gerbang | Syarat | Pemilik | Keadaan |
|---|---|---|---|
| `G1` | Kotak masuk Accounting dibangun dan diuji | Rizki | Belum ada kodenya — diverifikasi ulang pada `d6cdfaf9` |
| `G2` | Bagan akun rumah sakit yang sah tersedia, dan aturan posting tiap kode aktif tersusun | Pemilik proses akuntansi + Rizki | Belum — bagan akun saat ini data pengembangan |
| `G3` | Akun layanan aktif, mekanismenya sudah diputuskan | Platform + Yasmin + Rizki | Terbuka (`FIN-OQ-016`) |
| `G4` | Pengirim Finance siap | Yasmin | Kotak keluar sudah ada; **worker pengiriman belum dibangun** |
| `G5` | Saldo awal manual per tanggal cutover siap diinput | Rizki | Bergantung `G2` |
| `G6` | Kejelasan deposit pasien, kelebihan bayar, selisih kas shift, dan perubahan `FIN-DEC-004` | Yasmin + owner Billing | **Dijawab revisi 1.1 ini**; menunggu ratifikasi tujuh kode (`FIN-OQ-017`) |

**Tanggal 1 Oktober 2026 dibatalkan.** Referensi tanggal itu pada versi percakapan sebelumnya
dinyatakan **tidak berlaku** (`FIN-DEC-037`). Finance tidak mengejar tanggal itu dan tidak
meminta percepatan `G1`–`G3` yang bukan kendali Finance.

---

## 6. Yang Finance MUST NOT lakukan

| Larangan | Sebabnya |
|---|---|
| Menulis `BilCashierShift.SystemCash` atau `PhysicalCash` | Milik Billing. Finance membaca dan merekonsiliasi saja (aturan bisnis #12) |
| Menghitung ulang nilai tagihan atau fee dokter | Sumbernya otoritatif (aturan bisnis #1) |
| Mengirim data pasien ke Accounting | Larangan privasi (aturan bisnis #6) |
| Membuat kejadian baru saat menerima balasan `422` | Kejadiannya sudah tersimpan di Accounting |
| Memakai `SourceVersion` yang sama untuk koreksi | Koreksinya tidak akan dijurnal |
| Mengirim mata uang selain rupiah | Kontrak Accounting hanya menerima `IDR` |
| Membuka kembali periode akuntansi | Milik Accounting |
| Mengubah kas kecil dari alur kasir, atau sebaliknya | Dua kolam terpisah (aturan bisnis #15) |
| **Menulis ke `BilDepositAccount`, `BilDepositMovement`, `BilRefundableCredit`, `BilRefundCase`, `BilRefundLine`, atau `BilCashVarianceReview`** | Keenamnya milik Billing. Finance hanya membaca (revisi 1.1, aturan bisnis #9) |
| **Memakai `BilDepositAccount.AvailableBalance` sebagai nilai kejadian** | Itu saldo berjalan, bukan mutasi. Nilai kejadian selalu diambil dari baris mutasi (revisi 1.1) |
| **Menerbitkan `PEMAKAIAN-UANG-MUKA-DEPOSIT` untuk pengembalian uang, atau sebaliknya** | Lawan jurnalnya berbeda — satu mengurangi piutang, satu mengeluarkan kas (bagian 5.4, revisi 1.1) |
| **Menghitung kode pembalikan dari status tagihan saat pembalikan terjadi** | Kode pembalikan diturunkan dari kode penerimaan asli (`FIN-DEC-044`) |
| **Menerbitkan `SELISIH-KAS-SHIFT` sebelum selisihnya disahkan** | Angka sebelum `REVIEWED` belum final; menerbitkannya memaksa jurnal koreksi yang tidak perlu (`FIN-DEC-043`) |

---

## 7. Ketergantungan yang belum tertutup

**Diperbarui revisi 1.1.** Dua ketergantungan tertutup, empat masih terbuka.

| Ketergantungan | Pemilik | Keadaan | Dampak bila belum turun |
|---|---|---|---|
| ~~Konfirmasi bentuk `BilCollectionHandoff`~~ | Billing | **TERTUTUP** 22 September 2026 — tabelnya dibangun (`BKC-DES-037`) dan sudah dikonsumsi `FinanceBillingIntakeService` (`FIN-CAP-007`, `FIN-CAP-025`) | — |
| ~~Nama field saldo subledger~~ | Accounting | **TERTUTUP** 24-25 September 2026 — Accounting menetapkan bentuknya, Finance menerima (`FIN-DEC-035`) | — |
| Konfirmasi perluasan `BilArHandoff` | Billing + HR | Terbuka (`FIN-DEC-006`/`016`, `FIN-CQ-03`) | Rumpun manfaat karyawan tidak dapat dimulai — sudah `OPEN DECISION`, di luar seluruh gelombang |
| Endpoint penerima Accounting Event | Accounting | Terbuka — gerbang `G1`, diverifikasi ulang belum ada pada `d6cdfaf9` | Worker pengiriman tetap dimatikan; kotak keluar tetap terisi dan aman |
| **Ratifikasi tujuh kode kejadian baru** | Accounting | Terbuka (`FIN-OQ-017`) — dikirim lewat `evidence/04`, dikoreksi `evidence/05` | Kejadian berjenis belum terdaftar akan dijawab `422` `EVENT_TYPE_NOT_REGISTERED` dan tertahan. **Ini yang menahan implementasi `FIN-DEC-030`** |
| Mekanisme autentikasi akun layanan | Platform + Accounting | Terbuka (`FIN-OQ-016`) — tiga syarat organisasinya sudah ditetapkan (bagian 5.1) | Pengiriman tidak dapat diaktifkan; gerbang `G3` |
| Lawan jurnal refund `SETTLEMENT`/`REFERRED_OUTPATIENT_ADMIN` | Yasmin (Finance) | Terbuka (`FIN-OQ-018`) | Tidak menahan apa pun saat ini — sengaja di luar cakupan bagian 2a |
