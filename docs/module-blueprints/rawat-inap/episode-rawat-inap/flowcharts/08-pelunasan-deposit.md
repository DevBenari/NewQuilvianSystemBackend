# Flowchart — Pelunasan Deposit

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | `0.11.0` — `approved` 2026-10-08 (`RWI-DEC-265`) |
| Cakupan file ini | Surat pernyataan kesediaan melunasi kekurangan deposit, dari angka kasir sampai peringatan jatuh tempo |
| Keputusan | `RWI-DEC-231`, `248`, `252`, `258`, `260`, `261`, `263` |

```mermaid
flowchart TD
    subgraph admisi[Petugas admisi]
        A([Pasien dirawat dengan deposit kurang]) --> B[Buka Pelunasan Deposit]
    end
    subgraph sistem[Sistem]
        B --> C{Data deposit terbaca dari kasir?}
        C -- Tidak --> C1[/Angka tidak tampil, simpan ditolak, coba lagi/]
        C1 --> B
        C -- Ya --> D{Kasir mencatat kekurangan?}
        D -- Tidak --> D1([Surat tidak diperlukan])
        D -- Ya --> E[Tampilkan kekurangan dan jatuh tempo bawaan]
    end
    subgraph isi[Petugas admisi]
        E --> F[Pilih yang menyatakan dari data wali atau isi manual]
        F --> G[Periksa tanggal surat dan jatuh tempo]
        G --> H{Jatuh tempo di dalam batas kebijakan?}
        H -- Tidak --> H1[/Ditolak, pilih tanggal lebih awal/]
        H1 --> G
        H -- Ya --> I[Kunci; angka dibekukan]
        I --> J[Keluarga menandatangani; petugas mencatat dan menandatangani]
        J --> K[(Completed)]
    end
    subgraph pantau[Sistem]
        K --> L{Jatuh tempo lewat dan kasir masih mencatat kekurangan?}
        L -- Tidak --> M([Selesai])
        L -- Ya --> N[Peringatan jatuh tempo terlewati di Workspace PPRI dan Detail Episode]
        N --> O([Tindak lanjut manual oleh petugas dan kasir])
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Baca deposit | Sistem | Kebijakan deposit dan deposit diterima menurut kasir | Kekurangan = minimum − diterima | Kasir tidak menjawab: tidak mengetik angka sendiri; coba lagi |
| Jatuh tempo bawaan | Sistem | Tanggal surat, interval kebijakan | Hari kerja Senin–Jumat berikutnya pukul 11.00, dipotong ke batas tanggal surat + interval | — |
| Pilih yang menyatakan | Petugas admisi | Daftar relasi dan kontak darurat pasien, atau isian manual | Nama, alamat, telepon | Telepon lebih dari 13 digit: betulkan |
| Kunci | Petugas admisi | Isian lengkap | Angka kekurangan dibekukan bersama waktu bacanya | Kekurangan sudah lunas saat dikunci: surat tidak diperlukan |
| Tanda tangan | Keluarga, petugas admisi | Lembar untuk ditandatangani | Surat `Completed`; angka tetap walaupun deposit kemudian bertambah | Keluarga belum datang: dokumen menunggu |
| Pantau jatuh tempo | Sistem | Jatuh tempo pada surat, kekurangan terkini dari kasir | Peringatan tanpa rupiah di Detail Episode; dengan rupiah di header bagi pemegang hak | Penurunan kelas tidak otomatis; tetap transfer manual |

Contoh: surat Jumat 9 Oktober 2026, interval 3 hari → jatuh tempo bawaan Senin 12 Oktober pukul 11.00 WIB. Dengan interval 1 hari → Sabtu 10 Oktober pukul 11.00 WIB.
