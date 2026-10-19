# PRODUCT REQUIREMENTS DOCUMENT (PRD)
## MVP — Prakiraan Biaya Rawat Inap
**Sistem:** Quilvian Hospital Information System  
**Modul:** Admisi Rawat Inap  
**Feature Name:** Inpatient Cost Estimation  
**Versi:** 1.0 — Draft Implementasi  
**Tanggal:** 16 Oktober 2026  
**Prioritas:** P0 — MVP

---

# 1. Ringkasan Produk

## 1.1 Latar Belakang

Rumah sakit perlu memberikan informasi prakiraan biaya kepada pasien atau keluarga ketika pasien akan menjalani rawat inap.

Pada tahap tersebut, sebagian besar pelayanan belum dilaksanakan, sehingga belum tersedia transaksi aktual untuk seluruh obat, tindakan, pemeriksaan penunjang, konsultasi dokter, maupun fasilitas perawatan.

Oleh karena itu, sistem membutuhkan fitur **Prakiraan Biaya Rawat Inap** yang menghitung perkiraan biaya berdasarkan data klinis awal, tarif rumah sakit, kelas perawatan, estimasi lama rawat, dan rencana pelayanan.

Hasil prakiraan merupakan informasi awal kepada pasien, bukan tagihan final dan bukan paket harga.

## 1.2 Pernyataan Masalah

Permasalahan yang ingin diselesaikan:

1. Billing aktual belum tersedia ketika pasien baru akan masuk Rawat Inap.
2. Sistem belum tentu mengetahui seluruh tindakan, obat, pemeriksaan, atau lama rawat pasien.
3. Tidak semua pasien membutuhkan operasi, implant, radiologi, atau ICU.
4. Petugas Admisi membutuhkan dokumen prakiraan biaya tanpa membuat transaksi pelayanan fiktif.
5. Nilai prakiraan harus dapat diperbarui jika rencana perawatan berubah.
6. Pasien harus memahami komponen yang sudah diperkirakan dan komponen yang belum termasuk.

## 1.3 Tujuan Produk

Menyediakan fitur yang memungkinkan rumah sakit:

- Menghasilkan prakiraan biaya sebelum pelayanan Rawat Inap berlangsung.
- Menggunakan tarif master resmi sebagai dasar perhitungan.
- Menghitung biaya berdasarkan rencana perawatan yang relevan.
- Memisahkan prakiraan biaya utama dan potensi biaya tambahan.
- Menghasilkan dokumen prakiraan yang dapat dicetak.
- Menyimpan versi dan riwayat perubahan estimasi.
- Menjaga prakiraan tetap terpisah dari Billing aktual.

## 1.4 Prinsip Utama

**Prakiraan biaya bukan paket layanan, bukan tagihan, dan bukan jaminan nilai akhir biaya perawatan.**

Prakiraan tidak boleh:

- Membuat transaksi Billing.
- Membuat resep atau order pelayanan.
- Menganggap seluruh tindakan umum pasti dilakukan.
- Menetapkan biaya operasi atau implant tanpa dasar rencana.
- Menampilkan Rp0 untuk komponen yang sebenarnya belum diketahui.
- Mengganti atau menghapus prakiraan yang sudah diterbitkan tanpa riwayat revisi.

---

# 2. Ruang Lingkup MVP

## 2.1 Termasuk MVP (P0)

| ID | Fitur | Prioritas |
|---|---|---|
| MVP-001 | Integrasi permintaan Rawat Inap dari IGD | P0 |
| MVP-002 | Integrasi permintaan Rawat Inap dari Poliklinik | P0 |
| MVP-003 | Pengambilan tarif master | P0 |
| MVP-004 | Input estimasi lama rawat | P0 |
| MVP-005 | Estimasi biaya kamar | P0 |
| MVP-006 | Estimasi visit dokter | P0 |
| MVP-007 | Estimasi tindakan Rawat Inap | P0 |
| MVP-008 | Estimasi operasi dan implant yang direncanakan | P0 |
| MVP-009 | Estimasi obat dan BHP berdasarkan rencana | P0 |
| MVP-010 | Estimasi laboratorium dan radiologi | P0 |
| MVP-011 | Estimasi konsultasi khusus | P0 |
| MVP-012 | Estimasi ICU jika direncanakan | P0 |
| MVP-013 | Potensi biaya tambahan | P0 |
| MVP-014 | Kalkulasi otomatis dan validasi estimasi | P0 |
| MVP-015 | Preview dan cetak dokumen | P0 |
| MVP-016 | Penyimpanan snapshot dan revisi | P0 |
| MVP-017 | Hak akses dan audit trail | P0 |

## 2.2 Tidak Termasuk MVP

Fitur berikut ditunda ke pengembangan lanjutan:

- Prediksi biaya menggunakan AI atau machine learning.
- Prediksi otomatis berdasarkan histori ribuan episode.
- Clinical pathway engine lengkap.
- Prediksi komplikasi dan probabilitas masuk ICU.
- Perhitungan otomatis jaminan pembayaran penjamin yang belum terintegrasi.
- Simulasi deposit otomatis.
- Perbandingan prakiraan versus realisasi biaya secara analitik.
- Notifikasi otomatis kepada keluarga saat biaya diperkirakan meningkat.
- Tanda tangan digital tersertifikasi jika belum tersedia pada sistem.

**Catatan:** Integrasi master tarif dan data klinis yang sudah ada harus diutamakan. MVP tidak mengharuskan pembuatan ulang master data atau modul pelayanan.

---

# 3. Pengguna dan Hak Akses

| Role | Kewenangan |
|---|---|
| Dokter IGD / Poliklinik | Menentukan rencana klinis, indikasi, serta menyetujui permintaan Rawat Inap |
| Perawat IGD / Poliklinik | Membantu menginput permintaan dan rencana sesuai instruksi dokter |
| Petugas Admisi Ranap | Membuat, menghitung, meninjau, dan menerbitkan prakiraan biaya |
| Bed Manager | Mengelola alokasi kamar dan tempat tidur |
| Petugas Tarif / Keuangan | Mengelola master tarif berdasarkan kewenangan |
| Supervisor Admisi | Melakukan review, revisi administratif, dan pembatalan sesuai otorisasi |

### Aturan akses

- Admisi tidak boleh mengubah diagnosis atau menetapkan tindakan medis atas inisiatif sendiri.
- Petugas Admisi dapat memilih komponen rencana yang sudah disahkan serta memasukkan perkiraan kuantitas berdasarkan rencana terdokumentasi.
- Perubahan pada dasar klinis harus dikonfirmasi oleh petugas klinis berwenang.
- Harga satuan harus berasal dari master tarif yang berlaku.
- Perubahan atau pembatalan dokumen wajib dicatat.

---

# 4. Alur Bisnis Utama

## 4.1 Alur dari IGD / Poliklinik

1. Dokter menentukan bahwa pasien membutuhkan Rawat Inap.
2. Perawat membantu membuat Permintaan Rawat Inap.
3. Dokter memverifikasi dan mengesahkan permintaan.
4. Permintaan masuk ke daftar kerja Admisi Ranap.
5. Admisi membuka detail permintaan.
6. Sistem menampilkan data pasien, diagnosis, tujuan perawatan, kelas, penjamin, dan rencana klinis yang tersedia.
7. Admisi melengkapi parameter nonklinis dan estimasi berdasarkan rencana yang sudah disetujui.
8. Sistem mengambil tarif master sesuai konteks pasien.
9. Sistem menghitung seluruh komponen yang dapat diperkirakan.
10. Komponen tanpa dasar perkiraan ditampilkan dengan status yang sesuai.
11. Admisi meninjau hasil kalkulasi.
12. Admisi menerbitkan dokumen prakiraan biaya.
13. Dokumen dapat dicetak dan dijelaskan kepada pasien atau keluarga.
14. Snapshot dokumen tersimpan sebagai riwayat.
15. Jika rencana berubah, sistem membuat versi prakiraan baru.

## 4.2 Hubungan dengan proses Admisi

Urutan proses yang direkomendasikan:

**Permintaan Ranap Disahkan → Verifikasi Admisi → Tentukan Kelas/Tarif → Buat Prakiraan Biaya → Penjelasan kepada Pasien → Alokasi/Penerimaan sesuai workflow Admisi.**

Alokasi kamar boleh dilakukan sebelum prakiraan jika proses operasional membutuhkannya.

Pembuatan prakiraan tidak perlu menunggu pasien resmi menempati bed, selama informasi tarif kelas dan rencana perawatan sudah memadai.

## 4.3 Hubungan dengan Billing

Prakiraan dan Billing merupakan dua proses berbeda.

| Prakiraan Biaya | Billing Aktual |
|---|---|
| Dibuat sebelum pelayanan | Dibuat mengikuti pelayanan yang dapat ditagihkan |
| Menggunakan perkiraan kuantitas | Menggunakan kuantitas aktual |
| Tidak membuat tagihan | Menciptakan atau memperbarui tagihan |
| Nilai dapat berubah | Mengikuti transaksi dan koreksi resmi |
| Untuk informasi pasien | Untuk proses finansial rumah sakit |

Jika pasien sebelumnya mendapat pelayanan IGD atau Poliklinik, transaksi yang sudah terjadi tidak otomatis dijadikan komponen prakiraan Ranap.

Ruang lingkup default prakiraan adalah **biaya pelayanan Rawat Inap yang direncanakan**, bukan seluruh biaya kunjungan asal.

---

# 5. Parameter Prakiraan Biaya

## 5.1 Parameter Dasar

| Parameter | Sumber | Ketentuan |
|---|---|---|
| Identitas pasien | Master Pasien | Otomatis |
| Nomor RM | Master Pasien | Otomatis |
| Permintaan Rawat Inap | Modul IGD / Poliklinik | Referensi wajib untuk alur transfer MVP |
| Diagnosis kerja | Dokter | Read-only pada Admisi |
| Unit perawatan | Permintaan Ranap | Dapat disesuaikan melalui workflow resmi |
| Kelas perawatan | Admisi / hasil alokasi | Wajib untuk estimasi kamar |
| Penjamin | Data administrasi | Menentukan konteks tarif bila berlaku |
| Estimasi lama rawat | Rencana dokter | Dalam hari; tidak boleh diasumsikan tanpa dasar |
| Rencana operasi | Dokter | Opsional |
| Rencana tindakan | Dokter / rencana keperawatan | Opsional |
| Rencana pemeriksaan | Rencana klinis / order | Opsional |
| Rencana terapi | Dokter | Opsional |
| Kebutuhan khusus | Dokter | Opsional |

Estimasi lama rawat atau LOS (*Length of Stay*) harus dapat dikosongkan jika dokter belum dapat memperkirakannya.

Jika LOS tidak tersedia, kalkulasi biaya kamar yang membutuhkan jumlah hari dinyatakan belum lengkap.

---

# 6. Komponen Biaya

## 6.1 Biaya Kamar

**Dasar:** tarif kamar per kelas × estimasi hari rawat yang dapat ditagihkan.

Input:
- Kelas perawatan.
- Tarif kamar yang berlaku.
- Estimasi lama rawat.
- Ketentuan perhitungan hari kamar rumah sakit.

Contoh:

Tarif kamar: Rp750.000/hari  
Estimasi lama rawat: 3 hari  
Prakiraan kamar: Rp2.250.000

Perhitungan harus mengikuti kebijakan rumah sakit mengenai hari pertama, perpindahan kamar, waktu masuk/keluar, dan batas perhitungan harian.

Jangan mengasumsikan tarif ICU dan tarif kamar reguler ditagihkan bersamaan untuk periode yang sama.

## 6.2 Visit Dokter

**Dasar:** tarif visit × perkiraan jumlah visit.

Contoh:

Tarif visit: Rp250.000  
Estimasi: 3 kali  
Prakiraan: Rp750.000

Jumlah visit tidak boleh otomatis dianggap satu kali setiap hari jika aturan pelayanan rumah sakit berbeda.

## 6.3 Tindakan Rawat Inap

Meliputi tindakan medis atau keperawatan non-operatif yang memang memiliki tarif tersendiri.

Contoh tindakan:
- Pemasangan infus.
- Jasa injeksi.
- Perawatan luka.
- Penggantian balutan.
- Nebulisasi.
- Pemasangan kateter.
- Pemasangan NGT.
- Suction.
- Tindakan lain sesuai master tarif.

**Aturan utama:**

Tindakan tidak otomatis dimasukkan hanya karena sering dilakukan di Rawat Inap.

Tindakan harus memiliki dasar berupa rencana perawatan, indikasi klinis, atau asumsi terverifikasi sesuai kebijakan rumah sakit.

Aktivitas seperti tanda vital, observasi rutin, atau pengkajian keperawatan tidak boleh ditagihkan ulang jika sudah termasuk dalam biaya kamar atau jasa perawatan dasar.

## 6.4 Operasi

Biaya operasi dihitung hanya jika operasi sudah direncanakan.

Parameter:
- Jenis operasi.
- Kode tindakan.
- Tarif operasi.
- Kelas dan ketentuan tarif.
- Jumlah tindakan.
- Komponen yang sudah termasuk dalam tarif operasi.

Jika tidak ada rencana operasi:

**Status: Tidak Direncanakan.**

Jangan menampilkan biaya operasi Rp0 seolah-olah tindakan tersebut gratis.

## 6.5 Implant / Alat Khusus

Hanya dihitung ketika terdapat rencana penggunaan implant atau alat khusus.

Jika jenis dan jumlah belum pasti, sistem menandai komponen sebagai belum dapat diperkirakan.

Jika implant sudah termasuk dalam tarif operasi, sistem tidak boleh menghitung biaya yang sama dua kali.

## 6.6 Obat

Biaya obat dapat diperkirakan jika tersedia rencana terapi dengan informasi produk, jumlah, dosis, durasi, atau asumsi penggunaan yang memadai.

Sumber harga adalah master harga obat yang relevan.

Apabila dokter belum menetapkan terapi dan belum tersedia dasar estimasi, sistem menampilkan:

**Belum Dapat Diperkirakan.**

MVP tidak diperbolehkan membuat resep fiktif untuk menghasilkan harga.

## 6.7 Bahan Habis Pakai (BHP)

BHP dihitung berdasarkan perkiraan penggunaan yang berkaitan dengan rencana tindakan.

Jika bahan sudah termasuk tarif tindakan atau operasi, biaya tidak boleh diduplikasi.

## 6.8 Laboratorium

Hanya memasukkan pemeriksaan yang sudah direncanakan atau memiliki dasar perkiraan klinis yang memadai.

Contohnya CBC, elektrolit, fungsi ginjal, atau pemeriksaan lain sesuai indikasi.

Sistem menggunakan harga pemeriksaan dari master tarif Lab.

Pemeriksaan IGD yang sudah dikerjakan tidak otomatis dimasukkan kembali sebagai estimasi Lab Ranap.

## 6.9 Radiologi

Perlakuan sama dengan Laboratorium.

Pemeriksaan yang tidak direncanakan tidak dihitung otomatis.

Jika pemeriksaan diperkirakan diperlukan tetapi jenisnya belum diketahui, tampilkan status belum dapat diperkirakan.

## 6.10 Konsultasi Khusus

Dihitung jika dokter merencanakan konsultasi tambahan kepada dokter spesialis atau layanan konsultasi tertentu.

Formula:

**Tarif konsultasi × perkiraan jumlah konsultasi.**

Jika belum direncanakan, tidak masuk estimasi utama.

## 6.11 ICU / HCU

Biaya ICU atau HCU hanya dimasukkan ke estimasi utama apabila menjadi bagian dari rencana perawatan.

Jika ICU hanya merupakan kemungkinan akibat perubahan kondisi, tampilkan dalam informasi **Potensi Biaya Tambahan**, bukan total estimasi utama.

Jika pasien direncanakan menjalani sebagian hari di ICU dan sebagian di kamar reguler, masing-masing tarif dihitung menurut periode perawatan yang diperkirakan tanpa duplikasi hari yang tidak sah.

---

# 7. Klasifikasi Status Komponen

Setiap komponen prakiraan harus mempunyai status yang jelas.

| Status | Definisi | Masuk Total? |
|---|---|---|
| Diestimasi | Ada dasar perhitungan, tarif dan kuantitas | Ya |
| Tidak Direncanakan | Belum ada rencana menggunakan layanan | Tidak |
| Belum Dapat Diperkirakan | Ada kebutuhan/rencana tetapi data belum cukup | Tidak |
| Potensi Tambahan | Pelayanan mungkin diperlukan kemudian | Tidak |
| Sudah Termasuk | Biaya sudah tercakup pada komponen lain | Tidak dihitung ulang |

**Business Rules:**

BR-001: Jangan menggunakan Rp0 untuk menggantikan nilai yang belum diketahui.

BR-002: Komponen berstatus Tidak Direncanakan tidak dihitung.

BR-003: Komponen berstatus Potensi Tambahan tidak dihitung ke total utama.

BR-004: Komponen Sudah Termasuk tidak boleh dihitung ulang.

BR-005: Komponen yang direncanakan tetapi belum memiliki tarif atau kuantitas harus memunculkan peringatan.

BR-006: Sistem harus membedakan prakiraan lengkap dan prakiraan parsial.

BR-007: Prakiraan parsial tidak boleh ditampilkan sebagai total biaya seluruh perawatan tanpa penjelasan.

BR-008: Pelayanan aktual tetap mengikuti instruksi klinis dan tidak dibatasi oleh isi dokumen prakiraan.

---

# 8. Estimation Engine

## 8.1 Formula Dasar

**Subtotal Komponen = Tarif Satuan × Estimasi Kuantitas**

**Total Prakiraan Utama = Jumlah Subtotal Komponen yang Diestimasi**

Untuk komponen harian:

**Subtotal Harian = Tarif Harian × Jumlah Hari yang Diperkirakan**

Untuk tindakan berulang:

**Subtotal Tindakan = Tarif Tindakan × Estimasi Jumlah Tindakan**

Jika frekuensi dicatat per hari, sistem boleh menghitung kuantitas total dari frekuensi dan estimasi hari.

Jika kuantitas sudah merupakan jumlah total, sistem tidak boleh mengalikan LOS kembali.

## 8.2 Sumber Data Harga

Sistem mengambil tarif dari master data rumah sakit sesuai jenis komponen.

Kategori sumber meliputi:
- Tarif kamar dan kelas perawatan.
- Tarif visit/konsultasi dokter.
- Tarif tindakan.
- Tarif operasi.
- Tarif implant/alat kesehatan.
- Harga obat dan BHP.
- Tarif laboratorium.
- Tarif radiologi.
- Tarif administrasi jika berlaku.

Nama entitas/tabel di atas merupakan kategori konseptual, bukan nama tabel database yang sudah diverifikasi.

Sistem harus memperhatikan masa berlaku tarif, kelas, unit pelayanan, penjamin dan ketentuan harga yang berlaku.

## 8.3 Validasi Harga

- Tarif harus berasal dari master resmi.
- Tarif harus berlaku untuk konteks estimasi.
- Tarif dan kuantitas tidak boleh negatif.
- Kuantitas yang belum diketahui tidak boleh otomatis menjadi nol.
- Kesalahan harga atau tarif tidak tersedia harus ditampilkan kepada petugas.
- Tarif yang dipakai pada dokumen terbit disimpan sebagai snapshot.
- Nilai moneter menggunakan tipe desimal, bukan floating-point.
- Aturan pembulatan mengikuti kebijakan keuangan rumah sakit.

## 8.4 Kelengkapan Estimasi

Sistem mempunyai dua indikator:

**Lengkap:** seluruh komponen dalam rencana yang akan dihitung sudah memiliki data estimasi yang cukup.

**Parsial:** terdapat komponen yang direncanakan tetapi belum dapat dihitung.

Prakiraan parsial boleh disimpan dan ditinjau.

Jika diterbitkan untuk keperluan informasi pasien, dokumen wajib bertuliskan **PRAKIRAAN PARSIAL** dan angka yang tersedia diberi label **Subtotal Komponen Terestimasi**, bukan total seluruh biaya perawatan.

---

# 9. Rancangan Antarmuka (UI/UX)

## 9.1 Lokasi Fitur

**Admisi Rawat Inap → Permintaan Rawat Inap → Detail Permintaan → Prakiraan Biaya**

Akses tambahan dapat disediakan pada detail pasien di menu Daftar Pasien Rawat Inap.

## 9.2 Struktur Halaman

### A. Informasi Pasien

Menampilkan otomatis:
- Nama pasien.
- Nomor RM.
- Nomor permintaan Ranap.
- Asal pelayanan.
- Diagnosis kerja.
- Dokter pengirim.
- DPJP yang direncanakan.
- Penjamin.
- Status permintaan.

### B. Informasi Perawatan

Menampilkan:
- Unit tujuan.
- Kelas perawatan.
- Kamar dan bed bila sudah dialokasikan.
- Estimasi lama rawat.
- Rencana operasi.
- Kebutuhan ICU / HCU / isolasi jika ada.

### C. Daftar Komponen Prakiraan

Tabel utama menggunakan kolom:

| Kolom | Fungsi |
|---|---|
| No | Nomor urut |
| Komponen | Nama layanan |
| Sumber Tarif | Referensi master harga |
| Tarif Satuan | Harga satuan |
| Estimasi Qty | Perkiraan jumlah |
| Satuan | Hari, kali, unit, dan sebagainya |
| Subtotal | Hasil kalkulasi |
| Status | Diestimasi / Tidak Direncanakan / lainnya |

### D. Informasi Potensi Tambahan

Menampilkan komponen yang mungkin dibutuhkan kemudian, seperti ICU atau konsultasi khusus, berdasarkan kondisi dan informasi klinis yang relevan.

Tarif per satuan dapat ditampilkan jika tersedia.

Nilai tersebut tidak ikut dihitung dalam total utama.

### E. Ringkasan

Menampilkan:
- Total komponen yang dapat dihitung.
- Jumlah komponen yang belum dapat diperkirakan.
- Status kelengkapan.
- Tanggal dan versi prakiraan.
- Catatan bahwa prakiraan bukan paket.

### F. Tombol Aksi

- Simpan Draft.
- Hitung Ulang.
- Pratinjau Dokumen.
- Terbitkan Prakiraan.
- Cetak / Unduh PDF.
- Buat Revisi.
- Lihat Riwayat.

Tombol yang mengubah status harus mengikuti hak akses.

---

# 10. Rancangan Dokumen Cetak

## PENJELASAN PRAKIRAAN BIAYA RAWAT INAP

### A. Identitas

Nama Pasien  
Nomor Rekam Medis  
Nomor Permintaan Rawat Inap  
Diagnosis Kerja  
DPJP  
Kelas Perawatan  
Estimasi Lama Rawat  
Tanggal Prakiraan  
Nomor dan Versi Dokumen

### B. Prakiraan Biaya Utama

Contoh simulasi:

| Komponen | Prakiraan |
|---|---:|
| Kamar (3 hari) | Rp2.250.000 |
| Visit Dokter (3 kali) | Rp750.000 |
| Tindakan Rawat Inap | Rp425.000 |
| Obat dan BHP | Rp1.500.000 |
| Laboratorium | Rp500.000 |
| Administrasi | Rp150.000 |
| **Total Prakiraan** | **Rp5.575.000** |

Seluruh angka adalah ilustrasi, bukan tarif resmi rumah sakit.

### C. Komponen Tidak Direncanakan

Operasi: Tidak Direncanakan  
Implant: Tidak Direncanakan  
Radiologi: Belum Direncanakan

### D. Potensi Biaya Tambahan

ICU: Bila Diperlukan  
Konsultasi Spesialis Tambahan: Bila Diperlukan  
Pemeriksaan Tambahan: Mengikuti kebutuhan medis

Potensi biaya tambahan tidak termasuk dalam total prakiraan utama.

### E. Catatan Wajib

**Nilai yang tertera hanya merupakan prakiraan (bukan merupakan paket), tergantung jumlah dan jenis penggunaan obat, pemeriksaan penunjang, derajat kesulitan, konsultasi khusus, serta ruang ICU bila diperlukan.**

Biaya aktual dapat berbeda sesuai pelayanan yang benar-benar diberikan, perubahan kondisi medis, lama rawat, penggunaan alat kesehatan, dan ketentuan tarif rumah sakit.

Nilai prakiraan tidak secara otomatis menunjukkan jumlah yang harus dibayarkan sendiri oleh pasien karena ketentuan penjamin, manfaat asuransi, deposit dan pembayaran sebelumnya dapat berbeda.

### F. Informasi Penjelasan

- Nama petugas yang memberikan penjelasan.
- Tanggal dan waktu penjelasan.
- Nama pasien/keluarga penerima informasi.
- Kolom pengakuan penerimaan informasi atau tanda tangan jika diwajibkan oleh kebijakan rumah sakit.

Dokumen juga menampilkan status dan nomor versi untuk memastikan hasil cetak dapat ditelusuri.

---

# 11. Status dan Revisi

## 11.1 Status Dokumen

| Status | Keterangan |
|---|---|
| Draft | Estimasi sedang disusun |
| Ready | Perhitungan telah ditinjau dan siap diterbitkan |
| Issued | Dokumen telah diterbitkan |
| Superseded | Dokumen digantikan versi revisi |
| Cancelled | Dokumen dibatalkan dengan alasan |

Kelengkapan `Complete` atau `Partial` dicatat terpisah dari status dokumen.

## 11.2 Aturan Revisi

Ketika dokumen sudah terbit:

- Nilai dan komponen pada versi tersebut tidak boleh diubah langsung.
- Perubahan rencana perawatan menghasilkan draft revisi.
- Petugas harus memberikan alasan revisi.
- Setelah revisi diterbitkan, versi sebelumnya berstatus Superseded.
- Seluruh versi masih dapat dilihat untuk audit.
- Nomor revisi harus terlihat pada dokumen cetak.

Contoh:

Estimasi v1: Rp5.575.000  
Rencana operasi ditambahkan.  
Estimasi v2: Rp18.575.000

Estimasi v1 tetap tersimpan sebagai dokumentasi informasi yang sebelumnya diberikan kepada pasien.

---

# 12. Integrasi Sistem

## 12.1 Integrasi Wajib

| Modul | Data yang Dibutuhkan | Arah Integrasi |
|---|---|---|
| Master Pasien | Identitas, No. RM | Read |
| IGD | Permintaan Ranap dan rencana klinis | Read |
| Poliklinik | Permintaan Ranap dan rencana klinis | Read |
| Admisi Ranap | Kelas, penjamin, rencana penempatan | Read |
| Bed Management | Informasi ruang dan bed jika tersedia | Read |
| Master Tarif | Tarif layanan | Read |
| Farmasi | Harga obat/BHP | Read |
| Laboratorium | Tarif pemeriksaan | Read |
| Radiologi | Tarif pemeriksaan | Read |
| Dokter/Tindakan | Tarif visit, tindakan dan operasi | Read |
| Billing | Tidak membuat charge atau invoice | Tidak melakukan write |

## 12.2 Aturan Integrasi

**INT-001:** Prakiraan dibuat berdasarkan satu referensi permintaan Ranap yang sah.

**INT-002:** Sistem harus menghindari prakiraan aktif ganda yang tidak terhubung riwayatnya untuk permintaan yang sama.

**INT-003:** Perubahan rencana klinis setelah dokumen terbit tidak boleh mengubah snapshot lama secara diam-diam.

**INT-004:** Revisi harga master tidak mengubah dokumen yang sudah terbit.

**INT-005:** Perhitungan harus membedakan layanan yang sudah dilakukan pada kunjungan asal dari layanan yang baru direncanakan di Ranap.

**INT-006:** Tidak diperbolehkan membuat data transaksi pelayanan hanya untuk menghitung prakiraan.

**INT-007:** Seluruh integrasi mengikuti sumber data resmi dan kontrol akses yang berlaku.

---

# 13. Rancangan Data Konseptual

Rancangan berikut merupakan kebutuhan data, bukan instruksi membuat tabel baru tanpa memeriksa struktur database eksisting.

## 13.1 InpatientCostEstimate

Header prakiraan biaya.

Field utama:

| Field | Keterangan |
|---|---|
| Id | Identitas prakiraan |
| EstimateNumber | Nomor dokumen |
| AdmissionRequestId | Permintaan Ranap |
| PatientId | Identitas pasien |
| SourceEncounterId | Kunjungan asal |
| SourceUnit | IGD / Poliklinik |
| PlannedWardId | Ruang tujuan |
| BedId | Bed jika sudah dialokasikan |
| CareClassId | Kelas perawatan |
| PayerId | Penjamin |
| EstimatedLOS | Estimasi hari rawat |
| Completeness | Complete / Partial |
| Status | Status dokumen |
| Version | Versi prakiraan |
| TotalEstimated | Jumlah komponen yang dihitung |
| CreatedBy / CreatedAt | Pembuat dan waktu |
| IssuedBy / IssuedAt | Penerbit dan waktu |
| RevisionReason | Alasan revisi |

## 13.2 InpatientCostEstimateItem

Detail komponen prakiraan.

Field utama:

- EstimateId.
- Category.
- ServiceReferenceId.
- ServiceName.
- TariffReferenceId.
- TariffVersion / EffectiveDate.
- UnitPrice.
- EstimatedQuantity.
- Unit.
- Subtotal.
- ComponentStatus.
- EstimateSource.
- CalculationNote.
- IncludedInOtherComponent.

Setiap item harus dapat ditelusuri dari mana tarif dan asumsi kuantitasnya berasal.

## 13.3 Estimate Audit Trail

Mencatat:
- Aktivitas.
- Pengguna.
- Waktu.
- Nilai sebelum/sesudah jika relevan.
- Alasan perubahan.
- Versi dokumen.
- Referensi permintaan Ranap.

Implementasi dapat menggunakan mekanisme audit yang sudah tersedia pada Quilvian.

---

# 14. Kebutuhan Backend dan Frontend

## 14.1 Backend ASP.NET Core

Kemampuan yang diperlukan:

1. Mengambil konteks pasien dan permintaan Ranap.
2. Mengambil tarif master sesuai kelas, penjamin dan masa berlaku.
3. Menghitung komponen yang direncanakan.
4. Memvalidasi kuantitas, kategori, harga, dan kelengkapan data.
5. Memisahkan biaya utama dan potensi tambahan.
6. Menyimpan draft dan snapshot.
7. Mengelola versi revisi.
8. Menyediakan data dokumen cetak.
9. Menegakkan hak akses dan audit trail.
10. Menjaga seluruh proses bebas dari pembuatan Billing aktual.

Contoh rancangan API konseptual:

| Method | Endpoint | Fungsi |
|---|---|---|
| GET | `/inpatient-cost-estimates/context` | Mengambil data dasar estimasi |
| POST | `/inpatient-cost-estimates` | Membuat draft |
| GET | `/inpatient-cost-estimates/{id}` | Detail estimasi |
| PUT | `/inpatient-cost-estimates/{id}` | Perbarui draft |
| POST | `/inpatient-cost-estimates/{id}/calculate` | Hitung ulang |
| POST | `/inpatient-cost-estimates/{id}/issue` | Terbitkan |
| POST | `/inpatient-cost-estimates/{id}/revisions` | Buat revisi |
| GET | `/inpatient-cost-estimates/{id}/print` | Data/hasil cetak |

Endpoint bukan kontrak final. Nama dan struktur harus disesuaikan dengan arsitektur yang sudah ada.

## 14.2 Frontend Next.js

Halaman perlu memiliki:

- Informasi pasien otomatis.
- Informasi rencana Rawat Inap.
- Pemilihan tarif layanan melalui komponen master-data select.
- Input kuantitas perkiraan.
- Perhitungan subtotal.
- Ringkasan utama dan potensi tambahan.
- Penanda status kelengkapan.
- Validasi field.
- Preview dokumen.
- Aksi cetak.
- Riwayat revisi.

Gunakan pola komponen, form, hooks, autentikasi, dan tampilan yang sudah distandarkan dalam Quilvian. Hindari membuat form yang berbeda secara visual maupun perilaku dari halaman Admisi lain tanpa kebutuhan khusus.

---

# 15. Acceptance Criteria dan UAT MVP

| ID | Test Case | Expected Result |
|---|---|---|
| UAT-001 | Permintaan Ranap IGD telah disahkan | Admisi dapat membuat prakiraan |
| UAT-002 | Permintaan Ranap Poliklinik telah disahkan | Admisi dapat membuat prakiraan |
| UAT-003 | Kelas dan LOS tersedia | Biaya kamar dihitung berdasarkan tarif |
| UAT-004 | Visit dokter direncanakan | Estimasi visit terhitung sesuai jumlah |
| UAT-005 | Tidak ada rencana operasi | Operasi berstatus Tidak Direncanakan |
| UAT-006 | Ada rencana operasi | Sistem mengambil tarif operasi |
| UAT-007 | Tidak ada rencana implant | Implant tidak dihitung |
| UAT-008 | Tidak ada rencana radiologi | Radiologi tidak dihitung otomatis |
| UAT-009 | Obat direncanakan dan kuantitas diketahui | Sistem menghitung dari harga master |
| UAT-010 | Obat direncanakan tetapi belum dapat dihitung | Ditandai Belum Dapat Diperkirakan |
| UAT-011 | ICU hanya kemungkinan | Tarif ICU tidak masuk total utama |
| UAT-012 | ICU direncanakan | Biaya ICU dihitung sesuai rencana tanpa duplikasi kamar |
| UAT-013 | Tindakan sudah termasuk tarif lain | Biaya tidak dihitung ulang |
| UAT-014 | Tarif master tidak ditemukan | Sistem menampilkan error/ketidaklengkapan |
| UAT-015 | LOS belum tersedia | Estimasi kamar tidak dikarang |
| UAT-016 | Ada komponen direncanakan tetapi belum terhitung | Dokumen ditandai Parsial |
| UAT-017 | Estimasi dihitung ulang | Subtotal dan ringkasan konsisten |
| UAT-018 | Prakiraan diterbitkan | Snapshot dan versi tersimpan |
| UAT-019 | Dokumen diterbitkan lalu tarif berubah | Snapshot lama tetap tidak berubah |
| UAT-020 | Rencana perawatan berubah | Sistem dapat membuat revisi |
| UAT-021 | Dokumen dicetak | Identitas, rincian, versi, status, dan catatan muncul |
| UAT-022 | Prakiraan dibuat tanpa pelayanan aktual | Tidak tercipta charge, resep, order, invoice, atau settlement |
| UAT-023 | Petugas tanpa kewenangan mencoba mengubah estimasi | Aksi ditolak |
| UAT-024 | Pelayanan IGD sudah ditagihkan | Tidak otomatis terhitung kembali sebagai layanan Ranap |

## Kriteria Kelulusan MVP

MVP layak dinyatakan selesai jika:

- Seluruh test case kritis lulus.
- Seluruh perhitungan menggunakan sumber tarif yang sah.
- Tidak terjadi duplikasi biaya yang diketahui.
- Tidak ada transaksi Billing yang dibuat oleh proses prakiraan.
- Komponen yang belum diketahui tidak direpresentasikan sebagai Rp0.
- Estimasi parsial ditandai secara jelas.
- Snapshot dan audit riwayat berfungsi.
- Dokumen cetak sesuai data tersimpan.
- Hak akses pengguna berjalan sesuai ketentuan.

---

# 16. Rencana Implementasi MVP

## Tahap 0 — Audit Kesiapan

Sebelum coding:

- Audit fitur atau service prakiraan Ranap yang sudah tersedia.
- Audit master tarif kamar, tindakan, dokter, farmasi, lab, dan radiologi.
- Audit sumber data permintaan Rawat Inap.
- Audit relasi episode, kunjungan, dan penjamin.
- Identifikasi tarif yang sudah termasuk jasa atau alat tertentu.
- Identifikasi ketentuan perhitungan hari kamar.

**Output:** Gap Analysis dan keputusan reuse komponen existing.

## Tahap 1 — Core Estimation Engine

Implementasi:
- Data prakiraan.
- Integrasi master tarif.
- Perhitungan komponen.
- Validasi.
- Status kelengkapan.
- Snapshot.
- Unit test.

**Output:** Estimation engine yang dapat diuji tanpa UI.

## Tahap 2 — Integrasi Admisi dan UI

Implementasi:
- Akses prakiraan dari Permintaan Ranap.
- Form input komponen.
- Perhitungan otomatis.
- Preview.
- Status dan revisi.
- Hak akses.

**Output:** Petugas Admisi dapat menyusun prakiraan sampai siap diterbitkan.

## Tahap 3 — Dokumen dan UAT

Implementasi:
- Template dokumen cetak.
- Nomor dan versi dokumen.
- Keterangan bukan paket.
- Riwayat penerbitan.
- UAT IGD dan Poliklinik.
- Regression test untuk Billing dan Admisi.

**Output:** MVP siap digunakan sesuai hasil UAT dan persetujuan rumah sakit.

---

# 17. Pengembangan Setelah MVP

## P1 — Clinical Pathway Estimation

Sistem menyediakan template rencana penggunaan layanan berdasarkan clinical pathway yang disahkan rumah sakit.

Contohnya:
- Jenis tindakan yang direncanakan.
- Pemeriksaan awal.
- Rencana obat/BHP.
- Estimasi LOS.

Template merupakan alat bantu, bukan instruksi klinis otomatis untuk setiap pasien.

## P1 — Historical Cost Estimation

Menggunakan riwayat episode pasien dengan kasus, tindakan, dan kelas perawatan yang sebanding.

Hasil dapat berupa:
- Median LOS.
- Median biaya per kategori.
- Rentang biaya berdasarkan distribusi historis.
- Informasi jumlah sampel.

Metode dan pembaruan tarif harus ditentukan agar data historis tidak disalahartikan sebagai harga yang berlaku saat ini.

## P1 — Range Estimation

Menampilkan rentang estimasi minimum–maksimum hanya jika terdapat dasar kuantitas, tarif, atau data historis yang memadai.

Rentang tidak boleh dibuat dengan persentase tambahan sembarang tanpa kebijakan yang disetujui.

## P2 — Estimasi vs Realisasi

Menampilkan:
- Prakiraan awal.
- Prakiraan revisi terakhir.
- Billing aktual.
- Selisih biaya.
- Komponen yang mengalami perubahan terbesar.

## P2 — Estimasi Deposit dan Penjamin

Menghubungkan prakiraan dengan kebijakan deposit dan coverage.

Perhitungan tanggungan pasien dipisahkan dari total bruto biaya pelayanan.

---

# 18. Keputusan dan Ketergantungan yang Harus Dikonfirmasi

Sebelum implementasi final, Product Owner dan pihak rumah sakit perlu menetapkan:

| ID | Keputusan | Rekomendasi Awal |
|---|---|---|
| DEC-001 | Siapa boleh menerbitkan prakiraan? | Admisi dengan hak akses resmi |
| DEC-002 | Siapa menetapkan estimasi LOS? | Dokter/rencana klinis yang disahkan |
| DEC-003 | Bagaimana perhitungan hari kamar? | Mengikuti ketentuan tarif RS |
| DEC-004 | Apakah biaya administrasi masuk? | Jika memiliki dasar tarif |
| DEC-005 | Apakah tarif berbeda per penjamin? | Ikuti kontrak dan konfigurasi tarif |
| DEC-006 | Apakah pasien harus tanda tangan? | Mengikuti SPO dan kebijakan RS |
| DEC-007 | Apakah estimasi parsial boleh dicetak? | Ya, dengan label dan pengecualian tegas |
| DEC-008 | Apakah wajib sebelum pasien diterima? | Jangan menjadi penghambat pelayanan darurat |
| DEC-009 | Apa yang termasuk tarif kamar? | Wajib dipetakan untuk mencegah duplikasi |
| DEC-010 | Apakah prakiraan otomatis menentukan deposit? | Tidak pada MVP |

---

# 19. Definition of Done

Fitur **Prakiraan Biaya Rawat Inap MVP** dianggap selesai apabila:

- [ ] Dapat diakses dari permintaan IGD dan Poliklinik.
- [ ] Data pasien dan rencana perawatan terhubung.
- [ ] Tarif master digunakan sebagai sumber harga.
- [ ] Perhitungan kamar, dokter, tindakan, operasi, obat, dan penunjang berjalan sesuai dasar yang tersedia.
- [ ] Komponen tidak direncanakan tidak otomatis dihitung.
- [ ] Komponen tidak diketahui ditandai secara benar.
- [ ] Potensi biaya tambahan dipisahkan.
- [ ] Tidak terjadi penghitungan ganda.
- [ ] Prakiraan lengkap dan parsial dibedakan.
- [ ] Dokumen dapat diterbitkan dan dicetak.
- [ ] Riwayat revisi tersimpan.
- [ ] Hak akses dan audit trail diuji.
- [ ] Tidak ada efek samping pada transaksi Billing aktual.
- [ ] UAT dan regression test lulus.
- [ ] Keputusan kebijakan rumah sakit yang masih terbuka telah dikonfirmasi.

---

# 20. Kesimpulan Produk

**Prakiraan Biaya Rawat Inap Quilvian merupakan fitur estimasi finansial pra-pelayanan, bukan modul transaksi Billing dan bukan katalog paket tindakan.**

MVP berfokus pada kemampuan menghitung biaya yang memiliki dasar rencana klinis dan tarif master, memisahkan ketidakpastian secara transparan, serta menghasilkan dokumen yang dapat dijelaskan kepada pasien.

Sistem tidak harus mengetahui seluruh layanan yang akan dilakukan pasien sebelum masuk Ranap. Sistem harus mampu menunjukkan dengan jelas **mana yang sudah diperkirakan, mana yang belum direncanakan, mana yang belum dapat dihitung, dan mana yang hanya berpotensi menjadi biaya tambahan**.

Dengan prinsip ini, prakiraan biaya dapat digunakan sejak proses Admisi tanpa merusak integritas rekam medis maupun Billing aktual.