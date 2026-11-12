# Pemulihan Aset

**Tujuan:** aset yang sudah dihapus final tetapi ternyata ada (ditemukan kembali atau keliru dihapus)
kembali ke register tanpa membuka ulang usulan. **Pemicu:** aset hilang ditemukan; kesalahan penghapusan.
**Prasyarat:** butir dari usulan `Final`; untuk aset yang sudah dijual/dihibahkan/ditukar, berita acara
pembatalan pelepasan. **Hasil akhir:** aset `Terdaftar` kondisi `Aktif` dan `BelumDitempatkan`; buku nilai aktif
kembali; kejadian balik ke Akuntansi.

```mermaid
flowchart TD
    subgraph petugas[Petugas Aset]
        A([Aset yang dihapus ternyata ada]) --> B{Butir dari usulan final?}
        B -- Tidak --> B1[/Ditolak/]
        B -- Ya --> C{Sudah dijual, dihibahkan, atau ditukar?}
        C -- Ya --> D{Berita acara pembatalan terlampir?}
        D -- Tidak --> D1[/Ditolak, lampirkan berita acara pembatalan/]
        D1 --> D
        D -- Ya --> E[(Diajukan)]
        C -- Tidak --> E
    end
    subgraph kepala[Kepala Bagian Aset]
        E --> F{Pengaju adalah saya?}
        F -- Ya --> F1[/Ditolak, tidak boleh menyetujui sendiri/]
        F -- Tidak --> G{Setuju?}
        G -- Tidak --> H[(Ditolak)]
        G -- Ya --> I[(Disetujui)]
    end
    I --> J([Aset kembali; Petugas Aset menyerahkannya lagi ke unit])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Ajukan | Petugas Aset | Butir final, alasan, bukti | `Diajukan` | Lampirkan berita acara pembatalan bila wajib |
| Putuskan | Kepala Bagian Aset bukan pengaju | — | Aset dan buku aktif; kejadian `AST-PEMULIHAN`; penyesuaian penyusutan bulan yang terlewat pada proses berikutnya | Ditolak dengan alasan |
| Tempatkan ulang | Petugas Aset | Penyerahan tujuan `PascaPemulihan` (`register`) | Aset ditempatkan | — |

**Contoh.** Laptop `AST-TI-000077` dihapus final 22 Agustus karena hilang (nilai buku Rp 4.200.000). Pada
10 Oktober laptop ditemukan. Pemulihan disetujui 11 Oktober: aset `Terdaftar`, buku aktif dengan nilai buku
Rp 4.200.000, kejadian `AST-PEMULIHAN` membalik rugi kehilangan. Proses penyusutan Oktober memuat
penyesuaian Agustus dan September ditambah Oktober.
