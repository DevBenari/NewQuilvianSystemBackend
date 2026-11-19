# Flowchart — Check-in Pasien Online di Kiosk (Amendment PO)

| Field | Nilai |
| --- | --- |
| Revisi blueprint | `2` (`approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`)) |
| Keputusan | `KSK-DEC-033`, `036`, `039`, `040`, `046`, `048`, `053` |
| Status yang dipakai | Sama persis dengan `contracts/state-transition-matrix.md` *Amendment PO* |

Pelaku: pasien di kiosk, dan sistem.

```mermaid
flowchart TD
    subgraph pasien[Pasien di kiosk]
        A([Menekan Check-in Pasien Online]) --> B{Punya kode?}
        B -- Ya --> C[Masukkan kode dan tanggal lahir]
        B -- Lupa --> D[Masukkan NIK atau No. RM dan tanggal lahir]
        G{Pasien baru?}
        G -- Ya --> H[Pindai KTP atau KIA]
        G -- Tidak --> K[Tekan Check-in Sekarang]
    end
    subgraph sistem[Sistem]
        C --> E{Cocok dan masih Menunggu Check-in?}
        D --> E
        E -- Tidak --> E1[/Ditolak, data tidak sesuai atau tidak berlaku/]
        E1 --> B
        E -- Ya --> E2{Hari ini tanggal kunjungannya?}
        E2 -- Belum --> E3[/Ditolak, sebut tanggal kunjungan/]
        E3 --> X([Pasien pulang atau ke loket])
        E2 -- Ya --> F[Tampilkan ringkasan pendaftaran]
        H --> H1{Kartu KTP atau KIA dan NIK terbaca?}
        H1 -- Tidak --> H2[/Ditolak, pindai ulang atau ke loket/]
        H2 --> H
        H1 -- Ya --> I{NIK sama dengan pendaftaran?}
        I -- Tidak --> I1[/Ditolak, data kartu tidak sesuai, ke loket/]
        I1 --> X
        I -- Ya --> I2{NIK sudah ada di data pasien?}
        I2 -- Ya --> I3[Pakai No. RM lama tanpa mengubah datanya]
        I2 -- Belum --> I4[Buat data pasien dan No. RM dari hasil pindai]
        I3 --> J[Siap check-in]
        I4 --> J
        K --> L{Jadwal dokter masih berlaku dan kuota cukup?}
        L -- Tidak --> L1[/Ditolak, jadwal berubah, ke loket/]
        L1 --> X
        L -- Ya --> M[(Menjadi Kunjungan)]
        M --> N[Terbitkan nomor antrean dan cetak tiket]
    end
    F --> G
    J --> K
    N --> O([Pasien menuju poli])
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Masukkan kode | Pasien | Kode 8 karakter, tanggal lahir | Ringkasan pendaftaran | Pesan umum; pasien memeriksa ulang atau memakai Lupa Kode |
| Lupa Kode | Pasien | NIK/No. RM, tanggal lahir | Ringkasan pendaftaran hari ini | Pesan umum; pasien ke petugas |
| Cek hari kunjungan | Sistem | Tanggal kunjungan | — | Pesan menyebut tanggal; pasien datang lagi pada hari itu |
| Pindai kartu (pasien baru) | Pasien | KTP atau KIA | NIK terbaca dan terkunci | Pindai ulang; bila tetap gagal atau membawa SIM, ke loket |
| Cocokkan NIK | Sistem | NIK hasil pindai, NIK pendaftaran | No. RM lama atau No. RM baru | Ke loket; pendaftaran online tertutup otomatis saat petugas mendaftarkan |
| Check-in | Pasien | — | Status Menjadi Kunjungan, nomor antrean, tiket | Jadwal berubah: ke loket. Waktu check-in habis: ulangi dari awal |
| Cetak tiket | Sistem | Nomor antrean | Tiket | Tiket tampil di layar; cetak ulang lewat Cetak Antrean |
