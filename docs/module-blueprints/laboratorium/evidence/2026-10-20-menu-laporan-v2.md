# `LAB-EVD-014` — Artifact Menu Laporan Laboratorium v2 (2026-10-20)

| Field | Nilai |
|---|---|
| Evidence ID | `LAB-EVD-014` |
| Berkas | `Artifact_Laboratorium_Menu_Laporan_v2.md` — *Module Artifact - Laboratorium - Menu Laporan v2* |
| Diserahkan | Pemilik modul, 2026-10-20, dilampirkan langsung pada sesi `grill-me` |
| Penulis artifact | **Belum disebutkan** |
| Kedudukan | **Bukti untuk direkonsiliasi butir per butir, bukan baseline baru** (`LAB-DEC-230`) |
| Dibaca pada | Backend `4d3e4f98`, frontend `7fb1e1d3e`; FE v1 `C:\Users\Admin\QuilvianSystemFrontendDev` |
| Melahirkan | Amendment pass putaran 30; `LAB-DEC-230`; `LAB-CONFLICT-020`..`LAB-CONFLICT-023`; fakta `F30-1`..`F30-13` |

## 1. Cara membaca berkas ini

Berkas ini hanya **menyimpan artifact apa adanya** supaya bukti tidak hilang (pelajaran `LAB-EVD-002`).
Rekonsiliasinya — setiap klaim artifact disandingkan dengan keputusan dan source yang berlaku — ditulis pada
decision log bagian *Amendment Pass Putaran 30*.

Artifact ini **tidak** membuka izin membangun. Sesuai `LAB-DEC-181`, satu laporan baru menjadi task BE/FE
hanya sesudah kesebelas butir definisinya diserahkan pemilik modul.

## 2. Isi artifact (salinan apa adanya)

````markdown
# Module Artifact - Laboratorium - Menu Laporan v2

## 1. Ringkasan Modul

Menu **Laporan** pada modul Laboratorium menyediakan laporan operasional, pelayanan, finansial, rujukan, spesimen, rekonsiliasi, dan audit penghapusan order/parameter.

Alur umum:
1. User memilih periode laporan.
2. User menentukan filter.
3. Sistem menampilkan data dalam tabel.
4. User dapat export Excel/XLS.
5. User dapat melakukan print pada laporan tertentu.

---

# 2. Actor / User Role

## Petugas Laboratorium
Hak akses:
- Membuka laporan laboratorium.
- Filter laporan.
- Export Excel/XLS.
- Print laporan.
- Monitoring status pemeriksaan.
- Melihat rekonsiliasi.
- Melihat audit delete.

## Dokter Laboratorium
Hak akses:
- Melihat laporan pemeriksaan.
- Melihat laporan pasien per pemeriksaan.
- Melihat laporan Group Diseases.
- Monitoring pelayanan.

---

# 3. Master Data Laboratorium

## 3.1 Kategori Pemeriksaan

Menggunakan master Parameter Group Laboratorium.

Kategori:
- Hematologi
- Hemostasis
- Tes Hemolitik
- Analisa HB
- Hematologi Lain-Lain
- Kimia Klinik
- Elektrolit
- Imunologi
- Serologi
- Hormon
- Urinalisa
- Mikrobiologi
- Parasitologi
- Bakteriologi
- Mikologi
- Virologi
- Sitopatologi
- Histopatologi
- Imunohistokimia
- Molekular
- Genetik
- Toksikologi
- Bank Darah
- Pemeriksaan Khusus
- Panel Laboratorium
- Kultur
- PCR
- Cytology
- Anatomi Patologi
- Lain-Lain

---

# 4. Business Rules Laporan

## Jumlah Tes

Jumlah qty pemeriksaan berdasarkan kategori pemeriksaan.

Formula:

Jumlah Tes = SUM(Qty Pemeriksaan)

## Jumlah Semua Tes

Jumlah seluruh pemeriksaan berdasarkan jenis kunjungan pada halaman datatable aktif.

## Total

Jumlah seluruh pemeriksaan berdasarkan seluruh data hasil filter tanpa batas pagination.

## Harga

Harga satuan pemeriksaan.

## Jumlah Rupiah

Jumlah Rupiah = Harga Pemeriksaan x Qty

## Total Uang

Total Uang = Harga Pemeriksaan x Total Qty

## Tagihan

Nilai pemeriksaan pasien yang dilakukan pada instansi rujukan dan ditagihkan kepada rumah sakit/laboratorium.

## Jumlah

Jumlah qty pemeriksaan pada setiap unit layanan.

## Sub Total

Jumlah qty pemeriksaan berdasarkan kategori pemeriksaan.

---

# 5. Statistik Pesanan Laboratorium

## Unit / Ruang

Sumber unit:
- Rawat Inap
- Rawat Jalan
- IGD
- Laboratorium via kiosk
- Kamar perawatan
- Unit pelayanan

## Jenis Laboratorium

- Patologi Klinik
- Patologi Anatomi
- Mikrobiologi

## LABOR

LABOR merupakan ruang bersalin.

---

# 6. Rekonsiliasi Laboratorium

## Definisi

Rekonsiliasi digunakan untuk mencocokkan:
- Order pemeriksaan.
- Hasil pemeriksaan.
- Billing pemeriksaan.
- Status pembayaran.
- Status penyelesaian layanan.

## Status Rekonsiliasi

### Belum Selesai
Hasil pemeriksaan belum tersedia.

### Selesai
Hasil pemeriksaan telah tersedia.

### Sesuai
Order, hasil, dan billing sudah cocok.

### Tidak Sesuai
Terdapat perbedaan antara order, hasil, atau billing.

---

# 7. CITO

## Definisi

CITO adalah penanda pemeriksaan urgent/prioritas.

Penentuan dilakukan saat unit layanan/petugas laboratorium membuat order pemeriksaan.

Dampak tampilan:
- Row berwarna merah.
- Font putih.
- Bold.

---

# 8. LAB REPORT PER GROUP DISEASES

Tujuan:
Menyediakan laporan laboratorium berdasarkan kelompok penyakit.

Dasar pengelompokan:
- Diagnosis pasien.
- ICD Diagnosis.
- Kelompok penyakit.

Contoh:
- Cardiovascular Disease
- Infectious Disease
- Diabetes Mellitus
- Kidney Disease
- Liver Disease
- Hematology Disease

Output:
- Kelompok penyakit.
- Jumlah pasien.
- Jumlah pemeriksaan.
- Jenis pemeriksaan.
- Nilai pemeriksaan.

---

# 9. Rekap Pasien Perusahaan

## Per Registrasi

Menampilkan data berdasarkan tanggal registrasi pasien.

## Per Payment

Menampilkan data berdasarkan tanggal pembayaran pasien.

---

# 10. Laporan Pasien Per Pemeriksaan

## Status

Status menunjukkan kondisi verifikasi order berdasarkan nomor order pemeriksaan.

Contoh:
- Confirm
- Belum Confirm

## Status Billing

Menunjukkan kondisi pembayaran:
- Sudah Bayar
- Belum Bayar

## Share Dokter

Pembagian fee dokter berdasarkan harga pemeriksaan.

Formula:

Share Dokter = Harga Pemeriksaan x Persentase Fee Dokter

---

# 11. Audit Delete Order & Parameter

Nama Parameter merupakan tautan menuju detail.

Detail:
- Informasi parameter.
- Detail pemeriksaan.
- Riwayat perubahan.

---

# 12. Export dan Print

Output:
- Excel/XLS
- Print
- Print Preview

Export mengikuti filter aktif dan data laporan.

---

# 13. Validation Rules

| Rule | Deskripsi |
|---|---|
| RPT-001 | Semua laporan menggunakan filter periode |
| RPT-002 | Export mengikuti filter aktif |
| RPT-003 | Pagination tidak membatasi total perhitungan |
| RPT-004 | CITO terlihat jelas pada laporan |
| RPT-005 | Order rujukan tanpa tujuan memiliki status Rujukan Belum Dipilih |
| RPT-006 | Pemeriksaan tanpa hasil memiliki status Belum Selesai |
| RPT-007 | Audit delete menyimpan user, waktu, alasan |
| RPT-008 | Rekonsiliasi membandingkan order, hasil, billing |

---

# 14. Pending Decision

1. Mapping ICD Diagnosis ke Group Diseases menggunakan master diagnosis rumah sakit.
2. Persentase Share Dokter mengikuti konfigurasi fee.
3. Detail lifecycle rekonsiliasi mengikuti workflow billing.
4. Hak akses mengikuti permission role aplikasi.
````
