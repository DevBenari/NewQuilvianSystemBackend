# Register Aset — Kontrak API

| Field | Value |
|---|---|
| `last_changed_in` | `AST-REG-1.0.0` — `draft` |
| Owner | Rizki · `approved_by`: — · `approved_at`: — |
| `input_revision` | decision revision `4`; `02-backend-architecture.md` (sub-modul ini) |
| Dampak kompatibilitas | Seluruhnya endpoint baru; tidak ada endpoint lama yang berubah |
| Traceability | `CAP-01.1–01.3`, `01.6`, `02.*`, `03.*`, `04.*`, `06.*`, `08.*`, `11.1`, `11.3`, `11.7`, `CAP-EXT-01`, `CAP-EXT-05` |

**Semua endpoint di bawah berstatus Rencana (belum tersedia).**

Seluruh respons dibungkus `ApiResponse<T>`; daftar berhalaman memakai `PagedResult<T>` dengan query
`page` (bawaan 1), `pageSize` (bawaan 20, maks 100), `search`, `sortBy`, `sortDir`.

## 1. Kode status yang berlaku untuk seluruh grup

| Kode | Arti bagi pengguna |
|---|---|
| `200` | Berhasil |
| `201` | Data baru tersimpan |
| `400` | Isian tidak lengkap atau salah format; layar menampilkan pesan per isian (kode `AST-VAL-*`) |
| `403` | Anda tidak punya hak untuk tindakan ini, atau aset ini bukan milik unit Anda |
| `404` | Data tidak ditemukan atau sudah dihapus |
| `409` | Data sudah berubah oleh orang lain, statusnya tidak mengizinkan tindakan ini, atau aset sedang dalam proses lain |
| `413` | Berkas terlalu besar |
| `503` | Pengaturan lampiran belum diisi administrator (*fail-closed*) |

---

## 2. Grup endpoint

### 2.1 Corporate / Asset Management / Master Data / Asset Category

Base URL: `api/v1/corporate/asset-management/master-data/categories`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kategori | `AssetCategory : Read` | `AssetMasterPagedQuery` | `PagedResult<AssetCategoryResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan kategori aktif untuk dropdown | `AssetCategory : Read` | - | `List<AssetMasterOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian kategori beserta dokumen wajibnya | `AssetCategory : Read` | - | `AssetCategoryResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah kategori | `AssetCategory : Create` | `AssetCategoryUpsertRequest` | `AssetCategoryResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah kategori | `AssetCategory : Update` | `AssetCategoryUpsertRequest` | `AssetCategoryResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan/menonaktifkan | `AssetCategory : Update` | `AssetMasterStatusRequest` | `AssetCategoryResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/document-requirements` | Menetapkan jenis dokumen wajib kategori | `AssetCategory : Update` | `List<Guid>` jenis dokumen | `AssetCategoryResponse` | **Rencana (belum tersedia)** |

### 2.2 Corporate / Asset Management / Master Data / Asset Classification

Base URL: `api/v1/corporate/asset-management/master-data/classifications`

`GET /`, `GET /options`, `GET /{id}`, `POST /`, `PUT /{id}`, `PATCH /{id}/status` dengan bentuk dan
kode status sama seperti 2.1; hak akses `AssetClassification : Read/Create/Update`; DTO
`AssetClassificationUpsertRequest` / `AssetClassificationResponse`. **Rencana (belum tersedia).**

### 2.3 Corporate / Asset Management / Master Data / Asset Group

Base URL: `api/v1/corporate/asset-management/master-data/groups`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar kelompok | `AssetGroup : Read` | `AssetMasterPagedQuery` + `categoryId` | `PagedResult<AssetGroupResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/options` | Pilihan kelompok aktif | `AssetGroup : Read` | `categoryId` | `List<AssetMasterOptionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/inventory-codes` | Layar Kode Inventaris: kode, kelompok, nomor terakhir yang terbit | `AssetGroup : Read` | `AssetMasterPagedQuery` | `PagedResult<AssetInventoryCodeResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian kelompok | `AssetGroup : Read` | - | `AssetGroupResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah kelompok | `AssetGroup : Create` | `AssetGroupUpsertRequest` | `AssetGroupResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah kelompok (kode terkunci bila sudah ada aset bernomor) | `AssetGroup : Update` | `AssetGroupUpsertRequest` | `AssetGroupResponse` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan/menonaktifkan | `AssetGroup : Update` | `AssetMasterStatusRequest` | `AssetGroupResponse` | **Rencana (belum tersedia)** |

### 2.4 Corporate / Asset Management / Master Data / Asset Ownership Type

Base URL: `api/v1/corporate/asset-management/master-data/ownership-types`

Bentuk sama dengan 2.2; hak akses `AssetOwnershipType : Read/Create/Update`. **Rencana (belum tersedia).**

### 2.5 Corporate / Asset Management / Master Data / Asset Document Type

Base URL: `api/v1/corporate/asset-management/master-data/document-types`

Bentuk sama dengan 2.2; hak akses `AssetDocumentType : Read/Create/Update`. **Rencana (belum tersedia).**

### 2.6 Corporate / Asset Management / Master Data / Asset Location

Base URL: `api/v1/corporate/asset-management/master-data/locations`

Bentuk sama dengan 2.2 ditambah saringan `organizationUnitId`; hak akses
`AssetLocation : Read/Create/Update`. **Rencana (belum tersedia).**

### 2.7 Corporate / Asset Management / Master Data / Asset Setting

Base URL: `api/v1/corporate/asset-management/master-data/settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Membaca pengaturan aset | `AssetSetting : Read` | - | `AssetSettingResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/` | Mengubah pengaturan | `AssetSetting : Update` | `AssetSettingUpdateRequest` | `AssetSettingResponse` | **Rencana (belum tersedia)** |

### 2.8 Corporate / Asset Management / Register / Asset

Base URL: `api/v1/corporate/asset-management/register/assets`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar aset; tanpa `ReadAllUnits` hanya unit pengguna | `AssetRegister : Read` | `AssetPagedQuery` (`organizationUnitId`, `locationId`, `categoryId`, `groupId`, `recordStatus`, `condition`, `placementState`, `isIdle`) | `PagedResult<AssetListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian aset tanpa rupiah | `AssetRegister : Read` | - | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/amounts` | Harga perolehan saat pencatatan dan ringkasan nilai buku | `AssetRegister : ViewAmount` | - | `AssetAmountResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/history` | Linimasa aset | `AssetRegister : Read` | `page`, `pageSize` | `PagedResult<AssetHistoryItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/by-number/{inventoryNumber}` | Mencari aset dari hasil pindai QR | `AssetRegister : Read` | - | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/drafts` | Membuat 1–200 draft sekaligus | `AssetRegister : Create` | `AssetDraftCreateRequest` | `List<AssetListItemResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draft | `AssetRegister : Update` | `AssetDraftUpdateRequest` | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan draft untuk disetujui | `AssetRegister : Submit` | - | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Menyetujui: nomor terbit, aset Terdaftar | `AssetRegister : Approve` | - | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/approve-batch` | Menyetujui banyak aset; hasil per aset | `AssetRegister : Approve` | `AssetBatchApproveRequest` | `List<AssetDecisionResultResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Mengembalikan ke Draft beserta alasan | `AssetRegister : Approve` | `AssetDecisionRequest` | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan draft | `AssetRegister : Update` | `AssetDecisionRequest` | `AssetDetailResponse` | **Rencana (belum tersedia)** |
| `POST` | `/labels` | PDF label QR untuk aset terpilih | `AssetRegister : Read` | `AssetLabelRequest` | `application/pdf` | **Rencana (belum tersedia)** |
| `GET` | `/idle` | Pemantauan aset menganggur | `AssetRegister : Read` | `AssetPagedQuery` | `PagedResult<AssetIdleItemResponse>` | **Rencana (belum tersedia)** |

Arti khusus: `409` pada `/approve` berarti aset sudah disetujui atau ditolak orang lain, atau nomor
gagal terbit; `403` pada `/approve` dapat berarti Anda adalah pengaju aset ini (`AST-VAL-REG-012`).

### 2.9 Corporate / Asset Management / Register / Asset Handover

Base URL: `api/v1/corporate/asset-management/register/handovers`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar penyerahan (menu 4.3) | `AssetPlacement : Read` | `AssetPlacementPagedQuery` (`status`, `purpose`, `targetOrganizationUnitId`) | `PagedResult<AssetHandoverResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian penyerahan | `AssetPlacement : Read` | - | `AssetHandoverResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menyerahkan aset ke unit/ruangan/penanggung jawab | `AssetPlacement : Create` | `AssetHandoverCreateRequest` | `AssetHandoverResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/confirm` | Penerima mengonfirmasi; penempatan berlaku | `AssetPlacement : Confirm` | - | `AssetHandoverResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Penerima menolak beserta alasan | `AssetPlacement : Confirm` | `AssetHandoverDecisionRequest` | `AssetHandoverResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Bagian Aset membatalkan sebelum dikonfirmasi | `AssetPlacement : Cancel` | `AssetHandoverDecisionRequest` | `AssetHandoverResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/document` | PDF Berita Acara Penyerahan | `AssetPlacement : Read` | - | `application/pdf` | **Rencana (belum tersedia)** |

### 2.10 Corporate / Asset Management / Register / Asset Placement

Base URL: `api/v1/corporate/asset-management/register/placements`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Penempatan berjalan per unit/ruangan/penanggung jawab (menu 4.1, 4.2) | `AssetPlacement : Read` | `AssetPlacementPagedQuery` (`organizationUnitId`, `locationId`, `custodianEmployeeId`) | `PagedResult<AssetPlacementListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/history` | Riwayat lokasi dan penggunaan (menu 4.4) | `AssetPlacement : Read` | `assetId`, rentang tanggal | `PagedResult<AssetPlacementListItemResponse>` | **Rencana (belum tersedia)** |

### 2.11 Corporate / Asset Management / Register / Asset Condition Report

Base URL: `api/v1/corporate/asset-management/register/condition-reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar laporan kondisi | `AssetCondition : Read` | `AssetConditionPagedQuery` | `PagedResult<AssetConditionReportResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian laporan | `AssetCondition : Read` | - | `AssetConditionReportResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Melaporkan kondisi beserta foto | `AssetCondition : Report` | `AssetConditionReportCreateRequest` (multipart) | `AssetConditionReportResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/decide` | Menetapkan kondisi selain Hilang, atau menolak laporan | `AssetCondition : Decide` | `AssetConditionDecideRequest` | `AssetConditionReportResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-loss` | Menyetujui status Hilang | `AssetCondition : ApproveLoss` | `AssetConditionDecideRequest` | `AssetConditionReportResponse` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/loss-document` | PDF Berita Acara Kehilangan | `AssetCondition : Read` | - | `application/pdf` | **Rencana (belum tersedia)** |

### 2.12 Corporate / Asset Management / Register / Asset Document

Base URL: `api/v1/corporate/asset-management/register/documents`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar dokumen kepemilikan (menu 3.5) | `AssetDocument : Read` | `AssetDocumentPagedQuery` (`assetId`, `documentTypeId`) | `PagedResult<AssetDocumentResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/completeness` | Aset yang dokumen wajibnya belum lengkap (menu 8.2) | `AssetDocument : Read` | `AssetDocumentPagedQuery` | `PagedResult<AssetDocumentCompletenessResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/expiring` | Dokumen habis atau hampir habis (menu 8.3) | `AssetDocument : Read` | `AssetDocumentPagedQuery` | `PagedResult<AssetDocumentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah dokumen beserta berkas | `AssetDocument : Create` | `AssetDocumentUpsertRequest` (multipart) | `AssetDocumentResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah dokumen | `AssetDocument : Update` | `AssetDocumentUpsertRequest` | `AssetDocumentResponse` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menandai dokumen terhapus (`IsDelete`) | `AssetDocument : Delete` | - | `bool` | **Rencana (belum tersedia)** |

### 2.13 Corporate / Asset Management / Register / Asset Attachment

Base URL: `api/v1/corporate/asset-management/register/attachments`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Mengunduh berkas; service memeriksa hak atas dokumen pemiliknya | `AssetRegister : Read` | - | berkas | **Rencana (belum tersedia)** |

### 2.14 Corporate / Asset Management / Register / Asset Utilization

Base URL: `api/v1/corporate/asset-management/register/utilizations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pemanfaatan (menu 8.1, 8.5) | `AssetUtilization : Read` | `AssetUtilizationPagedQuery` | `PagedResult<AssetUtilizationResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian | `AssetUtilization : Read` | - | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draft permohonan | `AssetUtilization : Create` | `AssetUtilizationUpsertRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draft | `AssetUtilization : Create` | `AssetUtilizationUpsertRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan; sistem menentukan penyetuju | `AssetUtilization : Submit` | - | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Persetujuan Kepala Bagian Aset (atau penerusan ke Direktur) | `AssetUtilization : Approve` | `AssetUtilizationDecisionRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-director` | Persetujuan Direktur | `AssetUtilization : ApproveDirector` | `AssetUtilizationDecisionRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak (penyetuju sesuai tahap) | `AssetUtilization : Approve` | `AssetUtilizationDecisionRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/end` | Mengakhiri lebih awal | `AssetUtilization : End` | `AssetUtilizationDecisionRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan sebelum disetujui | `AssetUtilization : Create` | `AssetUtilizationDecisionRequest` | `AssetUtilizationResponse` | **Rencana (belum tersedia)** |

Penolakan pada tahap Direktur memakai `/approve-director` dengan keputusan "tolak", agar hak akses
tahap itu tetap `ApproveDirector`.

### 2.15 Corporate / Asset Management / Register / Asset Import

Base URL: `api/v1/corporate/asset-management/register/imports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/template` | Mengunduh templat Excel | `AssetImport : Read` | - | `xlsx` | **Rencana (belum tersedia)** |
| `GET` | `/` | Daftar batch | `AssetImport : Read` | `page`, `pageSize`, `status` | `PagedResult<AssetImportBatchResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian batch beserta galat per baris | `AssetImport : Read` | - | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mengunggah berkas dan langsung memvalidasi | `AssetImport : Upload` | `AssetImportUploadRequest` (multipart) | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-asset` | Persetujuan Kepala Bagian Aset | `AssetImport : ApproveAsset` | `AssetImportDecisionRequest` | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve-value` | Persetujuan Kepala Akuntansi; batch diterapkan | `AssetImport : ApproveValue` | `AssetImportDecisionRequest` | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak batch (tahap mana pun) | `AssetImport : ApproveAsset` | `AssetImportDecisionRequest` | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Pengunggah membatalkan sebelum diterapkan | `AssetImport : Upload` | - | `AssetImportBatchResponse` | **Rencana (belum tersedia)** |

Penolakan pada tahap Akuntansi memakai `/approve-value` dengan keputusan "tolak".

### 2.16 Corporate / Asset Management / Register / Dashboard

Base URL: `api/v1/corporate/asset-management/register/dashboard`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/summary` | Kartu ringkasan, kondisi, aset rusak (1.1, 1.3, 1.6) | `AssetDashboard : Read` | `organizationUnitId` opsional | `AssetDashboardSummaryResponse` | **Rencana (belum tersedia)** |
| `GET` | `/by-unit` | Jumlah aset per unit (1.2) | `AssetDashboard : Read` | - | `List<AssetUnitSummaryResponse>` | **Rencana (belum tersedia)** |

Kartu 1.4 dan 1.5 diambil dari `../../pergerakan/contracts/api-contract.md` dan
`../../inventarisasi/contracts/api-contract.md`.

### 2.17 Corporate / Asset Management / Register / Report

Base URL: `api/v1/corporate/asset-management/register/reports`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/inventory-by-unit` | Daftar inventaris per unit (11.1) | `AssetReport : Read` | `AssetReportQuery` | `PagedResult<AssetListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/condition` | Laporan kondisi (11.3) | `AssetReport : Read` | `AssetReportQuery` | `PagedResult<AssetListItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/history` | History & Audit lintas sub-modul (11.7) | `AssetReport : Read` | `AssetReportQuery` + `eventType`, `actorUserId` | `PagedResult<AssetHistoryItemResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{reportCode}/export` | Ekspor Excel laporan di atas | `AssetReport : Read` | `AssetReportQuery` | `xlsx` | **Rencana (belum tersedia)** |

Laporan mengikuti pembatasan unit yang sama dengan daftar aset (`AST-DES-016`).
