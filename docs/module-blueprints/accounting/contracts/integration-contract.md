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
| Pertama | Nomor kejadian (`EventId`) | Kunci utama |
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
[cross-module-contract.md](cross-module-contract.md) (`ACC-XMOD-0.1`), supaya Yasmin dapat
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
| 1 | `ACC-XM-001` diputuskan bersama owner Billing dan owner Finance | **Sebagian.** Diputuskan sisi Accounting lewat `ACC-DEC-044`; **ratifikasi owner Billing dan owner Finance belum ada** |
| 2 | Sembilan pertanyaan `DEFERRED` pada `ACC-DEC-036` dijawab | **Selesai.** Menjadi `ACC-DEC-045` sampai `ACC-DEC-053` |
| 3 | Kedua gerbang pada bagian 4 dilewati | **Selesai.** `READY_FOR_DOMAIN_DESIGN` dan `DOMAIN_ARCHITECTURE_READY` |

Dua dari tiga terpenuhi. Karena itu **rancangan** Phase 2 boleh disusun dan sudah disusun — lihat
bagian 6 di bawah.

> **Yang masih dilarang tidak berubah:** tidak ada satu pun kode integrasi yang boleh ditulis
> sampai syarat nomor 1 terpenuhi penuh. Rancangan boleh, kode belum.

---

## 6. Kontrak integrasi Phase 2

| Field | Nilai |
|---|---|
| `contract_version` | `ACC-INTEGRATION-0.3` |
| `last_changed_in` | `ACC-INTEGRATION-0.3` — 8 September 2026 |
| Status | **`approved`** — Rizki, 8 September 2026; **implementasi tetap terkunci** sampai `ACC-XM-001` diratifikasi owner Billing dan owner Finance |
| Traceability | `ACC-DEC-044`, `045`, `046`, `047`, `048`, `049`, `056` |

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

1. **Kesepuluhnya wajib.** Pesan dengan satu bidang kosong ditolak `400`, bukan diterima sebagian.
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
| Kedua | `SourceModule` + `SourceTransactionId` + `EventTypeId` + `SourceVersion` | Penerbit keliru membuat **nomor baru** untuk kejadian yang sama |

Kiriman ulang dijawab `200` beserta nomor jurnal yang sama, **bukan** `409`. Alasannya ada di
`api-contract.md`.

### 6.5 Perilaku saat gagal

| Keadaan | Perlakuan | Dasar |
|---|---|---|
| Gangguan teknis | Coba ulang 3 kali dengan jeda makin panjang, lalu masuk daftar gagal; Accounting Manager diberi tahu lewat penanda jumlah menu | `ACC-DEC-049`, `ACC-DEC-057` |
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
| Ratifikasi `ACC-DEC-048` bentuk pesan | Owner Finance (Yasmin) | **Belum.** Ditambah usulan dua bidang baru `CorrelationId` dan `CausationId` — lihat [`evidence/10`](../evidence/10-billing-arap-handoff-scan.md) bagian 21.2 |
| Menetapkan daftar jenis kejadian (`DEC-ACC-P2-002`) | Rizki dan Yasmin | **Belum** |
| Modul Finance berdiri (`ACC-DEP-004`) | Yasmin | **Belum** — diperiksa 8 September 2026, `Areas/Corporate/` hanya memuat `AccountingManagement` dan `HumanResource` |
