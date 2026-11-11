# Laporan Penyelesaian Backend Task: BE-HRD-194

## 1. Identitas Task
* **ID Task**: `BE-HRD-194`
* **Nama Task**: Mesin Penomoran Terpusat Gapless Berbasis Atomic Row-Locking (`DocumentAutoNumberingService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 13 — HR Service Management & Request
* **Slice / Milestone**: `Slice S-D4` · `Fase MVP-39: Katalog, Template Token Dinamis & Agregat Tiket Helpdesk`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Surat resmi yang dikeluarkan oleh manajemen rumah sakit (seperti Surat Keterangan Kerja, Surat Keterangan Gaji untuk KPR bank, dan Surat Rekomendasi SIP dokter) merupakan naskah dinas hukum. Sesuai tata kelola administrasi legal (`HRS-B3`, `VR-HRS-10`), penomoran surat resmi wajib:
1. **Berurutan Tanpa Celah (*Gapless*)**: Tidak boleh ada nomor yang terlewati atau hilang dari register naskah dinas resmi rumah sakit.
2. **Tidak Membakar Nomor (*No Sequence Burn*)**: Draf surat yang dibatalkan tidak boleh menghanguskan nomor urut. Nomor hanya dialokasikan secara atomik tepat pada saat surat final disetujui / diterbitkan.
3. **Aman dari Akses Bersamaan (*Concurrency Safe*)**: Jika dua analis HR menerbitkan surat secara bersamaan dalam detik yang sama, sistem tidak boleh menghasilkan nomor duplikat (*race condition*).

Fitur-fitur utama yang dibangun dalam task ini:
1. **Row-Level Locking PostgreSQL (`SELECT ... FOR UPDATE`)**:
   - Mesin penomoran mengeksekusi kunci baris `FOR UPDATE` dalam transaksi `Serializable` pada baris `MstEmployeeDocumentType`.
   - Thread kedua akan menunggu hingga transaksi thread pertama selesai, memastikan urutan nomor mutlak unik.
2. **Format Standar Penomoran Resmi Naskah Dinas**:
   - Mendukung format template: `[KODE]/RSQ-SDM/[BULAN_ROMAWI]/[TAHUN]/[NO_URUT]`.
   - Mengubah bulan numerik menjadi angka Romawi secara dinamis (misal bulan 11 menjadi `XI`).
   - Format 5 digit berurutan: `00001`, `00002`, dst.
3. **Otomatisasi Reset Pergantian Tahun Kalender**:
   - Mendeteksi pergantian tahun. Jika tahun dokumen tidak sama dengan tahun kalender berjalan, nomor urut otomatis di-reset kembali ke `1` untuk tahun baru.
4. **Penomoran Tiket Helpdesk SDM (`TKT-YYYYMM-XXXXX`)**:
   - Menyediakan generator nomor tiket unik bulanan anti-tabrakan untuk meja bantuan helpdesk SDM.

---

## 3. Komponen Teknis yang Dibuat & Dimodifikasi

### A. Antarmuka Layanan Penomoran
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/IDocumentAutoNumberingService.cs`
* Metode `GenerateOfficialDocumentNumberAsync`: Mengalokasikan nomor surat resmi berurutan.
* Metode `GenerateTicketNumberAsync`: Mengalokasikan nomor tiket bantuan ESS.

### B. Implementasi Layanan Mesin Penomoran
File: `Areas/Corporate/HumanResource/HrServiceManagement/Services/DocumentAutoNumberingService.cs`
* Menggunakan `ExecutionStrategy` Entity Framework Core untuk mendukung retry jika terjadi deadlock konkurensi.
* Array konversi bulan Romawi: `{"", "I", "II", "III", "IV", "V", "VI", "VII", "VIII", "IX", "X", "XI", "XII"}`.
* Pencatatan log penerbitan nomor resmi `[NUMBERING-HRS-01]`.

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario Penerbitan Surat Keterangan Kerja Dokter Spesialis Jantung**:
  - Dokter Spesialis Jantung dr. Hendra mengajukan Surat Keterangan Kerja Aktif untuk keperluan pengurusan visa luar negeri.
  - Setelah verifikasi HR selesai, sistem memanggil `GenerateOfficialDocumentNumberAsync(docTypeId)`.
  - Sistem mengunci baris template `SKK`, mendeteksi `CurrentYear = 2026`, membaca `LastSequenceNumber = 48`.
  - Sistem menaikkan nomor urut menjadi `49`, memformat bulan November menjadi `XI`, dan menghasilkan nomor surat:
    `SKK/RSQ-SDM/XI/2026/00049`
  - Transaksi di-commit dan nomor surat disematkan secara permanen pada naskah dinas PDF dr. Hendra.

---

## 5. Verifikasi & Integritas Bukti
* **Integritas Atomik**: Menggunakan transaksi terisolasi dan row locking untuk mencegah lompatan atau duplikasi nomor.
* **Kepatuhan Acceptance Test**: Memenuhi kriteria penerimaan `AT-HRD-HRS-01` dan aturan bisnis `HRS-B3`.