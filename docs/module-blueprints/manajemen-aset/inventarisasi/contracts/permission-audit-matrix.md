# Inventarisasi Fisik — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Endpoint → hak akses: kolom `Hak akses` pada [`api-contract.md`](api-contract.md). Logger: selain `GET`
dicatat; pengecualian bernama: `GET /reports/stock-takes/export` tidak dicatat. Cara kerja hak akses sama
dengan `../../register/contracts/permission-audit-matrix.md` bagian 1.

## 1. Peta peran

| Resource : Action | Petugas Aset | Kepala Bagian Aset | Tim Inventarisasi | Kepala Unit | Direktur | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| `AssetStockTake : Read` | ✓ | ✓ | ✓ | ✓ (unitnya) | ✓ | ✓ |
| `AssetStockTake : Create`, `Start` | ✓ | ✓ | `Start` bila ketua | | | |
| `AssetStockTake : Scan` | ✓ | ✓ | ✓ | | | |
| `AssetStockTake : Close`, `Submit` | ✓ | ✓ | ✓ (ketua) | | | |
| `AssetStockTake : Verify`, `Cancel` | | ✓ | | | | |
| `AssetStockTakeFollowUp : Read` | ✓ | ✓ | | | ✓ | ✓ |
| `AssetStockTakeFollowUp : Process` | ✓ | ✓ | | | | |
| `AssetStockTakeFollowUp : Dismiss` | | ✓ | | | | |
| `AssetReport : Read` (laporan inventaris) | Mengikuti `../../register/contracts/permission-audit-matrix.md` bagian 2; resource milik `register` | | | | | |

"Tim Inventarisasi" adalah peran Quilvian yang diberikan kepada pegawai yang biasa ditugaskan memindai;
keanggotaan sesi tetap diperiksa service.

## 2. Kewenangan yang tidak dapat dijaga mesin hak akses

| Aturan | Penjaga | Yang tidak dijaga | Risiko |
|---|---|---|---|
| Hanya anggota tim sesi yang memindai | `StockTakeScanService` mencocokkan pegawai pengguna dengan `AstStockTakeTeamMember` | Anggota yang memindai tanpa benar-benar memeriksa fisik | Pemindaian asal; dimitigasi foto opsional dan verifikasi |
| Ketua tim menutup/mengajukan | Penanda `IsLeader` | — | — |
| Verifikator bukan anggota tim | `StockTakeSessionService` | Kepala Bagian Aset tunggal yang juga harus ikut memindai | Harus menunjuk verifikator lain; bila tidak ada, sesi tidak dapat diverifikasi (sengaja) |

## 3. Audit

Pelaku dan waktu per langkah sesi; setiap pindaian menyimpan pemindai, waktu, ruangan, kondisi, foto, dan
apakah input manual; pindaian lama tidak dihapus (`AST-DES-029`); linimasa `Diopname` per aset.

## 4. Kolom sensitif

Tidak ada. Foto disimpan lewat `AstAttachment` (`StoredPath` sensitif, milik `register`).
