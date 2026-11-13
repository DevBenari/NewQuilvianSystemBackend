# Proses — Konsolidasi Kebutuhan dan Rencana Paket

Status dokumen: `approved` (`PBJ-DEC-034`). Keputusan: `PBJ-DEC-022`, `028`.

**Tujuan:** kebutuhan yang sama dari banyak unit dikelompokkan menjadi rencana paket, tanpa kehilangan asal
setiap unit.
**Pemicu:** rencana kebutuhan unit sudah disetujui.
**Prasyarat:** rencana berlaku (`APPROVED`) tersedia untuk tahun yang dipilih.

```mermaid
flowchart TD
    subgraph pengadaan[Tim Pengadaan]
        A([Pilih tahun dan kategori]) --> B[Lihat gabungan kebutuhan per barang]
        B --> C{Ada sisa yang belum dialokasikan?}
        C -- Tidak --> C1([Semua kebutuhan sudah masuk rencana paket])
        C -- Ya --> D[Pilih baris dan jumlah untuk rencana paket]
        D --> E{Jenis pengadaan baris sama dengan paket?}
        E -- Tidak --> E1[/Ditolak, jenis harus sama/]
        E1 --> D
        E -- Ya --> F{Jumlah melebihi sisa baris rencana?}
        F -- Ya --> F1[/Ditolak, melebihi rencana/]
        F1 --> D
        F -- Tidak --> G[Simpan dan ajukan rencana paket]
    end
    subgraph penyetuju[Penyetuju]
        G --> H{Keputusan}
        H -- Setujui --> I[(Disetujui)]
        H -- Kembalikan --> J[(Dikembalikan)]
        J --> D
        H -- Tolak --> K[(Ditolak)]
    end
    I --> L([Menjadi sumber paket di Pemilihan Penyedia])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
| --- | --- | --- | --- | --- |
| Lihat gabungan | Tim Pengadaan | Tahun, kategori, jenis | Baris gabungan per barang beserta unit asal dan sisa | Tidak ada rencana berlaku → meminta unit menyelesaikan rencana |
| Alokasi ke rencana paket | Tim Pengadaan | Baris dan jumlah | Rencana paket `DRAFT` | Jenis berbeda atau melebihi sisa → memperbaiki |
| Ajukan dan setujui | Tim Pengadaan / Penyetuju | — | Rencana paket `APPROVED` | Dikembalikan → memperbaiki |
| Batalkan | Tim Pengadaan | Alasan | `CANCELLED`; alokasi dilepas | Sudah dipakai paket eksekusi → tidak bisa |

**Contoh:** rencana 2027 untuk KTL-000045 Kursi Roda Standar: IGD 3, Rawat Inap 5, Poli 22, total 30. Tim
Pengadaan membuat rencana paket "Alat Bantu Mobilitas 2027" berisi 30 unit dengan tiga baris unit asal. Bila
kemudian rencana Poli direvisi turun menjadi 20, revisi itu ditolak sampai alokasi paketnya diturunkan lebih
dulu, karena 22 unit sudah dialokasikan.
