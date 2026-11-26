# Proses — Penilaian Vendor Berkala

Status dokumen: `approved` (`PBJ-DEC-062`). Keputusan: `PBJ-DEC-044`, `048`, `049`, `050`, `059`.

**Tujuan:** kinerja vendor tercatat per kategori dan per periode sebagai bukti tindakan.
**Pemicu:** akhir periode penilaian (bawaan semester **[NILAI AWAL `G-V05`]**).
**Prasyarat:** vendor punya kategori; kriteria aktif kategori itu berbobot total 100%.

```mermaid
flowchart TD
    subgraph penilai[Petugas Pengadaan]
        A([Periode penilaian tiba]) --> B{Sudah ada penilaian vendor, kategori, periode ini?}
        B -- Ya --> B1[/Ditolak, buka penilaian yang ada atau buat revisi/]
        B -- Tidak --> C{Bobot kriteria kategori sudah 100%?}
        C -- Tidak --> C1[/Ditolak, minta admin melengkapi kriteria/]
        C -- Ya --> D[Buat draf; kriteria dan bobot disalin]
        D --> E[(DRAFT)]
        E --> F[Isi nilai 1 sampai 5, catatan, rekomendasi]
        F --> Z{Dibatalkan?}
        Z -- Ya --> Z1[(CANCELLED)]
    end
    subgraph manajer[Manajer Pengadaan]
        Z -- Tidak --> G{Pemfinal sama dengan penilai?}
        G -- Ya --> G1[/Ditolak, perlu orang lain/]
        G -- Tidak --> H{Semua nilai dan rekomendasi terisi?}
        H -- Tidak --> H1[/Ditolak, lengkapi nilai/]
        H1 --> F
        H -- Ya --> I[(FINAL)]
    end
    subgraph koreksi[Koreksi]
        I --> J{Ditemukan salah input?}
        J -- Ya --> K[Buat revisi dengan alasan]
        K --> E
        J -- Tidak --> L([Penilaian berlaku])
        K -. revisi difinalkan .-> M[(SUPERSEDED)]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Buat draf | Petugas Pengadaan | Vendor, kategori, periode | `DRAFT` dengan salinan kriteria | Sudah ada → buka/revisi; bobot ≠ 100% → admin |
| Isi nilai | Petugas Pengadaan | Nilai per kriteria | Nilai akhir dihitung server | — |
| Finalkan | Manajer Pengadaan | — | `FINAL` | Pemfinal = penilai → orang lain; nilai belum lengkap → lengkapi |
| Revisi | Petugas Pengadaan | Alasan revisi | Draf revisi; versi lama `SUPERSEDED` saat revisi final | Revisi lain masih berjalan → selesaikan dulu |

**Contoh:** PT Sehat Medika (Alat Kesehatan dan Umum) mendapat dua penilaian Semester I 2027. Alat Kesehatan: bobot
40/40/20, nilai 4/5/3 → 4,20, rekomendasi Lanjutkan. Status vendor tidak berubah karena rekomendasi hanya saran.
