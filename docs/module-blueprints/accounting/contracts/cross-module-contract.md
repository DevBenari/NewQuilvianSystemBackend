# Accounting — Cross-Module Contract (Finance / AR / AP → Accounting)

| Field | Value |
|---|---|
| `contract_version` | `ACC-XMOD-0.7` |
| `last_changed_in` | `ACC-XMOD-0.7` — 5 Oktober 2026. Sebelumnya `0.6`, 1 Oktober 2026; `0.5`, 30 September 2026; `0.4`, 28 September 2026; `0.3`, 24 September 2026; `0.2`, 15 September 2026 |
| Amandemen `0.4` | **`ACC-XMOD-0.4` — approved sisi Accounting, Rizki, 28 September 2026 (`GATE-DESAIN-0928`).** (1) Katalog bagian 3a menjadi **26 kode** (`ACC-DEC-087`, `098`, `100`, `103`, `105`, `106`); (2) empat aturan pesan saldo untuk rekonsiliasi di bagian 8a — titik mulai, akun wajib, arah tanda `Amount`, tanggal cut-off (`ACC-DEC-107`..`110`); (3) bagian 9 dan 10 dimutakhirkan. **Nol perubahan pada kedua belas bidang, tipe, kunci anti-ganda, kode balasan, dan validasi penerimaan.** Butir (2) menunggu kesanggupan Finance (bagian 9), mengikuti preseden `0.3`: diputuskan sisi Accounting, dikonfirmasi Finance per butir |
| Amandemen `0.5` | **`ACC-XMOD-0.5` — approved sisi Accounting, Rizki, 30 September 2026 (`GATE-DESAIN-0930`).** (1) Gerbang G6 bagian 13 bertambah syarat penegakan shift kasir belum ditutup (`ACC-DEC-124`); (2) bagian 9 bertambah butir shift kasir (OQ-124-1); (3) bagian 4b diselaraskan dengan `api-contract.md` — `EventStatus` `Gagal`/`Diabaikan` dan nomor jurnal yang dapat berganti (`ACC-DEC-116`). **Nol perubahan pada kedua belas bidang, tipe, kunci anti-ganda, kode balasan, dan validasi penerimaan**; pengirim dari `0.4` tetap cocok |
| Amandemen `0.6` | **`ACC-XMOD-0.6` — 1 Oktober 2026, menuliskan keputusan owner `ACC-DEC-125`..`131` yang sudah `approved`** (preseden `ACC-DEC-074`: penyesuaian atas keputusan owner tidak menunggu gerbang tersendiri). (1) Katalog bagian 3a menjadi **34 kode aktif** ditambah dua kode penanda yang aktif sesudah G6; (2) bagian 3a.3 dan 9 dimutakhirkan dengan jawaban Finance (`finance-management/evidence/15`, `21`) dan lima pertanyaan balik `evidence/16` Accounting; (3) gerbang G6 bagian 13 bertambah keputusan akun debit refund `REFERRED_OUTPATIENT_ADMIN` dan penerimaan nilai nol untuk kode penanda. **Nol perubahan pada kedua belas bidang, tipe, kunci anti-ganda, kode balasan, dan validasi penerimaan** |
| Amandemen `0.7` | **`ACC-XMOD-0.7` — 5 Oktober 2026, menuliskan keputusan owner `ACC-DEC-132`..`140` yang sudah `approved`** (preseden `ACC-DEC-074`), menjawab `finance-management/evidence/22` dan `evidence/17`; dikirim bersama surat `evidence/18`. (1) Katalog bagian 3a: penanda ketiga `PEMBUKAAN-SHIFT-KASIR` (`ACC-DEC-133`, bagian 3a.2b), dua belas kode piutang sewa usulan Accounting (`ACC-DEC-136`..`140`, bagian 3a.2c), dan pemisahan per metode bayar untuk empat kode yang membawa `PaymentMethodCode` (`ACC-DEC-132`, bagian 3a.2d); (2) bagian 3a.3 dan 9 dimutakhirkan dengan jawaban Finance dan empat permintaan `evidence/18`; (3) bagian 13: G4 **bersyarat**, keadaan gerbang per 5 Oktober 2026, dan daftar periksa saldo periode pertama sesudah cutover (`ACC-DEC-135`); (4) kebersihan dokumen: baris *Implementasi*, bagian 4, dan bagian 10 diselaraskan dengan keadaan terbangun per 5 Oktober 2026, dan bagian 8 menegaskan `SourceTransactionId` pesan saldo unik per akun per periode, mengikuti format yang sudah dipakai Finance. **Nol perubahan pada kedua belas bidang, tipe, kunci anti-ganda, kode balasan, dan validasi penerimaan.** Lima ruas dimensi Finance (`FIN-INTEGRATION-1.7` bagian 5.12.1) diterima sebagai ruas tambahan untuk penelusuran dan **tidak** dibaca aturan posting |
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
| Implementasi | Kotak masuk kejadian **sudah dibangun** — Wave B tuntas 28 September 2026, rekonsiliasi saldo subledger 29 September 2026 — dan sudah di integration; UAT belum (bagian 10). *(`0.7`; sebelumnya tertulis "belum dibangun", basi sejak 28 September 2026.)* Pengaktifan pengiriman menunggu gerbang cutover (bagian 13) |

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

## 0c. Perubahan `0.3` → `0.4`

| Bagian | `0.3` | `0.4` | Dasar |
|---|---|---|---|
| Katalog jenis kejadian | 17 kode | **26 kode**; lima kebutuhan menunggu nama atau usulan Finance (bagian 3a.3) | `ACC-DEC-087`, `098`..`106` |
| Arah tanda `Amount` pesan saldo | Tidak diatur | **Menurut saldo normal akun** — positif saat wajar, termasuk akun bersaldo normal kredit (bagian 8a) | `ACC-DEC-109` |
| Tanggal cut-off pesan saldo | "Tanggal cut-off" | **Tanggal akhir periode**; tanggal lain diterima tetapi dianggap belum lengkap saat rekonsiliasi (bagian 8a) | `ACC-DEC-110` |
| Akun yang wajib dikirimi saldo | Tidak diatur | Setiap control account aktif yang menerima jurnal, **termasuk yang bersaldo nol** (bagian 8a) | `ACC-DEC-108` |
| Kapan rekonsiliasi menahan tutup bulan Accounting | "Saldo belum diterima atau belum cocok" | Sejak periode **saldo pertama** Finance diterima (bagian 8a) | `ACC-DEC-107` |

**Penerbit yang sudah dibangun dari `0.3` tetap cocok.** Tidak ada bidang baru dan tidak ada
penolakan baru; yang berubah hanya **isi** yang diharapkan pada pesan saldo.

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

## 3a. Katalog jenis kejadian — 34 kode aktif, ditambah 3 kode penanda dan 12 kode sewa usulan *(`0.7`; 34 kode dan 2 penanda pada `0.6`, 26 kode pada `0.4`, 17 kode pada `0.3`)*

Diratifikasi `ACC-DEC-083` atas `FIN-DEC-002` (17 kode pertama), lalu diperluas `ACC-DEC-087`,
`098`, `100`, `103`, `105`, dan `106` menjadi 26 kode, lalu oleh `ACC-DEC-099`, `104`, dan `125` menjadi **34 kode** (bagian 3a.2b). *(`0.7`:)* Ditambah penanda ketiga `PEMBUKAAN-SHIFT-KASIR` (`ACC-DEC-133`), dua belas kode piutang sewa yang **diusulkan** Accounting dan menunggu nama final Finance (`ACC-DEC-136`, bagian 3a.2c), dan pemisahan per metode bayar untuk empat kode yang membawa `PaymentMethodCode` (`ACC-DEC-132`, bagian 3a.2d). `SourceModule` seluruhnya `Finance`. Kode
baru hanya lewat keputusan kedua pihak. Kode yang sah tetapi belum punya aturan posting tetap
**Tertahan** — ratifikasi kode bukan aturan posting. **Nomor urut tidak mengikat; identitas kejadian
adalah kodenya** (`evidence/14` bagian 2).

### 3a.1 Tujuh belas kode pertama — `ACC-DEC-083`

| Kode | Dipicu oleh (menurut Finance) | Gelombang Finance |
|---|---|---|
| `PENGAKUAN-PIUTANG` | Piutang diakui dari fakta AR Billing | MVP-1 |
| `PENERIMAAN-KASIR` | Penerimaan dari tender Billing yang **final**; penerimaan sebelum tagihan final memakai `PENERIMAAN-UANG-MUKA` (`ACC-DEC-091`, `FIN-DEC-030`) | MVP-2 |
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

### 3a.2 Sembilan kode tambahan — diratifikasi 24 dan 28 September 2026

Gelombang Finance untuk kode-kode ini tidak dicatat di sini; lihat decision log Finance.

| Kode | Dipicu oleh (menurut Finance) | Lawan jurnal yang diratifikasi | Syarat | Dasar |
|---|---|---|---|---|
| `SALDO-SUBLEDGER` | Finance menutup periodenya | **Bukan jurnal** — dicocokkan dengan buku besar (bagian 8) | Bentuk bagian 8; `Amount` boleh nol atau negatif | `ACC-DEC-087`, `FIN-DEC-035` |
| `PENERIMAAN-UANG-MUKA` | Penerimaan sebelum tagihan final, deposit top-up | Debit Kas, kredit Uang Muka Pasien | Saldo Uang Muka Pasien hanya berkurang lewat tiga kode di bawah | `ACC-DEC-098`, `FIN-DEC-031` |
| `PEMAKAIAN-UANG-MUKA-DEPOSIT` | Tagihan final dilunasi dari uang muka atau deposit | Debit Uang Muka Pasien, kredit Piutang | — | `ACC-DEC-098`, `FIN-DEC-032` |
| `PENGEMBALIAN-UANG-MUKA` | Uang muka dikembalikan ke pasien | Debit Uang Muka Pasien, kredit Kas | — | `ACC-DEC-098`, `FIN-DEC-041` |
| `PEMBALIKAN-PENERIMAAN-UANG-MUKA` | Tender uang muka dibatalkan | Debit Uang Muka Pasien, kredit Kas | Bila uang mukanya sudah terpakai, dikirim **berpasangan** dengan `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` *(`0.6`, `ACC-DEC-125`)* | `ACC-DEC-098` |
| `PENGAKUAN-KELEBIHAN-BAYAR` | Kelebihan pembayaran atas piutang | Debit Piutang, kredit Uang Muka Pasien | **Bersyarat** — hanya bila pembayaran asal mengkredit Piutang; tidak terbit bila asalnya `PENERIMAAN-UANG-MUKA`. Mencakup kredit bertipe `SETTLEMENT` *(`0.6`, `ACC-DEC-130`)* | `ACC-DEC-100` |
| `PPN-MASUKAN-PEMBELIAN` | Faktur pembelian ber-PPN | Debit PPN Masukan **atau** beban PPN tak dapat dikreditkan, kredit Utang Supplier | Akun debit ditetapkan di G2 bersama pemilik pajak RS | `ACC-DEC-103` |
| `RETUR-PEMBELIAN` | Retur barang ke supplier | Debit Piutang Retur Supplier, kredit Persediaan | **Bersyarat** — nilai pokok tanpa PPN; porsi PPN lewat `PPN-MASUKAN-RETUR-PEMBELIAN` *(`0.6`)* | `ACC-DEC-105` |
| `PEMAKAIAN-KREDIT-RETUR-PEMBELIAN` *(`0.6`; semula `PEMAKAIAN-DEPOSIT-RETUR`)* | Kredit retur dipakai memotong pembayaran utang | Debit Utang Supplier, kredit Piutang Retur Supplier | Nama final Finance (`FIN-DEC-066`) | `ACC-DEC-106` |

### 3a.2b Delapan kode tambahan dan tiga kode penanda — 1 dan 5 Oktober 2026 *(`0.6`, `0.7`)*

| Kode | Dipicu oleh (menurut Finance) | Lawan jurnal yang diratifikasi | Syarat | Dasar |
|---|---|---|---|---|
| `SELISIH-KAS-KURANG` | Kas fisik shift lebih kecil dari catatan, selisih disahkan | Debit beban selisih kas, kredit Kas | `Amount` positif | `ACC-DEC-099`, `FIN-DEC-064` |
| `SELISIH-KAS-LEBIH` | Kas fisik shift lebih besar dari catatan, selisih disahkan | Debit Kas, kredit pendapatan selisih kas | `Amount` positif | `ACC-DEC-099`, `FIN-DEC-064` |
| `POTONGAN-PPH23-PIUTANG` | Penjamin memotong PPh 23 saat membayar | Debit PPh 23 dibayar di muka, kredit Piutang | — | `ACC-DEC-104`, `FIN-DEC-065` |
| `PEMBALIKAN-POTONGAN-PPH23-PIUTANG` | Pembalikan potongan di atas | Debit Piutang, kredit PPh 23 dibayar di muka | — | `ACC-DEC-104`, `FIN-DEC-065` |
| `POTONGAN-BIAYA-BANK-PIUTANG` | Biaya administrasi bank atas pembayaran piutang | Debit beban administrasi bank, kredit Piutang | — | `ACC-DEC-104`, `FIN-DEC-065` |
| `PEMBALIKAN-POTONGAN-BIAYA-BANK-PIUTANG` | Pembalikan potongan di atas | Debit Piutang, kredit beban administrasi bank | — | `ACC-DEC-104`, `FIN-DEC-065` |
| `PEMBALIKAN-PEMAKAIAN-UANG-MUKA-DEPOSIT` | Pemakaian uang muka untuk melunasi piutang dibalik | Debit Piutang, kredit Uang Muka Pasien | Berpasangan dengan `PEMBALIKAN-PENERIMAAN-UANG-MUKA` bila tendernya dibatalkan | `ACC-DEC-125` |
| `PPN-MASUKAN-RETUR-PEMBELIAN` | Retur pembelian dikonfirmasi dan membawa porsi PPN | Debit Piutang Retur Supplier, kredit akun yang sama dengan debit `PPN-MASUKAN-PEMBELIAN` | Nilainya porsi PPN saja | `ACC-DEC-125`, `FIN-DEC-068` |

**Tiga kode penanda — nama dan pemicu disepakati, aktif sesudah gerbang G6** (`ACC-DEC-126`, `127`; penanda ketiga `ACC-DEC-133` *(`0.7`)*):

| Kode | Dipicu oleh | Jurnal | `Amount` | Keadaan |
|---|---|---|---|---|
| `PEMBUKAAN-SHIFT-KASIR` *(`0.7`)* | Finance pertama kali melihat shift **belum final**: `OPEN`, `HANDED_OVER`, `REOPENED`, `CLOSED_WITH_VARIANCE`, atau `PERLU_TINDAK_LANJUT` (`FIN-DEC-121`) | Tidak ada — penanda status, tanpa aturan posting | `0` | Hari ini ditolak `400`; diterima sesudah penegakan shift dibangun (G6) |
| `PENUTUPAN-SHIFT-KASIR` | Shift mencapai `CLOSED` (kas pas) atau `REVIEWED` (selisih disahkan) | Tidak ada — penanda status, tanpa aturan posting | `0` | Hari ini ditolak `400`; diterima sesudah penegakan shift dibangun (G6) |
| `PEMBALIKAN-PENUTUPAN-SHIFT-KASIR` | Shift yang sudah tertutup dibuka kembali | Tidak ada | `0` | Sama |

*(`0.7`)* Ketiga penanda memakai `SourceTransactionId` = **Id shift** (`BilCashierShift.Id`), bukan
nomor shift yang terbaca manusia; `SourceVersion` = nomor siklus; `AccountingDate` = tanggal shift
dalam WIB. Kunci anti-ganda memuat `EventTypeCode`, sehingga ketiganya tidak bertabrakan untuk shift
dan siklus yang sama. Daftar tertutup nilai nol (`ACC-DEC-127` butir 2) berisi **ketiga** kode ini.
Dasar penegakan di G6: periode tertahan selama ada `PEMBUKAAN-SHIFT-KASIR` tanpa
`PENUTUPAN-SHIFT-KASIR` pada shift dan siklus yang sama, bertanggal di periode itu (`ACC-DEC-133`).
Pengirim ketiganya tetap tidak diaktifkan sampai Accounting menyatakan G6 siap.

Potongan piutang jenis "lain-lain" **tidak** diberi kode (`ACC-DEC-128`); Finance menolaknya di
validasi. Kredit retur tidak pernah dicairkan tunai (`FIN-DEC-069`), sehingga tidak ada kodenya.

### 3a.2c Dua belas kode piutang sewa — diusulkan Accounting 5 Oktober 2026 *(`0.7`)*

Menjawab `evidence/17` (`FIN-OQ-044(b)`); keputusan `ACC-DEC-136` sampai `ACC-DEC-140`. **Status:
usulan.** Nama final boleh diusulkan Finance (preseden `FIN-DEC-066`), dan kode menjadi aktif
sesudah Finance mengonfirmasinya (`evidence/18` butir 18.4). Seperti kode lain, ratifikasi bukan
aturan posting: kejadiannya Tertahan sampai aturan posting disusun di G2.

| Peristiwa di Finance | Sewa unit tenant | Sewa lahan parkir | Arah jurnal — akun persis ditetapkan di G2 |
|---|---|---|---|
| Tagihan dicatat | `PENGAKUAN-PIUTANG-SEWA-TENANT` | `PENGAKUAN-PIUTANG-SEWA-PARKIR` | Debit Piutang Sewa; kredit pendapatan sewa, atau pendapatan diterima di muka untuk tagihan tahunan |
| Tagihan dibatalkan | `PEMBALIKAN-PENGAKUAN-PIUTANG-SEWA-TENANT` | `PEMBALIKAN-PENGAKUAN-PIUTANG-SEWA-PARKIR` | Cermin pengakuan |
| Denda ditambahkan | `PENGAKUAN-DENDA-SEWA-TENANT` | `PENGAKUAN-DENDA-SEWA-PARKIR` | Debit Piutang Sewa; kredit akun denda |
| Pelunasan | `PENERIMAAN-PIUTANG-SEWA-TENANT` | `PENERIMAAN-PIUTANG-SEWA-PARKIR` | Debit bank atau kas — **bukan** Kas Kasir (`FIN-DEC-109`); kredit Piutang Sewa |
| Pelunasan dibetulkan | `PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-TENANT` | `PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-PARKIR` | Cermin pelunasan |
| Piutang dihapus | `PEMUTIHAN-PIUTANG-SEWA-TENANT` | `PEMUTIHAN-PIUTANG-SEWA-PARKIR` | Debit cadangan kerugian piutang atau beban piutang tak tertagih; kredit Piutang Sewa |

| Aturan | Isi | Dasar |
|---|---|---|
| Kode dipisah tenant dan parkir | Akun pendapatan dan perlakuan pajaknya berbeda, sedangkan aturan posting tidak memilih akun dari isi pesan | `ACC-DEC-136` |
| Nilai selalu positif | Baris pelunasan bernilai minus **tidak** dikirim; pembetulan pelunasan dan pembatalan tagihan memakai kode pembalik bernilai positif | `ACC-DEC-136` |
| Akun Piutang Sewa | Akun tersendiri, terpisah dari piutang pasien dan penjamin. **Tidak** ditandai control account sampai Finance menambahkan kelompok saldonya ke snapshot, sehingga **tidak** ada pesan `SALDO-SUBLEDGER` untuknya | `ACC-DEC-137` |
| Penghapusan | Aturan posting kedua kode `PEMUTIHAN-PIUTANG-SEWA-*` memakai perlakuan **Buat Draft**, sebagai pemeriksa kedua atas penghapusan tanpa jenjang persetujuan di Finance (`FIN-DEC-103`) | `ACC-DEC-138` |
| Pajak | Panduan awal: tenant dikenai PPN dan PPh Pasal 4 ayat (2) final; parkir dikenai pajak daerah. Karena yang ditagih adalah **sewa lahan** parkir, perlakuan parkir wajib dipastikan PIC Pajak. Bila PPN berlaku, kejadian pengakuan membawa dua komponen (nilai sewa dan PPN); bila PPh Pasal 4 ayat (2) dipotong penyewa, kode potongan tersendiri diusulkan sesudah konfirmasi | `ACC-DEC-139` |
| Rekening koran | Pencocokan rekening koran di luar lingkup rilis ini | `ACC-DEC-140` |

**Contoh:** tenant optik ditagih Oktober Rp 5.000.000 → `PENGAKUAN-PIUTANG-SEWA-TENANT`
Rp 5.000.000. Pelunasan tercatat Rp 5.200.000 padahal yang diterima Rp 5.100.000: layar Finance
menampilkan baris −Rp 100.000, sedangkan kepada Accounting dikirim
`PEMBALIKAN-PENERIMAAN-PIUTANG-SEWA-TENANT` Rp 100.000.

### 3a.2d Pemisahan per metode bayar — `ACC-DEC-132` *(`0.7`)*

Aturan posting dipilih menurut jenis kejadian saja, dan setiap barisnya menunjuk satu akun tetap.
Ruas `PaymentMethodCode` hanya tersimpan di pesan asli dan **tidak** dibaca saat menyusun jurnal.
Karena itu setiap kode yang membawa metode bayar **dipisah per metode**. Pola nama yang diusulkan
berakhiran `-TUNAI` dan `-NONTUNAI`; daftar finalnya diusulkan Finance lalu diratifikasi Accounting
(OQ-132-1). Sampai itu, keempat kode tanpa akhiran tetap tercantum di bagian 3a.1, dan **tidak**
dikirim dalam bentuk tak-terpisah sesudah cutover.

| Kode sekarang | Menjadi — pola nama usulan |
|---|---|
| `PENERIMAAN-KASIR` | `PENERIMAAN-KASIR-TUNAI`, `PENERIMAAN-KASIR-NONTUNAI` |
| `PEMBALIKAN-PENERIMAAN-KASIR` | `PEMBALIKAN-PENERIMAAN-KASIR-TUNAI`, `PEMBALIKAN-PENERIMAAN-KASIR-NONTUNAI` |
| `PENERIMAAN-PIUTANG` | `PENERIMAAN-PIUTANG-TUNAI`, `PENERIMAAN-PIUTANG-NONTUNAI` |
| `PEMBAYARAN-HUTANG-SUPPLIER` | `PEMBAYARAN-HUTANG-SUPPLIER-TUNAI`, `PEMBAYARAN-HUTANG-SUPPLIER-NONTUNAI` |

**Bentuk kejadian kas kasir.** Finance memilih salah satu (`evidence/18` butir 18.1):

| Hal | Posisi utama — `ACC-DEC-062` diperjelas | Posisi cadangan |
|---|---|---|
| Satuan | Satu kejadian per shift **per metode bayar** | Satu kejadian per kuitansi **per metode bayar** |
| `SourceTransactionId` | Id shift | Nomor kuitansi |
| `AccountingDate` | Tanggal pembukaan shift, dalam WIB | Tanggal pembukaan shift tempat kuitansi itu tercatat, dalam WIB — termasuk kuitansi pembalik, yang memakai shift pembalikan (`FIN-DEC-120`) |
| Syarat tambahan | — | Kebutuhan operasional mendesak Finance dan Billing; `ACC-DEC-062` diubah lewat keputusan baru bersama owner Billing |

Kedua bentuk menyamakan tanggal kejadian dengan tanggal mutasi kas shift di subledger Finance,
sehingga shift yang melewati tengah malam tidak menciptakan selisih rekonsiliasi Kas Kasir di dua
periode. Kejadian tunai dan non-tunai dari shift yang sama tidak bertabrakan, karena kunci
anti-ganda memuat `EventTypeCode`.

| Hal lain | Isi |
|---|---|
| Shift yang dibuka kembali | `SourceVersion` yang lebih tinggi pada kejadian transaksi **tidak menggantikan** versi sebelumnya; ia diproses sebagai kejadian tersendiri dengan jurnalnya sendiri. Hanya pesan saldo yang membandingkan versi. Tambahan dari shift yang dibuka kembali dikirim sebagai kejadian tambahan, misalnya ringkasan per siklus shift; rancangannya milik Finance |
| Lima ruas dimensi (`CashierShiftId`, `CashierShiftNumber`, `PaymentMethodCode`, `PaymentMethodAccountId`, `ReversalOfSourceTransactionId`) | Diterima sebagai ruas tambahan dan tampil di pesan asli untuk penelusuran. **Bukan** bagian dari dua belas bidang wajib, dan tidak memengaruhi jurnal |
| Rekening non-tunai | Satu kode non-tunai berarti satu akun debit. Bila rumah sakit memakai beberapa rekening penampung, cara pemisahannya diputuskan bersama bagan akun (OQ-132-2, G2) |

### 3a.3 Belum masuk katalog — menunggu Finance

| Kebutuhan | Keadaan | Dasar | Pemilik |
|---|---|---|---|
| ~~Selisih kas shift~~ | **Tertutup** *(`0.6`)* — `SELISIH-KAS-KURANG` dan `SELISIH-KAS-LEBIH` masuk katalog (bagian 3a.2b) | `ACC-DEC-099`, `FIN-DEC-064` | — |
| ~~Potongan piutang non-tunai~~ | **Tertutup** *(`0.6`)* — empat kode masuk katalog; jenis "lain-lain" tanpa kode | `ACC-DEC-104`, `128` | — |
| ~~PPN atas retur pembelian~~ | **Tertutup** *(`0.6`)* — `PPN-MASUKAN-RETUR-PEMBELIAN` | `ACC-DEC-125` | — |
| ~~Pencairan tunai kredit retur~~ | **Tertutup** *(`0.6`)* — tidak pernah terjadi (`FIN-DEC-069`) | `ACC-DEC-125` | — |
| Refund kategori `REFERRED_OUTPATIENT_ADMIN` | Akun debit belum ditetapkan; Finance mencatatnya sebagai kegagalan yang terlihat, nol kejadian. `SETTLEMENT` **tertutup** — masuk `PENGAKUAN-KELEBIHAN-BAYAR` (`ACC-DEC-130`) *(`0.6`)* | `ACC-DEC-102`, `129` | Rizki + owner Billing + pemilik proses akuntansi — syarat G6 |
| Kode per metode bayar *(`0.7`)* | Pola nama diusulkan (bagian 3a.2d); menunggu pilihan bentuk kas kasir dan daftar final dari Finance | `ACC-DEC-132` | Yasmin + owner Billing — G4 (OQ-132-1, `evidence/18` butir 18.1) |
| Kode piutang sewa *(`0.7`)* | Dua belas kode diusulkan (bagian 3a.2c); menunggu nama final dan saluran pelunasannya | `ACC-DEC-136` | Yasmin (`evidence/18` butir 18.3, 18.4) |
| Potongan PPh Pasal 4 ayat (2) atas sewa *(`0.7`)* | Hanya bila PIC Pajak mengonfirmasi pemotongan oleh penyewa; tidak membuka ulang `ACC-DEC-128` | `ACC-DEC-139` | PIC Pajak RS + Rizki (OQ-139-1) |

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

Base URL: `api/v1/corporate/accounting/accounting-events` — **Tersedia** sejak 28 September 2026 (`BE-ACC-P2-021`) *(`0.7`; sebelumnya tertulis "Rencana (belum tersedia)")*

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
| `EventStatus` | `string` | Ya | `Diterima`, `Terjurnal`, `Tertahan`, atau `Tercatat` (pesan saldo subledger). *(`0.5` — diselaraskan dengan `api-contract.md`:)* balasan kiriman ulang `200` dapat juga membaca `Gagal` atau `Diabaikan`, dan balasan `201` pesan saldo dapat membaca `Gagal` bila rinciannya lolos pemeriksaan awal tetapi tidak dapat dicatat. Pengirim tidak boleh menganggap nilai itu galat | — |
| `JournalNumber` | `string?` (maks 30) | Tidak | Hanya bila `Terjurnal`, contoh `JU/2026/11/00017`. *(`0.5`:)* dapat berganti bila draft hasil kejadian dihapus lalu dicoba ulang (`ACC-DEC-116`); kiriman ulang membaca nomor yang berlaku saat itu | `AccountingJournalNumber` — informasi "nomor saat diterima", **bukan** kunci; rujukan tetap adalah `AccountingEventId` |
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
| `SourceTransactionId` | *(`0.7`)* **Unik per akun control per periode.** Bentuk yang dipakai Finance: `SUBLEDGER-{AccountingPeriodCode}-{ControlAccountCode}`. Kunci anti-ganda kedua (bagian 5) tidak memuat kode akun, sehingga dua pesan saldo dengan `SourceTransactionId` dan `SourceVersion` yang sama dibaca sebagai kiriman ulang: yang kedua dijawab `200` dan saldonya **tidak** tercatat. Sebelumnya tertulis "rujukan dokumen tutup periode di Finance", yang mengundang satu nomor untuk semua akun |
| `SourceVersion` | Naik bila Finance menyatakan ulang saldo **akun yang sama** pada periode yang sama *(`0.7`)* |
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
  "SourceTransactionId": "SUBLEDGER-2026-11-1-1201",
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

### 8a. Empat aturan pesan saldo untuk rekonsiliasi *(`0.4`)*

Diputuskan sisi Accounting 28 September 2026. Tidak menambah bidang maupun penolakan; keempatnya
menjelaskan **isi** yang harus dikirim Finance supaya tutup bulan Accounting tidak tertahan.

| # | Aturan | Contoh | Akibat bila tidak dipenuhi | Dasar |
|---:|---|---|---|---|
| 1 | **Titik mulai.** Rekonsiliasi menahan tutup bulan Accounting sejak periode **saldo pertama** yang diterima untuk badan hukum itu | Saldo pertama untuk `2026-11` → November dan sesudahnya ditahan sampai lengkap; Oktober tidak | Mengirim saldo uji ke badan hukum produksi menyalakan rekonsiliasi lebih awal | `ACC-DEC-107` |
| 2 | **Setiap control account aktif yang menerima jurnal wajib dikirimi saldo** tiap periode — termasuk yang saldonya **nol** | Kas Kecil tanpa transaksi bulan itu tetap dikirim `Amount` `0.00` | Periode tertahan "belum menerima saldo" | `ACC-DEC-108` |
| 3 | **`Amount` menurut saldo normal akun**: positif saat akun wajar, termasuk akun bersaldo normal kredit | Utang Supplier Rp 300.000.000 → `300000000.00`, bukan `-300000000.00` | Selisih dua kali lipat, periode tertahan | `ACC-DEC-109` |
| 4 | **`AccountingDate` = tanggal akhir periode** | Periode `2026-09` → `2026-09-30` | Pesan tetap diterima, tetapi akun itu "belum lengkap" sampai versi lebih tinggi bertanggal akhir periode dikirim | `ACC-DEC-110` |

Toleransi selisih tetap **nol** dan tidak ada jalan pengecualian di sisi Accounting (`ACC-DEC-076`,
`113`). Selisih dibereskan dengan menyatakan ulang saldo lewat `SourceVersion` yang lebih tinggi,
atau dengan mengirim kejadian yang tertinggal.

## 9. Yang masih terbuka

Per 24 September 2026, dimutakhirkan 5 Oktober 2026 *(`0.7`)*. Yang sudah ditutup: `ACC-XM-001` dan ratifikasi bentuk pesan (`ACC-DEC-082`),
`DEC-ACC-P2-002` (`ACC-DEC-083`), `OD-ACC-01` (gugur), `OD-ACC-08` sisi Accounting (`ACC-DEC-087`).
*(`0.7`:)* OQ-131-1..4 dan OQ-124-1 tertutup oleh jawaban Finance `finance-management/evidence/22`
dan keputusan `ACC-DEC-132`..`134`; empat permintaan baru dibawa `evidence/18` bagian 8.

| Butir | Pertanyaan | Pemilik | Menahan |
|---|---|---|---|
| `OD-ACC-05` sisa | Mekanisme autentikasi akun layanan | Platform + Yasmin + Rizki | Gerbang G3 |
| ~~Kode saldo~~ | ~~Persetujuan kode `SALDO-SUBLEDGER` dan penyesuaian `FIN-DEC-023` ke bagian 8~~ **Tertutup 28 September 2026** — `FIN-DEC-035` | Yasmin | — |
| Cakupan saldo *(`0.4`)* | ~~Kesanggupan mengirim saldo untuk keempat kelompok control account, termasuk `Rp 0`~~ **Disanggupi** *(`0.6`)* — `FIN-DEC-090`, snapshot bulanan `BE-FIN-049`. ~~**Sisa:** empat baris per kelompok atau satu baris per akun control, dan utang honor dokter (`evidence/16` butir 16.2, OQ-131-2)~~ **Dijawab** *(`0.7`)* — satu baris per akun control sesuai pemetaan, termasuk utang jasa medis bernilai `0.00`; `SourceTransactionId` unik per akun (`SUBLEDGER-{periode}-{kode akun}`) (`FIN-DEC-113`, `122`). **Sisa:** daftar kode akun control definitif beserta pemetaan segmennya, dari Accounting kepada Finance sesudah G2. Piutang sewa **tidak** termasuk (`ACC-DEC-137`) | Rizki + pemilik proses akuntansi | G2 |
| Arah tanda *(`0.4`)* | ~~Kesanggupan mengirim `Amount` menurut saldo normal akun~~ **Disanggupi** *(`0.6`)* — `FIN-DEC-091`. ~~**Sisa:** Finance menolak saldo negatif tanpa pengecualian~~ **Tertutup** *(`0.7`)* — saldo negatif dikirim apa adanya (`FIN-DEC-112`) | — | — |
| Jadwal saldo *(`0.4`)* | ~~Kapan saldo terbit~~ **Dijawab** *(`0.6`)* — tanggal 1 pukul 00.05 WIB, `AccountingDate` tanggal akhir periode (`FIN-DEC-092`). ~~**Sisa:** pernyataan ulang saldo Kas Kasir~~ **Tertutup** *(`0.7`)* — dinyatakan ulang otomatis, hanya untuk akun yang nilainya berubah, diperiksa setiap hari pukul 00.05 WIB (`FIN-DEC-114`, `125`). Penjadwalnya belum aktif — lihat G4 bagian 13 | Yasmin | G4 |
| Komponen | Komponen yang dikirim per jenis kejadian | Yasmin | Aturan posting berbaris banyak |
| Kas di luar katalog | ~~Kode atau pernyataan tertulis untuk deposit, refund, selisih shift~~ **Sebagian besar tertutup** *(`0.6`)* — lihat bagian 3a.2b. **Sisa:** akun debit refund `REFERRED_OUTPATIENT_ADMIN` (`ACC-DEC-129`) | Rizki + owner Billing + pemilik proses akuntansi | Gerbang G6 |
| Selisih catatan | Versi percakapan (JWT Bearer, cutover 1 Oktober, amplop + 4 rincian) berbeda dengan `FIN-DEC-007`, `008`, `023` | Yasmin | Tidak menahan; wajib didamaikan (bagian 11) |
| `FIN-DEC-004` | Accounting meminta penerimaan sebelum tagihan final **terbit segera** sebagai uang muka pasien, beserta kode pemakaian uang muka (`ACC-DEC-091`) | Yasmin | Gerbang G6 |
| Shift kasir *(`0.5`, dimutakhirkan `0.6`, `0.7`)* | Kode penanda dan pemicunya **disepakati** — `PENUTUPAN-SHIFT-KASIR` dan pembaliknya, terbit pada `CLOSED` dan `REVIEWED` (`ACC-DEC-126`, `127`). ~~**Sisa:** bagaimana Accounting mengetahui shift yang dibuka (OQ-124-1)~~ **Tertutup** *(`0.7`)* — `PEMBUKAAN-SHIFT-KASIR` diratifikasi (`ACC-DEC-133`, bagian 3a.2b). **Sisa:** perkiraan pengaktifan pemicu otomatis penanda shift dan frekuensi pemeriksaan status shift (OQ-133-1, `evidence/18` butir 18.2) | Yasmin | Gerbang G6 (`ACC-DEC-124`) |
| Kas per shift atau per kuitansi *(`0.6`, dimutakhirkan `0.7`)* | ~~`ACC-DEC-062` menetapkan satu kejadian kas per shift; kode Finance menyiapkan `PENERIMAAN-KASIR` per kuitansi (OQ-131-1)~~ **Diputuskan sisi Accounting** *(`0.7`)* — posisi utama per shift, posisi cadangan per kuitansi bersyarat, keduanya dipisah per metode bayar (`ACC-DEC-132`, bagian 3a.2d). **Sisa:** pilihan bentuk dan daftar final kode per metode bayar (OQ-132-1, `evidence/18` butir 18.1) | Yasmin + owner Billing | G4 |
| Rekening non-tunai *(`0.7`)* | Satu kode non-tunai berarti satu akun debit. Bila rumah sakit memakai beberapa rekening penampung, bagaimana memisahkannya? (OQ-132-2) | Rizki + pemilik proses akuntansi | G2 |
| Piutang sewa *(`0.7`)* | Nama final kode dan saluran pelunasan (`evidence/18` butir 18.3, 18.4); akun pengakuan, denda, penghapusan, dan pelunasan (OQ-136-1); ketetapan pajak, termasuk perlakuan sewa lahan parkir (OQ-139-1) | Yasmin; Rizki + pemilik proses akuntansi; PIC Pajak RS | Aturan posting sewa (G2) |
| Konvensi tanggal *(`0.7`)* | ~~Tafsir `AccountingDate` dan batas periode~~ **Tertutup** — kalender WIB untuk `AccountingDate` dan batas periode; `EventOccurredAt` tetap waktu lengkap dengan zona (`ACC-DEC-134`, `FIN-DEC-116`) | — | — |
| Pasangan kontrak Finance *(`0.7`)* | `FIN-INTEGRATION-1.7`, tempat lima ruas dimensi dan penanda pembukaan shift ditulis, masih berstatus `draft`. Sesudah disetujui dengan isi yang sesuai `0.7`, dicatat sebagai pasangannya (bagian 11) | Yasmin | Tidak menahan; wajib didamaikan (bagian 11) |

## 10. Yang sudah dan belum dibangun sisi Accounting

| Bagian | Keadaan per 5 Oktober 2026 *(`0.7`; semula per 28 September 2026)* |
|---|---|
| Master jenis kejadian (`GET/POST/PUT/PATCH api/v1/corporate/accounting/event-types`) | **Sudah berdiri** — `BE-ACC-P2-017`, terbukti dipanggil 15 September 2026 |
| Master aturan posting (`api/v1/corporate/accounting/posting-rules`) | **Sudah berdiri** — `BE-ACC-P2-018` |
| Pintu masuk `POST /accounting-events`, status kejadian, coba ulang, daftar gagal | **Sudah berdiri** — Wave B tuntas 28 September 2026 (`BE-ACC-P2-019`..`026`). Penyesuaian sesudahnya: kejadian Gagal yang periodenya sudah tertutup menahan periode terbuka pertama (`BE-ACC-P2-029`); draft jurnal hasil kejadian yang menyentuh control account dapat diajukan (`034`); menghapus draft hasil kejadian mengembalikan kejadiannya ke Gagal (`035`) |
| Penerimaan dan penyimpanan saldo subledger | **Sudah berdiri** — `BE-ACC-P2-027` dan `028`, 28 September 2026 |
| Pembandingan saldo dan penghalang rekonsiliasi | **Sudah berdiri** — `BE-ACC-P2-014`, 29 September 2026: `GET api/v1/corporate/accounting/reconciliation/subledger-comparison`, dan penghalang `SUBLEDGER_RECONCILIATION` yang menolak `409` pengajuan tutup periode dan tutup permanen; menyala per badan hukum sejak saldo pertama (bagian 8a) |
| Penerimaan `Amount = 0` untuk kode penanda dan penegakan shift kasir | **Belum** — bagian gerbang G6 (`ACC-DEC-124`, `127`, `133`). Hari ini penanda bernilai nol ditolak `400` |
| Pemilihan akun dari ruas tambahan pesan, misalnya `PaymentMethodCode` | **Tidak ada, dan tidak direncanakan** — karena itu kode dipisah per metode bayar (bagian 3a.2d) |
| Uji penerimaan (UAT) seluruh bagian di atas | Belum — milik tim UAT terpisah |

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

| Gerbang | Syarat | Pemilik | Keadaan 5 Oktober 2026 *(`0.7`; semula per 24 September 2026)* |
|---|---|---|---|
| G1 | Kotak masuk dibangun dan diuji ujung-ke-ujung | Rizki | Development selesai dan sudah di integration; UAT belum |
| G2 | `ACC-TD-022` ditutup (bagan akun sah) dan aturan posting untuk setiap kode yang akan aktif tersusun. *(`0.7`:)* Termasuk akun Piutang Sewa tersendiri yang **tidak** ditandai control account (`ACC-DEC-137`), cara memisahkan rekening non-tunai (OQ-132-2), dan daftar kode akun control definitif beserta pemetaan segmennya untuk Finance | Pemilik proses akuntansi + Rizki | `OPEN` |
| G3 | Akun layanan aktif sesuai bagian 4c dan mekanismenya sudah diputuskan | Platform + Yasmin + Rizki | Mekanisme terbuka |
| G4 | Pengirim Finance siap. *(`0.7`:)* **Bersyarat** — worker pengiriman, penjadwal snapshot pukul 00.05 WIB, dan pemicu otomatis penanda shift terbukti berjalan (`FIN-DEC-118`), G3 sudah diputuskan, dan daftar final kode per metode bayar sudah diratifikasi (`ACC-DEC-132`, OQ-132-1) | Yasmin | **Bersyarat** menurut Finance sendiri (`finance-management/evidence/22` bagian 4.2): ketiga bagian baru dibangun dan dalam keadaan mati. Rencana cutover Accounting tidak menghitungnya selesai |
| G5 | Saldo awal manual per tanggal cutover siap diinput | Rizki | Bergantung G2. Finance menyimpan saldo awal yang **sama** dengan angka Accounting (`FIN-DEC-128`) |
| G6 | Kode atau pernyataan tertulis Finance untuk deposit pasien, kelebihan bayar dan refund, serta selisih kas shift — termasuk jaminan top-up deposit **tidak** dikirim sebagai `PENERIMAAN-KASIR`; ditambah perubahan `FIN-DEC-004` dan kode pemakaian uang muka (`ACC-DEC-091`). *(`0.5` — `ACC-DEC-124`:)* **ditambah penegakan shift kasir belum ditutup** (`FR-P2-038`, `ACC-DEC-065`) sudah dirancang, dibangun, dan diuji di Accounting — bergantung pada kode penanda shift dan OQ-124-1 (bagian 9). *(`0.6`:)* **ditambah** kotak masuk menerima `Amount = 0` hanya untuk daftar tertutup kode penanda (`ACC-DEC-127`), dan **akun debit refund `REFERRED_OUTPATIENT_ADMIN` sudah diputuskan** (`ACC-DEC-129`). *(`0.7`:)* Daftar tertutup nilai nol berisi **tiga** kode penanda, dan penegakan shift memakai pasangan `PEMBUKAAN-SHIFT-KASIR` dan `PENUTUPAN-SHIFT-KASIR` pada shift dan siklus yang sama (`ACC-DEC-133`) | Yasmin + owner Billing; penegakan shift: Rizki | Terbuka. Tiga kode penanda disepakati; penegakan shift dan nilai nol penanda belum dibangun; akun debit refund `REFERRED_OUTPATIENT_ADMIN` belum diputuskan |

**Kenapa penegakan shift masuk G6** *(`0.5`)*. Kas kasir datang per shift. Shift yang
melewati pergantian bulan dan belum ditutup membuat kasnya belum sampai ke buku besar, sementara
bulan itu tetap dapat ditutup. Sebelum cutover risiko ini tidak ada, karena belum ada kas yang
masuk lewat kejadian; sesudah cutover, ia langsung ada. **Contoh:** shift malam 31 Oktober ditutup
1 November pukul 07.00. Tanpa penahan, Accounting dapat mengajukan tutup Oktober pada 1 November
pukul 06.00 dengan kas Oktober yang kurang satu shift.

**1 Oktober 2026 tidak layak** karena G1, G2, G3, dan G4 belum terpenuhi tujuh hari sebelumnya.

**Daftar periksa sesudah pengiriman dimulai** *(`0.7`, `ACC-DEC-135`)*. Cutover baru dinyatakan
**selesai** sesudah sedikitnya satu snapshot `SALDO-SUBLEDGER` untuk periode pertama sesudah cutover
berstatus Tercatat di kotak masuk, per badan hukum. Sebelum itu, periode pertama tidak diajukan
tutup.

| Hal | Isi |
|---|---|
| Celah yang ditutup | Rekonsiliasi baru menahan tutup bulan sejak periode saldo **pertama** yang diterima (bagian 8a aturan 1, `ACC-DEC-107`). Finance tidak menerbitkan apa pun bila pemetaan akunnya belum lengkap (`FIN-DEC-113`, gagal tertutup). Tanpa daftar periksa ini, periode pertama dapat ditutup tanpa rekonsiliasi sama sekali |
| Kenapa satu cukup | Begitu satu saldo periode itu Tercatat, sistem menuntut saldo untuk setiap control account lainnya (bagian 8a aturan 2, `ACC-DEC-108`) |
| Bentuk | Langkah operasional petugas Accounting: memeriksa tab Tercatat di Kotak Masuk Kejadian sebelum mengajukan tutup periode pertama. **Bukan** penegakan sistem; `ACC-DEC-107` tidak berubah |
| Kapan | Sesudah periode pertama berakhir — snapshot terbit tanggal 1 berikutnya pukul 00.05 WIB, dan Finance menolak periode yang berakhir sebelum cutover. Karena itu daftar periksa ini **bukan** bagian G1–G6, dan **bukan** saldo awal manual G5 |

**Contoh:** cutover 1 Januari 2027. Snapshot Januari terbit 1 Februari pukul 00.05. Januari tidak
diajukan tutup sebelum minimal satu saldo Januari Tercatat; bila Finance menahan snapshot karena
pemetaan belum lengkap, Januari ikut menunggu.

**Kenapa G6 menjadi gerbang.** Top-up deposit Rp 5.000.000 yang terkirim sebagai `PENERIMAAN-KASIR`
dapat terbukukan sebagai pendapatan, padahal uang itu kewajiban kepada pasien. Buku besar tetap
seimbang dan tidak ada pesan galat, sehingga kesalahannya baru ketahuan saat pemeriksaan.
