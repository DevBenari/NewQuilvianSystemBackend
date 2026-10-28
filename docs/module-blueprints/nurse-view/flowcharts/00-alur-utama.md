# Flowchart — Alur Utama Nurse View

| Field | Nilai |
|---|---|
| Revision | `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Keputusan | `NV-DEC-004`, `009`, `014`, `022`, `023` |

Jalur normal saja. Jalur gagal ada di berkas proses.

```mermaid
flowchart TD
    subgraph it[Admin IT]
        A([TV baru dipasang di nurse station]) --> B[Daftarkan perangkat dan pilih lantai]
        B --> C[(Perangkat PendingLogin)]
        C --> D[Buat login, catat kata sandi]
        D --> E[(Perangkat Active)]
    end
    subgraph tv[TV di nurse station]
        E --> F[Masukkan nama pengguna dan kata sandi]
        F --> G[Papan lantai tampil]
        G --> H[Muat ulang otomatis setiap menit]
        H --> G
    end
    subgraph perawat[Perawat]
        I([Perawat menyimpan TTV pasien]) --> J[Kartu bed berubah dalam hitungan detik]
        G --> K[Perawat melihat papan dari jauh]
        J --> K
        K --> L([Perawat mendatangi pasien yang perlu perhatian])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Daftarkan perangkat | Admin IT | Nama, lantai | Perangkat `PendingLogin` | Lihat `01-pengelolaan-perangkat.md` |
| Buat login | Admin IT | Perangkat `PendingLogin` | Nama pengguna + kata sandi sekali tampil; perangkat `Active` | Idem |
| Login di TV | Admin IT di TV | Nama pengguna, kata sandi | Papan tampil | Lihat `02-pembaruan-papan.md` |
| Papan diperbarui | Sistem | Data bed, TTV, perawat | Kartu terbaru | Pita data basi |
| Perawat menindaklanjuti | Perawat | Papan | Pasien didatangi; detail dibaca di rekam medis | Papan hanya alat bantu (`G-02`) |
