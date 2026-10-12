# Alur Pendaftaran Pasien Rujukan (Amendment PM-B, `approved` 2026-10-08)

Keputusan `RJ-DOC-DEC-068`..`082`, `KSK-DEC-025`.

## 1. Petugas pendaftaran

```mermaid
flowchart TD
  subgraph Petugas
    A[Cari / input pasien] --> B[Data Kunjungan: tanggal dan jenis kunjungan]
    B -->|Umum| U[Pilih poli dan dokter seperti biasa]
    B -->|Rujukan| C[Isi identitas rujukan]
    C --> D{Fasilitas bermitra?}
    D -->|Ya| D1[Tampil alert mitra] --> E
    D -->|Tidak| E[Pilih unit tujuan]
    E -->|Poliklinik| F{Ada dokter praktik pada tanggal itu?}
    F -->|Ya| G[Pilih jadwal dokter]
    F -->|Tidak| F1[Buka popup jadwal praktik] --> F2[Ganti tanggal atau poli] --> E
    E -->|Laboratorium| L{Hari ini dan bukan penjamin perusahaan?}
    L -->|Tidak| L1[Tampil batasan, ganti pilihan] --> E
    L -->|Ya| H
    G --> H[Isi diagnosa, alasan, unggah surat]
    H --> P[Pembayaran]
    P --> Q{Tambah penjamin asuransi baru?}
    Q -->|Ya| Q1[Scan kartu] --> Q2{Nama dan No. polis cocok?}
    Q2 -->|Tidak| Q3[Alert data tidak match, scan ulang] --> Q1
    Q2 -->|Ya| R
    Q -->|Tidak| R[General consent dan verifikasi]
    R --> S[Simpan kunjungan]
    S --> T{Rincian dan surat tersimpan?}
    T -->|Ya| V[Selesai, rujukan lengkap]
    T -->|Tidak| W[Selesai, tanda rujukan belum lengkap dan coba lagi]
  end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi identitas rujukan | Petugas | Surat rujukan | No., tanggal, fasilitas, dokter perujuk | Fasilitas belum ada di master → minta petugas master data menambahkannya |
| Pilih unit tujuan | Petugas | Surat rujukan | Poli/Lab | Poli tanpa dokter → popup jadwal, ganti tanggal/poli |
| Scan kartu asuransi baru | Petugas | Kartu fisik | Penjamin tersimpan | Tidak cocok → scan ulang atau perbaiki pilihan |
| Simpan | Sistem | Semua isian | Kunjungan + rujukan | Surat gagal tersimpan → tombol coba lagi; kunjungan tetap ada |

## 2. Kiosk

```mermaid
flowchart TD
  subgraph Pasien di Kiosk
    A[Identifikasi dan review data] --> B[Tujuan layanan: poliklinik]
    B --> C[Jenis kunjungan]
    C -->|Umum| P[Pembayaran] --> L[Layanan dan dokter]
    C -->|Rujukan| P2[Pembayaran] --> D[Data rujukan: nomor, tanggal, fasilitas, dokter perujuk]
    D --> E[Scan surat rujukan]
    E -->|Gagal| E1[Scan ulang atau lewati, tunjukkan surat ke petugas] --> L
    E -->|Berhasil| L
    L --> M{Poli punya dokter praktik?}
    M -->|Tidak| M1[Popup jadwal praktik, pilih poli lain] --> L
    M -->|Ya| K[Konfirmasi dan cetak antrean]
  end
  subgraph Petugas
    K --> X[Daftar kunjungan: rujukan belum lengkap]
    X --> Y[Lengkapi diagnosa dan alasan]
  end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Data rujukan | Pasien | Surat rujukan | Identitas rujukan | Fasilitas tidak ditemukan → hubungi petugas |
| Scan surat | Pasien | Surat fisik | Gambar halaman | Scanner gagal → lewati, tiket menyarankan menunjukkan surat |
| Lengkapi rujukan | Petugas | Surat dari pasien | Rujukan lengkap | Konsultasi sudah dimulai → terkunci, tidak dapat diubah |

## 3. Koreksi rujukan

| Kondisi | Hasil |
|---|---|
| Sebelum konsultasi dokter dimulai | Petugas dapat mengoreksi; riwayat tersimpan |
| Sesudah konsultasi dimulai | Terkunci |
| Unit tujuan salah | Tidak dapat dikoreksi; kunjungan dibatalkan lalu didaftarkan ulang |
