# Alur Benefit RS — Pengajuan, Persetujuan, dan Sinkronisasi Penjamin Pasien

```yaml
last_changed_in: Revisi 22 (3 November 2026)
status: approved   # 2026-11-03, pemilik (pengguna sesi)
input_revision: 00-interview-decisions.md FIN-DEC-231..257; 02-backend-architecture.md bagian V
```

Nama status pada diagram sama dengan `contracts/state-transition-matrix.md` bagian V.

## 1. Pengajuan dan persetujuan

```mermaid
flowchart TD
    subgraph StafHR[Staf HR]
        A[Isi form pendaftaran benefit] --> B[Tekan Ajukan Benefit]
    end
    subgraph Sistem
        B --> C{Isian dan penerima sah?}
        C -- Tidak --> C1[Tolak, tidak ada yang tersimpan]
        C -- Ya --> D[Enrollment Submitted]
        E --> F{Tanggal mulai sudah tiba?}
        F -- Ya --> G{Syarat aktivasi terpenuhi?}
        G -- Tidak --> G1[Persetujuan batal, tetap Submitted]
        G -- Ya --> H[Enrollment Active dan saldo awal ditetapkan]
        F -- Belum --> I[Enrollment Approved]
        I --> J[Penjadwal pada tanggal mulai]
        J --> G
        H --> K[Jalankan sinkronisasi kartu]
    end
    subgraph HRD
        D --> E[HRD menekan Setujui]
        G1 --> E2[HRD membereskan penyebab lalu Setujui lagi]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Ajukan Benefit | Staf HR | Penerima dari HR, program, tanggal | Enrollment Diajukan | Petugas membetulkan isian yang ditandai; tidak ada Draft tertinggal |
| Setujui | HRD | Enrollment Diajukan | Aktif (tanggal tiba) atau Disetujui (tanggal depan) | Bila syarat aktivasi gagal, HRD membaca pesan penyebab, misalnya membatalkan benefit lama yang beririsan, lalu menyetujui lagi |
| Aktivasi terjadwal | Sistem | Enrollment Disetujui yang tanggalnya tiba | Aktif | Bila gagal, tetap Disetujui dan dicoba lagi; HRD dapat menekan Aktifkan manual |

## 2. Sinkronisasi kartu penjamin pasien

```mermaid
flowchart TD
    subgraph Sistem
        A[Benefit Active] --> B{Pemilik masih layak di HR?}
        B -- Tidak --> X1[Gagal sementara, dicoba lagi]
        B -- Ya --> C{Pasien dengan KTP atau NIK sama dan tanggal lahir sama}
        C -- Tidak ada --> W[Menunggu pasien]
        C -- Lebih dari satu --> H1[Ditahan untuk ditinjau]
        C -- Tepat satu --> D{Pasien sudah punya kartu internal?}
        D -- Milik pemilik sama --> S[Synced, kartu dipakai ulang]
        D -- Milik orang lain atau tanpa tautan --> H1
        D -- Belum ada --> E[Pasang kartu Benefit Karyawan RS]
        E --> S
        W --> R[Penjadwal mencoba lagi]
        X1 --> R
        R --> B
    end
    subgraph Registrasi
        H1 --> T[Petugas meninjau di daftar kerja, memperbaiki data atau mendaftarkan kartu manual]
    end
    subgraph HRD
        T --> U[HRD menekan Sinkronkan ulang]
        U --> B
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Cari pasien | Sistem | Identitas pemilik dari HR | Tepat satu pasien, nol, atau lebih | Nol: menunggu, tidak membuat pasien. Lebih dari satu: ditahan |
| Periksa kartu yang ada | Sistem | Pasien terpilih | Pakai ulang, pasang baru, atau ditahan | Kartu milik orang lain tidak pernah diubah |
| Tinjau kasus ditahan | Petugas Registrasi | Daftar kerja | Data pasien dibetulkan atau kartu manual didaftarkan | Bila tetap ragu, biarkan ditahan; pasien tetap bisa berobat sebagai Pasien Umum |
| Sinkronkan ulang | HRD | Enrollment Aktif | Hasil sinkronisasi baru | Pesan alasan tampil di detail enrollment |

## 3. Pembatalan, pencabutan, dan kunjungan

```mermaid
flowchart TD
    subgraph HRD
        A[HRD membatalkan enrollment] --> B[Enrollment Cancelled]
    end
    subgraph Registrasi
        C[Petugas menonaktifkan kartu yang salah pasien] --> D[Kartu nonaktif]
        K[Petugas mendaftarkan kunjungan dengan kartu]
    end
    subgraph Sistem
        B --> E[Kartu yang tercatat dinonaktifkan, tidak dihapus]
        D --> F[Catatan menjadi Revoked, tidak dipasang ulang otomatis]
        K --> G{Kartu hasil sinkronisasi?}
        G -- Tidak --> L[Pemeriksaan biasa]
        G -- Ya --> H{Identitas pasien masih cocok dengan pemilik?}
        H -- Ya --> L
        H -- Tidak --> M[Kartu ditolak, pendaftaran tetap bisa sebagai Pasien Umum]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Batal | HRD | Alasan | Enrollment Dibatalkan, kartu nonaktif | Bila penonaktifan tertunda, penjadwal mengulang; kartu tetap tidak bisa dipakai karena HR menolak |
| Cabut kartu | Petugas Registrasi | Kartu yang salah pasien | Kartu nonaktif, catatan Revoked | HRD memasang kembali hanya setelah data benar |
| Daftar kunjungan | Petugas Registrasi atau kasir | Kartu pasien | Kunjungan memakai benefit, atau kartu ditolak | Petugas memilih Pasien Umum atau penjamin lain; layanan darurat tidak tertunda |
