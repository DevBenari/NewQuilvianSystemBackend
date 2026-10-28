# Flowchart — Pengelolaan Perangkat Display

| Field | Nilai |
|---|---|
| Revision | `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Keputusan | `NV-DEC-016`, `022` |
| Status | Sama persis dengan `contracts/state-transition-matrix.md` |

```mermaid
flowchart TD
    subgraph it[Admin IT]
        A([TV perlu dipasang atau diganti]) --> B[Isi nama dan lantai]
        B --> C{Lantai dikenal?}
        C -- Tidak --> C1[/Ditolak, lantai tidak terdaftar pada data kamar/]
        C1 --> B
        C -- Ya --> D[(PendingLogin)]
        D --> E[Buat login]
        E --> F{Data masih terbaru?}
        F -- Tidak --> F1[/Ditolak, muat ulang dulu/]
        F1 --> E
        F -- Ya --> G[(Active)]
        G --> H{TV rusak, hilang, atau dipindah?}
        H -- Hilang atau rusak --> I[Nonaktifkan dengan alasan]
        I --> J[(Inactive)]
        H -- Kata sandi bocor --> K[Atur ulang login]
        K --> G
        J --> L{TV pengganti datang?}
        L -- Ya --> K
        L -- Tidak --> M([Perangkat tetap nonaktif])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi nama dan lantai | Admin IT | Nama, lantai dari pilihan | Perangkat `PendingLogin` | Lantai tidak dikenal → admin minta admin master data mengisi lantai kamar |
| Buat login | Admin IT | Perangkat `PendingLogin` | Kata sandi tampil sekali; `Active` | Data sudah diubah orang lain → muat ulang lalu ulangi |
| Atur ulang login | Admin IT | Perangkat `Active`/`Inactive` | Kata sandi baru; login lama tidak berlaku; `Active` | Idem |
| Nonaktifkan | Admin IT | Alasan | `Inactive`; TV langsung kembali ke layar login | Alasan kosong → isi alasan |
| Kata sandi hilang sebelum dicatat | Admin IT | — | — | Atur ulang login; kata sandi lama tidak bisa dilihat lagi |
