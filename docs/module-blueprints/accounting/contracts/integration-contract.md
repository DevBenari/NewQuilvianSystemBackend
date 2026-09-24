# Accounting — Integration Contract

| Field | Value |
|---|---|
| `contract_version` | `ACC-INTEGRATION-0.2` |
| Status | `draft` — dan **belum berlaku untuk MVP** |
| Owner | Rizki (Accounting), owner Billing, owner Finance/Yasmin |
| Perubahan `0.1` → `0.2` | Kontrak cross-module dipisah ke `ACC-XMOD-0.1`; berkas ini tetap memegang batas MVP dan gerbang Phase 2 |
| `approved_by` / `approved_at` | Belum ada |
| `input_revision` | `00-interview-decisions.md@3` |
| Traceability | `ACC-DEC-003`, `ACC-DEC-004`, `ACC-DEC-005`, `ACC-DEC-009`, `ACC-DEC-011`, `ACC-DEC-035`, `ACC-DEC-036` |

## Tidak berlaku untuk rilis pertama

**Modul Accounting MVP tidak memanggil sistem luar dan tidak menerima panggilan dari sistem
luar.** Seluruh jurnal pada rilis pertama dibuat manusia lewat layar Jurnal Manual. Ini akibat
langsung `ACC-DEC-009`, yang menempatkan integrasi otomatis di Phase 2.

Karena itu berkas ini **tidak** memuat kontrak yang berlaku sekarang. Ia mencatat dua hal yang
tetap perlu tertulis: batas yang tidak boleh dilanggar MVP, dan keputusan yang sudah diambil untuk
Phase 2 supaya rancangan MVP tidak menutup jalannya.

## 1. Batas yang mengikat MVP

Walaupun tidak ada integrasi, batas kepemilikan tetap berlaku dan tetap dapat dilanggar tanpa
sengaja. Berikut yang **MUST NOT** dilakukan modul Accounting pada MVP.

| Larangan | Keputusan asal | Kenapa mudah terlanggar |
|---|---|---|
| Membaca atau menulis tabel Billing | `ACC-DEC-004` | Tergoda mengambil data faktur untuk "memudahkan" pengisian jurnal |
| Membaca atau menulis tabel Finance | `ACC-DEC-003` | Belum ada tabelnya, jadi risikonya rendah — tetapi tetap dicatat |
| Membuat tabel piutang atau utang sendiri | `ACC-DEC-005` | Tergoda saat menyusun akun piutang, padahal yang dimiliki Accounting hanya akunnya, bukan transaksinya |
| Mengubah kontrak Billing yang sudah disetujui | `ACC-PRD-001` §36 aturan 13 | — |
| Menerbitkan kejadian keuangan ke modul lain | `ACC-DEC-002` | Accounting adalah muara, bukan sumber |

Perbedaan yang perlu dipahami: Accounting **memiliki akun** bernama `1-1201 Piutang Penjamin`,
tetapi **tidak memiliki** daftar siapa berutang berapa. Yang pertama adalah laci penggolongan
milik Accounting; yang kedua adalah transaksi operasional milik Finance.

## 2. Keputusan Phase 2 yang sudah diambil

Tiga keputusan sudah diambil dan mengikat rancangan Phase 2, walaupun belum diwujudkan.

### `ACC-DEC-011` — satu kejadian resmi, dua konsumen

Kejadian keuangan diterbitkan **sekali** dengan nomor unik, lalu dibaca Finance dan Accounting
untuk keperluan berbeda. Karena nomornya sama, pencatatan ganda dapat dicegah.

Yang belum diputuskan: **siapa yang menerbitkan** kejadian itu. Itulah `ACC-XM-001`.

### `ACC-DEC-035` — dua kunci pencegah pencatatan ganda

| Lapis | Kunci | Perannya |
|---|---|---|
| Pertama | Nomor kejadian (`EventId` pada rancangan awal; sejak Phase 2 bernama `EventNumber` — lihat bagian 6.4) | Kunci utama |
| Kedua | Gabungan modul asal, nomor transaksi asal, jenis kejadian, dan versi | Jaring pengaman bila pengirim keliru membuat ulang nomor |

Keduanya diwujudkan sebagai **dua index unik terpisah** pada tabel kotak masuk kejadian. Satu
index saja tidak cukup: index pertama gagal melindungi bila pengirim membuat nomor baru untuk
kejadian yang sama, dan index kedua rumit bila satu transaksi sah menghasilkan beberapa kejadian
sejenis.

**Contoh cara kerjanya.** Kejadian `EVT-100` diterima tiga kali karena jaringan bermasalah.
Penerimaan pertama membuat jurnal `JU/2026/09/00042`. Penerimaan kedua dan ketiga menemukan
`EVT-100` sudah tercatat, lalu mengembalikan nomor jurnal yang sama tanpa membuat jurnal baru.
Buku besar tetap berisi satu catatan.

### `ACC-DEC-036` — sembilan pertanyaan masih ditunda

Bentuk pesan, perlakuan kejadian gagal, perlakuan kejadian yang belum punya pemetaan akun, dan
perlakuan kejadian yang datang setelah periodenya ditutup — seluruhnya masih `DEFERRED`.
Merancangnya sekarang berisiko salah, karena `ACC-XM-001` belum diputuskan.

## 3. Konflik yang harus diselesaikan sebelum Phase 2

| ID | Isi | Status |
|---|---|---|
| `ACC-XM-001` | Siapa menerbitkan kejadian keuangan resmi atas tagihan pasien | **Terbuka** — `CROSS_MODULE_DECISION_REQUIRED`, pemilik: owner Billing + owner Finance/Yasmin + Rizki |
| `ACC-DEP-003` | Kontrak Billing `BIL-INTEGRATION-0.4` mengarahkan `BIL-INT-007`, `008`, dan `009` ke AR/AP, yaitu wilayah Finance — bukan ke Accounting | `CONFLICT` |
| `ACC-DEP-004` | Modul Finance belum ada. Developernya sudah ditunjuk: **Yasmin** | `MISSING` |

Bentuk batas Finance/AR/AP → Accounting sudah dituliskan lebih dahulu di
[cross-module-contract.md](cross-module-contract.md) (`ACC-XMOD-0.1` saat itu; sejak 15 September 2026 `ACC-XMOD-0.2`, diselaraskan dengan bagian 6 berkas ini), supaya Yasmin dapat
mengembangkan Finance secara paralel tanpa menunggu Phase 2. Kontrak itu mengunci **bentuk**
batas, bukan implementasinya, dan tidak memindahkan posting otomatis ke MVP.

Risiko yang dijaga ketiganya adalah sama: **satu tagihan tercatat dua kali di buku besar.** Bila
Accounting berlangganan langsung ke Billing sementara Finance meneruskan kejadian yang sama,
tagihan Rp 10.000.000 akan menghasilkan dua jurnal. Buku besar tetap seimbang, tetapi pendapatan
rumah sakit tercatat Rp 20.000.000.

## 4. Gerbang yang wajib dilewati sebelum Phase 2 dirancang

Ini bagian yang paling mudah terlewat, jadi ditulis tegas.

Accounting MVP diperlakukan sebagai kemampuan **non-rumah-sakit**, sehingga perancangannya boleh
berjalan tanpa `requirement-completeness-gate` maupun `hospital-domain-architect`. Dasarnya ada
di [../02-backend-architecture.md](../02-backend-architecture.md) bagian 1.

> **Kedua gerbang ini sudah DILEWATI pada 8 September 2026.**
>
> | Gerbang | Hasil | Bukti |
> |---|---|---|
> | `requirement-completeness-gate` | **`READY_FOR_DOMAIN_DESIGN`** keempat slice | [`evidence/08`](../evidence/08-phase2-requirement-completeness-gate.md) |
> | `hospital-domain-architect` | **`DOMAIN_ARCHITECTURE_READY`**, `ACC-DOMAIN-P2-0.1` | [`evidence/09`](../evidence/09-phase2-hospital-domain-architecture.md) |
>
> Teks di bawah dipertahankan sebagai catatan sejarah tentang mengapa kedua gerbang itu diwajibkan.

**Phase 2 tidak mendapat kelonggaran itu.** Begitu modul menerima kejadian keuangan yang berasal
dari tagihan pasien, ia melintasi bounded context Billing dan menyentuh data yang terikat pada
kunjungan pasien. Sebelum Phase 2 dirancang, dua skill berikut **wajib** dijalankan lebih dahulu:

1. `requirement-completeness-gate` — menilai kelengkapan requirement lintas modul.
2. `hospital-domain-architect` — menetapkan bounded context, ownership, dan dampak billing.

Menjalankan `design-business-module` untuk Phase 2 tanpa keduanya melanggar gerbang skill itu
sendiri.

## 5. Kapan berkas ini diisi

Berkas ini diperbarui menjadi kontrak sungguhan ketika tiga hal terpenuhi. Keadaannya per
8 September 2026:

| # | Syarat | Keadaan |
|---:|---|---|
| 1 | `ACC-XM-001` diputuskan bersama owner Billing dan owner Finance | **Selesai 24 September 2026.** Accounting `ACC-DEC-044`, Billing `ACC-DEC-059`, Finance `FIN-DEC-001`; ditutup `ACC-DEC-082` |
| 2 | Sembilan pertanyaan `DEFERRED` pada `ACC-DEC-036` dijawab | **Selesai.** Menjadi `ACC-DEC-045` sampai `ACC-DEC-053` |
| 3 | Kedua gerbang pada bagian 4 dilewati | **Selesai.** `READY_FOR_DOMAIN_DESIGN` dan `DOMAIN_ARCHITECTURE_READY` |

~~Dua dari tiga terpenuhi.~~ **Ketiganya terpenuhi sejak 24 September 2026.**

> ~~Yang masih dilarang tidak berubah: tidak ada satu pun kode integrasi yang boleh ditulis sampai
> syarat nomor 1 terpenuhi penuh.~~ **Larangan dicabut `ACC-DEC-082`.** Kode kotak masuk kejadian
> boleh ditulis. Yang masih ditahan adalah **pengaktifan pengiriman** dari Finance, oleh gerbang
> cutover pada bagian 6.9.

---

## 6. Kontrak integrasi Phase 2

| Field | Nilai |
|---|---|
| `contract_version` | `ACC-INTEGRATION-0.5` |
| `last_changed_in` | `ACC-INTEGRATION-0.5` — 24 September 2026. Sebelumnya `0.4`, 15 September 2026; `0.3`, 8 September 2026 |
| Status | **`approved`** sampai `0.3` — Rizki, 8 September 2026. Teks `0.4` dan `0.5` hanya menuliskan keputusan owner yang sudah `approved` (`ACC-DEC-060`, `075`, `082`..`090`); teksnya **di-approve Rizki 24 September 2026** (`GATE-DESAIN-0924`). **Implementasi tidak lagi terkunci**: `ACC-XM-001` ditutup `ACC-DEC-082` |
| Perubahan `0.4` → `0.5` | (1) Kunci implementasi dicabut (`ACC-DEC-082`). (2) Mode pemrosesan seketika dengan penjadwal cadangan, bagian 6.8 (`ACC-DEC-084`). (3) Gerbang cutover G1–G6, bagian 6.9 (`ACC-DEC-089`, `090`). (4) Tabel 6.7 dimutakhirkan. Nol perubahan arah, pintu masuk, bentuk pesan, dan kunci anti-ganda |
| Perubahan `0.3` → `0.4` | (1) Kunci anti-ganda kedua memakai `EventTypeCode`, bukan `EventTypeId` (`ACC-DEC-075` butir 4) — kamus data bagian 9 dan `02-backend-architecture.md` sudah memakainya sejak 14 September 2026, hanya berkas ini yang tertinggal. (2) Bagian 6.7 mencatat `CorrelationId`/`CausationId` sudah wajib, bukan lagi usulan. (3) Bagian 6.3 butir 1 "kesepuluhnya" menjadi "kedua belasnya", sesuai judul bagian dan `ACC-DEC-060`. Nol perubahan arah, pintu masuk, maupun perilaku |
| Traceability | `ACC-DEC-044`, `045`, `046`, `047`, `048`, `049`, `056`, `059`, `060`, `075`, `082`, `083`, `084`, `085`, `088`, `089`, `090`; `FIN-DEC-001`, `002`, `008` |

### 6.1 Arah dan pemilik

| Aspek | Ketentuan |
|---|---|
| Arah | **Satu arah**, Finance → Accounting |
| Penerbit | **Finance** (`ACC-DEC-044`) |
| Konsumen | Accounting |
| Yang dilarang | Accounting berlangganan langsung ke Billing; Accounting menerbitkan kejadian ke modul lain (`ACC-DEC-002`) |
| Bentuk hubungan | Customer-Supplier dengan Published Language — bentuk pesannya disepakati bersama, bukan didikte sepihak |

### 6.2 Titik masuk

Satu-satunya pintu masuk adalah
`POST api/v1/corporate/accounting/accounting-events`, dijaga `AccountingEvent : Receive` yang
hanya dimiliki akun layanan. Rinciannya di
[`api-contract.md`](api-contract.md) bagian Phase 2.

### 6.3 Bentuk pesan — dua belas bidang wajib

Dikunci `ACC-DEC-048` dan `ACC-DEC-060`. Daftar lengkap beserta tipenya ada di `api-contract.md`.
Empat hal yang mengikat kedua pihak:

1. **Kedua belasnya wajib.** Pesan dengan satu bidang kosong ditolak `400`, bukan diterima sebagian.
2. **Mata uang hanya `IDR`.** Nilai lain ditolak `409` (`ACC-DEC-020`).
3. **Nol pengenal pasien.** Nama, nomor rekam medis, dan nomor kunjungan **dilarang** ada di dalam
   pesan (`ACC-DEC-056`). Penelusuran ke pasien dilakukan lewat nomor transaksi asal.
4. **`CorrelationId` dan `CausationId` wajib** (`ACC-DEC-060`). Owner Billing menyatakan 9
   September 2026 bahwa kejadian Finance memang membawanya; Accounting menyimpannya supaya
   penelusuran jurnal ke faktur Billing tidak terputus di Finance.

### 6.4 Pencegahan pencatatan ganda

Dua kunci dipakai bersamaan (`ACC-DEC-035`), diwujudkan sebagai **dua unique index terpisah**:

| Lapis | Kunci | Melindungi dari |
|---|---|---|
| Pertama | `EventNumber` | Pesan yang sama terkirim berulang |
| Kedua | `SourceModule` + `SourceTransactionId` + `EventTypeCode` + `SourceVersion` | Penerbit keliru membuat **nomor baru** untuk kejadian yang sama |

Kiriman ulang dijawab `200` beserta nomor jurnal yang sama, **bukan** `409`. Alasannya ada di
`api-contract.md`.

**Kenapa kunci kedua memakai kode jenis, bukan id master jenis** (`ACC-DEC-075`, diselaraskan pada
`0.4`). Kejadian berjenis yang belum terdaftar disimpan **Tertahan** dengan id jenis kosong. Bila
kunci kedua memakai id, kejadian itu tidak punya kunci kedua sama sekali: Finance mengirim
`PENGHAPUSAN-PIUTANG` bernomor `EVT-200`, lalu karena gangguan mengirim ulang kejadian yang sama
bernomor `EVT-201` — keduanya lolos dan menjadi dua jurnal begitu jenisnya didaftarkan. Dengan kode
yang tersimpan apa adanya, kiriman kedua tertangkap sejak awal. Sampai `0.3` baris di atas masih
menulis `EventTypeId`.

### 6.5 Perilaku saat gagal

| Keadaan | Perlakuan | Dasar |
|---|---|---|
| Gangguan teknis | Kejadian yang sudah tersimpan tetap `Diterima` dan dijawab `201` tanpa nomor jurnal; penjadwal mencoba ulang 3 kali dengan jeda makin panjang, lalu masuk daftar gagal; Accounting Manager diberi tahu lewat penanda jumlah menu | `ACC-DEC-049`, `ACC-DEC-057`, `ACC-DEC-084` |
| Jenis kejadian belum dipetakan | Kejadian **Tertahan**, nol jurnal dibuat, nol akun sementara dipakai | `ACC-DEC-046` |
| Periode sudah tertutup | Jurnal masuk periode terbuka berikutnya; tanggal dokumen asli disimpan | `ACC-DEC-047` |
| Mata uang bukan rupiah | Ditolak `409` | `ACC-DEC-020` |

### 6.6 Rekonsiliasi

Daftar kejadian **Tertahan** dan **Gagal** wajib muncul pada daftar periksa penutupan bulan.
Kejadian `Gagal` **menahan** penutupan; kejadian `Tertahan` hanya **memperingatkan**
(`ACC-DEC-051`).

### 6.7 Yang wajib dilakukan sebelum implementasi

| Langkah | Pemilik | Keadaan |
|---|---|---|
| Ratifikasi `ACC-DEC-044` | Owner Billing | **Selesai 9 September 2026** — `ACC-DEC-059`. Finance menerbitkan kejadian tersendiri; Accounting **dilarang** membaca `BilArHandoff` langsung |
| Ratifikasi `ACC-DEC-048` bentuk pesan | Owner Finance (Yasmin) | **Selesai 20 September 2026** — `FIN-DEC-001`, apa adanya. Termasuk dua bidang `CorrelationId` dan `CausationId` yang **sudah wajib** sejak `ACC-DEC-060` — dulu tercatat sebagai usulan, lihat [`evidence/10`](../evidence/10-billing-arap-handoff-scan.md) bagian 21.2. Bahan untuk Yasmin: [`evidence/12`](../evidence/12-paket-kontrak-kejadian-untuk-finance.md) |
| Menetapkan daftar jenis kejadian (`DEC-ACC-P2-002`) | Rizki dan Yasmin | **Selesai 24 September 2026** — 17 kode, `FIN-DEC-002` + `ACC-DEC-083` |
| Modul Finance berdiri (`ACC-DEP-004`) | Yasmin | **Selesai** — diperiksa 24 September 2026, `Areas/Corporate/FinanceManagement/AccountingIntegration/` berdiri; `FinAccountingEventOutbox` cocok dua belas bidang |

### 6.8 Kapan jurnal dibuat — `ACC-DEC-084`

Seketika di dalam request penerimaan, dengan penjadwal sebagai cadangan. Kejadian disimpan dan
di-commit lebih dahulu, baru dijurnal. Hasilnya `201` dengan nomor jurnal (`Terjurnal`), `422`
(`Tertahan`), atau — bila gangguan teknis terjadi setelah kejadian tersimpan — `201` tanpa nomor
jurnal, lalu `AccAccountingEventSchedulerHostedService` mencoba ulang. Isi balasannya
(`AccountingEventReceiptDto`) ada di [`cross-module-contract.md`](cross-module-contract.md) bagian 4b.

### 6.9 Gerbang cutover — `ACC-DEC-089`, `ACC-DEC-090`

Pengiriman dari Finance diaktifkan pada tanggal 1 pukul 00.00 WIB di awal periode akuntansi
pertama setelah enam gerbang lolos: G1 kotak masuk teruji, G2 `ACC-TD-022` ditutup beserta aturan
posting, G3 akun layanan aktif, G4 pengirim Finance siap, G5 saldo awal siap, G6 deposit/refund/
selisih shift terjawab. Rinciannya di [`cross-module-contract.md`](cross-module-contract.md)
bagian 13. **1 Oktober 2026 dinyatakan tidak layak.**
