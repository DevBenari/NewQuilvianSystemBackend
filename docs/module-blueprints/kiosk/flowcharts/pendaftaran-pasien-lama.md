# Proses — Pendaftaran Pasien Lama (8 Step)

Keadaan pada node `[(...)]` sama persis dengan `contracts/state-transition-matrix.md` §2.

```mermaid
flowchart TD
    subgraph pasien[Pasien]
        A([Masuk Pendaftaran Pasien Lama]) --> ID[(IDENTIFICATION)]
        ID --> I1{Pasien sudah terpilih?}
        I1 -- Belum --> I2[Ketik atau pindai kartu] --> I3{Tepat satu pasien?}
        I3 -- Tidak --> I4[/Pasien tidak ditemukan atau perlu petugas/] --> I2
        I3 -- Ya --> I1
        I1 -- Sudah --> RV[(DATA_REVIEW)]
        RV --> R1{Data benar?}
        R1 -- Tidak --> R2[Cari Pasien Lain atau hubungi petugas] --> ID
        R1 -- Ya --> SD[(SERVICE_DESTINATION)]
        SD --> T1{Tujuan?}
    end
    subgraph sistem[Sistem]
        T1 -- Poliklinik --> SS1{Sesi kiosk tercatat?}
        T1 -- Laboratorium + jawab surat dokter --> SS2{Sesi kiosk tercatat?}
        SS1 -- Tidak --> E1[/Pilihan belum dapat disimpan/] --> SD
        SS2 -- Tidak --> E1
        SS2 -- Ya --> LH[(LAB_HANDOFF)]
        SS1 -- Ya --> VT[(VISIT_TYPE)]
    end
    VT --> PY[(PAYMENT)] --> SV[(SERVICE_AND_DOCTOR)] --> CF[(CONFIRMATION)]
    CF --> C1{Kunjungan tersimpan?}
    C1 -- Tidak --> C2[/Pendaftaran gagal, periksa pilihan/] --> CF
    C1 -- Ya --> QP[(QUEUE_PRINT)]
    QP --> END([Selesai, data dibersihkan])
    LH --> END
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Identifikasi | Pasien | KTP, HP, No. RM, nama, atau kartu (pindai) | Satu pasien terpilih | Tidak ada atau lebih dari satu → cari ulang atau hubungi petugas |
| Review Data | Pasien | Data pasien | Konfirmasi data benar | Data salah → Cari Pasien Lain; perbaikan data dilakukan petugas |
| Tujuan Layanan | Pasien | Poliklinik, atau Laboratorium + jawaban surat dokter | Sesi kiosk tercatat **sekali** | Gagal tersimpan → tetap di step ini, coba lagi atau hubungi petugas |
| Laboratorium selesai | Sistem | Sesi kiosk Laboratorium | Pasien diarahkan ke laboratorium | — |
| Jenis Kunjungan | Pasien | Umum / Rujukan | Pilihan | — |
| Pembayaran | Pasien | Tunai / Asuransi / Perusahaan | Satu penanggung kunjungan | Lihat `penjamin-utama.md` |
| Layanan & Dokter | Pasien | Poli, dokter, jadwal | Pilihan lengkap | Belum lengkap → pesan |
| Konfirmasi | Pasien | Ringkasan | Kunjungan terdaftar | Ditolak (misalnya penjamin tidak aktif) → tetap di Konfirmasi dengan pesan; pasien kembali ke Pembayaran atau ke petugas |
| Cetak Antrean | Sistem | Kunjungan | Nomor antrean | Printer gagal → pasien menemui petugas |
| Kapan pun: 120 detik tanpa sentuhan | Sistem | — | Peringatan 15 detik, lalu data dibersihkan | Ditunda bila sistem sedang menyimpan |
