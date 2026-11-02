# Flowchart — Pembaruan Papan dan Data Basi

| Field | Nilai |
|---|---|
| Revision | `1` · `approved` — Rizki Gunawan, 2026-10-28 (`NV-DEC-028`) |
| Keputusan | `NV-DEC-014`, `023`, `026`; `NV-AC-006`, `016`, `019`, `020` |

```mermaid
flowchart TD
    subgraph tv[TV]
        A([TV menyala dan login]) --> B{Perangkat masih aktif?}
        B -- Tidak --> B1[/Ditolak, kembali ke layar login/]
        B1 --> A
        B -- Ya --> C[Tampilkan papan lantai]
        C --> D{Ada sinyal TTV baru atau sudah satu menit?}
        D -- Ya --> E[Ambil papan terbaru]
        E --> F{Berhasil?}
        F -- Ya --> C
        F -- Tidak --> G{Sudah lebih dari dua menit tanpa papan baru?}
        G -- Belum --> D
        G -- Sudah --> H[Tampilkan pita data basi di atas papan lama]
        H --> I{Koneksi kembali?}
        I -- Ya --> E
        I -- Tidak --> H
        D -- Belum --> D
    end
    subgraph perawat[Perawat]
        J([Perawat menyimpan TTV]) --> K[Sinyal dikirim ke TV lantai pasien]
        K --> D
        L([Pasien pulang atau pindah]) --> M[Tertangkap muat ulang berikutnya]
        M --> D
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Periksa perangkat | Sistem | Akun yang login | Lanjut atau ditolak | Ditolak → IT memeriksa status perangkat |
| Ambil papan | TV | — | Papan lantai | Gagal → coba lagi otomatis; perawat tetap melihat papan lama |
| Sinyal TTV | Sistem | TTV tersimpan | Satu sinyal ke TV lantai | Sinyal gagal → TTV tetap tersimpan, papan berubah paling lambat 60 detik |
| Pita data basi | TV | > 2 menit tanpa papan baru | Pita abu-abu berisi jam data terakhir | Perawat memakai rekam medis sampai pita hilang; lapor IT bila berlanjut |
| Pasien pindah lantai | Sistem | Penempatan baru | Kartu hilang dari lantai lama, muncul di lantai baru | — |
