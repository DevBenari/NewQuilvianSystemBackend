# Proses: Penyelesaian Kantong Menunggu Keputusan — dengan jalur pengecualian

Kantong masuk `PendingReview` ketika ordernya berakhir, ketika datang berlebih, atau ketika datang
susulan setelah kunjungan berakhir. Ia **tidak pernah** menjadi stok bebas.

```mermaid
flowchart TD
    subgraph sistem[Sistem]
        A([Order berakhir / kantong berlebih / susulan]) --> B[(Kantong PendingReview)]
    end
    subgraph bdrs[Petugas berwenang]
        B --> C{Keputusan kelayakan oleh manusia}
        C -- Layak untuk pasien lain --> D[Alihkan, alasan wajib]
        D --> E[Bukti kecocokan pasien asal gugur otomatis]
        E --> F[(Kantong Reallocated)]
        C -- Dikembalikan --> G{Proses PMI mendukung pengembalian?}
        G -- Ya --> H[(Kantong ReturnedToProvider)]
        G -- Belum diketahui --> G1[/Tahan, tetap PendingReview/]
        C -- Tidak layak --> I[Nyatakan tidak layak, alasan wajib]
        I --> J[(Kantong NotUsable)]
    end
    F --> K([Pasien tujuan wajib punya bukti kecocokan sendiri])
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Kantong masuk menunggu keputusan | Sistem | Order berakhir / berlebih / susulan | Kantong `PendingReview` | — |
| Alihkan ke pasien lain | Petugas berwenang | Pernyataan kelayakan, alasan | Kantong `Reallocated`; bukti asal gugur | Ditolak tanpa alasan; rantai pasien asal→tujuan wajib tersimpan |
| Kembalikan ke PMI | Petugas berwenang | Proses PMI mendukung | Kantong `ReturnedToProvider` | Ditahan bila kesediaan PMI belum diketahui |
| Nyatakan tidak layak | Petugas berwenang | Alasan | Kantong `NotUsable` | Ditolak tanpa alasan |

Setelah dialihkan, pemberian ke pasien tujuan menuntut bukti kecocokan **baru** terhadap pasien itu,
walaupun golongan darahnya kebetulan sama. Sistem tidak pernah menyimpulkan bukti lama masih cocok.
