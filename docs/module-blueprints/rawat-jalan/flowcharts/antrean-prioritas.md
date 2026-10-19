# Flowchart — Antrean Prioritas dan Layar Display (Amendment RP)

| Field | Nilai |
|---|---|
| Revisi blueprint | `33` (`approved` 2026-10-16) |
| Keputusan | `RJ-PRI-DEC-004` s.d. `007`, `017`, `018`, `024`, `025` |

Flowchart ini berlaku untuk semua pasien yang mendapat nomor antrean Rawat Jalan, baik lewat reservasi, kiosk, maupun
loket.

```mermaid
flowchart TD
    subgraph daftar[Kiosk atau admisi]
        A([Kunjungan Rawat Jalan berpoliklinik didaftarkan]) --> B{Dari reservasi prioritas?}
        B -- Ya --> C{Membership pada reservasi dibatalkan?}
        C -- Ya --> R[Diperlakukan Reguler]
        C -- Tidak --> P[Diperlakukan sesuai tingkat saat reservasi dibuat]
        B -- Tidak --> D{Member aktif dengan tingkat prioritas pada hari pendaftaran?}
        D -- Ya --> P
        D -- Tidak --> R
    end
    subgraph sistem[Sistem]
        P --> P1[Ambil kelipatan 5 terkecil yang belum terpakai]
        R --> R1[Ambil nomor terkecil yang bukan kelipatan 5]
        P1 --> P2[Tiket dan bukti daftar mengarahkan ke ruang tunggu prioritas]
        R1 --> R2[Tiket mengarahkan ke ruang tunggu poli]
    end
    subgraph layanan[Perawat dan dokter]
        P2 --> P3[Tidak tampil di display dan tidak dipanggil suara]
        P3 --> P4([Perawat dan dokter menghampiri di ruang tunggu prioritas])
        R2 --> R3[Tampil di display dan dipanggil seperti biasa]
        R3 --> R4([Pasien menuju ruang periksa])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Tentukan klasifikasi | Sistem | Pasien, hari pendaftaran, atau snapshot reservasi | Prioritas atau Reguler | Status member tidak terbaca: pasien diperlakukan Reguler dan pendaftaran tetap jalan |
| Ambil nomor | Sistem | Klasifikasi, poli, tanggal | Nomor antrean | Dua pendaftaran bersamaan: kunci nomor membuat salah satunya menunggu, sehingga tidak ada nomor ganda |
| Cetak tiket / bukti | Kiosk, admisi | Klasifikasi, nama ruang tunggu prioritas | Arahan ruang tunggu | Nama ruang belum diatur: tiket memakai nama bawaan |
| Tampil di display | Sistem | Antrean Reguler saja | Display dan suara | Kabar realtime terputus: display memuat ulang daftar berkala |
| Menghampiri | Perawat, dokter | Penanda lokasi di worklist | Pasien dilayani | Pasien tidak ada di ruang prioritas: petugas menghubungi admisi |
