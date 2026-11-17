# Pergerakan Aset — Matriks Perubahan Status

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-DEC-011`, `012`, `048`; `AST-RULE-012`, `020..023`, `060`; `AST-DES-021..026` |

## 1. Mutasi (`TransferStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Ajukan (oleh Petugas Aset) | `Diajukan` | Petugas Aset | Semua butir `Terdaftar`, `Ditempatkan` di unit asal yang sama, bukan `Hilang`/`RusakBerat`, tanpa proses aktif; unit asal dan tujuan tidak sedang opname (sejak `MVP-3`) | `AST-VAL-MOV-001..006` |
| — | Ajukan (oleh Kepala Unit asal) | `DisetujuiUnitAsal` | Kepala Unit asal | Syarat yang sama; tahap 1 tercatat atas namanya (`AST-DEC-048`) | sama |
| `Diajukan` | Setujui unit | `DisetujuiUnitAsal` | Kepala Unit asal | Bukan pengaju | `AST-VAL-MOV-010`, `011` |
| `DisetujuiUnitAsal` | Setujui Bagian Aset | `DisetujuiBagianAset` | Kepala Bagian Aset | Bukan pengaju | `AST-VAL-MOV-011` |
| `DisetujuiBagianAset` | Terima | `Diserahterimakan` | Kepala Unit tujuan | Ruangan milik unit tujuan dan penanggung jawab aktif terisi | `AST-VAL-MOV-012`, `013` |
| `Diajukan`, `DisetujuiUnitAsal` | Tolak | `Ditolak` | Penyetuju tahap itu | Alasan | `AST-VAL-MOV-014` |
| `DisetujuiBagianAset` | Tolak terima | `Ditolak` | Kepala Unit tujuan | Alasan (`AST-DES-024`) | `AST-VAL-MOV-014` |
| `Diajukan`, `DisetujuiUnitAsal`, `DisetujuiBagianAset` | Batalkan | `Dibatalkan` | Pengaju atau Kepala Bagian Aset | Alasan (`AST-RCG-G05`) | `AST-VAL-MOV-014` |

Pada `Diserahterimakan`: `AssetPlacementService.ApplyPlacement` per butir, linimasa `Dimutasi`, kunci
lepas; butir yang diterima dengan kondisi berbeda membuka laporan kondisi bersumber sistem.
Pada `Ditolak`/`Dibatalkan`: kunci lepas, penempatan tidak berubah.

**Tidak sah:** `Diserahterimakan` → apa pun (koreksi = mutasi baru dengan `CorrectsTransferId`);
`Ditolak`/`Dibatalkan` → apa pun; melompati tahap Bagian Aset.

## 2. Peminjaman (`LoanStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Ajukan | `Diajukan` | Kepala Unit peminjam atau Petugas Aset | Aset seperti mutasi; unit peminjam ≠ unit pemilik; rencana tanggal kembali ≥ tanggal mulai | `AST-VAL-MOV-001..006`, `020` |
| — | Ajukan oleh Kepala Unit pemilik | `Disetujui` | Kepala Unit pemilik | `AST-DES-021` | sama |
| `Diajukan` | Setujui | `Disetujui` | Kepala Unit pemilik, bukan pengaju | — | `AST-VAL-MOV-010`, `011` |
| `Diajukan` | Tolak | `Ditolak` | Kepala Unit pemilik | Alasan | `AST-VAL-MOV-014` |
| `Disetujui` | Ambil | `Dipinjam` | Kepala Unit peminjam | — | `AST-VAL-MOV-021` |
| `Dipinjam` | Kembalikan | `MenungguKonfirmasiKembali` | Kepala Unit peminjam | — | `AST-VAL-MOV-021` |
| `MenungguKonfirmasiKembali` | Konfirmasi kembali | `Dikembalikan` | Kepala Unit pemilik | Kondisi per butir diisi | `AST-VAL-MOV-022` |
| `Diajukan`, `Disetujui` | Batalkan | `Dibatalkan` | Pengaju atau Kepala Unit pemilik | Alasan | `AST-VAL-MOV-014` |

**Keadaan turunan Terlambat** (`AST-DES-022`): `Dipinjam` atau `MenungguKonfirmasiKembali` dengan
`PlannedReturnDate` < hari ini. Contoh: pinjam 1–3 Juni, belum dikembalikan → tampil Terlambat sejak 4 Juni.

**Tidak sah:** `Dikembalikan` → apa pun; `Dipinjam` → `Dibatalkan` (aset sudah berpindah tangan; harus
dikembalikan).

## 3. Perpanjangan (`LoanExtensionStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat |
|---|---|---|---|---|
| — | Minta | `Diajukan` | Kepala Unit peminjam | Pinjaman `Dipinjam`; tanggal baru > tanggal kembali berjalan; tidak ada perpanjangan `Diajukan` lain |
| `Diajukan` | Setujui | `Disetujui` | Kepala Unit pemilik | `PlannedReturnDate` pinjaman diganti; tanggal lama tersimpan di perpanjangan |
| `Diajukan` | Tolak | `Ditolak` | Kepala Unit pemilik | Alasan |
