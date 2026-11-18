# Register Aset — Matriks Perubahan Status

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `AST-DEC-017`, `021`, `029`, `037`, `043`, `044`, `050`; `AST-DES-005`, `018` |

Kolom "Bila dilanggar" menunjuk kode pada `validation-matrix.md`.

## 1. Status pencatatan aset (`AssetRecordStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat draft | `Draft` | Petugas Aset | Isian wajib terisi | `AST-VAL-REG-001..008` |
| `Draft` | Ubah | `Draft` | Petugas Aset | — | — |
| `Draft` | Ajukan | `Diajukan` | Petugas Aset | Isian wajib lengkap | `AST-VAL-REG-001..008` |
| `Draft` | Batalkan | `Dibatalkan` | Petugas Aset | Alasan diisi | `AST-VAL-REG-010` |
| `Diajukan` | Setujui | `Terdaftar` (kondisi `Aktif`, `BelumDitempatkan`) | Kepala Bagian Aset, bukan pengaju | Kelompok aktif; nomor berhasil terbit | `AST-VAL-REG-011`, `012`, `013` |
| `Diajukan` | Tolak | `Draft` | Kepala Bagian Aset | Alasan diisi | `AST-VAL-REG-010` |
| (impor) | Batch diterapkan | `Terdaftar` | Sistem, sesudah persetujuan Akuntansi | `AST-DEC-050` | `AST-VAL-REG-060..066` |
| `Terdaftar` | Penghapusan final | `Dihapus` | Sistem atas perintah `penghapusan` | Usulan `Final` | — |
| `Dihapus` | Pemulihan disetujui | `Terdaftar` (kondisi `Aktif`, `BelumDitempatkan`) | Sistem atas perintah `penghapusan` | Pemulihan disetujui | — |

**Tidak sah:** `Terdaftar` → `Draft`; `Terdaftar` → `Dibatalkan`; `Dihapus` → apa pun selain lewat
pemulihan; `Dibatalkan` → apa pun; mengubah data aset `Terdaftar` lewat endpoint draft (`409`,
`AST-VAL-REG-014`).

## 2. Kondisi aset (`AssetCondition`, hanya pada `Terdaftar`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| kondisi mana pun kecuali `Hilang` | Tetapkan laporan | `Aktif`, `RusakRingan`, `RusakBerat`, `TidakDigunakan` | Petugas Aset | Laporan `Dilaporkan` | `AST-VAL-REG-031` |
| kondisi mana pun | Setujui Hilang | `Hilang` | Kepala Bagian Aset, bukan pelapor | Berita acara kehilangan terlampir | `AST-VAL-REG-032`, `033` |
| `Hilang` | Ditemukan kembali (laporan baru) | `Aktif` atau kondisi lain | Kepala Bagian Aset | Laporan baru beralasan "ditemukan" | `AST-VAL-REG-034` |

Aset `Hilang` atau `RusakBerat` tidak bisa dimutasi, dipinjam, atau dimanfaatkan (`AST-RULE-012`).

## 3. Laporan kondisi (`ConditionReportStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat |
|---|---|---|---|---|
| — | Laporkan | `Dilaporkan` | Penanggung jawab, Kepala Unit, Petugas Aset, atau sistem (pengembalian pinjaman, tindak lanjut opname) | Aset `Terdaftar`; foto untuk laporan unit |
| `Dilaporkan` | Tetapkan | `Ditetapkan` | Petugas Aset (Kepala Bagian Aset untuk Hilang) | Bagian 2 |
| `Dilaporkan` | Tolak | `Ditolak` | Petugas Aset | Alasan |

**Tidak sah:** mengubah laporan `Ditetapkan`/`Ditolak`; dua laporan `Dilaporkan` untuk satu aset
sekaligus (`AST-VAL-REG-035`).

## 4. Penyerahan (`HandoverStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Serahkan | `Diserahkan` | Petugas Aset | Semua butir `Terdaftar`, bukan `Hilang`, tanpa proses aktif; penempatan awal hanya untuk `BelumDitempatkan` | `AST-VAL-REG-020..024` |
| `Diserahkan` | Konfirmasi | `Dikonfirmasi` | Kepala Unit tujuan atau penanggung jawab yang dituju | — | `AST-VAL-REG-025` |
| `Diserahkan` | Tolak terima | `Ditolak` | Kepala Unit tujuan atau penanggung jawab yang dituju | Alasan | `AST-VAL-REG-010` |
| `Diserahkan` | Batalkan | `Dibatalkan` | Petugas Aset | Alasan | `AST-VAL-REG-010` |

Pada `Dikonfirmasi`: penempatan lama ditutup, penempatan baru dibuka, kolom "saat ini" aset diperbarui,
`PlacementState = Ditempatkan`, kunci proses dilepas, linimasa ditulis, Berita Acara Penyerahan dapat
dicetak. Pada `Ditolak`/`Dibatalkan`: kunci dilepas, penempatan tidak berubah.

## 5. Pemanfaatan (`UtilizationStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Buat | `Draft` | Petugas Aset | — | — |
| `Draft` | Ajukan | `Diajukan` | Petugas Aset | Aset `Terdaftar`, tanpa proses aktif, bukan `Hilang`/`RusakBerat`; tanggal dan nilai terisi | `AST-VAL-REG-040..044` |
| `Diajukan` | Setujui (cukup Kepala Bagian Aset) | `Disetujui` | Kepala Bagian Aset, bukan pengaju | Nilai < batas **dan** masa ≤ 12 bulan | `AST-VAL-REG-045` |
| `Diajukan` | Teruskan (perlu Direktur) | `MenungguDirektur` | Kepala Bagian Aset | Nilai ≥ batas **atau** masa > 12 bulan | — |
| `MenungguDirektur` | Setujui | `Disetujui` | Direktur | — | `AST-VAL-REG-045` |
| `Diajukan`, `MenungguDirektur` | Tolak | `Ditolak` | Penyetuju tahap itu | Alasan | `AST-VAL-REG-010` |
| `Draft`, `Diajukan` | Batalkan | `Dibatalkan` | Pengaju | Alasan | — |
| `Disetujui` | Tanggal mulai tiba | `Berjalan` | Sistem (pekerja harian) atau langsung saat disetujui bila tanggal mulai ≤ hari ini | — | — |
| `Berjalan` | Tanggal akhir lewat | `Berakhir` | Sistem | — | — |
| `Disetujui`, `Berjalan` | Akhiri lebih awal | `Berakhir` | Kepala Bagian Aset | Alasan | `AST-VAL-REG-010` |

Kunci proses `Pemanfaatan` dipasang saat `Diajukan` dan dilepas saat `Ditolak`, `Dibatalkan`, atau
`Berakhir`. **Tidak sah:** `Berakhir` → `Berjalan`; menyetujui pengajuan sendiri.

## 6. Batch impor (`ImportBatchStatus`)

| Dari | Tindakan | Ke | Siapa | Syarat | Bila dilanggar |
|---|---|---|---|---|---|
| — | Unggah + validasi otomatis | `GagalValidasi` atau `Tervalidasi` | Petugas Aset | `Tervalidasi` hanya bila **nol** baris salah | `AST-VAL-REG-060..066` |
| `GagalValidasi` | — (final; unggah berkas baru) | — | — | — | — |
| `Tervalidasi` | Setujui data aset | `DisetujuiBagianAset` | Kepala Bagian Aset, bukan pengunggah | — | `AST-VAL-REG-067` |
| `DisetujuiBagianAset` | Setujui angka nilai | `Diterapkan` | Kepala Akuntansi, bukan pengunggah | Penerapan seluruh baris berhasil dalam satu transaksi | `AST-VAL-REG-067`, `068` |
| `Tervalidasi`, `DisetujuiBagianAset` | Tolak | `Ditolak` | Penyetuju tahap itu | Alasan | `AST-VAL-REG-010` |
| `Tervalidasi`, `DisetujuiBagianAset` | Batalkan | `Dibatalkan` | Pengunggah | — | — |

Bila penerapan gagal di tengah jalan (misalnya satu nomor gagal terbit), seluruh transaksi dibatalkan,
batch tetap `DisetujuiBagianAset`, dan nomor yang sempat terbit hangus (`INV-PLT-002`).

## 7. Kunci proses aktif (`AssetActiveProcessKind`)

| Proses | Dipasang saat | Dilepas saat |
|---|---|---|
| `Penyerahan` | Penyerahan `Diserahkan` | `Dikonfirmasi`, `Ditolak`, `Dibatalkan` |
| `Mutasi` | Mutasi `Diajukan` (`pergerakan`) | `Diserahterimakan`, `Ditolak`, `Dibatalkan` |
| `Peminjaman` | Pinjaman `Diajukan` (`pergerakan`) | `Dikembalikan`, `Ditolak`, `Dibatalkan` |
| `Pemanfaatan` | Pemanfaatan `Diajukan` | `Ditolak`, `Dibatalkan`, `Berakhir` |
| `Penghapusan` | Usulan `Diusulkan` (`penghapusan`) | Butir dikeluarkan, `Ditolak`, `Dibatalkan`; pada `Final` aset menjadi `Dihapus` |

Memasang kunci pada aset yang kuncinya bukan `Tidak` ditolak `409` (`AST-VAL-REG-024`).
