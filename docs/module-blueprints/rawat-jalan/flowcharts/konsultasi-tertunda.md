# Alur Konsultasi Tertunda di Klinis Dokter

| Field | Nilai |
|---|---|
| Revisi | `29` — Amendment KT, `draft` |
| Keputusan | `RJ-DOC-DEC-028`..`031`, `RJ-DOC-FE-010`..`012` |
| Rujukan | `02-backend-architecture.md` *Amendment KT*, `03-frontend-architecture.md` *Amendment KT* |

Konsultasi tertunda adalah konsultasi dokter dari hari sebelumnya yang belum disimpan atau
dibatalkan, sehingga kunjungan pasien masih tertahan di tahap Sedang Konsultasi.

## Alur

```mermaid
flowchart TD
    subgraph Petugas
        P1[Mendaftarkan pasien lama] --> P2{Ditolak karena<br/>kunjungan aktif?}
        P2 -- Tidak --> P3[Pendaftaran berhasil]
        P2 -- Ya --> P4[Buka Daftar Pasien<br/>Rawat Jalan]
        P4 --> P5{Kunjungan masih<br/>dalam konsultasi?}
        P5 -- Tidak --> P6[Batalkan kunjungan]
        P5 -- Ya --> P7[Hubungi dokter<br/>penanggung jawab]
        P6 --> P1
    end

    subgraph Dokter
        D1[Buka Klinis Dokter] --> D2[Lihat bagian<br/>Konsultasi tertunda]
        D2 --> D3[Buka pasien]
        D3 --> D4{Pasien sudah<br/>diperiksa?}
        D4 -- Ya --> D5[Lengkapi catatan<br/>lalu Simpan]
        D5 --> D6{Ada resep atau<br/>tindakan?}
        D6 -- Ya --> D7[Centang konfirmasi<br/>kunjungan lampau]
        D6 -- Tidak --> D8[Konsultasi selesai]
        D7 --> D8
        D4 -- Tidak --> D9[Batalkan konsultasi<br/>dengan alasan]
        D5 -- Validasi gagal --> D10[Perbaiki data yang<br/>ditunjuk lalu ulangi]
        D10 --> D5
    end

    P7 --> D1
    D8 --> P1
    D9 --> P4
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| P1 | Petugas | Data pasien lama | Kunjungan baru, atau penolakan dengan nomor kunjungan lama | Baca nomor dan tanggal kunjungan lama di pesan |
| P4–P5 | Petugas | Nomor kunjungan lama | Petunjuk baris di Daftar Pasien Rawat Jalan | Bila kunjungan tidak terlihat, minta pemegang hak "lihat semua" |
| P6 | Petugas | Alasan pembatalan | Kunjungan batal | Pesan penolakan menjelaskan sebabnya |
| D2 | Dokter | — | Daftar konsultasi tertunda miliknya | Bila daftar gagal dimuat, tekan Coba lagi; antrean hari ini tetap bisa dipakai |
| D5 | Dokter | Catatan klinis lengkap | Konsultasi selesai; pasien dapat didaftarkan lagi | Perbaiki bagian yang ditunjuk pesan validasi |
| D7 | Dokter | Centang konfirmasi | Resep diteruskan ke farmasi, tindakan ditagihkan | Bila resep tidak lagi diperlukan, hapus dulu resepnya, lalu Simpan |
| D9 | Dokter | Alasan, maks 250 karakter | Konsultasi batal; kunjungan menunggu dibatalkan petugas | Bila ditolak karena bukan penulis, konsultasi harus dibatalkan oleh dokter penulisnya |

**Contoh:** IKBAL YULIYANTO ditolak saat didaftarkan pada 5 Okt 2026 karena ENC-RSMMC-00172
(30 Sep). Petugas melihat petunjuk "Konsultasi masih aktif…" lalu menghubungi dr. Arif Lesmana.
dr. Arif membuka Konsultasi tertunda, memastikan IKBAL memang sudah diperiksa, lalu menekan
Simpan. Pendaftaran IKBAL kemudian diterima.
