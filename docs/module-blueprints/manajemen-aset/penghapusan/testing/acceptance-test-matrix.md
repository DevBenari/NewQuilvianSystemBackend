# Penghapusan & Pelepasan Aset — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |

## 1. Jalur berhasil

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-HPS-01` | `AST-RULE-050` | Kursi rusak berat nilai buku Rp 0 | INTEG | Selesai di Kepala Bagian Aset |
| `AT-HPS-02` | `AST-DEC-049`, `AST-AC-012` | 10 kursi roda × Rp 6 jt, batas Rp 50 jt | INTEG | `MenungguPersetujuanDirektur` |
| `AT-HPS-03` | `AST-DES-019` | 2 dari 10 Tidak Layak | INTEG | Total dibekukan Rp 48 jt; tanpa Direktur; 2 aset lepas kunci |
| `AT-HPS-04` | `AST-RULE-051`, `AST-AC-013` | Contoh empat aset `01-usulan-sampai-final.md` | INTEG | Laba/rugi persis tabel contoh |
| `AT-HPS-05` | `AST-DEC-045` | Laptop hilang | INTEG | Langsung final tanpa pemindahtanganan; komponen `RUGI_PELEPASAN` |
| `AT-HPS-06` | Final | Finalisasi ambulans | INTEG | Aset `Dihapus`; buku `Dihentikan`; satu kejadian `AST-PENGHAPUSAN`; linimasa |
| `AT-HPS-07` | `AST-RULE-041` | Final 22 Agustus | INTEG | Proses Agustus tidak memuat aset |
| `AT-HPS-08` | `AST-DEC-028`, `AST-RULE-061` | Pemulihan laptop ditemukan | INTEG | Aset `Terdaftar`; kejadian `AST-PEMULIHAN`; penyesuaian penyusutan |
| `AT-HPS-09` | `CAP-10.6`, `11.5` | Riwayat dan laporan | INTEG | Kepala Unit melihat riwayat tanpa rupiah; laporan butuh `ViewAmount` |
| `AT-HPS-10` | `AST-DEC-046` | Cetak berita acara | INTEG | Memuat nama, jabatan, waktu setiap persetujuan |

## 2. Jalur gagal

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-HPS-30` | `AST-VAL-HPS-003` | Alasan Rusak Berat untuk aset Aktif | INTEG | 409 |
| `AT-HPS-31` | `AST-VAL-HPS-002` | Usulkan aset yang sedang dimutasi | INTEG | 409 |
| `AT-HPS-32` | `AST-VAL-HPS-011` | Bukan anggota tim mencatat kelayakan | INTEG | 403 |
| `AT-HPS-33` | `AST-VAL-HPS-020` | Pengusul (Kepala Bagian Aset) menyetujui | INTEG | 403 |
| `AT-HPS-34` | Tahap Direktur | Kepala Bagian Aset mencoba menyetujui final usulan Rp 60 jt | INTEG | Status menjadi `MenungguPersetujuanDirektur`, bukan `Disetujui` |
| `AT-HPS-35` | `AST-VAL-HPS-031` | Penjualan bernilai Rp 0 | INTEG | 400 |
| `AT-HPS-36` | `AST-VAL-HPS-034` | Catat pelepasan untuk alasan Hilang | INTEG | 409 |
| `AT-HPS-37` | `AST-VAL-HPS-040` | Finalisasi sebelum penyusutan bulan lalu | INTEG | 409 |
| `AT-HPS-38` | `AST-VAL-HPS-041` | Finalisasi dengan butir belum dicatat | INTEG | 409 |
| `AT-HPS-39` | `AST-VAL-HPS-091` | Batalkan sesudah pemindahtanganan | INTEG | 409 |
| `AT-HPS-40` | `AST-VAL-HPS-051` | Pulihkan ambulans terjual tanpa berita acara pembatalan | INTEG | 400 |
| `AT-HPS-41` | Status tidak sah | Ubah usulan `Final` | INTEG | 409 |
| `AT-HPS-42` | Transaksi final | Kegagalan pada butir ke-3 dari 5 saat final | INTEG | Nol butir final; usulan tetap `Dipindahtangankan` |
| `AT-HPS-43` | `AST-DEC-025` | Kepala Unit memanggil `/amounts` | INTEG | 403 |
