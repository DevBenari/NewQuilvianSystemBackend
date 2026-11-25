# Pemeriksaan Fisik dan Pencocokan

**Tujuan:** setiap aset unit diperiksa fisiknya dan dibandingkan dengan catatan.
**Pemicu:** tanggal mulai sesi tiba. **Prasyarat:** tidak ada penyerahan/mutasi/pinjaman yang belum selesai di
unit; tidak ada sesi aktif lain di unit itu. **Hasil akhir:** hasil pencocokan per aset dan berita acara.

```mermaid
flowchart TD
    subgraph ketua[Ketua tim]
        A([Tanggal mulai tiba]) --> B{Masih ada proses berjalan di unit?}
        B -- Ya --> B1[/Ditolak, daftar proses ditampilkan/]
        B1 --> B2[Minta Bagian Aset menyelesaikan proses]
        B2 --> B
        B -- Tidak --> C[(Berjalan)]
    end
    subgraph anggota[Anggota tim]
        C --> D[Pindai QR aset]
        D --> E{Nomor dikenal?}
        E -- Tidak --> E1[Catat temuan tidak terdaftar dengan foto]
        E -- Ya --> F[Pilih ruangan ditemukan dan kondisi]
        F --> G{Masih ada aset di ruangan?}
        E1 --> G
        G -- Ya --> D
    end
    subgraph ketua2[Ketua tim]
        G -- Tidak --> H[Tutup pemindaian]
        H --> I[(Pencocokan)]
        I --> J{Ada aset terlewat?}
        J -- Ya --> J1[Buka kembali pemindaian]
        J1 --> C
        J -- Tidak --> K[Beri catatan setiap selisih]
        K --> L[(MenungguVerifikasi)]
    end
    subgraph kepala[Kepala Bagian Aset]
        L --> M{Hasil dapat diterima?}
        M -- Tidak --> M1[Kembalikan dengan alasan]
        M1 --> I
        M -- Ya --> N[(Diverifikasi)]
    end
    N --> O([Berita acara terbit; usulan tindak lanjut dibuat])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Mulai | Ketua tim | — | Snapshot dibekukan | Selesaikan proses yang disebut |
| Pindai | Anggota tim | QR (kamera) atau nomor diketik | Pindaian; pindai ulang menggantikan yang lama | Kamera tidak bisa dipakai → ketik nomor |
| Temuan | Anggota tim | Keterangan + foto | Temuan `TidakTerdaftar` | Lengkapi keterangan dan foto |
| Tutup | Ketua tim | — | Hasil terhitung | — |
| Verifikasi | Kepala Bagian Aset | — | `Diverifikasi` | Bila ia anggota tim → tunjuk verifikator lain |

**Contoh.** IGD punya 312 aset diharapkan. Tim memindai 310 dan mencatat 1 temuan. Hasil: 303 `Sesuai`,
1 `SesuaiSedangDipinjam` (pompa di ICU, dipindai di ICU), 2 `TidakDitemukan`, 1 `BedaLokasi`
(monitor di ruang observasi, tercatat di resusitasi), 5 `BedaKondisi`, 1 `TidakTerdaftar`.
