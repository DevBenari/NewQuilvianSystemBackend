# Nilai Aset — Kontrak API

| Field | Value |
|---|---|
| `last_changed_in` | `AST-NLI-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `CAP-09.1`–`09.5`, `11.6`, `CAP-EXT-02`, `CAP-EXT-04` |

**Semua endpoint Rencana (belum tersedia).** Kode status umum: `../../register/contracts/api-contract.md` 1.
Seluruh endpoint sub-modul ini memuat rupiah, sehingga tidak ada yang dapat dibaca peran tanpa hak nilai.

### 2.1 Corporate / Asset Management / Valuation / Valuation Parameter

Base URL: `api/v1/corporate/asset-management/valuation/parameters`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Riwayat parameter | `AssetValuationParameter : Read` | `page`, `pageSize` | `PagedResult<ValuationParameterResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/current` | Parameter yang berlaku hari ini dan yang akan berlaku | `AssetValuationParameter : Read` | - | `ValuationParameterResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menetapkan parameter baru (berlaku bulan berikutnya) | `AssetValuationParameter : Update` | `ValuationParameterSetRequest` | `ValuationParameterResponse` | **Rencana (belum tersedia)** |

### 2.2 Corporate / Asset Management / Valuation / Depreciation Policy

Base URL: `api/v1/corporate/asset-management/valuation/depreciation-policies`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Kebijakan berlaku per kelompok | `AssetDepreciationPolicy : Read` | `assetGroupId` | `PagedResult<DepreciationPolicyResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{assetGroupId}/history` | Riwayat kebijakan satu kelompok | `AssetDepreciationPolicy : Read` | - | `List<DepreciationPolicyResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menetapkan umur dan residu kelompok | `AssetDepreciationPolicy : Update` | `DepreciationPolicySetRequest` | `DepreciationPolicyResponse` | **Rencana (belum tersedia)** |

### 2.3 Corporate / Asset Management / Valuation / Subledger Account Setup

Base URL: `api/v1/corporate/asset-management/valuation/subledger-account-setups`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Akun kontrol per kelompok | `AssetSubledgerSetup : Read` | - | `List<SubledgerAccountSetupResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{assetGroupId}` | Menetapkan kode akun kontrol | `AssetSubledgerSetup : Update` | `SubledgerAccountSetupRequest` | `SubledgerAccountSetupResponse` | **Rencana (belum tersedia)** |

### 2.4 Corporate / Asset Management / Valuation / Unit Cost Center

Base URL: `api/v1/corporate/asset-management/valuation/unit-cost-centers`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Unit beserta pusat biaya yang dipakai (otomatis atau pemetaan) dan unit yang belum jelas | `AssetSubledgerSetup : Read` | - | `List<UnitCostCenterResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{organizationUnitId}` | Memetakan unit ke pusat biaya | `AssetSubledgerSetup : Update` | `UnitCostCenterRequest` | `UnitCostCenterResponse` | **Rencana (belum tersedia)** |
| `DELETE` | `/{organizationUnitId}` | Menghapus pemetaan (kembali otomatis) | `AssetSubledgerSetup : Update` | - | `bool` | **Rencana (belum tersedia)** |

### 2.5 Corporate / Asset Management / Valuation / Asset Book

Base URL: `api/v1/corporate/asset-management/valuation/books`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Nilai buku (menu 9.2) per tanggal | `AssetBook : Read` | `ValuationPagedQuery` (`asOfPeriod`, `assetGroupId`, `organizationUnitId`, `isCapitalized`) | `PagedResult<AssetBookListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/acquisition-costs` | Harga perolehan (menu 9.1) | `AssetBook : Read` | `ValuationPagedQuery` | `PagedResult<AssetBookListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{assetId}` | Rincian buku satu aset | `AssetBook : Read` | - | `AssetBookDetailResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{assetId}/entries` | Riwayat perubahan nilai satu aset (menu 9.3) | `AssetBook : Read` | `page`, `pageSize` | `PagedResult<AssetValueEntryResponse>` | **Rencana (belum tersedia)** |

### 2.6 Corporate / Asset Management / Valuation / Depreciation Run

Base URL: `api/v1/corporate/asset-management/valuation/depreciation-runs`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar proses penyusutan (menu 9.4) | `AssetDepreciationRun : Read` | `year` | `PagedResult<DepreciationRunResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/preview` | Pratinjau periode berikut yang boleh diproses | `AssetDepreciationRun : Read` | `period` (`YYYY-MM`) | `DepreciationPreviewResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Memproses satu periode | `AssetDepreciationRun : Process` | `DepreciationRunCreateRequest` | `DepreciationRunResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian proses | `AssetDepreciationRun : Read` | - | `DepreciationRunResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/lines` | Baris per aset | `AssetDepreciationRun : Read` | `assetGroupId`, `costCenterId`, `page`, `pageSize` | `PagedResult<DepreciationRunLineResponse>` | **Rencana (belum tersedia)** |

Arti khusus `409` pada `POST /`: periode sudah diproses, periode sebelumnya belum diproses, atau pratinjau
masih punya galat (misalnya unit tanpa pusat biaya).

### 2.7 Corporate / Asset Management / Valuation / Value Change

Base URL: `api/v1/corporate/asset-management/valuation/value-changes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pengajuan perubahan nilai | `AssetValueChange : Read` | `status`, `changeType`, `assetId`, `page`, `pageSize` | `PagedResult<ValueChangeResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian | `AssetValueChange : Read` | - | `ValueChangeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengajukan beserta bukti | `AssetValueChange : Create` | `ValueChangeCreateRequest` (multipart) | `ValueChangeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decide` | Kepala Akuntansi setuju/tolak | `AssetValueChange : Approve` | `ValueChangeDecisionRequest` | `ValueChangeResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Pengaju membatalkan | `AssetValueChange : Create` | `ValueChangeDecisionRequest` | `ValueChangeResponse` | **Rencana (belum tersedia)** |

### 2.8 Corporate / Asset Management / Valuation / Accounting Outbox

Base URL: `api/v1/corporate/asset-management/valuation/accounting-outbox`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Status pengiriman kejadian ke Akuntansi | `AssetAccountingOutbox : Read` | `status`, `eventTypeCode`, `page`, `pageSize` | `PagedResult<AccountingOutboxItemResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/retry` | Mencoba ulang baris `Failed` | `AssetAccountingOutbox : Retry` | - | `AccountingOutboxItemResponse` | **Rencana (belum tersedia)** |

### 2.9 Corporate / Asset Management / Valuation / Reconciliation

Base URL: `api/v1/corporate/asset-management/valuation/reconciliation`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Saldo subledger aset per akun kontrol dan status kirimnya (menu 9.5) | `AssetReconciliation : Read` | `period` | `List<SubledgerBalanceResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/resend` | Mengirim ulang saldo satu periode (versi baru) | `AssetReconciliation : Send` | `period` | `List<SubledgerBalanceResponse>` | **Rencana (belum tersedia)** |

Perbandingan dengan buku besar ada di layar rekonsiliasi Akuntansi (`TS-09`); layar Aset menautkannya.

### 2.10 Corporate / Asset Management / Valuation / Report

Base URL: `api/v1/corporate/asset-management/valuation/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/valuation` | Laporan nilai aset (11.6) per kelompok/unit per periode | `AssetBook : Read` | `ValuationReportQuery` | `PagedResult<AssetBookListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/valuation/export` | Ekspor Excel | `AssetBook : Read` | sama | `xlsx` | **Rencana (belum tersedia)** |
