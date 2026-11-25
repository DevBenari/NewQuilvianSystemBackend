# Pergerakan Aset — Alur Utama

Jalur normal mutasi aset dari satu unit ke unit lain. Peminjaman ada di `02-peminjaman.md`.

```mermaid
flowchart TD
    subgraph asal[Unit asal]
        A([Aset perlu pindah unit]) --> B[Ajukan mutasi]
        B --> C[(Diajukan)]
        C --> D[Kepala Unit asal menyetujui]
        D --> E[(DisetujuiUnitAsal)]
    end
    subgraph bagian[Kepala Bagian Aset]
        E --> F[Setujui]
        F --> G[(DisetujuiBagianAset)]
    end
    subgraph tujuan[Kepala Unit tujuan]
        G --> H[Terima aset, isi ruangan dan penanggung jawab]
        H --> I[(Diserahterimakan)]
    end
    I --> J([Aset tercatat di unit tujuan; berita acara serah terima terbit])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Ajukan | Petugas Aset atau Kepala Unit asal | Aset dari satu unit, unit tujuan, alasan | `Diajukan` (atau langsung `DisetujuiUnitAsal` bila diajukan Kepala Unit asal); aset terkunci | Mengganti aset yang sedang dalam proses lain |
| Setujui unit | Kepala Unit asal (bukan pengaju) | — | `DisetujuiUnitAsal` | Menolak → `Ditolak` |
| Setujui Bagian Aset | Kepala Bagian Aset (bukan pengaju) | — | `DisetujuiBagianAset` | Menolak → `Ditolak` |
| Terima | Kepala Unit tujuan | Ruangan, penanggung jawab, kondisi per aset | `Diserahterimakan`; penempatan berpindah | Menolak terima → `Ditolak`, aset tetap di unit asal |
