# Penghapusan & Pelepasan Aset — Alur Utama

```mermaid
flowchart TD
    subgraph pengusul[Unit atau Bagian Aset]
        A([Aset rusak berat, usang, atau hilang]) --> B[Usulkan penghapusan]
        B --> C[(Diusulkan)]
    end
    subgraph tim[Tim Pemeriksa]
        C --> D[Periksa kelayakan setiap aset]
        D --> E[(MenungguPersetujuan)]
    end
    subgraph penyetuju[Kepala Bagian Aset, lalu Direktur bila di atas batas]
        E --> F[Setujui]
        F --> G[(Disetujui)]
    end
    subgraph petugas[Petugas Aset]
        G --> H[Laksanakan pelepasan dan catat]
        H --> I[(Dipindahtangankan)]
    end
    subgraph kepala[Kepala Bagian Aset]
        I --> J[Finalisasi]
        J --> K[(Final)]
    end
    K --> L([Aset keluar dari register; berita acara terbit; kejadian ke Akuntansi])
```

| Langkah | Pelaku | Masukan | Keluaran | Bila gagal |
|---|---|---|---|---|
| Usulkan | Petugas Aset / Kepala Unit | Alasan, aset, uraian | `Diusulkan`, aset terkunci | Kondisi tidak sesuai alasan → tetapkan kondisi dulu |
| Periksa | Tim Pemeriksa | Hasil Layak/Tidak Layak per aset | `MenungguPersetujuan` | Semua Tidak Layak → `Ditolak` |
| Setujui | Kepala Bagian Aset → (Direktur) | Total nilai buku vs batas | `Disetujui` | Ditolak → aset kembali bebas |
| Catat pelepasan | Petugas Aset | Metode, pihak, nilai, bukti | `Dipindahtangankan` | Alasan Hilang melewati langkah ini |
| Finalisasi | Kepala Bagian Aset | Penyusutan bulan lalu sudah diproses | `Final` | Proses penyusutan dulu |
