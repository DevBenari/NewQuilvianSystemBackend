# Flowchart — Pengesahan Interval Ukur Ulang TTV

| Field | Nilai |
|---|---|
| Revision | `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Keputusan | `NV-DEC-010`; gap `G-01`; open question `NV-OQ-010` |

```mermaid
flowchart TD
    subgraph komite[Komite Keperawatan]
        A([Kebijakan frekuensi TTV disusun]) --> B[Sahkan interval per tingkat risiko]
        B --> C[Terbitkan surat atau notulen]
    end
    subgraph it[Admin IT]
        C --> D[Isi interval, nomor surat, dan tanggal]
        D --> E{Isian sah?}
        E -- Tidak --> E1[/Ditolak, perbaiki isian/]
        E1 --> D
        E -- Ya --> F[Simpan, himpunan lama menjadi riwayat]
    end
    subgraph tv[TV]
        F --> G{Interval untuk tingkat ini ada?}
        G -- Tidak --> H[Kartu hanya menampilkan umur pengukuran]
        G -- Ya --> I{Umur pengukuran melewati interval?}
        I -- Ya --> J[Kartu ditandai lewat jadwal]
        I -- Tidak --> K[Kartu normal]
    end
    X([Belum ada pengesahan]) --> H
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Sahkan interval | Komite Keperawatan | Kebijakan klinis | Surat/notulen | Belum disahkan → TV tetap aman-gagal, tanpa tanda |
| Isi interval | Admin IT | Angka, nomor surat, tanggal | Himpunan aktif baru | Isian ditolak → perbaiki sesuai pesan |
| Tanda di kartu | Sistem | TTV terakhir + interval tingkatnya | Tanda lewat jadwal atau tidak | Tingkat tanpa interval → tanpa tanda |
| Cabut interval | Admin IT | Himpunan kosong + nomor surat pencabutan | Kembali aman-gagal | — |

**Contoh.** SK Komite menetapkan Sedang = 4 jam. Pasien dengan EWS Sedang terakhir diukur
08.00. Pukul 12.00 kartunya mulai ditandai lewat jadwal.
