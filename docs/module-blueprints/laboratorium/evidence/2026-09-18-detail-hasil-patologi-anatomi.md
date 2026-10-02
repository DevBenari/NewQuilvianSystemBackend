# Bukti Lapangan — `LAB-EVD-003`

| Field | Value |
|---|---|
| Evidence ID | `LAB-EVD-003` |
| Judul artifact | *Module Artifact - Laboratorium - Detail Hasil Patologi Anatomi* |
| Nama berkas asal | `Laboratorium-20--20Detail-20Hasil-20Patologi-20Anatomi (1).md` |
| Dilampirkan | 2026-09-18 oleh pemilik modul |
| Tingkat wewenang | Bukti analis dan klarifikasi pemilik proses — **tingkat 4**, di bawah keputusan pemilik modul |
| Sumber asalnya | `01-Halaman-detail-hasil-patologi-Anatomi.txt` lines 1-92, diwarisi dari artifact baseline |
| Rekonsiliasi | `05-evidence-reconciliation.md` bagian 12 — **putaran 4** |

> **Disimpan verbatim.** Isi di bawah adalah salinan apa adanya dari artifact yang dilampirkan
> pemilik modul. Ia **bukan** keputusan blueprint dan **bukan** desain. Penafsiran, pertentangan,
> dan konsekuensinya ditulis pada `05-evidence-reconciliation.md` bagian 12 — bukan di sini.

---

# Module Artifact - Laboratorium - Detail Hasil Patologi Anatomi

## 1. Ringkasan Modul

Artifact ini mendokumentasikan kebutuhan halaman detail hasil pemeriksaan Patologi Anatomi (PA) di modul Laboratorium. Halaman dibuka dari baris nomor order pasien dan digunakan untuk melihat konteks pasien/pemeriksaan, mencatat specimen, mengisi hasil berdasarkan kategori pemeriksaan, menetapkan Status Hasil PA, menyimpan draft atau final, mengonfirmasi hasil kritis, membuka kembali hasil final oleh pihak yang berwenang, serta mencetak hasil.

Versi ini merupakan pembaruan atas artifact sebelumnya dengan **14 klarifikasi requirement dari user pada 17 September 2026**. Klarifikasi terbaru diperlakukan sebagai keputusan requirement yang lebih baru ketika bertentangan dengan ketentuan pada artifact baseline. Secara khusus, versi ini memperjelas mandatory field hasil, klasifikasi nama pemeriksaan, hak akses, validasi duplikasi lokasi specimen, arti Diplo/Definitif, pengisian HL7 dan waktu diagnostik, aturan Draft/Final/Print/WhatsApp, data konfirmasi dokter, terjemahan Inggris, serta definisi hasil kritis PA.

Bukti asal artifact baseline tetap berasal dari `01-Halaman-detail-hasil-patologi-Anatomi.txt` lines 1-92 sebagaimana tercatat pada artifact sebelumnya. File TXT asal tidak diproses ulang pada run ini; provenance tersebut diwarisi dari artifact baseline yang di-upload.

## 2. Evidence yang Diproses

| Source | Type | Status | Coverage | Notes |
|---|---|---|---|---|
| `Laboratorium - Detail Hasil Patologi Anatomi.md` | Existing Module Artifact (`.md`) | processed as baseline | 206 lines | Artifact baseline yang di-upload pada turn ini; memuat hasil ekstraksi sebelumnya dan provenance sumber TXT asal. |
| `Klarifikasi user 2026-09-17 (Q1-Q14)` | Requirement clarification | processed | 14 jawaban | Menetapkan keputusan terbaru untuk seluruh unresolved question sebelumnya, kecuali satu fallback klasifikasi yang masih belum eksplisit. |
| `01-Halaman-detail-hasil-patologi-Anatomi.txt` | Inherited source provenance | inherited, not re-read in this run | lines 1-92 | Sumber asli yang tercatat dalam artifact baseline; spesifikasi halaman, field, kategori hasil, aksi, dan ketentuan print. |

## 3. Actors / Roles

| Actor / Role | Keterlibatan / Hak Akses | Evidence |
|---|---|---|
| Dokter Lab | Memiliki seluruh wewenang pada halaman hasil PA: membuka, mengisi, menyimpan draft, memfinalkan, mengonfirmasi hasil kritis, membuka kembali hasil final, dan mencetak hasil. | Klarifikasi user Q4 |
| Petugas Lab | Hanya berwenang membuka halaman hasil dan mencetak hasil. Tidak berwenang mengisi/mengubah hasil, menyimpan draft/final, mengonfirmasi, atau membuka kembali hasil. | Klarifikasi user Q4 |
| Penanggung Jawab Analis / Analis | Dicatat sebagai penanggung jawab; pilihan Analis menampilkan nama petugas lab. Pengisian pilihan pada form dilakukan oleh role yang memiliki hak edit, yaitu Dokter Lab. | baseline: sumber asli lines 20, 47; Klarifikasi user Q4 |
| Dokter Perujuk / DPJP | Ditampilkan pada informasi pemeriksaan dan dapat dipilih sebagai dokter konfirmator untuk hasil kritis. | baseline: lines 18, 83-87 |
| Dokter Lantai | Alternatif dokter konfirmator hasil kritis; dipilih dari dokter yang sedang bertugas. | baseline: lines 83-87 |
| Pasien | Subjek pemeriksaan dan penerima hasil; hasil melalui WhatsApp kepada pasien hanya boleh dikirim setelah hasil disimpan final. | baseline: lines 3, 5-13; Klarifikasi user Q9 |

## 4. Capability / Feature Registry

| ID | Capability / Feature | Description | Actor | Evidence | Confidence |
|---|---|---|---|---|---|
| CAP-001 | Membuka detail hasil PA | Klik dua kali pada baris nomor order membuka halaman detail hasil pemeriksaan. | Dokter Lab; Petugas Lab | baseline: line 3; Klarifikasi Q4 | High |
| CAP-002 | Menampilkan konteks pasien | Menampilkan identitas dan konteks kunjungan pasien. | Dokter Lab; Petugas Lab | baseline: lines 5-13 | High |
| CAP-003 | Menampilkan konteks pemeriksaan | Menampilkan registrasi/order, dokter, analis, dokter konfirmator kondisional, dan waktu mulai pemeriksaan. | Dokter Lab; Petugas Lab | baseline: lines 15-22; Klarifikasi Q14 | High |
| CAP-004 | Menampilkan diagnosis pemeriksaan | Menyediakan Diagnosa Awal, Riwayat Penyakit Relevan, Masa Terakhir Haid, dan Keterangan Klinis. | Dokter Lab; Petugas Lab | baseline: lines 24-28 | High |
| CAP-005 | Mengelola informasi specimen | Menyediakan specimen, volume, lokasi, pola, metode, dan waktu penerimaan sampling. | Dokter Lab | baseline: lines 30-39; Klarifikasi Q4 | High |
| CAP-006 | Menambah lokasi specimen manual | Opsi `lainnya` memunculkan input manual; sebelum ditambahkan ke master dilakukan pemeriksaan duplikasi dan alert ditampilkan jika duplikat. | Dokter Lab | baseline: line 33; Klarifikasi Q5 | High |
| CAP-007 | Mengelompokkan pemeriksaan | Mengategorikan pemeriksaan berdasarkan keyword pada nama pemeriksaan secara case-insensitive tanpa fuzzy/varian matching khusus. Jika tidak ada keyword kategori yang dikenali, kategori pemeriksaan tidak ditetapkan. | Sistem | baseline: lines 41-46; Klarifikasi Q2-Q3; Klarifikasi lanjutan fallback kategori | High |
| CAP-008 | Menampilkan format hasil dinamis | Field hasil berubah sesuai kategori pemeriksaan; parameter yang sama tidak diduplikasi dalam satu hasil order. | Sistem | baseline: lines 48-67, 90-91 | High |
| CAP-009 | Memvalidasi field hasil | Seluruh field hasil yang tampil sesuai kategori wajib memiliki nilai. Field hasil menggunakan format text tanpa batas panjang, satuan, atau rentang nilai khusus. | Sistem; Dokter Lab | Klarifikasi Q1 | High |
| CAP-010 | Auto-fill Diagnosa Klinis IHK | `Diagnosa Klinis` pada hasil IHK otomatis diambil dari `Diagnosa Awal`, bukan diketik manual. | Sistem | Klarifikasi Q1 | High |
| CAP-011 | Mencatat atribut Cito/Diplo/Definitif | Diplo dan Definitif bersifat indikator informasi; keduanya tidak mengubah flow atau output. | Dokter Lab | baseline: lines 68-70; Klarifikasi Q7 | High |
| CAP-012 | Mengisi laporan diagnostik | HL7 menampilkan list data HL7; Waktu Issued dan Waktu Efektif diisi manual; Status Hasil PA dipilih manual. | Dokter Lab | baseline: lines 71-78; Klarifikasi Q6, Q8, Q14 | High |
| CAP-013 | Menyimpan draft | Menyimpan hasil sementara yang tetap dapat diedit oleh Dokter Lab dan tetap dapat dicetak. Hasil draft tidak boleh dikirim kepada pasien melalui WhatsApp. | Dokter Lab | Klarifikasi Q4, Q9 | High |
| CAP-014 | Menyimpan final | Menyimpan hasil sebagai final dan mengunci editing normal. Hasil final dapat dikirim kepada pasien melalui WhatsApp. | Dokter Lab | baseline: line 81; Klarifikasi Q9 | High |
| CAP-015 | Membuka kembali hasil final | Dokter Lab dapat membuka kembali hasil final untuk diedit kembali; Petugas Lab tidak memiliki wewenang ini. | Dokter Lab | Klarifikasi Q4 | High |
| CAP-016 | Mengonfirmasi hasil kritis | Status Hasil PA `Kritis` dipilih manual; tombol konfirmasi aktif dan dokter konfirmator dapat berupa DPJP atau Dokter Lantai. | Dokter Lab; DPJP/Dokter Lantai | baseline: lines 75-78, 83-87; Klarifikasi Q14 | High |
| CAP-017 | Mencatat konfirmasi dokter | Konfirmasi menyimpan Dokter Konfirmator dan waktu konfirmasi. Tidak menghasilkan status klinis tambahan yang terpisah. | Sistem; Dokter Lab | Klarifikasi Q11 | High |
| CAP-018 | Mengirim konfirmasi kritis via WhatsApp | Pengiriman hasil kritis melalui WhatsApp memiliki status pengiriman, retry terbatas, audit per attempt, dan penanganan gagal; status pengiriman tidak dianggap sama dengan konfirmasi klinis. | Sistem; Dokter Lab | Klarifikasi Q10 + keputusan rekomendasi | High |
| CAP-019 | Mencetak laporan multi-halaman | Kop, footer, dan informasi pasien tetap tampil pada setiap lembar laporan multi-halaman. | Dokter Lab; Petugas Lab | baseline: line 88; Klarifikasi Q4 | High |
| CAP-020 | Mencetak bilingual dengan terjemahan otomatis | Bahasa Inggris dihasilkan dengan terjemahan otomatis; preview dapat diedit manual untuk kebutuhan cetak dan perubahan preview tidak menjadi versi hasil tersimpan. | Dokter Lab; Petugas Lab untuk print | baseline: line 92; Klarifikasi Q12-Q13 | High |

## 5. Menu / Screen / UI Elements

### 5.1 Titik Masuk

- Baris nomor order pemeriksaan pasien menerima interaksi klik dua kali untuk membuka halaman detail. Evidence: baseline line 3.
- Dokter Lab dan Petugas Lab dapat membuka halaman. Evidence: Klarifikasi Q4.

### 5.2 Informasi Pasien

- NIK; Nama Pasien; No. RM; Umur Pasien; Jenis Kelamin; Tipe Kunjungan; Unit Layanan; Penjamin. Evidence: baseline lines 5-13.

### 5.3 Informasi Pemeriksaan

- No. Registrasi; No. Order; Dokter Perujuk/DPJP; Penanggung Jawab Lab (Dokter Lab); Penanggung Jawab Analis; Tanggal Mulai Pemeriksaan (tanggal dan waktu). Evidence: baseline lines 15-22.
- `Dokter Konfirmator` relevan ketika **Status Hasil PA = Kritis**, bukan karena angka/nilai numerik kritis. Evidence: baseline line 21 diselaraskan oleh Klarifikasi Q14.
- Data konfirmasi yang disimpan adalah Dokter Konfirmator dan Waktu Konfirmasi. Evidence: Klarifikasi Q11.

### 5.4 Diagnosa Pemeriksaan

- Diagnosa Awal; Riwayat Penyakit Relevan; Masa Terakhir Haid; Keterangan Klinis. Evidence: baseline lines 24-28.
- Untuk IHK, `Diagnosa Klinis` pada bagian hasil otomatis mengambil nilai dari `Diagnosa Awal`. Evidence: Klarifikasi Q1.

### 5.5 Informasi Sampling / Specimen

| Elemen | Bentuk / Perilaku | Evidence |
|---|---|---|
| Specimen | Selection yang dapat dicari | baseline line 31 |
| Volume Specimen | Field; tipe input spesifik tidak ditetapkan pada baseline | baseline line 32 |
| Lokasi Specimen | Selection yang dapat dicari; opsi `lainnya` memunculkan input manual. Sebelum nilai manual ditambahkan ke master, sistem memeriksa duplikasi; jika duplikat, tampilkan alert dan jangan menambah record duplikat. | baseline line 33; Klarifikasi Q5 |
| Pola Pengambilan Specimen | Pilihan Tunggal, Serial, atau Hormonal | baseline lines 34-37 |
| Metode Pengambilan Specimen | Selection yang dapat dicari | baseline line 38 |
| Tanggal Penerimaan Sampling | Tanggal dan jam saat sampling diterima | baseline line 39 |

### 5.6 Form Hasil Pemeriksaan

- List pemeriksaan memuat pemeriksaan dalam satu nomor order, kategori pemeriksaan, dan harga. Evidence: baseline line 42.
- Pilihan Analis dapat dicari dan menampilkan nama petugas lab. Evidence: baseline line 47.
- Format Histologi dan Sitologi Non-Ginekologi: Makroskopik, Mikroskopik, Kesimpulan. Evidence: baseline lines 49-52.
- Format Sitologi Ginekologi: Kondisi, Kategori, Anjuran. Evidence: baseline lines 53-56.
- Format IHK: Diagnosa Klinis, Diagnosa PA, Reseptor Estrogen (ER), Reseptor Progesteron (PR), HER2, Ki-67, Status Reseptor Estrogen (ER), Status Reseptor Progesteron (PR), HER2 dengan pemeriksaan Imunohistokimia, dan Anjuran. Evidence: baseline lines 57-67.
- Seluruh field hasil yang ditampilkan sesuai kategori wajib terisi sebelum penyimpanan yang memerlukan hasil lengkap. Evidence: Klarifikasi Q1.
- Seluruh field hasil menggunakan format text; tidak ditetapkan batas panjang, satuan, atau rentang nilai khusus. Evidence: Klarifikasi Q1.
- `Diagnosa Klinis` IHK terisi otomatis dari `Diagnosa Awal`. Evidence: Klarifikasi Q1.
- Checkbox `Diplo`: jika dicentang hanya menyatakan hasil/pemeriksaan beratribut Diplo; tidak mengubah flow atau output. Evidence: Klarifikasi Q7.
- Checkbox `Definitif`: jika dicentang hanya menyatakan hasil sudah fix/definitif; tidak mengubah flow atau output. Evidence: Klarifikasi Q7.

### 5.7 Diagnostik Report dan Catatan

- HL7 berupa selection yang dapat dicari dan menampilkan list data yang berasal dari data HL7. Evidence: baseline line 72; Klarifikasi Q6.
- Waktu Issued dan Waktu Efektif berupa tanggal dan jam serta diisi **manual**. Evidence: baseline lines 73-75; Klarifikasi Q8.
- Status Hasil PA: Normal, Perlu Perhatian, Kritis. Pemilihan dilakukan manual oleh role yang berhak mengisi, yaitu Dokter Lab. Evidence: baseline lines 75-78; Klarifikasi Q4 dan Q14.
- Status `Kritis` pada PA adalah klasifikasi manual, bukan hasil evaluasi terhadap angka kritis. Evidence: Klarifikasi Q14.
- Catatan Hasil Pemeriksaan berupa form input. Evidence: baseline line 79.

### 5.8 Tombol Aksi dan Hak Akses

- `Simpan Draft`: hanya Dokter Lab.
- `Simpan Final`: hanya Dokter Lab.
- `Konfirmasi DPJP/Dokter Lantai`: hanya Dokter Lab dan hanya aktif ketika Status Hasil PA = `Kritis`.
- `Buka Kembali/Reopen`: hanya Dokter Lab.
- `Print`: Dokter Lab dan Petugas Lab; dapat dilakukan pada hasil Draft maupun Final.
- Pengiriman hasil kepada pasien melalui WhatsApp hanya tersedia setelah hasil Final.
- Pop-up konfirmasi memuat jenis Dokter Konfirmator. Untuk DPJP, nama dan nomor HP terisi otomatis. Untuk Dokter Lantai, Dokter Lab memilih dokter yang sedang bertugas dan nomor WhatsApp terisi sesuai pilihan. Evidence: baseline lines 84-87; Klarifikasi Q4 dan Q9.

### 5.9 Print Bahasa Inggris

- User memilih bahasa Indonesia atau Inggris. Evidence: baseline line 92.
- Preview Inggris dibuat melalui **terjemahan otomatis**. Evidence: Klarifikasi Q12.
- Preview Inggris dapat diedit manual sebelum cetak. Evidence: baseline line 92.
- Edit manual tersebut hanya memengaruhi output cetak saat itu dan tidak disimpan sebagai versi hasil baru. Evidence: Klarifikasi Q13.

## 6. Business Process / Ordered Flow

### BP-001 - Membuka dan Mengisi Detail Hasil PA

1. Dokter Lab atau Petugas Lab melakukan klik dua kali pada baris nomor order; sistem membuka halaman detail hasil. Evidence: baseline line 3; Klarifikasi Q4.
2. Sistem menampilkan konteks pasien, pemeriksaan, diagnosis, dan sampling/specimen. Evidence: baseline lines 5-39.
3. Petugas Lab berhenti pada mode read/print; semua kontrol perubahan data, save, finalisasi, konfirmasi, dan reopen tidak tersedia baginya. Evidence: Klarifikasi Q4.
4. Dokter Lab dapat memilih Analis dan mengisi data hasil. Evidence: baseline lines 47-79; Klarifikasi Q4.
5. Sistem mengategorikan pemeriksaan berdasarkan keyword nama secara case-insensitive. Evidence: baseline lines 43-46; Klarifikasi Q2-Q3.
6. Jika nama hanya memakai pola `HISTO` tanpa keyword kategori tambahan, kategori adalah Histologi. Jika terdapat keyword kategori tambahan yang dikenali, keyword tambahan tersebut menentukan kategori pemeriksaan. Dalam satu nama pemeriksaan, kombinasi keyword kategori dibatasi maksimal dua. Evidence: Klarifikasi Q2.
7. Jika nama pemeriksaan sama sekali tidak mengandung keyword kategori yang dikenali (`HISTO`, `PAPSMEAR`, `LBC`, `HPV`, `NON GINEKOLOGI`, atau `IHK`), sistem tidak menetapkan kategori pemeriksaan dan tidak melakukan fallback/tebakan kategori. Evidence: Klarifikasi lanjutan fallback kategori.
8. Sistem menampilkan gabungan parameter hasil sesuai kategori tanpa menduplikasi field yang sama. Evidence: baseline lines 48-67, 90-91.
9. Seluruh field hasil wajib terisi; khusus IHK, `Diagnosa Klinis` otomatis berasal dari `Diagnosa Awal`. Evidence: Klarifikasi Q1.
10. Dokter Lab melengkapi atribut hasil, HL7, Waktu Issued, Waktu Efektif, Status Hasil PA, dan catatan. Waktu Issued/Efektif diisi manual. Evidence: baseline lines 68-79; Klarifikasi Q6-Q8.

### BP-002 - Menyimpan Draft, Final, dan Membuka Kembali

1. Dokter Lab memilih `Simpan Draft`; hasil tersimpan sementara dan tetap dapat diedit oleh Dokter Lab. Evidence: Klarifikasi Q4 dan Q9.
2. Hasil Draft dapat dicetak oleh Dokter Lab maupun Petugas Lab. Evidence: Klarifikasi Q4 dan Q9.
3. Hasil Draft tidak boleh dikirim kepada pasien melalui WhatsApp. Evidence: Klarifikasi Q9.
4. Ketika hasil dianggap akhir, Dokter Lab memilih `Simpan Final`; hasil menjadi final dan editing normal dikunci. Evidence: baseline line 81; Klarifikasi Q4.
5. Hasil Final dapat dikirim kepada pasien melalui WhatsApp. Evidence: Klarifikasi Q9.
6. Bila koreksi diperlukan setelah final, hanya Dokter Lab yang dapat menjalankan `Buka Kembali/Reopen`; setelah dibuka kembali, hasil dapat diedit dan harus disimpan kembali sesuai state yang dipilih. Evidence: Klarifikasi Q4.

### BP-003 - Konfirmasi Hasil Kritis

1. Dokter Lab memilih Status Hasil PA = `Kritis` secara manual. Kritis pada PA tidak ditentukan dari angka kritis. Evidence: Klarifikasi Q14.
2. Sistem mengaktifkan tombol `Konfirmasi DPJP/Dokter Lantai`. Evidence: baseline line 83.
3. Dokter Lab membuka pop-up dan memilih Dokter Konfirmator: DPJP atau Dokter Lantai. Evidence: baseline lines 84-85.
4. Jika DPJP dipilih, sistem menampilkan otomatis nama DPJP dan nomor HP. Evidence: baseline line 86.
5. Jika Dokter Lantai dipilih, sistem menampilkan daftar dokter yang sedang bertugas; Dokter Lab memilih dokter dan sistem menampilkan nomor WhatsApp dokter. Evidence: baseline line 87.
6. Sistem mengirim hasil melalui WhatsApp untuk kebutuhan komunikasi hasil kritis dan mencatat status pengiriman secara terpisah dari konfirmasi klinis. Evidence: baseline line 83; Klarifikasi Q10.
7. Saat konfirmasi klinis dicatat, sistem hanya menyimpan Dokter Konfirmator dan Waktu Konfirmasi; tidak membuat status klinis tambahan. Evidence: Klarifikasi Q11.

### BP-004 - Pengiriman WhatsApp Hasil Kritis: Retry dan Failure Handling

> Bagian ini adalah **keputusan desain rekomendasi** yang diterapkan karena pada Q10 user secara eksplisit memilih mengikuti rekomendasi cara terbaik.

1. Saat pengiriman dimulai, sistem membuat satu record pengiriman dengan status awal `Menunggu`/`Pending` dan identifier unik agar pengiriman yang sama tidak diproses ganda secara bersamaan.
2. Setelah provider menerima permintaan, status menjadi `Terkirim ke Provider/Sent`. Jika provider menyediakan callback delivery, status diperbarui menjadi `Tersampaikan/Delivered`. `Dibaca/Read` boleh dicatat hanya jika benar-benar disediakan oleh provider WhatsApp yang digunakan.
3. Jika pengiriman gagal karena error sementara, sistem melakukan retry otomatis maksimal **3 kali** dengan interval bertahap/backoff yang konfiguratif.
4. Jika nomor penerima kosong/tidak valid, credential/provider menolak permanen, atau seluruh retry gagal, status menjadi `Gagal/Failed`; jangan terus melakukan retry tanpa batas.
5. Pada status `Gagal`, Dokter Lab melihat alasan kegagalan dan memperoleh aksi `Kirim Ulang`. Untuk hasil kritis, sistem juga harus mendorong fallback komunikasi manual/telepon agar komunikasi klinis tidak berhenti hanya karena WhatsApp gagal.
6. Setiap attempt dicatat dalam audit: No. Order/hasil, tujuan dokter, nomor tujuan, waktu attempt, actor, provider message ID bila ada, status, retry count, response/error code, dan waktu perubahan status. Nomor kontak yang ditampilkan pada UI/log operasional sebaiknya dimasking sesuai kebutuhan akses.
7. Keberhasilan pengiriman (`Sent/Delivered/Read`) **tidak otomatis** berarti hasil sudah dikonfirmasi secara klinis. Record konfirmasi klinis tetap mengikuti Q11: Dokter Konfirmator + Waktu Konfirmasi.
8. Kegagalan WhatsApp tidak membatalkan atau me-rollback hasil pemeriksaan; kegagalan hanya memengaruhi proses komunikasi dan harus tetap terlihat untuk ditindaklanjuti.

### BP-005 - Mencetak Hasil

1. Dokter Lab atau Petugas Lab memilih `Print`. Hasil Draft maupun Final boleh dicetak. Evidence: Klarifikasi Q4 dan Q9.
2. User memilih bahasa Indonesia atau Inggris. Evidence: baseline line 92.
3. Jika Inggris dipilih, sistem membuat terjemahan otomatis lalu menampilkan preview. Evidence: Klarifikasi Q12.
4. Preview Inggris dapat diedit manual untuk koreksi cetak; edit tersebut tidak menjadi versi hasil tersimpan. Evidence: baseline line 92; Klarifikasi Q13.
5. Sistem mencetak hasil. Bila lebih dari satu lembar, kop surat, footer, dan informasi pasien tampil pada setiap lembar. Evidence: baseline line 88.

### BP-006 - Menambah Lokasi Specimen Manual

1. Dokter Lab memilih `lainnya` pada Lokasi Specimen. Evidence: baseline line 33; Klarifikasi Q4.
2. Sistem menampilkan input lokasi manual. Evidence: baseline line 33.
3. Sebelum menambahkan ke master, sistem memeriksa apakah data yang sama sudah ada. Evidence: Klarifikasi Q5.
4. Jika duplikat ditemukan, sistem menampilkan alert dan tidak membuat data master duplikat. Evidence: Klarifikasi Q5.
5. Jika tidak duplikat, nilai dapat digunakan pada hasil dan ditambahkan ke master lokasi specimen. Evidence: baseline line 33; Klarifikasi Q5.

## 7. Business Rules, Validation, and Exceptions

| ID | Rule / Validation / Exception | Evidence | Confidence |
|---|---|---|---|
| RULE-001 | Nama yang mengandung `HISTO` dikategorikan sebagai Histologi jika tidak ada keyword kategori tambahan yang mengambil prioritas. | baseline lines 42-43; Klarifikasi Q2 | High |
| RULE-002 | Nama yang mengandung `PAPSMEAR`, `LBC`, atau `HPV` dikategorikan sebagai Sitologi Ginekologi. | baseline line 44 | High |
| RULE-003 | Nama yang mengandung `NON GINEKOLOGI` dikategorikan sebagai Sitologi Non-Ginekologi. | baseline line 45 | High |
| RULE-004 | Nama yang mengandung `IHK` dikategorikan sebagai Imunohistokimia. | baseline line 46 | High |
| RULE-005 | Pencocokan keyword tidak peka huruf besar/kecil (`case-insensitive`) dan tidak menggunakan pencocokan variasi/fuzzy khusus. | Klarifikasi Q3 | High |
| RULE-006 | Pola normal nama pemeriksaan memakai `HISTO` sebagai prefix. Jika terdapat keyword kategori tambahan yang dikenali, kategori mengikuti keyword tambahan tersebut. Kombinasi keyword kategori dalam satu nama pemeriksaan maksimal dua. | Klarifikasi Q2 | High |
| RULE-007 | Histologi dan Sitologi Non-Ginekologi menggunakan format Makroskopik, Mikroskopik, dan Kesimpulan. | baseline lines 48-52 | High |
| RULE-008 | Sitologi Ginekologi menggunakan format Kondisi, Kategori, dan Anjuran. | baseline lines 53-56 | High |
| RULE-009 | IHK menggunakan parameter hasil khusus yang tercantum pada bagian UI. | baseline lines 57-67 | High |
| RULE-010 | Seluruh field hasil yang ditampilkan berdasarkan kategori wajib memiliki nilai. | Klarifikasi Q1 | High |
| RULE-011 | Field hasil menggunakan format text dan tidak memiliki batas panjang, satuan, atau rentang nilai khusus yang ditetapkan. | Klarifikasi Q1 | High |
| RULE-012 | `Diagnosa Klinis` IHK diisi otomatis dari `Diagnosa Awal`. | Klarifikasi Q1 | High |
| RULE-013 | Bila beberapa kategori ada dalam satu order, format hasil digabung tanpa menduplikasi parameter yang sama. | baseline lines 90-91 | High |
| RULE-014 | Satu order dapat berisi beberapa pemeriksaan, tetapi mempunyai satu hasil terintegrasi. | baseline line 90 | High |
| RULE-015 | Lokasi specimen manual diperiksa terhadap duplikasi sebelum ditambahkan ke master; jika duplikat, tampilkan alert dan jangan menambah duplikat. | baseline line 33; Klarifikasi Q5 | High |
| RULE-016 | Checkbox `Diplo` hanya menandai atribut Diplo; tidak memengaruhi alur atau output. | Klarifikasi Q7 | High |
| RULE-017 | Checkbox `Definitif` hanya menandai bahwa hasil sudah fix/definitif; tidak memengaruhi alur atau output. | Klarifikasi Q7 | High |
| RULE-018 | Selection HL7 menampilkan list data yang berasal dari data HL7 yang tersedia. | Klarifikasi Q6 | High |
| RULE-019 | Waktu Issued dan Waktu Efektif diisi manual. | Klarifikasi Q8 | High |
| RULE-020 | Status Hasil PA `Kritis` dipilih manual; klasifikasi kritis PA tidak menggunakan ambang angka kritis. | Klarifikasi Q14 | High |
| RULE-021 | Tombol konfirmasi hanya aktif ketika Status Hasil PA = `Kritis`. | baseline line 83; Klarifikasi Q14 | High |
| RULE-022 | Dokter Lab memiliki hak membuka, mengisi, save draft, save final, konfirmasi, reopen, dan print. | Klarifikasi Q4 | High |
| RULE-023 | Petugas Lab hanya memiliki hak membuka dan print. | Klarifikasi Q4 | High |
| RULE-024 | Hasil Draft tetap dapat diedit oleh Dokter Lab dan **boleh dicetak** oleh Dokter Lab/Petugas Lab. | Klarifikasi Q9 | High |
| RULE-025 | Hasil Draft tidak boleh dikirim kepada pasien melalui WhatsApp; pengiriman hasil kepada pasien mensyaratkan hasil sudah Final. | Klarifikasi Q9 | High |
| RULE-026 | Hasil Final dikunci dari editing normal, tetapi Dokter Lab memiliki hak `Buka Kembali/Reopen`. | baseline line 81 diselaraskan dengan Klarifikasi Q4 | High |
| RULE-027 | Konfirmasi dokter tidak membuat status klinis baru; data yang disimpan hanya Dokter Konfirmator dan Waktu Konfirmasi. | Klarifikasi Q11 | High |
| RULE-028 | Preview Inggris berasal dari terjemahan otomatis. | Klarifikasi Q12 | High |
| RULE-029 | Edit manual pada preview Inggris hanya memengaruhi hasil cetak dan tidak disimpan sebagai versi hasil. | Klarifikasi Q13 | High |
| RULE-030 | Pada cetakan multi-halaman, kop, footer, dan informasi pasien tampil di setiap lembar. | baseline line 88 | High |
| RULE-031 | Status pengiriman WhatsApp hasil kritis minimal membedakan Pending/Menunggu, Sent/Terkirim ke Provider, Delivered/Tersampaikan bila didukung, dan Failed/Gagal; Read/Dibaca hanya digunakan bila provider benar-benar menyediakan callback tersebut. | Klarifikasi Q10 + keputusan rekomendasi | High |
| RULE-032 | Error sementara WhatsApp di-retry otomatis maksimal 3 kali dengan backoff konfiguratif; setelah gagal permanen/max retry, status tetap `Gagal` dan Dokter Lab dapat melakukan `Kirim Ulang`. | Klarifikasi Q10 + keputusan rekomendasi | High |
| RULE-033 | Setiap attempt WhatsApp diaudit dengan order/result, penerima, nomor tujuan, timestamp, actor, provider message ID bila ada, status, retry count, dan error/response code. | Klarifikasi Q10 + keputusan rekomendasi | High |
| RULE-034 | Delivery WhatsApp tidak sama dengan konfirmasi klinis; konfirmasi klinis tetap dicatat terpisah sebagai Dokter Konfirmator + Waktu Konfirmasi. | Klarifikasi Q10-Q11 + keputusan rekomendasi | High |
| RULE-035 | Untuk hasil kritis, kegagalan WhatsApp setelah retry harus terlihat jelas dan memicu fallback komunikasi manual/telepon; kegagalan komunikasi tidak me-rollback hasil pemeriksaan. | Klarifikasi Q10 + keputusan rekomendasi | High |
| RULE-036 | Jika nama pemeriksaan tidak mengandung keyword kategori yang dikenali (`HISTO`, `PAPSMEAR`, `LBC`, `HPV`, `NON GINEKOLOGI`, atau `IHK`), kategori pemeriksaan dibiarkan tidak terdefinisi/tidak diisi. Sistem tidak melakukan fallback dan tidak menebak kategori. | Klarifikasi lanjutan fallback kategori | High |

## 8. Data / Input / Output Observed

| Kelompok | Data / Input | Output / Relasi | Evidence |
|---|---|---|---|
| Identitas pasien | NIK, Nama Pasien, No. RM, Umur, Jenis Kelamin | Ditampilkan sebagai Informasi Pasien dan diulang pada setiap lembar print multi-halaman | baseline lines 5-10, 88 |
| Kunjungan | Tipe Kunjungan, Unit Layanan, Penjamin | Bagian Informasi Pasien | baseline lines 11-13 |
| Pemeriksaan | No. Registrasi, No. Order, DPJP, dokter lab, analis, waktu mulai | Menghubungkan order dengan penanggung jawab dan waktu pemeriksaan | baseline lines 15-22 |
| Diagnosis | Diagnosa Awal, Riwayat Penyakit Relevan, Masa Terakhir Haid, Keterangan Klinis | Konteks klinis; Diagnosa Awal menjadi sumber auto-fill Diagnosa Klinis IHK | baseline lines 24-28; Klarifikasi Q1 |
| Specimen | Specimen, volume, lokasi, pola, metode, waktu penerimaan | Lokasi manual dapat menambah master setelah lolos duplicate check | baseline lines 30-39; Klarifikasi Q5 |
| Daftar pemeriksaan | Nama pemeriksaan, kategori, harga | Keyword menentukan kategori dan format hasil | baseline lines 41-67; Klarifikasi Q2-Q3 |
| Hasil | Parameter kategori; Cito; Diplo; Definitif; catatan | Field hasil wajib; disimpan Draft atau Final; Diplo/Definitif hanya indikator | baseline lines 48-70, 79-82; Klarifikasi Q1, Q7, Q9 |
| Diagnostik | HL7, Waktu Issued, Waktu Efektif, Status Hasil PA | HL7 dari list data HL7; dua waktu manual; status PA dipilih manual | baseline lines 71-78; Klarifikasi Q6, Q8, Q14 |
| Konfirmasi klinis | Dokter Konfirmator, Waktu Konfirmasi | Dua data ini menjadi catatan konfirmasi; tidak ada status klinis baru | Klarifikasi Q11 |
| Pengiriman WhatsApp kritis | Order/result, dokter tujuan, nomor tujuan, attempt time, provider message ID, delivery status, retry count, error/response code, actor | Audit komunikasi hasil kritis; dipisahkan dari konfirmasi klinis | Klarifikasi Q10 + keputusan rekomendasi |
| Print | Bahasa, hasil terjemahan otomatis, preview Inggris editable | Laporan cetak; edit preview hanya untuk print saat itu | baseline lines 88, 92; Klarifikasi Q12-Q13 |
| Otorisasi | Role Dokter Lab / Petugas Lab | Menentukan kontrol edit/save/final/confirm/reopen/print yang tersedia | Klarifikasi Q4 |

## 9. Integrations / Dependencies Mentioned

- **Master lokasi specimen:** menerima nilai lokasi manual hanya setelah pemeriksaan duplikasi; duplikat memunculkan alert dan tidak ditambahkan. Evidence: baseline line 33; Klarifikasi Q5.
- **Data jadwal dokter lantai:** menyediakan daftar dokter lantai yang sedang bertugas untuk pemilihan Dokter Konfirmator. Evidence: baseline line 87.
- **Data dokter dan kontak:** menyediakan nama serta nomor HP/WhatsApp DPJP atau Dokter Lantai. Evidence: baseline lines 86-87.
- **WhatsApp provider:** media komunikasi hasil kritis kepada dokter konfirmator dan media pengiriman hasil kepada pasien setelah Final. Untuk komunikasi kritis diperlukan delivery status, retry, audit, dan failure handling sebagaimana BP-004. Evidence: baseline lines 82-87; Klarifikasi Q9-Q10.
- **HL7 data source/list:** selection HL7 mengambil list data HL7 yang tersedia. Jenis object/profil HL7 secara teknis tidak dirinci lebih lanjut oleh requirement. Evidence: baseline line 72; Klarifikasi Q6.
- **Layanan terjemahan otomatis:** menghasilkan preview bahasa Inggris untuk print; hasil edit manual preview tidak menjadi versi hasil tersimpan. Evidence: baseline line 92; Klarifikasi Q12-Q13.
- **Authorization/role management:** harus mampu membedakan kontrol Dokter Lab dan Petugas Lab sesuai RULE-022/RULE-023. Evidence: Klarifikasi Q4.

## 10. Conflicts in Evidence

1. **Print pada Draft**
   Artifact baseline menyatakan Draft tidak boleh dicetak. Klarifikasi terbaru Q9 menyatakan Draft **tetap boleh diprint**. Requirement operasional terbaru: Draft boleh dicetak oleh Dokter Lab dan Petugas Lab; pembatasan final hanya berlaku untuk pengiriman hasil kepada pasien melalui WhatsApp.

2. **Final tidak dapat diedit vs hak Reopen**
   Artifact baseline menyatakan hasil Final tidak dapat diedit kembali. Klarifikasi Q4 memberi Dokter Lab wewenang `membuka kembali`. Requirement terbaru diselaraskan menjadi: Final terkunci untuk editing normal, tetapi Dokter Lab dapat menjalankan Reopen untuk melakukan koreksi.

3. **Istilah angka kritis**
   Artifact baseline menggunakan istilah `angka kritis`. Klarifikasi Q14 menyatakan PA tidak menggunakan angka kritis; kondisi kritis ditentukan melalui `Status Hasil PA = Kritis` yang dipilih manual. Semua flow terbaru menggunakan Status Hasil PA, bukan threshold numerik.

4. **Mekanisme preview Inggris**
   Artifact baseline belum menjelaskan asal preview Inggris. Klarifikasi Q12 menetapkan bahwa preview berasal dari terjemahan otomatis; Q13 menetapkan edit manual hanya memengaruhi hasil cetak.

## 11. Unresolved Questions

Tidak ada unresolved question aktif dari daftar klarifikasi saat ini. Edge case fallback kategori telah diputuskan: bila nama pemeriksaan tidak mengandung keyword kategori yang dikenali, kategori pemeriksaan tidak perlu didefinisikan dan sistem tidak melakukan fallback/tebakan kategori.

## 12. Source Coverage

| Source | Manifest Type | SHA-256 / Identity | Processing | Coverage / Result |
|---|---|---|---|---|
| `Laboratorium - Detail Hasil Patologi Anatomi.md` | document | `c584099b57add8fe3f517a36ccbd544da0db251362dd0a9f7295b4d674408669` | Processed current-turn baseline | Seluruh 206 lines artifact baseline dibaca dan dijadikan basis pembaruan. |
| `Klarifikasi user 2026-09-17 (Q1-Q14)` | requirement clarification | current conversation | Processed new | 14 jawaban diterapkan ke role, capability, UI, business process, business rules, integration, dan conflict resolution. |
| `Klarifikasi user 2026-09-17 - fallback kategori tanpa keyword` | requirement clarification | current conversation | Processed new | Menetapkan bahwa nama pemeriksaan tanpa keyword kategori yang dikenali tidak diberi kategori; tidak ada fallback atau penebakan kategori. |
| `01-Halaman-detail-hasil-patologi-Anatomi.txt` | inherited source provenance | `d24b15f18b68f408b9f45c14242d39f8c1f412363aefda6bd7e3f33853b53cc0` | Inherited from baseline, not re-read this run | Baseline menyatakan lines 1-92 telah diekstrak; lines 93-101 kosong. |

- ZIP containers: 0
- Evidence files/members total in current runtime manifest: 1 attachment artifact baseline
- Additional requirement clarification sets processed: 2 (14 answers + 1 keputusan fallback kategori)
- Processed new/changed: 1 klarifikasi lanjutan pada artifact existing; baseline attachment dan 14 jawaban sebelumnya dipertahankan
- Reused from cache: 0
- Unreadable/unsupported: 0
- Extraction scope: `/mnt/data/.basic-extraction-workspace/Laboratorium - Detail Hasil Patologi Anatomi`
- Workspace mode: writable fallback under `/mnt/data` (preferred `/workspace/agent_files/...` was not available/writable in this runtime)
