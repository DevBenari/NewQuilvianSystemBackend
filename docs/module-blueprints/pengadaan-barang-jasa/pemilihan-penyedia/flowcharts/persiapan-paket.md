# Proses — Pembentukan dan Persiapan Paket

Mencakup pembentukan paket dari baris PR, persiapan, persetujuan persiapan, pecah paket, dan pembatalan sebelum RFQ.

```mermaid
flowchart TD
    subgraph TP[Tim Pengadaan]
        A[Pilih baris PR disetujui] --> B{Baris masih bebas, satu jenis, satu tahun?}
        B -- Tidak --> A
        B -- Ya --> C[Paket DRAFT]
        C --> D[Isi spesifikasi dan HPS per barang]
        D --> E{Metode yang dipilih sesuai rentang HPS?}
        E -- Tidak --> F[Tulis alasan memakai metode itu]
        E -- Ya --> G{Dokumen wajib lengkap?}
        F --> G
        G -- Tidak --> H[Unggah dokumen wajib] --> G
        G -- Ya --> I{Alur persetujuan sudah dikonfigurasi?}
        I -- Tidak --> J[Hubungi administrator, paket tetap DRAFT]
        I -- Ya --> K[Paket PREPARATION_PENDING_APPROVAL]
        R[Perbaiki persiapan] --> K
        X[Batalkan paket dengan alasan] --> Y[Paket CANCELLED, baris PR dilepas]
        S[Pecah sebagian barang ke paket baru] --> T[Dua paket dengan metode yang sama]
    end
    subgraph PY[Penyetuju]
        K --> L{Keputusan}
    end
    L -- Disetujui --> M[Paket READY_FOR_RFQ]
    L -- Dikembalikan --> N[Paket PREPARATION_RETURNED] --> R
    L -- Ditolak --> O[Paket REJECTED, baris PR dilepas]
    C -.-> X
    M -.-> X
    C -.-> S
    M -.-> S
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih baris | Tim Pengadaan | Daftar baris PR siap paket | Paket `DRAFT` | Baris sudah dipaketkan petugas lain → muat ulang daftar |
| Isi HPS | Tim Pengadaan | Harga perkiraan PR (termasuk PPN), dasar harga | HPS per barang | HPS kosong → pengajuan ditolak |
| Pilih metode | Tim Pengadaan | Saran sistem dari HPS | Metode tersalin ke paket | Di luar rentang tanpa alasan → isi alasan |
| Lengkapi checklist | Tim Pengadaan | Dokumen KAK/TOR, dasar HPS, dll. | Checklist terpenuhi | Jenis dokumen tidak ada di daftar → minta admin menambah jenisnya |
| Ajukan | Tim Pengadaan | Persiapan lengkap | `PREPARATION_PENDING_APPROVAL` | Alur belum dikonfigurasi → admin `Wfl` mengisi matriks |
| Putuskan | Penyetuju | HPS, metode, alasan | `READY_FOR_RFQ` / `PREPARATION_RETURNED` / `REJECTED` | Dikembalikan → perbaiki sesuai catatan |
| Pecah | Tim Pengadaan | Barang yang dipindah, alasan | Paket baru berstatus sama, metode sama | Sedang menunggu persetujuan → tarik dulu |
| Batal | Tim Pengadaan | Alasan | `CANCELLED`; baris PR kembali bebas | Sedang menunggu persetujuan → tarik dulu |
