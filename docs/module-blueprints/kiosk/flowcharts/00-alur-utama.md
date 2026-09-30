# Kiosk — Alur Utama

Jalur normal dari pasien datang ke Kiosk sampai memegang nomor antrean. Percabangan dan jalur gagal ada di berkas proses masing-masing:

- [cek-nomor-rekam-medis.md](cek-nomor-rekam-medis.md)
- [pendaftaran-pasien-lama.md](pendaftaran-pasien-lama.md)
- [penjamin-utama.md](penjamin-utama.md)

```mermaid
flowchart TD
    subgraph pasien[Pasien di Kiosk]
        A([Pasien datang ke Kiosk]) --> B[Pilih Cek Nomor Rekam Medis]
        B --> C[Masukkan No. KTP atau No. HP]
        C --> D[Lihat Kartu Pasien]
        D --> E[Lanjut Pendaftaran Pasien Lama]
        E --> F[Identifikasi: pasien sudah terisi]
        F --> G[Review Data]
        G --> H[Pilih Tujuan Layanan: Poliklinik]
        H --> I[Pilih Jenis Kunjungan]
        I --> J[Pilih Pembayaran dan Penjamin Utama]
        J --> K[Pilih Layanan, Dokter, Jadwal]
        K --> L[Periksa Konfirmasi lalu Daftar]
    end
    subgraph sistem[Sistem]
        C --> S1{Pasien ditemukan tepat satu?}
        S1 -- Ya --> D
        H --> S2[(Sesi kiosk tercatat)]
        L --> S3[(Kunjungan terdaftar)]
    end
    S3 --> M[Cetak Antrean]
    M --> N([Pasien menunggu dipanggil])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Cek Nomor Rekam Medis | Pasien | No. KTP atau No. HP | Kartu Pasien | Lihat `cek-nomor-rekam-medis.md` |
| Identifikasi | Pasien | Pasien hasil pencarian | Pasien terpilih | Cari ulang |
| Review Data | Pasien | Data pasien | Data dinyatakan benar | Pasien menemui petugas bila datanya salah |
| Tujuan Layanan | Pasien | Poliklinik / Laboratorium | Sesi kiosk tercatat | Coba lagi atau hubungi petugas |
| Jenis Kunjungan → Layanan & Dokter | Pasien | Pilihan | Draf pendaftaran lengkap | Pesan di step itu |
| Konfirmasi | Pasien | Draf lengkap | Kunjungan terdaftar | Tetap di Konfirmasi dengan pesan |
| Cetak Antrean | Sistem | Kunjungan | Nomor antrean | Pasien menemui petugas |
