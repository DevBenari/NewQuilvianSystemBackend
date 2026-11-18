# Peminjaman dan Pengembalian

**Tujuan:** unit lain memakai aset sementara tanpa memindahkan kepemilikan unit, dengan batas waktu.
**Pemicu:** unit membutuhkan alat sementara. **Prasyarat:** sama dengan mutasi; unit peminjam ≠ pemilik.
**Hasil akhir:** aset kembali ke pemilik dengan kondisi tercatat; keterlambatan terlihat di dashboard.

```mermaid
flowchart TD
    subgraph peminjam[Kepala Unit peminjam atau Petugas Aset]
        A([Butuh alat sementara]) --> B[Ajukan pinjaman dengan tanggal kembali]
        B --> C{Aset layak?}
        C -- Tidak --> C1[/Ditolak, sebab ditampilkan/]
        C1 --> B
        C -- Ya --> D[(Diajukan)]
    end
    subgraph pemilik[Kepala Unit pemilik]
        D --> E{Setuju?}
        E -- Tidak --> R[(Ditolak)]
        E -- Ya --> F[(Disetujui)]
    end
    subgraph peminjam2[Kepala Unit peminjam]
        F --> G[Ambil dan konfirmasi terima]
        G --> H[(Dipinjam)]
        H --> I{Masih butuh lewat tanggal kembali?}
        I -- Ya --> J[Minta perpanjangan]
        J --> K{Pemilik setuju?}
        K -- Ya --> H
        K -- Tidak --> L[Kembalikan]
        I -- Tidak --> L
        L --> M[(MenungguKonfirmasiKembali)]
    end
    subgraph pemilik2[Kepala Unit pemilik]
        M --> N[Periksa dan isi kondisi tiap aset]
        N --> O{Kondisi berubah?}
        O -- Ya --> O1[Sistem membuka laporan kondisi]
        O1 --> P[(Dikembalikan)]
        O -- Tidak --> P
    end
    P --> Z([Kunci lepas; aset kembali dipakai pemilik])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Ajukan | Kepala Unit peminjam atau Petugas Aset | Aset, unit peminjam, tujuan, tanggal mulai dan kembali | `Diajukan` (langsung `Disetujui` bila diajukan Kepala Unit pemilik) | — |
| Setujui | Kepala Unit pemilik | — | `Disetujui` | Menolak dengan alasan |
| Ambil | Kepala Unit peminjam | — | `Dipinjam` | Pengguna lain mendapat penolakan hak |
| Terlambat | Sistem (dihitung saat dibaca) | Tanggal kembali lewat | Tanda Terlambat di daftar dan dashboard | Peminjam meminta perpanjangan atau mengembalikan |
| Kembalikan dan konfirmasi | Peminjam lalu pemilik | Kondisi per aset | `Dikembalikan`; laporan kondisi bila berubah | Kondisi kosong → lengkapi |

**Contoh.** ICU meminjam syringe pump `AST-ALKMED-000310` milik IGD 1–3 Juni. Pada 4 Juni pinjaman tampil
Terlambat. ICU meminta perpanjangan sampai 6 Juni, Kepala IGD setuju, tanda Terlambat hilang. Pada 6 Juni
pompa kembali dengan kabel putus; Kepala IGD mengisi kondisi Rusak Ringan dan sistem membuka laporan
kondisi yang kemudian ditetapkan Petugas Aset.
