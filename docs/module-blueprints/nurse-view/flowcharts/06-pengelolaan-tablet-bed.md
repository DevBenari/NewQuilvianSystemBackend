# Flowchart — Pengelolaan Tablet Bed

| Field | Nilai |
|---|---|
| Revision | `2` · `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Keputusan | `NV-DEC-035`, `046`; usulan `AD-01`–`04` |
| Status | Sama persis dengan `contracts/state-transition-matrix.md` §4 |

Polanya sama dengan perangkat display (`01-pengelolaan-perangkat.md`), dengan bed sebagai
pengganti lantai.

```mermaid
flowchart TD
    subgraph it[Admin IT]
        A([Tablet perlu dipasang atau diganti]) --> B[Pilih lantai lalu bed, isi nama]
        B --> C{Bed sah dan belum punya tablet?}
        C -- Bed tidak sah --> C1[/Ditolak, pilih bed dari daftar/]
        C1 --> B
        C -- Sudah punya tablet --> C2[/Ditolak, nonaktifkan tablet lama dulu/]
        C2 --> B
        C -- Ya --> D[(PendingLogin)]
        D --> E[Buat login, catat kata sandi]
        E --> F[(Active)]
        F --> G{Ada masalah?}
        G -- Rusak atau hilang --> H[Nonaktifkan dengan alasan]
        H --> I[(Inactive)]
        G -- Kata sandi bocor --> J[Atur ulang login]
        J --> F
        G -- Pindah ke bed lain --> K{Bed lama masih punya panggilan aktif?}
        K -- Ya --> K1[/Ditolak, selesaikan panggilan dulu/]
        K1 --> G
        K -- Tidak --> L[Ubah bed tablet]
        L --> F
        I --> M{Tablet pengganti datang?}
        M -- Ya --> J
        M -- Tidak --> N([Tablet tetap nonaktif])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Pilih bed | Admin IT | Lantai, bed dari daftar | Tablet `PendingLogin` | Bed tanpa lantai → minta admin master data mengisi lantai kamar |
| Buat login | Admin IT | Tablet `PendingLogin` | Kata sandi tampil sekali; `Active` | Data diubah orang lain → muat ulang lalu ulangi |
| Pasang tablet | Admin IT | Nama pengguna, kata sandi | Layar tablet bed tampil | Ditolak → periksa status tablet |
| Nonaktifkan | Admin IT | Alasan | `Inactive`; tablet kembali ke layar login | Alasan kosong → isi alasan |
| Pindah bed | Admin IT | Bed baru | Tablet melayani bed baru | Masih ada panggilan aktif → perawat menutupnya dulu |

**Contoh.** Tablet `NVT-000001` di bed 305-A retak pukul 14.00. Admin menonaktifkannya dengan
alasan "Layar retak". Panggilan aktif 305-A, bila ada, ditutup perawat dari konsol dengan
alasan "Tablet tidak dapat dipakai". Tablet pengganti dipasang; admin mengatur ulang login
`NVT-000001` dan tablet kembali `Active`.
