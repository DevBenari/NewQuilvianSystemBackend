# Proses — Permintaan dan Disposisi (FEAT-001..003)

```mermaid
flowchart TD
    subgraph admisi[Petugas admisi rehab]
        A([Rujukan masuk]) --> B[Pilih pasien]
        B --> C{Asal rujukan?}
        C -- Internal RJ/RWI/IGD --> D[Pilih kunjungan asal yang masih terbuka]
        C -- Rujukan luar --> E[Isi fasilitas dan perujuk]
        D --> F[Isi indikasi dan layanan yang diminta]
        E --> F
        F --> G[(Permintaan Draft)]
        G --> H[Ajukan]
    end
    subgraph sistem[Sistem]
        H --> I{Isian lengkap?}
        I -- Tidak --> I1[/Ditolak, isian kurang ditandai/]
        I1 --> F
        I -- Ya --> J{Ada permintaan/episode berjalan?}
        J -- Ya, tanpa alasan --> J1[/Ditolak, daftar episode yang mirip ditampilkan/]
        J1 --> K{Lanjutkan?}
        K -- Isi alasan --> H
        K -- Tidak --> X1[Batalkan permintaan]
        X1 --> X2([Permintaan Cancelled])
        J -- Tidak / alasan ada --> L[(Permintaan Submitted)]
    end
    subgraph admisi2[Petugas admisi rehab]
        L --> M{Kelengkapan administratif?}
        M -- Kurang --> M1[Kembalikan dengan alasan]
        M1 --> N[(Permintaan NeedsClarification)]
        N --> F
    end
    subgraph kfr[Dokter Sp.KFR]
        M -- Lengkap --> O{Keputusan klinis}
        O -- Perlu klarifikasi --> M1
        O -- Alihkan --> P([Permintaan Redirected])
        O -- Tidak ada indikasi --> Q([Permintaan Rejected])
        O -- Terima --> R[Pilih dokter penanggung jawab]
        R --> S([Permintaan Accepted, Episode AwaitingMedicalAssessment])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih kunjungan asal | Admisi | Pasien | Kunjungan terbuka milik pasien | Kunjungan tidak muncul: minta pendaftaran memeriksa status kunjungan |
| Ajukan | Admisi | Draft | `Submitted` | Isian kurang → lengkapi. Duplikasi → isi alasan atau batalkan. |
| Kembalikan | Admisi | Alasan administratif | `NeedsClarification` | — |
| Keputusan klinis | Sp.KFR | Permintaan | `Accepted` / `Redirected` / `Rejected` / `NeedsClarification` | Admisi tidak bisa menolak secara klinis (tombol tidak muncul, dan server menolak) |
| Terima | Sp.KFR | Dokter penanggung jawab | Episode baru | Bila dokter tidak aktif, pilih dokter lain |
