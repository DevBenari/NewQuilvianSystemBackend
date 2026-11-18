# Penempatan dan Penyerahan Aset

**Tujuan:** menetapkan unit, ruangan, dan penanggung jawab dengan bukti penerimaan.
**Pemicu:** aset baru terdaftar, pindah ruangan/penanggung jawab di unit yang sama, koreksi hasil opname
beda ruangan, atau aset dipulihkan. **Prasyarat:** aset `Terdaftar`, bukan `Hilang`, tanpa proses aktif,
unit tidak sedang opname. **Hasil akhir:** penempatan berjalan baru dan Berita Acara Penyerahan.
Perpindahan **antar unit** bukan di sini, melainkan mutasi (`pergerakan`).

```mermaid
flowchart TD
    subgraph petugas[Petugas Aset]
        A([Aset perlu ditempatkan]) --> B[Pilih aset, unit, ruangan, penanggung jawab]
        B --> C{Aset boleh diserahkan?}
        C -- Tidak --> C1[/Ditolak, aset sedang dalam proses lain atau hilang/]
        C1 --> Z([Selesaikan proses lain dahulu])
        C -- Ya --> D{Unit sedang opname?}
        D -- Ya --> D1[/Ditolak, tunggu opname selesai/]
        D1 --> Z
        D -- Tidak --> E[(Diserahkan)]
        E --> X[Batalkan bila salah tujuan]
        X --> X1[(Dibatalkan)]
    end
    subgraph penerima[Kepala Unit atau penanggung jawab tujuan]
        E --> F{Barang diterima sesuai?}
        F -- Tidak --> F1[Tolak terima dengan alasan]
        F1 --> F2[(Ditolak)]
        F2 --> B
        F -- Ya --> G[Konfirmasi]
        G --> H[(Dikonfirmasi)]
    end
    H --> I([Penempatan berlaku; berita acara dapat dicetak])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Serahkan | Petugas Aset | Daftar aset, tujuan, keperluan (penempatan awal / pindah dalam unit / koreksi opname / pasca pemulihan) | Penyerahan `Diserahkan`, kunci `Penyerahan` | Bila aset terkunci: selesaikan proses pemegang kunci |
| Konfirmasi | Kepala Unit tujuan atau penanggung jawab yang dituju | — | Penempatan lama ditutup, baru dibuka, kunci lepas | Pengguna lain mendapat penolakan hak |
| Tolak terima | Penerima | Alasan | Penyerahan `Ditolak`, kunci lepas, penempatan tidak berubah | Petugas Aset membuat penyerahan baru |
| Batalkan | Petugas Aset | Alasan | `Dibatalkan`, kunci lepas | — |

**Contoh.** Lima kursi roda diserahkan ke unit Rawat Jalan, ruang tunggu Poli Anak, penanggung jawab
Kepala Ruang Poli Anak. Sampai konfirmasi, kelimanya masih `BelumDitempatkan` di gudang Bagian Aset.
