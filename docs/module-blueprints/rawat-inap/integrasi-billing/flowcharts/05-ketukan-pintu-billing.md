# Flowchart — Ketukan pintu Rawat Inap ke Billing

| Field | Nilai |
|---|---|
| Sub-modul | `integrasi-billing` |
| Kontrak | `1.1.0` — `draft` |
| Proses | `BP-RWF-01` |
| Keputusan | `RWI-DEC-161`, `166`, `192` |

```mermaid
flowchart TD
    subgraph rawatinap[Petugas Rawat Inap]
        A([Admisi, tempati bed, pindah, koreksi, atau keluar ruangan]) --> B[Simpan perubahan]
    end
    subgraph sistem[Sistem Rawat Inap]
        B --> C[(Pending)]
        C --> D[Ambil pesan untuk dikirim]
        D --> E[(Processing)]
        E --> E1{Aplikasi mati sebelum ada jawaban?}
        E1 -- Ya, masa sewa habis --> C
        E1 -- Tidak --> F{Billing menjawab diterima?}
        F -- Ya --> G[(Published)]
        F -- Tidak atau tidak menjawab --> H[(Failed)]
        H --> H1{Sudah gagal sepuluh kali?}
        H1 -- Belum --> H2[Tunggu jeda coba ulang]
        H2 --> D
        H1 -- Sudah --> I[(DeadLetter)]
    end
    subgraph billing[Sistem Billing]
        F -.-> J{Pesan ini pernah diterima?}
        J -- Ya --> J1[Jawab diterima tanpa efek kedua]
        J -- Tidak --> K{Jenis kejadian}
        K -- Admisi --> L[Buka invoice rawat inap bila belum ada]
        K -- Bed --> M[Hitung ulang tarif kamar]
    end
    subgraph ti[Tim TI]
        I --> N[Periksa pesan gagal di pemantauan outbox]
        N --> O([Perbaiki penyebab, lalu putar ulang dengan wewenang tertulis])
    end
    G --> P([Invoice dan tarif kamar mutakhir])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Simpan perubahan | Petugas Rawat Inap | Tindakan bisnis | Data tersimpan dan pesan `Pending` | Penyimpanan gagal: tidak ada pesan |
| Kirim | Sistem | Pesan `Pending` atau `Failed` jatuh tempo | `Processing` | Aplikasi mati: diambil ulang setelah masa sewa |
| Terima | Sistem Billing | Penanda kejadian | Invoice dibuka atau tarif kamar dihitung ulang; tanda terima | Billing gangguan: pesan `Failed` dan dicoba ulang |
| Tandai terkirim | Sistem | Tanda terima diterima | `Published` | Tanpa tanda terima, pesan tidak pernah `Published` |
| Tangani pesan gagal | Tim TI | `DeadLetter` | Penyebab diperbaiki | Kasir tetap dapat bekerja pada invoice yang sudah ada |
