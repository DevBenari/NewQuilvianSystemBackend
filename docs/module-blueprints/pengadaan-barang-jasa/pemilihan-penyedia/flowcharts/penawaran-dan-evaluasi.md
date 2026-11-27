# Proses — RFQ, Penawaran, dan Evaluasi Teknis

```mermaid
flowchart TD
    subgraph TP[Tim Pengadaan]
        A[Paket READY_FOR_RFQ] --> B[Pilih vendor yang layak untuk semua kategori]
        B --> C{Vendor dibatasi?}
        C -- Ya --> D[Tulis alasan mengundang]
        C -- Tidak --> E[Tentukan batas waktu]
        D --> E
        E --> F[Kirim RFQ, paket RFQ_OPEN]
        F --> G[Kirim dokumen RFQ ke vendor lewat email]
        G --> H[Catat penawaran dan waktu diterima]
        H --> I{Diterima setelah batas waktu?}
        I -- Ya --> J[Tercatat terlambat, tidak sah]
        I -- Tidak --> K[Tercatat sah]
        P[Perpanjang batas waktu sebelum lewat] --> G
        Q[Tutup RFQ, paket EVALUATION]
        V[Nyatakan RFQ gagal] --> W{Ganti metode?}
        W -- Tidak --> X[Paket READY_FOR_RFQ, kirim RFQ ulang]
        W -- Ya --> Y[Buka ulang persiapan, paket DRAFT]
    end
    J --> Q
    K --> Q
    subgraph EV[Evaluator teknis]
        Q --> R{Metode mewajibkan evaluasi teknis?}
        R -- Ya --> S[Nilai setiap kriteria, alasan bila tidak lulus]
    end
    R -- Tidak --> T{Penawaran sah cukup?}
    S --> T
    T -- Tidak --> V
    T -- Ya --> U[Lanjut perbandingan dan negosiasi]
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih vendor | Tim Pengadaan | Daftar vendor layak per kategori paket | Undangan | Vendor tidak layak tidak tampil; vendor dibatasi butuh alasan |
| Kirim RFQ | Tim Pengadaan | Undangan, batas waktu | Nomor RFQ; paket `RFQ_OPEN` | Batas waktu lampau → ubah batas waktu |
| Catat penawaran | Tim Pengadaan | Berkas penawaran, PPN vendor | Penawaran tercatat dan harga akhir | Vendor tidak diundang → tidak bisa dicatat |
| Perpanjang | Tim Pengadaan | Batas waktu baru, alasan | Batas waktu baru untuk semua vendor | Sudah lewat → tidak bisa diperpanjang; nyatakan gagal atau tutup |
| Tutup RFQ | Tim Pengadaan | — | Paket `EVALUATION` | Batas waktu belum lewat dan masih ada vendor belum membalas → tunggu |
| Evaluasi | Evaluator | Data teknis tanpa harga | Lulus / Tidak lulus per kriteria | Evaluator mencatat penawaran itu sendiri → tugaskan evaluator lain |
| RFQ gagal | Tim Pengadaan | Alasan | RFQ ulang atau persiapan ulang | — |
