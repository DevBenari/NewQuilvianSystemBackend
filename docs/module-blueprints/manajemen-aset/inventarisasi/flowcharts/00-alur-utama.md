# Inventarisasi Fisik — Alur Utama

```mermaid
flowchart TD
    subgraph petugas[Petugas Aset]
        A([Tahun berjalan]) --> B[Jadwalkan opname unit dan tunjuk tim]
        B --> C[(Dijadwalkan)]
    end
    subgraph tim[Tim Inventarisasi]
        C --> D[Mulai sesi pada tanggalnya]
        D --> E[(Berjalan)]
        E --> F[Pindai QR setiap aset di ruangan]
        F --> G[Tutup pemindaian]
        G --> H[(Pencocokan)]
        H --> I[Beri catatan pada selisih lalu ajukan]
        I --> J[(MenungguVerifikasi)]
    end
    subgraph kepala[Kepala Bagian Aset]
        J --> K[Verifikasi dan setujui berita acara]
        K --> L[(Diverifikasi)]
    end
    L --> M([Usulan tindak lanjut lahir; penahanan unit lepas])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Jadwalkan | Petugas Aset | Unit, ruangan opsional, tanggal, tim | `Dijadwalkan` | — |
| Mulai | Ketua tim | — | Snapshot aset; unit ditahan | Proses berjalan di unit → selesaikan dulu |
| Pindai | Anggota tim | QR atau nomor diketik, ruangan, kondisi | Pindaian | Nomor tak dikenal → catat sebagai temuan |
| Tutup | Ketua tim | — | Hasil per kategori | Buka kembali bila ada yang terlewat |
| Ajukan | Ketua tim | Catatan per selisih | `MenungguVerifikasi` | Lengkapi catatan |
| Verifikasi | Kepala Bagian Aset bukan anggota tim | — | Berita acara disetujui, usulan dibuat | Dikembalikan ke `Pencocokan` dengan alasan |
