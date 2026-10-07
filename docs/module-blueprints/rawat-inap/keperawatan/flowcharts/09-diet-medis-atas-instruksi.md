# Flowchart — Diet Medis atas instruksi dokter

| Field | Nilai |
|---|---|
| Sub-modul | `keperawatan` |
| Kontrak | `0.6.0` — `draft` |
| Keputusan | `RWI-DEC-178`, `RWI-DEC-188` |

```mermaid
flowchart TD
    subgraph penulis[Perawat, dokter, atau ahli gizi]
        A([Diet perlu ditetapkan atau diubah]) --> B{Siapa yang menulis?}
        B -- Dokter berpenugasan atau ahli gizi --> C[Tulis diet atas nama sendiri]
        B -- Perawat --> D[Pilih dokter pemberi instruksi, tulis diet]
    end
    subgraph sistem[Sistem Rawat Inap]
        D --> E{Dokter berpenugasan aktif?}
        E -- Tidak --> E1[/Ditolak/]
        E1 --> D
        E -- Ya --> F[Teruskan ke Gizi dengan status menunggu verifikasi]
        C --> G[Teruskan ke Gizi tanpa perlu verifikasi]
    end
    subgraph gizi[Sistem Gizi]
        F --> H{Mengganti diet aktif tanpa alasan?}
        G --> H
        H -- Ya --> H1[/Ditolak, alasan wajib/]
        H -- Tidak --> I[Diet berlaku; diet lama menjadi riwayat]
    end
    subgraph dokter[Dokter pemberi instruksi]
        I --> J{Perlu verifikasi?}
        J -- Ya --> K[Verifikasi dari daftar perlu diverifikasi]
        J -- Tidak --> L([Diet berlaku])
        K --> L
    end
    H1 --> A
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Tulis diet | Dokter, ahli gizi, atau perawat atas instruksi | Jenis diet, bentuk makanan, instruksi | Diet berlaku | Perawat tanpa dokter: diminta memilih dokter |
| Periksa penugasan | Sistem Rawat Inap | Dokter pemberi instruksi | Diteruskan ke Gizi | Tidak berpenugasan: ditolak |
| Ganti atau hentikan | Penulis yang sama | Alasan | Diet lama menjadi riwayat | Tanpa alasan: ditolak |
| Verifikasi | Dokter pemberi instruksi | Daftar perlu diverifikasi | Diet terverifikasi atas namanya | Bukan dokter penetap: ditolak |
