# Proses — Perbandingan, Negosiasi, Penetapan, dan Nomor PO

```mermaid
flowchart TD
    subgraph TP[Tim Pengadaan]
        A[Paket EVALUATION] --> B[Lihat perbandingan harga akhir dan peringkat]
        B --> C{Harga terbaik di atas HPS?}
        C -- Ya --> D[Negosiasi dengan vendor]
        D --> C
        C -- Tidak --> E{Memilih vendor terendah?}
        E -- Tidak --> F[Tulis alasan memilih]
        E -- Ya --> G[Ajukan penetapan]
        F --> G
        H[Tulis alasan melebihi HPS] --> G
        C -- Tetap di atas --> H
        G --> I{Vendor masih layak saat diajukan?}
        I -- Tidak --> J[Pilih vendor lain] --> B
        I -- Ya --> K[Paket AWARD_PENDING_APPROVAL]
        R[Catat nomor PO dari BLM] --> S{Nomor sudah dipakai paket lain?}
        S -- Ya --> T[Periksa nomor di BLM] --> R
        S -- Tidak --> U[Paket ORDERED]
        W[Cabut penetapan dengan alasan] --> A
    end
    subgraph PY[Penyetuju]
        K --> L{Keputusan}
    end
    L -- Dikembalikan atau ditolak --> A
    L -- Disetujui --> M{Vendor masih layak saat diterapkan?}
    M -- Tidak --> N[Penetapan tidak dapat diterapkan, tanpa komitmen] --> A
    M -- Ya --> O[Paket AWARDED dan komitmen pagu per unit tercatat]
    O --> R
    O -.-> W
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Bandingkan | Tim Pengadaan | Penawaran sah dan lulus | Peringkat harga akhir termasuk PPN | Jumlah penawaran sah kurang dari minimum → RFQ ulang |
| Negosiasi | Tim Pengadaan | Berita acara | Harga baru per barang | Harga baru lebih tinggi → ditolak |
| Ajukan penetapan | Tim Pengadaan | Vendor, alasan bila bukan terendah atau di atas HPS | Paket `AWARD_PENDING_APPROVAL` | Vendor tidak layak → pilih vendor lain atau tunggu dokumen vendor diverifikasi |
| Putuskan | Penyetuju | Vendor, nilai, peringkat, alasan | Disetujui / dikembalikan / ditolak | Dikembalikan → ubah pilihan atau alasan |
| Terapkan | Sistem | Keputusan disetujui | Paket `AWARDED`; komitmen per unit, kategori, sumber dana | Vendor tidak layak atau pagu belum ada → paket kembali ke evaluasi / sinkron ulang setelah pagu disusun |
| Catat PO | Tim Pengadaan | Nomor PO BLM | Paket `ORDERED`; diserahkan ke kontrak-pelaksanaan | Nomor ganda → periksa ulang |
| Cabut | Manajer Pengadaan | Alasan | Paket `EVALUATION`; komitmen dilepas | Nomor PO sudah dicatat → tidak bisa dicabut |
