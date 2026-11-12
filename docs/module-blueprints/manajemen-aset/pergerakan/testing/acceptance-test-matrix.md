# Pergerakan Aset — Matriks Uji Penerimaan

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |

## 1. Jalur berhasil

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-MOV-01` | `AST-RULE-020`, `021` | Mutasi IGD → ICU oleh Petugas Aset, dua persetujuan, diterima | INTEG | Penempatan berpindah tepat saat diterima; linimasa `Dimutasi` |
| `AT-MOV-02` | `AST-DEC-048` | Mutasi diajukan Kepala IGD | INTEG | Status awal `DisetujuiUnitAsal`, `IsUnitApprovalByRequester=true` |
| `AT-MOV-03` | `AST-RULE-060` | Mutasi koreksi ICU → HCU merujuk mutasi asal | INTEG | Dua mutasi tampil di linimasa |
| `AT-MOV-04` | `AST-RULE-022` | Pinjam 1–3 Juni, belum kembali 4 Juni | INTEG | `isOverdue=true`; kartu dashboard bertambah |
| `AT-MOV-05` | `AST-RULE-022` | Perpanjangan disetujui | INTEG | `PlannedReturnDate` baru; `isOverdue=false` |
| `AT-MOV-06` | `AST-RCG-G07` | Kembali dengan kondisi Rusak Ringan | INTEG | Laporan kondisi sumber `PengembalianPinjaman` terbuka |
| `AT-MOV-07` | `CAP-11.2` | Laporan mutasi April | INTEG | Baris sesuai filter; ekspor berisi data sama |
| `AT-MOV-08` | `AST-RULE-046` (bersama `nilai`) | Mutasi diterima 15 April | INTEG | Penyusutan April dibebankan ke pusat biaya ICU |

## 2. Jalur gagal

| ID | Requirement | Skenario | Jenis | Bukti |
|---|---|---|---|---|
| `AT-MOV-20` | `AST-VAL-MOV-002` | Mutasi ventilator Rusak Berat | INTEG | 409 |
| `AT-MOV-21` | `AST-VAL-MOV-003` | Mutasi aset yang sedang dipinjam | INTEG | 409 menyebut proses Peminjaman |
| `AT-MOV-22` | `AST-VAL-MOV-004` | Satu mutasi berisi aset IGD dan ICU | INTEG | 400 |
| `AT-MOV-23` | `AST-VAL-MOV-005` | Unit tujuan = unit asal | INTEG | 400 |
| `AT-MOV-24` | `AST-VAL-MOV-006` (sejak `MVP-3`) | Ajukan saat opname IGD `Berjalan` | INTEG | 409 |
| `AT-MOV-25` | `AST-VAL-MOV-010` | Kepala ICU menyetujui tahap unit asal IGD | INTEG | 403 |
| `AT-MOV-26` | `AST-VAL-MOV-011` | Kepala Bagian Aset yang mengajukan menyetujui tahap Bagian Aset | INTEG | 403 |
| `AT-MOV-27` | `AST-VAL-MOV-012` | Kepala HCU menerima mutasi ke ICU | INTEG | 403 |
| `AT-MOV-28` | `AST-VAL-MOV-013` | Terima dengan ruangan milik IGD | INTEG | 400 |
| `AT-MOV-29` | `AST-DES-024` | Unit tujuan menolak terima | INTEG | `Ditolak`; aset tetap di IGD; kunci lepas |
| `AT-MOV-30` | Status tidak sah | Batalkan mutasi `Diserahterimakan` | INTEG | 409 |
| `AT-MOV-31` | `AST-VAL-MOV-016` | Dua penyetuju bersamaan | INTEG | Satu 200, satu 409 |
| `AT-MOV-32` | `AST-VAL-MOV-020` | Tanggal kembali sebelum tanggal mulai | INTEG | 400 |
| `AT-MOV-33` | Status tidak sah | Batalkan pinjaman `Dipinjam` | INTEG | 409 |
| `AT-MOV-34` | `AST-VAL-MOV-023` | Perpanjangan kedua saat pertama terbuka | INTEG | 409 |
| `AT-MOV-35` | `AST-UNK-004` | Kepala Unit tanpa penugasan manajerial | INTEG | 403 pada persetujuan |
| `AT-MOV-36` | Keterjangkauan | Seluruh 8 layar dapat dicapai | UI | — |
