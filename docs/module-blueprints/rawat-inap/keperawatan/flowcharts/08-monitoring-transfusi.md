# Flowchart — Monitoring transfusi per kantong

| Field | Nilai |
|---|---|
| Sub-modul | `keperawatan` |
| Kontrak | `0.6.0` — `draft` |
| Keputusan | `RWI-DEC-203` |
| Gerbang | ~~`RWI-OQ-115`~~ — disetujui `RWI-DEC-209` |

```mermaid
flowchart TD
    subgraph bankdarah[Bank Darah]
        A([Kantong diserahkan untuk pasien])
    end
    subgraph perawat[Perawat bangsal]
        A --> B[Pilih kantong dari daftar, isi jam diterima dan jam mulai]
        B --> C{Kantong sudah diserahkan untuk pasien ini dan belum dipantau?}
        C -- Tidak --> C1[/Ditolak/]
        C -- Ya --> D[(InProgress)]
        D --> E[Catat titik ukur: sebelum, 15 menit, 1 jam, 4 jam]
        E --> F{Dicatat melewati jatuh tempo?}
        F -- Ya, tanpa keterangan --> F1[/Ditolak, tulis keterangan/]
        F1 --> E
        F -- Tidak, atau dengan keterangan --> G{Ada reaksi?}
        G -- Ya --> H[Catat reaksi]
        G -- Tidak --> I{Semua titik selesai?}
        H --> J{Transfusi dihentikan?}
        J -- Ya --> K[(Stopped)]
        J -- Tidak --> I
        I -- Belum --> E
        I -- Sudah --> L[(Completed)]
    end
    subgraph sistem[Sistem]
        H --> M[Kirim pemberitahuan reaksi ke Bank Darah]
        M --> N{Terkirim?}
        N -- Tidak --> N1[Dicoba ulang otomatis]
        N1 --> M
    end
    subgraph bdr[Petugas Bank Darah]
        N -- Ya --> O[Terima dan tindak lanjuti menurut prosedur Bank Darah]
    end
    K --> P([Monitoring tersimpan])
    L --> P
    O --> P
    C1 --> P
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih kantong | Perawat | Kantong dari daftar Bank Darah | Monitoring `InProgress`, empat titik terjadwal | Kantong bukan untuk pasien ini atau sudah dipantau: ditolak |
| Catat titik | Perawat | Tekanan darah, suhu, nadi | Titik tercatat | Terlambat tanpa keterangan: ditolak |
| Catat reaksi | Perawat | Ringkasan reaksi | Reaksi dan pemberitahuan Bank Darah | Gagal kirim: dicoba ulang tiap menit |
| Hentikan atau selesai | Perawat | Alasan bila dihentikan | `Stopped` atau `Completed` | Titik 4 jam belum ada: belum dapat selesai |
| Terima pemberitahuan | Petugas Bank Darah | Pemberitahuan baru | Diterima | — |
