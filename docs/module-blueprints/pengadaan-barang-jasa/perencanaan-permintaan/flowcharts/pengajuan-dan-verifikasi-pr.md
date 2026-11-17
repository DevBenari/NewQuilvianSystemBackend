# Proses — Pengajuan, Verifikasi, dan Persetujuan PR

Status dokumen: `approved` (`PBJ-DEC-034`). Keputusan: `PBJ-DEC-008`, `014`, `016`, `027`, `029`, `030`.

**Tujuan:** kebutuhan unit menjadi permintaan yang terverifikasi dan disetujui atas nilai yang benar-benar
akan dibeli.
**Pemicu:** unit membutuhkan barang/jasa.
**Prasyarat:** pengusul punya penempatan aktif di unit; barang ada di katalog; alur persetujuan `PBJ-PR`
sudah dikonfigurasi.

## 1. Pengajuan

```mermaid
flowchart TD
    subgraph unit[Unit Peminta]
        A([Butuh barang atau jasa]) --> B[Isi permintaan dan baris barang]
        B --> C{Mendesak?}
        C -- Ya --> C1[Isi alasan dan batas waktu]
        C -- Tidak --> D
        C1 --> D[Tekan Ajukan]
    end
    subgraph sistem[Sistem]
        D --> E{Isian lengkap dan sah?}
        E -- Tidak --> E1[/Ditolak, isian perlu dilengkapi/]
        E1 --> B
        E -- Ya --> F[Hitung sisa rencana per baris]
        F --> G{Ada jumlah di luar rencana?}
        G -- Ya --> H{Alasan luar rencana diisi?}
        H -- Tidak --> H1[/Ditolak, alasan luar rencana wajib/]
        H1 --> B
        H -- Ya --> I
        G -- Tidak --> I[Beri nomor permintaan]
        I --> J[(Menunggu Verifikasi)]
    end
```

## 2. Verifikasi dan cek stok

```mermaid
flowchart TD
    subgraph pengadaan[Tim Pengadaan]
        A[(Menunggu Verifikasi)] --> B{Kelengkapan dan kewajaran baik?}
        B -- Perlu diperbaiki --> B1[Kembalikan dengan alasan]
        B -- Duplikat atau tidak layak --> B2[Tolak dengan alasan]
        B -- Baik --> C[Catat stok tersedia dan jumlah dipenuhi stok per baris]
        C --> D[Tekan Selesaikan Verifikasi]
    end
    B1 --> R[(Dikembalikan)]
    B2 --> X[(Ditolak)]
    subgraph sistem[Sistem]
        D --> E{Semua baris sudah dicek?}
        E -- Belum --> E1[/Ditolak, masih ada baris belum dicek/]
        E1 --> C
        E -- Sudah --> F{Masih ada kebutuhan beli?}
        F -- Tidak --> S[(Dipenuhi dari Stok)]
        F -- Ya --> G[Hitung nilai beli dan bandingkan dengan sisa pagu]
        G --> H{Alur persetujuan tersedia?}
        H -- Tidak --> H1[/Ditolak, alur persetujuan belum dikonfigurasi/]
        H1 --> Z([Tim Pengadaan menghubungi administrator])
        H -- Ya --> I[Kirim ke persetujuan berjenjang dengan tanda mendesak, luar rencana, lewat pagu]
        I --> P[(Menunggu Persetujuan)]
    end
```

## 3. Persetujuan dan pembatalan

```mermaid
flowchart TD
    subgraph penyetuju[Penyetuju sesuai matriks]
        A[(Menunggu Persetujuan)] --> B{Penyetuju adalah pengusul?}
        B -- Ya --> B1[/Ditolak, tidak boleh menyetujui sendiri/]
        B -- Tidak --> C{Keputusan}
        C -- Setujui, masih ada jenjang --> A
        C -- Setujui, jenjang terakhir --> D[(Disetujui)]
        C -- Tolak --> E[(Ditolak)]
        C -- Kembalikan --> F[(Dikembalikan)]
    end
    subgraph unit[Unit Peminta]
        F --> G[Perbaiki lalu ajukan ulang]
        G --> H[(Menunggu Verifikasi)]
        A --> I{Ingin menarik?}
        I -- Ya --> J[(Dibatalkan)]
    end
    subgraph pengadaan[Tim Pengadaan]
        D --> K{Perlu dibatalkan sebelum masuk paket?}
        K -- Ya, dengan alasan --> J
        K -- Tidak --> L([Diambil Pemilihan Penyedia])
    end
```

## 4. Tabel langkah

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Isi permintaan | Unit Peminta | Unit, tahun, sumber dana, tanggal dibutuhkan, alasan, baris barang | PR `DRAFT` | — |
| Ajukan | Unit Peminta | PR `DRAFT`/`RETURNED` | PR `SUBMITTED` bernomor; pembagian dalam/luar rencana per baris | Melengkapi isian yang disebut pesan, lalu mengajukan ulang |
| Kembalikan | Tim Pengadaan | Alasan | PR `RETURNED` | — |
| Tolak | Tim Pengadaan | Alasan | PR `REJECTED`, final | Unit membuat PR baru bila masih perlu |
| Catat cek stok | Tim Pengadaan | Stok tersedia, dipenuhi stok, catatan sumber | Kebutuhan beli per baris | Memperbaiki angka yang melebihi stok atau permintaan |
| Selesaikan verifikasi | Tim Pengadaan | Semua baris tercek | `FULFILLED_FROM_STOCK` atau `PENDING_APPROVAL` | Belum dicek → melengkapi; alur belum ada → menghubungi administrator, PR tetap `SUBMITTED` |
| Persetujuan | Penyetuju | Tugas di kotak masuk | `APPROVED`, `REJECTED`, atau `RETURNED` | Pengusul tidak dapat menyetujui sendiri; tugas diarahkan ke penyetuju lain sesuai matriks |
| Tarik | Unit Peminta | PR `PENDING_APPROVAL` | `CANCELLED` | Bila approval sudah final, penarikan ditolak |
| Batal setelah disetujui | Tim Pengadaan | Alasan | `CANCELLED`; sisa rencana kembali | Sudah masuk paket → tidak bisa dari sini |

**Hasil akhir:** PR `APPROVED` dengan nilai beli yang tercatat dan jejak siapa menyetujui pada jenjang apa,
atau PR yang selesai dari stok tanpa pembelian.
