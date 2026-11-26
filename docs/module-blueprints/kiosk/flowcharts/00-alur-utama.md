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


---

## Amendment PO — Pendaftaran Pasien Online sampai tiket (jalur normal)

Revisi blueprint `2` (`approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`)). Jalur pengecualian ada di [pendaftaran-pasien-online.md](pendaftaran-pasien-online.md), [check-in-pasien-online.md](check-in-pasien-online.md), dan [pendaftaran-online-petugas-dan-penutupan.md](pendaftaran-online-petugas-dan-penutupan.md).

```mermaid
flowchart TD
    subgraph rumah[Pasien di rumah]
        A([Membuka halaman Pendaftaran Pasien Online]) --> B[Isi data, pilih jadwal besok s.d. 30 hari, kirim]
    end
    subgraph sistem[Sistem]
        B --> C[(Menunggu Check-in)]
        C --> D[Kode tampil dan dikirim ke email atau WhatsApp]
    end
    subgraph kiosk[Pasien di kiosk pada hari kunjungan]
        D --> E[Masukkan kode dan tanggal lahir]
        E --> F[Pasien baru memindai KTP atau KIA]
        F --> G[Tekan Check-in Sekarang]
    end
    G --> H[(Menjadi Kunjungan)]
    H --> I([Tiket antrean tercetak, pasien menuju poli])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Daftar online | Pasien | Data pasien, kontak, jadwal, penjamin, rujukan, consent (pasien baru) | Menunggu Check-in, kode | Lihat flowchart pendaftaran |
| Terima kode | Sistem | Email atau No. WA | Email otomatis atau tombol WhatsApp | Kode tetap tampil di layar; Lupa Kode di kiosk |
| Check-in | Pasien | Kode atau identitas, tanggal lahir, KTP/KIA (pasien baru) | Menjadi Kunjungan, nomor antrean, tiket | Lihat flowchart check-in; pasien diarahkan ke loket |
