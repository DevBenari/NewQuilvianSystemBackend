# Flowchart — Surveilans infeksi luka operasi

| Field | Nilai |
|---|---|
| Sub-modul | `keperawatan` |
| Kontrak | `0.6.0` — `draft` |
| Proses | `BP-RWF-08` langkah 6–8 |
| Keputusan | `RWI-DEC-202` |

```mermaid
flowchart TD
    subgraph sistem[Sistem]
        A([Kasus operasi pasien rawat inap selesai]) --> B{Formulir surveilans sudah disahkan?}
        B -- Belum --> B1[Peringatan di Daftar Pantau, formulir tidak dibentuk]
        B -- Sudah --> C[(Active)]
    end
    subgraph perawat[Perawat bangsal]
        C --> D[Buka hari ke-N]
        D --> E{Pasien sudah keluar ruangan?}
        E -- Ya --> E1[/Ditolak, surveilans berhenti/]
        E -- Tidak --> F[Isi indikator dan tanda per lokasi; suhu terbaca dari tanda vital]
        F --> G{Mengubah isian lama?}
        G -- Ya, tanpa alasan --> G1[/Ditolak, alasan wajib/]
        G1 --> F
        G -- Tidak, atau dengan alasan --> H[Simpan isian]
    end
    subgraph ppi[Tim PPI]
        H --> I{Ada tanda infeksi luka operasi?}
        I -- Ya --> J[Tandai dicurigai]
        J --> K[Kejadian infeksi tercatat untuk ditindaklanjuti PPI]
        I -- Tidak --> L[Lanjut pemantauan]
    end
    subgraph akhir[Sistem]
        L --> M{Hari ke-15 terlewati atau pasien keluar?}
        K --> M
        M -- Keluar sebelum hari ke-15 --> N[(StoppedOnDeparture)]
        M -- Hari ke-15 terlewati --> O[(Completed)]
        M -- Belum --> D
    end
    N --> P([Formulir tetap tampil di daftar PPI])
    O --> P
    E1 --> P
    B1 --> Q([Pemilik klinis mengesahkan formulir])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Bentuk formulir | Sistem | Kasus OK selesai, versi formulir disahkan | Formulir `Active`, hari ke-1 esok hari | Belum disahkan: peringatan di Daftar Pantau |
| Isi harian | Perawat | Indikator, tanda per lokasi | Isian hari ke-N; suhu dari tanda vital | Pasien pulang: formulir berhenti; ubah tanpa alasan: ditolak |
| Tandai dicurigai | Tim PPI | Tanggal awal gejala, catatan | Kejadian infeksi berstatus dicurigai | Bukan tim PPI: tombol tidak tampil |
| Berhenti atau selesai | Sistem | Waktu keluar ruangan, tanggal | `StoppedOnDeparture` atau `Completed` | — |
