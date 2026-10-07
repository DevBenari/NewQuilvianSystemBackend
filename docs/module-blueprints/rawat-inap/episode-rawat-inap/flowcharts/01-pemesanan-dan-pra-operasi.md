# Flowchart — Pemesanan ruang bedah, penolakan, dan catatan pra-operasi

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | `0.10.0` — `draft` |
| Proses | `BP-RWF-03` |
| Keputusan | `RWI-DEC-173` s.d. `176`, `199`, `204` |

## 1. Pemesanan dan penolakan

```mermaid
flowchart TD
    subgraph bangsal[Perawat atau dokter bangsal]
        A([Buka menu Pemesanan Ruangan Bedah]) --> B{Ada order tindakan operasi aktif?}
        B -- Tidak --> B1[/Pesanan tidak dapat dibuat/]
        B1 --> B2[Minta dokter memesan tindakan]
        B -- Ya --> C[Pilih tab Bedah Operasi atau Bedah Obgyn]
        C --> D[Isi tanggal, anestesi, prioritas, sisi tubuh, indikasi]
        D --> E[Kirim pesanan]
    end
    subgraph ok[Petugas penjadwalan OK]
        E --> F{Pesanan layak dijadwalkan?}
        F -- Ya --> G[Jadwalkan]
        F -- Tidak --> H[Tolak dengan alasan]
    end
    subgraph lihat[Bangsal melihat hasil]
        G --> I([Status Terjadwal beserta jam])
        H --> J[Status Ditolak, alasan, penolak, waktu]
        J --> K{Masih perlu operasi?}
        K -- Ya --> C
        K -- Tidak --> L([Selesai tanpa biaya])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih order | Perawat atau dokter | Order tindakan aktif pasien | Tindakan dan dokter operator terisi | Tidak ada order: hubungi dokter |
| Kirim pesanan | Perawat atau dokter | Isian pesanan | Kasus "Diminta" | Pasien bukan lagi dirawat: pesanan ditolak |
| Jadwalkan | Petugas OK | Kasus "Diminta" | "Terjadwal" | — |
| Tolak | Petugas OK | Alasan | "Ditolak", final | Alasan kosong: isi alasan |
| Pesan ulang | Perawat atau dokter | Order yang sama | Kasus baru "Diminta" | — |

## 2. Catatan pra-operasi dan penundaan

```mermaid
flowchart TD
    subgraph pengirim[Perawat bangsal]
        A([Kasus diminta atau terjadwal]) --> B{Tanda vital sudah dicatat?}
        B -- Belum --> B1[Catat tanda vital dulu]
        B1 --> B
        B -- Sudah --> C[Isi checklist dan penandaan area operasi]
        C --> D{Butir wajib lengkap dan sisi sesuai pesanan?}
        D -- Tidak --> C
        D -- Ya --> E[Kirim; tanda vital dan nyeri dibekukan]
    end
    subgraph penerima[Perawat OK, akun lain]
        E --> F[Periksa dan konfirmasi setiap butir]
        F --> G{Semua butir wajib dan penandaan terkonfirmasi?}
        G -- Belum --> F
        G -- Ya --> H[Catatan terkonfirmasi]
    end
    subgraph okjadwal[Kamar Operasi]
        H --> I{Persetujuan tindakan dan anestesi ada?}
        I -- Belum --> I1[Lengkapi persetujuan]
        I1 --> I
        I -- Ya --> J([Kasus boleh Siap])
        H --> K{Kasus ditunda?}
        K -- Ya --> L[Catatan menjadi perlu diperbarui]
        L --> M[Setelah dijadwalkan ulang, bangsal kirim versi baru]
        M --> E
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi checklist | Perawat bangsal | Butir dari master | Draf | Master kosong: minta admin mengisi butir persiapan |
| Penandaan | Perawat bangsal | Titik pada gambar tubuh, sisi | Penandaan | Sisi berbeda dengan pesanan: perbaiki sisi atau minta OK memperbaiki pesanan |
| Kirim | Perawat bangsal | Draf lengkap | Catatan terkirim dengan potret | Tanda vital belum ada: catat dulu |
| Konfirmasi | Perawat OK | Catatan terkirim | Terkonfirmasi | Akun sama dengan pengirim: minta rekan lain |
| Penundaan | Petugas OK | Kasus ditunda | Catatan perlu diperbarui | — |
| Versi baru | Perawat bangsal | Butir lama sebagai usulan, tanda vital terbaru | Versi baru terkirim | Sama dengan kirim |
