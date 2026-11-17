# Penghapusan & Pelepasan Aset — Kontrak Integrasi

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Tidak ada sistem di luar Quilvian. Akibat keuangan dikirim lewat kotak keluar `nilai` (bagian 3 kontrak
integrasi `nilai`). Uang hasil penjualan diterima Finance tanpa integrasi otomatis pada MVP; modul ini
menyimpan nomor rujukan penerimaannya (`AST-RULE-052`).

| ID | Arah | Mitra | Service | Saat | Jaminan |
|---|---|---|---|---|---|
| `INT-HPS-01` | Memanggil | `register` | `AssetProcessLockService` | Usulkan; butir dikeluarkan; ditolak/dibatalkan | Ikut transaksi |
| `INT-HPS-02` | Memanggil | `register` | `AssetRecordService.MarkDisposed` / `Restore` | Final / pemulihan disetujui | Ikut transaksi |
| `INT-HPS-03` | Memanggil | `register` | `AssetHistoryService.Append` | Diusulkan, final, dipulihkan | Ikut transaksi |
| `INT-HPS-04` | Memanggil | `register` | `AssetActorResolver` | Usulan Kepala Unit | Unit dari penugasan aktif |
| `INT-HPS-05` | Memanggil | `nilai` | `AssetBookService.GetNetBookValue`, `Stop`, `Reactivate` | Ajukan ke persetujuan; final; pemulihan | Nilai buku per tanggal |
| `INT-HPS-06` | Memanggil | `nilai` | `AssetValuationParameterQueryService` | Ajukan ke persetujuan | Batas Direktur berlaku |
| `INT-HPS-07` | Memanggil | `nilai` | `AssetValuationQueryService.IsPeriodProcessed` | Finalisasi | `AST-DES-013` |
| `INT-HPS-08` | Memanggil | `nilai` | `AssetAccountingEventService.StageDisposal` / `StageRestoration` | Final / pemulihan | Satu kejadian per butir; komponen sesuai `../../nilai/contracts/integration-contract.md` bagian 3 |
| `INT-HPS-09` | Dibaca | `register` (`AstConditionReport`) | — | Usulan alasan Hilang | Berita acara kehilangan dirujuk dari laporan kondisi yang menyetujui status Hilang |

**Contoh.** Usulan `AST-HPS-000012` berisi ambulans `AST-KEND-000004`, dijual Rp 35.000.000 pada 20 Agustus
2028. Finalisasi 22 Agustus (penyusutan Juli sudah diproses): nilai buku Rp 20.000.000, laba Rp 15.000.000;
aset `Dihapus`; kejadian `AST-PENGHAPUSAN` komponen `PEROLEHAN_KEND` 400.000.000, `AKUMULASI_KEND`
380.000.000, `HASIL_PELEPASAN` 35.000.000, `LABA_PELEPASAN` 15.000.000. Proses penyusutan Agustus tidak lagi
memuat ambulans itu.
