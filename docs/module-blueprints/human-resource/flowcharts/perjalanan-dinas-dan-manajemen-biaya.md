# Alur Proses Bisnis: Perjalanan Dinas & Manajemen Biaya Operasional (Business Travel & Expense Management — Slice `S-D5`)

Dokumen ini mendefinisikan alur proses bisnis ujung-ke-ujung untuk **Sub-Modul 13: Perjalanan Dinas & Manajemen Biaya Operasional — Slice `S-D5`**.

---

## 1. Alur Utama Pengajuan, Persetujuan SPPD & Eksekusi Perjalanan Dinas

```mermaid
flowchart TD
    A([Pegawai Membuka Portal ESS]) --> B[Pilih Menu Permohonan SPPD /hrd/travel/requests/create]
    B --> C[Isi Rute, Kota Tujuan, Tanggal & Agenda Dinas]
    C --> D[Sistem Hitung Otomatis Lumpsum Uang Harian Berbasis Matriks Zonasi & Grade]
    D --> E[Input Estimasi Tiket & Hotel Sesuai Plafon]
    E --> F{Ajukan Uang Muka Kerja?}
    
    F -->|Ya| G[Tentukan Nominal Uang Muka Maksimal 80% Pagu]
    F -->|Tidak| H[Kirim Pengajuan SPPD Langsung]
    G --> H
    
    H --> I[Pemeriksaan Otomatis: Ketersediaan Anggaran & Bentrok Cuti/Roster]
    I -->|Ada Konflik| J[Tolak Pengajuan & Tampilkan Peringatan]
    I -->|Lolos Validasi| K[Alur Persetujuan Bertingkat: Atasan, HRD & Direksi]
    
    K --> L{Keputusan Persetujuan SPPD}
    L -->|Ditolak| M([SPPD Ditolak - Alasan Tertulis Disampaikan])
    L -->|Disetujui| N[Status SPPD: Approved & Terbit Surat Tugas Resmi]
    
    N --> O[Sinyal INT-HR-97: Suntik Presensi Dinas Luar & Gembok Hard-Stop Roster]
    O --> P([Pegawai Melaksanakan Tugas Perjalanan Dinas])
```

---

## 2. Alur Pengajuan, Verifikasi 2-Pintu & Pencairan Uang Muka Kerja (*Travel Advance*)

```mermaid
flowchart TD
    A1([SPPD Disetujui dengan Permohonan Uang Muka]) --> B1[Tiket Uang Muka Masuk ke Antrean Keuangan]
    B1 --> C1[Verifikasi Pintu 1: Atasan Langsung Konfirmasi Urgensi Kas]
    C1 --> D1[Verifikasi Pintu 2: Finance Manager Konfirmasi Ketersediaan Kas RS]
    
    D1 --> E1{Persetujuan Finance}
    E1 -->|Ditolak / Anggaran Kosong| F1[Uang Muka Dibatalkan - Pegawai Menalangi Mandiri]
    E1 -->|Disetujui| G1[Terbit Voucher Pengeluaran Kasir Cito]
    
    G1 --> H1{Metode Pencairan}
    H1 -->|Transfer Bank| I1[Finance Transfer ke Rekening Payroll Staf]
    H1 -->|Uang Tunai| J1[Pegawai Ambil Kas di Loket Kasir RS]
    
    I1 --> K1[Status Advance: Paid]
    J1 --> K1
    K1 --> L1([Dana Siap Digunakan untuk Operasional Dinas])
```

---

## 3. Alur Rekonsiliasi Settlement, Pengembalian Kas & Eskalasi Pemotongan Payroll

```mermaid
flowchart TD
    A2([Pegawai Kembali dari Perjalanan Dinas]) --> B2[Hitung Mundur SLA Penyerahan LPJ: Maksimal 7 Hari Kalender]
    B2 --> C2{Apakah Diserahkan <= 7 Hari?}
    
    C2 -->|Ya| D2[Pegawai Unggah Rincian Biaya Riil, Boarding Pass & Invoice Hotel]
    C2 -->|Tidak - Lewat 7 Hari| E2[Worker TRV-W1 Kirim Peringatan Darurat H+3 & H+5]
    
    E2 --> F2{Apakah Melewati 14 Hari?}
    F2 -->|Ya| G2[Sistem Bekukan Pembuatan SPPD Baru Pegawai]
    F2 -->|Tidak| D2
    
    G2 --> H2{Apakah Menunggak > 30 Hari?}
    H2 -->|Ya| I2[Worker TRV-W2 Kirim Sisa Uang Muka ke Modul Payroll Sub-10]
    I2 --> J2[Gaji Bulan Berjalan Dipotong Otomatis Maks 50% Upah]
    
    D2 --> K2[Verifikator Keuangan Audit Fisik Dokumen Kuitansi]
    K2 --> L2{Hasil Komparasi Uang Muka vs Aktual}
    
    L2 -->|Aktual > Uang Muka - Kurang Bayar| M2[Kasir RS Bayar Selisih Kurang ke Pegawai dalam 3 Hari]
    L2 -->|Uang Muka > Aktual - Lebih Bayar| N2[Pegawai Wajib Setor Balik Sisa Kas ke Kasir dalam 3 Hari]
    L2 -->|Aktual == Uang Muka - Seimbang| O2[Status Settlement: Balanced]
    
    M2 --> P2[Status Settlement: Settled & SPPD Ditutup Selesai]
    N2 --> P2
    O2 --> P2
    J2 --> P2
```

---

## 4. Alur Klaim Pengeluaran Operasional Non-Trip (*Direct Expense Claim*)

```mermaid
flowchart TD
    A3([Staf Mengeluarkan Biaya Operasional Lokal Cito]) --> B3[Simpan Struk / Kuitansi Toko Asli]
    B3 --> C3[Buka Portal ESS Menu /hrd/travel/expenses]
    C3 --> D3[Input Kategori Pos Beban, Tanggal Pembelian, Nominal & Foto Struk]
    D3 --> E3[Kirim Pengajuan Klaim Non-Trip]
    
    E3 --> F3[Persetujuan Kepala Unit Kerja / Manajer Ruangan]
    F3 --> G3{Keputusan Atasan}
    G3 -->|Ditolak| H3([Klaim Ditolak - Alasan Disampaikan])
    G3 -->|Disetujui| I3[Verifikasi Kelayakan Struk oleh Tim Finance RS]
    
    I3 --> J3{Pemeriksaan Fisik Bukti Kasir}
    J3 -->|Struk Tidak Sah / Buram| K3[Kembalikan untuk Perbaikan / Revisi]
    J3 -->|Lolos Verifikasi| L3[Kasir RS Cairkan Dana Reimbursement Tunai / Transfer]
    
    L3 --> M3([Klaim Selesai & Dicatat pada Buku Kasir])
```

---

## 5. Alur Integrasi Presensi Dinas Luar & Gembok Hard-Stop Roster Medis

```mermaid
flowchart TD
    A4([SPPD Nakes Dinyatakan Disetujui Final]) --> B4[Event Publisher Terbitkan Hrd.BusinessTravel.TravelApproved]
    
    B4 --> C4[Event Consumer Sub-07 Presensi & Kehadiran]
    B4 --> D4[Event Consumer Sub-06 Penjadwalan Roster Medis]
    
    C4 --> E4[Suntik Log Presensi Otomatis: Hadir Dinas Luar pada Tanggal SPPD]
    E4 --> F4[Tunjangan Hadir Tetap Dihitung Sah & Bebas Penalti Alpa]
    
    D4 --> G4[Pasang Status Gembok Roster Restriction Lock pada ID Pegawai]
    G4 --> H4{Kepala Ruangan Coba Masukkan Nama ke Shift Jaga Bangsal?}
    H4 -->|Mencoba Menjadwalkan| I4[Sistem Hard-Stop: Blokir Jadwal & Tampilkan Notifikasi Dinas Luar]
    H4 -->|Normal| J4[Pilih Staf Pengganti / Buddy Nurse]
    
    I4 --> J4
    J4 --> K4([Rasio Ketenagaan Bangsal Rawat Inap Aman & Pasien Terlindungi])
```
