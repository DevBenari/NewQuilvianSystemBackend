# USER GUIDELINE UAT

# MODUL BANK DARAH

Versi: UAT v1.0

## 1. Tujuan Modul

Modul Bank Darah digunakan untuk mengelola proses:

Permintaan Darah\
↓\
Penerimaan Darah\
↓\
Penyimpanan Kantong\
↓\
Alokasi Kantong\
↓\
Pemeriksaan Golongan Darah\
↓\
Bukti Kecocokan\
↓\
Pemberian Darah\
↓\
Jalur Darurat\
↓\
Koreksi / Penyelesaian

------------------------------------------------------------------------

# 2. Role Pengujian

  Role                 Fungsi
  -------------------- ------------------------------------------
  Petugas Bank Darah   Input order, penyimpanan, proses kantong
  Petugas Pemeriksa    Pemeriksaan golongan darah
  Petugas Pemberian    Alokasi dan pemberian darah
  Supervisor           Approval koreksi
  Tester Admin         Validasi permission

------------------------------------------------------------------------

# 3. Persiapan Data

Pastikan tersedia:

-   Unit Pelayanan
-   Komponen Darah
-   Lokasi Penyimpanan
-   Alasan Pembatalan
-   Alasan Emergency
-   Dokter Peminta
-   Provider PMI

------------------------------------------------------------------------

# 4. Skenario UAT End-to-End

## BD-UAT-001 Membuat Order Darah

Menu:

Bank Darah → Order Darah → Tambah Order

Input contoh:

  Field            Nilai
  ---------------- ------------
  Sumber Order     Elektronik
  Golongan Darah   A+
  Pasien           Pasien UAT
  Unit Pelayanan   ICU
  Dokter Peminta   Dokter UAT
  Komponen         PRC
  Jumlah           2

Expected:

-   Order berhasil dibuat
-   Status Requested

Negative: - Jumlah 0 harus ditolak - Unit tidak berwenang harus ditolak

------------------------------------------------------------------------

## BD-UAT-002 Provider Request PMI

Flow:

Order Darah → Provider Request → Penerimaan PMI

Expected:

Status berubah sesuai proses penerimaan.

------------------------------------------------------------------------

## BD-UAT-003 Penerimaan Kantong Darah

Input:

-   Nomor Kantong PMI
-   Komponen
-   Golongan Darah
-   Expired Date

Expected:

Status kantong Received.

------------------------------------------------------------------------

## BD-UAT-004 Penyimpanan Kantong

Action:

Kantong Darah → Simpan Lokasi

Expected:

-   Status Stored
-   Lokasi tersimpan

Negative:

Lokasi nonaktif tidak dapat digunakan.

------------------------------------------------------------------------

## BD-UAT-005 Alokasi Kantong

Flow:

Order Darah → Pilih Kantong → Allocate

Expected:

Status Allocated.

Negative:

Kantong yang sudah dialokasi tidak dapat dipakai ulang.

------------------------------------------------------------------------

## BD-UAT-006 Pemeriksaan Golongan Darah

Flow:

Detail Kantong → Periksa Golongan Darah

Tahap:

1.  Catat Sampel
2.  Catat Hasil ABO/Rhesus
3.  Validasi

Expected:

Status Validated.

Negative:

Identifier sampel duplikat harus ditolak.

------------------------------------------------------------------------

## BD-UAT-007 Bukti Kecocokan

Flow:

Allocated → Catat Bukti Kecocokan

Expected:

Compatibility Evidence valid.

------------------------------------------------------------------------

## BD-UAT-008 Pemberian Darah Normal

Prasyarat:

-   Kantong Allocated
-   Golongan darah valid
-   Bukti kecocokan tersedia

Expected:

Status Issued.

------------------------------------------------------------------------

## BD-UAT-009 Konflik Golongan Darah

Skenario:

Pasien A+\
Hasil pemeriksaan B+

Expected:

-   Sistem menampilkan konflik
-   Pemberian diblokir
-   Emergency Issue juga diblokir

------------------------------------------------------------------------

## BD-UAT-010 Jalur Darurat

Input:

-   Alasan Emergency
-   Otorisasi
-   Catatan

Expected:

Emergency authorization tersimpan.

Negative:

Tanpa alasan harus ditolak.

------------------------------------------------------------------------

## BD-UAT-011 Pending Review

Action:

-   Alihkan
-   Kembalikan PMI
-   Tidak Layak

Expected:

Status berubah sesuai keputusan.

------------------------------------------------------------------------

## BD-UAT-012 Koreksi Pemberian Darah

User A:

Ajukan Koreksi

User B:

Approve / Reject

Expected:

-   Approval berhasil
-   Reject membutuhkan alasan
-   User yang mengajukan tidak dapat approve sendiri

------------------------------------------------------------------------

# 5. Permission Testing

Pastikan:

  Action               Expected
  -------------------- --------------------------------------
  Create Order         Button tidak muncul tanpa permission
  Issue                Button tidak muncul tanpa permission
  Emergency Issue      Button tidak muncul tanpa permission
  Validate             Button tidak muncul tanpa permission
  Approve Correction   Button tidak muncul tanpa permission

------------------------------------------------------------------------

# 6. Error Handling

Pastikan:

-   Button loading saat submit
-   Tidak terjadi double submit
-   Pesan error menggunakan bahasa bisnis

Contoh:

Benar: "Golongan darah pasien bertentangan. Pemberian tidak dapat
dilakukan."

Bukan: "Request failed 422"

------------------------------------------------------------------------

# 7. Template Bukti Testing

Test ID:

Menu:

User:

Tanggal:

Scenario:

Step: 1. 2. 3.

Expected Result:

Actual Result:

Status: PASS / FAIL

Screenshot:

------------------------------------------------------------------------

# 8. Known Limitation

Tidak dianggap bug:

1.  Histori lokasi belum menampilkan nama pelaku jika backend belum
    menyediakan nama user.

2.  Masa berlaku evidence mengikuti validasi backend.

3.  Konflik golongan darah merupakan kondisi klinis, bukan error sistem.

------------------------------------------------------------------------

# 9. Final UAT Checklist

-   [ ] Order Darah berhasil dibuat
-   [ ] Provider Request berjalan
-   [ ] Kantong diterima
-   [ ] Lokasi tersimpan
-   [ ] Kantong dialokasi
-   [ ] Pemeriksaan golongan darah selesai
-   [ ] Bukti kecocokan valid
-   [ ] Darah berhasil diberikan
-   [ ] Emergency flow diuji
-   [ ] Konflik golongan darah diblokir
-   [ ] Pending Review diuji
-   [ ] Correction approval diuji
-   [ ] Permission diuji
