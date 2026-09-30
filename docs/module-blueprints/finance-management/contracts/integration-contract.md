# Kontrak Integrasi — Finance Management

| Field | Nilai |
|---|---|
| Contract version | `FIN-INTEGRATION-1.6` |
| `last_changed_in` (1.6) | `FIN-INTEGRATION-1.6` — AMENDMENT REVISI 9, 29 September 2026. **Tiga pemicu dikoreksi berbasis bukti source `7811c048`** (`FIN-DES-064`, `FIN-DES-065`): mutasi `BilDepositMovement` `RELEASE` menerbitkan `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT`, bukan `PENGEMBALIAN-UANG-MUKA`; pemicu kode 37 yang semula "tidak pernah ada" kini ada lewat `BKC-DEC-131`; dan mutasi `RELEASE` tanpa pasangan `REVERSAL` ditolak *fail-closed*. Status **`approved`** — disahkan oleh Yasmin via `FIN-DEC-080` dan `FIN-DEC-081` (mengoreksi `FIN-DEC-041`) |
| `last_changed_in` | `FIN-INTEGRATION-1.5` — AMENDMENT REVISI 7, 28 September 2026 (bagian 5.10.4 butir 1 dan 5.10.5 butir 1 **dikoreksi berbasis bukti** kontrak as-is kotak masuk Accounting; bagian 5.11 baru). Sebelumnya `1.4` — AMENDMENT REVISI 6 (bagian 5.10 baru) |
| Status | `1.2` `approved` dan `locked` 25 September 2026; `1.3` disetujui dan dikunci owner 26 September 2026; `1.4` disetujui owner 28 September 2026. **`1.6` (koreksi REVISI 9) `approved` 29 September 2026 bersama `FIN-DEC-080`/`081`** |
| Owner | Yasmin (Product/Domain Owner Finance) |
| `approved_by` / `approved_at` | Yasmin / 2026-09-29 (untuk `1.6`) |
| Input revision | `00-interview-decisions.md` — `FIN-DEC-001`..`081` (`FIN-DEC-080`..`081` ditambahkan 29 September 2026, Amendment Pass mutasi `RELEASE`) |
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
