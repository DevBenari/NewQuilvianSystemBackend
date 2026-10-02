<!--
INTAKE METADATA — ditambahkan saat penyimpanan, bukan bagian artifact.

  Evidence ID   : LAB-EVD-002
  Berkas asal   : Laboratorium (4).md
  Diserahkan    : Yoga Aji Pratama (pemilik modul), 2026-09-16
  Tingkat bukti : pemilik (atas sepuluh klarifikasi 16 Sep 2026); pengamatan (atas sisanya)
  Rekonsiliasi  : ../05-evidence-reconciliation.md bagian 11
  Turunannya    : LAB-DEC-064..LAB-DEC-069, BR-50, LAB-CONFLICT-009,
                  LAB-COORD-011, LAB-OPEN-028..LAB-OPEN-033

  Isi di bawah baris pemisah adalah SALINAN VERBATIM. Jangan disunting. Koreksi apa pun
  ditulis pada dokumen rekonsiliasi, bukan pada bukti.

  Catatan: baseline yang dirujuk artifact ini — `Laboratorium (3).md` dan
  `01-Menu-Hasil-Patologi-Klinik-Patologi-Anatomi-dan-Mikrobiologi.txt` — TIDAK ikut
  diserahkan dan tidak ada di repository ini. Rujukan `Sumber baris ...` di dalamnya
  karena itu tidak dapat diverifikasi.
-->

---

# Module Artifact - Laboratorium

## 1. Ringkasan Modul

Artifact ini mendokumentasikan **Menu Hasil Patologi Klinik, Patologi Anatomi dan Mikrobiologi** pada modul Laboratorium. Menu menggabungkan hasil Patologi Klinik, Patologi Anatomi, dan Mikrobiologi dalam satu datatable serta mencakup filter pencarian, status pemeriksaan dan pembayaran, informasi angka kritis, pencetakan Nota/Label, dan pengiriman hasil pemeriksaan kepada pasien melalui WhatsApp.

Versi ini merupakan pembaruan dari artifact `Laboratorium (3).md` berdasarkan klarifikasi user tanggal **16 September 2026**. Klarifikasi terbaru menetapkan aturan rentang tanggal, lifecycle status pemeriksaan, counter `Terkirim ke Pasien`, mekanisme pengiriman hasil WhatsApp, otorisasi tombol aksi, ukuran cetak default umum, serta perilaku pagination/sorting/empty/error state.

Untuk `Keyword Search`, requirement menetapkan bahwa kolom dan pola pencocokan harus mengikuti tabel/relasi dan query order pemeriksaan yang **sudah tersedia di Backend (BE) sistem**. Artifact ini tidak mengarang nama kolom BE yang belum diperiksa pada evidence saat ini.

## 2. Evidence yang Diproses

| Source | Type | Status | Coverage | Notes |
|---|---|---|---|---|
| `Laboratorium (3).md` | Module Artifact Markdown | processed / baseline | Seluruh artifact | Baseline struktur modul, feature, flow, rule, integrasi, dan 10 Unresolved Questions. |
| Klarifikasi user - 16 September 2026 | Current-turn clarification | processed new | Jawaban 1-10 | Menetapkan keputusan terbaru untuk seluruh Unresolved Questions pada baseline. |

### 2.1 Provenance Baseline yang Dipertahankan

Baseline sebelumnya mencatat sumber asli `01-Menu-Hasil-Patologi-Klinik-Patologi-Anatomi-dan-Mikrobiologi.txt` baris 1-48. Referensi `Sumber baris ...` pada artifact ini dipertahankan sebagai provenance dari baseline tersebut; file sumber mentah tersebut tidak diproses ulang pada pembaruan ini.

## 3. Actors / Roles

| Actor / Role | Keterlibatan yang Dinyatakan | Evidence |
|---|---|---|
| Petugas Lab / Admin | Menggunakan filter, melihat datatable, melakukan aksi cetak, mengirim/kirim ulang hasil, dan menjalankan aksi yang tersedia pada menu hasil. | Klarifikasi user 16 Sep 2026, jawaban 8 |
| Pasien | Pemilik data pemeriksaan, membayar tagihan, dan menerima hasil pemeriksaan melalui WhatsApp pada nomor yang tersimpan di master pasien. | Baseline sumber baris 4-5, 26-29, 39-46; klarifikasi jawaban 6 |
| Dokter Konfirmator | Ditampilkan dengan nama dan jabatan Dokter DPJP atau Dokter Lantai ketika terdapat angka kritis. | Baseline sumber baris 36 |
| Profesor | Memberikan persetujuan hasil sebelum hasil dapat dikirim kepada pasien. | Klarifikasi user, jawaban 6 |
| Dokter Lab | Memberikan persetujuan hasil sebelum hasil dapat dikirim kepada pasien. | Klarifikasi user, jawaban 6 |
| Kasir | Menjadi titik pembayaran tagihan pasien yang menyebabkan status pembayaran menjadi `Lunas`. | Baseline sumber baris 40 |

## 4. Capability / Feature Registry

| ID | Capability / Feature | Description | Actor | Evidence | Confidence |
|---|---|---|---|---|---|
| CAP-001 | Filter hasil pemeriksaan | Mencari data order memakai Keyword Search, NIK/No. RM, kategori periode, rentang tanggal, dan jenis kunjungan. Keyword Search mengikuti field/query order yang tersedia di BE. | Petugas Lab / Admin | Baseline sumber baris 2-16; klarifikasi jawaban 1 | High |
| CAP-002 | Reset filter | Membersihkan parameter yang terisi dan mengembalikan datatable ke hasil sebelum filter diterapkan. | Petugas Lab / Admin | Baseline sumber baris 17 | High |
| CAP-003 | Daftar hasil terpadu | Menampilkan hasil Patologi Klinik, Patologi Anatomi, dan Mikrobiologi dalam satu datatable. | Petugas Lab / Admin | Baseline sumber baris 1, 23-46 | High |
| CAP-004 | Pelacakan status pemeriksaan | Menampilkan status pemeriksaan beserta tanggal/waktu perubahan status. Alur normal wajib berjalan sampai `Selesai`; `Dibatalkan` hanya terjadi karena aksi pembatalan order oleh user. | Petugas Lab / Admin | Baseline sumber baris 30-35; klarifikasi jawaban 4 | High |
| CAP-005 | Informasi angka kritis | Menampilkan dokter konfirmator beserta jabatannya jika hasil memiliki angka kritis; jika tidak, menampilkan `-`. | Dokter Konfirmator; Petugas Lab / Admin | Baseline sumber baris 36 | High |
| CAP-006 | Pelacakan status pembayaran | Menampilkan `Belum ditagihkan`, `Belum Lunas`, atau `Lunas` berdasarkan verifikasi order dan pembayaran. | Petugas Lab / Admin; pasien; kasir | Baseline sumber baris 37-40 | High |
| CAP-007 | Counter Terkirim ke Pasien | Menampilkan angka integer jumlah keberhasilan pengiriman hasil melalui WhatsApp. Setiap pengiriman berhasil menambah nilai `+1`; kegagalan tidak menambah counter. | Petugas Lab / Admin; pasien | Baseline sumber baris 41; klarifikasi jawaban 5-6 | High |
| CAP-008 | Preview dan print dokumen laboratorium | Menampilkan preview Nota Lab, Label Lab, dan Label Goldar menggunakan ukuran default umum sesuai media cetak. | Petugas Lab / Admin | Baseline sumber baris 42-45; klarifikasi jawaban 9 | High |
| CAP-009 | Kirim hasil ke pasien | Mengirim satu dokumen hasil final per order melalui WhatsApp setelah seluruh pemeriksaan dalam order memiliki hasil dan hasil mendapat persetujuan Profesor serta Dokter Lab. | Petugas Lab / Admin; pasien; Profesor; Dokter Lab | Baseline sumber baris 46; klarifikasi jawaban 6-8 | High |
| CAP-010 | Kirim ulang hasil | Tombol kirim ulang tetap tersedia setelah fungsi pengiriman hasil dapat digunakan, termasuk ketika pengiriman sebelumnya gagal. | Petugas Lab / Admin | Klarifikasi jawaban 6 | High |
| CAP-011 | Loading state datatable | Menampilkan animasi loading selama sistem memuat data hasil, termasuk ketika rentang tanggal besar. | Petugas Lab / Admin | Klarifikasi jawaban 3 | High |
| CAP-012 | Pagination, sorting, empty, dan error state | Pagination menggunakan jumlah data per halaman dalam kelipatan 5, sorting default data terbaru di atas, alert saat data kosong, dan alasan error saat datatable gagal dimuat. | Petugas Lab / Admin | Klarifikasi jawaban 10 | High |

## 5. Menu / Screen / UI Elements

### 5.1 Menu Utama

- Nama: **Menu Hasil Patologi Klinik, Patologi Anatomi dan Mikrobiologi**.
- Menu memuat area utama:
  1. Filter Data.
  2. Datatable Hasil.
  3. Loading/empty/error state pada area datatable.
- Evidence baseline: sumber baris 1-2, 23.
- Tambahan state UI: klarifikasi user jawaban 3 dan 10.

### 5.2 Filter Data

| Elemen | Tipe / Opsi | Fungsi atau Ketentuan | Evidence |
|---|---|---|---|
| Keyword Search | Input bebas | Mencari order berdasarkan kolom/relasi yang memang tersedia pada tabel/query order pemeriksaan di BE sistem. Nama kolom dan pola pencocokan tidak boleh dibuat terpisah dari implementasi BE yang sudah ada. | Baseline sumber baris 4; klarifikasi jawaban 1 |
| NIK/No. RM | Input pencarian | Mencari order pemeriksaan berdasarkan NIK atau nomor rekam medis. | Baseline sumber baris 5 |
| Kategori Periode | Selection | Opsi: Tanggal Sampling, Tanggal Order, Tanggal Pemeriksaan. Default: Tanggal Order. | Baseline sumber baris 6-9, 20 |
| Tgl Awal | Input tanggal | Dapat memilih tanggal hari ini, tidak boleh memilih tanggal masa depan, dan nilainya wajib lebih kecil daripada Tgl Akhir. | Baseline sumber baris 10, 18-21; klarifikasi jawaban 2-3 |
| Tgl Akhir | Input tanggal | Dapat memilih tanggal hari ini, tidak boleh memilih tanggal masa depan, dan nilainya wajib lebih besar daripada Tgl Awal. | Baseline sumber baris 11, 18-21; klarifikasi jawaban 2-3 |
| Jenis Kunjungan | Selection | Opsi: Rawat Jalan, Rawat Inap, IGD. | Baseline sumber baris 12-15 |
| Search | Button | Menjalankan filter berdasarkan parameter yang terisi. Selama pemuatan, tampilkan animasi loading. | Baseline sumber baris 16; klarifikasi jawaban 3 |
| Reset | Button | Membersihkan parameter dan memulihkan datatable sebelum filter. | Baseline sumber baris 17 |

#### Aturan Rentang Tanggal

- Tanggal hari ini **boleh** dipilih untuk Tgl Awal maupun Tgl Akhir.
- Tanggal setelah hari ini tidak boleh dipilih.
- `Tgl Awal < Tgl Akhir`.
- Tidak ada batas maksimum jumlah hari/bulan/tahun untuk rentang pencarian.
- Jika proses load membutuhkan waktu, sistem wajib menampilkan animasi loading agar user mengetahui bahwa data sedang diproses.

### 5.3 Datatable Hasil

| Kolom | Isi yang Dinyatakan | Evidence |
|---|---|---|
| No. | Nomor urut. | Baseline sumber baris 25 |
| Order | Tanggal order dan No. Order. | Baseline sumber baris 26 |
| Registrasi | Tipe kunjungan dan No. Registrasi. | Baseline sumber baris 27 |
| Pasien | Nama Pasien, No. RM, dan ikon gender; biru/navy untuk laki-laki dan pink untuk perempuan. | Baseline sumber baris 28 |
| Unit Layanan | Poliklinik, kamar rawat inap, IGD, Laboratorium, atau Radiologi sesuai konteks kunjungan/asal pendaftaran. | Baseline sumber baris 29 |
| Status Pemeriksaan | Menunggu, Terkonfirmasi, Diproses, Selesai, atau Dibatalkan, beserta tanggal/waktu perubahan status. | Baseline sumber baris 30-35; klarifikasi jawaban 4 |
| Dokter Konfirmator | Nama dan jabatan Dokter DPJP/Dokter Lantai saat ada angka kritis; selain itu `-`. | Baseline sumber baris 36 |
| Pembayaran | Belum ditagihkan, Belum Lunas, atau Lunas. | Baseline sumber baris 37-40 |
| Terkirim ke Pasien | Angka integer jumlah pengiriman hasil WhatsApp yang berhasil. Setiap keberhasilan menambah `+1`. | Baseline sumber baris 41; klarifikasi jawaban 5-6 |
| Aksi | Nota Lab, Label Lab, Label Goldar, Kirim Hasil ke Pasien, dan Kirim Ulang sesuai kondisi. | Baseline sumber baris 42-46; klarifikasi jawaban 6, 8-9 |

### 5.4 Perilaku Datatable

| Aspek | Ketentuan |
|---|---|
| Pagination | Jumlah data per halaman menggunakan kelipatan 5, misalnya 5, 10, 15, 20, dan seterusnya. |
| Sorting | Data terbaru ditampilkan paling atas. |
| Loading state | Tampilkan animasi loading selama request/proses pemuatan data berlangsung. |
| Empty state | Jika hasil pencarian/filter kosong, tampilkan alert bahwa data tidak ditemukan/kosong. |
| Error state | Jika datatable gagal dimuat, tampilkan reason/alasan error agar user mengetahui penyebab kegagalan. |

Evidence: klarifikasi user jawaban 3 dan 10.

### 5.5 Keluaran Aksi dan Format Cetak

| Aksi | Keluaran / Kondisi | Ukuran Default Umum | Evidence |
|---|---|---|---|
| Nota Lab | Preview print berisi informasi pasien, data yang dibutuhkan untuk pemeriksaan, dan pemeriksaan yang dipilih pasien. | **A4 portrait, 210 x 297 mm** sebagai default umum. | Baseline sumber baris 43; klarifikasi jawaban 9 |
| Label Lab | Preview print berisi informasi utama pasien dan barcode dari nomor order; ditujukan untuk ditempel pada amplop hasil pemeriksaan pasien. | **100 x 50 mm, landscape** sebagai default umum label amplop. | Baseline sumber baris 44; klarifikasi jawaban 9 |
| Label Goldar | Preview print berisi informasi utama pasien; ditujukan untuk ditempel pada tube berisi sampling/specimen pasien. | **50 x 25 mm, landscape** sebagai default umum label tube. | Baseline sumber baris 45; klarifikasi jawaban 9 |
| Kirim Hasil ke Pasien | Mengirim satu dokumen hasil final per order ke nomor WhatsApp pasien yang tersimpan di master pasien. | PDF atau format file final yang tidak dapat diedit oleh pasien. | Baseline sumber baris 46; klarifikasi jawaban 6-7 |
| Kirim Ulang | Mengirim ulang dokumen hasil kepada pasien. | Tombol tetap tersedia termasuk setelah pengiriman sebelumnya gagal. | Klarifikasi jawaban 6 |

> Catatan ukuran cetak: ukuran di atas adalah **default umum implementasi**, bukan ukuran klinis/regulasi yang ditetapkan oleh evidence. Margin, DPI, dan penyesuaian akhir dapat mengikuti printer/media fisik yang digunakan rumah sakit tanpa mengubah isi data yang diwajibkan.

### 5.6 Otorisasi Tombol Aksi

Seluruh tombol aksi pada menu hasil digunakan oleh:

- **Petugas Lab**; dan/atau
- **Admin**.

Evidence: klarifikasi user jawaban 8.

## 6. Business Process / Ordered Flow

### BP-001 - Mencari Hasil Pemeriksaan

1. Petugas Lab/Admin membuka Menu Hasil Patologi Klinik, Patologi Anatomi dan Mikrobiologi.
2. Sistem menampilkan `Kategori Periode` default `Tanggal Order`. Evidence baseline: sumber baris 6-9, 19-20.
3. Petugas dapat mengisi Keyword Search, NIK/No. RM, Jenis Kunjungan, Tgl Awal, dan Tgl Akhir. Evidence baseline: sumber baris 4-5, 10-15.
4. Untuk Keyword Search, sistem menggunakan kolom/relasi dan pola query yang tersedia pada BE order pemeriksaan; artifact ini tidak menambahkan field BE baru. Evidence: klarifikasi jawaban 1.
5. Tanggal hari ini dapat dipilih, tanggal masa depan tidak dapat dipilih, dan `Tgl Awal < Tgl Akhir`. Tidak ada batas maksimum rentang. Evidence: klarifikasi jawaban 2-3.
6. Petugas memilih `Search`.
7. Sistem menampilkan animasi loading selama data sedang diproses. Evidence: klarifikasi jawaban 3.
8. Sistem menampilkan hasil dengan data terbaru di bagian paling atas dan pagination dalam kelipatan 5. Evidence: klarifikasi jawaban 10.
9. Jika tidak ada data, sistem menampilkan alert empty state. Jika terjadi error, sistem menampilkan reason/alasan error. Evidence: klarifikasi jawaban 10.

### BP-002 - Mereset Filter

1. Setelah filter pernah digunakan, Petugas Lab/Admin memilih `Reset`. Evidence baseline: sumber baris 17.
2. Parameter filter dibersihkan dan datatable kembali ke hasil sebelum filter diterapkan. Evidence baseline: sumber baris 17.

### BP-003 - Perubahan Status Pembayaran

1. Setelah order dibuat tetapi belum diverifikasi petugas/user, status pembayaran adalah `Belum ditagihkan`. Evidence baseline: sumber baris 38.
2. Setelah order diverifikasi tetapi belum dibayar pasien, status menjadi `Belum Lunas`. Evidence baseline: sumber baris 39.
3. Setelah pasien membayar tagihan di kasir, status menjadi `Lunas`. Evidence baseline: sumber baris 40.

### BP-004 - Menampilkan Dokter Konfirmator untuk Angka Kritis

1. Sistem mengevaluasi keberadaan angka kritis pada hasil order pemeriksaan. Evidence baseline: sumber baris 36.
2. Jika terdapat angka kritis, kolom Dokter Konfirmator menampilkan nama serta jabatan Dokter DPJP/Dokter Lantai. Evidence baseline: sumber baris 36.
3. Jika tidak terdapat angka kritis, kolom menampilkan `-`. Evidence baseline: sumber baris 36.

### BP-005 - Lifecycle Status Pemeriksaan

#### Alur Normal Wajib

`Menunggu -> Terkonfirmasi -> Diproses -> Selesai`

1. Order pemeriksaan dimulai pada status proses yang berlaku dalam workflow.
2. Transisi normal wajib mengikuti alur sampai status `Selesai`.
3. `Dibatalkan` bukan bagian dari alur normal wajib menuju `Selesai`.
4. Status `Dibatalkan` hanya terjadi ketika user melakukan pembatalan order pemeriksaan pasien.
5. Artifact tidak menetapkan perpindahan otomatis ke `Dibatalkan`; status tersebut harus dipicu oleh aksi pembatalan user.

Evidence: baseline sumber baris 30-35; klarifikasi user jawaban 4.

### BP-006 - Menyiapkan Satu Hasil Final per Order

1. Satu order dapat memiliki satu atau banyak item/pemeriksaan.
2. Agar hasil final order dianggap lengkap, **seluruh item/pemeriksaan dalam order wajib sudah mempunyai hasil**.
3. Banyaknya item/pemeriksaan tidak menghasilkan banyak dokumen hasil final pada level order.
4. Sistem membentuk **satu hasil/dokumen hasil final untuk satu nomor order**, yang mengonsolidasikan hasil dari seluruh pemeriksaan dalam order tersebut.

Evidence: klarifikasi user jawaban 7.

### BP-007 - Approval dan Pengiriman Hasil kepada Pasien via WhatsApp

1. Seluruh item/pemeriksaan pada order telah mempunyai hasil. Evidence: klarifikasi jawaban 7.
2. Sistem menyiapkan satu dokumen hasil final untuk nomor order tersebut. Evidence: klarifikasi jawaban 7.
3. Hasil wajib memperoleh persetujuan dari **Profesor** dan **Dokter Lab** sebelum dikirim kepada pasien. Evidence: klarifikasi jawaban 6.
4. Nomor WhatsApp tujuan diambil dari nomor pasien yang tersimpan pada data/master pasien. Evidence: klarifikasi jawaban 6.
5. Dokumen yang dikirim berbentuk PDF atau file laporan hasil final yang tidak dapat diedit oleh pasien. Evidence: klarifikasi jawaban 6.
6. Petugas Lab/Admin menjalankan aksi `Kirim Hasil ke Pasien`. Evidence: klarifikasi jawaban 8.
7. Jika pengiriman berhasil, nilai `Terkirim ke Pasien` bertambah `+1`. Evidence: klarifikasi jawaban 6.
8. Jika pengiriman gagal, nilai `Terkirim ke Pasien` tidak bertambah.
9. Tombol `Kirim Ulang` tetap disediakan, termasuk ketika pengiriman sebelumnya gagal. Evidence: klarifikasi jawaban 6.
10. Setiap pengiriman ulang yang berhasil menambah counter `Terkirim ke Pasien` sebesar `+1` lagi.

### BP-008 - Preview dan Print Dokumen

1. Petugas Lab/Admin memilih aksi `Nota Lab`, `Label Lab`, atau `Label Goldar`.
2. Sistem menampilkan preview dokumen sebelum proses print sesuai capability baseline.
3. Nota Lab menggunakan default umum A4 portrait.
4. Label Lab menggunakan default umum 100 x 50 mm landscape untuk ditempel pada amplop hasil pemeriksaan.
5. Label Goldar menggunakan default umum 50 x 25 mm landscape untuk ditempel pada tube sampling/specimen.
6. Ukuran akhir dapat disesuaikan dengan konfigurasi printer/media aktual tanpa menghilangkan informasi wajib pada dokumen/label.

Evidence: baseline sumber baris 42-45; klarifikasi user jawaban 9.

## 7. Business Rules, Validation, and Exceptions

| ID | Rule / Validation / Exception | Evidence | Confidence |
|---|---|---|---|
| RULE-001 | Kategori Periode harus ditentukan sebelum Tgl Awal dan Tgl Akhir digunakan. | Baseline sumber baris 18-19 | High |
| RULE-002 | Nilai default Kategori Periode adalah `Tanggal Order`. | Baseline sumber baris 8, 20 | High |
| RULE-003 | Tgl Awal dan Tgl Akhir boleh menggunakan tanggal hari ini, tetapi tidak boleh tanggal masa depan. | Klarifikasi jawaban 2 | High |
| RULE-004 | `Tgl Awal < Tgl Akhir`. | Klarifikasi jawaban 3 | High |
| RULE-005 | Tidak ada batas maksimum rentang antara Tgl Awal dan Tgl Akhir. | Klarifikasi jawaban 3 | High |
| RULE-006 | Saat data sedang dimuat, sistem wajib menampilkan animasi loading. | Klarifikasi jawaban 3 | High |
| RULE-007 | Unit Layanan menampilkan poliklinik untuk rawat jalan, kamar pasien untuk rawat inap, dan IGD untuk kunjungan IGD. | Baseline sumber baris 29 | High |
| RULE-008 | Penerimaan sampling atau pendaftaran Laboratorium dari Kiosk menampilkan Unit Layanan `Laboratorium`; pendaftaran radiologi dari Kiosk menampilkan `Radiologi`. | Baseline sumber baris 29 | High |
| RULE-009 | Nama/jabatan Dokter Konfirmator ditampilkan hanya jika terdapat angka kritis; jika tidak ada, tampil `-`. | Baseline sumber baris 36 | High |
| RULE-010 | Status pembayaran mengikuti kondisi verifikasi order dan pembayaran pasien: Belum ditagihkan, Belum Lunas, atau Lunas. | Baseline sumber baris 37-40 | High |
| RULE-011 | Alur normal status pemeriksaan wajib berjalan `Menunggu -> Terkonfirmasi -> Diproses -> Selesai`. | Klarifikasi jawaban 4 + baseline status baris 30-35 | High |
| RULE-012 | `Dibatalkan` hanya terjadi ketika user membatalkan order pemeriksaan pasien dan bukan transisi otomatis pada alur normal. | Klarifikasi jawaban 4 | High |
| RULE-013 | Keyword Search harus menyesuaikan field/relasi/query order yang sudah tersedia di BE; jangan menambah asumsi kolom yang tidak terdapat pada implementasi BE. | Klarifikasi jawaban 1 | High |
| RULE-014 | `Terkirim ke Pasien` adalah counter angka/integer, bukan rasio dan tidak menggunakan pembilang/penyebut. | Klarifikasi jawaban 5-6 | High |
| RULE-015 | Setiap keberhasilan pengiriman WhatsApp menambah counter `Terkirim ke Pasien` sebesar `+1`; kegagalan tidak menambah nilai. | Klarifikasi jawaban 6 | High |
| RULE-016 | Nomor tujuan WhatsApp berasal dari nomor pasien pada data/master pasien. | Klarifikasi jawaban 6 | High |
| RULE-017 | Hasil hanya boleh dikirim setelah memperoleh persetujuan Profesor dan Dokter Lab. | Klarifikasi jawaban 6 | High |
| RULE-018 | File hasil yang dikirim harus berupa PDF atau file laporan final yang tidak dapat diedit oleh pasien. | Klarifikasi jawaban 6 | High |
| RULE-019 | Tombol `Kirim Ulang` selalu tersedia setelah fungsi pengiriman hasil dapat digunakan, termasuk jika pengiriman sebelumnya gagal. | Klarifikasi jawaban 6 | High |
| RULE-020 | Seluruh item/pemeriksaan di dalam satu order harus mempunyai hasil sebelum hasil final order dianggap lengkap. | Klarifikasi jawaban 7 | High |
| RULE-021 | Satu nomor order hanya memiliki satu hasil/dokumen hasil final meskipun mempunyai banyak pemeriksaan. | Klarifikasi jawaban 7 | High |
| RULE-022 | Tombol aksi pada menu hasil digunakan oleh Petugas Lab/Admin. | Klarifikasi jawaban 8 | High |
| RULE-023 | Pagination menggunakan jumlah data per halaman dalam kelipatan 5. | Klarifikasi jawaban 10 | High |
| RULE-024 | Sorting default menempatkan data terbaru paling atas. | Klarifikasi jawaban 10 | High |
| RULE-025 | Empty state harus menampilkan alert ketika data kosong. | Klarifikasi jawaban 10 | High |
| RULE-026 | Error state datatable harus menampilkan reason/alasan error. | Klarifikasi jawaban 10 | High |
| RULE-027 | Bahan data berasal dari tabel atau relasi tabel yang sama dengan tiga menu pemeriksaan: Patologi Klinik, Patologi Anatomi, dan Mikrobiologi. | Baseline sumber baris 47-48 | High |
| RULE-028 | Ukuran cetak default umum: Nota Lab A4 portrait; Label Lab 100 x 50 mm landscape; Label Goldar 50 x 25 mm landscape. Profil printer aktual boleh menyesuaikan margin/DPI/media. | Klarifikasi jawaban 9 + default implementasi umum | Medium |

## 8. Data / Input / Output Observed

### 8.1 Input Filter

- Keyword bebas, dengan searchable field mengikuti implementasi BE order pemeriksaan.
- NIK atau No. RM.
- Kategori periode: Tanggal Sampling, Tanggal Order, atau Tanggal Pemeriksaan.
- Tgl Awal dan Tgl Akhir.
- Jenis kunjungan: Rawat Jalan, Rawat Inap, atau IGD.

### 8.2 Data yang Ditampilkan

- Identitas order: tanggal order dan No. Order.
- Identitas registrasi: tipe kunjungan dan No. Registrasi.
- Identitas pasien: Nama Pasien, No. RM, dan gender berbentuk ikon.
- Unit layanan sesuai konteks kunjungan atau sumber pendaftaran.
- Status pemeriksaan dan tanggal/waktu perubahan status.
- Nama dan jabatan dokter konfirmator pada hasil dengan angka kritis.
- Status pembayaran.
- `Terkirim ke Pasien` berupa angka integer jumlah pengiriman WhatsApp yang berhasil.
- Aksi Nota Lab, Label Lab, Label Goldar, Kirim Hasil, dan Kirim Ulang sesuai kondisi.

### 8.3 Enumerasi yang Dinyatakan

- Status Pemeriksaan: `Menunggu`, `Terkonfirmasi`, `Diproses`, `Selesai`, `Dibatalkan`.
- Status Pembayaran: `Belum ditagihkan`, `Belum Lunas`, `Lunas`.
- Jabatan Dokter Konfirmator: `Dokter DPJP`, `Dokter Lantai`.

### 8.4 Output

- Datatable hasil gabungan tiga kelompok pemeriksaan.
- Preview/print Nota Lab.
- Preview/print Label Lab dengan barcode nomor order.
- Preview/print Label Goldar.
- Satu dokumen hasil final per order yang menggabungkan hasil dari seluruh pemeriksaan dalam order.
- PDF/file laporan final non-editable yang dikirim melalui WhatsApp.
- Counter numerik keberhasilan pengiriman hasil ke pasien.

## 9. Integrations / Dependencies Mentioned

| Dependency | Ketentuan |
|---|---|
| Backend order pemeriksaan | Menentukan kolom/relasi dan pola query yang digunakan Keyword Search. Exact field mengikuti BE yang sudah ada dan tidak didefinisikan ulang di artifact ini. |
| Tabel/relasi Patologi Klinik, Patologi Anatomi, Mikrobiologi | Menjadi sumber data hasil terpadu sebagaimana baseline. |
| Master Pasien | Menjadi sumber nomor pasien/nomor WhatsApp tujuan. |
| Approval Profesor | Wajib terpenuhi sebelum hasil dikirim. |
| Approval Dokter Lab | Wajib terpenuhi sebelum hasil dikirim. |
| Kasir | Mengubah kondisi pembayaran menjadi `Lunas` setelah pembayaran pasien. |
| WhatsApp | Kanal pengiriman hasil final pasien. |
| Printer Nota Lab | Default A4 portrait; konfigurasi aktual dapat menyesuaikan perangkat. |
| Printer Label Lab | Default 100 x 50 mm landscape untuk media label amplop. |
| Printer Label Goldar | Default 50 x 25 mm landscape untuk media label tube specimen. |

## 10. Conflicts in Evidence

Ambiguitas baseline terkait tanggal telah **diselesaikan** oleh klarifikasi user tanggal 16 September 2026:

- tanggal hari ini boleh dipilih;
- tanggal masa depan tidak boleh dipilih;
- `Tgl Awal < Tgl Akhir`;
- tidak ada batas maksimum rentang tanggal.

Tidak ada konflik baru yang mengubah requirement bisnis setelah klarifikasi terbaru.

## 11. Unresolved Questions

Tidak ada **Unresolved Questions bisnis** yang tersisa dari daftar 10 pertanyaan pada baseline. Seluruh pertanyaan telah ditindaklanjuti dalam artifact ini.

### 11.1 Catatan Implementasi yang Tetap Perlu Diverifikasi Saat Development

1. **Keyword Search BE**  
   Exact nama kolom, relasi, operator pencocokan, dan bentuk query harus mengikuti tabel/query order pemeriksaan yang sudah ada pada Backend. Ini merupakan dependency implementasi, bukan keputusan bisnis yang perlu didefinisikan ulang pada artifact ini.

2. **Profil printer aktual**  
   Ukuran cetak telah diberi default umum. Margin, DPI, gap label, dan offset printer perlu disesuaikan dengan printer/media aktual rumah sakit saat implementasi atau UAT.

3. **Status sumber menuju `Dibatalkan`**  
   Requirement terbaru menetapkan bahwa `Dibatalkan` hanya terjadi ketika user membatalkan order dan bukan bagian alur normal wajib. Tidak ada aturan tambahan pada klarifikasi yang membatasi pembatalan hanya dari satu status sumber tertentu; implementasi tidak boleh membuat perpindahan otomatis tanpa aksi pembatalan user.

## 12. Source Coverage

| Source Ref | Status | Hasil Pemrosesan |
|---|---|---|
| `Laboratorium (3).md` | Processed baseline | Seluruh 204 baris baseline diperiksa dan digunakan sebagai struktur awal artifact. |
| Klarifikasi user - 16 September 2026 | Processed new | Seluruh jawaban 1-10 diterapkan ke feature registry, UI, flow, rule, data, integrations, conflicts, dan unresolved section. |

- ZIP containers: 0
- Evidence files/members total: 1 Markdown attachment
- Supplementary current-turn clarification: 1 set jawaban (10 butir)
- Processed new/changed: 1 attachment baseline + 1 clarification set
- Reused from cache: 0
- Unreadable/unsupported: 0
- Extraction scope: `Laboratorium`
- Workspace mode: writable fallback `/mnt/data/.basic-extraction-workspace/Laboratorium`
- Preferred `/workspace/agent_files/Laboratorium` unavailable/not writable in current runtime.
