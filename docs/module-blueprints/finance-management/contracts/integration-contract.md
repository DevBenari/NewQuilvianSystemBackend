# Kontrak Integrasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.8` |
| `last_changed_in` (1.8) | `FIN-INTEGRATION-1.8` — AMENDMENT REVISI 18, 6 Oktober 2026. **Kontrak Integrasi Resmi Lintas Domain Piutang Manfaat Karyawan** (bagian P: `INT-FIN-BIL-001`, `INT-FIN-HR-001`, `INT-HR-FIN-001`, `INT-HR-FIN-002`, `INT-HR-FIN-003`, `INT-BIL-FIN-002`). Status **`draft`** |
| `last_changed_in` | `FIN-INTEGRATION-1.7` — AMENDMENT REVISI 17, 5 Oktober 2026 (bagian O: catatan keadaan tanpa kenaikan nomor kontrak). Sebelumnya `1.6` (Revisi 9) |
| Status | `approved` — Revisi 1.8 disetujui pemilik (Yasmin) 6 Oktober 2026 |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-10-06 (untuk `1.8`) |
| Input revision | `00-interview-decisions.md` — `FIN-DEC-183`..`202` (Konfirmasi Billing, HR, dan Registrasi), `02-backend-architecture.md` bagian P |
| Kontrak eksternal yang diikuti | **`ACC-XMOD-0.3`** milik Accounting — naik dari `0.2`, diratifikasi sisi Finance lewat `FIN-DEC-039` |
| Backend SHA yang diverifikasi | **`cba60cb0`** (impact scan 28 September 2026, `02-backend-architecture.md` bagian `E.1`) — naik dari `d6cdfaf9`/`96bf9746` |
| Dampak kompatibilitas | **Revisi 1.4 memuat tiga perubahan perilaku pada kode yang sudah berjalan** (nama empat `EventTypeCode`, nilai kredit retur termasuk PPN, properti `Components` dihilangkan dari pesan) — rinciannya bagian 5.10 dan `02-backend-architecture.md` `E.9`. **Lima kode dihapus dan diganti** (bukan aditif): `SELISIH-KAS-SHIFT`, `POTONGAN-PIUTANG-NON-TUNAI` beserta pembaliknya, `PEMAKAIAN-DEPOSIT-RETUR`, dan lima alias `AR_*`/`AP_*` |

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
| `BilDepositMovement` tipe `RELEASE` **yang berpasangan dengan mutasi `REVERSAL` ber-`SettlementId` sama** | Alokasi uang muka ke tagihan dibatalkan; **tidak ada kas yang bergerak** | `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` — **dikoreksi `FIN-INTEGRATION-1.6`**, sebelumnya `PENGEMBALIAN-UANG-MUKA` | `IdempotencyKey` milik Billing |
| `BilDepositMovement` tipe `RELEASE` **tanpa** mutasi `REVERSAL` yang bersesuaian | Jalur yang belum dikenal Finance | **Nol kejadian.** Baris intake `ERROR` menunjuk `FIN-OQ-037` (`FIN-VAL-145`) | `IdempotencyKey` milik Billing |
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
| `PENGEMBALIAN-UANG-MUKA` | `BilRefundCase` `EXECUTED` dengan sumber `ALLOCATION_EXCESS` **atau `SETTLEMENT`** (`FIN-DES-056`). Butir `BilDepositMovement` `RELEASE` **dicabut `FIN-INTEGRATION-1.6`** — mutasi itu tidak mengeluarkan kas, lihat `FIN-DES-064` | Debit Uang Muka Pasien, **Kredit Kas** — kas benar-benar keluar | `FIN-DEC-041`, dikoreksi `FIN-DES-064` |
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

### 5.8 Kode ke-25 — PPN Masukan Pembelian (diusulkan, AMENDMENT REVISI 4)

Dipicu rumpun Purchasing/AP baru (`FIN-SC-008`, `FIN-DEC-045`, `FIN-DEC-046`). **Berbeda dari
tujuh kode bagian 5.4** yang sudah dikirim sebagai koreksi atas kode yang sudah berjalan, kode
ini diusulkan **sebelum satu baris kode pun ditulis** untuk Purchasing/AP — dikirim lewat
`evidence/06-usulan-kode-ppn-masukan-untuk-accounting.md`.

| Kode | Dipicu oleh | Lawan jurnal yang diusulkan | Dasar |
|---|---|---|---|
| `PPN-MASUKAN-PEMBELIAN` | Purchasing Invoice disetujui, dicatat sebagai utang supplier | Debit PPN Masukan (kredit pajak), Kredit Utang Supplier — bagian PPN dari nilai invoice | `FIN-DEC-046`, `FIN-DEC-053` |

**`Amount` pada kejadian ini adalah nilai PPN-nya saja**, bukan nilai invoice penuh — nilai pokok
barang/jasa dicatat lewat kejadian pengakuan utang supplier terpisah, yang kodenya **belum
diusulkan** pada amendment ini (`02-backend-architecture.md` bagian C.11).

**Status per 25 September 2026: diusulkan, belum diratifikasi (`FIN-OQ-020`).** Ini adalah
**gerbang keras** (`FIN-DEC-046`) — berbeda dari tujuh kode `FIN-OQ-017` yang boleh menunggu
ratifikasi sambil kodenya tetap dipakai secara terbatas, worker pengiriman untuk
`PPN-MASUKAN-PEMBELIAN` **MUST NOT** diaktifkan sebelum Accounting meratifikasi. Baris outbox
tetap ditulis `PENDING` saat Purchasing Invoice disetujui (`state-transition-matrix.md` B.4),
hanya pengirimannya yang tertahan (`FIN-VAL-122`).

### 5.9 Kode ke-26 s.d. 29 — potongan AR, retur, dan pemakaian deposit (diusulkan, AMENDMENT REVISI 5)

Diputuskan sisi Finance lewat `FIN-DEC-058`, `061`, `062`. Keempatnya diusulkan ke Accounting
dalam **satu** surat evidence (`FIN-OQ-026`) — surat itu **belum dikirim**.

| # | Kode | Dipicu oleh | `SourceTransactionId` | `Amount` | Lawan jurnal yang diusulkan | Dasar |
|---|---|---|---|---|---|---|
| 26 | `POTONGAN-PIUTANG-NON-TUNAI` | Potongan PPh 23/biaya admin bank dicatat bersama alokasi penerimaan | `DeductionNumber` | Nilai potongan | Debit PPh 23 Dibayar di Muka (atau Beban Administrasi Bank), Kredit Piutang — **tidak menyentuh kas** | `FIN-DEC-055`, `058` |
| 27 | `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` | Alokasi yang membawa potongan dibalik, manual maupun otomatis (`FIN-DEC-021`) | `DeductionNumber` baris pembalik | Nilai potongan asli, positif | Kebalikan kode 26 | `FIN-DEC-062` |
| 28 | `RETUR-PEMBELIAN` | Retur pembelian `CONFIRMED` | `ReturnNumber` | Nilai retur | Debit Piutang Retur Supplier, Kredit Persediaan/Pembelian | `FIN-DEC-047`, `061` |
| 29 | `PEMAKAIAN-DEPOSIT-RETUR` | Pembayaran supplier `PAID` yang memakai Deposit Retur | `PaymentNumber` | `DepositAppliedAmount` | Debit Utang Supplier, Kredit Piutang Retur Supplier — **tidak menyentuh kas** | `FIN-DEC-057`, `061` |

**Satu perubahan pada kejadian yang sudah berjalan.** Kejadian pembayaran utang (`AP_PAYMENT`)
yang ditulis `FinancePaymentService.MarkPaidAsync` hari ini bernilai `TotalAmount`. Untuk pembayaran
yang memakai deposit, nilainya **turun menjadi `TotalAmount − DepositAppliedAmount`**; porsi deposit
pindah ke kode 29. Bila seluruh pembayaran dilunasi deposit, `AP_PAYMENT` **tidak** ditulis sama
sekali. Pembayaran tanpa deposit: nilainya identik dengan hari ini.

**Contoh berangka.** Pembayaran Rp 10.000.000 ke PT Contoh Farma, Rp 2.500.000 dari Deposit Retur:

| Kode | Nilai | Akibat di buku besar |
|---|---|---|
| `AP_PAYMENT` | Rp 7.500.000 | Debit Utang Supplier, Kredit Kas — sama dengan uang yang benar-benar keluar |
| `PEMAKAIAN-DEPOSIT-RETUR` | Rp 2.500.000 | Debit Utang Supplier, Kredit Piutang Retur Supplier |
| Jumlah pengurang utang | Rp 10.000.000 | = `TotalAmount` |

**Gerbangnya sama dengan kode 25** (`FIN-DEC-056`): baris outbox keempat kode **ditulis `PENDING`**
sejak transaksinya terjadi; hanya worker pengirimannya yang **MUST NOT** diaktifkan sebelum
Rizki meratifikasi (`FIN-VAL-132`).

### 5.10 Hasil ratifikasi Accounting dan katalog final (AMENDMENT REVISI 6)

Owner Accounting menjawab ketiga surat Finance dalam satu balasan
(`docs/module-blueprints/accounting/evidence/14-balasan-accounting-atas-kode-finance-05-06-07.md`,
28 September 2026). Sisi Finance ditutup `FIN-DEC-063`..`071`; pemetaannya ke source ada di
`02-backend-architecture.md` AMENDMENT REVISI 6.

#### 5.10.1 Hasil ratifikasi per kode yang pernah diusulkan

| Kode yang diusulkan Finance | Keputusan Accounting | Tindakan Finance |
|---|---|---|
| `SALDO-SUBLEDGER` | Disepakati | Tidak ada |
| `PENERIMAAN-UANG-MUKA` | Diratifikasi | Tidak ada |
| `PEMAKAIAN-UANG-MUKA-DEPOSIT` | Diratifikasi | Tidak ada |
| `PENGEMBALIAN-UANG-MUKA` | Diratifikasi | Cakupan diperluas ke kredit `SETTLEMENT` (`FIN-DES-056`) |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Diratifikasi + satu pertanyaan | Dijawab `FIN-DEC-063`; pasangannya `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` diusulkan |
| `PENGAKUAN-KELEBIHAN-BAYAR` | Diratifikasi **bersyarat** | Tiga syarat diterima (`FIN-DEC-067`); cakupan diperluas ke `SETTLEMENT` |
| `SELISIH-KAS-SHIFT` | **Tidak diratifikasi** — dipecah dua | Diganti `SELISIH-KAS-KURANG` + `SELISIH-KAS-LEBIH` (`FIN-DEC-064`) |
| `PPN-MASUKAN-PEMBELIAN` | **Diratifikasi**; akun debit menunggu G2 Accounting | Tidak ada. `FIN-OQ-020` tertutup sisi Finance |
| `POTONGAN-PIUTANG-NON-TUNAI` + pembaliknya | **Tidak diratifikasi** — dipecah dua pasang | Diganti empat kode (`FIN-DEC-065`) |
| `RETUR-PEMBELIAN` | Diratifikasi **bersyarat** — pokok tanpa PPN, kredit satu akun | Nilai diperjelas; porsi PPN lewat kode baru (`FIN-DEC-068`, `FIN-DES-055`) |
| `PEMAKAIAN-DEPOSIT-RETUR` | Diratifikasi; saran nama tidak mengikat | Nama diganti `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` (`FIN-DEC-066`) |

#### 5.10.2 Kode yang tidak ada lagi

Kelima nama berikut **MUST NOT** ditulis ke `EventTypeCode` oleh kode mana pun sesudah revisi ini.
Menyimpannya sebagai alias dilarang: dua nama untuk satu kejadian adalah cara paling mudah membuat
satu fakta terkirim dua kali.

| Nama yang dihapus | Penggantinya |
|---|---|
| `SELISIH-KAS-SHIFT` | `SELISIH-KAS-KURANG` atau `SELISIH-KAS-LEBIH`, sesuai arah selisih |
| `POTONGAN-PIUTANG-NON-TUNAI` | `POTONGAN-PPH23-PIUTANG` atau `POTONGAN-BIAYA-BANK-PIUTANG` |
| `PEMBALIKAN-POTONGAN-PIUTANG-NON-TUNAI` | `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` atau `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` |
| `PEMAKAIAN-DEPOSIT-RETUR` | `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` |
| `AR_CREATED`, `AR_PAYMENT`, `AR_WRITEOFF`, `AP_CREATED`, `AP_PAYMENT` | `PENGAKUAN-PIUTANG`, `PENERIMAAN-PIUTANG`, `PEMUTIHAN-PIUTANG`, `PENGAKUAN-HUTANG-SUPPLIER`, `PEMBAYARAN-HUTANG-SUPPLIER` |

#### 5.10.3 Delapan kode baru pada revisi ini

| # | Kode | Dipicu oleh | `SourceTransactionId` | `Amount` | Lawan jurnal yang diusulkan | Keadaan |
|---|---|---|---|---|---|---|
| 30 | `SELISIH-KAS-KURANG` | Shift kasir mencapai `REVIEWED`, kas fisik **kurang** | `BilCashierShift.Id` | Selisihnya, positif | Debit beban selisih kas, kredit Kas | Nama disepakati Accounting |
| 31 | `SELISIH-KAS-LEBIH` | Shift kasir mencapai `REVIEWED`, kas fisik **lebih** | `BilCashierShift.Id` | Selisihnya, positif | Debit Kas, kredit pendapatan selisih kas | Nama disepakati Accounting |
| 32 | `POTONGAN-PPH23-PIUTANG` | Potongan `DeductionType = PPH23` dicatat | `DeductionNumber` | Nilai potongan | Debit PPh 23 dibayar di muka, kredit Piutang | Nama diusulkan Accounting, diterima Finance |
| 33 | `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` | Alokasi pembawa potongan `PPH23` dibalik | `DeductionNumber` pembalik | Nilai asli, positif | Kebalikan kode 32 | Idem |
| 34 | `POTONGAN-BIAYA-BANK-PIUTANG` | Potongan `DeductionType = BANK_ADMIN_FEE` dicatat | `DeductionNumber` | Nilai potongan | Debit beban administrasi bank, kredit Piutang | Idem |
| 35 | `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | Alokasi pembawa potongan bank dibalik | `DeductionNumber` pembalik | Nilai asli, positif | Kebalikan kode 34 | Idem |
| 36 | `PPN-MASUKAN-RETUR-PEMBELIAN` | Retur `CONFIRMED` dengan `PPNAmount > 0` | `ReturnNumber` | `PPNAmount` | Debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN` | **Diusulkan** (`FIN-OQ-029`) |
| 37 | `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` | Mutasi `RELEASE` yang berpasangan dengan mutasi `REVERSAL` ber-`SettlementId` sama — **dikoreksi `FIN-INTEGRATION-1.6`**, sebelumnya "pembalikan mutasi bertipe `ALLOCATION`" yang tidak pernah ada | `BilDepositMovement.Id` | Nilai mutasi `RELEASE` | Debit Piutang, kredit Uang Muka Pasien | **Diusulkan** (`FIN-OQ-027`); **pemicunya kini ADA** sejak `BKC-DEC-131` — lihat `FIN-DES-064` |

#### 5.10.4 Dua kode penanda — satu-satunya kejadian bernilai nol

Kedua kode ini **bukan transaksi** dan **tidak membawa lawan jurnal**. Keduanya ada semata supaya
Accounting dapat menegakkan `ACC-DEC-065`: shift yang belum ditutup menahan tutup bulan.

| # | Kode | Kapan terbit | `Amount` | Dasar |
|---|---|---|---|---|
| 38 | `PENUTUPAN-SHIFT-KASIR` | Shift mencapai keadaan tertutup final: `CLOSED` (tanpa selisih) **atau** `REVIEWED` (selisihnya sudah disahkan) | **`0`** | `FIN-DEC-070`, `FIN-DES-054` |
| 39 | `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Shift `CLOSED`/`REVIEWED` dibuka kembali menjadi `REOPENED` | **`0`** | `FIN-DES-054` |

**Dua hal yang MUST dikonfirmasi Accounting** (`FIN-OQ-030`, `FIN-OQ-032`, `FIN-OQ-035`) —
**butir 1 sudah terjawab bukti pada 28 September 2026**:

1. **TERJAWAB, DAN JAWABANNYA: DITOLAK.** `/trace-existing-capabilities` membaca langsung
   `AccAccountingEventService.cs`: baris 1098 menolak `Amount <= 0` dengan **`400`** untuk pesan
   transaksi, **dan** baris 1075-1079 menolak pesan saldo dengan **`409`** karena jalurnya belum
   dibangun (`BE-ACC-P2-028`). **Tidak ada celah yang lolos** untuk kejadian bernilai nol hari ini.
   Karena itu butir ini berubah sifat: dari "mohon dikonfirmasi" menjadi **permintaan perubahan**
   — Accounting diminta memperluas validasinya untuk kedua kode penanda, atau mengaktifkan jalur
   pesan saldo. Dicatat sebagai `FIN-OQ-035` dan dikirim lewat `evidence/16`, **terpisah** dari
   surat ratifikasi nama kode, karena levelnya berbeda: ini menyentuh aturan validasi di source
   Accounting sendiri. Keputusan Finance (`FIN-DEC-075`): bentuk penanda **tidak** diubah, dan
   worker pengirimannya digerbang sampai jawabannya turun (`FIN-DES-059`).
2. Kode pembalik (39) belum pernah diusulkan sebelumnya. Ia ada karena
   `CashierShiftService.ReopenAsync` mengizinkan shift `CLOSED`/`REVIEWED` dibuka kembali; tanpa
   penanda pembalik, Accounting akan terus menganggap shift itu tertutup dan mengizinkan tutup
   bulan atas periode yang sebenarnya kembali terbuka.

**Kenapa pemicunya bukan hanya `REVIEWED`.** `FIN-DEC-070` semula menulis "terbit saat status
menjadi `REVIEWED`". Pemeriksaan source menunjukkan shift yang kasnya **pas** tidak pernah mencapai
`REVIEWED` — ia berhenti di `CLOSED`. Diikuti apa adanya, mayoritas shift tidak akan pernah
menerbitkan penanda dan tutup bulan tertahan selamanya. Karena itu **kedua** keadaan tertutup final
menerbitkan penanda, sementara `CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT` **tidak** — kedua
keadaan itu memang harus tetap menahan tutup bulan.

#### 5.10.5 Empat pelurusan bentuk pesan — MUST selesai sebelum worker mana pun hidup

Urgensinya naik pada revisi ini: impact scan 28 September 2026 menemukan **kotak masuk Accounting
sudah dibangun** (`AccAccountingEventService`). Pesan yang salah bentuk kini benar-benar akan
ditolak, bukan sekadar salah di dokumen.

| # | Yang salah | Yang benar | Akibat bila dibiarkan |
|---:|---|---|---|
| 1 | `PayloadJson` selalu memuat properti `Components`, terkirim sebagai `"Components": null` | Properti **dihilangkan** bila tidak ada komponen. Pesan tanpa `Components` sudah berarti seluruh nilai memakai komponen `TOTAL` | **DIKOREKSI 28 September 2026 (`FIN-CQ-05`): bukan pemblokir.** Kotak masuk Accounting menormalkan `null` menjadi daftar kosong (`AccAccountingEventService.cs` baris 1043), sehingga pesan Finance hari ini **diterima**. Yang benar-benar akan ditolak adalah `"Components": "TOTAL"` sebagai **teks** — bentuk yang hanya pernah muncul di **contoh dokumen** Finance, tidak pernah di kodenya. Pelurusan ini tetap dikerjakan agar sejalan dengan permintaan Accounting, tetapi **prioritasnya kebersihan kontrak**, bukan pemblokir runtime |
| 2 | `evidence/07` menyatakan `SELISIH-KAS-SHIFT` boleh bernilai negatif | Tidak ada kejadian transaksi yang boleh negatif. Nilai nol hanya untuk kedua kode penanda (5.10.4) dan pesan saldo | Kejadian bernilai negatif ditolak `400` |
| 3 | Lima titik tulis memakai nama pendek `AP_CREATED`/`AP_PAYMENT`/`AR_PAYMENT`/`AR_WRITEOFF` | Nama katalog. **Dugaan Accounting sudah diverifikasi benar** terhadap bagian 5.4 berkas ini | Kejadian tersimpan **Tertahan** `EVENT_TYPE_NOT_REGISTERED` dan tidak pernah menjadi jurnal |
| 4 | Contoh `evidence/06` menulis total Rp 11.000.000 untuk barang Rp 10.000.000 + PPN Rp 1.100.000 | Rp 11.100.000 | Hanya contoh; diperbaiki agar tidak disalin ke uji |

**Baris outbox yang sudah tertulis dengan nama lama MUST NOT ditimpa** oleh migration data — ia
salinan pesan yang memang pernah disusun begitu. Karena worker belum pernah hidup, seluruhnya masih
`PENDING` dan belum pernah sampai ke Accounting; penanganannya keputusan operasional terpisah yang
**MUST** diambil sebelum worker diaktifkan.

#### 5.10.6 Dua hal yang tidak dapat ditutup kontrak ini

| Hal | Sebabnya | Dicatat sebagai |
|---|---|---|
| Kejadian untuk refund kredit `REFERRED_OUTPATIENT_ADMIN` | Kredit itu lahir dari biaya administrasi rawat jalan yang **sudah dibayar** lalu dialihkan ke tagihan rawat inap (`BKC-DEC-119`), dan baris biaya aslinya tidak di-void. Akun debit saat dicairkan tunai bergantung pada apakah pendapatan administrasi itu dibalik — kebijakan Billing + Accounting, bukan Finance | `FIN-OQ-031`. Sementara itu baris intake ditulis **`ERROR`** dan **nol** kejadian diterbitkan; `PENGEMBALIAN-UANG-MUKA` **MUST NOT** dipakai karena lawan jurnalnya salah |
| ~~Pemicu kode 37 (`PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`)~~ | **TERTUTUP `FIN-INTEGRATION-1.6`.** Sebelumnya tercatat "tidak ada jalur di Billing yang menghasilkannya". Billing menutup `FIN-OQ-034` lewat `BKC-DEC-128`..`131`: pembalikan tender top-up kini membatalkan alokasi tagihan secara LIFO dan menulis mutasi `RELEASE`, lalu menulis mutasi `REVERSAL` atas top-up-nya. Pemicu kode 37 karena itu **ada**, hanya bukan bentuk yang semula dibayangkan — lihat `FIN-DES-064` | — |
| Nama `RELEASE` masih bermakna ganda di sisi Billing | Satu-satunya penulisnya hari ini adalah pembatalan alokasi (`BKC-DEC-131`), tetapi `BillingDepositService` masih menjumlahkan seluruh mutasi `RELEASE` sebagai `totalRefunded` — "dana yang dikembalikan". Penulis berikutnya dapat memakainya untuk pengembalian kas tanpa Finance mengetahuinya | `FIN-OQ-037` — permintaan penanda eksplisit ke owner **Billing**, MUST dikirim sebagai surat evidence tersendiri. Selama belum turun, mutasi `RELEASE` tanpa pasangan `REVERSAL` ditolak *fail-closed* sebagai baris intake `ERROR` (`FIN-VAL-145`) |

#### 5.10.7 Kode potongan AR untuk `DeductionType = OTHER` — belum ada

Pemecahan satu kode menjadi dua meninggalkan `DeductionType = OTHER` tanpa kode: sebelum
pemecahan ia menumpang `POTONGAN-PIUTANG-NON-TUNAI`, sesudah pemecahan tidak ada akun debit yang
sah untuknya. Potongan berjenis `OTHER` karena itu **MUST ditolak** (`FIN-VAL-137`) sampai
Accounting meratifikasi kode ketiga (`FIN-OQ-033`). Menerima barisnya lalu tidak menerbitkan
kejadian adalah pilihan yang **MUST NOT** diambil — piutang berkurang di Finance tanpa jejak di
buku besar.

### 5.11 Kontrak as-is kotak masuk Accounting (AMENDMENT REVISI 7)

Sampai 25 September 2026 bagian ini hanya dapat dikutip dari dokumen Accounting, karena endpoint
penerimanya belum ada. Sejak `cba60cb0` endpoint itu **sudah ada dan sudah dapat dibaca**, dan
`/trace-existing-capabilities` mencatat kontraknya baris demi baris di
`01-existing-capability-map.md` **bagian 15.3**. Bagian ini tidak menyalinnya ulang; ia hanya
menyebut yang mengubah cara kerja worker Finance.

| Hal | Kenyataan yang MUST diikuti worker Finance |
|---|---|
| Base URL | `api/v1/corporate/accounting/accounting-events`, `POST /`, hak akses `AccountingEvent : Receive` |
| Pesan yang sudah pernah diterima | Dijawab **`200`**, bukan error — kombinasi `SourceModule`+`SourceTransactionId`+`EventTypeCode`+`SourceVersion` bersifat **idempoten**. Worker **MUST** memperlakukan `200` sebagai sukses, bukan sebagai kegagalan yang perlu diulang |
| Pesan baru | Dijawab **`201`** beserta tanda terima |
| Bentuk tanda terima | `AccountingEventId`, `EventNumber`, `EventStatus`, `JournalNumber?`, `AccountingPeriodCode?`, `HoldReasonCode?`, `ReceivedAt`. **Tidak ada** "nomor tanda terima" tersendiri — kolom `FinAccountingEventOutbox.AccountingReceiptNumber` karena itu MUST diisi dari `AccountingEventId`, dan penamaan kolomnya adalah utang penamaan yang dicatat, bukan field yang hilang |
| Kejadian yang tertahan | Dijawab **`422`** beserta `HoldReasonCode`: `EVENT_TYPE_NOT_REGISTERED`, `POSTING_RULE_MISSING`, `COMPONENT_UNMAPPED`, `COMPONENT_MISSING`. Worker **MUST NOT** membuat kejadian baru sebagai tanggapan — barisnya sudah tersimpan di Accounting (larangan yang sudah ada di bagian 6) |
| Nilai nol | **`400`** untuk pesan transaksi; **`409`** untuk pesan saldo (jalurnya belum dibangun). Inilah yang menggerbang kedua kode penanda — lihat 5.10.4 butir 1 dan `FIN-OQ-035` |
| Mata uang selain rupiah | **`409`** |
| `LegalEntityId` tidak dikenal/tidak aktif | **`422`** |
| Field tambahan di luar 12 field + `Components` | Dipindai penanda identitas pasien; bila terdeteksi → **`400`**. Bentuk payload Finance yang hanya 12 field + `Components` aman |
| Batas coba ulang di sisi Accounting | 3 percobaan terjadwal, 100 baris per gelombang |

**Konsekuensi yang paling mudah terlewat.** `200` dan `201` **sama-sama sukses**. Worker yang
memperlakukan apa pun selain `201` sebagai kegagalan akan mengulang pesan yang sebenarnya sudah
diterima, menaikkan `AttemptCount` tanpa sebab, dan akhirnya menandai baris `FAILED` padahal
jurnalnya sudah terbentuk di Accounting.

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
| **Menerbitkan kejadian selisih kas sebelum selisihnya disahkan** | Angka sebelum `REVIEWED` belum final; menerbitkannya memaksa jurnal koreksi yang tidak perlu (`FIN-DEC-043`) |
| **Menerbitkan kejadian selisih kas dari baris review yang hasilnya `NEEDS_FOLLOW_UP`** | Satu shift dapat menghasilkan **dua** baris pengesahan selisih dengan nilai yang sama; menerbitkan keduanya membuat satu selisih terjurnal dua kali. Hanya baris yang membawa shift ke `REVIEWED` yang menerbitkan kejadian (bagian 5.10, `FIN-DES-053`) |
| **Memakai `BilCashVarianceReview.Id` sebagai `SourceTransactionId` kejadian selisih kas atau penanda shift** | Kuncinya MUST `BilCashierShift.Id`: baris review bisa ada dua, dan shift tanpa selisih tidak punya baris review sama sekali (`FIN-DES-053`, `FIN-DES-054`) |
| **Membiarkan `SourceVersion` dihitung otomatis untuk kejadian selisih kas dan penanda shift** | Versi otomatis menaikkan angka dan **melewati** unique index dua lapis, sehingga kejadian kedua untuk shift yang sama lolos. Versi MUST dipatok per siklus tutup (`FIN-DES-053`) |
| **Menerbitkan penanda shift tertutup hanya saat `REVIEWED`** | Shift yang kasnya pas berhenti di `CLOSED` dan tidak pernah melewati `REVIEWED`; mayoritas shift tidak akan pernah menerbitkan penanda dan tutup bulan tertahan selamanya (bagian 5.10.4) |
| **Mengirim `Amount = 0` untuk kode selain kedua kode penanda dan pesan saldo** | Aturan Accounting menolak kejadian transaksi bernilai nol. Daftar kode yang boleh bernilai nol MUST tertutup, bukan pemeriksaan `Amount == 0` yang longgar (`FIN-DES-054`) |
| **Menyertakan properti `Components` pada pesan yang tidak punya komponen** | Pesan tanpa `Components` sudah berarti seluruh nilai memakai komponen `TOTAL`; menyertakannya bernilai null berpotensi ditolak `400` (bagian 5.10.5 butir 1) |
| **Menulis kelima alias `AR_*`/`AP_*`, atau menyimpannya sebagai alias di samping nama katalog** | Satu fakta akan terkirim dengan dua nama berbeda (bagian 5.10.2) |
| **Memakai `PENGEMBALIAN-UANG-MUKA` untuk refund kredit `REFERRED_OUTPATIENT_ADMIN`** | Lawan jurnalnya salah — tidak pernah ada uang muka yang diakui untuk kredit itu. Jurnal yang seimbang tetapi keliru lebih sulit ditemukan daripada baris `ERROR` (bagian 5.10.6) |
| **Mencatat potongan AR berjenis `OTHER`** | Tidak ada kode dan tidak ada akun debit yang sah untuknya sesudah pemecahan (bagian 5.10.7) |
| **Menulis mutasi pembalik ke `BilDepositMovement` untuk menutup gap pembalikan tender** | Finance MUST NOT menulis ke tabel Billing. Perbaikannya milik owner Billing (`FIN-OQ-034`) |
| **Mengirim nilai retur pembelian termasuk PPN** | Syarat ratifikasi Accounting: nilai `RETUR-PEMBELIAN` MUST pokok tanpa PPN; porsi PPN lewat kode 36 (bagian 5.10.3) |
| **Mengirim kejadian `PPN-MASUKAN-PEMBELIAN` sebelum ratifikasi Accounting** | Gerbang keras `FIN-DEC-046` — berbeda dari kode lain, rumpun Purchasing/AP sengaja tidak boleh mengirim kode ini walau sebagai usulan sepihak (bagian 5.8, AMENDMENT REVISI 4) |
| **Menulis akun/COA PPN Masukan sendiri** | Wewenang Accounting, bukan Finance (`evidence/06` bagian 3) |
| **Menulis potongan AR dengan `AR_PAYMENT`, `PENERIMAAN-PIUTANG`, atau `PENYESUAIAN-PIUTANG`** | Potongan tidak membawa kas dan bukan koreksi maker-checker — MUST memakai kode 26/27 (bagian 5.9, AMENDMENT REVISI 5) |
| **Memasukkan porsi deposit ke `AP_PAYMENT`** | Membukukan kas keluar untuk uang yang tidak pernah bergerak — porsi deposit MUST memakai kode 29 (bagian 5.9) |

---

## 7. Ketergantungan yang belum tertutup

**Diperbarui revisi 1.1.** Dua ketergantungan tertutup, empat masih terbuka. **AMENDMENT REVISI 4
menambah satu ketergantungan baru** (baris terakhir).

| Ketergantungan | Pemilik | Keadaan | Dampak bila belum turun |
|---|---|---|---|
| ~~Konfirmasi bentuk `BilCollectionHandoff`~~ | Billing | **TERTUTUP** 22 September 2026 — tabelnya dibangun (`BKC-DES-037`) dan sudah dikonsumsi `FinanceBillingIntakeService` (`FIN-CAP-007`, `FIN-CAP-025`) | — |
| ~~Nama field saldo subledger~~ | Accounting | **TERTUTUP** 24-25 September 2026 — Accounting menetapkan bentuknya, Finance menerima (`FIN-DEC-035`) | — |
| Konfirmasi perluasan `BilArHandoff` | Billing + HR | Terbuka (`FIN-DEC-006`/`016`, `FIN-CQ-03`) | Rumpun manfaat karyawan tidak dapat dimulai — sudah `OPEN DECISION`, di luar seluruh gelombang |
| ~~Endpoint penerima Accounting Event~~ | Accounting | **TERTUTUP** 28 September 2026 — `AccAccountingEventService` ditemukan ada pada `cba60cb0`, sejalan dengan pernyataan Accounting di `evidence/14` bagian 4.4 (dibangun dan diuji pengembang; UAT belum). `FIN-CAP-018` **stale** dan MUST diperbarui `/trace-existing-capabilities` | Gerbang `G1` sisi keberadaan endpoint tidak lagi menahan. Yang tersisa UAT, milik Accounting |
| ~~Ratifikasi tujuh kode kejadian baru~~ | Accounting | **TERTUTUP** 28 September 2026 lewat `evidence/14` — lima diratifikasi apa adanya, satu bersyarat (syarat diterima `FIN-DEC-067`), satu dipecah dua (`FIN-DEC-064`) | — |
| ~~Ratifikasi kode `PPN-MASUKAN-PEMBELIAN`~~ | Accounting | **TERTUTUP** 28 September 2026 — diratifikasi; akun debit persis menjadi urusan G2 Accounting | Worker boleh hidup begitu G2 Accounting menetapkan aturan postingnya |
| ~~Ratifikasi kode 26–29~~ | Accounting | **TERTUTUP/DIGANTI** — kode 26/27 ditolak sebagai satu kode dan dipecah empat; ratifikasi penggantinya menjadi `FIN-OQ-028` | — |
| Mekanisme autentikasi akun layanan | Platform + Accounting | Terbuka (`FIN-OQ-016`) — tiga syarat organisasinya sudah ditetapkan (bagian 5.1) | Pengiriman tidak dapat diaktifkan; gerbang `G3` |
| Konfirmasi perluasan `BilArHandoff` | Billing + HR | Terbuka (`FIN-DEC-006`/`016`, `FIN-CQ-03`) | Rumpun manfaat karyawan tetap `OPEN DECISION` |
| **Ratifikasi kode 37** `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` | Accounting | Terbuka (`FIN-OQ-027`) — surat `evidence/15` belum dikirim | Hanya menahan aktivasi worker kode ini — dan pemicunya sendiri masih tertahan `FIN-OQ-034` |
| **Ratifikasi empat kode potongan piutang** (kode 32–35) | Accounting | Terbuka (`FIN-OQ-028`) — menggantikan `FIN-OQ-026` | Hanya menahan aktivasi worker keempat kode itu |
| **Ratifikasi kode 36** `PPN-MASUKAN-RETUR-PEMBELIAN` | Accounting | Terbuka (`FIN-OQ-029`) | Menahan aktivasi worker retur yang membawa PPN |
| **Ratifikasi kode 38 dan 39** beserta penerimaan `Amount = 0` | Accounting | Terbuka (`FIN-OQ-030`, `FIN-OQ-032`) | Menahan penegakan `ACC-DEC-065`. Bila `Amount = 0` tidak diterima kotak masuk Accounting, bentuk penandanya MUST dirancang ulang |
| **Akun debit refund `REFERRED_OUTPATIENT_ADMIN`** | Billing + Accounting | Terbuka (`FIN-OQ-031`, sebelumnya `FIN-OQ-018`) | Refund kategori itu menghasilkan baris intake `ERROR` dan nol kejadian — terlihat, tetapi belum terjurnal |
| **Kode potongan AR untuk `DeductionType = OTHER`** | Accounting | Terbuka (`FIN-OQ-033`) | Potongan berjenis `OTHER` ditolak sampai kodenya ada |
| **Gap pembalikan tender top-up deposit** | **Billing** | Terbuka (`FIN-OQ-034`) — surat evidence ke owner Billing belum dikirim | Saldo deposit dapat kelebihan catat tanpa jejak; kode 37 tidak pernah punya baris untuk dikirim |

---

# Bagian 5.12 — Revisi 14: dimensi kejadian, penanda pembukaan shift, dan saldo negatif

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.7` — status **`draft`** |
| Naik dari | `FIN-INTEGRATION-1.6` (`approved` 29 September 2026) |
| Kontrak Accounting yang berlaku | `ACC-XMOD-0.6` |
| Traceability | `FIN-DEC-111`..`116`, `120`..`122`, `130`, `132`, `137`; `FIN-DES-078`, `083`, `084` |
| Dampak kompatibilitas | **Aditif pada payload**, satu kode baru, satu aturan nilai dicabut |

## 5.12.1 Ruas dimensi baru pada payload

Lima ruas ditambahkan ke `PayloadJson`. Kotak masuk Accounting menerimanya lewat
`AdditionalFields` yang ber-`[JsonExtensionData]`, sehingga **tidak ada perubahan kode** yang
dibutuhkan di sisi Accounting untuk berhenti menolaknya. Yang dibutuhkan adalah **persetujuan
kontraknya** (`FIN-OQ-045`).

| Ruas | Tipe | Terisi pada | Kegunaan bagi Accounting |
|---|---|---|---|
| `CashierShiftId` | `uuid?` | Kejadian penerimaan kasir dan pembaliknya | Meringkas menjadi satu jurnal per shift (`ACC-DEC-062`) |
| `CashierShiftNumber` | `string?` | Sama | Rujukan yang terbaca manusia |
| `PaymentMethodCode` | `string?` | Penerimaan kasir, penerimaan piutang, pembayaran utang supplier | Menentukan akun debit atau kredit: kas atau bank |
| `PaymentMethodAccountId` | `uuid?` | Sama | Rekening sumber atau tujuan dana (`FIN-DEC-137`) |
| `ReversalOfSourceTransactionId` | `string?` | Kejadian pembalik | Menelusuri pembalik ke kejadian aslinya |

**Contoh payload penerimaan kasir sesudah revisi 14** (nilai contoh, bukan data nyata):

```json
{
  "EventNumber": "FIN-EVT-20261002-0001",
  "EventTypeCode": "PENERIMAAN-KASIR",
  "SourceModule": "Finance",
  "SourceTransactionId": "RCP-20261002-0007",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-10-01T19:10:00+00:00",
  "AccountingDate": "2026-10-02",
  "Amount": 150000.00,
  "CashierShiftId": "0f6f1a9c-0000-0000-0000-000000000012",
  "CashierShiftNumber": "SHIFT-20261002-12",
  "PaymentMethodCode": "CASH",
  "PaymentMethodAccountId": null,
  "Components": null
}
```

Perhatikan `EventOccurredAt` pukul 19.10 UTC menghasilkan `AccountingDate` **2 Oktober**, karena
tanggal akuntansi kini dihitung dalam WIB (`FIN-DEC-116`). Sebelum revisi 14 ia akan tercatat
1 Oktober — dan pada pergantian bulan itu berarti periode yang salah.

## 5.12.2 Kuitansi pembalik dan shift-nya

`FIN-DEC-120` menetapkan kuitansi pembalik memakai **shift saat pembalikan terjadi**, bukan shift
kuitansi asli, karena uang fisik keluar dari laci shift itu.

| Ruas pada kejadian pembalik | Isi |
|---|---|
| `CashierShiftId`, `CashierShiftNumber` | Shift **pembalikan** |
| `ReversalOfSourceTransactionId` | Nomor kuitansi **asli** |

Akibat bagi Accounting, dan ini **MUST** dipertimbangkan saat menyusun aturan posting: satu pasangan
penerimaan dan pembaliknya **dapat tersebar di dua shift**, bahkan dua periode. Penelusuran pasangan
karena itu memakai `ReversalOfSourceTransactionId`, **bukan** nomor shift.

## 5.12.3 Kode baru: `PEMBUKAAN-SHIFT-KASIR`

| Field | Nilai |
|---|---|
| Kode | `PEMBUKAAN-SHIFT-KASIR` |
| Sifat | **Penanda status**, bukan transaksi. `Amount = 0`, nol lawan jurnal, nol aturan posting |
| Pemicu | Finance pertama kali melihat shift berstatus **belum final** — seluruh status selain `CLOSED` dan `REVIEWED` (`FIN-DEC-121`) |
| `SourceTransactionId` | Nomor shift |
| `SourceVersion` | Nomor siklus, sama polanya dengan kedua penanda yang sudah ada |
| `AccountingDate` | Tanggal shift, dalam WIB |
| Yang diminta ke Accounting | **Ratifikasi nama kode** dan **penambahan ke daftar tertutup nilai nol** di kotak masuk Accounting — `FIN-OQ-047` |

**Kenapa kode ini diminta.** Accounting menyatakan `CLOSED_WITH_VARIANCE` dan `PERLU_TINDAK_LANJUT`
"tetap menahan tutup bulan" (`evidence/16` bagian 2 butir 2). Tetapi Accounting hanya dapat menahan
sesuatu yang ia **ketahui**: shift yang dibuka lalu berakhir `CLOSED_WITH_VARIANCE` di antara dua
sinkronisasi tidak pernah terlihat sebagai terbuka. Penanda pembukaan menutup celah itu — ia
menjawab pertanyaan 16.5 Accounting dengan bentuk yang sama seperti dua penanda yang sudah
disepakati, bukan bentuk baru.

**Pasangan yang terbentuk per siklus shift:**

| Penanda | Terbit saat |
|---|---|
| `PEMBUKAAN-SHIFT-KASIR` | Shift terlihat belum final |
| `PENUTUPAN-SHIFT-KASIR` | Shift mencapai `CLOSED` atau `REVIEWED` |
| `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Shift yang sudah tertutup dibuka kembali |

Accounting menahan tutup bulan selama ada `PEMBUKAAN-` tanpa `PENUTUPAN-` pada siklus yang sama.

## 5.12.4 Saldo subledger: nilai negatif dan jumlah baris

Dua aturan berubah, keduanya menjawab pertanyaan Accounting 16.2 dan 16.3.

| Aturan | Sebelum | Sesudah | Dasar |
|---|---|---|---|
| Nilai negatif pada `SALDO-SUBLEDGER` | Ditolak Finance **tanpa pengecualian** (`FIN-DEC-091`, `evidence/21` butir 15.2) | **Diterima**; nilai negatif berarti berlawanan dengan saldo normal akun | `FIN-DEC-112` — mengamandemen `FIN-DEC-091` |
| Jumlah baris per periode | **Tepat empat**, satu per kelompok | **Sebanyak pemetaan akun control aktif**, satu baris per akun | `FIN-DEC-113` |

> **Koreksi atas `evidence/21`.** Surat itu menyatakan nominal negatif "ditolak tanpa pengecualian".
> Pernyataan itu **tidak lagi berlaku** dan **MUST** diluruskan dalam surat berikutnya. Kontrak
> Accounting sendiri sudah mengizinkan negatif untuk akun yang saldonya sedang tidak wajar, dan
> Finance kini mengikutinya.
>
> **Satu catatan kejujuran.** Pemeriksaan source menemukan skenario yang semula dipakai sebagai
> contoh — piutang lebih bayar menjadi negatif — **tidak dapat terjadi**: `CK_FinReceivable_Outstanding`
> menjaganya tetap `>= 0`, dan alokasi melebihi sisa ditolak. Kandidat saldo negatif yang diketahui
> tinggal **Kas Kasir**. Aturannya tetap diambil karena ia benar secara prinsip, bukan karena
> contohnya terbukti.

### Kelompok saldo dan segmennya

| Kelompok | Segmen | Sumber nilai |
|---|---|---|
| `KAS-KASIR`, `KAS-KECIL` | Tanpa segmen | — |
| `PIUTANG` | Tanpa segmen, atau `PAYER` / `PATIENT_GUARANTOR` / `EMPLOYEE_BENEFIT` | Jenis debitur yang sudah berjalan |
| `UTANG-SUPPLIER` | Tanpa segmen | Belum ada sumbu pemecah |
| `UTANG-JASA-MEDIS` | Tanpa segmen, atau `DOCTOR` / `NURSE` / `OTHER_PRACTITIONER` | Jenis penerima jasa |

**Utang jasa medis dikirim walaupun bernilai nol** (`FIN-DEC-122`), mengikuti aturan Accounting
bahwa setiap akun control dikirim termasuk yang bersaldo `0.00`. Tabelnya belum punya penulis apa
pun, jadi angkanya memang nol — bukan karena tidak dihitung.

**Gagal tertutup.** Bila pemetaan tidak lengkap, Finance menerbitkan **nol** baris untuk periode itu
dan menahan snapshot, bukan mengirim sebagian. Accounting akan melihat periode yang belum menerima
saldo sama sekali — keadaan yang jelas — bukan periode yang menerima saldo kurang tanpa tanda.

## 5.12.5 Pernyataan ulang saldo

`FIN-DEC-114` menjanjikan pernyataan ulang bila posisi periode berubah sesudah snapshot terbit.
Mekanismenya memakai `SourceVersion` yang **sudah ada**, tanpa penambahan kontrak:

| Hal | Perilaku |
|---|---|
| Pemicu | Mutasi baru bertanggal di dalam periode yang snapshot-nya sudah terbit — misalnya shift malam yang baru ditutup |
| Cakupan | **Hanya** akun yang nilainya berubah. Akun yang sama nilainya tidak diterbitkan ulang |
| Identitas | `SourceTransactionId` tetap sama; `SourceVersion` naik |
| Jadwal | Diperiksa penjadwal harian pukul 00.05 WIB |

## 5.12.6 Jalur pengiriman dan apa yang masih tertahan

`FIN-DEC-118` membuka kembali pembangunan worker pengiriman. Keadaan jujurnya:

| Hal | Keadaan |
|---|---|
| Worker pengiriman | **Dibangun**, tetapi **mati secara bawaan**. Menyalakannya menuntut konfigurasi eksplisit |
| Kredensial akun layanan (G3) | **Belum diputuskan** bersama Platform dan Accounting. Worker mengambilnya dari konfigurasi dan **MUST NOT** menanamkannya di source |
| Penanda shift (ketiga kode) | **Tetap dilewati** worker sampai Accounting menyatakan G6 siap, mempertahankan `FIN-DES-059` |
| Kode yang belum diratifikasi | Dilewati dengan alasan yang sama |
| Balasan `200` dan `201` | Keduanya **sukses**; `AccountingReceiptNumber` diisi dari `AccountingEventId`. Sudah dikonfirmasi Accounting pada `evidence/16` bagian 5 |
| Status `Gagal` dan `Diabaikan` | Diterima sebagai status tanda terima yang sah, bukan galat jaringan (`FIN-DEC-093`) |

> **Janji yang MUST diluruskan.** `evidence/15` dan `21` menyatakan G4 siap dan snapshot terbit
> otomatis. Pada tanggal surat itu ditulis, **nol** jalur pengiriman ada di source. Surat berikutnya
> **MUST** menyatakan G4 **bersyarat**: siap sesudah ketiga hosted service terbukti jalan dan
> kredensial G3 turun.

## 5.12.7 Yang sengaja tidak diminta ke Accounting

| Yang tidak diminta | Alasan |
|---|---|
| Jalur baca saldo awal Accounting | Akan memudahkan rekonsiliasi batch migrasi, tetapi ia kontrak baru. Dicatat `FIN-OQ-078` untuk kemudian hari; rilis ini memakai angka yang dinyatakan petugas |
| Akun debit untuk refund `REFERRED_OUTPATIENT_ADMIN` | Sudah menjadi syarat G6 milik Accounting bersama Billing; tidak dibuka ulang |
| Kode kejadian untuk pendapatan sewa | Tetap `FIN-OQ-044(b)`, di luar cakupan revisi 14 |
| Kode kejadian untuk item migrasi | **Tidak ada dan tidak diminta.** `FIN-DEC-129` melarang migrasi menerbitkan kejadian; nilainya sudah tercakup saldo awal Accounting |

---

# AMENDMENT REVISI 17 — Piutang Manfaat Karyawan: dua permukaan yang sengaja BELUM dikontrakkan

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.7` — **tidak bergerak** |
| `last_changed_in` | Tidak berubah. Bagian ini **tidak** menambah, mengubah, atau mencabut satu pun kontrak integrasi |
| Status | Mengikuti status revisi sebelumnya |
| Tanggal catatan | 5 Oktober 2026 |
| Mengapa ditulis di sini | Pembaca kontrak integrasi **MUST** tahu bahwa dua permukaan di bawah sudah diputuskan arahnya tetapi **belum** punya kontrak, supaya tidak ada yang menyangka keduanya sudah tersedia atau mulai membangunnya sendiri |

Angka versi **tidak** dinaikkan. Bagian ini catatan keadaan, bukan kontrak baru.

## O.1 HR → Finance: hasil potongan gaji

| Hal | Keadaan |
|---|---|
| Arah | HR mengirim hasil potongan gaji ke Finance, **otomatis** |
| Keputusan yang mendasarinya | `FIN-DEC-167` (arah dan sifat otomatis) dan `FIN-DEC-179` (HR menegakkan batas, Finance menerima apa adanya) |
| Yang **sudah** diputuskan Finance | Hasil potongan punya **tiga** keadaan: penuh, **sebagian**, dan gagal. Kiriman ganda **MUST** aman — tidak boleh mengurangi piutang dua kali (`FIN-VAL-240`). Perpindahan status angsuran yang akan dihasilkannya sudah ditetapkan pada `state-transition-matrix.md` bagian O.2 |
| Yang **belum** ada | Bentuk pesannya, waktu kirimnya, cara menandai kiriman ulang, dan kesanggupan HR membangunnya |
| Pemblokir | `FIN-OQ-091`, pemilik HR/Payroll |
| Status | **`OPEN DECISION`.** Nol kontrak, nol endpoint, nol payload yang boleh diasumsikan |
| Yang MUST NOT dilakukan sementara ini | Finance **MUST NOT** membaca tabel potongan gaji HR secara langsung. Itu melanggar batas modul, dan akan pecah setiap kali penggajian berubah — alasan yang sama dengan penolakan membaca `BilTender` pada `evidence/02` |

## O.2 Finance → Accounting: jurnal beban manfaat karyawan

| Hal | Keadaan |
|---|---|
| Arah | Finance menitipkan satu kejadian berkala ke outbox Accounting ketika pelunasan internal diterbitkan |
| Keputusan yang mendasarinya | `FIN-DEC-178` — piutang "RS Benefit" ditutup berkala lewat pelunasan internal, lawan jurnalnya beban manfaat karyawan |
| Yang **sudah** diputuskan Finance | Penutupan bersifat **berkala**, satu kejadian per periode per penjamin internal, **bukan** satu kejadian per tagihan. Penutupan **MUST NOT** memakai penghapusan buku |
| Yang **belum** ada | Nama jenis kejadiannya, akun yang didebet dan dikredit, serta kapan ia dikirim |
| Pemblokir | `FIN-OQ-103`, pemilik Accounting bersama pemilik Finance |
| Status | **`OPEN DECISION`** |
| Perilaku sementara yang sudah dirancang | Kolom `AccountingEventId` pada pelunasan internal **tetap kosong** sampai kontraknya ada, dan itu **bukan** cacat. Penutupan piutang di sisi Finance **tetap berjalan** tanpanya — yang belum berjalan hanyalah pembukuan jurnalnya di Accounting |

## O.3 Yang TIDAK berubah pada kontrak integrasi yang berjalan

| Permukaan | Dampak amandemen ini |
|---|---|
| Serah terima penerimaan dari kasir | Nol |
| Outbox kejadian ke Accounting yang sudah berjalan | Nol. Jenis kejadian baru untuk pelunasan internal menunggu `FIN-OQ-103` |
| Pembacaan master penjamin milik Administrator | Nol. Dirujuk lewat Id saja, tanpa salinan |

---

# AMENDMENT REVISI 18 — Piutang Manfaat Karyawan: Kontrak Integrasi Resmi Lintas Domain (Billing, HR, dan Registrasi)

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.8` |
| `last_changed_in` | Revisi 18 (6 Oktober 2026) |
| Status | `draft` (menunggu persetujuan pemilik) |
| Dasar keputusan | `00-interview-decisions.md` (`FIN-DEC-183` s.d. `FIN-DEC-202`) |
| Sifat | **Resmi & Mengikat.** Menutup catatan keadaan Bagian O sebelumnya dengan kontrak antarmodul konkret |

---

### P.1 INT-FIN-BIL-001 — Serah Terima Piutang Karyawan Dua Baris (`Billing → Finance`)

**Tujuan:** Billing menyerahterimakan tagihan layanan RS atas pegawai/keluarga ke Finance dalam dua porsi terpisah saat invoice difinalisasi (`FIN-DEC-183`, `FIN-DEC-184`, `FIN-DEC-185`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | Finalisasi Invoice (`BilInvoice`) dengan sumber pembayaran Penjamin Internal RS (`MstCompanyGuarantor`, `FIN-DEC-197`) |
| Entitas Media | `BilArHandoff` (tabel di schema `public` milik Billing) |
| Jumlah Baris | Tepat **DUA baris** per invoice |
| Unik Key | `(InvoiceId, DebtorType)` |

#### Skema Baris 1: Porsi Penjamin RS Benefit (`PAYER`)
- `InvoiceId`: Guid invoice.
- `DebtorType`: `"PAYER"`.
- `DebtorReferenceId`: Guid `CompanyGuarantorId` milik Rumah Sakit ("RS Benefit").
- `Amount`: Nominal yang ditanggung plafon benefit RS (dihitung oleh Billing berdasarkan sisa limit HR, `FIN-DEC-185`, `FIN-DEC-189`).
- `BenefitOwnerId`: `NULL`.
- `BenefitRelationship`: `NULL`.
- `HandoffStatus`: `"PENDING"`.

#### Skema Baris 2: Porsi Tanggungan Pegawai (`EMPLOYEE_BENEFIT`)
- `InvoiceId`: Guid invoice.
- `DebtorType`: `"EMPLOYEE_BENEFIT"` (konstanta baru `BillingArDebtorTypes.EMPLOYEE_BENEFIT`).
- `DebtorReferenceId`: `NULL`.
- `Amount`: Kelebihan tagihan di atas plafon benefit yang wajib dibayar pegawai (`FIN-DEC-164`, `FIN-DEC-185`).
- `BenefitOwnerId`: Guid `EmployeeId` / `WorkforceProfileId` pegawai yang sah dari HR (wajib terisi, `FIN-DEC-199`).
- `BenefitRelationship`: Kode hubungan keluarga (`"SELF"`, `"SPOUSE"`, `"CHILD"`, `"PARENT"`, `"OTHER"`) dari snapshot kunjungan (`FIN-DEC-198`).
- `HandoffStatus`: `"PENDING"`.

---

### P.2 INT-FIN-HR-001 — Penerbitan Jadwal Cicilan ke Input Variabel Penggajian (`Finance → HR`)

**Tujuan:** Finance mengirim jadwal pemotongan angsuran piutang yang telah disetujui berjenjang ke modul Payroll HR (`FIN-DEC-190`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | Persetujuan Perjanjian Cicilan Angsuran (`POST /receivable-installment-plans/{id}/approve`) |
| Entitas Media | `TrxPayrollVariableInput` (tabel di schema `public` milik HR Payroll) |
| Waktu Tulis | Ditulis otomatis di dalam transaksi persetujuan perjanjian |

#### Pemetaan Nilai Kolom:
- `Id`: `Guid.NewGuid()`.
- `WorkforceProfileId`: `FinReceivableInstallmentPlan.WorkforceProfileId`.
- `PayrollPeriodId`: Target `MstPayrollPeriod.Id` pada baris angsuran (`FinReceivableInstallment.TargetPayrollPeriodId`).
- `ComponentId`: Guid `MstPayrollComponent` bertipe potongan piutang RS (`FIN-DEC-190`).
- `Amount`: Nominal yang wajib dipotong (`ScheduledAmount + CarriedOverAmount`).
- `SourceType`: `"FinanceReceivableInstallment"`.
- `SourceId`: Guid `FinReceivableInstallment.Id`.
- `Notes`: `"Cicilan Piutang Layanan RS No. {ReceivableNumber} (Angsuran ke-{InstallmentOrder})"`
- `IsActive`: `true`.

---

### P.3 INT-HR-FIN-001 — Pengiriman Hasil Pemotongan Penggajian Otomatis (`HR → Finance`)

**Tujuan:** HR Payroll melaporkan realisasi hasil pemotongan gaji secara otomatis ke Finance segera setelah status *Payroll Run* disahkan (`FIN-DEC-191`, `FIN-DEC-192`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | Pengesahan/Penyelesaian Payroll Run periode penggajian di modul HR |
| Protokol | HTTP POST internal / in-process event handler |
| Endpoint Target | `POST /api/v1/finance/receivables/installments/payroll-results` |
| Autentikasi | Service-to-Service Token / System Worker Identity |
| Idempotensi | **Wajib idempoten** berbasis `(InstallmentId, PayrollPeriodId)` (`FIN-VAL-240`) |

#### Spesifikasi Payload Request:
```json
{
  "installmentId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "payrollPeriodId": "7ca24a12-6821-4982-a1bb-3f821c99ada1",
  "status": "BERHASIL", 
  "deductedAmount": 1000000.00,
  "executionTimestamp": "2026-10-25T17:00:00+07:00",
  "payrollRunId": "9ea14b55-8812-4211-b021-1f928a44bcb2",
  "notes": "Potongan reguler slip gaji periode Oktober 2026"
}
```
*Daftar Nilai Status yang Diterima:*
- `"BERHASIL"`: Dipotong penuh sesuai jadwal.
- `"SEBAGIAN"`: Dipotong sebagian karena perlindungan batas take-home pay minimum (`FIN-DEC-192`). Sisa tunggakan dialihkan ke angsuran berikutnya.
- `"GAGAL"`: Gagal dipotong (contoh: gaji tidak mencukupi / unpaid leave). Seluruh nominal dialihkan ke angsuran berikutnya.

#### Spesifikasi Response:
- `200 OK`: Sukses diproses atau duplikat kiriman yang diabaikan secara aman (idempoten).
- `404 Not Found`: `InstallmentId` tidak ditemukan di Finance.
- `422 Unprocessable Entity`: Status angsuran sudah dibatalkan atau bukan dalam status menunggu pembayaran.

---

### P.4 INT-HR-FIN-002 — Pemeriksaan Bebas Tanggungan Exit Clearance (`HR ↔ Finance`)

**Tujuan:** Memvalidasi status bebas piutang pegawai sebelum administrasi berhenti kerja disahkan di HR (`FIN-DEC-170`, `FIN-DEC-193`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | Validasi pengajuan/penutupan formulir `TrxExitClearance` di HR |
| Endpoint Target | `GET /api/v1/finance/receivables/clearance/{benefitOwnerId}` |
| Izin Akses | `FinanceReceivable : Read` / Inter-module Internal Access |

#### Spesifikasi Response:
```json
{
  "benefitOwnerId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "isCleared": false,
  "outstandingAmount": 1500000.00,
  "activeReceivableCount": 1,
  "overdueInstallmentCount": 1,
  "blockingReceivables": [
    {
      "receivableId": "4ca12a88-2191-4aa2-8172-2a912bb81201",
      "receivableNumber": "AR-EMP-202610-0012",
      "outstandingAmount": 1500000.00,
      "status": "PARTIALLY_PAID"
    }
  ]
}
```
*Aturan Penegakan di HR:*
- Jika `isCleared == true`: Kolom `TrxExitClearance.IsFinanceCleared` diset `true`, proses offboarding dapat diselesaikan.
- Jika `isCleared == false`: Kolom diset `false`, tombol simpan/selesaikan di HR terkunci.

---

### P.5 INT-HR-FIN-003 — Notifikasi Pemisahan Pegawai (`HR → Finance`)

**Tujuan:** Memberitahu Finance ketika pegawai mengajukan resign/PHK agar Finance dapat memetakan sisa kewajiban dan memotongnya dari hak akhir (`FIN-DEC-169`, `FIN-DEC-194`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | Pembuatan transaksi pemisahan pegawai `TrxEmployeeSeparation` di HR |
| Endpoint Target | `POST /api/v1/finance/receivables/separation-notice` |
| Payload | `{ "employeeId": "uuid", "separationDate": "2026-11-30", "reason": "Resign", "separationId": "uuid" }` |
| Tindakan di Finance | Menandai kartu piutang pegawai bersangkutan sebagai `SEPARATION_PENDING`. |

---

### P.6 INT-BIL-FIN-002 — Koreksi Pemilik Manfaat Salah Orang (`Billing ↔ Finance ↔ HR`)

**Tujuan:** Membatalkan piutang yang keliru dibebankan ke pegawai lain tanpa manipulasi langsung pada data historis (`FIN-DEC-173`, `FIN-DEC-187`, `FIN-DEC-195`, `FIN-DEC-200`).

1. **Inisiasi di Billing**:
   - Billing menerbitkan serah terima pembalik `BilHandoffAdjustment`:
     - `AdjustmentType = "REVERSAL"`
     - `SourceHandoffId = BilArHandoff_Lama.Id`
     - `AdjustedAmount = -Amount`
     - `Reason = "Koreksi pemilik manfaat salah orang"`
   - Billing menerbitkan `BilArHandoff` baru membawa `BenefitOwnerId` pegawai yang benar.
2. **Penanganan di Finance**:
   - `FinanceReceivableIntakeService` membatalkan kartu piutang lama (`Status = CANCELLED`, menulis mutasi pembatalan).
   - Menerbitkan kartu piutang baru atas pegawai yang benar.
3. **Restitusi Salah Potong di HR (`FIN-DEC-195`)**:
   - Bila cicilan atas pegawai yang salah sempat terpotong pada penggajian sebelumnya, Finance mengirim event pembalikan angsuran ke HR.
   - HR menyisipkan penyesuaian gaji net (*reimbursement adjustment*) pada slip gaji pegawai yang salah di periode penggajian berikutnya. Finance tidak mengeluarkan uang kas manual.


---

# AMENDMENT REVISI 19 — Piutang Manfaat Karyawan: Kontrak Plafon Benefit dan Verifikasi Tanggungan (`FIN-INTEGRATION-1.9`)

```yaml
contract_version: FIN-INTEGRATION-1.9
last_changed_in: Revisi 19 (13 Oktober 2026)
status: approved
approved_by: Pemilik Finance dan HR (pengguna sesi); konfirmasi pemilik Billing dan Registrasi belum turun
approved_at: 2026-10-13
owner: Pemilik Finance (Product/Domain), bersama pemilik HR, Billing, dan Registrasi
input_revision: 18
input_hash: eb29c5dbe78097fc   # SHA-256 (16 karakter pertama) berkas ini sebelum amandemen Revisi 19
input_decisions: FIN-DEC-183..205 (sudah ada); FIN-DEC-206..210 (baru, 13 Oktober 2026)
compatibility_impact: lihat bagian R.7 — satu koreksi atas INT-FIN-BIL-001 (§P.1); selebihnya tambahan
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN
domain_architecture_readiness_reason: >
  hospital-domain-architect tidak dijalankan. Batas domain (Billing, HR, Registrasi, Finance), kepemilikan data,
  dan dampak billing sudah diputuskan oleh pemilik masing-masing pada Revisi 18 (FIN-DEC-183..202) dan
  dipertegas pada sesi 13 Oktober 2026 (FIN-DEC-204..210). Tidak ada keputusan klinis di rumpun ini.
```

| Field | Nilai |
|---|---|
| Sifat | Kontrak **approved sisi pemilik Finance dan HR** (13 Oktober 2026, `FIN-DEC-212`). Menutup `FIN-OQ-105`. **Konfirmasi pemilik Billing dan Registrasi belum turun**; perilaku pada sisi mereka tetap menunggu konfirmasi itu |
| Yang baru | `INT-REG-HR-001` (Registrasi → HR) dan `INT-BIL-HR-001` (Billing → HR) |
| Yang dikoreksi | `INT-FIN-BIL-001` (§`P.1`) — jumlah baris, identitas pemilik manfaat, status awal, panjang kolom (bagian `R.4`) |
| Yang tidak berubah | `INT-FIN-HR-001`, `INT-HR-FIN-001`, `INT-HR-FIN-002`, `INT-HR-FIN-003`, `INT-BIL-FIN-002` (§`P.2`..`P.6`) |

---

## R.0 Peta kontrak

| Kontrak | Arah | Media | Pemicu | Status |
|---|---|---|---|---|
| `INT-REG-HR-001` | Registrasi → HR (baca) | Layanan HR dipanggil dalam proses yang sama | Petugas mencari pegawai, memilih keluarga, menerbitkan kartu, mendaftarkan kunjungan | **Baru** |
| `INT-BIL-HR-001` | Billing → HR (baca, pakai, kembalikan) | Layanan HR dipanggil di dalam transaksi finalisasi Billing | Pratinjau dan finalisasi tagihan; koreksi atau pembatalan tagihan | **Baru** |
| `INT-FIN-BIL-001` | Billing → Finance | Tabel serah terima `BilArHandoff` | Finalisasi tagihan | **Dikoreksi** (`R.4`) |

**Mengapa layanan dalam proses yang sama, bukan HTTP.** Seluruh modul berjalan dalam satu aplikasi dan satu
`ApplicationDbContext`. Pola yang sudah dipakai: `BillingArApHandoffService.StageHandoffsForFinalizationAsync`
menambahkan baris ke context yang sama, sedangkan `BillingFinalizationService` memegang batas transaksi dan
`SaveChanges`. Layanan HR di bawah ini mengikuti pola itu: ia **tidak membuka transaksi sendiri** pada operasi
`pakai` dan `kembalikan`. Akibatnya, pengurangan plafon dan penerbitan serah terima **berhasil bersama atau gagal bersama**.

---

## R.1 Istilah dan identitas

| Istilah | Arti | Contoh (data samaran) |
|---|---|---|
| Pemilik manfaat | Pegawai yang menanggung. Identitasnya `MstEmployee.Id`. Nama kolomnya `BenefitOwnerId` | Pegawai "Budi" |
| Orang yang ditanggung | Pegawai sendiri **atau** satu tanggungan. Saldo plafon melekat pada orang ini (`FIN-DEC-207`) | Budi, atau anaknya "Sari" |
| Jenis orang | `ENROLLMENT` (pegawai sendiri; saldo di `TrxEmployeeBenefitEnrollment`) atau `DEPENDENT` (tanggungan; saldo di `TrxEmployeeBenefitDependent`) | Budi = `ENROLLMENT`; Sari = `DEPENDENT` |
| Enrollment layanan RS | Enrollment berstatus `Active` milik pegawai pada plan yang jenis benefitnya `BenefitCategory = 'Medical'` dan `IsClaimBased = false` (usulan baseline, `FIN-OQ-109`) | Plan "Benefit Medis Pegawai RS" |
| Kode hubungan baku | `SELF`, `SPOUSE`, `CHILD`, `PARENT`, `OTHER` | — |

### R.1.1 Pemetaan teks hubungan HR ke kode baku (usulan baseline, `FIN-OQ-109`)

`WfpFamilyMember.Relationship` adalah teks bebas (maksimum 100 karakter). Pilihan pada layar HR saat ini:
`Spouse`, `Child`, `Father`, `Mother`, `Sibling`, `Guardian`, `Other`.

| Teks di HR | Kode baku |
|---|---|
| (pegawai sendiri; tidak ada baris keluarga) | `SELF` |
| `Spouse` | `SPOUSE` |
| `Child` | `CHILD` |
| `Father`, `Mother` | `PARENT` |
| `Sibling`, `Guardian`, `Other`, dan setiap nilai lain yang tidak dikenal | `OTHER` |

### R.1.2 Hubungan yang boleh ditanggung menurut `CoverageType` plan (`FIN-DEC-207`)

| `CoverageType` | `SELF` | `SPOUSE` | `CHILD` | `PARENT` | `OTHER` |
|---|:---:|:---:|:---:|:---:|:---:|
| `Individual` | Ya | Tidak | Tidak | Tidak | Tidak |
| `EmployeeAndSpouse` | Ya | Ya | Tidak | Tidak | Tidak |
| `EmployeeAndChildren` | Ya | Tidak | Ya | Tidak | Tidak |
| `Family` | Ya | Ya | Ya | Ya | Ya |

`Family` membolehkan semua hubungan, **tetapi tetap hanya orang yang sudah diverifikasi HR** (`FIN-DEC-201`, `FIN-DEC-204`).
Jumlah tanggungan aktif tidak boleh melebihi `MaximumDependents` pada plan.

---

## R.2 `INT-REG-HR-001` — Pencarian Pegawai dan Verifikasi Tanggungan (`Registrasi → HR`)

**Tujuan.** Petugas Registrasi memilih pegawai dari data HR (bukan mengetik bebas), memilih anggota keluarga, dan
langsung tahu apakah orang itu **sudah terdaftar** sebagai tanggungan benefit (`FIN-DEC-199`, `FIN-DEC-204`).

| Properti | Spesifikasi |
|---|---|
| Pemicu | (1) kartu penjamin internal dibuat atau diubah; (2) kunjungan didaftarkan memakai kartu itu |
| Sifat | **Hanya membaca.** Tidak mengubah data HR |
| Idempotensi | Tidak berlaku (tanpa efek samping) |
| Data saldo | **Tidak pernah dikembalikan ke Registrasi.** Registrasi hanya tahu layak atau tidak (`FIN-DEC-202`) |
| Kegagalan | Bila HR tidak dapat dibaca, kartu **tidak diterbitkan**; petugas diberi pesan dan jalur Pasien Umum |

### R.2.1 Operasi `CariPegawai`

| Masukan | Aturan |
|---|---|
| `keyword` | Wajib, **minimal 3 karakter**; dicocokkan ke NIP, NIK, dan nama |
| `maxResults` | Maksimum 20; bila lebih, petugas diminta mengetik lebih spesifik |

| Keluaran per baris | Keterangan |
|---|---|
| `employeeId` | `MstEmployee.Id` |
| `employeeNumber` | NIP |
| `nationalIdMasked` | NIK **tersamar**: empat digit pertama dan empat digit terakhir, sisanya `•` (usulan privasi; pemilik HR boleh melonggarkan) |
| `fullName` | Nama pegawai |
| `organizationUnitName` | Unit kerja |
| `hasActiveMedicalEnrollment` | Ya/Tidak, agar petugas langsung tahu pegawai punya benefit aktif atau tidak |

Hanya pegawai **aktif** dan tidak terhapus. Tidak ada data lain (gaji, alamat, kontak) yang keluar dari HR.

### R.2.2 Operasi `DaftarKeluarga`

Masukan: `employeeId`. Keluaran per anggota keluarga **aktif**: `familyMemberId`, `fullName`, `relationshipCode`
(`R.1.1`), `ageYears`, `identityMasked`, `dependentVerification` (`VERIFIED`, `REQUESTED`, `NOT_REGISTERED`,
`REJECTED`, `ENDED`), dan `coveredByPlan` (Ya/Tidak menurut `R.1.2`).

### R.2.3 Operasi `PeriksaKelayakan`

Masukan: `employeeId`, `familyMemberId` (kosong bila pegawai sendiri), `serviceDate`.

**Urutan pemeriksaan** (berhenti pada kegagalan pertama):

1. Pegawai ada dan aktif.
2. Tepat **satu** enrollment layanan RS berstatus `Active` yang masa berlakunya mencakup `serviceDate`.
3. Bila pegawai sendiri: lolos (saldo di enrollment).
4. Bila keluarga: anggota keluarga ada, aktif, dan milik **pegawai yang sama**.
5. Hubungan anggota keluarga dibolehkan `CoverageType` plan (`R.1.2`).
6. Ada tanggungan benefit pada enrollment itu dengan status `Verified`, `IsEligible`, `IsCovered`, dan masa berlakunya mencakup `serviceDate`.

| Kode alasan | Arti | Pesan bagi petugas Registrasi | Yang memperbaiki |
|---|---|---|---|
| `OK` | Layak | — | — |
| `EMPLOYEE_NOT_FOUND` | Pegawai tidak ada atau tidak aktif | "Pegawai tidak ditemukan atau sudah tidak aktif di HR." | HR |
| `ENROLLMENT_NOT_ACTIVE` | Tidak ada benefit layanan RS aktif pada tanggal layanan | "Pegawai belum memiliki benefit layanan RS yang aktif. Daftarkan sebagai Pasien Umum atau hubungi HR." | HR |
| `ENROLLMENT_AMBIGUOUS` | Lebih dari satu enrollment layanan RS aktif | "Data benefit pegawai ganda di HR. Hubungi HR sebelum melanjutkan." | HR |
| `FAMILY_MEMBER_NOT_FOUND` | Anggota keluarga tidak ada atau bukan milik pegawai ini | "Anggota keluarga tidak terdaftar pada pegawai ini di HR." | HR |
| `FAMILY_MEMBER_INACTIVE` | Anggota keluarga tidak aktif | "Anggota keluarga ini sudah tidak aktif di HR." | HR |
| `RELATIONSHIP_NOT_COVERED` | `CoverageType` plan tidak mencakup hubungan ini | "Jenis hubungan keluarga ini tidak ditanggung oleh plan benefit pegawai." | HR (ganti plan) |
| `DEPENDENT_NOT_REGISTERED` | Belum ada tanggungan benefit | "Pasien belum terdaftar sebagai tanggungan benefit pegawai. Daftarkan di HR terlebih dahulu atau daftarkan sebagai Pasien Umum." | HR |
| `DEPENDENT_NOT_VERIFIED` | Ada, tetapi belum diverifikasi | "Pendaftaran tanggungan pasien belum diverifikasi HR." | HR |
| `DEPENDENT_NOT_ELIGIBLE` | Ditolak atau tidak layak/ditanggung | "Pasien tidak ditanggung benefit pegawai menurut HR." | HR |
| `DEPENDENT_OUT_OF_PERIOD` | Di luar masa berlaku | "Masa berlaku tanggungan pasien tidak mencakup tanggal layanan." | HR |
| `HR_UNAVAILABLE` | Pembacaan HR gagal | "Data HR belum dapat dibaca. Coba lagi, atau daftarkan sebagai Pasien Umum." | Teknis |

Keluaran saat `OK`: `employeeId`, `familyMemberId`, `relationshipCode`, `benefitPersonKind`, `benefitPersonId`,
`enrollmentId`, `planCode`, `planName`, `coverageType`, `effectiveStartDate`, `effectiveEndDate`. **Tanpa saldo.**

**Aturan kunci: kode hubungan ditentukan sistem, bukan dikirim klien.** Kartu pasien menyimpan
`BenefitRelationship` dari `relationshipCode` hasil pemeriksaan ini. Klien hanya mengirim `employeeId` dan
`familyMemberId`. Dengan begitu petugas tidak bisa memilih "anak" untuk orang yang di HR tercatat "paman".

**Contoh.**

| Kasus | Hasil |
|---|---|
| Sari, anak Budi; terdaftar `Child`, tanggungan `Verified`, plan `Family`, berlaku 1 Jan–31 Des 2026; layanan 13 Okt 2026 | `OK`, `CHILD`, `DEPENDENT` |
| Pak Andi, paman, tidak ada di HR | Tidak bisa dipilih (bukan anggota keluarga Budi); kartu internal tidak diterbitkan |
| Sari terdaftar, tetapi tanggungan masih `REQUESTED` | `DEPENDENT_NOT_VERIFIED`; kartu ditolak |
| Budi sendiri, enrollment berakhir 30 Sep 2026; layanan 13 Okt | `ENROLLMENT_NOT_ACTIVE`; didaftarkan Pasien Umum tunai (`FIN-DEC-209`) |

---

## R.3 `INT-BIL-HR-001` — Baca, Pakai, dan Kembalikan Plafon (`Billing → HR`)

**Tujuan.** Billing mengetahui sisa plafon yang sah saat menghitung dua porsi, mengurangi tepat sekali saat tagihan
final, dan mengembalikannya bila tagihan dikoreksi atau dibatalkan (`FIN-DEC-185`, `FIN-DEC-202`, `FIN-DEC-205`).

| Properti | Spesifikasi |
|---|---|
| Pemilik saldo | HR. Billing tidak pernah menulis langsung ke tabel benefit |
| Pemilik transaksi | Billing (`BillingFinalizationService`) untuk `pakai` dan `kembalikan` |
| Kunci idempotensi | `sourceHandoffKey` = `HandoffKey` baris serah terima `PAYER` pada invoice itu |
| Cakupan saldo | **Per orang** (`FIN-DEC-207`): pegawai sendiri memakai saldo enrollment; tiap tanggungan memakai saldo barisnya sendiri |
| Yang dikurangi | **Hanya porsi `PAYER`** (porsi yang ditanggung RS Benefit). Kelebihan di atas plafon dan porsi pilihan pegawai **tidak** mengurangi plafon |

### R.3.1 Operasi `BacaPlafon`

Masukan: `employeeId`, `familyMemberId` (kosong bila pegawai sendiri), `asOfDate` (tanggal layanan).
Keluaran: `isUsable`, `reasonCode`, `benefitPersonKind`, `benefitPersonId`, `limitAmount`, `usedAmount`,
`remainingAmount`, `planCode`. Bila `isUsable = false`, `reasonCode` memakai daftar pada `R.2.3`
(tanpa `OK`). Pemeriksaan kelayakan yang sama dengan `R.2.3` dijalankan **ulang** di sini karena masa berlaku
bisa berakhir antara pendaftaran dan finalisasi.

### R.3.2 Operasi `PakaiPlafon`

Masukan: `sourceHandoffKey`, `invoiceId`, `benefitPersonKind`, `benefitPersonId`, `amount` (> 0).

| Hasil | Arti | Kode HTTP bila dipetakan ke API | Akibat pada finalisasi |
|---|---|:---:|---|
| `APPLIED` | Saldo berkurang sebesar `amount`; satu baris mutasi `USE` tertulis | `200` | Lanjut |
| `ALREADY_APPLIED` | Kunci sama, nominal sama; **tidak ada perubahan** | `200` | Lanjut (pengulangan aman) |
| `KEY_CONFLICT` | Kunci sama, nominal **berbeda** | `409` | Finalisasi dibatalkan; dilaporkan sebagai galat data |
| `INSUFFICIENT` | `amount` melebihi sisa saat ini (membawa sisa terkini) | `422` | Billing menghitung ulang dari sisa terkini (maksimal satu kali), lalu gagal bila masih tidak cukup |
| `NOT_USABLE` | Orang tidak lagi layak (`R.3.1`) | `422` | Finalisasi **ditahan** (`R.3.4`) |

Pengurangan memakai **satu pernyataan pembaruan bersyarat** yang hanya berhasil bila `RemainingAmount >= amount`,
sehingga dua finalisasi bersamaan untuk orang yang sama tidak pernah membuat saldo negatif.

### R.3.3 Operasi `KembalikanPlafon`

Masukan: `sourceHandoffKey`, `reason`.

| Hasil | Arti |
|---|---|
| `RETURNED` | Saldo bertambah sebesar nominal mutasi `USE` aslinya (selalu **penuh**); satu baris mutasi `RETURN` tertulis |
| `ALREADY_RETURNED` | Sudah pernah dikembalikan; tidak ada perubahan |
| `NOT_FOUND` | Tidak ada mutasi `USE` dengan kunci itu (tidak ada yang dikembalikan) |

Satu mutasi `USE` hanya dapat dikembalikan **satu kali**. Serah terima pengganti (misalnya koreksi salah orang)
membawa `HandoffKey` baru, sehingga pemakaian berikutnya memakai kunci baru dan tidak bentrok.

### R.3.4 Urutan di dalam transaksi finalisasi

| # | Langkah | Bila gagal |
|---:|---|---|
| 1 | Billing memeriksa kunjungan memakai kartu internal yang bertaut ke pegawai | Bukan kunjungan internal: jalur lama, kontrak ini tidak dipakai |
| 2 | `BacaPlafon` | `isUsable = false`: finalisasi **ditahan** dan petugas melihat alasannya; tagihan menunggu konfirmasi manual HR (flowchart `01-alur-piutang-manfaat-karyawan-end-to-end.md` tahap 2). **Tidak** ada pembulatan ke "semua ditanggung RS" atau "semua ditanggung pegawai" |
| 3 | Billing menghitung dua porsi (`R.4.2`) | Penjamin kedua (`ExcessAmount` > 0): ditolak, kombinasi belum didukung |
| 4 | Billing menambahkan baris serah terima ke context (`PAYER` bila porsi RS > 0; `EMPLOYEE_BENEFIT` bila total tanggungan pegawai > 0) | — |
| 5 | `PakaiPlafon` sebesar porsi `PAYER` (dilewati bila porsi RS = 0) | `INSUFFICIENT`: ulangi dari langkah 2 sekali; `KEY_CONFLICT`: batalkan |
| 6 | `SaveChanges` | Gagal: **seluruhnya** dibatalkan; plafon tidak berkurang |

### R.3.5 Contoh

Sisa plafon Sari Rp 3.000.000 (aturan penjamin RS Benefit menanggung 100%).

| Peristiwa | Porsi RS | Porsi pegawai | Sisa plafon |
|---|---:|---:|---:|
| Tagihan A Rp 2.000.000 final | Rp 2.000.000 | Rp 0 | Rp 1.000.000 |
| Tagihan B Rp 2.500.000 final | Rp 1.000.000 | Rp 1.500.000 | Rp 0 |
| Tagihan B difinalisasi ulang (jaringan putus) | tetap | tetap | Rp 0 |
| Tagihan A dikoreksi, `KembalikanPlafon` | — | — | Rp 2.000.000 |
| Tagihan C Rp 1.000.000 final (setelah A kembali) | Rp 1.000.000 | Rp 0 | Rp 1.000.000 |

---

## R.4 Koreksi atas `INT-FIN-BIL-001` (`Billing → Finance`, menggantikan §`P.1`)

### R.4.1 Hal yang dikoreksi

| # | §`P.1` semula | Menjadi | Dasar |
|---:|---|---|---|
| 1 | "Tepat DUA baris per invoice" | **Satu atau dua baris** (`PAYER` dan/atau `EMPLOYEE_BENEFIT`), tidak pernah baris bernilai nol. Baris `PATIENT_GUARANTOR` untuk pasien pulang tanpa bayar tetap mungkin, sehingga paling banyak tiga baris | `CK_BilArHandoff_Amount`: `Amount > 0` (`BilArHandoffConfiguration.cs:19`); `FIN-DEC-210` |
| 2 | `BenefitOwnerId` = `EmployeeId` / `WorkforceProfileId` | `BenefitOwnerId` = **`MstEmployee.Id`** | Seluruh kode Finance memakainya demikian (`FinanceReceivableMigrationService.cs:198`, `FinanceReceivablePayrollSyncService.cs:78-82`, `FinanceSettlementService.cs:187`) |
| 3 | `HandoffStatus = "PENDING"` | Status awal `Created` (`BillingHandoffStatuses.Created`) | `BillingArApHandoffService.cs:54,91` |
| 4 | `BenefitRelationship` 50 karakter di Billing, 30 di Finance | **50 karakter di kedua sisi** (kolom Finance diperlebar; tidak merusak data) | `BilArHandoff.cs:29`, `FinReceivable.cs:52` |
| 5 | Nominal baris pegawai = "kelebihan di atas plafon" | Nominal = kelebihan di atas plafon **ditambah** porsi yang dipilih pegawai untuk dijadikan utang (`R.4.3`) | `FIN-DEC-208` |

### R.4.2 Rumus pembagian

Notasi: `C` = porsi yang ditanggung penjamin RS Benefit menurut aturan penjamin yang sudah ada
(`BillingCoverageDecision.PrimaryAmount`); `S` = sisa plafon hasil `BacaPlafon`; `K` = sisa tagihan pasien di luar `C`
(co-pay dan item di luar cakupan, yang belum dibayar); `D` = nominal pilihan pegawai (`R.4.3`), `0 ≤ D ≤ K`.

| Besaran | Rumus | Menjadi |
|---|---|---|
| Porsi RS Benefit `P` | `min(C, S)` | Baris `PAYER`, `DebtorReferenceId` = penjamin "RS Benefit" (hanya jika `FinancingChoice = EMPLOYEE_BENEFIT`) |
| Kelebihan di atas plafon `E` | `C − P` | Masuk ke total kewajiban pasien `(E + K)`; dapat dibayar langsung di kasir atau ditangguhkan dengan persetujuan |
| Sisa Pasien / Co-Pay `K` | tagihan di luar cakupan aturan penjamin | Masuk ke total kewajiban pasien `(E + K)` |
| Pilihan Penangguhan Pegawai `D` | dipilih pegawai/pasien (`0 ≤ D ≤ E + K`) | Baris `EMPLOYEE_BENEFIT`; **hanya diterbitkan bila `D > 0`** |
| Dibayar langsung di kasir (Direct Pay) | `(E + K) − D` | Settlement kasir (Cash / Transfer / QRIS / EDC); **tidak menghasilkan AR** |
| Pengurangan plafon HR | `P` | `PakaiPlafon` (`R.3.2`); **tidak berkurang bila memilih `GENERAL_SELF_PAY`** |

> **Ketentuan Baru Pembiayaan & AR Karyawan (Section 2, 7, 17):**
> 1. **Pilihan Eksplisit (`FinancingChoice`)**: Pasien (pegawai atau keluarga eligible) wajib memilih antara `EMPLOYEE_BENEFIT` atau `GENERAL_SELF_PAY`. Benefit tidak pernah otomatis dikonsumsi. Jika memilih `GENERAL_SELF_PAY`, plafon HR tidak dipakai (`P = 0`), seluruh tagihan dibayar mandiri, dan tidak ada AR Karyawan.
> 2. **Kelebihan Plafon (`E`) Tidak Otomatis Menjadi AR**: Kelebihan di atas plafon `E = C - P` dapat dibayar langsung secara tunai/transfer di kasir, ditangguhkan seluruhnya menjadi AR Karyawan, atau dipecah sebagian dengan persetujuan pegawai.
> 3. **Penerbitan Baris Handoff**: Baris `PAYER` diterbitkan hanya bila `P > 0`. Baris `EMPLOYEE_BENEFIT` diterbitkan hanya bila `D > 0` (sebesar nominal ditangguhkan). Jika tagihan diselesaikan penuh secara langsung (`D = 0`), tidak ada baris AR Karyawan yang dibentuk.

| Contoh Skenario Pelayanan Rp 2.000.000 | `C` | `S` | `P` | `E` | `K` | `D` (Deferred) | `PAYER` | `EMPLOYEE_BENEFIT` | Bayar Langsung (Kasir) |
|---|---:|---:|---:|---:|---:|---:|---:|---:|---:|
| A. Bayar tunai penuh (Self-Pay) | 2.000.000 | 1.500.000 | 0 | 0 | 2.000.000 | 0 | tidak terbit | tidak terbit | 2.000.000 |
| B. Benefit Rp 1.5jt, sisa bayar langsung | 2.000.000 | 1.500.000 | 1.500.000 | 500.000 | 0 | 0 | 1.500.000 | tidak terbit | 500.000 |
| C. Benefit Rp 1.5jt, sisa ditangguhkan | 2.000.000 | 1.500.000 | 1.500.000 | 500.000 | 0 | 500.000 | 1.500.000 | 500.000 | 0 |
| D. Benefit Rp 1.5jt, bayar Rp 200rb, tangguhkan Rp 300rb | 2.000.000 | 1.500.000 | 1.500.000 | 500.000 | 0 | 300.000 | 1.500.000 | 300.000 | 200.000 |

### R.4.3 Pilihan penangguhan pegawai atas seluruh kewajiban pasien (`FIN-DEC-208`, Section 7)

**Aturan.** Untuk total kewajiban pasien `(E + K)` (kelebihan plafon ditambah co-pay/item di luar cakupan):
1. **Pilihan Bebas Pegawai/Pasien**: Membayar langsung di kasir (Cash/Transfer), menangguhkan seluruhnya menjadi AR Karyawan (potong gaji/cicilan), atau membagi sebagian.
2. **Persetujuan Tertulis untuk Keluarga**: Bila pasien bukan pegawai sendiri (pasangan, anak, orang tua), penangguhan `D > 0` wajib menyertakan identitas pemberi persetujuan (`EmployeeReceivableConsentGivenBy`) dan catatan nomor referensi persetujuan (`EmployeeReceivableConsentNotes`).
3. **Pemisahan Identitas Debitur vs Pasien**: Debitur pada `FinReceivable` selalu **Pegawai Pemilik Manfaat** (`BenefitOwnerId`), sedangkan pasien penerima layanan dicatat pada rincian (`FinReceivableItem.PatientId`).
4. **Handoff & AR Separation**: Bila `D = 0`, tidak ada `BilArHandoff` dengan `EMPLOYEE_BENEFIT` yang diterbitkan dan saldo piutang pegawai tidak pernah terbentuk di Finance.
5. **Koreksi Debitur**: Jika terjadi salah penetapan debitur pegawai, koreksi dilakukan melalui `CorrectBenefitDebtor` dengan Berita Acara resmi, dan restitusi salah potong diselesaikan lewat payroll periode berikutnya di HR tanpa pengeluaran kas manual dari Finance.

### R.4.4 Penjaga

| Kondisi | Perilaku |
|---|---|
| Ada penjamin kedua (`ExcessAmount` > 0) pada kunjungan berpenjamin internal | Finalisasi **ditolak**; kombinasi ini belum didukung |
| Jumlah baris serah terima tidak sama dengan `C + D` | Finalisasi dibatalkan sebagai galat invarian |
| Finalisasi diulang untuk invoice yang sama | Tidak menggandakan baris (unik `(InvoiceId, DebtorType)`) dan tidak mengurangi plafon dua kali |

---

## R.5 Kegagalan, pengulangan, dan rekonsiliasi

| Situasi | Perilaku | Idempoten? |
|---|---|:---:|
| HR tidak terbaca saat `BacaPlafon` | Finalisasi ditahan; petugas diminta konfirmasi manual HR | Ya |
| Jaringan putus setelah transaksi berhasil, klien mengulang | Finalisasi kembali sebagai pengulangan (`IsReplay`); plafon tidak berubah | Ya |
| Dua finalisasi bersamaan untuk orang yang sama | Salah satu mendapat `INSUFFICIENT` dan menghitung ulang | Ya |
| Koreksi salah orang | `KembalikanPlafon` untuk kunci lama, serah terima pengganti memakai kunci baru | Ya |
| Enrollment dibatalkan HR setelah finalisasi | Mutasi `USE` yang sudah terjadi tidak berubah; hanya pemakaian baru yang ditolak | Ya |

**Rekonsiliasi (invarian).** Untuk setiap orang yang ditanggung: `UsedAmount = jumlah mutasi USE − jumlah mutasi RETURN`
dan `RemainingAmount = LimitAmount − UsedAmount`. Selisih adalah galat data yang harus dilaporkan dan diperbaiki HR;
sistem **tidak** memperbaikinya diam-diam.

---

## R.6 Keamanan dan privasi

| Hal | Aturan |
|---|---|
| NIK | Tersamar pada pencarian dan daftar keluarga; **tidak** masuk log |
| Nama pegawai dan keluarga | Tidak masuk log; log hanya memuat ID, jenis operasi, dan hasil |
| Saldo plafon | Hanya terbaca oleh Billing dan HR; **tidak** oleh Registrasi (`FIN-DEC-202`) |
| Hubungan keluarga | Kolom sensitif (`BenefitRelationship`); tidak dipakai sebagai contoh data asli |
| Hak akses | `EmployeeBenefitLookup : Read` untuk Registrasi; `BillingFinalization : Create` untuk finalisasi (lihat `contracts/permission-audit-matrix.md` bagian `R`) |

---

## R.7 Dampak kompatibilitas

| Yang berubah | Pengguna terdampak | Kompatibel mundur? |
|---|---|:---:|
| Baris `PAYER`/`EMPLOYEE_BENEFIT` boleh hanya satu | `FinanceBillingIntakeService` | Ya — intake sudah memproses baris satu per satu |
| Kolom baru `BilArHandoff.DeferredPatientAmount` (default 0) | Billing | Ya — nilai bawaan 0 |
| Kolom `FinReceivable.BenefitRelationship` diperlebar 30 → 50 | Finance | Ya |
| Kartu pasien dan snapshot kunjungan: kolom baru nullable | Registrasi, Patient Management | Ya — kartu lama tetap sah tanpa tautan |
| Dua operasi HR baru | HR | Tambahan |
| Intake Finance, cicilan, payroll, clearance, pelunasan internal | — | **Tidak berubah** |

## R.8 Traceability

| Keputusan | Bagian | Task |
|---|---|---|
| `FIN-DEC-183`, `184`, `185` | `R.4` | `BE-FIN-107` |
| `FIN-DEC-199`, `201`, `204`, `209` | `R.2` | `BE-FIN-105`, `BE-FIN-106` |
| `FIN-DEC-202`, `205`, `207` | `R.3` | `BE-FIN-103`, `BE-FIN-104`, `BE-FIN-107`, `BE-FIN-108` (`107` membaca dan membagi; `108` memakai dan mengembalikan) |
| `FIN-DEC-208` | `R.4.3` | `BE-FIN-107`, `FE-FIN-047` |
| `FIN-DEC-210` | `R.4.1` | `BE-FIN-107` |

## R.9 Yang sengaja tidak dikontrakkan

| Yang dipertimbangkan | Alasan |
|---|---|
| Pengembalian plafon sebagian | Pengembalian selalu penuh sebesar mutasi `USE`; koreksi nominal dilakukan lewat pembalikan lalu serah terima pengganti |
| Registrasi memotong atau menulis saldo | Ditolak (`FIN-DEC-202`) |
| Billing menulis langsung ke tabel benefit HR | Ditolak: HR pemegang saldo; Billing hanya memanggil layanan |
| Notifikasi ke pegawai tentang pemakaian plafon | Belum dibahas; tidak mengubah model |
| Penjamin kedua pada kunjungan berpenjamin internal | Di luar cakupan; ditolak oleh penjaga `R.4.4` |
| Klaim benefit (`TrxBenefitClaim*`) | Di luar `EPIC FIN-04` |

---

# AMENDMENT REVISI 20 — Jembatan Manual Potong Gaji, Persetujuan Pegawai, dan Koreksi `PayrollRunId` (`FIN-INTEGRATION-1.10`)

```yaml
contract_version: FIN-INTEGRATION-1.10
last_changed_in: Revisi 20 (13 Oktober 2026)
status: approved
approved_by: Pemilik Finance, HR, dan Billing (pengguna sesi); konfirmasi pemilik Registrasi belum turun
approved_at: 2026-10-13
owner: Pemilik Finance (Product/Domain), bersama pemilik HR dan Billing
input_revision: 19
input_hash: 1180a57b12011633   # SHA-256 (16 karakter pertama) berkas ini sebelum amandemen Revisi 20
input_decisions: FIN-DEC-214..222
compatibility_impact: lihat S.7 — dua koreksi atas kontrak approved (P.3/FIN-API-1.9 P.1.1 dan R.4.3); selebihnya tambahan
domain_architecture_readiness: DOMAIN_ARCHITECTURE_NOT_RUN (alasan sama dengan FIN-INTEGRATION-1.9)
```

| Field | Nilai |
|---|---|
| Sifat | Kontrak **approved** pemilik Finance, HR, dan Billing (13 Oktober 2026, `FIN-DEC-223`). Mengubah sebagian kontrak yang sudah `approved` (`FIN-INTEGRATION-1.9`), sehingga dibuat sebagai versi baru, **bukan** suntingan |
| Yang baru | `INT-FIN-PAY-001` — jembatan manual potong gaji (bagian `S.1`..`S.4`) |
| Yang dikoreksi | `INT-HR-FIN-001` (`P.3`): `payrollRunId` opsional (`S.5`); `INT-FIN-BIL-001` (`R.4.3`): nama pemberi persetujuan (`S.6`) |
| Yang tidak berubah | Seluruh bagian `R` lainnya; `P.1`, `P.2`, `P.4`..`P.6` |

---

## S.0 Mengapa jembatan ini ada

Potong gaji otomatis menunggu mesin payroll run HR yang belum ada (`FIN-DEC-214`). Sampai mesin itu aktif, pemilik
memutuskan cicilan tetap boleh disetujui dan gaji dipotong pada penggajian yang berjalan di luar sistem; Finance
mencatat hasilnya secara manual per periode (`FIN-DEC-215`). Jembatan ini mengikat empat hal: bagaimana penggajian
mengetahui jadwal (`FIN-DEC-219`), bagaimana hasil masuk (`FIN-DEC-221`), kendalinya (`FIN-DEC-220`), dan kapan
jembatan berhenti (`FIN-DEC-222`).

**Prinsip utama: satu inti pencatatan.** Jalur manual dan jalur mesin memakai **inti pencatatan hasil yang sama**
(yang hari ini dipakai `POST .../payroll-results`), sehingga aturan pelimpahan tunggakan, idempotensi, dan mutasi
`POTONGAN-GAJI` tidak pernah berbeda antara keduanya.

## S.1 Jadwal potongan per periode (`FIN-DEC-219`)

| Properti | Spesifikasi |
|---|---|
| Arah | Finance → petugas penggajian (berkas, bukan antarsistem) |
| Isi | Setiap angsuran pada perjanjian berstatus `DISETUJUI` yang `DeductionPeriod`-nya sama dengan periode diminta dan statusnya `DIJADWALKAN` |
| Nominal terutang | `OutstandingAmount` baris itu = `ScheduledAmount + CarriedOverAmount − PaidAmount` (sudah termasuk tunggakan yang dilimpahkan) |
| Kolom | Kode angsuran (`PlanNumber`-`InstallmentNumber`), NIP, nama pegawai, unit, nomor piutang, angsuran ke-, nominal terjadwal, tunggakan terbawa, nominal terutang; ditambah dua kolom kosong untuk diisi penggajian: **status** dan **nominal terpotong** |
| Bentuk | Excel (sekaligus templat unggah hasil) dan PDF (arsip/serah terima) |
| Hak akses | Membaca jadwal: Finance |

**Contoh.** Cicilan Budi Rp 1.000.000 per bulan. Juli gagal, Agustus gagal. Jadwal September menampilkan Budi dengan
terjadwal Rp 1.000.000, tunggakan terbawa Rp 2.000.000, terutang **Rp 3.000.000**. Baris Juli dan Agustus (`TERTUNGGAK`)
**tidak** muncul lagi, karena sisanya sudah dilimpahkan ke September.

## S.2 Batch hasil potongan (`FIN-DEC-220`, `221`)

**Siklus.** Unggah → divalidasi per baris → diajukan dengan bukti → disahkan oleh orang lain → diterapkan ke saldo.

| Aturan | Ketentuan |
|---|---|
| Satu batch per periode | Satu batch `REGULER` aktif (`DRAFT`, `DIAJUKAN`, atau `DISAHKAN`) per periode. Batch yang ditolak atau dibatalkan boleh diganti batch baru |
| Periode payroll | Batch membawa `DeductionPeriod` (`YYYY-MM`) **dan** `PayrollPeriodId` (master periode payroll HR yang sudah ada) yang bulannya sama. `PayrollPeriodId` adalah kunci idempotensi inti pencatatan |
| Pencocokan baris | Kode angsuran **dan** NIP wajib cocok dengan pemilik manfaat angsuran itu. Salah satu saja tidak cukup |
| Status dan nominal | `BERHASIL` ⇒ nominal = terutang; `SEBAGIAN` ⇒ 0 < nominal < terutang; `GAGAL` ⇒ nominal 0 |
| Kelengkapan | **Setiap** angsuran pada jadwal periode itu wajib muncul tepat sekali (yang tidak dipotong ditulis `GAGAL`). Tanpa ini, tunggakan tidak akan pernah dilimpahkan |
| Total kontrol | Pengaju mengisi total potongan menurut laporan penggajian; harus sama dengan jumlah nominal terpotong pada berkas |
| Bukti | Nomor laporan penggajian dan lampiran laporan (memakai mekanisme bukti transaksi Finance yang sudah ada, jenis `LAPORAN-PENGGAJIAN`) wajib saat diajukan |
| Baris tidak valid | Batch dengan satu baris tidak valid **tidak dapat diajukan**; perbaiki berkas lalu unggah ulang |
| Pengesahan | Oleh orang **berbeda** dari pengaju. Seluruh baris diterapkan dalam **satu transaksi**: semua berhasil atau tidak ada yang berubah |
| Penerapan | Tiap baris diterapkan lewat inti pencatatan yang sama dengan `payroll-results` (`payrollRunId` kosong) |

**Contoh.** Periode September 2026, tiga angsuran terjadwal: Budi terutang Rp 3.000.000, Sari Rp 500.000, Ani Rp 750.000.
Berkas: Budi `SEBAGIAN` Rp 1.500.000; Sari `BERHASIL` Rp 500.000; Ani `GAGAL` Rp 0. Total kontrol Rp 2.000.000 = jumlah berkas.
Setelah disahkan: piutang Budi turun Rp 1.500.000 dan sisa Rp 1.500.000 dilimpahkan ke Oktober; Sari lunas untuk September;
seluruh Rp 750.000 milik Ani dilimpahkan ke Oktober.

## S.3 Koreksi berotorisasi (`FIN-DEC-222`)

| Aturan | Ketentuan |
|---|---|
| Kapan | Bila hasil yang sudah disahkan ternyata salah (misalnya laporan penggajian direvisi) |
| Bentuk | Batch `KOREKSI` untuk satu periode yang **sudah** punya batch `REGULER` `DISAHKAN`; berisi hanya baris yang dikoreksi, dengan nominal yang **benar** |
| Alasan | Wajib |
| Penyetuju | Pejabat dengan hak khusus pengesahan koreksi; tetap berbeda dari pengaju |
| Penerapan | Sistem menghitung selisih (benar − tercatat). Selisih positif: tambahan potongan (`POTONGAN-GAJI`). Selisih negatif: **pembalikan** (`PEMBALIKAN-POTONGAN-GAJI`), saldo piutang dan angsuran naik kembali. Tunggakan yang sudah dilimpahkan ke angsuran berikutnya ikut disesuaikan |
| Batas | Hanya bila angsuran berikutnya pada perjanjian yang sama **belum** menerima hasil. Bila sudah, koreksi ditolak dan ditangani lewat penyesuaian piutang yang sudah ada |
| Restitusi | Koreksi ini **bukan** restitusi salah orang; restitusi tetap lewat payroll HR (`FIN-DEC-195`) |

**Contoh.** September tercatat Budi terpotong Rp 1.500.000, padahal laporan revisi menyebut Rp 1.200.000. Koreksi: selisih
−Rp 300.000 → mutasi pembalikan Rp 300.000; piutang Budi naik Rp 300.000; tunggakan Oktober naik dari Rp 1.500.000 menjadi
Rp 1.800.000 (Oktober belum menerima hasil, jadi koreksi diizinkan).

## S.4 Peralihan ke mesin payroll (`FIN-DEC-222`)

| Aturan | Ketentuan |
|---|---|
| Penanda | Satu nilai aktif "periode pertama yang ditangani mesin" (`AutomaticFromPeriod`, `YYYY-MM`), disimpan sebagai master Finance berversi |
| Sebelum ada nilai | Seluruh periode memakai jalur manual |
| Jalur manual | Batch `REGULER` **dan** `KOREKSI` untuk periode ≥ `AutomaticFromPeriod` ditolak |
| Jalur mesin | Hasil dari mesin (`payroll-results` dengan `payrollRunId` terisi) untuk periode < `AutomaticFromPeriod` ditolak |
| Penetapan | Pejabat Finance, setelah pemilik HR menyatakan mesin siap; alasan wajib; tidak boleh mundur ke periode yang sudah punya batch manual `DISAHKAN` |

**Contoh.** Mesin siap untuk Januari 2027. `AutomaticFromPeriod = 2027-01`. Batch manual Desember 2026 masih bisa diajukan dan
dikoreksi; batch manual Januari 2027 ditolak; hasil mesin untuk Desember 2026 ditolak.

## S.5 Koreksi `INT-HR-FIN-001` (§`P.3`) dan `FIN-API-1.9` §`P.1.1`

| Semula | Menjadi | Dasar |
|---|---|---|
| `payrollRunId` wajib | `payrollRunId` **opsional**. Terisi = hasil jalur mesin; kosong = hasil jalur manual (lewat batch `S.2`) | Kode: `PayrollResultRequest.PayrollRunId` bertipe `Guid?` (`FinanceReceivablePayrollResultDtos.cs:13`) |
| — | Pemanggil langsung tanpa `payrollRunId` hanya sah bila datang dari pengesahan batch; panggilan HTTP langsung tanpa `payrollRunId` ditolak setelah jembatan aktif | Mencegah jalan pintas melewati kendali `S.2` |

## S.6 Koreksi `INT-FIN-BIL-001` (§`R.4.3`): nama pemberi persetujuan (`FIN-DEC-218`)

| Aturan | Ketentuan |
|---|---|
| Kapan wajib | `D` (pilihan utang) lebih dari 0 **dan** hubungan bukan `SELF` |
| Isi | Nama orang yang menyetujui utang dibuat atas nama pegawai (teks bebas, maksimal 200 karakter). Waktu = waktu finalisasi (tercatat otomatis) |
| Tempat | Satu kolom pada baris serah terima `EMPLOYEE_BENEFIT`; Finance tidak membacanya |
| Hak akses | `BillingFinalization : Create` yang sudah ada; tanpa butir baru |

**Contoh.** Sari (anak Budi) berobat; sisa co-pay Rp 400.000 dijadikan utang. Kasir wajib mengisi nama pemberi persetujuan,
misalnya "Budi (pegawai, via telepon)". Bila Budi sendiri yang berobat, isian ini tidak diwajibkan.

## S.7 Dampak kompatibilitas

| Yang berubah | Terdampak | Kompatibel mundur? |
|---|---|:---:|
| `payrollRunId` opsional | Penerima hasil potongan | Ya — kode sudah begitu |
| Penolakan panggilan langsung tanpa `payrollRunId` setelah jembatan aktif | Pemanggil manual non-batch | Tidak, disengaja |
| Kolom nama persetujuan pada `BilArHandoff` (nullable) | Billing | Ya |
| Inti pencatatan dipisah agar bisa berjalan dalam transaksi luar | `FinanceReceivablePayrollSyncService` | Ya — perilaku endpoint tidak berubah |
| Tabel dan endpoint jembatan | Finance | Tambahan |

## S.8 Traceability

| Keputusan | Bagian |
|---|---|
| `FIN-DEC-215`, `219` | `S.0`, `S.1` |
| `FIN-DEC-220`, `221` | `S.2` |
| `FIN-DEC-222` | `S.3`, `S.4` |
| `FIN-DEC-218` | `S.6` |
| Fakta kode `PayrollRunId` | `S.5` |

## S.9 Yang sengaja tidak dikontrakkan

| Yang dipertimbangkan | Alasan |
|---|---|
| Input hasil satu per satu di layar | Ditolak `FIN-DEC-221` |
| Pengesahan sebagian baris | Ditolak: total harus cocok dengan laporan penggajian; batch utuh atau tidak sama sekali |
| Menulis jadwal ke input variabel HR selama jembatan | Penggajian di luar sistem tidak membacanya (`FIN-DEC-219`); `BE-FIN-094` tetap untuk tahap mesin |
| Koreksi lintas beberapa periode berurutan | Hanya angsuran yang berikutnya belum menerima hasil; selebihnya lewat penyesuaian piutang yang ada |
| Peralihan mundur | Tidak boleh mundur ke periode yang sudah punya batch manual disahkan |
