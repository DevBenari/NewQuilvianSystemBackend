# Penghapusan & Pelepasan Aset — Kontrak API

| Field | Value |
|---|---|
| `last_changed_in` | `AST-HPS-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `CAP-10.1`–`10.6`, `11.5`, `CAP-EXT-03`; `AST-DEC-020`, `022`, `028`, `045`, `049` |

**Semua endpoint Rencana (belum tersedia).** Kode status umum: `../../register/contracts/api-contract.md` 1.
Rupiah (nilai buku, harga pelepasan, laba/rugi) hanya dari endpoint ber-`ViewAmount`.

### 2.1 Corporate / Asset Management / Disposal / Disposal Proposal

Base URL: `api/v1/corporate/asset-management/disposal/proposals`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar usulan (menu 10.1–10.5 dengan saringan status) | `AssetDisposal : Read` | `DisposalPagedQuery` (`status`, `reason`, `awaitingMe`) | `PagedResult<DisposalProposalResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian tanpa rupiah | `AssetDisposal : Read` | - | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/amounts` | Nilai buku, total, batas, harga pelepasan, laba/rugi | `AssetDisposal : ViewAmount` | - | `DisposalAmountResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengusulkan | `AssetDisposal : Create` | `DisposalProposalCreateRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reviewers` | Menunjuk tim pemeriksa | `AssetDisposal : AssignReviewer` | `DisposalReviewerAssignRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/feasibility` | Mencatat hasil kelayakan per butir dan mengajukan ke persetujuan (menu 10.2) | `AssetDisposal : RecordFeasibility` | `DisposalFeasibilityRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Kepala Bagian Aset setuju/tolak (menu 10.3) | `AssetDisposal : ApproveAsset` | `DisposalDecisionRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-director` | Direktur setuju/tolak | `AssetDisposal : ApproveDirector` | `DisposalDecisionRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/items/{itemId}/transfer` | Mencatat pemindahtanganan satu butir (menu 10.4) | `AssetDisposal : RecordTransfer` | `DisposalTransferRecordRequest` (multipart) | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finalize` | Finalisasi dan terbitkan Berita Acara Penghapusan (menu 10.5) | `AssetDisposal : Finalize` | - | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan sebelum pemindahtanganan | `AssetDisposal : Cancel` | `DisposalDecisionRequest` | `DisposalProposalResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | PDF Berita Acara Penghapusan (sesudah `Final`) atau Berita Acara Pemeriksaan Kelayakan | `AssetDisposal : Read` | `type` | `application/pdf` | **Rencana (belum tersedia)** |

Arti khusus `409` pada `/finalize`: penyusutan bulan sebelumnya belum diproses, atau ada butir yang belum
dicatat pemindahtanganannya.

### 2.2 Corporate / Asset Management / Disposal / Asset Restoration

Base URL: `api/v1/corporate/asset-management/disposal/restorations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pemulihan | `AssetRestoration : Read` | `status`, `page`, `pageSize` | `PagedResult<RestorationResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian | `AssetRestoration : Read` | - | `RestorationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengajukan pemulihan butir `Final` | `AssetRestoration : Create` | `RestorationCreateRequest` (multipart) | `RestorationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decide` | Kepala Bagian Aset setuju/tolak | `AssetRestoration : Approve` | `RestorationDecisionRequest` | `RestorationResponse` | **Rencana (belum tersedia)** |

### 2.3 Corporate / Asset Management / Disposal / Disposed Asset

Base URL: `api/v1/corporate/asset-management/disposal/disposed-assets`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Riwayat aset dihapus (menu 10.6): nomor, nama, alasan, metode, tanggal final, nomor usulan | `AssetDisposal : Read` | `DisposalPagedQuery` + rentang tanggal | `PagedResult<DisposedAssetResponse>` | **Rencana (belum tersedia)** |

### 2.4 Corporate / Asset Management / Disposal / Report

Base URL: `api/v1/corporate/asset-management/disposal/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/disposals` | Laporan penghapusan (11.5) beserta nilai buku, hasil, laba/rugi | `AssetDisposal : ViewAmount` | `DisposalPagedQuery` + periode | `PagedResult<DisposedAssetResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/disposals/export` | Ekspor Excel | `AssetDisposal : ViewAmount` | sama | `xlsx` | **Rencana (belum tersedia)** |
