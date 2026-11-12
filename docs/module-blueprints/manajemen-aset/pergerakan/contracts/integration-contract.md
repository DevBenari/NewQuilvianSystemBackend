# Pergerakan Aset — Kontrak Integrasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Tidak ada sistem di luar Quilvian. Tidak ada kejadian ke Akuntansi: perpindahan pusat biaya penyusutan
dibaca `nilai` dari riwayat penempatan pada akhir bulan (`AST-RULE-046`), bukan dikirim dari sini.

| ID | Arah | Mitra | Service | Saat | Jaminan |
|---|---|---|---|---|---|
| `INT-MOV-01` | Memanggil | `register` | `AssetProcessLockService.Acquire/Release` | Ajukan; tolak/batal/selesai | Ikut transaksi; `409` bila kunci terpakai |
| `INT-MOV-02` | Memanggil | `register` | `AssetPlacementService.ApplyPlacement` | Mutasi diterima | Satu transaksi dengan perubahan status mutasi |
| `INT-MOV-03` | Memanggil | `register` | `AssetConditionService.OpenReport` (sumber `PengembalianPinjaman`) | Kondisi kembali/diterima berbeda | Laporan tetap harus ditetapkan Petugas Aset |
| `INT-MOV-04` | Memanggil | `register` | `AssetActorResolver` | Setiap langkah | Kepala Unit dari penugasan manajerial aktif; fail-closed (`AST-UNK-004`) |
| `INT-MOV-05` | Memanggil | `register` | `AssetHistoryService.Append` | Diajukan, diserahterimakan, dipinjam, dikembalikan | Ikut transaksi |
| `INT-MOV-06` | Memanggil (sejak `MVP-3`) | `inventarisasi` | `StockTakeGateQueryService.IsUnitUnderStockTake` | Ajukan | Baca saja |
| `INT-MOV-07` | Dipanggil | `inventarisasi` | `AssetTransferService.CreateFromStockTake` | Tindak lanjut beda unit (`AST-DES-007`) | Membuat mutasi `Diajukan` beralasan `KoreksiOpname`; persetujuan tetap berjalan normal |
| `INT-MOV-08` | Dibaca | `inventarisasi` | `MovementQueryService.HasInFlightForUnit` | Sesi opname dimulai (`AST-DES-008`) | Baca saja |
| `INT-MOV-09` | Dibaca | `penghapusan` | — (memakai kunci proses `register`) | Usulan penghapusan | Aset dalam mutasi/pinjaman tidak dapat diusulkan |

Contoh `INT-MOV-02`: mutasi `AST-MUT-000045` (monitor IGD → ICU) diterima Kepala Ruang ICU pada 15 April
pukul 10.12. Penempatan IGD ditutup 10.12, penempatan ICU dibuka 10.12, linimasa `Dimutasi` tercatat, dan
pada proses penyusutan April beban monitor dibebankan ke pusat biaya ICU.
