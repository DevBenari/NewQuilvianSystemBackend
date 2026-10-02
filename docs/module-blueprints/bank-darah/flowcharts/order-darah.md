# Proses: Order Darah — dengan jalur pengecualian

```mermaid
flowchart TD
    subgraph unit[Unit pelayanan / Petugas Bank Darah]
        A([Pasien butuh darah]) --> B{Order lewat sistem atau kertas?}
        B -- Elektronik --> C[Unit pelayanan buat order]
        B -- Kertas --> D[Petugas Bank Darah input manual]
        C --> E{Unit berwenang memesan darah?}
        D --> F{Isian manual lengkap?}
        E -- Tidak --> E1[/Ditolak, unit tak berwenang/]
        F -- Tidak --> F1[/Ditolak, isian wajib belum lengkap/]
    end
    subgraph sistem[Sistem]
        E -- Ya --> G{Ada order aktif sama pasien, kunjungan, komponen?}
        F -- Ya --> G
        G -- Ya --> H[/Ditahan, minta alasan tertulis/]
        H --> I{Alasan diisi?}
        I -- Tidak --> H
        I -- Ya --> J[(Order Active)]
        G -- Tidak --> J
    end
    J --> K([Order siap diproses])
    J --> L{Kunjungan berakhir sebelum terpenuhi?}
    L -- Ya --> M[(Order Expired)]
    M --> N([Buat order baru pada kunjungan baru bila masih perlu])
```

## Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Pilih jalur order | Unit / petugas | Kebutuhan klinis | Jalur elektronik atau manual | — |
| Periksa kewenangan unit | Sistem | Penanda unit boleh memesan darah | Lolos | Ditolak; unit diberi kewenangan lewat konfigurasi lebih dulu |
| Periksa order ganda | Sistem | Pasien + kunjungan + komponen + status aktif | Lolos atau ditahan | Ditahan; petugas mengisi alasan tertulis untuk melanjutkan |
| Simpan order | Sistem | Data lengkap | Order `Active` | — |
| Kedaluwarsa | Sistem | Sinyal kunjungan berakhir | Order `Expired` | Order lama tak dihidupkan; buat order baru |

Order `Expired` tidak dapat dibuka kembali. Pembatalan menyimpan alasan, tidak menghapus apa pun.
