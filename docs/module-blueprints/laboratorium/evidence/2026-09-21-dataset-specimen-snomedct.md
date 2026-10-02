# `LAB-EVD-007` — Dataset specimen SNOMED CT (2026-09-21)

| Field | Nilai |
|---|---|
| Evidence ID | `LAB-EVD-007` |
| Berkas | `snomedct_specimen_dikelompokan_berdasarkan_jenis.xlsx` |
| Diserahkan | Pemilik modul, 2026-09-21 |
| Menutup | `LAB-OPEN-040` |
| Melahirkan | `LAB-DEC-129` sampai `LAB-DEC-132` (amendment pass putaran 13) |

## 1. Bentuk berkas

Tiga sheet:

| Sheet | Baris | Kegunaan |
|---|---:|---|
| `Semua Specimen` | **1.767** data + 1 header | Sumber operasional |
| `Ringkasan` | 98 data | Rekap; **tidak dipakai** (`LAB-DEC-005`) |
| `Perlu Review` | 166 data | Baris berkonfidensi `Rendah`; **tidak dipakai sebagai pilihan** |

Tujuh kolom, persis seperti yang disebut artifact: `ssid`, `code`, `display`,
`jenis_specimen`, `subjenis_specimen`, `confidence_klasifikasi`, `dasar_klasifikasi`.

**Jumlah 1.767 terverifikasi.** Klaim artifact benar apa adanya.

## 2. Temuan yang mengubah rancangan

### 2.1 Datanya BERTINGKAT TIGA, bukan dua

| Tingkat | Kolom | Nilai unik |
|---|---|---:|
| 1 | `jenis_specimen` | **31** |
| 2 | `subjenis_specimen` | **85** |
| 3 | `display` | **1.767** |

Nesting-nya **bersih**: setiap `subjenis_specimen` milik tepat satu `jenis_specimen`, nol
kecuali.

### 2.2 Tetapi tingkat kedua hampir kosong gunanya

**21 dari 31 kelompok hanya punya SATU subjenis.** Hanya sepuluh yang benar-benar bercabang:

| Kelompok | Jumlah subjenis |
|---|---:|
| Cairan Tubuh | 25 |
| Sekret / Isi Organ | 11 |
| Jaringan / Spesimen Bedah | 9 |
| Sumsum Tulang, Sputum/Respirasi, Sitologi/Smear, Rambut/Kuku | 3 masing-masing |
| Tulang/Gigi, Darah, Aspirat/Pungsi | 2 masing-masing |

Inilah yang membatalkan pilihan artifact atas `subjenis_specimen` sebagai ruas **Specimen**:
pada dua pertiga data, memilihnya berarti melewati layar yang nol punya alternatif.

### 2.3 Ketujuh nilai yang sudah ter-seed punya padanan

`LabSpecimenTypeSeeder` mengisi tujuh nilai, dan **seluruhnya** ada di antara 31 kelompok:

| Ter-seed | Padanan pada dataset | Baris |
|---|---|---:|
| Blood | Darah | 54 |
| Urine | Urine | 45 |
| Body Fluid | Cairan Tubuh | 175 |
| Sputum | Sputum / Specimen Respirasi | 13 |
| Pus | Pus / Luka / Drainase | 23 |
| Jaringan | Jaringan / Spesimen Bedah | 434 |
| Lainnya | Lainnya / Belum Spesifik | 8 |

Perluasan 7 → 31 karena itu **penambahan beserta penggantian nama**, bukan pembongkaran.

### 2.4 Seluruh `display` berbahasa Inggris, dan nol duplikat

Contoh: `Sigmoid colon biopsy specimen`, `Bursa tissue specimen`,
`Specimen from right lower lobe of lung obtained by bronchial aspiration procedure`.
Pemeriksaan duplikasi atas 1.767 nilai: **nol duplikat**.

### 2.5 Konfidensi rendah terpusat pada dua kelompok

| Konfidensi | Baris |
|---|---:|
| Tinggi | 1.113 |
| Sedang | 488 |
| **Rendah** | **166** |

Ke-166 baris `Rendah` seluruhnya berasal dari dua kelompok: **158** dari
`Spesimen Anatomi - Material Tidak Disebutkan` dan **8** dari `Lainnya / Belum Spesifik`.
Keduanya menyebut **lokasi tanpa menyebut bahan** — misalnya `Specimen from abdominal cavity`.

## 3. Sebaran lengkap 31 kelompok

| Kelompok | Baris | Subjenis | Berkonfidensi Rendah |
|---|---:|---:|---:|
| Jaringan / Spesimen Bedah | 434 | 9 | 0 |
| Biopsi | 195 | 1 | 0 |
| Cairan Tubuh | 175 | 25 | 0 |
| Spesimen Anatomi - Material Tidak Disebutkan | 158 | 1 | 158 |
| Sitologi / Smear / Brushing | 136 | 3 | 0 |
| Swab | 121 | 1 | 0 |
| Aspirat / Pungsi | 113 | 2 | 0 |
| Perangkat / Kateter / Benda Asing | 93 | 1 | 0 |
| Darah | 54 | 2 | 0 |
| Urine | 45 | 1 | 0 |
| Pus / Luka / Drainase | 23 | 1 | 0 |
| Lingkungan / Non-pasien | 19 | 1 | 0 |
| Sekret / Isi Organ | 16 | 11 | 0 |
| Produk Kehamilan / Plasenta | 16 | 1 | 0 |
| ASI / Milk | 14 | 1 | 0 |
| Sumsum Tulang | 13 | 3 | 0 |
| Sputum / Specimen Respirasi | 13 | 3 | 0 |
| Saliva / Oral Fluid | 13 | 1 | 0 |
| Rambut / Kuku / Kerokan | 13 | 3 | 0 |
| Serum | 12 | 1 | 0 |
| Isolat / Organisme | 12 | 1 | 0 |
| Plasma | 11 | 1 | 0 |
| Feses / Stool | 11 | 1 | 0 |
| Tulang / Gigi | 10 | 2 | 0 |
| Wadah / Preparat / Media Koleksi | 9 | 1 | 0 |
| Lainnya / Belum Spesifik | 8 | 1 | 8 |
| Batu / Kalkulus / Kristal | 8 | 1 | 0 |
| Semen | 7 | 1 | 0 |
| Sel / Material Seluler | 7 | 1 | 0 |
| Material Molekuler | 4 | 1 | 0 |
| Kontrol / Material Referensi | 4 | 1 | 0 |

## 4. Pertentangan yang dibuka bukti ini

| Temuan | Bertentangan dengan | Ditutup oleh |
|---|---|---|
| Tiga tingkat, dan tingkat kedua hampir kosong gunanya | `LAB-DEC-098` memilih dua tingkat dari subjenis | `LAB-DEC-129` |
| Dasar penyaringan isi awal kini terlihat | `LAB-DEC-099` menyerahkannya tanpa dasar | `LAB-DEC-130` |
| 1.767 nama seluruhnya Inggris | `LAB-DEC-008` mewajibkan padanan Indonesia | `LAB-DEC-131` |
| Setiap baris punya kode SNOMED CT | `LAB-DEC-098` menetapkan kode sendiri | `LAB-DEC-132` |

## 5. Catatan bagi pelaksana seeder

Berkas sumber ada pada mesin pemilik modul, bukan di dalam repository. Ekstraksi yang dipakai
analisis ini membaca `xl/worksheets/sheet1.xml` beserta `xl/sharedStrings.xml` langsung dari
dalam `.xlsx` — nol pustaka tambahan dibutuhkan. Sheet `Ringkasan` dan `Perlu Review`
**tidak** ikut diimpor sebagai pilihan; sheet `Perlu Review` hanya menjadi dasar penonaktifan
ke-166 baris.
