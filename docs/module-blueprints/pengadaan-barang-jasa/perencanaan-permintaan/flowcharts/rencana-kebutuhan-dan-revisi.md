# Proses — Rencana Kebutuhan Tahunan dan Revisi

Status dokumen: `approved` (`PBJ-DEC-034`). Siklus rencana **[DIKONFIRMASI `PBJ-DEC-033`, `G-05`]**.

**Tujuan:** setiap unit punya satu rencana kebutuhan berlaku per tahun, dengan perubahan yang tercatat
sebagai versi.
**Pemicu:** awal penyusunan anggaran tahunan, atau perubahan kebutuhan di tengah tahun.
**Prasyarat:** barang ada di katalog.

```mermaid
flowchart TD
    subgraph unit[Unit Peminta]
        A([Mulai rencana tahun baru]) --> B{Sudah ada rencana unit untuk tahun ini?}
        B -- Ya --> B1[/Ditolak, buka rencana yang ada atau buat revisi/]
        B -- Tidak --> C[Isi baris barang, jumlah, perkiraan harga]
        C --> D[Ajukan]
        R0([Kebutuhan berubah]) --> R1{Ada revisi lain yang belum selesai?}
        R1 -- Ya --> R2[/Ditolak, selesaikan revisi sebelumnya/]
        R1 -- Tidak --> R3[Buat revisi dengan alasan]
        R3 --> R4[Ubah baris pada versi baru]
        R4 --> R5{Jumlah baru di bawah yang sudah dipakai?}
        R5 -- Ya --> R6[/Ditolak, sudah terpakai/]
        R6 --> R4
        R5 -- Tidak --> D
    end
    subgraph penyetuju[Penyetuju]
        D --> E{Keputusan}
        E -- Kembalikan --> F[(Dikembalikan)]
        F --> C
        E -- Tolak --> G[(Ditolak)]
        E -- Setujui --> H[(Disetujui)]
    end
    H --> I[Versi lama menjadi Digantikan, versi baru berlaku]
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Buat rencana | Unit Peminta | Tahun, unit | Rencana `DRAFT` versi 1 | Sudah ada → membuka rencana yang ada |
| Ajukan | Unit Peminta | Minimal satu baris | `PENDING_APPROVAL`, nomor `RKB` | Melengkapi baris |
| Buat revisi | Unit Peminta / Tim Pengadaan | Alasan revisi | Versi baru `DRAFT`, baris disalin dengan kunci baris yang sama | Revisi lain masih berjalan → menyelesaikannya dulu |
| Turunkan jumlah | Unit Peminta | Jumlah baru | — | Di bawah pemakaian PR atau rencana paket → ditolak |
| Disetujui | Penyetuju | — | Versi baru berlaku, versi lama `SUPERSEDED` | — |

**Contoh revisi:** rencana IGD 2027 v1 berisi 10 monitor; 6 sudah dipakai PR. Revisi v2 menaikkan menjadi
12. Setelah disetujui, sisa = 12 − 6 = 6. PR yang merujuk baris ini tetap tertaut, karena kunci barisnya sama.

**Banding versi (2.5):** layar revisi menampilkan per baris jumlah dan nilai sebelum–sesudah, beserta alasan
revisi.
