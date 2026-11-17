# Penghapusan & Pelepasan Aset — Matriks Perubahan Status

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

## 1. Usulan penghapusan (`DisposalStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Usulkan | `Diusulkan` | Petugas Aset, Kepala Unit (aset unitnya) | Butir `Terdaftar`, tanpa proses aktif, sesuai syarat alasan (`AST-DES-038`) | `AST-VAL-HPS-001..005` |
| `Diusulkan` | Tunjuk tim | `DalamPemeriksaan` | Petugas Aset, Kepala Bagian Aset | ≥ 1 pegawai aktif | `AST-VAL-HPS-010` |
| `DalamPemeriksaan` | Catat kelayakan dan ajukan | `MenungguPersetujuan` | Anggota tim | Semua butir dinilai; minimal satu `Layak`; total nilai buku dan batas dibekukan | `AST-VAL-HPS-011`, `012` |
| `DalamPemeriksaan` | Catat kelayakan, semua Tidak Layak | `Ditolak` | Anggota tim | — | — |
| `MenungguPersetujuan` | Setujui, total ≤ batas | `Disetujui` | Kepala Bagian Aset (`AST-DES-039`) | — | `AST-VAL-HPS-020` |
| `MenungguPersetujuan` | Setujui, total > batas | `MenungguPersetujuanDirektur` | Kepala Bagian Aset | — | `AST-VAL-HPS-020` |
| `MenungguPersetujuanDirektur` | Setujui | `Disetujui` | Direktur | — | `AST-VAL-HPS-021` |
| `MenungguPersetujuan`, `MenungguPersetujuanDirektur` | Tolak | `Ditolak` | Penyetuju tahap itu | Alasan | `AST-VAL-HPS-090` |
| `Disetujui` | Catat pemindahtanganan semua butir | `Dipindahtangankan` | Petugas Aset | Alasan bukan `Hilang` | `AST-VAL-HPS-030..034` |
| `Disetujui` (alasan `Hilang`) | Finalisasi | `Final` | Kepala Bagian Aset | `AST-DES-013` | `AST-VAL-HPS-040` |
| `Dipindahtangankan` | Finalisasi | `Final` | Kepala Bagian Aset | `AST-DES-013` | `AST-VAL-HPS-040`, `041` |
| `Diusulkan`, `DalamPemeriksaan`, `MenungguPersetujuan`, `MenungguPersetujuanDirektur`, `Disetujui` | Batalkan | `Dibatalkan` | Pengusul (sampai `DalamPemeriksaan`) atau Kepala Bagian Aset | Alasan; belum ada pemindahtanganan tercatat (`AST-DES-041`) | `AST-VAL-HPS-090`, `091` |

**Akibat finalisasi**, per butir tidak dikeluarkan: nilai buku saat final dibekukan; laba/rugi = harga
pelepasan − nilai buku; aset `Dihapus` (`register`); buku `Dihentikan` dan entri `Penghapusan` (`nilai`);
kejadian `AST-PENGHAPUSAN` ke kotak keluar; linimasa `Dihapus`; Berita Acara Penghapusan dapat dicetak.
Kunci butir yang dikeluarkan atau usulan yang `Ditolak`/`Dibatalkan` dilepas.

**Tidak sah:** `Final` → apa pun (koreksi = Pemulihan Aset); melompati tahap Direktur bila total di atas
batas; mencatat pemindahtanganan untuk alasan `Hilang`; finalisasi sebelum semua butir dicatat.

**Contoh berangka.** Batas Rp 50.000.000. Usulan 10 kursi roda × nilai buku Rp 6.000.000 = Rp 60.000.000
→ `MenungguPersetujuanDirektur`. Bila tim menilai 2 kursi Tidak Layak, total dibekukan untuk 8 kursi =
Rp 48.000.000 → cukup Kepala Bagian Aset.

## 2. Pemulihan (`RestorationStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Akibat |
|---|---|---|---|---|---|
| — | Ajukan | `Diajukan` | Petugas Aset | Butir `Final`; tidak ada pemulihan lain yang terbuka; lampiran sesuai `AST-DES-037` | — |
| `Diajukan` | Setujui | `Disetujui` | Kepala Bagian Aset, bukan pengaju | — | Aset `Terdaftar` kondisi `Aktif`, `BelumDitempatkan`; buku `Aktif` dengan nilai saat dihapus; penyusutan bulan yang terlewat dikejar (`AST-DES-010`); kejadian `AST-PEMULIHAN`; linimasa `Dipulihkan` |
| `Diajukan` | Tolak | `Ditolak` | Kepala Bagian Aset | Alasan | — |

**Tidak sah:** memulihkan butir yang sudah dipulihkan; memulihkan butir dari usulan yang bukan `Final`.
