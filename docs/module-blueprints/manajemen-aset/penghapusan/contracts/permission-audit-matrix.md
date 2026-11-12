# Penghapusan & Pelepasan Aset — Matriks Hak Akses dan Audit

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |

Endpoint → hak akses: kolom `Hak akses` pada [`api-contract.md`](api-contract.md). Logger: selain `GET`
dicatat; pengecualian bernama: `GET /reports/disposals/export` tidak dicatat. Cara kerja hak akses sama
dengan `../../register/contracts/permission-audit-matrix.md` bagian 1.

## 1. Peta peran

| Resource : Action | Petugas Aset | Kepala Bagian Aset | Kepala Unit | Tim Pemeriksa | Kepala Akuntansi | Direktur | Auditor |
|---|:---:|:---:|:---:|:---:|:---:|:---:|:---:|
| `AssetDisposal : Read` | ✓ | ✓ | ✓ (unitnya) | ✓ | ✓ | ✓ | ✓ |
| `AssetDisposal : ViewAmount` | ✓ | ✓ | | | ✓ | ✓ | |
| `AssetDisposal : Create` | ✓ | ✓ | ✓ | | | | |
| `AssetDisposal : AssignReviewer` | ✓ | ✓ | | | | | |
| `AssetDisposal : RecordFeasibility` | | | | ✓ | | | |
| `AssetDisposal : ApproveAsset`, `Finalize` | | ✓ | | | | | |
| `AssetDisposal : ApproveDirector` | | | | | | ✓ | |
| `AssetDisposal : RecordTransfer` | ✓ | ✓ | | | | | |
| `AssetDisposal : Cancel` | ✓ | ✓ | ✓ (bila pengusul) | | | | |
| `AssetRestoration : Read` | ✓ | ✓ | | | ✓ | ✓ | ✓ |
| `AssetRestoration : Create` | ✓ | | | | | | |
| `AssetRestoration : Approve` | | ✓ | | | | | |

"Tim Pemeriksa" adalah peran Quilvian untuk pegawai yang biasa ditunjuk memeriksa kelayakan;
keanggotaan per usulan tetap diperiksa service.

## 2. Kewenangan yang tidak dapat dijaga mesin hak akses

| Aturan | Penjaga | Yang tidak dijaga | Risiko |
|---|---|---|---|
| Penyetuju bukan pengusul/pemeriksa | `AssetDisposalService` (`AST-DES-039`) | Kepala Bagian Aset tunggal yang juga mengusulkan | Usulan harus diajukan Petugas Aset atau Kepala Unit; bila tidak ada Kepala Bagian Aset lain, usulan tidak dapat disetujui (sengaja) |
| Tingkat penyetuju dari total nilai buku | Batas dan total dibekukan (`AST-DES-040`) | Pemecahan usulan besar menjadi beberapa usulan kecil | Terlihat di laporan penghapusan per periode; tidak dicegah mesin |
| Kepala Unit hanya untuk unitnya | `AssetActorResolver` | Penugasan HR basi | `AST-UNK-004` |

## 3. Audit

Pelaku dan waktu setiap langkah; hasil kelayakan per butir beserta pemeriksanya; batas dan total yang
dipakai; angka final per butir dibekukan; Berita Acara Penghapusan memuat nama, jabatan, dan waktu setiap
persetujuan (`AST-DEC-046`); linimasa aset `Dihapus`/`Dipulihkan`.

## 4. Kolom sensitif

`TotalNetBookValueAtSubmission`, `DirectorThresholdUsed`, `NetBookValueAtProposal`, `ProceedsAmount`,
`NetBookValueAtFinal`, `GainLossAmount`, `CounterpartyName`. Tidak masuk logger; hanya lewat endpoint
ber-`ViewAmount`.
