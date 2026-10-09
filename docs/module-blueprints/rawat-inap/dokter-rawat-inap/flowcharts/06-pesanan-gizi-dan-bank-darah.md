# Flowchart — Pesanan Gizi dan Bank Darah dari bangsal

| Field | Nilai |
|---|---|
| Sub-modul | `dokter-rawat-inap` |
| Kontrak | `0.7.0` — `draft` |
| Proses | `BP-RWF-05` |
| Keputusan | `RWI-DEC-171`, `RWI-DEC-188` |

```mermaid
flowchart TD
    subgraph pemesan[Dokter atau perawat]
        A([Instruksi pemeriksaan gizi atau darah]) --> B{Siapa yang memesan?}
        B -- Dokter berpenugasan --> C[Isi pesanan atas nama sendiri]
        B -- Perawat --> D[Pilih dokter pemberi instruksi, isi pesanan]
    end
    subgraph sistem[Sistem Rawat Inap]
        D --> E{Dokter dipilih?}
        E -- Tidak --> E1[/Ditolak, dokter wajib dipilih/]
        E1 --> D
        E -- Ya --> F{Dokter berpenugasan aktif?}
        F -- Tidak atau tidak terbaca --> F1[/Ditolak/]
        F1 --> D
        F -- Ya --> G[Teruskan dengan status menunggu verifikasi]
        C --> H[Teruskan tanpa perlu verifikasi]
    end
    subgraph pemilik[Modul Gizi atau Bank Darah]
        G --> I{Pesanan darah mirip pesanan sebelumnya?}
        H --> I
        I -- Ya --> I1[Minta konfirmasi beralasan]
        I1 --> J[Pesanan tersimpan]
        I -- Tidak --> J
    end
    subgraph verifikasi[Dokter pemberi instruksi]
        J --> K{Perlu verifikasi?}
        K -- Ya --> L[Verifikasi dari daftar gabungan]
        K -- Tidak --> M([Pesanan diproses unit penunjang])
        L --> M
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Isi pesanan | Dokter atau perawat | Layanan, alasan, komponen darah | Pesanan siap dikirim | — |
| Periksa penugasan | Sistem Rawat Inap | Dokter pemberi instruksi | Pesanan diteruskan | Tanpa dokter atau dokter tidak berpenugasan: ditolak |
| Simpan di modul pemilik | Gizi atau Bank Darah | Pesanan | Pesanan dengan peminta dan penginput | Pesanan darah mirip: konfirmasi beralasan |
| Verifikasi | Dokter pemberi instruksi | Daftar gabungan | Terverifikasi atas namanya | Bukan dokter peminta: ditolak |
