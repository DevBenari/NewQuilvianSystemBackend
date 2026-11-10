# Proses — Reasesmen dan Penutupan Episode (FEAT-024..027)

```mermaid
flowchart TD
    subgraph terapis[Terapis / Sp.KFR]
        A([Tanggal evaluasi tiba atau kondisi berubah]) --> B[Ukur ulang dengan instrumen yang sama dengan baseline]
        B --> C{Instrumen sama?}
        C -- Tidak --> C1[Jelaskan alasan pergantian instrumen]
        C1 --> D
        C -- Ya --> D[Nilai capaian setiap tujuan]
        D --> E{Keputusan}
        E -- Lanjut --> F([Program tetap Active])
        E -- Modifikasi --> G[Program ditandai perlu revisi]
        G --> G1[Susun versi program baru lalu aktifkan]
        E -- Hentikan / Rujuk / Selesai --> H[(Program Stopped atau Completed)]
    end
    subgraph kfr[Dokter Sp.KFR]
        H --> I{Masih ada layanan lain berjalan?}
        I -- Ya --> F
        I -- Tidak --> J[Susun ringkasan akhir: alasan, capaian, kondisi akhir, tindak lanjut]
        J --> K[Tanda tangan]
    end
    subgraph sistem[Sistem]
        K --> L{Semua catatan episode sudah final?}
        L -- Belum --> L1[/Ditolak, daftar catatan draft ditampilkan/]
        L1 --> M[Penulis menyelesaikan atau membatalkan draft]
        M --> K
        L -- Sudah --> N([Episode Closed])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Ukur ulang | Terapis/Sp.KFR | Baseline | Pengukuran kini | Instrumen berbeda: isi alasan pergantian |
| Keputusan | Terapis/Sp.KFR | Capaian | Lanjut/Modifikasi/Henti/Rujuk/Selesai | Data belum lengkap: simpan draft |
| Ringkasan akhir | Sp.KFR | Semua catatan final | Episode `Closed` | Ada catatan draft: penulisnya menyelesaikan atau membatalkan |
| Putus layanan | Sp.KFR | Riwayat tidak hadir | Ditutup dengan alasan "putus layanan", **bukan** "selesai klinis" | — |
