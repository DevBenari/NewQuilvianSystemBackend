# Inventarisasi Fisik — Kontrak API

| Field | Value |
|---|---|
| `last_changed_in` | `AST-INV-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| Traceability | `CAP-01.5`, `07.1`–`07.6`, `11.4`; `AST-DEC-018`, `019`, `031` |

**Semua endpoint Rencana (belum tersedia).** Kode status umum: `../../register/contracts/api-contract.md` 1.

### 2.1 Corporate / Asset Management / Physical Inventory / Stock Take

Base URL: `api/v1/corporate/asset-management/physical-inventory/stock-takes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar jadwal/sesi (menu 7.1) | `AssetStockTake : Read` | `StockTakePagedQuery` (`status`, `organizationUnitId`, `year`) | `PagedResult<StockTakeSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian sesi beserta tim | `AssetStockTake : Read` | - | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menjadwalkan sesi | `AssetStockTake : Create` | `StockTakeScheduleRequest` | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah jadwal yang belum dimulai | `AssetStockTake : Create` | `StockTakeScheduleRequest` | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/start` | Memulai: snapshot dibekukan, unit ditahan | `AssetStockTake : Start` | - | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/expected-items` | Daftar aset yang diharapkan beserta tanda sudah dipindai | `AssetStockTake : Read` | `page`, `pageSize`, `scanned` | `PagedResult<StockTakeResultResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/scans` | Mencatat satu pindaian (menu 7.2) | `AssetStockTake : Scan` | `StockTakeScanRequest` (multipart) | `StockTakeScanResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/findings` | Mencatat temuan tidak terdaftar | `AssetStockTake : Scan` | `StockTakeUnregisteredFindingRequest` (multipart) | `StockTakeScanResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/scans` | Riwayat pindaian | `AssetStockTake : Read` | `page`, `pageSize` | `PagedResult<StockTakeScanResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/close-counting` | Menutup pemindaian dan menghitung hasil (menu 7.3) | `AssetStockTake : Close` | - | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reopen-counting` | Membuka kembali pemindaian | `AssetStockTake : Close` | `StockTakeDecisionRequest` | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/results` | Hasil pencocokan; saringan kategori (menu 7.3, 7.4) | `AssetStockTake : Read` | `category`, `page`, `pageSize` | `PagedResult<StockTakeResultResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/results/{resultId}` | Catatan tim pada satu hasil | `AssetStockTake : Close` | `StockTakeResultNoteRequest` | `StockTakeResultResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Ajukan verifikasi | `AssetStockTake : Submit` | - | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/verify` | Kepala Bagian Aset memverifikasi (menyetujui berita acara) atau mengembalikan (menu 7.5) | `AssetStockTake : Verify` | `StockTakeDecisionRequest` | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan sesi yang belum diverifikasi | `AssetStockTake : Cancel` | `StockTakeDecisionRequest` | `StockTakeSessionResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | PDF Berita Acara Inventarisasi (menu 7.6) | `AssetStockTake : Read` | - | `application/pdf` | **Rencana (belum tersedia)** |

Arti khusus: `409` pada `/start` berarti unit masih punya penyerahan, mutasi, atau pinjaman yang belum
selesai; `403` pada `/scans` berarti Anda bukan anggota tim sesi ini; `403` pada `/verify` berarti Anda
anggota tim.

### 2.2 Corporate / Asset Management / Physical Inventory / Stock Take Follow Up

Base URL: `api/v1/corporate/asset-management/physical-inventory/follow-ups`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar usulan tindak lanjut | `AssetStockTakeFollowUp : Read` | `status`, `type`, `sessionId`, `page`, `pageSize` | `PagedResult<StockTakeFollowUpResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/process` | Membuat dokumen tujuan (mutasi, penyerahan, laporan kondisi, draft aset) | `AssetStockTakeFollowUp : Process` | `StockTakeFollowUpProcessRequest` | `StockTakeFollowUpResponse` (berisi nomor dokumen tujuan) | **Rencana (belum tersedia)** |
| `POST` | `/{id}/dismiss` | Mengabaikan usulan beserta alasan | `AssetStockTakeFollowUp : Dismiss` | `StockTakeDecisionRequest` | `StockTakeFollowUpResponse` | **Rencana (belum tersedia)** |

### 2.3 Corporate / Asset Management / Physical Inventory / Dashboard

Base URL: `api/v1/corporate/asset-management/physical-inventory/dashboard`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/summary` | Kartu 1.5: sesi lewat tanggal belum diverifikasi + usulan belum ditindaklanjuti | `AssetStockTake : Read` | `organizationUnitId` opsional | `StockTakeSummaryResponse` | **Rencana (belum tersedia)** |

### 2.4 Corporate / Asset Management / Physical Inventory / Report

Base URL: `api/v1/corporate/asset-management/physical-inventory/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/stock-takes` | Laporan inventaris (11.4): per sesi, jumlah per kategori | `AssetReport : Read` | `StockTakePagedQuery` | `PagedResult<StockTakeSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/stock-takes/export` | Ekspor Excel | `AssetReport : Read` | sama | `xlsx` | **Rencana (belum tersedia)** |
