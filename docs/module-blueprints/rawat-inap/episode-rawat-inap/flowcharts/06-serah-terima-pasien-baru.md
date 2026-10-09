# Flowchart — Serah Terima Pasien Baru

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | `0.11.0` — `approved` 2026-10-08 (`RWI-DEC-265`) |
| Cakupan file ini | Checklist serah terima dari admisi ke ruangan dengan tiga tanda tangan petugas berbeda |
| Keputusan | `RWI-DEC-239`, `241`, `255`, `262` |

```mermaid
flowchart TD
    subgraph admisi[Petugas admisi]
        A([Pasien diterima rawat inap]) --> B[Buka Serah Terima Pasien]
        B --> C[Periksa saran sistem pada butir yang sudah terbukti]
        C --> D[Pilih Sudah atau Belum untuk setiap butir]
        D --> E[Kunci dokumen]
        G2[Tanda tangan kolom Admission]
    end
    subgraph sistem[Sistem]
        E --> F{Semua butir dipilih dan butir Belum berketerangan?}
        F -- Tidak --> F1[/Ditolak, butir yang kurang disebut nomornya/]
        F1 --> D
        F -- Ya --> G[(AwaitingSignature)]
        G --> G2
    end
    subgraph cro[CRO]
        G2 --> H{Akun ini sudah menandatangani kolom lain?}
        H -- Ya --> H1[/Ditolak, minta CRO lain/]
        H1 --> H
        H -- Tidak --> I[Tanda tangan kolom CRO]
    end
    subgraph perawat[Perawat ruangan]
        I --> J{Pasien sudah menempati tempat tidur?}
        J -- Belum --> J1[/Tombol terkunci, tunggu pasien menempati bed/]
        J1 --> J
        J -- Sudah --> K[Tanda tangan kolom Perawat]
    end
    K --> L[(Completed)]
    L --> M([Serah terima lengkap; kelengkapan naik])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka Serah Terima | Petugas admisi | Butir dari master jenis serah terima | Checklist 15 butir dan 3 sub-butir; nama butir dibekukan | Master kosong: admin mengisi butir di Butir Administrasi Rawat Inap |
| Periksa saran | Petugas admisi | Saran "Sudah" pada butir surat pengantar, IPD, deposit, gelang, dan prakiraan biaya bila sudah terbukti | Pilihan petugas | Saran tidak muncul: dokumen sumbernya belum lengkap; petugas tetap boleh memilih sendiri |
| Pilih Sudah/Belum | Petugas admisi | Pengecekan nyata di lapangan | Setiap butir terpilih; butir Belum berketerangan | Butir terlewat: lengkapi butir yang disebut |
| Kunci | Petugas admisi | Seluruh butir terpilih | Dokumen `AwaitingSignature` | Butir kurang: lengkapi |
| Tanda tangan Admission | Petugas admisi | Akun sendiri, dokumen terkunci | Kolom Admission terisi | — |
| Tanda tangan CRO | CRO | Dokumen terkunci | Kolom CRO terisi | CRO sama dengan petugas admisi: ditolak; CRO tidak bertugas: dokumen menunggu, perawatan tetap berjalan |
| Tanda tangan Perawat | Perawat ruangan | Pasien menempati bed | Kolom Perawat terisi; dokumen `Completed` | Bed masih dipesan: tempatkan pasien dulu lewat penempatan bed |

Contoh: Sari mengunci pukul 10.05 dan menandatangani Admission; Dewi menandatangani CRO pukul 10.20; Budi menempati bed pukul 10.35; Andi menandatangani Perawat pukul 10.40 dan dokumen lengkap.
