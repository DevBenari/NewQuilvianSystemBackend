# Flowchart — Serah terima klinis saat transfer antarunit (`P2`)

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` (data milik `ClinicalManagement`) |
| Kontrak | `0.10.0` — `draft` |
| Proses | `BP-RWF-07` |
| Keputusan | `RWI-DEC-182`, `189` |

```mermaid
flowchart TD
    subgraph asal[Perawat unit asal]
        A([Transfer pasien ke tempat tidur lain]) --> B{Unit tujuan berbeda?}
        B -- Tidak --> B1([Tidak ada dokumen serah terima])
        B -- Ya --> C[Bed langsung pindah]
        C --> D[Dokumen serah terima belum dikirim lahir]
        D --> E[Lengkapi kondisi, barang, instruksi]
        E --> F[Kirim; nilai klinis dibekukan]
    end
    subgraph tujuan[Perawat unit tujuan]
        F --> G{Penerima bukan pengirim dan pasien di unit ini?}
        G -- Tidak --> G1[/Tidak dapat menerima/]
        G -- Ya --> H{Isi memadai?}
        H -- Tidak --> I[Tolak dengan alasan]
        I --> E
        H -- Ya --> J([Diterima, penanda tertunda hilang])
    end
    subgraph penanda[Kedua unit]
        D --> K[Penanda serah terima tertunda tampil]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Transfer | Perawat atau kepala ruangan | Bed tujuan | Bed pindah | Aturan transfer yang sudah ada |
| Dokumen lahir | Sistem | Transfer antarunit | Dokumen belum dikirim | Gagal dibuat: transfer tetap sah; dokumen dibuat ulang dari Daftar Pantau |
| Kirim | Perawat unit asal | Isian dan nilai klinis terakhir | Dokumen terkirim | — |
| Terima atau tolak | Perawat unit tujuan | Dokumen | Diterima atau ditolak | Akun sama atau pasien bukan di unit itu: minta perawat unit tujuan |

Dokumen ini tidak pernah menahan transfer, keluar ruangan, atau penutupan episode.
