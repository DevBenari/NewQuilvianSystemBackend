# Flowchart — Pendaftaran Online di Layar Petugas dan Penutupan Otomatis (Amendment PO)

| Field | Nilai |
| --- | --- |
| Revisi blueprint | `2` (`approved` — Sukma Giri Pratama, 19 Nov 2026 (`KSK-DEC-054`)) |
| Keputusan | `KSK-DEC-037`, `045`, `050`, `051`, `052`; `GATE-PO-13` |
| Status yang dipakai | Sama persis dengan `contracts/state-transition-matrix.md` *Amendment PO* |

## 1. Ubah dan batal oleh petugas

```mermaid
flowchart TD
    subgraph petugas[Petugas admisi]
        A([Pasien menelepon atau datang meminta perubahan]) --> B[Cari pendaftaran di layar Pendaftaran Online]
        B --> C{Yang diminta?}
        C -- Ganti hari atau dokter --> D[Pilih tanggal dan jadwal di poli yang sama]
        C -- Ganti kontak --> E[Isi No. WhatsApp atau email baru]
        C -- Batal atau mencurigakan --> F[Isi alasan dan konfirmasi]
    end
    subgraph sistem[Sistem]
        D --> D1{Masih Menunggu Check-in, tanggal besok s.d. 30 hari, kuota ada, belum ada pendaftaran lain?}
        D1 -- Tidak --> D2[/Ditolak, pendaftaran lama tetap/]
        D2 --> D
        D1 -- Ya --> D3[(Menunggu Check-in)]
        D3 --> D4[Lepas kuota lama, pakai kuota baru, catat riwayat]
        E --> E1{Kontak sah dan data belum diubah orang lain?}
        E1 -- Tidak --> E2[/Ditolak, perbaiki atau muat ulang/]
        E2 --> E
        E1 -- Ya --> E3[Simpan kontak dan catat riwayat]
        F --> F1{Masih Menunggu Check-in dan alasan terisi?}
        F1 -- Tidak --> F2[/Ditolak, sudah tidak dapat diubah/]
        F1 -- Ya --> F3[(Dibatalkan)]
        F3 --> F4[Kode mati, kuota dilepas, catat riwayat]
    end
    D4 --> Z([Petugas menyampaikan perubahan kepada pasien])
    E3 --> Z
    F4 --> Z
    F2 --> Z
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Cari pendaftaran | Petugas | Kode, nomor pendaftaran, nama, No. RM, atau NIK | Detail pendaftaran | Tidak ditemukan: periksa kata kunci dan rentang tanggal |
| Ganti jadwal | Petugas | Tanggal baru, jadwal di poli yang sama | Jadwal baru, kode tetap | Kuota penuh atau sudah ada pendaftaran: pilih jadwal lain. Pindah poli: batalkan lalu minta pasien mendaftar ulang |
| Ganti kontak | Petugas | Kontak baru | Kontak tersimpan, kode tidak dikirim ulang | Muat ulang bila data diubah orang lain |
| Batal | Petugas | Alasan 5–500 karakter | Status Dibatalkan | Pendaftaran yang sudah berstatus akhir tidak dapat dibatalkan |

## 2. Penutupan otomatis

```mermaid
flowchart TD
    subgraph lain[Kiosk Pasien Lama/Baru atau loket]
        A([Pasien didaftarkan lewat jalur lain pada hari yang sama]) --> B[Kunjungan Rawat Jalan terbit]
    end
    subgraph sistem[Sistem]
        B --> C{Pasien punya pendaftaran online Menunggu Check-in pada tanggal itu?}
        C -- Tidak --> C1([Tidak ada yang berubah])
        C -- Ya --> D[(Didaftarkan lewat Jalur Lain)]
        D --> D1[Tautkan ke kunjungan, kode mati, kuota dilepas]
        T([Pukul 00.05 setiap hari]) --> U{Ada pendaftaran Menunggu Check-in dengan tanggal sebelum hari ini?}
        U -- Tidak --> U1([Selesai])
        U -- Ya --> V[(Tidak Datang)]
        V --> V1[Kode mati, kuota dilepas, catat riwayat]
    end
```

| Langkah | Pelaku | Masukan yang dibutuhkan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Tutup lewat jalur lain | Sistem, di dalam pembuatan kunjungan | Pasien dan tanggal kunjungan; pasien baru dicari lewat NIK | Didaftarkan lewat Jalur Lain | Seluruh pembuatan kunjungan batal bersama; petugas mengulang |
| Tutup Tidak Datang | Sistem terjadwal | Tanggal hari ini (WIB) | Tidak Datang | Dijalankan lagi saat aplikasi menyala; hari yang tertinggal ikut diproses |
