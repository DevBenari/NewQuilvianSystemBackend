# Flowchart — Klaim dan Penutupan Panggilan

| Field | Nilai |
|---|---|
| Revision | `2` · `approved` — Rizki Gunawan, 2026-11-05 (`NV-DEC-055`) |
| Keputusan | `NV-DEC-040`, `041`, `046`, `050`, `052`, `053`, `054` |
| Status | Sama persis dengan `contracts/state-transition-matrix.md` §5 |

Panggilan selesai lewat salah satu dari lima jalan: perawat hadir di bed, tutup dari konsol,
pasien membatalkan, sistem menutup karena pasien pindah, atau karena pasien pulang. Koreksi dan
pembatalan penempatan **tidak** menutup panggilan.

```mermaid
flowchart TD
    subgraph perawat[Perawat di konsol dan di bed]
        A([Panggilan Waiting]) --> B{Ada perawat yang mengklaim?}
        B -- Ya --> C[(Claimed)]
        C --> D{Pengklaim tertahan?}
        D -- Lepas sendiri --> A
        D -- Perawat lain mengambil alih --> C
        D -- Tidak --> E{Tablet bisa dipakai?}
        E -- Ya --> F[Tahan Perawat hadir 3 detik]
        F --> G[(Handled)]
        E -- Tidak atau lupa --> H[Tutup panggilan dari konsol dengan alasan]
        H --> G2[(Handled)]
        G2 --> G3[Tercatat tidak dikonfirmasi di bed]
    end
    subgraph pasien[Pasien]
        B -- Belum --> P{Panggilan biasa dan pasien tidak butuh lagi?}
        P -- Ya --> P1[Tekan Batal]
        P1 --> P2[(Cancelled)]
        P -- Tidak --> B
    end
    subgraph sistem[Sistem, paling lambat 60 detik]
        S([Penempatan bed berakhir]) --> T{Sebabnya?}
        T -- Pindah --> U[(ClosedBySystem)]
        T -- Pulang --> U
        T -- Koreksi atau pembatalan --> V[Panggilan tetap menunggu perawat]
        V --> E
    end
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Klaim | Perawat berizin | Panggilan `Waiting` | `Claimed`, nama di TV | Sudah diklaim perawat lain → ambil alih bila perlu |
| Lepas | Pengklaim | Panggilan `Claimed` | Kembali `Waiting`, jam awal tetap | Bukan pengklaim → minta pengklaim melepas atau ambil alih |
| Ambil alih | Perawat berizin lain | Panggilan `Claimed` | Pengklaim berganti; jejak mencatat keduanya | — |
| Perawat hadir | Perawat di bed | Panggilan aktif | `Handled`, dikonfirmasi di bed | Dilepas sebelum 3 detik → ulangi; tablet tidak dapat dipakai → tutup dari konsol |
| Tutup dari konsol | Perawat berizin | Alasan wajib; keterangan untuk Lainnya | `Handled`, tidak dikonfirmasi di bed | Alasan kosong → pilih alasan |
| Batal | Pasien | Panggilan biasa yang masih menunggu | `Cancelled` | Darurat atau sudah diklaim → tidak ada tombol Batal; perawat tetap datang |
| Pindah atau pulang | Sistem | Akhir penempatan dari Rawat Inap | `ClosedBySystem` dengan alasannya | Pemeriksaan gagal → diulang pada putaran berikutnya |
| Koreksi atau pembatalan penempatan | Sistem | Akhir penempatan administratif | Tidak ada perubahan | Perawat menutup lewat tablet atau konsol |

**Contoh tutup cadangan.** Tablet 312-B mati pukul 02.00 sesudah pasien memanggil. Dewi
mengklaim, datang, dan menangani pasien. Karena "Perawat hadir" tidak bisa ditekan, ia menutup
dari konsol dengan alasan "Tablet tidak dapat dipakai". Panggilan selesai dan ditandai tidak
dikonfirmasi di bed.
