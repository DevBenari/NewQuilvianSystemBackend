# Flowchart — Admisi rawat inap dari kamar pulih

| Field | Nilai |
|---|---|
| Sub-modul | `episode-rawat-inap` |
| Kontrak | `0.10.0` — `draft` |
| Proses | `BP-RWF-08` |
| Keputusan | `RWI-DEC-201`; persetujuan OK `RWI-DEC-208`; tagihan kunjungan asal `RWI-DEC-207` |
| Keputusan yang belum ada | Tidak ada — `DEC-INP-018` ditutup `RWI-DEC-207` |

```mermaid
flowchart TD
    subgraph ok[Kamar pulih]
        A([Keputusan kamar pulih disimpan]) --> B{Keputusan rawat inap atau ICU?}
        B -- Tidak --> B1([Alur OK biasa])
        B -- Ya --> C{Pasien sudah punya episode rawat inap aktif?}
        C -- Ya --> C1([Tidak perlu permintaan, serah terima biasa])
        C -- Tidak --> D[Permintaan admisi dibuat]
        D --> E{Keputusan berubah atau pasien boleh pulang?}
        E -- Ya --> F[Permintaan dibatalkan dengan alasan]
    end
    subgraph admisi[Petugas admisi]
        E -- Tidak --> G[Lihat daftar permintaan admisi]
        G --> H[Buka admisi dari permintaan]
        H --> I[Lengkapi penjamin, kelas, DPJP, deposit]
        I --> J{Bed sesuai aturan jenis kelamin dan isolasi tersedia?}
        J -- Tidak --> J1[Pilih bed lain atau tunggu]
        J1 --> J
        J -- Ya --> K[Admisi selesai, permintaan selesai]
    end
    subgraph pantau[Daftar pantau]
        G --> L{Menunggu melewati batas waktu?}
        L -- Ya --> M[Tampil di daftar pantau admisi]
    end
    K --> N([Pasien menempati bed, serah terima dapat diterima])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Simpan keputusan | Tim kamar pulih | Keputusan rawat inap atau ICU | Permintaan admisi | Pasien sudah dirawat: tidak ada permintaan, lanjut serah terima biasa |
| Batalkan | Tim kamar pulih | Alasan | Permintaan hilang dari daftar | — |
| Buka admisi | Petugas admisi | Permintaan | Alur berlangkah terisi awal | Permintaan sudah dibatalkan: muat ulang daftar |
| Admisi biasa tanpa permintaan | Petugas admisi | Pasien yang punya permintaan | Ditolak | Buka admisi dari permintaannya |
| Pilih bed | Petugas admisi | Kelas, jenis kelamin, isolasi | Bed ditempati | Tidak ada bed sesuai: pilih lain atau tunggu |
| Tautan tagihan | Sistem kasir | Admisi selesai | Biaya operasi kunjungan asal tertaut ke tagihan rawat inap, dibayar bersama saat pulang | Gagal: dicoba ulang otomatis; admisi tetap sah |
| Pantau | Petugas admisi, kepala ruangan | Permintaan menunggu | Daftar | — |

Tidak ada admisi otomatis. Selama permintaan belum selesai, kasus OK belum selesai dan biaya operasinya belum terkirim.
