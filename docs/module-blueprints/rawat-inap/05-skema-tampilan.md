# Rawat Inap — Skema Tampilan

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.4` |
| Status | `draft` — menunggu persetujuan pemilik |
| Cakupan revision ini | **Seluruh 19 layar fungsional `FE-INP-01` s.d. `FE-INP-19`** |
| Masukan otoritatif | `00-interview-decisions.md` revision `7`; `03-frontend-architecture.md` revision `0.4`; kontrak `0.4.0` |
| Keluaran hilir | `roadmap/frontend-roadmap.md` revision `5` draft disinkronkan setelah skema ini |
| Baseline desain | Frontend `dec4fdeff07c3c96ad9f07f41f184c54cf771371`; backend `5afb54bd75281648010e50ef14f43ca1f80d8efd` |
| Impact scan kontrak | Frontend `12562f17e12ee43b7d8cdaeaff3f1a1fca5a8360`; backend `f102020611fc3d605fdef1949a3af23da93e4215`; 28 Agustus 2026, baca-saja |
| Refresh bukti enam layar | Frontend tetap `12562f17e12ee43b7d8cdaeaff3f1a1fca5a8360`; backend teramati `b71a6a3d12190c4db60fe3433f10b6eb92131629`; enam screenshot runtime dari pemilik, 28 Agustus 2026; audit source terbatas pada layar terkait dan master seeder |
| Pemeriksaan drift setelah refresh | HEAD frontend `efb389ea69da080309632ca2af387a39bd637819`; HEAD backend `f5fdbaf629fe4581b6fa063a2593d950e38e9fe1`; pemeriksaan rentang setelah snapshot bukti tidak menemukan perubahan source aplikasi, hanya aturan engineering, tooling, dan dokumen blueprint. Temuan capability tetap berlaku pada scope yang sama |
| Brief UI terkini | Instruksi pemilik 28 Agustus 2026: master/configuration Rawat Inap ditempatkan pada `Pelayanan Kesehatan → Master Data`; masih menunggu approval revision ini |
| Batas tulis | Hanya dokumen blueprint |

---

## 0. Kedudukan dokumen ini

`03-frontend-architecture.md` sengaja **tidak** menetapkan tata letak, dan menyerahkannya ke
`DEV_DISCRETION`. Setelah mendapat approval, dokumen ini menempati tingkat di atas
`DEV_DISCRETION`, yaitu **brief UI yang disetujui**:

```text
keamanan / privasi / invariant / keterjangkauan     ← 03-frontend-architecture.md
  -> brief produk atau UI yang disetujui            ← DOKUMEN INI
     -> konvensi dan design system project
        -> DEV_DISCRETION
```

Artinya:

| Hal | Keadaannya setelah dokumen ini berlaku |
| --- | --- |
| Aturan bagian 2A, 2B, 3, 3A, 5.4, dan 6 pada `03-frontend-architecture.md` | **Tetap menang.** Skema di bawah tidak boleh melanggarnya, dan bila ada yang berselisih, dokumen inilah yang salah |
| `RWI-FE-003` nama dan label langkah | **Diusulkan ditutup** oleh dokumen ini setelah mendapat persetujuan pemilik |
| `RWI-FE-004` bentuk penanda langkah | **Diusulkan ditutup** oleh dokumen ini setelah mendapat persetujuan pemilik |
| Warna, ukuran, jarak, ikon, tipografi | **Tetap** `DEV_DISCRETION`, mengikuti design system project |
| Pilihan komponen konkret | **Tetap** `DEV_DISCRETION`, tetapi bagian 4.4 menyebut komponen yang sudah ada supaya tidak dibuat kembar |

**Skema di bawah adalah usulan sampai disetujui pemilik.** Sebelum disetujui, ia tidak mengikat
pelaksana dan tidak boleh dipakai sebagai dasar acceptance criteria.

---

## 1. Cara membaca skema

Setiap layar ditulis dengan empat bagian tetap:

| Bagian | Isinya |
| --- | --- |
| **Kerangka** | Gambar rangka ASCII. Yang mengikat adalah **susunan dan urutan wilayah**, bukan lebar, warna, atau ikonnya |
| **Wilayah** | Tabel: nama wilayah → isi → dari mana datanya → komponen yang sudah ada |
| **Tombol** | Tabel: label → jenis → kapan aktif → apa yang terjadi |
| **Keadaan** | Memuat, kosong, gagal, dan tidak berhak — khusus untuk layar itu |

Tanda pada rangka:

```text
[ Tombol ]      tombol             ( ) pilihan tunggal      [ o] sakelar
[ isian      ]  isian teks         (•) terpilih             ▾ daftar pilihan
(×) tidak dapat dipilih            ⚠ peringatan             * wajib diisi
```

---

## 2. Peta modul dan menu

Register ini memetakan seluruh layar fungsional. Target navigasinya memisahkan **tujuh menu
operasional** pada `Pelayanan Kesehatan → Rawat Inap` dari **dua menu master/configuration Rawat
Inap** pada `Pelayanan Kesehatan → Master Data`. Layar per episode tetap dicapai dari Daftar Kerja
Episode atau Detail Episode sesuai `IA-INP-05`.

### Modul: Health Services / Rawat Inap

| Butir navigasi / layar | Induk target | Layar fungsional | Skema |
| --- | --- | --- | :---: |
| Beranda Rawat Inap — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-19` | ✅ **bagian 5** |
| Admisi Rawat Inap — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-03` | ✅ **bagian 3** |
| Papan Tempat Tidur — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-02` | ✅ **bagian 7** |
| Daftar Kerja Episode — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-16` | ✅ **bagian 6** |
| Pasien Sedang Dirawat — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-01` | ✅ **bagian 8** |
| Daftar Pantau — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-09` | ✅ **bagian 14** |
| Selisih Tempat Tidur — menu operasional | `Pelayanan Kesehatan → Rawat Inap` | `FE-INP-10` | ✅ **bagian 15** |
| Pengaturan Rawat Inap — master/configuration | `Pelayanan Kesehatan → Master Data` | `FE-INP-12` | ✅ **bagian 17** |
| Butir Administrasi Rawat Inap — master/configuration | `Pelayanan Kesehatan → Master Data` | `FE-INP-13` | ✅ **bagian 18** |
| Detail Episode | Daftar Kerja Episode / Census | `FE-INP-04` | ✅ **bagian 9** |
| Perpindahan Pasien | Detail Episode | `FE-INP-05` | ✅ **bagian 10** |
| Keputusan Pulang dan Resume | Detail Episode | `FE-INP-06` | ✅ **bagian 11** |
| Penutupan Episode | Detail Episode | `FE-INP-07` | ✅ **bagian 12** |
| Kelayakan Keuangan | Detail Episode | `FE-INP-08` | ✅ **bagian 13** |
| Sesi Koreksi | Detail Episode `Closed` | `FE-INP-11` | ✅ **bagian 16** |
| Pencatatan Kepergian | Detail Episode | `FE-INP-14` | ✅ **bagian 19** |
| Kebutuhan Isolasi | Detail Episode / alur admisi | `FE-INP-15` | ✅ **bagian 20** |
| Pembatalan Admisi | Daftar Kerja Episode / Detail Episode | `FE-INP-17` | ✅ **bagian 21** |
| Cetak Persetujuan | Alur admisi / Detail Episode | `FE-INP-18` | ✅ **bagian 22**; terhubung juga dari langkah 8 bagian 3.10 |

---

## 3. Menu: Admisi Rawat Inap — `FE-INP-03`

Route: `/health-services/inpatient-management/admissions`

Urutan bernama dan isi langkah sudah dikunci `03-frontend-architecture.md` bagian 3A. Jalur pasien
baru memuat sembilan langkah; jumlah resmi jalur pasien lama masih tertahan `RWI-UI-GAP-001`.
Dokumen ini menambahkan **susunan wilayah di layar** dan **kata yang dipakai**.

### 3.0 Kerangka halaman

Berlaku untuk seluruh langkah kecuali layar pembuka.

```text
┌─ sidebar ─┬─────────────────────────────────────────────────────────────┐
│           │  Health Services / Rawat Inap                               │
│  Rawat    │  Admisi Rawat Inap                                          │
│  Inap  ▸  │  Daftarkan pasien, tentukan penjamin dan DPJP, lalu pesan   │
│           │  tempat tidurnya.                                           │
│           ├─────────────────────────────────────────────────────────────┤
│           │  ①─②─③─④─⑤─⑥─⑦─⑧─⑨            PENANDA LANGKAH        │
│           ├─────────────────────────────────────────────────────────────┤
│           │                                                             │
│           │   ISI LANGKAH                                               │
│           │                                                             │
│           ├─────────────────────────────────────────────────────────────┤
│           │  RINGKASAN BERJALAN                                         │
│           │  Sari Dewi · RM 00123456 · BPJS Kelas 1 · Melati · ML-101-A │
│           ├─────────────────────────────────────────────────────────────┤
│           │                        [ Kembali ]   [ Lanjut ke Dokter ]  │
└───────────┴─────────────────────────────────────────────────────────────┘
```

| Wilayah | Isi | Dari mana | Komponen yang sudah ada |
| --- | --- | --- | --- |
| Kepala halaman | Remah jejak, judul menu, satu kalimat penjelas | tetap | `Hero` |
| Penanda langkah | Urutan langkah sesuai jalur, termasuk yang sedang berjalan dan sudah lewat | keadaan alur | `emergency-registration-stepper` |
| Isi langkah | Berganti per langkah, bagian 3.2 s.d. 3.13 | — | — |
| Ringkasan berjalan | Pasien, nomor RM, penjamin, kelas, unit, tempat tidur — **hanya yang sudah terisi** | keadaan alur | pola `patientCompactSummary` |
| Aksi langkah | Kembali dan lanjut | — | pola `stepActions` |

**Tiga aturan yang mengikat pada kerangka ini:**

1. **Penanda langkah tidak dapat diklik untuk melompat.** Ia penunjuk posisi, bukan navigasi.
   Melompat merusak urutan titik tulis pada `03-frontend-architecture.md` 3A.4.
2. **Ringkasan berjalan tidak pernah memuat kolom sensitif.** Tanpa diagnosis, tanpa catatan
   episode, tanpa keterangan kebutuhan isolasi, dan tanpa nomor kartu penjamin — bagian 6.
   Penanda isolasi boleh tampil sebagai ikon tanpa alasannya.
3. **Langkah yang sedang berjalan tercermin di URL.** Memuat ulang halaman mengembalikan pengguna
   ke langkah yang sama, bukan ke langkah 1 — `03-frontend-architecture.md` 5.5.

---

### 3.1 Layar pembuka — Pilih Tipe Pendaftaran

Tampil **sebelum** penanda langkah muncul, sama seperti pendaftaran IGD.

```text
                        Pilih Tipe Pendaftaran
        Tentukan apakah pasien perlu didaftarkan lebih dulu atau
        sudah punya nomor rekam medis.

  ┌──────────────────────────────┐  ┌──────────────────────────────┐
  │            [+ orang]         │  │          [orang orang]       │
  │                              │  │                              │
  │   Pendaftaran Pasien Baru    │  │   Pendaftaran Pasien Lama    │
  │   Belum pernah terdaftar     │  │   Sudah punya No. RM         │
  │                              │  │                              │
  │   ✓ Scan KTP mengisi form    │  │   ✓ Cari No. RM atau NIK     │
  │   ✓ Sembilan langkah         │  │   ✓ Jumlah menunggu putusan  │
  │                              │  │                              │
  │        Klik untuk memilih    │  │        Klik untuk memilih    │
  └──────────────────────────────┘  └──────────────────────────────┘
```

| Wilayah | Isi | Komponen |
| --- | --- | --- |
| Judul | "Pilih Tipe Pendaftaran" beserta satu kalimat penjelas | teks |
| Dua kartu | Judul, keterangan singkat, dua butir pembeda | `patient-entry-choice-step` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kartu Pasien Baru | kartu | selalu | Masuk jalur pasien baru, langkah 1 |
| Kartu Pasien Lama | kartu | selalu | Masuk jalur pasien lama, langkah 1 |

**Keadaan tidak berhak:** peran tanpa `InpatientEpisode : Create` **tidak membuka layar ini sama
sekali** — bukan membukanya lalu menemukan kartu yang mati.

---

### 3.2 Langkah 1 — Tipe Pasien

Berlaku pada kedua jalur. Pada jalur pasien lama, ini langkah ketiga pada penanda.

```text
  LANGKAH 1
  Jenis Pasien

  ┌──────────┐ ┌──────────┐ ┌──────────────┐ ┌──────────┐
  │ (•) Umum │ │ ( ) Ibu  │ │ ( ) Bayi     │ │ ( ) Anak │
  │          │ │          │ │  Baru Lahir  │ │          │
  └──────────┘ └──────────┘ └──────────────┘ └──────────┘
  ┌──────────────┐ ┌──────────────┐
  │ ( ) Pegawai  │ │ ( ) Korporat │
  └──────────────┘ └──────────────┘

  ── tampil HANYA bila Bayi Baru Lahir dipilih ──────────────────────
  ┌── Episode Ibu ───────────────────────────────────────────────────┐
  │ Episode Ibu *  [ Cari nama ibu atau nomor episode…           ▾ ] │
  │ Bayi mendapat episode, kunjungan, dan hitungan hari rawat        │
  │ sendiri. Kolom ini hanya merekam hubungannya dengan ibu.         │
  └──────────────────────────────────────────────────────────────────┘

                         [ Lanjut ke Pendaftaran / Pembayaran ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Kartu jenis pasien | Enam pilihan tunggal | tetap | `base-checkbox-card` |
| Episode ibu | Isian pilihan episode aktif | daftar episode tersaring `Admitted` | `ResourceFilterSelect` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Pendaftaran | utama | Jalur pasien baru; jenis terpilih; bila Bayi Baru Lahir, episode ibu wajib terisi | Maju ke Pendaftaran |
| Lanjut ke Pembayaran | utama | Jalur pasien lama; syarat jenis pasien sama | Maju langsung ke Pembayaran; **tidak** membuka form pasien baru |

**Catatan yang mengikat.** Wilayah Episode Ibu **hanya** muncul untuk Bayi Baru Lahir. Menampilkannya
selalu membuat petugas mengira setiap admisi menuntut episode ibu.

---

### 3.3 Langkah 2 — Pendaftaran (jalur pasien baru)

```text
  LANGKAH 2
  Pendaftaran Pasien Baru

  ┌── Scan KTP ──────────────────────────────────────────────────────┐
  │  [ Mulai Scan KTP ]        Menghubungkan ke pemindai…            │
  │  Pemindai tidak tersedia? Isi formulir di bawah secara manual.   │
  └──────────────────────────────────────────────────────────────────┘

  ┌── Identitas Pasien ──────────────────────────────────────────────┐
  │ NIK *          [ 3273xxxxxxxxxxxx        ]                       │
  │ Nama Lengkap * [ Sari Dewi               ]                       │
  │ Tgl Lahir *    [ 12-04-1988 ]   Jenis Kelamin * ( ) L  (•) P     │
  │ Alamat         [                                            ]    │
  │ No. HP         [ 0812xxxxxxx ]  Email  [                    ]    │
  └──────────────────────────────────────────────────────────────────┘

  ┌── Kontak Darurat ────────────────────────────────────────────────┐
  │ Nama *  [ Budi Santoso ]  Hubungan * [ Suami  ▾ ]                │
  │ No. HP *[ 0813xxxxxxx  ]                                         │
  └──────────────────────────────────────────────────────────────────┘

                            [ Kembali ]   [ Simpan & Lanjut ke Pembayaran ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Scan KTP | Tombol pindai dan keadaan sambungannya | jembatan pemindai | `plustek-scan-panel` |
| Identitas Pasien | Isian pasien baru | isian pengguna | `new-patient-form`, `base-form-control` |
| Kontak Darurat | Nama, hubungan, nomor | isian pengguna | idem |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Mulai Scan KTP | kedua | Pemindai tersedia | Mengisi otomatis wilayah Identitas |
| Kembali | kedua | selalu | Kembali ke langkah 1 |
| Simpan & Lanjut ke Pembayaran | utama | Seluruh isian wajib terisi; **mati selama permintaan berjalan** | Berurutan: `POST /patients` → `POST /patient-identity-documents` → `POST /patient-emergency-contacts`; baru lalu maju. Kegagalan berhenti pada operasi terkait |

**Keadaan:**

| Keadaan | Yang tampil |
| --- | --- |
| Pemindai tidak tersedia | Wilayah Scan KTP tetap tampil disertai kalimat bahwa formulir dapat diisi manual. **Bukan** wilayah yang hilang tanpa penjelasan |
| Server menolak | `InformationAlert` merah berisi pesan server apa adanya, **di atas** formulir. Isian **tidak hilang** |

---

### 3.4 Langkah 1–2 jalur pasien lama — Pasien Lama dan Informasi Pasien Lama

```text
  PASIEN LAMA
  Cari Data Pasien

  ┌── Cara Mencari ──────────────────────────────────────────────────┐
  │  ┌──────────────┐  ┌──────────────┐                              │
  │  │ (•) [RM]     │  │ ( ) [ID]     │                              │
  │  │  Nomor RM    │  │  NIK         │                              │
  │  └──────────────┘  └──────────────┘                              │
  │                                                                  │
  │  [ 00123456                                    ]  [ Cari ]       │
  │  Ketik nomor rekam medis, atau tempelkan kartu pasien.           │
  └──────────────────────────────────────────────────────────────────┘

  ── sesudah ditemukan, layar berganti ke Informasi Pasien Lama ─────

  INFORMASI PASIEN LAMA
  Periksa Data Pasien

  ┌── Identitas ──────────┐ ┌── Validasi Data ────────────────────────┐
  │      [foto/inisial]   │ │ NIK          3273xxxxxxxxxxxx           │
  │      Sari Dewi        │ │ Tgl Lahir    12-04-1988 (38 th)         │
  │      RM 00123456      │ │ Jenis Kelamin Perempuan                 │
  │      Perempuan        │ │ No. HP       0812xxxxxxx                │
  │                       │ │ Alamat       …                          │
  │  [ Ganti Pasien ]     │ │ Kunjungan terakhir  02-08-2026 Poli PD  │
  └───────────────────────┘ └─────────────────────────────────────────┘

                              [ Kembali ]   [ Lanjut ke Tipe Pasien ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Cara Mencari | Dua kartu cara cari, satu isian, satu tombol | isian pengguna | `patient-selection-step` |
| Identitas | Foto atau inisial, nama, nomor RM, jenis kelamin | `patients/options` | idem |
| Validasi Data | Data pasien untuk ditinjau | idem | idem |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cari | utama | Isian terisi | Mencari pasien |
| Ganti Pasien | kedua | selalu | Kembali ke pencarian |
| Lanjut ke Tipe Pasien | utama | Satu pasien terpilih | Maju ke langkah Tipe Pasien |

**Keadaan:**

| Keadaan | Yang tampil |
| --- | --- |
| Tidak ditemukan | "Pasien dengan nomor itu tidak ditemukan." disertai tautan **Daftarkan sebagai pasien baru** yang memindahkan ke jalur pasien baru |
| Lebih dari satu hasil | Daftar hasil untuk dipilih, memuat nama, nomor RM, dan tanggal lahir. **Tanpa** NIK penuh |

---

### 3.5 Langkah 3 — Pembayaran

**Inilah langkah yang tidak pernah ada pada revision `0.3`.** Kelas perawatan ikut dipilih di sini.

```text
  LANGKAH 3
  Cara Bayar dan Kelas Perawatan

  ┌── Cara Bayar ────────────────────────────────────────────────────┐
  │ ┌──────────────┐ ┌──────────────┐ ┌────────────────────────────┐ │
  │ │ ( )   Rp     │ │ (•)    ▣     │ │ ( )          ▣             │ │
  │ │ Tunai / Umum │ │  Asuransi    │ │  Penjamin Perusahaan       │ │
  │ └──────────────┘ └──────────────┘ └────────────────────────────┘ │
  └──────────────────────────────────────────────────────────────────┘

  ── tampil bila Asuransi atau Penjamin Perusahaan dipilih ──────────
  ┌── Kartu Penjamin Pasien ──────────┐ ┌── Penjamin Dipilih ───────┐
  │                  [ + Tambah Kartu ]│ │  BPJS Kesehatan          │
  │ ┌───────────────────────────────┐ │ │  No. 000123456789        │
  │ │ (•) BPJS Kesehatan            │ │ │  Kelas hak    Kelas 1    │
  │ │     000123456789 · Aktif      │ │ │  Berlaku s.d. 31-12-2026 │
  │ ├───────────────────────────────┤ │ │  [ Aktif ]               │
  │ │ ( ) Prudential                │ │ │                          │
  │ │     A-99887766 · Aktif        │ │ │                          │
  │ └───────────────────────────────┘ │ │                          │
  └───────────────────────────────────┘ └──────────────────────────┘

  ┌── Kelas Perawatan ───────────────────────────────────────────────┐
  │ Kelas *  [ Kelas 1                                          ▾ ]  │
  │ Kelas yang ditagihkan nanti mengikuti kamar yang benar-benar     │
  │ ditempati.                                                       │
  └──────────────────────────────────────────────────────────────────┘

                                  [ Kembali ]   [ Lanjut ke Dokter ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Cara Bayar | Tiga kartu pilihan tunggal | tetap | `payment-method-step` |
| Kartu Penjamin Pasien | Daftar kartu milik pasien beserta keadaannya | `patient-insurances`, `patient-company-guarantors` | `patient-payer-table` |
| Penjamin Dipilih | Rincian kartu yang dipilih | idem | `selectedPayerPanel` |
| Kelas Perawatan | Isian pilihan kelas | isian pilihan sumber daya | `ResourceFilterSelect` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| + Tambah Kartu | kedua | Cara bayar bukan tunai | Membuka laci pendaftaran kartu — `patient-payer-drawer` |
| Kembali | kedua | selalu | Kembali ke langkah sebelumnya |
| Lanjut ke Dokter | utama | Cara bayar dipilih; bila bukan tunai, satu kartu terpilih; kelas terisi | Maju |

**Empat aturan yang mengikat pada langkah ini:**

1. **Tidak ada cara bayar yang terpilih otomatis.** Ketiga kartu berangkat kosong. Menyalakan
   "Tunai" sebagai bawaan mengulang cacat yang justru ditutup revision `0.4`.
2. **Nomor kartu penuh hanya tampil di langkah ini dan pada formulir cetak** — bagian 6.
3. Kalimat "Kelas hak" pada wilayah Penjamin Dipilih adalah **keterangan, bukan aturan**. Tidak ada
   aturan backend yang menolak kelas di luar hak peserta; jangan menuliskannya seolah menolak.
4. Wilayah kartu penjamin **tidak muncul** ketika Tunai dipilih. Yang muncul satu kartu ringkas
   bertuliskan "Pembayaran Tunai / Umum" beserta penanda "Dipilih".

---

### 3.6 Langkah 4 — Dokter

Langkah paling berat akibatnya: di sinilah **titik tulis 1** terjadi.

```text
  LANGKAH 4
  Unit Tujuan, DPJP, dan Kebutuhan Isolasi

  ┌── Tujuan Perawatan ──────────────────────────────────────────────┐
  │ Unit Layanan *  [ Ruang Melati — Rawat Inap                  ▾ ] │
  │ DPJP *          [ dr. Andi Wijaya, Sp.PD                     ▾ ] │
  │ Catatan Admisi  [                                             ] │
  │                 Opsional. Paling panjang 1000 karakter.         │
  └──────────────────────────────────────────────────────────────────┘

  ┌── Kebutuhan Isolasi ─────────────────────────────────────────────┐
  │ [ o]  Pasien membutuhkan isolasi                                 │
  │                                                                  │
  │ Keterangan *  [ Rujukan menyebut suspek TB paru aktif        ]   │
  │ Wajib diisi ketika kebutuhan isolasi dinyalakan. Keterangan ini  │
  │ tidak ditampilkan pada census maupun papan tempat tidur.         │
  └──────────────────────────────────────────────────────────────────┘

  ⚠  Setelah langkah ini disimpan, kunjungan dan episode terbentuk.
     Penjamin dan cara bayar TIDAK dapat diubah lagi. Bila keliru,
     admisi harus dibatalkan lalu dibuka ulang.

                     [ Kembali ]   [ Simpan & Cari Tempat Tidur ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Tujuan Perawatan | Unit layanan **bertipe rawat inap saja**, DPJP, catatan | isian pilihan sumber daya | `ResourceFilterSelect`, `BaseTextAreaField` |
| Kebutuhan Isolasi | Sakelar dan keterangan | isian pengguna | `BaseCheckboxField`, `BaseTextAreaField` |
| Peringatan | Kalimat tetap tentang akibat menyimpan | tetap | `InformationAlert` variasi peringatan |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali | kedua | selalu | Kembali ke Pembayaran |
| Simpan & Cari Tempat Tidur | utama | Unit dan DPJP terisi; bila isolasi menyala, keterangan terisi; **mati selama permintaan berjalan** | `POST /patient-encounters` → `POST /episodes` → `PATCH …/isolation-requirement`, lalu maju |

**Empat aturan yang mengikat:**

1. **Peringatan tampil sebelum disimpan, bukan sesudah.** Ini tuntutan `03-frontend-architecture.md`
   3A.5 dan alasan wilayah peringatan ada di rangka.
2. **Bila `POST /patient-encounters` gagal, alur berhenti di sini.** Jangan meneruskan ke
   `POST /episodes`. Pesan server ditampilkan apa adanya, isian utuh.
3. Sesudah berhasil, nomor episode tampil pada ringkasan berjalan. DPJP dan penjamin terkunci.
   Unit, kelas, dan catatan masih dapat dikoreksi pada Konfirmasi melalui `PUT /episodes/{id}`;
   mengubah DPJP adalah pengalihan dan bukan edit form.
4. Setelah titik tulis 1, tombol Kembali **tidak lagi** membawa ke Pembayaran. Ia dinonaktifkan
   disertai keterangan "Penjamin sudah terkunci pada kunjungan. Batalkan admisi bila keliru."

---

### 3.7 Langkah 5 — Pilih Bed

```text
  LANGKAH 5
  Pilih Tempat Tidur

  ┌ Tersedia 12 ┐ ┌ Dipesan 3 ┐ ┌ Terisi 40 ┐ ┌ Ditutup 2 ┐
  └─────────────┘ └───────────┘ └───────────┘ └───────────┘
  12 tempat tidur dapat dipilih untuk pasien ini.

  Penyaring:  [ Unit: Melati ▾ ]  [ Kelas: Kelas 1 ▾ ]
              [ o] tampilkan juga yang tidak dapat dipakai

  ┌ Ruang Melati 101 · Kelas 1 · dihuni pasien perempuan ────────────┐
  │  ( )  ML-101-A    Tersedia                                       │
  │  ( )  ML-101-B    Tersedia                                       │
  └──────────────────────────────────────────────────────────────────┘
  ┌ Ruang Melati 102 · Kelas 1 ──────────────────────────────────────┐
  │  ( )  ML-102-A    Dipesan · sisa 01:24 · EP-2026-000188          │
  │  (×)  ML-102-B    Tidak dapat dipakai — kamar sedang ditempati   │
  │                   pasien laki-laki                               │
  └──────────────────────────────────────────────────────────────────┘

                      [ Kembali ]   [ Lanjut ke Pemesanan ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Ringkasan keadaan | Empat angka keadaan tempat tidur | `bed-board` | `inpatient-bed-board` |
| Baris jumlah terpilih | Berapa yang benar-benar dapat dipilih | `available-beds` | idem |
| Penyaring | Unit, kelas, sakelar tampilkan yang tidak layak | isian pengguna | `FilterSelect` |
| Daftar per kamar | Kamar sebagai kartu, tempat tidur sebagai baris | `available-beds` | idem |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali | kedua | selalu | Kembali ke langkah Dokter dalam mode koreksi pascatitik tulis 1: unit, kelas, dan catatan dapat diubah melalui `PUT`; DPJP dan penjamin tetap terkunci |
| Lanjut ke Pemesanan | utama | Satu tempat tidur yang **dapat dipilih** terpilih | Maju ke Booking Bed |

**Lima aturan yang mengikat:**

1. **Daftar berasal hanya dari `available-beds`.** Layar tidak menyaring ulang dengan aturannya
   sendiri — `03-frontend-architecture.md` 4.3A.
2. Tempat tidur yang tidak layak ditampilkan sebagai baris **redup dan tidak dapat dipilih**,
   disertai alasan dari server **apa adanya**, termasuk nama kamarnya.
3. Baris `Dipesan` menampilkan sisa waktu dan nomor episode pemegangnya bagi peran yang berhak;
   **nama pasien tidak** ditampilkan pada layar admisi.
4. Sakelar "tampilkan juga yang tidak dapat dipakai" **menyala** secara bawaan. Petugas perlu tahu
   tempat tidurnya ada tetapi tidak boleh dipakai, bukan mengira kamarnya penuh.
5. Daftar dimuat ulang **sebelum** dialog konfirmasi pemesanan tampil — bagian 5.2.

---

### 3.8 Langkah 6 — Booking Bed

Dua keadaan: sebelum dipesan, dan sesudah dipesan.

```text
  LANGKAH 6 — sebelum dipesan
  Pesan Tempat Tidur

  ┌── Tempat Tidur Dipilih ──────────────────────────────────────────┐
  │  ML-101-A                                                        │
  │  Ruang Melati 101 · Kelas 1 · Ruang Melati                       │
  │                                                                  │
  │  Masa pemesanan mengikuti pengaturan server. Selama aktif, bed   │
  │  tidak dapat dipesan pasien lain.                                │
  └──────────────────────────────────────────────────────────────────┘

                     [ Kembali ]   [ Pesan Tempat Tidur ]


  LANGKAH 6 — sesudah dipesan
  Tempat Tidur Sudah Dipesan

  ┌── ML-101-A · Ruang Melati 101 · Kelas 1 ─────────────────────────┐
  │                                          Sisa waktu   01:58:12   │
  │                                                                  │
  │  Pasien menjadi "Sedang Dirawat" setelah kedatangannya           │
  │  dikonfirmasi petugas admisi di Papan Tempat Tidur.              │
  │                                                                  │
  │  [ Batalkan Pemesanan ]                                          │
  └──────────────────────────────────────────────────────────────────┘

                    [ Kembali ]   [ Lanjut ke Konfirmasi ]
```

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Pesan Tempat Tidur | utama | Belum ada pemesanan aktif; **mati selama permintaan berjalan** | Dialog konfirmasi menyebut nama tempat tidur, lalu `POST /reservations` |
| Batalkan Pemesanan | kedua, nada bahaya | Ada pemesanan aktif | Dialog konfirmasi, lalu `PATCH …/reservations/{id}/cancel` |
| Kembali | kedua | selalu | **Bila ada pemesanan aktif**, pengguna diminta membatalkannya lebih dulu — 3A.5 |
| Lanjut ke Konfirmasi | utama | Ada pemesanan aktif | Maju |

**Tiga aturan yang mengikat:**

1. **Sisa waktu boleh dihitung mundur di layar, tetapi keputusan gugur bukan milik layar.** Angka
   mundur diturunkan dari waktu kedaluwarsa yang dijawab server. Ketika angkanya habis, layar
   **memuat ulang** dan menampilkan apa yang dijawab server — bukan menyatakan sendiri bahwa
   pemesanan sudah gugur. Dasarnya `03-frontend-architecture.md` 3A.6.
2. Pemesanan gugur saat layar terbuka mengembalikan pengguna ke langkah Pilih Bed disertai kalimat
   yang menyebutkan pemesanan sebelumnya gugur — bagian 5.5.
3. Layar ini **tidak pernah** memanggil `POST /placements`. Kalimat "pasien menjadi Sedang Dirawat
   setelah kedatangannya dikonfirmasi" wajib ada supaya petugas tidak menunggu sesuatu yang tidak
   akan terjadi di sini.

---

### 3.9 Langkah 7 — Konfirmasi

```text
  LANGKAH 7
  Periksa Kembali Sebelum Dikunci

  ┌── Pasien ───────────────────┐ ┌── Penjamin ─────────────────────┐
  │ Sari Dewi                   │ │ Asuransi — BPJS Kesehatan       │
  │ RM 00123456 · Perempuan     │ │ No. ****6789                    │
  │ 12-04-1988 (38 th)          │ │ Kelas Perawatan  Kelas 1        │
  └─────────────────────────────┘ └─────────────────────────────────┘
  ┌── Perawatan ────────────────┐ ┌── Tempat Tidur ─────────────────┐
  │ Episode  EP-2026-000204     │ │ ML-101-A                        │
  │ Unit     Ruang Melati       │ │ Ruang Melati 101 · Kelas 1      │
  │ DPJP     dr. Andi W, Sp.PD  │ │ Dipesan · sisa 01:52            │
  │ Isolasi  Ya  [ikon]         │ │                                 │
  │ Status   Admisi disiapkan   │ │                                 │
  └─────────────────────────────┘ └─────────────────────────────────┘

  ┌── Yang Masih Dapat Diubah ───────────────────────────────────────┐
  │ Unit Layanan  [ Ruang Melati — Rawat Inap                    ▾ ] │
  │ Kelas         [ Kelas 1                                      ▾ ] │
  │ Catatan       [                                               ] │
  │ DPJP dan penjamin tidak dapat diubah dari sini.                 │
  └──────────────────────────────────────────────────────────────────┘

  Langkah berikutnya: cetak persetujuan, lalu konfirmasi kedatangan
  pasien di Papan Tempat Tidur.

              [ Kembali ]   [ Kunci Admisi & Cetak Persetujuan ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Empat kartu ringkasan | Pasien, penjamin, perawatan, tempat tidur | keadaan alur dan `GET /episodes/{id}` | `base-detail-card`, `SummaryGrid` |
| Yang Masih Dapat Diubah | Unit, kelas, catatan | isian pengguna | `BaseEditorForm` |
| Kalimat langkah berikutnya | Tetap | tetap | teks |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Kembali | kedua | selalu | Kembali ke Booking Bed |
| Kunci Admisi & Cetak Persetujuan | utama | selalu; **mati selama permintaan berjalan** | `PUT /episodes/{id}` bila ada perubahan, lalu maju ke Cetak Persetujuan |

**Tiga aturan yang mengikat:**

1. Kartu Perawatan menampilkan penanda isolasi sebagai **ikon atau kata "Ya"**, tanpa
   keterangannya — bagian 6.
2. Status yang tampil adalah status yang **dijawab server**, bukan kalimat tetap. Setelah langkah
   ini pun episodenya tetap "Admisi sedang disiapkan".
3. Wilayah Yang Masih Dapat Diubah **menyebutkan sendiri** apa yang tidak dapat diubah. Menyembunyikan
   DPJP dan penjamin tanpa penjelasan membuat petugas mencarinya berputar-putar.

---

### 3.10 Langkah 8 — Cetak Persetujuan Pasien Ranap

```text
  LANGKAH 8
  Cetak Persetujuan Rawat Inap

  ┌──────────────────────────────────────────────────────────────────┐
  │  [ pratinjau formulir, satu halaman ]                            │
  │                                                                  │
  │   PERSETUJUAN UMUM RAWAT INAP                                    │
  │   Pasien   Sari Dewi · RM 00123456 · 12-04-1988                  │
  │   Penjamin BPJS Kesehatan · 000123456789                         │
  │   Unit     Ruang Melati · Kelas 1 · DPJP dr. Andi W, Sp.PD       │
  │   Episode  EP-2026-000204        Tanggal  27-08-2026             │
  │   ────────────────────────────────────────────────────           │
  │   1. Persetujuan tindakan kedokteran umum                        │
  │   2. Persetujuan pemberian informasi kepada penjamin             │
  │   3. Penunjukan orang yang boleh menerima informasi              │
  │   ────────────────────────────────────────────────────           │
  │   Tanda tangan pasien / keluarga     Petugas admisi              │
  └──────────────────────────────────────────────────────────────────┘

  ⓘ Formulir ini dicetak, tidak disimpan sistem. Lembar bertanda
    tangan disimpan sesuai tata kelola berkas rekam medis.

                        [ Cetak ]   [ Lanjut ke Kartu Pasien ]
```

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cetak | utama | selalu | Membuka dialog cetak peramban |
| Lanjut ke Kartu Pasien | utama | selalu | Maju. **Pada jalur pasien lama**, tombol ini berbunyi **Selesai** dan menutup alur |

**Dua aturan yang mengikat:**

1. Layar **tidak boleh** menyatakan persetujuan sudah tersimpan atau sudah ditandatangani.
   Kalimat berikon ⓘ di atas adalah bagian dari skema, bukan hiasan — dasarnya `RWI-DEC-077`.
2. Melewati langkah ini tanpa mencetak **tidak** membatalkan apa pun. Admisi sudah terkunci pada
   langkah 7.

---

### 3.11 Langkah 9 — Kartu Pasien

Hanya pada jalur pasien baru.

```text
  LANGKAH 9
  Cetak Kartu Pasien

  ┌── Kartu Pasien ──────────────────────────────────────────────────┐
  │   [ pratinjau kartu ]                                            │
  │   Sari Dewi                                                      │
  │   RM 00123456                                                    │
  └──────────────────────────────────────────────────────────────────┘

  ✓ Admisi selesai. Episode EP-2026-000204 menunggu konfirmasi
    kedatangan di Papan Tempat Tidur.

  [ Cetak Kartu ]   [ Buka Papan Tempat Tidur ]   [ Admisi Baru ]
```

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Cetak Kartu | utama | selalu | Dialog cetak |
| Buka Papan Tempat Tidur | kedua | selalu | Menuju `FE-INP-02` — memenuhi `IA-INP-01` |
| Admisi Baru | kedua | selalu | Mengosongkan alur dan kembali ke layar pembuka |

---

### 3.12 Ringkasan tombol utama seluruh langkah

Satu tabel supaya kata yang dipakai tidak berselisih antar langkah.

| Langkah | Tombol utama | Menulis ke server? |
| --- | --- | :---: |
| Pembuka | kartu tipe pendaftaran | – |
| 1 Tipe Pasien | Lanjut ke Pendaftaran **atau** Lanjut ke Pembayaran, mengikuti jalur | – |
| 2 Pendaftaran | Simpan & Lanjut ke Pembayaran | **ya** |
| Pasien Lama | Lanjut ke Tipe Pasien | – |
| 3 Pembayaran | Lanjut ke Dokter | ya, bila kartu baru didaftarkan |
| 4 Dokter | Simpan & Cari Tempat Tidur | **ya — titik tulis 1** |
| 5 Pilih Bed | Lanjut ke Pemesanan | – |
| 6 Booking Bed | Pesan Tempat Tidur | **ya — titik tulis 2** |
| 7 Konfirmasi | Kunci Admisi & Cetak Persetujuan | **ya — titik tulis 3** |
| 8 Cetak Persetujuan | Cetak | – |
| 9 Kartu Pasien | Cetak Kartu | – |

**Pola kata yang mengikat:** tombol yang menulis ke server diawali kata kerja yang menyebut
tulisannya — "Simpan", "Pesan", "Kunci". Tombol yang hanya berpindah langkah diawali "Lanjut ke".
Petugas harus dapat menebak dari labelnya apakah menekan tombol itu mengubah data.

---

### 3.13 Keluar dari alur di tengah jalan

```text
  ┌── Tinggalkan Admisi? ────────────────────────────────────────────┐
  │                                                                  │
  │  Episode EP-2026-000204 sudah terbentuk dan berstatus            │
  │  "Admisi sedang disiapkan".                                      │
  │                                                                  │
  │  Tempat tidur ML-101-A masih dipesan, sisa 01:41.                │
  │                                                                  │
  │  Anda dapat melanjutkannya nanti dari Daftar Kerja Episode.      │
  │                                                                  │
  │            [ Tetap di Sini ]   [ Tinggalkan Admisi ]             │
  └──────────────────────────────────────────────────────────────────┘
```

Muncul ketika pengguna meninggalkan alur **setelah titik tulis 1**. Sebelum titik tulis 1, tidak ada
apa pun di server dan dialog ini tidak perlu muncul.

Isi dialog menyesuaikan keadaan:

| Keadaan | Kalimat yang berubah |
| --- | --- |
| Belum ada pemesanan | Baris tempat tidur tidak ada |
| Pemesanan aktif | Baris tempat tidur menyebut nama dan sisa waktunya |
| Alur belum melewati titik tulis 1 | Dialog **tidak muncul** |

---

## 4. Kontrak lintas layar

### 4.1 Keadaan yang berlaku di seluruh langkah dan layar

| Keadaan | Yang tampil | Letaknya |
| --- | --- | --- |
| Sedang memuat isian pilihan | Penanda memuat di dalam isian, bukan layar kosong | dalam wilayah |
| Sedang menyimpan | Seluruh aksi tulis dan navigasi yang dapat memicu request kedua dinonaktifkan; tombol utama berpenanda memuat | wilayah aksi |
| Gagal — pesan server ada | `InformationAlert` merah berisi pesan server **apa adanya** | di atas isi langkah |
| Gagal — 409 | Pesan server, ditambah pemuatan ulang data yang basi. Isian **tidak hilang** | idem |
| Gagal — 422 | **Daftar** aturan yang gagal, satu baris per aturan | idem |
| Tidak berhak | Layar tidak dibuka sama sekali | `AccessDeniedGate` |
| Berhasil satu langkah | Notifikasi singkat, lalu berpindah langkah | `ToastStack` |

**Yang tidak boleh:** menggantikan pesan server dengan kalimat buatan sendiri, terutama pada
penolakan Kelayakan Penempatan yang menyebut nama kamar.

Setiap layar daftar juga wajib membedakan memuat, kosong, gagal, dan tidak berhak. Data dari
request yang gagal tidak boleh diganti angka nol atau daftar kosong, karena itu mengubah arti
operasional.

### 4.2 Responsif dan aksesibilitas

| Area | Ketetapan usulan |
| --- | --- |
| Layar lebar | Sidebar dan isi berdampingan; tabel, papan, serta dua-kolom form boleh memakai lebar penuh |
| Layar sedang | Filter membungkus ke beberapa baris; wilayah dua kolom menjadi satu kolom tanpa mengubah urutan baca |
| Layar sempit | Tabel boleh digeser horizontal dengan kolom identitas dan aksi tetap mudah ditemukan; kartu bed menjadi satu kolom. Alur admisi tidak memotong langkah |
| Keyboard | Semua kartu pilihan, tab, tautan, tombol, dialog, dan kontrol form dapat dicapai serta dijalankan tanpa mouse |
| Fokus | Dialog menahan fokus dan mengembalikannya ke pemicu; setelah error fokus pindah ke ringkasan error |
| Status | Warna tidak menjadi satu-satunya pembeda; setiap badge punya teks dan nama aksesibel |
| Tabel | Header terbaca pembaca layar; aksi baris memiliki nama yang menyebut episode atau bed sasaran |
| Cetak | Urutan baca identitas → isi persetujuan → tanda tangan tetap benar tanpa navigasi aplikasi |

Breakpoint, nilai piksel, dan bentuk ikon tetap mengikuti design token repository; dokumen ini
tidak membuat sistem responsif tandingan.

### 4.3 Privasi lintas layar

| Data | Tempat yang boleh | Tempat yang tidak boleh |
| --- | --- | --- |
| Diagnosis dan isi resume | Detail/discharge bagi permission yang tepat | beranda, census, worklist, papan, monitoring |
| Catatan episode | Detail episode yang berhak | seluruh daftar |
| Keterangan isolasi | Detail/admisi bagi peran berhak | census, worklist, papan; hanya penanda tanpa alasan yang boleh |
| Nomor kartu asuransi/peserta | Langkah Pembayaran dan formulir cetak bila kontrak mengizinkan | seluruh daftar dan ringkasan beranda |
| Data contoh | Data pseudonim | data pasien atau pegawai nyata |

### 4.4 Keputusan reuse atau komponen baru

| Elemen | Bukti repository | Keputusan usulan |
| --- | --- | --- |
| Header halaman | `Hero` dipakai layar Rawat Inap saat ini | **Reuse** |
| Filter, tabel, pagination | `DataFilter`, `FilterSelect`, `ResourceFilterSelect`, `DataTable`, `RegionPagination` sudah dipakai census/worklist | **Reuse**; tambahkan konfigurasi, bukan komponen paralel |
| Alert, toast, akses | `InformationAlert`, `ToastStack`, `AccessDeniedGate/Alert` sudah menangani state yang sama | **Reuse** |
| Editor master | `HealthServicesMasterDataEditorView`, `BaseEditorForm` sudah dipakai pengaturan | **Reuse** |
| Papan dan alasan kelayakan | `inpatient-bed-board`, `placement-failure-list`, hook terkait sudah tersedia | **Reuse dan extend** untuk konfirmasi/reservation |
| Kerangka admisi | Komponen `emergency-registration/` dan kiosk sudah membuktikan stepper, pasien, pembayaran, scan, verifikasi | **Reuse dan komposisi**; tidak membuat stepper generik keempat |
| Ringkasan beranda | Belum ada komposisi yang memenuhi tiga isi wajib | **Buat komposisi khusus Rawat Inap** dari kartu/stat yang sudah ada, tanpa membuat primitive kartu baru |
| Penanda reservation | Belum ada pembaca status + sisa waktu yang dapat dipakai lintas worklist/bed-board/admisi | **Buat satu komponen domain bersama** setelah `RWI-UI-GAP-003` tertutup |
| Cetak persetujuan | Pola print ada; isi persetujuan Rawat Inap belum ada | **Buat template domain baru** di atas shell cetak existing |

---

## 5. Beranda Rawat Inap — `FE-INP-19`

Route: `/health-services/inpatient-management`

Tujuan: memberi ringkasan kerja hari ini dan menjadi pintu masuk ke seluruh layar tingkat dua.
Bagian ini menjalankan kewenangan `RWI-FE-005` tanpa menjadikan angka sebagai hiasan.

### 5.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│ Rawat Inap│ Beranda Rawat Inap                         [Muat Ulang] │
│           │ Ringkasan keadaan operasional hari ini                  │
│           ├─────────────────────────────────────────────────────────┤
│           │ PASIEN DIRAWAT                                          │
│           │ [Melati 18] [Anggrek 12] [Kelas 1: 8] [Kelas 2: 22]    │
│           ├─────────────────────────────────────────────────────────┤
│           │ EPISODE                                                 │
│           │ [Disiapkan 4] [Dirawat 30] [Rencana pulang 6]          │
│           │ [Selesai 11] [Batal 1]                                 │
│           ├─────────────────────────────────────────────────────────┤
│           │ PERLU DITINDAKLANJUTI                                   │
│           │ [Tertunda 3] [Override 1] [Tanpa perawat 2] [Isolasi 1]│
│           ├─────────────────────────────────────────────────────────┤
│           │ Akses cepat: Admisi · Daftar Episode · Bed · Census    │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 5.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Pasien dirawat | Jumlah per unit layanan dan kelas | `GET /census/summary` | `InpatientCensus : Read` | Nol tetap tampil `0`; gagal hanya menggagalkan blok ini dan menyediakan **Coba Lagi** |
| Episode | Lima status dengan kata pengguna bagian 4.1 arsitektur frontend | `GET /episodes/summary` | `InpatientEpisode : Read` | Lima nilai tetap tampil; data gagal tidak diganti nol |
| Perlu ditindaklanjuti | Jumlah empat daftar pantau | Empat endpoint monitoring | Permission baca tiap daftar | Kartu yang tidak berhak disembunyikan; kegagalan satu daftar tidak menutup tiga lainnya |
| Akses cepat | Tujuh menu operasional Rawat Inap yang diizinkan per peran | route aplikasi | permission route | Tautan yang tidak berhak tidak dirender; master/configuration tetap masuk melalui kelompok **Master Data** |

### 5.3 Tombol dan tautan

| Label | Aktif bila | Yang terjadi |
| --- | --- | --- |
| Angka unit atau kelas | pengguna boleh membaca census | Membuka `FE-INP-01` dengan penyaring terkait |
| Angka status episode | pengguna boleh membaca episode | Membuka `FE-INP-16` dengan status terkait; `Draft` wajib sudah tersaring |
| Angka daftar pantau | pengguna berhak membaca daftar itu | Membuka tab terkait pada `FE-INP-09` |
| Muat Ulang | tidak sedang memuat | Memuat ulang keempat kelompok tanpa menghapus kelompok yang masih berhasil |

### 5.4 Keadaan

| Keadaan | Yang tampil |
| --- | --- |
| Memuat pertama kali | Kerangka blok ringkasan, bukan empat angka palsu bernilai nol |
| Seluruh data kosong | Semua angka `0` dan kalimat “Belum ada pekerjaan Rawat Inap pada penyaring ini.” |
| Gagal sebagian | Pesan dan **Coba Lagi** hanya pada blok yang gagal |
| Tidak berhak | Beranda tetap terbuka bila ada minimal satu kemampuan baca; hanya blok yang berhak yang tampil |

**Keadaan source 28 Agustus 2026:** route sudah ada, tetapi masih berupa halaman penantian. Target
bagian ini dimiliki `FE-RWI-021`.

---

## 6. Daftar Kerja Episode — `FE-INP-16`

Route: `/health-services/inpatient-management/episodes`

### 6.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Daftar Kerja Episode                     [Admisi Baru] │
│           │ [Tanggal] [Unit] [Kelas] [Status: Semua ▾] [Cari...]   │
│           │ Status: Disiapkan · Dirawat · Rencana pulang · Selesai│
│           │         · Batal                                         │
│           ├─────────────────────────────────────────────────────────┤
│           │ Episode │ Pasien │ Lokasi │ DPJP │ Hari rawat │ Status │
│           │ RI-...  │ Sari   │ Melati │ ...  │ 3 hari     │ ...    │
│           │                          [Detail] [Lanjutkan/Batalkan]* │
│           └─────────────────────────────────────────────────────────┤
│           │ 1–20 dari 87                              ‹ 1 2 3 ›    │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 6.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Penyaring | Rentang tanggal, unit, kelas, lima status, kata kunci, ukuran halaman | `GET /episodes/filters/metadata` | `InpatientEpisode : Read` | Metadata gagal: daftar tidak dibuka dengan pilihan buatan sendiri; tampil **Coba Lagi** |
| Tabel | Nomor episode, pasien, lokasi, DPJP, hitungan hari rawat, status | `GET /episodes` | sama | Kosong menjelaskan penyaring dapat diubah; gagal mempertahankan penyaring |
| Penanda pemesanan `Draft` | Aktif, kedaluwarsa, sisa waktu | Kontrak baca pemesanan episode | sama | **Gerbang `RWI-UI-GAP-003`: sumber belum tersedia; tidak boleh dihitung dari browser** |
| Aksi baris | Detail; lanjutkan dan batalkan bila aturan mengizinkan | status dan permission | permission per aksi | Aksi yang tidak berhak tidak dirender |

### 6.3 Tombol

| Label | Kapan aktif | Yang terjadi |
| --- | --- | --- |
| Detail Episode | setiap baris | Membuka `FE-INP-04` |
| Lanjutkan Admisi | hanya `Draft` dan metadata pemesanan tersedia | Membuka `FE-INP-03` pada langkah yang ditentukan server |
| Batalkan Admisi | `Draft` bagi admisi/supervisor; status lain mengikuti bagian 21 | Membuka `FE-INP-17` |
| Admisi Baru | petugas admisi atau supervisor | Membuka awal `FE-INP-03` |

### 6.4 Keadaan

Memuat, kosong, gagal, dan tidak berhak mengikuti bagian 4. Baris `Draft` tanpa metadata pemesanan
**tidak boleh** diberi label “belum memesan” karena keadaan itu belum dapat dibuktikan.

**Keadaan source:** tabel, kelima status, penyaring, pagination, retry, dan detail sudah ada.
Indikator pemesanan, **Lanjutkan Admisi**, dan **Batalkan Admisi** belum ada.

---

## 7. Papan Tempat Tidur — `FE-INP-02`

Route: `/health-services/inpatient-management/bed-board`

### 7.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Papan Tempat Tidur                         [Muat Ulang] │
│           │ [Tersedia 12] [Dipesan 3] [Terisi 30] [Perbaikan 2]   │
│           │ [Unit ▾] [Kamar ▾] [Kelas ▾] [Cari bed...]             │
│           ├─────────────────────────────────────────────────────────┤
│           │ MELATI / Kamar 3 — Kelas 2                             │
│           │ [A Tersedia] [B Dipesan 01:41] [C Terisi — Sari D.]   │
│           │                 [Konfirmasi Masuk] [Batalkan Pesanan]  │
│           │ [D Pembersihan] [E Perbaikan]                          │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 7.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Jumlah per keadaan tempat tidur | `GET /bed-occupancies/board` | `InpatientBedOccupancy : Read` | Nol tampil `0`; gagal tidak menyisakan ringkasan lama |
| Penyaring | Unit, kamar, kelas, kata kunci | jawaban board dan metadata master | sama | Pilihan gagal dimuat: tampil pesan, bukan daftar tertanam |
| Papan | Unit → kamar → bed; keadaan, pasien pada `Occupied`, pemegang dan sisa waktu pada `Reserved` bila berhak | board + kontrak reservation | sama | Kosong: “Tidak ada tempat tidur pada penyaring ini”; gagal menyediakan **Coba Lagi** |
| Penolakan kelayakan | Daftar alasan server | jawaban 422 Kelayakan Penempatan | permission tindakan | Alasan kamar ditampilkan apa adanya |

### 7.3 Tombol

| Label | Kapan aktif | Yang terjadi |
| --- | --- | --- |
| Konfirmasi Masuk | `Reserved`; petugas admisi/supervisor; metadata episode tersedia | Muat ulang papan, lalu modal menyebut pasien dan bed; `POST /placements` |
| Batalkan Pesanan | pemesanan aktif; petugas admisi/supervisor | Modal konfirmasi lalu membatalkan reservation |
| Muat Ulang | selalu kecuali request berjalan | Membaca ulang; juga otomatis saat jendela kembali fokus |

### 7.4 Keadaan

409 memuat ulang papan dan mempertahankan konteks; 422 menampilkan semua alasan. Tempat tidur
tidak layak dapat tampil nonaktif dengan alasan, tetapi layar tidak membuat ulang aturan server.

**Keadaan source — `REPAIR`:** papan baca sudah ada, tetapi standalone dipanggil dengan
`selectable={false}` sehingga tidak merender aksi apa pun. Ia juga belum memuat ulang saat fokus,
belum mempunyai retry, konfirmasi masuk, pembatalan reservation, atau countdown yang dapat dibaca
ulang. Screenshot runtime pemilik menegaskan layar berhenti sebagai daftar pasif. Target aksi tetap
dimiliki `FE-RWI-026/030`; perbaikan susunan, state kosong, dan integrasi seluruh aksi ke layar final
dimiliki `FE-RWI-036`. Data reservation tertahan `RWI-UI-GAP-003` dan bukti runtime data master
tertahan `RWI-UI-GAP-007`.

---

## 8. Pasien Sedang Dirawat (Census) — `FE-INP-01`

Route: `/health-services/inpatient-management/census`

### 8.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Pasien Sedang Dirawat                                  │
│           │ [Unit ▾] [Kelas ▾] [Cari pasien/episode...] [20 ▾]    │
│           ├─────────────────────────────────────────────────────────┤
│           │ Episode │ Pasien │ Lokasi │ DPJP │ Perawat │ Hari rawat│
│           │ RI-...  │ Sari   │ Mlt-3A │ ...  │ ...     │ 3 hari   │
│           │                                             [Detail]   │
│           └─────────────────────────────────────────────────────────┤
│           │ 1–20 dari 30                              ‹ 1 2 ›      │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 8.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Penyaring | Unit, kelas, pencarian, ukuran halaman | `GET /census/filters/metadata` | `InpatientCensus : Read` | Metadata gagal tidak diganti master data langsung |
| Tabel | Episode, pasien, lokasi, DPJP, perawat, **hitungan hari rawat**, status | `GET /census` | sama | Kosong menjelaskan tidak ada pasien dirawat pada filter; gagal + **Coba Lagi** |
| Aksi | Detail Episode | route detail | `InpatientEpisode : Read` | Disembunyikan bila tidak berhak |

Diagnosis, catatan episode, nomor penjamin, dan alasan isolasi tidak tampil. Penanda isolasi boleh
berupa teks atau ikon berlabel aksesibel tanpa mengungkap keterangannya.

### 8.3 Tombol dan keadaan

**Detail Episode** membuka `FE-INP-04`. Data dimuat ulang saat jendela kembali fokus. Kata “hari
rawat” wajib menyatakan hitungan tanggal, bukan durasi 24 jam (`RWI-FE-001`). Keempat keadaan daftar
bagian 4 berlaku.

**Keadaan source — `REPAIR`:** tombol **Detail Episode** sudah ada, tetapi hanya muncul ketika tabel
memiliki baris. Screenshot runtime pemilik memperlihatkan halaman kosong tanpa jalan lanjut yang
berguna; penyaring juga masih membaca resource master dan belum memakai
`GET /census/filters/metadata`. `FE-RWI-037` memperbaiki presentasi, empty-state action, dan
keterlihatan aksi baris; metadata tetap dimiliki `FE-RWI-033`.

---

## 9. Detail Episode — `FE-INP-04`

Route: `/health-services/inpatient-management/episodes/{id}`

### 9.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Episode RI-2026-000204     [Sedang dirawat] [Kembali] │
│           │ Pasien · lokasi · DPJP · perawat · hari rawat          │
│           │ [Pulang & Resume] [Keuangan] [Penutupan] [Cetak]      │
│           ├─────────────────────────────────────────────────────────┤
│           │ Kebutuhan Isolasi │ Penanggung Jawab │ Kepergian       │
│           ├─────────────────────────────────────────────────────────┤
│           │ Perpindahan Tempat Tidur                               │
│           ├─────────────────────────────────────────────────────────┤
│           │ Riwayat status · lokasi · DPJP · perawat               │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 9.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Nomor, pasien, status, lokasi, DPJP, perawat, hitungan hari rawat | `GET /episodes/{id}` | `InpatientEpisode : Read` | ID salah atau gagal punya state sendiri dan **Coba Lagi** |
| Aksi anak | Pulang, keuangan, penutupan, koreksi, cetak | status + permission | permission tiap kemampuan | Tautan tidak sah disembunyikan atau diberi alasan nonaktif |
| Ruang kerja | Isolasi `FE-INP-15`, transfer `FE-INP-05`, kepergian `FE-INP-14`, penugasan | endpoint masing-masing | matriks peran | Satu wilayah gagal tidak menghapus ringkasan episode |
| Riwayat | Status, lokasi, DPJP, perawat | endpoint riwayat | permission baca | Tiap daftar punya empty text sendiri |

### 9.3 Tombol dan keadaan

Tautan anak selalu membaca ulang episode ketika dibuka. **Sesi Koreksi** hanya tampil bagi
supervisor pada `Closed`; **Ubah Isolasi** mengikuti status dan DPJP aktif; **Pindahkan** bagi dokter
hanya aktif bila ia DPJP aktif. Loading, invalid ID, error/retry, empty per riwayat, dan unauthorized
tidak digabung menjadi satu layar kosong.

**Keadaan source:** pola ini sudah ada dan direkomendasikan **reuse**. Jalur ke Sesi Koreksi kini
sudah terjangkau dari daftar kerja melalui detail episode.

---

## 10. Perpindahan Pasien — `FE-INP-05`

Lokasi: wilayah anak di `FE-INP-04`; tidak mendapat menu atau route tingkat dua.

### 10.1 Kerangka

```text
┌─ Perpindahan Tempat Tidur ──────────────────────────────────────────┐
│ Lokasi sekarang: Melati / Kamar 3 / Bed A — Kelas 2               │
│ [Unit ▾] [Kamar ▾] [Kelas ▾] [Cari bed...]                         │
│ [Bed B — Tersedia] [Bed C — tidak layak: kamar berbeda gender]   │
│ Alasan medis * [                                                ] │
│                                      [Batal] [Pindahkan Pasien]   │
└────────────────────────────────────────────────────────────────────┘
```

### 10.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Lokasi asal | Unit, kamar, bed, kelas | detail episode | pembaca episode | Bila lokasi aktif tidak ada, transfer diblokir dan alasannya tampil |
| Tujuan | Papan `available-beds`, termasuk baris tidak layak nonaktif | `GET /available-beds` | `InpatientBedOccupancy : Read/Transfer` | Kosong: tidak ada bed layak; 409 memuat ulang; 422 menampilkan daftar alasan |
| Alasan | Alasan medis wajib untuk DPJP; alasan perpindahan bagi peran lain | input pengguna | permission transfer + penjaga DPJP | Tidak boleh hanya spasi |

### 10.3 Tombol dan keadaan

**Pindahkan Pasien** hanya aktif setelah bed tujuan dan alasan sah. Modal menyebut bed asal dan
tujuan. Tombol dikunci saat request. Dokter bukan DPJP aktif melihat alasan nonaktif, bukan tombol
yang pasti ditolak. Source saat ini sudah menyediakan pola ini dan direkomendasikan **reuse**.

---

## 11. Keputusan Pulang dan Resume — `FE-INP-06`

Route: `/health-services/inpatient-management/episodes/{id}/discharge`

### 11.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Keputusan Pulang & Resume                 [Ke Detail] │
│           │ RI-... · Sari Dewi · DPJP dr. Andi                    │
│           ├─────────────────────────────────────────────────────────┤
│           │ KEPUTUSAN PULANG                                       │
│           │ (•) Izin DPJP  ( ) APS  ( ) Dirujuk                   │
│           │ [Nyatakan Boleh Pulang]                                │
│           ├─────────────────────────────────────────────────────────┤
│           │ RESUME: diagnosis · tindakan · obat · kontrol · cara  │
│           │ [Simpan Resume] [Tandatangani Resume]                  │
│           ├─────────────────────────────────────────────────────────┤
│           │ RIWAYAT VERSI                                           │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 11.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Pasien, episode, status, DPJP aktif | detail episode | `InpatientDischarge : Read` | Gagal + **Coba Lagi**; detail lama tidak dipakai untuk menulis |
| Keputusan pulang | Tiga cara MVP: izin DPJP, APS, dirujuk; isian kondisional | `POST /discharges/{id}/decide` | DPJP aktif | Belum diputuskan: form tersedia; sudah diputuskan: ringkasan readonly |
| Resume | Isi resmi sesuai `RWI-RULE-032`; DPJP dan periodenya otomatis | endpoint summary | DPJP aktif untuk tulis/tanda tangan | Belum ada: form baru; gagal simpan mempertahankan isian |
| Riwayat versi | Versi resume yang sudah ditandatangani | endpoint revisions | pembaca discharge | Kosong dijelaskan sebagai belum ada versi terdahulu |

### 11.3 Tombol dan keadaan

**Nyatakan Boleh Pulang**, **Simpan Resume**, dan **Tandatangani Resume** dikunci saat request dan
masing-masing memakai konfirmasi yang menyebut akibatnya. Pengguna selain DPJP aktif memperoleh
mode baca beserta penjelasan. Meninggal dan kabur tetap tidak dapat dipilih karena keputusan
klinisnya di luar MVP. Source saat ini dapat dipakai ulang; satu kriteria lama tentang bentuk pesan
server masih diselesaikan di gerbang kesiapan roadmap.

---

## 12. Penutupan Episode — `FE-INP-07`

Route: `/health-services/inpatient-management/episodes/{id}/closure`

### 12.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Penutupan Episode                         [Ke Detail]  │
│           │ [✓ Keputusan] [✓ Resume] [! Administrasi] [! Finansial]│
│           │ [✓ Lokasi/kepergian]                                   │
│           ├─────────────────────────────────────────────────────────┤
│           │ DAFTAR PERIKSA ADMINISTRASI                             │
│           │ [ ] Persetujuan umum  [Tandai Selesai]                 │
│           │ [✓] Barang dikembalikan — Wati, 13:22                  │
│           ├─────────────────────────────────────────────────────────┤
│           │ [Tutup Episode]                                        │
│           │ ─ setelah ditolak karena finansial, supervisor saja ─ │
│           │ Alasan override * [                              ]      │
│           │ [Tutup Menembus Gerbang Keuangan]                      │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 12.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Kesiapan | Lima syarat, masing-masing sudah/belum dan alasannya | `GET /discharges/{id}/closure-readiness` | `InpatientDischarge : Read` | Wajib dimuat ulang tepat sebelum menutup; 422 menampilkan kelima syarat |
| Checklist | Butir aktif, wajib, pelaku dan waktu | endpoint clearance | admisi/supervisor untuk menandai | Daftar kosong bukan otomatis siap; tampil jawaban server |
| Tutup biasa | Konfirmasi pasien dan cara pulang | `POST /close` | admisi/supervisor | Gagal mempertahankan seluruh keadaan |
| Override | Alasan dan peringatan laporan pengecualian | `POST /close-with-override` | supervisor | Baru muncul setelah tutup biasa ditolak karena finansial |

### 12.3 Tombol dan keadaan

Override tidak diletakkan sebagai pilihan setara. Setelah sukses, layar menunjukkan episode
`Selesai`, tempat tidur bebas, serta tautan ke detail dan papan. Source saat ini direkomendasikan
**reuse** karena urutan, state, modal, dan pemisahan override sudah tersedia.

---

## 13. Kelayakan Keuangan — `FE-INP-08`

Route: `/health-services/inpatient-management/episodes/{id}/financial-clearance`

### 13.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Kelayakan Keuangan                        [Ke Detail]  │
│           │ Penandaan manual sementara — bukan hasil mesin Billing │
│           ├─────────────────────────────────────────────────────────┤
│           │ KEADAAN TERKINI: Pending / Cleared / Blocked           │
│           │ Ditandai oleh · waktu · catatan                         │
│           ├─────────────────────────────────────────────────────────┤
│           │ Nilai * [Pending ▾]   Catatan * [                  ]   │
│           │                                      [Simpan Penandaan]│
│           ├─────────────────────────────────────────────────────────┤
│           │ RIWAYAT PENANDAAN                                       │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 13.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan episode | Pasien, episode, status | detail episode | pembaca discharge/episode | Gagal + **Coba Lagi** |
| Keadaan terkini | Nilai, pelaku, waktu, catatan, label **manual** | kontrak baca financial clearance | kasir/billing | **Gerbang `RWI-UI-GAP-004`: GET khusus belum ada; tidak boleh ditebak dari sesi browser** |
| Form | `Pending`, `Cleared`, `Blocked`, catatan wajib | `POST /financial-clearance` | kasir/billing | Server menolak: isian tetap utuh |
| Riwayat | Semua penandaan | kontrak riwayat | permission baca | Belum tersedia harus dinyatakan, bukan daftar kosong palsu |

### 13.3 Tombol dan keadaan

**Simpan Penandaan** aktif setelah nilai dipilih sadar dan catatan bermakna. Source saat ini hanya
dapat membaca closure-readiness dan mengingat hasil POST pada sesi halaman; refresh kehilangan
nilai tepat serta riwayat. Selain itu hak baca discharge untuk peran kasir perlu dipastikan.
Skema target ini tidak dianggap implementable sebelum `RWI-UI-GAP-004` ditutup.

---

## 14. Daftar Pantau — `FE-INP-09`

Route: `/health-services/inpatient-management/monitoring`

### 14.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Daftar Pantau — tidak menghalangi tindakan             │
│           │ [Penutupan Tertunda 3] [Override 1] [Tanpa Perawat 2] │
│           │ [Penempatan Tidak Sesuai 1]                            │
│           │ [Unit ▾] [20 ▾]                                       │
│           ├─────────────────────────────────────────────────────────┤
│           │ Episode │ Pasien │ Unit │ Sejak/Lama │ Tindak lanjut  │
│           │ RI-...  │ Sari   │ ...  │ 40 menit   │ [Buka]         │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 14.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Empat tab | Jumlah dan nama daftar | empat endpoint monitoring | permission per daftar | Tab tanpa hak disembunyikan; tab berhak tetap bekerja bila tab lain gagal |
| Penyaring | Unit dan ukuran halaman | query endpoint | pembaca daftar | Mempertahankan tab saat gagal |
| Tabel | Episode, pasien, unit, penyebab, sejak/lama, tindak lanjut | endpoint tab aktif | sesuai penanggung jawab | Empty text spesifik; error + **Coba Lagi** |

### 14.3 Tombol dan keadaan

**Buka Detail**, **Buka Penutupan**, atau **Buka Perpindahan** menautkan ke pemilik aksi; daftar
sendiri tidak menulis data. Lama keterlambatan tampil bila konsep keterlambatan berlaku. Daftar
ketidakcocokan isolasi memakai nada netral, bukan menyalahkan petugas. Source memang memiliki tautan
per baris, tetapi screenshot runtime kosong membuat seluruh layar tidak memberi jalan kerja yang
terlihat. Statusnya dinaikkan menjadi **`REPAIR`**: `FE-RWI-038` menyusun ulang tab, state kosong,
aksi tindak lanjut, dan hierarki visual tanpa menambah endpoint tulis.

---

## 15. Selisih Tempat Tidur — `FE-INP-10`

Route: `/health-services/inpatient-management/bed-drift`

### 15.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Selisih Tempat Tidur                 [Buka Papan Bed] │
│           │ Laporan deteksi; koreksi tidak dilakukan dari sini     │
│           │ [Unit ▾] [20 ▾]                                       │
│           ├─────────────────────────────────────────────────────────┤
│           │ Bed │ Unit/Kamar │ Status salinan │ Seharusnya │ Sejak│
│           │ 3A  │ Melati 3   │ Tersedia       │ Terisi      │ ...  │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 15.2 Wilayah, tombol, dan keadaan

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Tabel selisih | Bed, lokasi, status salinan, status dari penempatan, konteks waktu | endpoint bed drift | admin/supervisor yang berhak | Kosong adalah keadaan positif; gagal + **Coba Lagi** |
| Navigasi | Buka Papan Tempat Tidur | route `FE-INP-02` | pembaca papan | Tidak ada tombol “perbaiki” karena endpoint rekonsiliasi tidak dikontrak |

Source memiliki tautan **Papan Tempat Tidur**, tetapi bukti runtime menunjukkan hierarki visual dan
keadaan kosong membuat fungsi diagnostiknya tidak terbaca sebagai tindakan yang dapat dilanjutkan.
Statusnya **`REPAIR`** melalui `FE-RWI-039`. Layar ini tetap **read-only**: task hanya memperjelas
selisih serta navigasi ke papan dengan konteks filter; ia tidak boleh mengarang tombol “Perbaiki”
karena endpoint rekonsiliasi tidak dikontrak. Unauthorized ditolak sebelum request.

---

## 16. Sesi Koreksi Episode — `FE-INP-11`

Route: `/health-services/inpatient-management/episodes/{id}/correction`

### 16.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Sesi Koreksi — Episode tetap Selesai       [Ke Detail]│
│           │ [Buka Sesi Koreksi]                                    │
│           ├─────────────────────────────────────────────────────────┤
│           │ BOLEH: resume, diagnosis, cara pulang, catatan          │
│           │ TIDAK: bed, census, hari rawat, melanjutkan perawatan   │
│           ├─────────────────────────────────────────────────────────┤
│           │ Koreksi Resume [                                      ]│
│           │ [Simpan Koreksi]                  [Tutup Sesi Koreksi] │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 16.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Ringkasan | Episode `Closed`, pasien, alasan koreksi | detail episode | supervisor | Status bukan `Closed`: aksi tidak tersedia |
| Sesi | Pelaku, waktu buka, alasan, keadaan terbuka/tertutup | kontrak sesi koreksi | supervisor | **Gerbang `RWI-UI-GAP-005`: tidak ada GET sesi; refresh belum dapat memulihkan sesi terbuka** |
| Koreksi | Resume dan field yang diizinkan | endpoint koreksi | supervisor dalam sesi aktif | Resume boleh belum ada; error mempertahankan isian |

### 16.3 Tombol dan keadaan

**Buka Sesi**, **Simpan Koreksi**, dan **Tutup Sesi** tampil berurutan, tidak bersamaan bila tidak
sah. Keterangan tegas menyebut episode tetap `Closed`, tidak kembali ke census, dan tidak memperoleh
bed. Source dapat di-**reuse dengan adapter** setelah kontrak baca sesi diputuskan. Jalurnya kini
sudah terjangkau: `FE-INP-16` → Detail Episode `Closed` → Sesi Koreksi.

---

## 17. Pengaturan Rawat Inap — `FE-INP-12`

Route: `/health-services/inpatient-management/settings`

Klasifikasi: **master/configuration milik Rawat Inap**. Induk sidebar target adalah `Pelayanan
Kesehatan → Master Data`, bukan submenu operasional `Rawat Inap`. Route yang sudah ada dipertahankan
pada tahap pemindahan menu agar deep link dan tautan internal tidak putus.

### 17.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Pengaturan Rawat Inap                      [Ke Beranda]│
│           │ Perubahan berlaku pada pembacaan berikutnya             │
│           ├─────────────────────────────────────────────────────────┤
│           │ Pemesanan bed           [ 120 ] menit                   │
│           │ Draft telantar           [ 1   ] hari                    │
│           │ Penutupan tertunda       [ 4   ] jam                     │
│           │ Parameter lain kontrak   [ ... ]                         │
│           │                                      [Simpan Pengaturan]│
│           │ Diubah terakhir oleh ... pada ...                       │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 17.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Form pengaturan | Seluruh parameter kontrak beserta satuan dan batasnya | `GET /inpatient-settings` | `InpatientSetting : Read` | 404 dibedakan dari 403 dan kegagalan; tersedia **Muat Ulang** |
| Audit ringkas | Pengubah dan waktu terakhir | jawaban pengaturan | sama | Bila belum pernah diubah, tampil “Nilai bawaan sistem” |
| Simpan | Hanya field berubah; pesan validasi per field | endpoint update | `InpatientSetting : Update` | Error mempertahankan isian |

### 17.3 Tombol dan keadaan

**Simpan Pengaturan** aktif bila form valid dan berubah. Satuan selalu tampil di dekat input, tidak
hanya pada placeholder. Source memakai `HealthServicesMasterDataEditorView` dan `BaseEditorForm`;
namun screenshot runtime hanya menampilkan peringatan dan **Muat ulang** karena `settingId` tidak
tersedia. Source sengaja menyembunyikan submit pada keadaan itu; frontend tidak dapat membuat baris
`DEFAULT` karena kontrak tidak mempunyai `POST`. Status layar **`REPAIR` dengan dependency**:
`FE-RWI-041` memperbaiki shell/keadaan kosong dan form, sedangkan pengisian master data tetap
tanggung jawab `RWI-DEC-063`/`BE-RWI-002` serta dicatat `RWI-UI-GAP-007`. Sidebar harus mengikuti
permission, bukan hanya route gate.

---

## 18. Butir Administrasi Rawat Inap — `FE-INP-13`

Route: `/health-services/inpatient-management/clearance-items`

Klasifikasi: **master data milik Rawat Inap**. Label sidebar target adalah **Butir Administrasi Rawat
Inap** di `Pelayanan Kesehatan → Master Data`. Route yang sudah ada dipertahankan pada tahap
pemindahan menu agar deep link dan tautan internal tidak putus.

### 18.1 Kerangka

```text
┌─ sidebar ─┬──────────────────────────────────────────────────────────┐
│           │ Butir Administrasi Rawat Inap              [Tambah]    │
│           │ [Status ▾] [Wajib/Opsional ▾] [Cari...]                │
│           ├─────────────────────────────────────────────────────────┤
│           │ Nama butir │ Wajib │ Aktif │ Urutan │ Aksi             │
│           │ Persetujuan│ Ya    │ Ya    │ 10     │ Detail · Ubah   │
│           │                         Aktifkan/Nonaktifkan · Hapus    │
└───────────┴─────────────────────────────────────────────────────────┘
```

### 18.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Penyaring dan tabel | Nama, wajib, aktif, urutan, audit ringkas | endpoint clearance-item list | `InpatientClearanceItem : Read` | Kosong mengarahkan **Tambah**; load error wajib punya **Coba Lagi** |
| Editor | Nama, deskripsi, sifat wajib, urutan | endpoint create/update/detail | create/update permission | Error modal tidak menutup dan tidak menghapus isian |
| Perubahan status/hapus | Konfirmasi menyebut dampak pada penutupan berikutnya | endpoint status/delete | permission terkait | Konflik server ditampilkan apa adanya |

### 18.3 Tombol dan keadaan

**Tambah**, **Detail**, **Ubah**, **Aktifkan/Nonaktifkan**, dan **Hapus** mengikuti permission
masing-masing. Perubahan tidak ditampilkan seolah mengubah checklist episode lama bila kontrak tidak
menjanjikannya. Source memuat handler seluruh aksi dan tombol **Tambah**, tetapi screenshot runtime
menampilkan daftar kosong dan pemilik melaporkan aksi belum dapat dipakai. Fakta source dan runtime
karena itu belum boleh dianggap “selesai”; statusnya **`REPAIR`** dan dimiliki `FE-RWI-040` untuk
memastikan aksi empty state maupun aksi baris benar-benar terlihat, permission-aware, dan dapat
ditelusuri. Ketersediaan tiga butir awal tetap bagian `RWI-UI-GAP-007`, bukan alasan membuat data
tiruan di frontend.

---

## 19. Pencatatan Kepergian — `FE-INP-14`

Lokasi: wilayah anak di Detail Episode `FE-INP-04`.

### 19.1 Kerangka

```text
┌─ Pasien Meninggalkan Ruangan ───────────────────────────────────────┐
│ Status: Rencana pulang · Bed Melati 3A masih dipegang              │
│ Waktu kepergian * [28-08-2026 13:10]                               │
│ Catatan            [                                            ] │
│ ⚠ Setelah disimpan, bed langsung bebas dan tindakan tak dibatalkan│
│                               [Catat Pasien Sudah Pergi]           │
└────────────────────────────────────────────────────────────────────┘
```

### 19.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Keadaan kini | Status episode, lokasi/bed, kepergian yang sudah tercatat | detail + departure | admisi, perawat, kepala ruangan, supervisor | Detail dimuat ulang sebelum konfirmasi; data lama tidak dipakai |
| Form | Waktu dan catatan | input + `POST /record-departure` | permission departure | Status selain `DischargePending` memberi alasan nonaktif |
| Hasil | Waktu, pencatat, bed bebas, episode masih menunggu penutupan | jawaban server | pembaca episode | Gagal mempertahankan form |

### 19.3 Tombol dan keadaan

Modal **Catat Pasien Sudah Pergi** menyebut tindakan tidak dapat dibatalkan, tempat tidur segera
bebas, dan episode belum selesai. Sesudah berhasil tampil tautan ke `FE-INP-07`. Source saat ini
direkomendasikan **reuse**.

---

## 20. Kebutuhan Isolasi — `FE-INP-15`

Lokasi: bagian Dokter di `FE-INP-03` saat `Draft`, dan wilayah anak di `FE-INP-04` setelah episode
aktif. Tidak mendapat menu sendiri.

### 20.1 Kerangka

```text
┌─ Kebutuhan Isolasi ─────────────────────────────────────────────────┐
│ [on/off] Membutuhkan isolasi                                       │
│ Keterangan * [                                                  ] │
│ Sumber: Catatan awal admisi / Keputusan klinis DPJP                │
│                                        [Simpan Kebutuhan Isolasi]  │
└────────────────────────────────────────────────────────────────────┘
```

### 20.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Keadaan | Ya/tidak, keterangan sensitif, sumber, pelaku, waktu | detail episode | pembaca detail yang berhak | Keterangan tidak dibawa ke census/papan/daftar |
| Editor | sakelar dan keterangan wajib kondisional | `PATCH /isolation-requirement` | admisi pada `Draft`; DPJP aktif setelah aktif | Dokter bukan DPJP/petugas admisi setelah `Admitted` mendapat alasan nonaktif |
| Dampak | Informasi bahwa pencarian bed akan dimuat ulang | jawaban server | sama | 422 menampilkan seluruh alasan |

### 20.3 Tombol dan keadaan

**Simpan Kebutuhan Isolasi** dikunci saat request. Setelah berhasil, hasil `available-beds`
dibaca ulang. Perubahan menjadi tidak sesuai dengan bed saat ini tetap diterima dan episode masuk
daftar pantau; layar tidak menyalahkan pengguna. Source kedua lokasi dapat **reuse**.

---

## 21. Pembatalan Admisi — `FE-INP-17`

Lokasi: dialog aksi dari `FE-INP-16` dan `FE-INP-04`; bukan menu tersendiri.

### 21.1 Kerangka

```text
┌─ Batalkan Admisi? ──────────────────────────────────────────────────┐
│ Sari Dewi · RI-2026-000204 · Admisi sedang disiapkan               │
│ Pemesanan/penempatan yang masih ada ikut dilepas.                  │
│ Alasan pembatalan * [                                           ] │
│                              [Kembali] [Batalkan Admisi]           │
└────────────────────────────────────────────────────────────────────┘
```

### 21.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Identitas tindakan | pasien, episode, status, bed/reservation bila terbaca | detail episode + reservation | pembaca episode | Metadata reservation yang belum tersedia tidak boleh dikarang |
| Dampak | Episode menjadi `Cancelled`; reservation/placement dilepas sebagai satu tindakan | kontrak cancel | sesuai status | Konfirmasi selalu menyebut pelepasan |
| Alasan | teks wajib, bukan spasi/tanda baca saja | input | pelaku pembatalan | Error mempertahankan alasan |

### 21.3 Tombol dan keadaan

Pada `Draft`, aksi tersedia bagi petugas admisi dan supervisor. Pada `Admitted` tanpa catatan
klinis, hanya kepala ruangan dan supervisor. Status lain tidak menawarkan aksi. 409 memuat ulang
detail; 422 menampilkan alasan penolakan. Source saat ini **belum memiliki** route, dialog, tombol,
atau panggilan cancel; target dimiliki `FE-RWI-031`.

---

## 22. Cetak Persetujuan Rawat Inap — `FE-INP-18`

Route: `/health-services/inpatient-management/episodes/{id}/consent-print`

### 22.1 Kerangka cetak

```text
┌────────────────────────────────────────────────────────────────────┐
│ [Identitas rumah sakit]       PERSETUJUAN UMUM RAWAT INAP          │
│ Pasien · No. RM · Episode · Tanggal                               │
│ Penjamin · Unit · Kelas · DPJP                                    │
├────────────────────────────────────────────────────────────────────┤
│ 1. Persetujuan tindakan kedokteran umum                           │
│ 2. Persetujuan pemberian informasi kepada penjamin                │
│ 3. Penunjukan penerima informasi pasien                           │
│ Nama penerima informasi: __________________ Hubungan: ___________  │
│ Tanda tangan pasien/keluarga: __________ Petugas: ______________  │
└────────────────────────────────────────────────────────────────────┘
                         [Cetak] [Kembali ke Episode]
```

### 22.2 Wilayah

| Wilayah | Isi | Sumber | Hak akses | Kosong atau gagal |
| --- | --- | --- | --- | --- |
| Identitas | pasien, RM, episode, tanggal | detail episode/kunjungan | permission cetak + baca episode | Data wajib hilang: cetak diblokir dan ada **Coba Lagi** |
| Perawatan | penjamin, unit, kelas, DPJP | detail episode/kunjungan | sama | Nomor kartu hanya muncul bila memang dikontrak untuk formulir |
| Isi persetujuan | tiga isi minimum `RWI-DEC-035` | teks blueprint | sama | Tidak ada status “tersimpan” atau “ditandatangani” |
| Tanda tangan kertas | ruang kosong untuk pihak terkait | hasil cetak | sama | Tidak disimpan ke browser atau endpoint |

### 22.3 Tombol dan keadaan

**Cetak** membuka dialog cetak; **Kembali ke Episode** kembali tanpa menyimpan. Halaman cetak tidak
memuat sidebar, tidak dicache sebagai salinan dokumen, dan tidak dapat dibuka tanpa hak. Jalur masuk
tersedia dari langkah 8 admisi dan Detail Episode. Source saat ini **belum memiliki** route atau
komponen; target dimiliki `FE-RWI-028`.

---

## 23. Peta navigasi: tujuh menu operasional dan dua master data

Impact scan membuktikan source saat ini menaruh sembilan item di submenu `Rawat Inap`. Target brief
UI memisahkannya berdasarkan sifat datanya tanpa menambah jumlah tujuan navigasi:

```text
Pelayanan Kesehatan
├── Master Data
│   ├── Unit Layanan                    ← master bersama; sudah ada
│   ├── Klinik                          ← master bersama; sudah ada
│   ├── Ruangan                         ← master bersama; sudah ada
│   ├── Tempat Tidur                    ← master bersama; sudah ada
│   ├── Kelas Pasien                    ← master bersama; sudah ada
│   ├── …                               ← item master bersama lain tetap dipertahankan
│   ├── Butir Administrasi Rawat Inap   ← FE-INP-13; dipindah induknya
│   └── Pengaturan Rawat Inap           ← FE-INP-12; dipindah induknya
└── Rawat Inap
    ├── Beranda Rawat Inap
    ├── Admisi Rawat Inap
    ├── Papan Tempat Tidur
    ├── Daftar Kerja Episode
    ├── Pasien Sedang Dirawat
    ├── Daftar Pantau
    └── Selisih Tempat Tidur
```

`Unit Layanan`, `Ruangan`, `Tempat Tidur`, dan `Kelas Pasien` adalah master bersama yang dipakai
Rawat Inap; ownership dan itemnya **tidak diduplikasi**. `FE-INP-12` mengelola
`MstInpatientSetting`, sedangkan `FE-INP-13` mengelola `MstInpatientClearanceItem`; keduanya memang
dimiliki bounded context **Health Services / Master Data** pada arsitektur backend dan permission
matrix. `Klinik` tetap master bersama pada kelompok yang sama, tetapi dokumen ini tidak
mengalihkannya menjadi milik Rawat Inap.

Aturan migrasi navigasi yang mengikat untuk `FE-RWI-033`:

1. pindahkan definisi menu `FE-INP-12` dan `FE-INP-13` ke kelompok **Master Data**; jangan menyalin
   dan membiarkan item lama tetap hidup;
2. pertahankan route, page, hook, kontrak, dan permission yang sudah ada selama pemindahan induk;
3. hapus dua entri lama dari submenu **Rawat Inap** hanya setelah dua entri baru mengarah ke route
   dan permission yang sama;
4. normalisasi URL ke `/health-services/master-data/...` bukan bagian perubahan ini. Bila kelak
   disetujui, migrasi route harus memakai compatibility redirect dan task tersendiri;
5. layar per episode tidak mendapat menu baru.

Jalur klik yang mengikat:

| Layar tujuan | Jalur dari Beranda | Maksimum klik |
| --- | --- | :---: |
| `FE-INP-01`, `02`, `03`, `09`, `10`, `16` | Beranda → menu operasional Rawat Inap | 1 |
| `FE-INP-12`, `13` | Beranda → Master Data → item master Rawat Inap | 2 |
| `FE-INP-04` | Beranda → Daftar Kerja/Census → Detail | 2 |
| `FE-INP-05`, `14`, `15`, `17` | Beranda → Daftar Kerja/Census → Detail/aksi tertanam | 2–3 |
| `FE-INP-06`, `07`, `08`, `11`, `18` | Beranda → Daftar Kerja → Detail → layar anak | 3 |
| `FE-INP-19` | titik awal | 0 |

Setiap layar per-episode juga memiliki tautan kembali ke Detail Episode, bukan kembali ke Beranda
secara membabi buta. Route yang tidak sah ditolak oleh gate permission sebelum request data.

---

## 24. Impact scan source terkini — as-is versus target

Skema bagian 3 dan 5–23 adalah **target draft**, bukan klaim bahwa source sudah sama. Audit baca-saja
28 Agustus 2026 menghasilkan klasifikasi berikut.

| Klasifikasi | Layar | Dampak |
| --- | --- | --- |
| Reuse | `FE-INP-04`, `05`, `07`, `14`, `15` | Struktur dan state utama sudah sesuai; perubahan hanya delta yang disebut per bagian |
| Extend | `FE-INP-06`, `08`, `16` | Dasar layar ada, tetapi kontrak atau aksi target belum lengkap |
| Reuse dengan adapter | `FE-INP-11` | Komponen ada; perlu adapter kontrak atau state tambahan |
| Repair | `FE-INP-01`, `02`, `09`, `10`, `12`, `13` | Enam layar sudah ada, tetapi bukti runtime pemilik menunjukkan tampilan tidak layak dipakai atau tidak menyediakan jalan kerja yang efektif. Logic/API yang benar boleh direuse; layout, empty/error state, dan surface aksi wajib diperbaiki oleh `FE-RWI-036` s.d. `041` |
| Conflict/replace | `FE-INP-03` | Source masih form tunggal dan langsung menempatkan pasien; bertentangan dengan alur reservasi target |
| Missing | `FE-INP-17`, `18` | Belum ada route/view/hook/action |
| Replace placeholder | `FE-INP-19` | Route ada, hanya berisi kalimat penantian |

Koreksi fakta terhadap roadmap revision `3`: `FE-INP-11` kini sudah dapat dicapai oleh supervisor
dari `FE-INP-16` → Detail Episode `Closed` → Sesi Koreksi. Yang tetap belum selesai pada
`FE-RWI-033` adalah pembuktian keterjangkauan **seluruh** 19 layar dan kepemilikan semua operasi.

Menu atau tampilan yang salah **tidak dibuang sekaligus**. Klasifikasi di atas menentukan nasibnya:
reuse tetap dipertahankan, extend diperbaiki melalui task pemilik, adapter mempertahankan komponen
dan menyesuaikan kontrak/navigation, conflict/placeholder diganti setelah target tersedia, dan
missing dibuat melalui task yang sudah ada. Status **repair** berarti label “task lama selesai” tidak
menjadikan tampilan sekarang final: logic yang benar dipertahankan, tetapi hasil layar wajib
diselaraskan melalui task delta baru. Khusus dua master/configuration, `FE-RWI-033` memindahkan induk
navigasi dan label `FE-INP-13`, sedangkan `FE-RWI-040/041` memperbaiki layarnya. Form admisi tunggal
baru dibongkar oleh `FE-RWI-034`, bukan oleh perubahan menu ini.

### 24.1 Bukti runtime enam layar dari pemilik

Enam screenshot yang diberikan pemilik pada 28 Agustus 2026 merupakan bukti keadaan runtime. Karena
gambar tidak berada sebagai file repository, buktinya dicatat sebagai **user-provided runtime
evidence**, bukan sebagai bukti source atau hasil test agent. Seluruh fakta source frontend pada
tabel di bawah dibaca dari repository `QuilvianSystemFrontendDev` SHA
`12562f17e12ee43b7d8cdaeaff3f1a1fca5a8360`.

| Layar | Yang terlihat pada runtime | Fakta source baca-saja | Kesimpulan delivery |
| --- | --- | --- | --- |
| `FE-INP-02` Papan Tempat Tidur | Ringkasan nol, daftar pasif, tidak ada aksi pada bed | `inpatient-bed-board-view.jsx:31` mengirim `selectable={false}`; tombol pilih hanya dirender bila selectable | `REPAIR`; `FE-RWI-036`, dengan aksi domain tetap berasal dari `FE-RWI-026/030` |
| `FE-INP-01` Census | Tabel kosong dan tidak ada jalan kerja selain filter | Source memiliki **Detail Episode** pada baris (`inpatient-census-view.jsx:131–140`), tetapi tidak ada baris runtime | `REPAIR`; tambah empty-state action dan pastikan aksi baris terbaca lewat `FE-RWI-037` |
| `FE-INP-09` Daftar Pantau | Tab dan tabel kosong; tindak lanjut tidak tampak | Source memiliki link Detail/Penutupan/Perpindahan pada baris; seluruhnya hilang dari pengalaman pengguna saat data kosong | `REPAIR`; `FE-RWI-038`; daftar tetap tidak menulis data |
| `FE-INP-10` Selisih Tempat Tidur | Laporan kosong; tombol ke papan tidak terbaca sebagai tindakan utama | Source memiliki link **Papan Tempat Tidur** (`inpatient-bed-drift-view.jsx:175–182`) | `REPAIR`; `FE-RWI-039`; tetap read-only dan tidak membuat aksi rekonsiliasi palsu |
| `FE-INP-13` Butir Administrasi | Daftar kosong; aksi baris tidak mungkin dipakai | Source memiliki **Tambah Butir** dan handler CRUD (`inpatient-clearance-item-view.jsx:138–180`, `232–240`) | `REPAIR`; `FE-RWI-040`; efektivitas runtime dan permission harus dibuktikan saat task dijalankan |
| `FE-INP-12` Pengaturan Rawat Inap | Hanya peringatan belum tersedia dan **Muat ulang** | Source menyembunyikan form ketika 404 dan hanya menampilkan submit bila `settingId` ada (`inpatient-setting-view.jsx:29–76`) | `REPAIR` + dependency data; `FE-RWI-041`; frontend tidak boleh mengarang `POST` atau nilai master |

---

## 25. Gerbang kontrak dan keputusan yang ditemukan

Skema tidak menyelesaikan gap kontrak dengan menggambar UI seolah datanya ada.

| ID | Temuan | Layar/task terdampak | Keputusan atau bukti yang dibutuhkan |
| --- | --- | --- | --- |
| `RWI-UI-GAP-001` | Dokumen approved menyebut jalur pasien lama **delapan langkah**, tetapi urutan bernama menghasilkan sembilan bila Cetak Persetujuan dihitung | `FE-INP-03`; `FE-RWI-022`, `035` | Product/UI owner menetapkan jumlah resmi atau memperbaiki rentang langkah. Sampai itu, skema mengikuti nama/urutan tanpa mengunci jumlah |
| `RWI-UI-GAP-002` | Target meminta penjamin perusahaan, tetapi source request kunjungan hanya membuktikan `Cash/Insurance` dan `PatientInsuranceId`; belum ada field penjamin perusahaan | `FE-INP-03`; `FE-RWI-024`, `025`, `035` | Kontrak request/response dan enum yang mendukung tiga cara bayar, atau keputusan scope yang mencabut opsi perusahaan |
| `RWI-UI-GAP-003` | List/detail episode dan bed board tidak membawa reservation aktif, `ReservationId`, dan `ExpiresAt` | `FE-INP-02`, `03`, `16`, `17`; `FE-RWI-020`, `026`, `030`, `032` | Endpoint baca server-authoritative beserta permission dan task backend pemilik |
| `RWI-UI-GAP-004` | Tidak ada GET financial clearance/riwayat; peran kasir juga belum terbukti memiliki baca discharge yang dipakai layar | `FE-INP-08`; `FE-RWI-013`, `035` | Kontrak baca keadaan + riwayat dan matriks permission kasir/billing |
| `RWI-UI-GAP-005` | Tidak ada GET sesi koreksi; refresh tidak dapat memulihkan sesi terbuka | `FE-INP-11`; `FE-RWI-018`, `035` | Kontrak baca sesi atau keputusan eksplisit bahwa sesi tidak perlu dipulihkan |
| `RWI-UI-GAP-006` | Endpoint pasien/identitas/kontak/penjamin/encounter yang dirujuk tanpa `/admin` dijaga policy kiosk; hak petugas admisi belum terbukti | `FE-INP-03`; `FE-RWI-023`–`025`, `035` | Pilih route operasional yang nyata (`/admin` atau non-admin) dan kunci permission-nya |
| `RWI-UI-GAP-007` | Runtime yang ditunjukkan pemilik belum memiliki master Rawat Inap yang layak: pengaturan `DEFAULT` tidak ditemukan, butir administrasi kosong, papan menunjukkan nol bed, dan tidak ada data episode untuk membuktikan aksi berbasis baris | `FE-INP-01`, `02`, `09`, `10`, `12`, `13`; `FE-RWI-036`–`041`, `035` | Admin Master Data/Tim Master Data menjalankan dan membuktikan pengisian `BE-RWI-002` pada environment target. Frontend tetap memperbaiki empty/error state, tetapi tidak boleh menanam data tiruan atau menambah `POST /inpatient-settings` |

`RWI-UI-GAP-003` sudah terbukti oleh laporan `FE-RWI-020`. Gap `007` terbukti oleh screenshot runtime
pemilik dan konsisten dengan source pengaturan yang berhenti pada 404. Lima gap lain berasal dari
impact scan dan perlu dipindahkan ke kontrak/task pemilik sebelum task terkait dinyatakan siap.

---

## 26. Sinkronisasi ke task frontend

Tabel ini membuat setiap task menunjuk layar/region yang dimilikinya. Ia tidak membuka ulang task
selesai; skema untuk layar tersebut merekam as-built dan hanya delta bertask terbuka yang boleh
mengubah source.

| Task | Skema pemilik | Kedudukan |
| --- | --- | --- |
| `FE-RWI-001` | bagian 7 dan master bed di luar 19 layar | Selesai; tidak dibuka ulang |
| `FE-RWI-002` | bagian 4 dan 23 | Selesai; kerangka pemanggilan/navigation |
| `FE-RWI-003` | `FE-INP-12`, bagian 17 | Selesai parsial menurut laporan lama; layarnya dipertahankan dan tidak dibuka ulang |
| `FE-RWI-004` | `FE-INP-13`, bagian 18 | Selesai parsial menurut laporan lama; layarnya dipertahankan dan tidak dibuka ulang |
| `FE-RWI-005`, `007` | `FE-INP-02`, bagian 7 | As-built papan dan penolakan; aksi baru dimiliki task lain |
| `FE-RWI-006` | `FE-INP-03` legacy dan `FE-INP-15`, bagian 3 dan 20 | Form legacy akan diganti; isolasi dapat dipakai ulang |
| `FE-RWI-008` | `FE-INP-01`, bagian 8 | As-built; metadata filter adalah delta `FE-RWI-033` |
| `FE-RWI-009`, `011` | `FE-INP-04`, bagian 9 | As-built detail/penanggung jawab |
| `FE-RWI-010` | `FE-INP-05`, bagian 10 | As-built transfer |
| `FE-RWI-012` | `FE-INP-06`, bagian 11 | As-built parsial |
| `FE-RWI-013` | `FE-INP-08`, bagian 13 | As-built parsial; tertahan `RWI-UI-GAP-004` |
| `FE-RWI-014` | `FE-INP-07`, bagian 12 | As-built penutupan |
| `FE-RWI-015` | `FE-INP-14`, bagian 19 | As-built kepergian |
| `FE-RWI-016` | `FE-INP-09`, bagian 14 | As-built empat daftar pantau |
| `FE-RWI-017` | `FE-INP-10`, bagian 15 | As-built laporan selisih |
| `FE-RWI-018` | `FE-INP-11`, bagian 16 | As-built dan kini terjangkau; gap baca sesi tetap dicatat |
| `FE-RWI-019` | seluruh bagian | Dibuka ulang dan digantikan `FE-RWI-035` |
| `FE-RWI-020` | `FE-INP-16`, bagian 6 | `AS_BUILT_PARTIAL`; gap reservation tidak menjadi acceptance baru retroaktif |
| `FE-RWI-021` | `FE-INP-19`, bagian 5 | Target beranda |
| `FE-RWI-022` | bagian 3.0–3.2 dan 3.4 | Kerangka/pembuka/Tipe Pasien; tertahan `RWI-UI-GAP-001` untuk jumlah langkah |
| `FE-RWI-023` | bagian 3.3–3.4 | Pendaftaran/pasien lama; tertahan `RWI-UI-GAP-006` |
| `FE-RWI-024` | bagian 3.5 | Pembayaran; tertahan `RWI-UI-GAP-002` |
| `FE-RWI-025` | bagian 3.6 | Dokter/titik tulis 1; tertahan `RWI-UI-GAP-002` dan `006` |
| `FE-RWI-026` | bagian 3.7–3.8 dan aksi reservation bagian 7 | Pilih/booking; tertahan `RWI-UI-GAP-003` untuk episode existing |
| `FE-RWI-027` | bagian 3.9 dan 3.13 | Konfirmasi/keluar alur |
| `FE-RWI-028` | bagian 3.10 dan `FE-INP-18` bagian 22 | Cetak dalam alur dan standalone |
| `FE-RWI-029` | bagian 3.11 | Kartu pasien |
| `FE-RWI-030` | `FE-INP-02`, bagian 7 | Konfirmasi masuk; metadata reservation tertahan gap 003 |
| `FE-RWI-031` | `FE-INP-17`, bagian 21 | Pembatalan dari worklist/detail |
| `FE-RWI-032` | `FE-INP-16` → `FE-INP-03`, bagian 6 dan 3 | Pemulihan; tertahan gap 003 |
| `FE-RWI-033` | bagian 23 serta delta menu/filter pada bagian terkait | Keterjangkauan seluruh layar; memindahkan dua menu master/configuration tanpa menduplikasi atau mengubah route |
| `FE-RWI-034` | bagian 3 dan 24 | Bongkar form legacy setelah target berdiri |
| `FE-RWI-036` | `FE-INP-02`, bagian 7 dan 24.1 | Repair tampilan/state papan serta integrasi aksi hasil `FE-RWI-026/030` |
| `FE-RWI-037` | `FE-INP-01`, bagian 8 dan 24.1 | Repair Census, empty-state action, dan keterlihatan Detail Episode |
| `FE-RWI-038` | `FE-INP-09`, bagian 14 dan 24.1 | Repair Daftar Pantau dan jalan tindak lanjut tanpa menambah write endpoint |
| `FE-RWI-039` | `FE-INP-10`, bagian 15 dan 24.1 | Repair laporan selisih; tetap read-only, navigasi ke papan dibuat jelas |
| `FE-RWI-040` | `FE-INP-13`, bagian 18 dan 24.1 | Repair Butir Administrasi dan pembuktian seluruh aksi CRUD/permission |
| `FE-RWI-041` | `FE-INP-12`, bagian 17 dan 24.1 | Repair Pengaturan; form bergantung pada baris master `DEFAULT` |
| `FE-RWI-035` | seluruh bagian 3–25 | Verifikasi akhir setelah `FE-RWI-036`–`041`; tidak menutup gap kontrak/data dengan mock tersembunyi |

---

## 27. Yang tetap `DEV_DISCRETION` dan di luar dokumen

| Hal | Kedudukan |
| --- | --- |
| Warna, ukuran, jarak, ikon, tipografi, breakpoint numerik | Mengikuti design token dan base component repository |
| Nama route final | Boleh menyesuaikan konvensi App Router untuk layar baru. Dua route existing `FE-INP-12/13` dipertahankan saat re-parenting; migrasi URL memerlukan task dan approval terpisah |
| Modal versus drawer | Bebas untuk aksi yang sama; satu kemampuan tetap hanya memiliki satu pemilik |
| Skema modul klinis, farmasi, IGD, billing, kiosk | Di luar Rawat Inap; hanya pola existing yang dipakai ulang |
| Penyimpanan persetujuan umum | Tetap di luar MVP; bagian 22 hanya mencetak |
| Aturan pasien meninggal/kabur | Tetap menunggu pemilik klinis dan tidak ditambahkan ke layar |

Dokumen dapat ditinjau pada desktop, medium, dan sempit sesuai bagian 4.2, tetapi detail visual baru
mengikat setelah pemilik menyetujui revision ini.

---

## 28. Traceability

| Bagian | Sumber |
| --- | --- |
| 0–2 | `03-frontend-architecture.md` bagian 0, 2, 2B, 2C, dan 9 |
| 3.0–3.13 | `RWI-DEC-075` s.d. `RWI-DEC-079`; arsitektur frontend 3A; `RWI-CAP-002`, `006` |
| 4 | arsitektur frontend bagian 5 dan 6; bukti komponen pada frontend SHA impact scan |
| 5 | `FE-INP-19`; `RWI-FE-005`; arsitektur frontend 2B |
| 6 | `FE-INP-16`; `IA-INP-02` s.d. `04`; laporan `FE-RWI-020` |
| 7–10 | `FE-INP-01`, `02`, `04`, `05`; arsitektur frontend 2A, 3, 4.3A, 5.2 |
| 11–22 | arsitektur frontend 2, 2A, 2C, 3; permission matrix `0.4.0`; aturan bisnis masing-masing |
| 23 | `IA-INP-01` s.d. `IA-INP-05`; `02-backend-architecture.md` bagian 4.12–4.13 dan 5.1; permission matrix bagian master data; sembilan submenu source terkini; brief UI pemilik 28 Agustus 2026 untuk target tujuh operasional + dua master data |
| 24 | impact scan frontend `12562f17e12ee43b7d8cdaeaff3f1a1fca5a8360`; enam screenshot runtime pemilik 28 Agustus 2026; spot-check backend master pada `b71a6a3d12190c4db60fe3433f10b6eb92131629` |
| 25 | source/contract impact scan 28 Agustus 2026; `FE-RWI-020.md` untuk gap reservation; bukti runtime pemilik untuk gap data master 007 |
| 26 | `roadmap/frontend-roadmap.md` revision `5` draft dan `roadmap/requirement-traceability.md` |
