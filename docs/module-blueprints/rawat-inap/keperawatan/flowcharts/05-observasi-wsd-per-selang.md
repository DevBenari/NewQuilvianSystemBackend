# Flowchart — Observasi pengeluaran cairan WSD per selang

| Field | Nilai |
|---|---|
| Sub-modul | `keperawatan` |
| Kontrak | `0.6.0` — `draft` |
| Proses | `BP-RWF-06` |
| Keputusan | `RWI-DEC-172`, `RWI-DEC-200` |

```mermaid
flowchart TD
    subgraph perawat[Perawat shift]
        A([Pasien terpasang WSD]) --> B{Selang sudah terdaftar?}
        B -- Belum --> C[Daftarkan selang dengan lokasi dan sisa awal]
        C --> D[(Active)]
        B -- Sudah --> D
        D --> E[Pilih selang, isi jam awal, jam akhir, sisa sekarang, volume dibuang]
    end
    subgraph sistem[Sistem]
        E --> F{Selang sudah dilepas?}
        F -- Ya --> F1[/Ditolak, selang sudah dilepas/]
        F -- Tidak --> G[Ambil sisa terakhir selang yang sama atau sisa awal]
        G --> H{Hasil bertambah negatif?}
        H -- Ya --> H1[/Ditolak, periksa volume dibuang/]
        H1 --> E
        H -- Tidak --> I[Simpan pembacaan dan catat output cairan]
    end
    subgraph koreksi[Perawat bila salah ketik]
        I --> J{Ada salah ketik?}
        J -- Ya, pembacaan terakhir --> K[Koreksi dengan alasan]
        J -- Ya, bukan terakhir --> K1[/Ditolak, hanya pembacaan terakhir/]
        K --> I
    end
    J -- Tidak --> L{Selang dilepas?}
    L -- Ya --> M[(Removed)]
    L -- Tidak --> N([Pembacaan shift berikutnya])
    F1 --> N
    K1 --> N
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Daftarkan selang | Perawat | Label, lokasi, waktu pasang, sisa awal | Selang `Active` | Label sama masih aktif: pakai label lain |
| Catat pembacaan | Perawat | Jam awal, jam akhir, sisa sekarang, volume dibuang | Bertambah dihitung server; output cairan | Negatif: periksa volume dibuang; selang dilepas: tidak dapat dicatat |
| Koreksi | Perawat | Alasan | Revisi pembacaan dan entri cairan | Bukan pembacaan terakhir: ditolak |
| Lepas selang | Perawat | Waktu lepas | Selang `Removed` | Waktu lepas sebelum pembacaan terakhir: ditolak |
