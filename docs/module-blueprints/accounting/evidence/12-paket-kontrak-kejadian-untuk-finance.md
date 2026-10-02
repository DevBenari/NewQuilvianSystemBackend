# Paket Kontrak untuk Owner Finance — Kejadian Keuangan Finance → Accounting

| Field | Nilai |
|---|---|
| Dari | Rizki, owner modul Accounting |
| Untuk | Yasmin, owner / penggarap modul Finance (AR/AP) |
| Tanggal | 15 September 2026 |
| Sifat | Bahan ratifikasi dan enam pertanyaan. **Tidak** meminta Finance menulis kode apa pun sekarang |
| Kontrak yang diringkas | `ACC-XMOD-0.2` ([cross-module-contract.md](../contracts/cross-module-contract.md)), `ACC-INTEGRATION-0.4` bagian 6 ([integration-contract.md](../contracts/integration-contract.md)), `ACC-API-0.10` grup Accounting Event ([api-contract.md](../contracts/api-contract.md)) |
| Bila berbeda | Ketiga berkas kontrak di atas yang berlaku, bukan ringkasan ini |

Berkas ini sengaja berdiri sendiri, supaya dapat dibaca tanpa membuka dokumen blueprint Accounting
yang lain.

---

## 1. Kenapa dikirim sekarang

Accounting sudah menyelesaikan seluruh pekerjaan yang **tidak** bergantung pada Finance: jurnal,
buku besar, tutup bulan, tutup tahun, serta master **Jenis Kejadian** dan **Aturan Posting**. Yang
tersisa adalah menerima kejadian keuangan dari Finance lalu mengubahnya menjadi jurnal otomatis.

Bagian itu **sengaja belum dibangun**. Kami ingin bentuk pesannya disepakati lebih dahulu, supaya
Finance dan Accounting tidak masing-masing membangun versi yang berbeda lalu salah satunya harus
dibongkar.

**Satu koreksi yang perlu diketahui.** Bila Anda pernah membaca `cross-module-contract.md` versi
`0.1` (1 September 2026), bentuk pesan di sana **sudah usang**: `EventId` bertipe `Guid`,
`SourceDomain`, `IdempotencyKey`, dan tanpa `EventOccurredAt` serta `LegalEntityId`. Pesan dengan
bentuk itu akan ditolak. Yang berlaku adalah bagian 3 di bawah.

## 2. Yang sudah pasti

| Hal | Ketentuan | Dasar |
|---|---|---|
| Arah | **Satu arah**: Billing → Finance → Accounting | `ACC-DEC-044` |
| Penerbit kejadian | **Finance**, sebagai kejadian tersendiri — bukan meneruskan `BilArHandoff` | `ACC-DEC-044`, dikonfirmasi owner Billing `ACC-DEC-059` |
| Accounting membaca tabel Billing atau Finance | **Tidak pernah.** Accounting juga tidak menarik data lewat API Finance | `ACC-DEC-003`, `059`, `071` |
| Pintu masuk | `POST api/v1/corporate/accounting/accounting-events`, satu kejadian per permintaan | `ACC-INTEGRATION` 6.2 |
| Mata uang | `IDR` saja | `ACC-DEC-020` |
| Data pasien | **Dilarang** ada di pesan | `ACC-DEC-056` |

**Yang belum pasti, dan butuh jawaban Anda,** ada di bagian 8.

## 3. Bentuk pesan — dua belas bidang wajib

| Bidang | Tipe | Contoh | Kegunaan |
|---|---|---|---|
| `EventNumber` | `string` | `EVT-100` | Nomor unik kejadian, dibuat Finance |
| `EventTypeCode` | `string`, maks 50 | `PENGAKUAN-PIUTANG` | Jenis kejadian; menentukan akun mana yang dipakai |
| `SourceModule` | `string` | `Finance` | Modul asal |
| `SourceTransactionId` | `string` | `AR-2026-09-00871` | Nomor transaksi di Finance |
| `SourceVersion` | `string` | `1` | Versi transaksi; dinaikkan bila transaksinya dikoreksi |
| `EventOccurredAt` | tanggal-waktu berzona | `2026-09-08T10:15:00+07:00` | Waktu kejadian sebenarnya |
| `AccountingDate` | tanggal | `2026-09-08` | Tanggal pembukuan; menentukan periode |
| `Amount` | `decimal(18,2)` | `10000000.00` | Nilai total |
| `CurrencyCode` | `string` | `IDR` | Hanya `IDR` |
| `LegalEntityId` | `Guid` | Id badan hukum | Buku badan hukum mana yang disentuh |
| `CorrelationId` | `Guid` | — | Menghubungkan kejadian ini sampai ke faktur Billing asalnya |
| `CausationId` | `Guid` | — | Tindakan yang menyebabkan kejadian ini |

Satu bidang **opsional**: `Components` — daftar `{ ComponentCode, Amount }` untuk merinci nilai.
Tanpa `Components`, seluruh nilai dianggap komponen `TOTAL`.

**Kenapa `CorrelationId` dan `CausationId` wajib.** Owner Billing menyatakan pada 9 September 2026
bahwa kejadian Finance memang membawa keduanya. Tanpa disimpan Accounting, pertanyaan *"jurnal ini
dari tagihan pasien yang mana"* menuntut membuka Finance lebih dahulu hanya untuk mencari nomor
fakturnya.

### Contoh — pendapatan rawat jalan dengan jasa medis dokter

> **Koreksi 24 September 2026 — contoh ini tidak berlaku lagi.** Finance memisahkan jasa medis dari
> pengakuan piutang (`FIN-DEC-003`), sehingga `PENGAKUAN-PIUTANG` **tidak pernah** membawa komponen
> `JASA_MEDIS`; jasa medis datang lewat `PENGAKUAN-HUTANG-DOKTER`. Contoh yang berlaku ada di
> [`cross-module-contract.md`](../contracts/cross-module-contract.md) bagian 3 (`ACC-XMOD-0.3`,
> `ACC-DEC-086`). Teks di bawah dibiarkan apa adanya karena inilah yang dikirim ke Finance
> 15 September 2026. Jawaban Finance dan balasan Accounting ada di
> [`13-balasan-accounting-untuk-finance.md`](13-balasan-accounting-untuk-finance.md).

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
  "CausationId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
  "Components": [
    { "ComponentCode": "JASA_MEDIS", "Amount": 3000000.00 }
  ]
}
```

Seluruh id di atas hanya contoh. Bila aturan posting jenis itu memuat empat baris — dua memakai
`TOTAL`, dua memakai `JASA_MEDIS` — pesan ini menghasilkan satu jurnal empat baris senilai debit
Rp 13.000.000 lawan kredit Rp 13.000.000, misalnya piutang dan pendapatan Rp 10.000.000 ditambah
beban jasa medis dan utang jasa dokter Rp 3.000.000. Pemetaan ke akun
dikerjakan Accounting lewat master Aturan Posting — **Finance tidak perlu tahu nomor akunnya**.

## 4. Jawaban Accounting atas setiap kiriman

| Kode | Artinya | Yang perlu Finance lakukan |
|---|---|---|
| `201` | Kejadian baru diterima | Tidak ada |
| `200` | Kejadian ini **sudah pernah diterima**; nomor jurnal yang sama dikembalikan | Tidak ada. Aman untuk kiriman ulang |
| `400` | Ada bidang wajib yang kosong atau tidak masuk akal | Perbaiki pesan, lalu kirim ulang |
| `403` | Akun pengirim tidak berhak | Periksa akun layanan Finance |
| `409` | Mata uang bukan rupiah | Kiriman ulang akan ditolak lagi |
| `422` | Pesan sah, tetapi Accounting belum punya aturan untuk jenis atau komponennya. Kejadian **tetap tersimpan** berstatus **Tertahan** | Tidak perlu kirim ulang. Accounting yang melengkapi aturannya |

## 5. Pencegahan pencatatan ganda

Dua kunci dipakai bersamaan:

| Lapis | Kunci | Menangkap |
|---|---|---|
| Pertama | `EventNumber` | Pesan yang sama terkirim berulang karena gangguan jaringan |
| Kedua | `SourceModule` + `SourceTransactionId` + `EventTypeCode` + `SourceVersion` | Sistem Finance keliru membuat **nomor baru** untuk kejadian yang sama |

**Contoh.** Finance mengirim `EVT-100` untuk `AR-2026-09-00871` versi `1`, lalu karena gangguan
mengirim ulang kejadian yang sama sebagai `EVT-101`. Lapis kedua menangkapnya. Buku besar tetap
berisi satu jurnal.

**Yang diharapkan dari Finance:** koreksi atas transaksi yang sama dikirim dengan `SourceVersion`
yang dinaikkan, bukan dengan `SourceVersion` yang sama. Bila versinya tetap sama, koreksi itu
terbaca sebagai kiriman ulang dan tidak dijurnal.

## 6. Setelah kejadian diterima

| Keadaan | Yang terjadi di Accounting |
|---|---|
| Aturan posting ada | Jurnal dibuat — langsung disahkan atau berupa draft, sesuai aturan jenis itu |
| Aturan posting belum ada | Kejadian **Tertahan**, nol jurnal, menunggu Accounting |
| Gangguan teknis | Dicoba ulang otomatis 3 kali, lalu masuk daftar gagal dan Accounting Manager diberi tahu |
| Periode akuntansinya sudah ditutup | **Tidak ditolak.** Jurnal dicatat pada periode terbuka berikutnya; tanggal dokumen aslinya tetap disimpan |

`201` berarti kejadian sudah tercatat di Accounting. Status pembukuannya — terjurnal, tertahan,
atau gagal — dilihat dan ditangani di sisi Accounting.

## 7. Saldo subledger per periode

Untuk rekonsiliasi akun piutang dan utang, Accounting membutuhkan **saldo subledger final** dari
Finance pada setiap cut-off periode akuntansi (`ACC-DEC-071`). Isi minimumnya:

| Isi | Contoh |
|---|---|
| Badan hukum | PT Metropolitan Medical Centre |
| Periode akuntansi | September 2026 |
| Control account | Piutang Penjamin |
| Saldo subledger | Rp 425.000.000 |
| Per tanggal | 30 September 2026 |

Saldo ini dikirim lewat pintu masuk yang sama, sebagai kejadian. Accounting **tidak** akan menarik
data dari Finance. Begitu fitur rekonsiliasi ini dibangun, penutupan bulan Accounting tertahan
bila saldo belum diterima atau berselisih dengan buku besar — tanpa toleransi selisih
(`ACC-DEC-076`).

## 8. Enam pertanyaan untuk Finance

| # | Pertanyaan | Kenapa penting | Rujukan |
|---:|---|---|---|
| 1 | Apakah Finance **meratifikasi** rantai Billing → Finance → Accounting dengan Finance sebagai penerbit? | Sisi Accounting dan Billing sudah setuju. Tanpa ratifikasi Finance, Accounting belum boleh menulis kode penerimaannya | `ACC-XM-001` |
| 2 | Apakah **bentuk pesan** pada bagian 3 dapat dipenuhi Finance? Bila ada bidang yang tidak dimiliki Finance, bidang mana? | Bentuk pesan dikunci bersama, bukan didikte satu pihak | `ACC-DEC-048`, `060` |
| 3 | **Jenis kejadian apa saja** yang akan diterbitkan Finance, beserta kodenya? | Accounting perlu mendaftarkannya dan menyusun aturan posting sebelum kejadian pertama tiba. Kejadian berjenis yang belum terdaftar akan Tertahan | `DEC-ACC-P2-002` |
| 4 | Bagaimana **akun layanan** Finance akan login saat mengirim kejadian? | Pintu masuk hanya menerima akun ber-hak `AccountingEvent : Receive`; cara autentikasinya belum disepakati | `OD-ACC-05` |
| 5 | Bagaimana **bentuk rinci** pesan saldo subledger per periode — nama bidang dan tipenya? | Isi minimumnya sudah pasti (bagian 7), nama bidangnya belum | `OD-ACC-08` |
| 6 | Sejak **tanggal berapa** transaksi Finance mulai dikirim sebagai kejadian, dan bagaimana saldo sebelum tanggal itu masuk ke buku Accounting? | Tanpa titik mulai yang disepakati, transaksi di sekitar tanggal itu dapat terjurnal dua kali atau tidak terjurnal sama sekali | Pertanyaan baru, belum tercatat sebagai keputusan |

## 9. Yang kami jaga dari sisi Accounting

| Hal | Ketentuan |
|---|---|
| Data pribadi | Accounting **tidak menyimpan** nama pasien, nomor rekam medis, nomor kunjungan, maupun `DoctorId` |
| Lifecycle Finance | Accounting **tidak menentukan** kapan AR dianggap diakui atau kapan Finance menerbitkan kejadian — itu wewenang Finance |
| Arah | Accounting adalah muara. Ia tidak menerbitkan kejadian balik ke Finance |
| Perubahan kontrak | Setiap perubahan bentuk pesan menaikkan versi `ACC-XMOD` pada perubahan yang sama, sehingga Finance dapat mencocokkan versi yang dipakainya (`cross-module-contract.md` bagian 11) |

## 10. Rujukan

| Berkas | Isi |
|---|---|
| [contracts/cross-module-contract.md](../contracts/cross-module-contract.md) | Kontrak lengkap `ACC-XMOD-0.2`, termasuk pemetaan bidang dari `0.1` |
| [contracts/integration-contract.md](../contracts/integration-contract.md) bagian 6 | Arah, pintu masuk, anti-ganda, perilaku gagal — `ACC-INTEGRATION-0.4` |
| [contracts/api-contract.md](../contracts/api-contract.md) grup *Accounting Event* dan bagian *Isi `ReceiveAccountingEventRequest`* | Endpoint dan kode status |
| [06-shared-migration-coordination-rule.md](../06-shared-migration-coordination-rule.md) | Aturan koordinasi migration bila kedua modul menambah tabel pada waktu berdekatan |
