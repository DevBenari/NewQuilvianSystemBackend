# Rawat Inap — Skema Tampilan

| Field | Nilai |
| --- | --- |
| Blueprint ID | `RWI-BP-001` |
| Revision | `0.1` |
| Status | `draft` — menunggu persetujuan pemilik |
| Cakupan revision ini | **Menu Admisi Rawat Inap saja.** Menu lain belum disusun |
| Masukan | `03-frontend-architecture.md` revision `0.4`; `roadmap/frontend-roadmap.md` revision `3` |
| Frontend SHA | `dec4fdeff07c3c96ad9f07f41f184c54cf771371` |
| Batas tulis | Hanya dokumen blueprint |

---

## 0. Kedudukan dokumen ini

`03-frontend-architecture.md` sengaja **tidak** menetapkan tata letak, dan menyerahkannya ke
`DEV_DISCRETION`. Dokumen ini menempati tingkat di atas `DEV_DISCRETION`, yaitu **brief UI yang
disetujui**:

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
| `RWI-FE-003` nama dan label langkah | **Ditutup** oleh dokumen ini |
| `RWI-FE-004` bentuk penanda langkah | **Ditutup** oleh dokumen ini |
| Warna, ukuran, jarak, ikon, tipografi | **Tetap** `DEV_DISCRETION`, mengikuti design system project |
| Pilihan komponen konkret | **Tetap** `DEV_DISCRETION`, tetapi bagian 6 menyebut komponen yang sudah ada supaya tidak dibuat kembar |

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

Register ini tumbuh satu menu per kali. Yang belum disusun ditulis apa adanya, bukan dikosongkan.

### Modul: Health Services / Rawat Inap

| Menu | Layar fungsional | Skema |
| --- | --- | :---: |
| Admisi Rawat Inap | `FE-INP-03` | ✅ **bagian 3** |
| Beranda Rawat Inap | `FE-INP-19` | ⬜ belum disusun |
| Daftar Kerja Episode | `FE-INP-16` | ⬜ belum disusun |
| Papan Tempat Tidur | `FE-INP-02` | ⬜ belum disusun |
| Pasien Sedang Dirawat | `FE-INP-01` | ⬜ belum disusun |
| Detail Episode | `FE-INP-04` | ⬜ belum disusun |
| Perpindahan Pasien | `FE-INP-05` | ⬜ belum disusun |
| Keputusan Pulang dan Resume | `FE-INP-06` | ⬜ belum disusun |
| Penutupan Episode | `FE-INP-07` | ⬜ belum disusun |
| Kelayakan Keuangan | `FE-INP-08` | ⬜ belum disusun |
| Daftar Pantau | `FE-INP-09` | ⬜ belum disusun |
| Selisih Tempat Tidur | `FE-INP-10` | ⬜ belum disusun |
| Sesi Koreksi | `FE-INP-11` | ⬜ belum disusun |
| Pengaturan Rawat Inap | `FE-INP-12` | ⬜ belum disusun |
| Butir Administrasi | `FE-INP-13` | ⬜ belum disusun |
| Pencatatan Kepergian | `FE-INP-14` | ⬜ belum disusun |
| Kebutuhan Isolasi | `FE-INP-15` | ⬜ belum disusun |
| Pembatalan Admisi | `FE-INP-17` | ⬜ belum disusun |
| Cetak Persetujuan | `FE-INP-18` | ⬜ bagian 3.13 memuat langkahnya di dalam admisi; layar berdiri sendirinya belum disusun |

---

## 3. Menu: Admisi Rawat Inap — `FE-INP-03`

Route: `/health-services/inpatient-management/admissions`

Kesembilan langkah dan isinya sudah dikunci `03-frontend-architecture.md` bagian 3A. Yang dokumen
ini tambahkan adalah **susunan wilayah di layar** dan **kata yang dipakai**.

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
| Penanda langkah | Kesembilan langkah beserta yang sedang berjalan | keadaan alur | `emergency-registration-stepper` |
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
  │   ✓ Sembilan langkah         │  │   ✓ Delapan langkah          │
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

                                          [ Lanjut ke Pendaftaran ]
```

| Wilayah | Isi | Dari mana | Komponen |
| --- | --- | --- | --- |
| Kartu jenis pasien | Enam pilihan tunggal | tetap | `base-checkbox-card` |
| Episode ibu | Isian pilihan episode aktif | daftar episode tersaring `Admitted` | `ResourceFilterSelect` |

| Tombol | Jenis | Kapan aktif | Yang terjadi |
| --- | --- | --- | --- |
| Lanjut ke Pendaftaran | utama | Jenis pasien terpilih; bila Bayi Baru Lahir, episode ibu wajib terisi | Maju ke langkah berikutnya |

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
| Simpan & Lanjut ke Pembayaran | utama | Seluruh isian wajib terisi; **mati selama permintaan berjalan** | `POST /patients` beserta dokumen identitas dan kontak darurat, lalu maju |

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
3. Sesudah berhasil, wilayah Tujuan Perawatan **terkunci** dan nomor episode tampil pada ringkasan
   berjalan.
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
| Kembali | kedua | selalu | Kembali ke langkah Dokter dalam keadaan terkunci |
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
  │  Pemesanan berlaku 120 menit. Selama itu tempat tidur tidak      │
  │  dapat dipesan pasien lain.                                      │
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
  │ RM 00123456 · Perempuan     │ │ No. 000123456789                │
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
| 1 Tipe Pasien | Lanjut ke Pendaftaran | – |
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

## 4. Keadaan yang berlaku di seluruh langkah

| Keadaan | Yang tampil | Letaknya |
| --- | --- | --- |
| Sedang memuat isian pilihan | Penanda memuat di dalam isian, bukan layar kosong | dalam wilayah |
| Sedang menyimpan | Tombol utama mati dan berpenanda memuat; tombol lain tetap dapat ditekan | wilayah aksi |
| Gagal — pesan server ada | `InformationAlert` merah berisi pesan server **apa adanya** | di atas isi langkah |
| Gagal — 409 | Pesan server, ditambah pemuatan ulang data yang basi. Isian **tidak hilang** | idem |
| Gagal — 422 | **Daftar** aturan yang gagal, satu baris per aturan | idem |
| Tidak berhak | Layar tidak dibuka sama sekali | `AccessDeniedGate` |
| Berhasil satu langkah | Notifikasi singkat, lalu berpindah langkah | `ToastStack` |

**Yang tidak boleh:** menggantikan pesan server dengan kalimat buatan sendiri, terutama pada
penolakan Kelayakan Penempatan yang menyebut nama kamar.

---

## 5. Yang belum disusun pada revision ini

| Yang belum ada | Alasan |
| --- | --- |
| Skema delapan belas menu Rawat Inap lainnya | Disusun menyusul, satu menu per kali |
| Skema modul lain | Dokumen ini khusus Rawat Inap |
| Perilaku pada layar sempit | Belum diputuskan apakah admisi dipakai di perangkat selain komputer meja |
| Bentuk cetak persetujuan yang presisi | Yang dikunci hanya isinya. Tata letak cetak `DEV_DISCRETION` |
| Ikon dan warna | `DEV_DISCRETION`, mengikuti design system project |

---

## 6. Traceability

| Bagian | Sumber |
| --- | --- |
| 0 | `03-frontend-architecture.md` bagian 0 urutan wewenang, bagian 9 `RWI-FE-003` dan `RWI-FE-004` |
| 3.0 | `03-frontend-architecture.md` 2C, 5.5, bagian 6 |
| 3.1 | `RWI-DEC-075`; `03-frontend-architecture.md` 3A.1 |
| 3.2 | `03-frontend-architecture.md` 3A.2 langkah 1; `RWI-DEC-020` bayi baru lahir |
| 3.3, 3.4 | `03-frontend-architecture.md` 3A.2 langkah 2, 3A.3 langkah 1–2 |
| 3.5 | `RWI-CAP-002`; `03-frontend-architecture.md` 3A.2 langkah 3, bagian 6 |
| 3.6 | `03-frontend-architecture.md` 3A.2 langkah 4, 3A.4 titik tulis 1, 3A.5 |
| 3.7 | `RWI-RULE-012`; `03-frontend-architecture.md` 4.3A, 5.2 |
| 3.8 | `RWI-CAP-006`; `03-frontend-architecture.md` 3A.4 titik tulis 2, 3A.6 |
| 3.9 | `03-frontend-architecture.md` 3A.2 langkah 7, 3A.7 |
| 3.10 | `RWI-DEC-077`; `RWI-DEC-035`; `03-frontend-architecture.md` 3A.8 |
| 3.13 | `03-frontend-architecture.md` 5.5, 3A.6 |
| 4 | `03-frontend-architecture.md` 5.1, 5.3, 5.4 |
