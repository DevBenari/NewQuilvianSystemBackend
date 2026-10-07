# Flowchart — Keluar ruangan, izin kasir, dan penutupan episode

| Field | Nilai |
|---|---|
| Sub-modul | `integrasi-billing` |
| Kontrak | `1.1.0` — `draft` |
| Proses | `BP-RWF-02` |
| Keputusan | `RWI-DEC-167`, `186`, `187` |
| Menggantikan | `03-clearance-dan-auto-reblock.md` |

```mermaid
flowchart TD
    subgraph perawat[Perawat atau petugas bangsal]
        A([Keluarga datang menjemput]) --> B[Buka pencatatan keluar ruangan]
        B --> C{Status kasir disetujui?}
        C -- Ya --> E[Catat keluar ruangan]
        C -- Belum atau tidak terbaca --> D[/Peringatan kasir belum memberi izin/]
        D --> D1{Tetap catat?}
        D1 -- Tidak --> D2([Pasien menunggu urusan kasir])
        D1 -- Ya --> E
    end
    subgraph sistem[Sistem Rawat Inap]
        E --> F[(Bed Available, status kasir saat keluar tercatat)]
        F --> G{Status kasir saat keluar disetujui?}
        G -- Tidak --> G1[Masukkan ke daftar pulang sebelum izin kasir]
        G -- Ya --> H[Tunggu penutupan]
        G1 --> H
    end
    subgraph kasir[Kasir]
        H --> I{Tagihan beres?}
        I -- Ya --> J[Setujui izin kasir]
        I -- Tidak --> I1[Tahan dan tindak lanjuti piutang]
        J --> J1{Ada tagihan susulan?}
        J1 -- Ya --> J2[Izin dicabut otomatis]
        J2 --> I
    end
    subgraph admisi[Petugas admisi atau supervisor]
        J1 -- Tidak --> K[Tutup episode]
        K --> L{Izin kasir terbaca disetujui saat tombol ditekan?}
        L -- Ya --> M[(Closed)]
        L -- Tidak atau tidak terbaca --> L1[/Penutupan ditolak/]
        L1 --> N{Supervisor menutup tanpa izin kasir?}
        I1 --> N
        N -- Tidak --> H
        N -- Ya, dengan alasan --> O[(Closed, ditutup tanpa kelayakan keuangan)]
    end
    M --> P([Selesai])
    O --> P
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Buka pencatatan keluar | Perawat, kepala ruangan, admisi, supervisor | Episode `DischargePending` | Badge status kasir | Episode belum boleh pulang: tombol tidak tersedia |
| Akui peringatan | Pencatat | Status kasir bukan disetujui atau tidak terbaca | Pengakuan sekali klik | Pencatat membatalkan; pasien menunggu |
| Catat keluar | Pencatat | Waktu keluar | Bed kosong, tarif kamar terkunci di jam keluar | Waktu tidak valid: pencatat memperbaiki waktu |
| Setujui izin | Kasir | Tagihan final | Izin disetujui | Keluarga belum membayar: episode masuk daftar pantau |
| Tutup | Petugas admisi | Izin disetujui | `Closed` | Ditolak: tunggu kasir, atau minta supervisor |
| Tutup tanpa izin | Supervisor pemegang permission | Alasan jelas | `Closed`, masuk laporan | Alasan kosong: diminta mengisi; tanpa permission: tombol tidak tampil |
