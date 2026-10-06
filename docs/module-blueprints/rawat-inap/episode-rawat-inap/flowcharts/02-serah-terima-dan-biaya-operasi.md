# Flowchart — Serah terima pasca operasi dan biaya operasi

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | `0.10.0` — `draft` |
| Proses | `BP-RWF-03`, `BP-RWF-08` |
| Keputusan | `RWI-DEC-177`, `189`, `196`, `197` |

```mermaid
flowchart TD
    subgraph ok[Perawat OK]
        A([Pasien keluar kamar pulih]) --> B[Kirim serah terima ke unit tujuan]
    end
    subgraph tujuan[Perawat unit tujuan]
        B --> C{Penerima bukan pengirim?}
        C -- Bukan --> C1[/Tidak dapat menerima/]
        C -- Ya --> D{Pasien sudah di bed unit ini?}
        D -- Belum --> D1[Pindahkan pasien lewat Transfer Pasien]
        D1 --> D
        D -- Sudah --> E{Isi serah terima memadai?}
        E -- Tidak --> F[Tolak dengan alasan]
        F --> B
        E -- Ya --> G[Terima]
    end
    subgraph sistem[Sistem]
        G --> H{Laporan operasi final dan pasien keluar kamar pulih?}
        H -- Belum --> H1[Kasus menunggu]
        H -- Ya --> I[Kasus operasi selesai]
        I --> J[Order tindakan ditandai selesai, tindakan tertagih sekali]
        I --> K[OK mengirim anestesi, sewa kamar, dan bahan]
        K --> L{Tarif ada di master?}
        L -- Tidak --> L1[Baris tarif belum ada, invoice belum dapat difinalkan]
        L -- Ya --> M([Biaya operasi di invoice])
        J --> M
    end
    subgraph pantau[Daftar pantau]
        B --> N{Belum diterima melewati batas jam?}
        N -- Ya --> O[Tampil di daftar pantau bangsal dan OK]
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Kirim serah terima | Perawat OK | Kondisi, alat, risiko, instruksi | Serah terima terkirim | — |
| Periksa penerima | Sistem | Akun penerima | Lanjut | Akun sama dengan pengirim: minta perawat unit tujuan |
| Periksa bed | Sistem | Lokasi bed pasien | Lanjut | Pasien belum di unit tujuan: lakukan Transfer Pasien dulu. Bila lokasi tidak dapat dibaca, penerimaan ditolak dan dicoba lagi |
| Tolak | Perawat unit tujuan | Alasan | OK melengkapi dan mengirim ulang | — |
| Kasus selesai | Sistem | Serah terima diterima, laporan final | Kasus selesai | Laporan belum final: dokter operator memfinalkan laporan |
| Biaya | Sistem | Kasus selesai | Baris tagihan | Tarif belum ada: admin tarif melengkapi master; kirimannya dicoba ulang dari rekonsiliasi OK |
| Pantau | Kepala ruangan, OK | Serah terima tertunda | Daftar | — |

Selama operasi pasien tetap menempati bed asal dan tarif kamarnya tetap berjalan. Menerima serah terima tidak memindahkan bed.
