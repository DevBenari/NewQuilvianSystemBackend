# Pergerakan Aset — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Endpoint → hak akses: kolom `Hak akses` pada [`api-contract.md`](api-contract.md). String atribut
`[AccessPermission("<Resource>", "<Action>")]`; logger: selain `GET` dicatat. Pengecualian bernama:
`GET /movement/reports/transfers/export` tidak dicatat. Cara kerja hak akses sama dengan
`../../register/contracts/permission-audit-matrix.md` bagian 1.

## 1. Peta peran ke butir hak akses

| Resource : Action | Petugas Aset | Kepala Bagian Aset | Kepala Unit | Kepala Akuntansi | Direktur | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|:---:|
| `AssetTransfer : Read` | ✓ | ✓ | ✓ (unitnya) | ✓ | ✓ | ✓ |
| `AssetTransfer : Create` | ✓ | ✓ | ✓ | | | |
| `AssetTransfer : ApproveUnit` | | | ✓ | | | |
| `AssetTransfer : ApproveAsset` | | ✓ | | | | |
| `AssetTransfer : Receive` | | | ✓ | | | |
| `AssetTransfer : Cancel` | ✓ | ✓ | ✓ (bila pengaju) | | | |
| `AssetLoan : Read` | ✓ | ✓ | ✓ (unitnya) | | ✓ | ✓ |
| `AssetLoan : Create`, `Receive`, `Return`, `Extend` | `Create` saja | `Create` saja | ✓ | | | |
| `AssetLoan : Approve`, `ConfirmReturn` | | | ✓ | | | |
| `AssetLoan : Cancel` | ✓ | ✓ | ✓ | | | |
| `AssetReport : Read` (laporan mutasi) | Mengikuti `../../register/contracts/permission-audit-matrix.md` bagian 2; resource ini milik `register` dan dipakai bersama seluruh laporan modul | | | | | |

## 2. Kewenangan yang tidak dapat dijaga mesin hak akses

| Aturan | Penjaga | Yang tidak dijaga | Risiko |
|---|---|---|---|
| Kepala Unit yang benar per langkah (asal, tujuan, pemilik, peminjam) | `AssetActorResolver` + perbandingan unit dokumen | Penugasan HR yang belum diperbarui | Kepala Unit lama masih dapat bertindak (`AST-UNK-004`) |
| Pengaju ≠ penyetuju | Perbandingan `RequestedBy` | Satu orang dengan dua akun | Risiko organisasi |
| Tahap 1 otomatis hanya bila pengaju benar-benar Kepala Unit asal | `AssetTransferService` memeriksa penugasan manajerial **pada saat pengajuan** | — | — |

## 3. Audit

Kolom pelaku dan waktu per langkah pada `AstTransfer` (`RequestedBy`, `UnitApprovedBy`, `AssetHeadApprovedBy`,
`ReceivedBy`, `RejectedBy`, `CancelledBy` beserta waktunya) dan `AstLoan`; linimasa aset untuk setiap
perpindahan; logger hanya `EntityId`, controller, action, status.

## 4. Kolom sensitif

Tidak ada kolom bertanda sensitif pada sub-modul ini. Tidak ada rupiah.
