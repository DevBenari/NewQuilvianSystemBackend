# Proses: Permintaan ke PMI dan Penerimaan Kantong — dengan jalur pengecualian

```mermaid
flowchart TD
    subgraph bdrs[Petugas Bank Darah]
        A([Order aktif]) --> B{Sudah ada permintaan berjalan untuk kebutuhan sama?}
        B -- Ya --> B1[/Ditolak, tak boleh permintaan ganda/]
        B -- Tidak --> C[Buat permintaan]
        C --> D[(Permintaan Requested)]
        D --> E[Teruskan ke PMI secara manual]
        E --> F[Kantong datang, catat penerimaan fisik]
    end
    subgraph sistem[Sistem]
        F --> G{Jumlah diterima dibanding diminta}
        G -- Kurang --> H[(Permintaan PartiallyFulfilled)]
        H --> F
        G -- Sama --> I[(Permintaan Fulfilled)]
        G -- Lebih --> J[Kantong pas dicatat, sisa berhenti di 0]
        J --> K[(Permintaan Fulfilled)]
        J --> L[Kantong berlebih ditandai, masuk PendingReview]
        F --> M{Kunjungan berakhir saat masih kurang?}
        M -- Ya --> N[(Permintaan ClosedEncounter)]
        N --> O{Kantong susulan tetap datang?}
        O -- Ya --> P[Penerimaan tetap dicatat, kantong ke PendingReview]
    end
    I --> Q([Stok operasional bertambah])
    K --> Q
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Periksa permintaan ganda | Sistem | Kebutuhan order | Lolos | Ditolak; pakai permintaan yang sudah berjalan |
| Buat permintaan | Petugas | Order aktif | Permintaan `Requested` | — |
| Catat penerimaan | Petugas | Kantong fisik | Stok bertambah | — |
| Bandingkan diterima vs diminta | Sistem | Jumlah | Partial / Fulfilled / kelebihan | Sisa tak pernah negatif; kelebihan tetap dicatat lalu `PendingReview` |
| Kunjungan berakhir | Sistem | Sinyal kunjungan | `ClosedEncounter` | Kantong susulan tetap dicatat, masuk `PendingReview` |

Kekurangan pengiriman **tidak** melahirkan permintaan baru; pengiriman berikutnya menambah ke
permintaan yang sama.
