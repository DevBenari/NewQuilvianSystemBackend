# Accounting — Cross-Module Contract (Finance / AR / AP → Accounting)

| Field | Value |
|---|---|
| `contract_version` | `ACC-XMOD-0.3` |
| `last_changed_in` | `ACC-XMOD-0.3` — 24 September 2026. Sebelumnya `0.2`, 15 September 2026 |
| Klasifikasi artefak | **`CROSS_MODULE_REQUIRED`** |
| Status | **`approved`** — bentuk batas (bagian 2–7) diratifikasi owner Finance lewat `FIN-DEC-001` atas `0.2` apa adanya; tambahan `0.3` diputuskan sisi Accounting (`ACC-DEC-082`..`090`) dan **menunggu konfirmasi Finance hanya untuk butir yang ditandai** di bagian 9 |
| Consumer | Accounting (owner: Rizki) |
| Producer | Finance / AR / AP (owner: Yasmin) — **lifecycle internalnya bukan wewenang Accounting** |
| `approved_by` / `approved_at` | Rizki, 24 September 2026 (`ACC-DEC-082`); Yasmin, 20 September 2026 (`FIN-DEC-001`, atas `0.2`) |
| `input_revision` | `00-interview-decisions.md@10` (sampai `ACC-DEC-091`) |
| `input_hash` | `00-interview-decisions.md` sha256 `de6bc421…9126a6`; `contracts/api-contract.md` sha256 `86979e11…bddef49` — diukur 15 September 2026 pada `rizkiG` `7b0c2ece` |
| Compatibility impact | **Tidak kompatibel dengan `0.1`**: nama dan tipe bidang berubah (bagian 0a). **Nol penerbit terdampak** — modul Finance belum punya kode di branch mana pun (diperiksa 14 September 2026: nol kelas `Fin*` di `QuilvianIntegrationBackend`, `rizkiG`, dan `Yasmina`) |
| Traceability | `ACC-DEC-002`, `003`, `020`, `021`, `035`, `044`, `046`, `047`, `048`, `049`, `051`, `056`, `058`, `059`, `060`, `071`, `075`, `082`, `083`, `084`, `085`, `086`, `087`, `088`, `089`, `090`, `091`; `ACC-XM-001` (`CLOSED`); `FIN-DEC-001`, `002`, `003`, `004`, `007`, `008`, `023` |
| Sumber kebenaran | Bila berkas ini berbeda dengan `api-contract.md` bagian *Isi `ReceiveAccountingEventRequest`* atau `integration-contract.md` bagian 6, **kedua berkas itu yang berlaku**, dan selisihnya adalah cacat dokumen yang wajib diperbaiki dalam perubahan yang sama |
| Implementasi | Kotak masuk kejadian **belum dibangun**, dan **boleh dibangun** sejak 24 September 2026 (`ACC-DEC-082`). Pengaktifan pengiriman menunggu gerbang cutover (bagian 13) |

## 0. Kenapa berkas ini ada

Finance dikembangkan **paralel** oleh Yasmin, bukan setelah Accounting selesai. Kalau bentuk
batas antar keduanya baru disepakati saat penyambungan dimulai, Finance sudah terlanjur mengunci
bentuk kejadiannya dan Accounting terpaksa menerima apa pun yang sampai — atau sebaliknya.

Berkas ini mengunci **bentuk batas**, bukan implementasinya. Ia memberi tahu Finance apa yang
Accounting butuhkan agar dapat membukukan dengan benar, dan menandai dengan jujur mana yang
**bukan** hak Accounting untuk menentukan.

## 0a. Perubahan `0.1` → `0.2`

`ACC-XMOD-0.1` disusun 1 September 2026, sebelum Phase 2 dirancang. Sesudah itu delapan keputusan
owner mengunci bentuk pesan yang sesungguhnya (`ACC-DEC-048`, `058`, `060`, `075`), dan kontrak
`ACC-API` serta `ACC-INTEGRATION` sudah memuatnya sejak 8 September 2026. Tetapi berkas ini **tidak
ikut diperbarui**, sehingga dua kontrak yang sama-sama dirujuk untuk Finance berbeda isi.

Akibatnya bila dibiarkan: penerbit yang dibangun dari `0.1` mengirim `EventId` bertipe `Guid` tanpa
`EventOccurredAt` dan `LegalEntityId`, lalu **setiap pesannya ditolak `400`** oleh Accounting.

`0.2` tidak menambah satu keputusan pun. Ia hanya menyamakan berkas ini dengan yang sudah berlaku.

### Pemetaan bidang

| `0.1` | `0.2` | Alasan |
|---|---|---|
| `EventId` (`Guid`) | **`EventNumber`** (`string`) | `ACC-DEC-048`. Nomor kejadian dibuat penerbit dan boleh berbentuk teks seperti `EVT-100` |
| `EventType` (`string(60)`) | **`EventTypeCode`** (`string`, maks 50) | `ACC-DEC-048`, `ACC-DEC-075`. Kode disimpan apa adanya walaupun belum terdaftar di Accounting |
| `SourceDomain` (`string(30)`) | **`SourceModule`** (`string`) | `ACC-DEC-048` |
| `SourceTransactionId` (`Guid`) | `SourceTransactionId` (**`string`**) | Nomor transaksi Finance boleh berupa teks seperti `AR-2026-09-00871` |
| `SourceVersion` (`int`) | `SourceVersion` (**`string`**) | `ACC-DEC-048` |
| — | **`EventOccurredAt`** (`timestamptz`) | Baru. Tanggal dokumen asli, dipakai saat periodenya sudah tertutup (`ACC-DEC-047`) |
| `AccountingDate` (`date`) | `AccountingDate` (`date`) | Tidak berubah |
| `Amount` (`decimal(18,2)`) | `Amount` (`decimal(18,2)`) | Tidak berubah |
| `CurrencyCode` (`string(3)`) | `CurrencyCode` (`string`) | Tidak berubah maknanya — hanya `IDR` |
| — | **`LegalEntityId`** (`Guid`) | Baru. Buku badan hukum mana yang disentuh (`ACC-DEC-037`) |
| `CorrelationId` (`Guid`) | `CorrelationId` (`Guid`) | Tidak berubah; kini wajib lewat keputusan tersendiri `ACC-DEC-060` |
| `CausationId` (`Guid`) | `CausationId` (`Guid`) | Tidak berubah; wajib lewat `ACC-DEC-060` |
| `IdempotencyKey` (`string(100)`) | **Dihapus** | Kunci anti-ganda pertama sudah dipegang `EventNumber`; bidang kedua dengan fungsi yang sama hanya menambah cara salah mengisi |
| — | `Components` (daftar, **opsional**) | Baru. Rincian nilai per komponen (`ACC-DEC-058`) |

### Perubahan lain

| Bagian | `0.1` | `0.2` |
|---|---|---|
| Kunci anti-ganda kedua | `SourceDomain` + `SourceTransactionId` + `EventType` + `SourceVersion` | `SourceModule` + `SourceTransactionId` + **`EventTypeCode`** + `SourceVersion` (`ACC-DEC-075`) |
| Periode sudah tertutup | Ditolak (`RejectedPeriodClosed`) | **Tidak ditolak.** Dicatat pada periode terbuka berikutnya (`ACC-DEC-047`) |
| Mata uang bukan rupiah | Disimpan sebagai `RejectedUnsupportedCurrency` | Ditolak `409` di pintu masuk, **tidak** menjadi kejadian `Diterima` |
| Nama status | "Belum final" | Final: `Diterima`, `Terjurnal`, `Tertahan`, `Gagal`, `Diabaikan` (`ACC-STATE` Phase 2 bagian 1) |
| `ACC-XM-001` | Terbuka seluruhnya | Diputuskan sisi Accounting (`ACC-DEC-044`), dikonfirmasi owner Billing (`ACC-DEC-059`); **ratifikasi owner Finance belum ada** |

## 0b. Perubahan `0.2` → `0.3`

| Bagian | `0.2` | `0.3` | Dasar |
|---|---|---|---|
| Status kontrak | `draft`, menunggu Finance | **`approved`** | `FIN-DEC-001`, `ACC-DEC-082` |
| Katalog jenis kejadian | Belum ditetapkan | **17 kode diratifikasi** (bagian 3a) | `FIN-DEC-002`, `ACC-DEC-083` |
| Contoh pesan | `PENGAKUAN-PIUTANG` membawa `JASA_MEDIS` | `PENGAKUAN-PIUTANG` bernilai `TOTAL` saja; jasa medis lewat `PENGAKUAN-HUTANG-DOKTER` | `FIN-DEC-003`, `ACC-DEC-086` |
| Kapan jurnal dibuat | Tersirat | **Seketika di dalam request**, penjadwal sebagai cadangan (bagian 4a) | `ACC-DEC-084` |
| Isi balasan | Hanya nama `AccountingEventReceiptDto` | **Tujuh bidang** (bagian 4b) | `ACC-DEC-085` |
| Akun layanan | Belum diputuskan | **Empat syarat Accounting**; mekanisme tetap terbuka (bagian 4c) | `ACC-DEC-088` |
| Saldo subledger | Isi minimum saja | **Amplop dua belas bidang + dua rincian** (bagian 8) | `ACC-DEC-087` |
| Cutover | Tidak diatur | **Enam gerbang** (bagian 13) | `ACC-DEC-089`, `090`, `091` |

**Nol perubahan pada kedua belas bidang, tipe, kunci anti-ganda, dan kode balasan.** Penerbit yang
sudah dibangun dari `0.2` — `FinAccountingEventOutbox` — tetap cocok tanpa perubahan.

## 1. Batas kewenangan — dibaca lebih dahulu

| Accounting **boleh** menentukan | Accounting **TIDAK boleh** menentukan |
|---|---|
| Kejadian apa yang ia butuhkan agar dapat membukukan | Kapan AR menganggap sesuatu `recognized` |
| Data apa yang wajib ada pada kejadian itu | Status internal AR/AP dan perpindahannya |
| Apa yang terjadi bila data itu kurang atau salah | Kapan Finance menerbitkan kejadian |
| Bahwa pembukuan ganda harus dapat dicegah | Bagaimana Finance menyimpan piutang dan utangnya |

Contoh yang sah: *"Accounting membutuhkan kejadian pengakuan piutang."*

Contoh yang **tidak** sah: *"AR harus dianggap recognized ketika invoice difinalisasi."*
Kalimat kedua menentukan lifecycle Finance, dan itu wewenang Yasmin.

Setiap kali kontrak ini menyentuh wilayah kedua, ia menandainya
**`CROSS_MODULE_DECISION_REQUIRED`** dan menyebut siapa pemiliknya.

## 2. Arah aliran

```
Billing  ──BIL-INT-007/008/009──▶  Finance (AR/AP)  ──ACC-XMOD (berkas ini)──▶  Accounting
```

| Hal | Keadaan per 24 September 2026 |
|---|---|
| Arah | **Satu arah**, Finance → Accounting |
| Penerbit kejadian keuangan resmi | **Finance** — `ACC-DEC-044`, 8 September 2026 |
| Konfirmasi owner Billing | **Sudah** — `ACC-DEC-059`, 9 September 2026. Finance menerbitkan kejadian **tersendiri**, bukan meneruskan `BilArHandoff` |
| Ratifikasi owner Finance | **Sudah** — `FIN-DEC-001`, 20 September 2026. `ACC-XM-001` `CLOSED` lewat `ACC-DEC-082`, 24 September 2026 |
| Yang dilarang Accounting | Membaca `BilArHandoff` atau tabel Billing lain secara langsung (`ACC-DEC-059`); menerbitkan kejadian ke modul lain (`ACC-DEC-002`) |

**Kenapa satu arah dan satu penerbit.** Bila Accounting membaca Billing sementara Finance juga
menerbitkan kejadian atas tagihan yang sama, satu tagihan Rp 10.000.000 menghasilkan dua jurnal.
Buku besar tetap seimbang, tetapi pendapatan rumah sakit tercatat Rp 20.000.000 — tanpa satu pun
pesan galat.

## 3. Bentuk pesan — dua belas bidang wajib

Diambil apa adanya dari `api-contract.md` bagian *Isi `ReceiveAccountingEventRequest`*
(`ACC-DEC-048` untuk sepuluh bidang pertama, `ACC-DEC-060` untuk dua bidang penelusuran).

| Bidang | Tipe | Wajib | Pemilik makna | Contoh | Kegunaan bagi Accounting |
|---|---|:---:|---|---|---|
| `EventNumber` | `string` | Ya | Finance | `EVT-100` | Nomor unik kejadian. Kunci anti-ganda **pertama** |
| `EventTypeCode` | `string` (maks 50) | Ya | **Bersama** | `PENGAKUAN-PIUTANG` | Menentukan aturan posting mana yang dipakai. Salah satu dari 17 kode pada bagian 3a (`ACC-DEC-083`) |
| `SourceModule` | `string` | Ya | Finance | `Finance` | Modul asal. Bagian kunci anti-ganda kedua |
| `SourceTransactionId` | `string` | Ya | Finance | `AR-2026-09-00871` | Nomor transaksi asal. Bagian kunci kedua, dan akar penelusuran balik |
| `SourceVersion` | `string` | Ya | Finance | `1` | Versi transaksi asal. Membedakan koreksi dari kiriman ulang |
| `EventOccurredAt` | `timestamptz` | Ya | Finance | `2026-09-08T10:15:00+07:00` | Waktu kejadian sebenarnya. Disimpan sebagai tanggal dokumen |
| `AccountingDate` | `date` | Ya | **Bersama** | `2026-09-08` | Menentukan periode akuntansi. **Bukan** waktu kirim |
| `Amount` | `decimal(18,2)` | Ya | Finance | `10000000.00` | Nilai total kejadian |
| `CurrencyCode` | `string` | Ya | Finance | `IDR` | Hanya `IDR`; nilai lain ditolak `409` (bagian 7) |
| `LegalEntityId` | `Guid` | Ya | Finance | Id badan hukum | Buku badan hukum mana yang disentuh |
| `CorrelationId` | `Guid` | Ya | Finance | — | Merangkai satu alur bisnis sampai ke faktur Billing |
| `CausationId` | `Guid` | Ya | Finance | — | Tindakan yang menyebabkan kejadian ini |

### Bidang opsional: `Components`

| Bidang | Tipe | Wajib | Keterangan |
|---|---|:---:|---|
| `Components` | daftar | Tidak | Rincian nilai. Setiap butir berisi `ComponentCode` dan `Amount`. Pesan tanpa `Components` berarti seluruh nilai memakai komponen `TOTAL` (`ACC-DEC-058`) |

### Empat aturan yang mengikat kedua pihak

1. **Kedua belas bidang wajib.** Pesan dengan satu bidang kosong ditolak `400`, bukan diterima
   sebagian.
2. **Mata uang hanya `IDR`.** Nilai lain ditolak `409`.
3. **Nol pengenal pasien** (`ACC-DEC-056`). Nama pasien, nomor rekam medis, nomor kunjungan, dan
   `DoctorId` **dilarang** ada di dalam pesan. Penelusuran ke pasien dilakukan lewat
   `SourceTransactionId` dan `CorrelationId`, di modul asalnya.
4. **`CorrelationId` dan `CausationId` wajib.** Tanpa keduanya, `SourceTransactionId` hanya
   menunjuk catatan AR milik Finance, dan pertanyaan *"jurnal ini dari tagihan pasien yang mana"*
   menuntut membuka Finance lebih dahulu.

### Contoh lengkap — pengakuan piutang rawat jalan

Diganti pada `0.3` (`ACC-DEC-086`). Sampai `0.2` contoh ini membawa komponen `JASA_MEDIS`, padahal
Finance **tidak pernah** menyertakan jasa medis pada `PENGAKUAN-PIUTANG` (`FIN-DEC-003`).

```json
{
  "EventNumber": "EVT-100",
  "EventTypeCode": "PENGAKUAN-PIUTANG",
  "SourceModule": "Finance",
  "SourceTransactionId": "AR-2026-09-00871",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-09-08T10:15:00+07:00",
  "AccountingDate": "2026-09-08",
  "Amount": 10000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "7d2f0c1e-5a3b-4c8d-9e21-0f6a4b7c8d90",
  "CausationId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d"
}
```

Tanpa `Components`, seluruh nilai memakai komponen `TOTAL`. Aturan posting untuk jenis itu memuat
dua baris: debit Piutang Penjamin Rp 10.000.000, kredit Pendapatan Rawat Jalan Rp 10.000.000.

### Contoh — jasa medis dokter

Jasa medis dibukukan **terpisah**, setelah fee dokter disetujui di Finance:

```json
{
  "EventNumber": "EVT-131",
  "EventTypeCode": "PENGAKUAN-HUTANG-DOKTER",
  "SourceModule": "Finance",
  "SourceTransactionId": "AP-DR-2026-09-00112",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-09-12T14:00:00+07:00",
  "AccountingDate": "2026-09-12",
  "Amount": 3000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "7d2f0c1e-5a3b-4c8d-9e21-0f6a4b7c8d90",
  "CausationId": "0c9d8e7f-6a5b-4c3d-8e2f-1a0b9c8d7e6f"
}
```

Aturan posting: debit Beban Jasa Medis Rp 3.000.000, kredit Utang Jasa Medis Dokter Rp 3.000.000.

**Aturan tertulis (`ACC-DEC-086`).** Aturan posting `PENGAKUAN-PIUTANG` **tidak boleh** memuat baris
berkomponen `JASA_MEDIS`. Bila memuatnya, **setiap** kejadian pengakuan piutang Tertahan (`422`),
karena baris aturan menuntut komponen yang tidak pernah dikirim Finance — tidak satu pun piutang
terjurnal sampai aturannya dibetulkan. Aturan ini ditegakkan saat penyusunan aturan posting, bukan
oleh kode.

Daftar komponen yang dikirim Finance per jenis kejadian **belum ditetapkan** (bagian 9).

## 3a. Katalog jenis kejadian — 17 kode

Diratifikasi `ACC-DEC-083` atas `FIN-DEC-002`. `SourceModule` seluruhnya `Finance`. Kode baru
hanya lewat keputusan kedua pihak. Kode yang sah tetapi belum punya aturan posting tetap
**Tertahan** — ratifikasi kode bukan aturan posting.

| Kode | Dipicu oleh (menurut Finance) | Gelombang Finance |
|---|---|---|
| `PENGAKUAN-PIUTANG` | Piutang diakui dari fakta AR Billing | MVP-1 |
| `PENERIMAAN-KASIR` | Penerimaan dari tender Billing **sebelum/tanpa** piutang | MVP-2 |
| `PEMBALIKAN-PENERIMAAN-KASIR` | Pembalikan penerimaan kasir | MVP-3 |
| `PENERIMAAN-PIUTANG` | Uang diterima untuk piutang yang **sudah** ada | MVP-3 |
| `PENYESUAIAN-PIUTANG` | Koreksi piutang disetujui | MVP-3 |
| `PEMUTIHAN-PIUTANG` | Penghapusan piutang disetujui | MVP-3 |
| `SETORAN-BANK` | Setoran bank diposting | MVP-4 |
| `PETTY-CASH-TOP-UP` | Penambahan saldo kas kecil | MVP-5 |
| `PETTY-CASH-DISBURSEMENT` | Pencairan kas kecil | MVP-5 |
| `PETTY-CASH-RETURN` | Pengembalian sisa kas kecil | MVP-5 |
| `PETTY-CASH-REVERSAL` | Pembalikan pencairan kas kecil | MVP-5 |
| `PETTY-CASH-ADJUSTMENT` | Koreksi saldo kas kecil | MVP-5 |
| `PENGAKUAN-HUTANG-SUPPLIER` | Utang supplier diinput | Pasca-MVP |
| `PEMBAYARAN-HUTANG-SUPPLIER` | Pembayaran supplier ditandai dibayar | Pasca-MVP |
| `PENGAKUAN-HUTANG-DOKTER` | Fee dokter disetujui menjadi utang | Pasca-MVP |
| `PEMBAYARAN-HUTANG-DOKTER` | Pembayaran dokter ditandai dibayar | Pasca-MVP |
| `PENYESUAIAN-HUTANG` | Koreksi utang disetujui | Pasca-MVP |

**`PENERIMAAN-KASIR` dan `PENERIMAAN-PIUTANG` jangan disamakan.** Yang pertama terbit saat belum ada
piutang, sehingga lawannya ditentukan aturan posting (piutang, uang muka pasien, atau pendapatan).
Yang kedua melunasi piutang yang sudah diakui, sehingga lawannya akun kontrol piutang. Menyamakan
keduanya membuat piutang berkurang dua kali atau tidak berkurang sama sekali.

**Kode di luar katalog.** Data uji `PATIENT_PAYMENT` bersumber `CASHIER` di database pengembangan
bertentangan dengan penerbit tunggal Finance; ia dinonaktifkan lewat layar master, bukan dihapus
lewat SQL (`ACC-DEC-083`). Peristiwa kas deposit pasien, kelebihan bayar beserta pengembaliannya,
dan selisih kas shift **tidak** tercakup katalog ini — lihat bagian 13 gerbang G6.

## 4. Pintu masuk

#### Corporate - Accounting - Accounting Event

Base URL: `api/v1/corporate/accounting/accounting-events` — **Rencana (belum tersedia)**

| Method | Path | Kegunaan | Hak akses | Request | Response |
|---|---|---|---|---|---|
| `POST` | `/` | Menerima satu kejadian keuangan dari Finance | `AccountingEvent : Receive` — hanya akun layanan | `ReceiveAccountingEventRequest` | `ApiResponse<AccountingEventReceiptDto>` |

| Kode | Arti bagi penerbit | Yang dilakukan Finance |
|---|---|---|
| `201` | Kejadian baru diterima dan dicatat. Membawa `AccountingEventReceiptDto`; `JournalNumber` bisa kosong bila penjurnalan tertunda gangguan teknis (bagian 4a) | Simpan `AccountingEventId` sebagai `AccountingReceiptNumber` dan `JournalNumber` bila ada |
| `200` | Kejadian **sudah pernah diterima**. Accounting mengembalikan `AccountingEventReceiptDto` dengan keadaan terkini — termasuk nomor jurnal bila sudah terbentuk — tanpa membuat jurnal baru | Tidak ada. **Bukan** kesalahan — aman untuk kiriman ulang |
| `400` | Salah satu dari kedua belas bidang kosong atau tidak masuk akal | Perbaiki pesan, kirim ulang |
| `403` | Akun pengirim tidak punya hak `Receive`, atau badan hukumnya bukan haknya | Periksa akun layanan |
| `409` | Mata uang bukan rupiah | Kiriman ulang akan ditolak lagi selama belum ada keputusan multi-mata uang |
| `422` | Pesan sah, tetapi jenisnya belum punya aturan posting, kode jenisnya belum terdaftar, atau membawa komponen yang tidak dikenal aturannya. Kejadian **tetap tersimpan** berstatus **Tertahan** dan **tidak ada jurnal yang dibuat** | Tidak perlu kirim ulang. Accounting yang melengkapi aturannya, lalu memproses ulang |

### 4a. Kapan jurnal dibuat — `ACC-DEC-084`

Jurnal dibuat **seketika di dalam request penerimaan**, dengan penjadwal sebagai cadangan.

1. Pesan divalidasi. Gagal validasi → `400` atau `409`, tidak ada yang tersimpan.
2. Kejadian disimpan berstatus `Diterima` dan **di-commit**.
3. Aturan posting dicocokkan.
4. Hasilnya salah satu dari tiga:

| Keadaan | Status | Balasan |
|---|---|---|
| Jurnal terbentuk | `Terjurnal` | `201` + `JournalNumber` |
| Kode belum terdaftar, aturan belum ada, atau komponen tidak dikenal | `Tertahan` | `422` + `HoldReasonCode` |
| Gangguan teknis setelah langkah 2 | Tetap `Diterima` | `201` **tanpa** `JournalNumber`; penjadwal mencoba ulang sampai 3 kali, lalu `Gagal` (`ACC-DEC-049`) |

**Contoh.** `EVT-300` datang saat database sibuk. Kejadian tersimpan, balasannya `201` dengan
`JournalNumber` kosong. Dua menit kemudian penjadwal berhasil membuat `JU/2026/11/00017`. Accounting
**tidak** mengabari Finance (Accounting tidak menerbitkan balik, `ACC-DEC-002`); bila Finance ingin
nomor itu, kiriman ulang `EVT-300` dijawab `200` beserta nomornya.

### 4b. Isi `AccountingEventReceiptDto` — `ACC-DEC-085`

Bentuknya sama pada `201`, `200`, dan `422`. Balasan `400`, `403`, `409`, dan `422` karena badan hukum tidak ditemukan
tidak membawanya, karena kejadiannya tidak tersimpan.

| Bidang | Tipe | Selalu terisi | Keterangan | Disimpan Finance sebagai |
|---|---|:---:|---|---|
| `AccountingEventId` | `Guid` | Ya | Rujukan tanda terima di Accounting (`AccAccountingEvent.Id`) | `AccountingReceiptNumber` |
| `EventNumber` | `string` | Ya | Gema dari pesan | — |
| `EventStatus` | `string` | Ya | `Diterima`, `Terjurnal`, `Tertahan`, atau `Tercatat` (pesan saldo subledger) | — |
| `JournalNumber` | `string?` (maks 30) | Tidak | Hanya bila `Terjurnal`, contoh `JU/2026/11/00017` | `AccountingJournalNumber` |
| `AccountingPeriodCode` | `string?` (`YYYY-MM`) | Tidak | Periode tempat jurnal jatuh — bisa berbeda dari `AccountingDate` bila periodenya sudah tertutup (`ACC-DEC-047`) | — |
| `HoldReasonCode` | `string?` | Tidak | Hanya bila `Tertahan`: `EVENT_TYPE_NOT_REGISTERED`, `POSTING_RULE_MISSING`, `COMPONENT_UNMAPPED` (kejadian membawa komponen yang tidak dipakai aturan), atau `COMPONENT_MISSING` (aturan menuntut komponen yang tidak dibawa kejadian) | Dapat dipakai mengisi `HoldReason` |
| `ReceivedAt` | `timestamptz` | Ya | Waktu kejadian pertama kali diterima | — |

```json
{
  "AccountingEventId": "5b0f7a9e-3c21-4d8e-9f10-2a7c6b5d4e31",
  "EventNumber": "EVT-210",
  "EventStatus": "Tertahan",
  "JournalNumber": null,
  "AccountingPeriodCode": null,
  "HoldReasonCode": "POSTING_RULE_MISSING",
  "ReceivedAt": "2026-11-03T09:12:44+07:00"
}
```

### 4c. Akun layanan penerbit — `ACC-DEC-088`

Syarat yang ditetapkan Accounting:

| # | Syarat | Akibat bila dilanggar |
|---:|---|---|
| 1 | Satu akun khusus untuk Finance — bukan akun manusia, **bukan SuperAdmin** | — (dilarang) |
| 2 | Punya penugasan **Departemen + Jabatan khusus** (misalnya "Integrasi Sistem") yang di `SysAccessPolicy` **hanya** diberi `AccountingEvent : Receive` | Tanpa penugasan organisasi, setiap kiriman `403` — hak Accounting diberikan per Departemen + Jabatan |
| 3 | Berhak atas badan hukum yang dikirimi kejadian | `403` |
| 4 | Token berumur pendek — **preferensi**, bukan syarat | — |

**Mekanismenya belum diputuskan** (`CROSS_MODULE_DECISION_REQUIRED` — Platform, Yasmin, Rizki).
Finance mengusulkan mekanisme auth existing (`FIN-DEC-007`, `draft`). Keterbukaan ini menahan
pengaktifan pengiriman (gerbang G3), bukan pembangunan kotak masuk.

## 5. Yang Accounting jamin kepada penerbit

| Jaminan | Isi |
|---|---|
| Anti-ganda lapis pertama | `EventNumber` yang sama datang berkali-kali menghasilkan **tepat satu** jurnal. Kiriman berikutnya dijawab `200` beserta nomor jurnal yang sama |
| Anti-ganda lapis kedua | Gabungan `SourceModule` + `SourceTransactionId` + `EventTypeCode` + `SourceVersion` juga unik — jaring pengaman bila penerbit keliru membuat `EventNumber` baru untuk kejadian yang sama (`ACC-DEC-035`). Memakai **kode** jenis yang tersimpan, bukan id master, supaya kejadian berjenis yang belum terdaftar tetap tertangkap sebagai kiriman ulang (`ACC-DEC-075`) |
| Tidak ada angka yang dibuang | Kejadian yang belum dapat dijurnal **ditahan**, tidak dibuang, dan tidak dipindahkan ke akun sementara (`ACC-DEC-046`) |
| Penelusuran balik | Dari baris buku besar mana pun dapat ditelusuri sampai `SourceTransactionId` dan `CorrelationId` |
| Tidak menerbitkan balik | Accounting adalah muara; ia **tidak** menerbitkan kejadian ke modul lain (`ACC-DEC-002`) |
| Tidak menyentuh tabel Finance | Accounting tidak membaca dan tidak menulis tabel Finance, dan tidak menarik data lewat API Finance (`ACC-DEC-003`, `ACC-DEC-071`) |

**Contoh lapis kedua.** Finance mengirim `EVT-100` untuk transaksi `AR-2026-09-00871` versi `1`.
Karena gangguan, sistem Finance mengirim ulang kejadian yang sama tetapi dengan nomor `EVT-101`.
Lapis pertama tidak menangkapnya karena nomornya berbeda; lapis kedua menangkapnya karena modul,
transaksi, kode jenis, dan versinya sama. Buku besar tetap berisi satu jurnal.

## 6. Perlakuan setiap keadaan

Diambil dari `ACC-STATE` Phase 2 bagian 1 dan `integration-contract.md` bagian 6.5.

| Keadaan | Status kejadian | Perlakuan | Dasar |
|---|---|---|---|
| Pesan sah, aturan posting ada, periode menerima pencatatan | `Diterima` → **`Terjurnal`** | Jurnal dibuat — langsung disahkan atau berupa draft untuk diperiksa, menurut perlakuan yang ditetapkan pada aturan posting jenis itu | `ACC-DEC-045` |
| Jenis belum punya aturan posting, atau kode jenis belum terdaftar | `Diterima` → **`Tertahan`** | Nol jurnal. Diproses ulang begitu aturannya dilengkapi Accounting | `ACC-DEC-046`, `075` |
| Gangguan teknis saat memproses | Dicoba ulang 3 kali dengan jeda makin panjang, lalu **`Gagal`** | Accounting Manager diberi tahu; dapat dicoba ulang manual | `ACC-DEC-049` |
| Kejadian `Gagal` dinyatakan tidak perlu dijurnal | `Gagal` → **`Diabaikan`** | Alasan tertulis wajib. Kejadian `Tertahan` **tidak boleh** diabaikan | `ACC-DEC-078` |
| Periode akuntansinya sudah tertutup | Tetap diproses | Jurnal dicatat pada **periode terbuka berikutnya**; tanggal dokumen asli tetap disimpan. Periode tertutup **tidak** dibuka otomatis | `ACC-DEC-047` |
| Pesan tidak lengkap | — | Ditolak `400`, tidak tersimpan | `ACC-DEC-048` |
| Mata uang bukan rupiah | — | Ditolak `409`, tidak tersimpan sebagai kejadian | `ACC-DEC-020` |

**Dampak ke tutup bulan.** Kejadian `Gagal` **menahan** penutupan periode, sedangkan kejadian
`Tertahan` hanya **memperingatkan** (`ACC-DEC-051`).

## 7. Mata uang — `ACC-DEC-020` dan `ACC-DEC-021`

| Aspek | Ketentuan |
|---|---|
| Base currency | `IDR` |
| Mata uang yang diterima | `IDR` **saja** |
| Keseimbangan debit = kredit | Diukur dalam `IDR` (`ACC-DEC-021`) |

`DEFERRED` — tidak dirancang dalam bentuk apa pun: posting multi-mata uang, kurs, selisih kurs
terealisasi, selisih kurs belum terealisasi, dan revaluasi mata uang asing.

## 8. Saldo subledger per periode — untuk rekonsiliasi

Diputuskan `ACC-DEC-071`, 10 September 2026:

| Ketetapan | Isi |
|---|---|
| Waktu | Pada cut-off setiap periode akuntansi, bukan langsung |
| Penerbit | **Finance**, sebagai saldo subledger **final** setiap control account pada periode itu |
| Isi minimum | `LegalEntity`, `AccountingPeriod`, `ControlAccount`, `SubledgerBalance`, `AsOfDate` |
| Cara sampai | Lewat pintu masuk kejadian yang sama. Accounting **tidak** menarik data dari Finance |
| Akibat bagi Accounting | Penutupan periode tidak dianggap selesai bila saldo belum diterima atau belum cocok; toleransi selisih nol (`ACC-DEC-076`) |

### Bentuk rinci — `ACC-DEC-087`

Memakai **amplop dua belas bidang yang sama** (bagian 3), ditambah dua rincian. Dengan begitu
anti-ganda, koreksi lewat `SourceVersion`, dan penelusuran berlaku tanpa aturan baru.

| Bidang | Isi untuk pesan saldo |
|---|---|
| `EventTypeCode` | Kode khusus saldo — **usulan `SALDO-SUBLEDGER`**, menunggu persetujuan Finance |
| `SourceTransactionId` | Rujukan dokumen tutup periode di Finance |
| `SourceVersion` | Naik bila Finance menyatakan ulang saldo periode yang sama |
| `AccountingDate` | **Tanggal cut-off** (pengganti `AsOfDate` usulan Finance) |
| `Amount` | **Saldo subledger.** Khusus jenis ini boleh **nol atau negatif** |
| Rincian `AccountingPeriodCode` | `string`, **maks 7**, bentuk `YYYY-MM` — sama dengan `AccAccountingPeriod.PeriodCode` |
| Rincian `ControlAccountCode` | `string`, maks 50 — wajib akun yang ditandai control account |

`SubledgerBalance` dan `AsOfDate` usulan Finance (`FIN-DEC-023`) **tidak** diulang: nilainya sudah
dibawa `Amount` dan `AccountingDate`, dan bidang kembar hanya membuka kemungkinan keduanya berbeda.

```json
{
  "EventNumber": "EVT-SL-2026-11-001",
  "EventTypeCode": "SALDO-SUBLEDGER",
  "SourceModule": "Finance",
  "SourceTransactionId": "FIN-CLOSE-2026-11",
  "SourceVersion": "1",
  "EventOccurredAt": "2026-12-01T08:00:00+07:00",
  "AccountingDate": "2026-11-30",
  "Amount": 425000000.00,
  "CurrencyCode": "IDR",
  "LegalEntityId": "11111111-2222-4333-8444-555555555555",
  "CorrelationId": "3e1d2c0b-9a8f-4e7d-8c6b-5a4f3e2d1c0b",
  "CausationId": "3e1d2c0b-9a8f-4e7d-8c6b-5a4f3e2d1c0b",
  "SubledgerBalance": {
    "AccountingPeriodCode": "2026-11",
    "ControlAccountCode": "1-1201"
  }
}
```

**Kejadian saldo tidak pernah menghasilkan jurnal.** Ia dipakai penghalang rekonsiliasi
`ACC-DEC-076`. Letak dua rincian di dalam pesan (objek `SubledgerBalance` di atas), status, dan
tempat simpannya dirancang di `02-backend-architecture.md` dan `api-contract.md`.

## 9. Yang masih terbuka

Per 24 September 2026. Yang sudah ditutup: `ACC-XM-001` dan ratifikasi bentuk pesan (`ACC-DEC-082`),
`DEC-ACC-P2-002` (`ACC-DEC-083`), `OD-ACC-01` (gugur), `OD-ACC-08` sisi Accounting (`ACC-DEC-087`).

| Butir | Pertanyaan | Pemilik | Menahan |
|---|---|---|---|
| `OD-ACC-05` sisa | Mekanisme autentikasi akun layanan | Platform + Yasmin + Rizki | Gerbang G3 |
| Kode saldo | Persetujuan kode `SALDO-SUBLEDGER` dan penyesuaian `FIN-DEC-023` ke bagian 8 | Yasmin | Wave D |
| Komponen | Komponen yang dikirim per jenis kejadian | Yasmin | Aturan posting berbaris banyak |
| Kas di luar katalog | Kode atau pernyataan tertulis untuk deposit, refund, selisih shift | Yasmin + owner Billing | Gerbang G6 |
| Selisih catatan | Versi percakapan (JWT Bearer, cutover 1 Oktober, amplop + 4 rincian) berbeda dengan `FIN-DEC-007`, `008`, `023` | Yasmin | Tidak menahan; wajib didamaikan (bagian 11) |
| `FIN-DEC-004` | Accounting meminta penerimaan sebelum tagihan final **terbit segera** sebagai uang muka pasien, beserta kode pemakaian uang muka (`ACC-DEC-091`) | Yasmin | Gerbang G6 |

## 10. Yang sudah dan belum dibangun sisi Accounting

| Bagian | Keadaan per 24 September 2026 |
|---|---|
| Master jenis kejadian (`GET/POST/PUT/PATCH api/v1/corporate/accounting/event-types`) | **Sudah berdiri** — `BE-ACC-P2-017`, terbukti dipanggil 15 September 2026 |
| Master aturan posting (`api/v1/corporate/accounting/posting-rules`) | **Sudah berdiri** — `BE-ACC-P2-018` |
| Pintu masuk `POST /accounting-events`, status kejadian, coba ulang, daftar gagal | **Belum** — **boleh dibangun** sejak `ACC-DEC-082`; direncanakan sebagai Wave B |
| Penerimaan saldo subledger dan penghalang rekonsiliasi | **Belum** — Wave D, menunggu kode jenis saldo dari Finance |

## 11. Aturan referensi revisi

Supaya ketidakcocokan kontrak terlihat sebelum menjadi bug, kedua sisi mencatat revisi yang
mereka pakai.

```
Finance Blueprint rev X
        └── depends_on:  ACC-XMOD-<versi> APPROVED

Accounting Blueprint rev Z
        └── provides:    ACC-XMOD-<versi> APPROVED
        └── depends_on:  FIN-XMOD-<versi> APPROVED   (bila Finance menerbitkannya)
```

| Kewajiban | Siapa |
|---|---|
| Membaca `ACC-XMOD` revisi `APPROVED` terakhir sebelum mengunci integrasi AR → Accounting, AP → Accounting, settlement yang berdampak Accounting, atau migration/artefak yang bergantung Accounting | Agent Finance / Yasmin |
| Membaca kontrak cross-module Finance revisi `APPROVED` terakhir sebelum implementasi Finance → Accounting | Agent Accounting / Rizki |
| Menaikkan `ACC-XMOD` pada perubahan yang sama setiap kali `api-contract.md` bagian *Isi `ReceiveAccountingEventRequest`* atau `integration-contract.md` bagian 6 berubah | Agent Accounting / Rizki |

Bila versi yang tercatat pada satu sisi bukan versi `APPROVED` terakhir sisi lain, itu
**contract mismatch** dan pekerjaan integrasi berhenti sampai didamaikan.

**Pelajaran dari `0.1`.** Baris ketiga tabel di atas baru ditambahkan pada `0.2`. Tanpanya,
`ACC-API` bergerak dari `0.5` ke `0.10` sementara berkas ini diam di `0.1`, dan sejak 8 September
2026 dua kontrak untuk pembaca yang sama berbeda isi.

## 12. Yang wajib dibaca Finance, dan yang tidak

Agent Finance **tidak perlu** membaca seluruh `docs/module-blueprints/accounting/`. Daftar
artefak yang wajib dibaca beserta klasifikasinya ada di
[../blueprint-manifest.md](../blueprint-manifest.md) bagian *Klasifikasi artefak*. Ringkasan satu
berkas untuk owner Finance ada di
[../evidence/12-paket-kontrak-kejadian-untuk-finance.md](../evidence/12-paket-kontrak-kejadian-untuk-finance.md).

## 13. Gerbang cutover — `ACC-DEC-089` dan `ACC-DEC-090`

Pengiriman kejadian mulai berlaku pada **tanggal 1 pukul 00.00 WIB di awal periode akuntansi
pertama setelah keenam gerbang lolos**. Tanggal pastinya diputuskan Rizki bersama Yasmin saat
gerbang terakhir lolos. Transaksi sebelum tanggal itu tidak dikirim dan tidak direkonstruksi; saldo
sebelum cutover diinput Accounting sebagai saldo awal manual, satu kali (`FIN-DEC-008`).

| Gerbang | Syarat | Pemilik | Keadaan 24 September 2026 |
|---|---|---|---|
| G1 | Kotak masuk dibangun dan diuji ujung-ke-ujung | Rizki | Nol kode |
| G2 | `ACC-TD-022` ditutup (bagan akun sah) dan aturan posting untuk setiap kode yang akan aktif tersusun | Pemilik proses akuntansi + Rizki | `OPEN` |
| G3 | Akun layanan aktif sesuai bagian 4c dan mekanismenya sudah diputuskan | Platform + Yasmin + Rizki | Mekanisme terbuka |
| G4 | Pengirim Finance siap | Yasmin | Belum dibangun |
| G5 | Saldo awal manual per tanggal cutover siap diinput | Rizki | Bergantung G2 |
| G6 | Kode atau pernyataan tertulis Finance untuk deposit pasien, kelebihan bayar dan refund, serta selisih kas shift — termasuk jaminan top-up deposit **tidak** dikirim sebagai `PENERIMAAN-KASIR`; ditambah perubahan `FIN-DEC-004` dan kode pemakaian uang muka (`ACC-DEC-091`) | Yasmin + owner Billing | Terbuka |

**1 Oktober 2026 tidak layak** karena G1, G2, G3, dan G4 belum terpenuhi tujuh hari sebelumnya.

**Kenapa G6 menjadi gerbang.** Top-up deposit Rp 5.000.000 yang terkirim sebagai `PENERIMAAN-KASIR`
dapat terbukukan sebagai pendapatan, padahal uang itu kewajiban kepada pasien. Buku besar tetap
seimbang dan tidak ada pesan galat, sehingga kesalahannya baru ketahuan saat pemeriksaan.
