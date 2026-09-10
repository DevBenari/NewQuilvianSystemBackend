# API Contract — Modul Radiologi

| Field | Value |
|---|---|
| Contract version | `RAD-API-001` |
| Revision | `2` |
| Status | `draft` |
| Backend SHA | `64da911` |
| Input | `RAD-ARCH-BE-001`, `RAD-DA-001-r1` |
| Owner | Yoga Aji Pratama |
| `approved_by` / `approved_at` | Belum |

Endpoint yang **belum ada di kode** diberi label `Rencana (belum tersedia)`. Yang tidak berlabel
sudah dapat dipakai sekarang.

---

## 1. As-Is — Endpoint yang Sudah Tersedia

### Health Services / Radiology Management / Rad Order

Base URL: `api/v1/health-services/radiology-management/rad-orders`
Contract version: `v1` — status **berjalan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar pesanan dengan penyaringan dan halaman | `RadOrder : Read` | Query | `ApiResponse<PagedResult<RadOrderListResponse>>` | Tersedia |
| `GET` | `/{id}` | Melihat rincian satu pesanan | `RadOrder : Read` | — | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `GET` | `/episodes/{episodeId}` | Melihat pesanan milik satu kunjungan | `RadOrder : Read` | — | `ApiResponse<List<RadOrderListResponse>>` | Tersedia |
| `POST` | `/` | Dokter membuat pesanan baru | `RadOrder : Create` | `CreateRadOrderRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/accept` | Radiologi menerima pesanan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/schedule` | Menjadwalkan pemeriksaan | `RadOrder : Schedule` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/start` | Menandai mulai dikerjakan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/complete` | Menandai selesai dikerjakan | `RadOrder : Process` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/hold` | Menahan pesanan sementara | `RadOrder : Hold` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/resume` | Melanjutkan pesanan yang tertahan | `RadOrder : Hold` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/reject` | Radiologi menolak pesanan | `RadOrder : Update` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |
| `PUT` | `/{id}/cancel` | Membatalkan pesanan | `RadOrder : Cancel` | `RadOrderTransitionRequest` | `ApiResponse<RadOrderDetailResponse>` | Tersedia |

### Health Services / Radiology Management / Rad Study

Base URL: `api/v1/health-services/radiology-management/rad-studies`
Contract version: `v1` — status **berjalan**

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/modalities` | Melihat daftar alat pencitraan | `RadStudy : Read` | — | `ApiResponse<List<RadModalityResponse>>` | Tersedia — **akan digantikan**, lihat bagian 3 |
| `GET` | `/safety-requirements` | Melihat daftar butir keselamatan | `RadStudy : Read` | — | `ApiResponse<List<RadSafetyRequirementResponse>>` | Tersedia — **akan digantikan** |
| `GET` | `/by-order/{radOrderId}` | Melihat study milik satu pesanan | `RadStudy : Read` | — | `ApiResponse<List<RadStudyResponse>>` | Tersedia |
| `GET` | `/by-order/{radOrderId}/history` | Melihat riwayat perpindahan status | `RadStudy : Read` | — | `ApiResponse<List<RadTransitionHistoryResponse>>` | Tersedia |
| `POST` | `/by-order/{radOrderId}` | Membuat rencana pengambilan citra | `RadStudy : Create` | `CreateRadStudyRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/verify-patient` | Memastikan identitas pasien benar | `RadStudy : Verify` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/safety-checks` | Mengisi jawaban butir keselamatan | `RadStudy : Safety` | `RadSafetyCheckDecisionRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/clear-safety` | Menyatakan gerbang keselamatan lolos | `RadStudy : Safety` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/start-acquisition` | Mulai mengambil citra | `RadStudy : Acquire` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/complete-acquisition` | Selesai mengambil citra | `RadStudy : Acquire` | — | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/abort-acquisition` | Menghentikan di tengah jalan | `RadStudy : Acquire` | `RadAbortAcquisitionRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/decide-quality` | Menyatakan citra layak atau tidak | `RadStudy : Quality` | `RadAcquisitionQualityRequest` | `ApiResponse<RadStudyActionResult>` | Tersedia |
| `POST` | `/{id}/repeat` | Membuat study pengulangan | `RadStudy : Repeat` | `RadRepeatStudyRequest` | `ApiResponse<RadStudyResponse>` | Tersedia |
| `POST` | `/{id}/consumptions` | Mencatat bahan terpakai | `RadStudy : Consumption` | `RadConsumptionRequest` | `ApiResponse<RadConsumptionResponse>` | Tersedia |

---

## 2. To-Be — Endpoint Baru

### Health Services / Radiology Management / Rad Order — tambahan

Base URL: `api/v1/health-services/radiology-management/rad-orders`
Contract version: `v2` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/worklist` | **Daftar kerja petugas pada satu alat**, pesanan cito di urutan atas | `RadOrder : Read` | Query: `modalityId` wajib, `date`, `status` | `ApiResponse<List<RadWorklistItemResponse>>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/urgency` | Mengubah penanda cito setelah pesanan dibuat | `RadOrder : Update` | `RadOrderUrgencyRequest` | `ApiResponse<RadOrderDetailResponse>` | **Rencana (belum tersedia)** |

**Perubahan pada endpoint yang sudah ada:**

| Endpoint | Perubahan | Kompatibilitas |
|---|---|---|
| `POST /` | `CreateRadOrderRequest` menerima field `IsUrgent` yang **boleh kosong**, bawaannya `false` | **Aman.** Pemanggil lama yang tidak mengirim field itu tetap berjalan |
| `GET /`, `GET /{id}` | Response memuat `IsUrgent` | **Aman.** Penambahan field pada response tidak merusak pemanggil lama |

> **Mengapa `GET /worklist` diletakkan pada controller pesanan, bukan controller study.**
> Daftar kerja dimulai dari pesanan yang sudah diterima tetapi belum tentu punya study. Kalau
> diletakkan di controller study, pekerjaan yang belum direncanakan sama sekali tidak akan
> muncul — padahal justru itu yang paling perlu dikerjakan.

### Health Services / Radiology Management / Rad Report

Base URL: `api/v1/health-services/radiology-management/rad-reports`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar bacaan dengan penyaringan dan halaman | `RadReport : Read` | Query | `ApiResponse<PagedResult<RadReportListResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Melihat bacaan beserta versi yang sedang berlaku | `RadReport : Read` | — | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/versions` | Melihat seluruh versi bacaan, terbaru lebih dulu | `RadReport : Read` | — | `ApiResponse<List<RadReportVersionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/by-study/{radStudyId}` | Melihat bacaan atas satu study | `RadReport : Read` | — | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/by-encounter/{encounterId}` | Melihat seluruh bacaan satu kunjungan — dipakai rekam medis | `RadReport : Read` | — | `ApiResponse<List<RadReportListResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/by-study/{radStudyId}/draft` | Menulis draf bacaan | `RadReport : Create` | `CreateRadReportDraftRequest` | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/draft` | Mengubah draf yang belum disahkan | `RadReport : Update` | `UpdateRadReportDraftRequest` | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/validate` | Mengesahkan bacaan | `RadReport : Validate` | `RadReportValidateRequest` | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/release` | Merilis bacaan ke dokter pengirim | `RadReport : Release` | — | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/amendments` | Menulis draf koreksi atas bacaan yang sudah dirilis | `RadReport : Amend` | `CreateRadReportAmendmentRequest` | `ApiResponse<RadReportDetailResponse>` | **Rencana (belum tersedia)** |

### Health Services / Radiology Management / Master Data / Rad Modality

Base URL: `api/v1/health-services/radiology-management/master-data/rad-modalities`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar alat pencitraan | `RadModality : Read` | Query | `ApiResponse<PagedResult<RadModalityResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Melihat rincian satu alat | `RadModality : Read` | — | `ApiResponse<RadModalityResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mendaftarkan alat baru | `RadModality : Create` | `CreateRadModalityRequest` | `ApiResponse<RadModalityResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah data alat | `RadModality : Update` | `UpdateRadModalityRequest` | `ApiResponse<RadModalityResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan alat | `RadModality : Delete` | — | `ApiResponse<bool>` | **Rencana (belum tersedia)** |

### Health Services / Radiology Management / Master Data / Rad Safety Requirement

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-requirements`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar butir keselamatan | `RadSafetyRequirement : Read` | Query | `ApiResponse<PagedResult<RadSafetyRequirementResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Melihat rincian satu butir | `RadSafetyRequirement : Read` | — | `ApiResponse<RadSafetyRequirementResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah butir keselamatan | `RadSafetyRequirement : Create` | `CreateRadSafetyRequirementRequest` | `ApiResponse<RadSafetyRequirementResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah butir keselamatan | `RadSafetyRequirement : Update` | `UpdateRadSafetyRequirementRequest` | `ApiResponse<RadSafetyRequirementResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan butir | `RadSafetyRequirement : Delete` | — | `ApiResponse<bool>` | **Rencana (belum tersedia)** |

### Health Services / Radiology Management / Master Data / Rad Safety Rule

Base URL: `api/v1/health-services/radiology-management/master-data/rad-safety-rules`
Contract version: `v1` — status `draft`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Melihat daftar aturan keselamatan | `RadSafetyRule : Read` | Query | `ApiResponse<PagedResult<RadSafetyRuleResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/coverage` | **Memeriksa alat mana yang belum punya aturan aktif** | `RadSafetyRule : Read` | — | `ApiResponse<List<RadModalityCoverageResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menyusun draf aturan | `RadSafetyRule : Create` | `CreateRadSafetyRuleRequest` | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Mengubah draf aturan | `RadSafetyRule : Update` | `UpdateRadSafetyRuleRequest` | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit` | Mengajukan aturan untuk disahkan | `RadSafetyRule : Submit` | — | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/approve` | Mengesahkan aturan; versi naik satu | `RadSafetyRule : Approve` | — | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Menolak pengajuan aturan | `RadSafetyRule : Reject` | `RadSafetyRuleRejectRequest` | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/deactivate` | Menonaktifkan aturan yang berlaku | `RadSafetyRule : Deactivate` | — | `ApiResponse<RadSafetyRuleResponse>` | **Rencana (belum tersedia)** |

> **Mengapa `GET /coverage` ada.** Gerbang keselamatan bersifat fail-closed. Tanpa layar yang
> memberi tahu alat mana yang belum punya aturan aktif, admin baru tahu ada yang kurang ketika
> pasien sudah berdiri di depan alat dan pemeriksaannya ditolak.

---

## 3. Endpoint yang Akan Digantikan

| Endpoint lama | Penggantinya | Rencana |
|---|---|---|
| `GET /rad-studies/modalities` | `GET /master-data/rad-modalities` | **Tetap dipertahankan** sampai seluruh konsumen berpindah |
| `GET /rad-studies/safety-requirements` | `GET /master-data/rad-safety-requirements` | **Tetap dipertahankan** sampai seluruh konsumen berpindah |

Penghapusan keduanya menjadi **task tersendiri**, bukan bagian dari pekerjaan ini. Menghapusnya
sekarang akan merusak konsumen yang belum tentu diketahui semuanya.

---

## 4. Arti Kode Status bagi Pengguna

| Kode | Arti bagi pengguna | Contoh kejadian di modul ini |
|---|---|---|
| `200` | Permintaan berhasil dan datanya dikembalikan | Membuka daftar pesanan |
| `201` | Data baru berhasil dibuat | Draf bacaan tersimpan |
| `400` | Isian yang dikirim tidak lengkap atau formatnya salah | Menolak aturan tanpa mengisi alasan |
| `403` | Pengguna tidak punya hak akses untuk tindakan ini | Residen mengesahkan drafnya sendiri |
| `404` | Data yang dicari tidak ditemukan | Membuka bacaan yang tidak ada |
| `409` | Tindakan bertabrakan dengan keadaan data saat ini | Mulai foto padahal gerbang keselamatan belum lolos; dua orang mengesahkan draf yang sama bersamaan |
| `422` | Aturan bisnis menolak, walau bentuk isiannya benar | Menulis bacaan atas study yang citranya dinyatakan tidak layak |

---

## 5. Traceability

| Endpoint | Decision asal | Slice |
|---|---|---|
| `POST /rad-reports/by-study/{id}/draft` | `RJ-BIL-GATE-DEC-004`, `RAD-DEC-003` | `S9` |
| `POST /rad-reports/{id}/validate` | `RAD-DEC-003` | `S9` |
| `POST /rad-reports/{id}/amendments` | `RJ-BIL-GATE-DEC-004` | `S10` |
| `GET /rad-reports/by-encounter/{id}` | `RAD-DEC-006` | `S14` |
| `POST /master-data/rad-safety-rules/{id}/approve` | `RAD-DEC-005` | `S4` |
| `GET /master-data/rad-safety-rules/coverage` | `RJ-BIL-DEC-014` | `S4` |
| CRUD `master-data/rad-modalities` | `RAD-DEC-001` butir 14 | `S13` |
| `GET /rad-orders/worklist` | `RAD-DEC-012` | `S12` |
| `PUT /rad-orders/{id}/urgency` | `RAD-DEC-013` | `S12` |
