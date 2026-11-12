# Register Aset — Alur Utama

Jalur normal satu aset dari dicatat sampai dipakai di unit. Status pada node `[( )]` sama persis
dengan `../contracts/state-transition-matrix.md`.

```mermaid
flowchart TD
    subgraph petugas[Petugas Aset]
        A([Barang tahan lama diterima RS]) --> B[Catat aset sebagai draft]
        B --> C[(Draft)]
        C --> D[Ajukan untuk disetujui]
        D --> E[(Diajukan)]
    end
    subgraph kepala[Kepala Bagian Aset]
        E --> F[Periksa dan setujui]
        F --> G[(Terdaftar)]
    end
    subgraph petugas2[Petugas Aset]
        G --> H[Cetak dan tempel label QR]
        H --> I[Serahkan ke unit dan penanggung jawab]
        I --> J[(Diserahkan)]
    end
    subgraph unit[Kepala Unit atau penanggung jawab]
        J --> K[Konfirmasi penerimaan]
        K --> L[(Dikonfirmasi)]
    end
    L --> M([Aset dipakai di unit; tercatat di linimasa])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Catat draft | Petugas Aset | Kelompok, jenis kepemilikan, data perolehan, atribut medis bila alat medis | Satu atau banyak aset `Draft` tanpa nomor | Melengkapi isian yang ditandai |
| Ajukan | Petugas Aset | Draft lengkap | Aset `Diajukan` | Melengkapi atribut wajib (misalnya nomor seri) |
| Setujui | Kepala Bagian Aset | Aset `Diajukan` | Nomor inventaris terbit, aset `Terdaftar`, buku nilai terbentuk | Menolak dengan alasan → aset kembali `Draft` |
| Cetak label | Petugas Aset | Aset `Terdaftar` | PDF label QR | Mencetak ulang |
| Serahkan | Petugas Aset | Unit, ruangan, penanggung jawab | Penyerahan `Diserahkan`, aset terkunci | Memilih ruangan milik unit tujuan |
| Konfirmasi | Kepala Unit / penanggung jawab | Penyerahan yang ditujukan kepadanya | Penempatan berlaku, Berita Acara Penyerahan terbit | Menolak terima dengan alasan → Petugas Aset menyerahkan ulang |
