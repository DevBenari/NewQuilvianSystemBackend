# Pergerakan Aset — Kontrak API

| Field | Value |
|---|---|
| `last_changed_in` | `AST-MOV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Dampak kompatibilitas | Endpoint baru seluruhnya |
| Traceability | `CAP-01.4`, `05.1`–`05.5`, `11.2`; `AST-DEC-011`, `012`, `048` |

**Semua endpoint Rencana (belum tersedia).** Kode status umum sama dengan
`../../register/contracts/api-contract.md` bagian 1.

### 2.1 Corporate / Asset Management / Movement / Asset Transfer

Base URL: `api/v1/corporate/asset-management/movement/transfers`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar mutasi yang melibatkan unit pengguna (asal atau tujuan) | `AssetTransfer : Read` | `AssetTransferPagedQuery` (`status`, `sourceOrganizationUnitId`, `targetOrganizationUnitId`, `awaitingMe`) | `PagedResult<AssetTransferResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian mutasi | `AssetTransfer : Read` | - | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengajukan mutasi | `AssetTransfer : Create` | `AssetTransferCreateRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-unit` | Kepala Unit asal setuju/tolak | `AssetTransfer : ApproveUnit` | `AssetTransferDecisionRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-asset` | Bagian Aset setuju/tolak | `AssetTransfer : ApproveAsset` | `AssetTransferDecisionRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/receive` | Kepala Unit tujuan menerima; penempatan berpindah | `AssetTransfer : Receive` | `AssetTransferReceiveRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject-receipt` | Kepala Unit tujuan menolak terima | `AssetTransfer : Receive` | `AssetTransferDecisionRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Pengaju atau Kepala Bagian Aset membatalkan sebelum diterima | `AssetTransfer : Cancel` | `AssetTransferDecisionRequest` | `AssetTransferResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | PDF Berita Acara Serah Terima Mutasi | `AssetTransfer : Read` | - | `application/pdf` | **Rencana (belum tersedia)** |

Arti khusus: `409` pada `POST /` dapat berarti aset sedang dalam proses lain, berstatus `Hilang`/`RusakBerat`,
atau unit sedang opname; `403` pada `/approve-asset` dapat berarti Anda pengaju mutasi ini.

### 2.2 Corporate / Asset Management / Movement / Asset Loan

Base URL: `api/v1/corporate/asset-management/movement/loans`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pinjaman unit pengguna (pemilik atau peminjam); saringan `isOverdue` | `AssetLoan : Read` | `AssetLoanPagedQuery` | `PagedResult<AssetLoanResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian pinjaman beserta perpanjangan | `AssetLoan : Read` | - | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengajukan pinjaman | `AssetLoan : Create` | `AssetLoanCreateRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decide` | Kepala Unit pemilik setuju/tolak | `AssetLoan : Approve` | `AssetLoanDecisionRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/pickup` | Peminjam mengonfirmasi menerima aset | `AssetLoan : Receive` | - | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/return` | Peminjam menyerahkan kembali | `AssetLoan : Return` | - | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/confirm-return` | Pemilik mengonfirmasi kembali dan kondisi per butir | `AssetLoan : ConfirmReturn` | `AssetLoanReturnRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/extensions` | Meminta perpanjangan | `AssetLoan : Extend` | `AssetLoanExtensionRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/extensions/{extensionId}/decide` | Pemilik memutuskan perpanjangan | `AssetLoan : Approve` | `AssetLoanDecisionRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan sebelum diambil | `AssetLoan : Cancel` | `AssetLoanDecisionRequest` | `AssetLoanResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | PDF Berita Acara Peminjaman/Pengembalian | `AssetLoan : Read` | - | `application/pdf` | **Rencana (belum tersedia)** |

### 2.3 Corporate / Asset Management / Movement / Dashboard

Base URL: `api/v1/corporate/asset-management/movement/dashboard`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/summary` | Kartu 1.4: mutasi per status berjalan, pinjaman aktif, pinjaman terlambat | `AssetTransfer : Read` | `organizationUnitId` opsional | `AssetMovementSummaryResponse` | **Rencana (belum tersedia)** |

### 2.4 Corporate / Asset Management / Movement / Report

Base URL: `api/v1/corporate/asset-management/movement/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/transfers` | Laporan mutasi (11.2): per periode, unit asal/tujuan, status | `AssetReport : Read` | `AssetTransferPagedQuery` + rentang tanggal | `PagedResult<AssetTransferResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/transfers/export` | Ekspor Excel | `AssetReport : Read` | sama | `xlsx` | **Rencana (belum tersedia)** |
