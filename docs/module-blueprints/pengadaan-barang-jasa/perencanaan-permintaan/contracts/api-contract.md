# Perencanaan & Permintaan — Kontrak API

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PP-API` · `last_changed_in: 1.1` (`PBJ-DEC-037`, 2026-11-12) · status `approved` (`PBJ-DEC-034`) |
| Pemilik | Pengguna sesi · `approved_by: Pengguna sesi` · `approved_at: 2026-11-12` |
| Masukan | `02-backend-architecture.md` revisi 2; `PBJ-PP-STATE` 1.0; `PBJ-PP-VAL` 1.0 |
| Kompatibilitas | Seluruh endpoint baru. **Tidak ada endpoint existing yang diubah.** Perubahan `Wfl` bersifat internal (tanpa endpoint baru) |

**Semua endpoint di dokumen ini berstatus `Rencana (belum tersedia)`.**

Konvensi bersama:
- Respons dibungkus `ApiResponse<T>`; daftar memakai `PagedResult<T>`.
- Aksi pengubah status menerima header `Idempotency-Key` (UUID). Request ulang dengan kunci sama mengembalikan hasil pertama.
- Ubah data mewajibkan `expectedRowVersion` di body.
- Kode umum: 400 isian tidak terbaca; 401 belum masuk; 403 tidak punya izin; 404 tidak ada atau di luar cakupan unit; 409 bentrok status atau data basi; 422 aturan bisnis (`PBJ-VAL-*`).

Kolom **Hak akses** di sini adalah satu-satunya pemetaan endpoint ke izin di blueprint ini.

---

### Corporate / Procurement Management / Request / Purchase Request

Base URL: `api/v1/corporate/procurement-management/purchase-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar PR dalam cakupan unit pengguna. Saring status, unit, tahun, urgensi, tanggal, kata kunci nomor | `ProcurementRequest : Read` | `PurchaseRequestQuery` | `PagedResult<PurchaseRequestListItem>` |
| `GET` | `/summary` | Jumlah PR per status (untuk 1.2 dan penanda menu) | `ProcurementRequest : Read` | `PurchaseRequestQuery` | `PurchaseRequestSummaryResponse` |
| `GET` | `/{id}` | Detail PR: kepala, baris, cek stok, pembagian rencana, tanda lewat pagu, nomor instance approval | `ProcurementRequest : Read` | — | `PurchaseRequestResponse` |
| `POST` | `/` | Buat PR draft | `ProcurementRequest : Create` | `CreatePurchaseRequestRequest` | `PurchaseRequestResponse` (201) |
| `PUT` | `/{id}` | Ubah PR `DRAFT`/`RETURNED` | `ProcurementRequest : Update` | `UpdatePurchaseRequestRequest` | `PurchaseRequestResponse` |
| `POST` | `/{id}/submit` | Ajukan; alokasi nomor; bagi dalam/luar rencana | `ProcurementRequest : Submit` | header `Idempotency-Key` | `PurchaseRequestResponse` |
| `POST` | `/{id}/cancel` | Batal oleh pengusul (`DRAFT`, `SUBMITTED`, `RETURNED`, `PENDING_APPROVAL`) | `ProcurementRequest : Cancel` | `CancelPurchaseRequestRequest` | `PurchaseRequestResponse` |
| `GET` | `/{id}/history` | Riwayat aksi dan status (3.5) | `ProcurementRequest : Read` | — | `List<DocumentHistoryResponse>` |
| `GET` | `/plan-line-options` | Baris rencana berlaku beserta sisa, untuk barang × unit × tahun | `ProcurementRequest : Create` | `departmentId`, `fiscalYear`, `itemId` | `List<PlanLineQuotaResponse>` |

> **Delta implementasi `BE-PBJ-006` (12 November 2026) — menunggu pengesahan ke versi kontrak berikutnya.**
> Tidak ada endpoint yang dihapus; perilaku bisnis tidak berubah.
> - Riwayat tersedia di `GET /{id}/status-history`, bukan `GET /{id}/history`, seragam dengan sub-resource
>   riwayat dokumen lain di repository.
> - Tambahan `GET /filters/metadata` (`ProcurementRequest : Read`) → `PurchaseRequestFilterMetadataResponse`:
>   pilihan status, urgensi, urutan, ukuran halaman.
> - `PurchaseRequestQuery` mendapat `mineOnly` ("PR saya"); `GET /summary` tidak menerima saringan.
> - `POST /` menjawab 200 (pola `ApiResponse` repository), bukan 201.
> - Sudah ada di source: `GET /`, `/summary`, `/filters/metadata`, `/{id}`, `/{id}/status-history`, `POST /`,
>   `PUT /{id}`. Aksi `submit`/`cancel` dan `plan-line-options` menyusul `BE-PBJ-007`.

**Contoh `POST /`:**
```json
{
  "departmentId": "<id Departemen IGD>",
  "fiscalYear": 2027,
  "fundSourceId": "<id Operasional>",
  "neededByDate": "2027-02-15",
  "urgencyLevel": "NORMAL",
  "justification": "Penggantian kursi roda yang rusak di ruang tunggu",
  "lines": [
    { "itemId": "<id KTL-000045 Kursi Roda Standar>", "requestedQuantity": 20,
      "estimatedUnitPrice": 3000000, "needPlanLineKey": "<kunci baris rencana>", "specification": "Rangka baja, roda 24 inci" }
  ]
}
```

**Arti kode bagi pengguna:**
- 404 pada `GET /{id}`: PR tidak ada **atau** milik unit di luar cakupan Anda.
- 409 pada `PUT`: data sudah diubah orang lain (`PBJ-VAL-090`) atau status tidak bisa diubah (`PBJ-VAL-093`).
- 422 pada `submit`: isian belum lengkap, dengan kode `PBJ-VAL-001`..`012`.

### Corporate / Procurement Management / Request / Verification

Base URL: `api/v1/corporate/procurement-management/purchase-request-verifications`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Antrean verifikasi: PR `SUBMITTED` (3.2, 11.2). Saring urgensi untuk 3.4 | `ProcurementRequestVerification : Read` | `PurchaseRequestQuery` | `PagedResult<PurchaseRequestListItem>` |
| `GET` | `/stock-checks` | Daftar hasil cek stok per baris (3.3) | `ProcurementRequestVerification : Read` | `StockCheckQuery` | `PagedResult<StockCheckListItem>` |
| `PUT` | `/{id}/stock-checks` | Catat atau ubah cek stok per baris saat `SUBMITTED` | `ProcurementRequestVerification : Verify` | `RecordStockCheckRequest` | `PurchaseRequestResponse` |
| `POST` | `/{id}/complete` | Selesaikan verifikasi → `FULFILLED_FROM_STOCK` atau `PENDING_APPROVAL` (instance `Wfl` dibuat dan diajukan) | `ProcurementRequestVerification : Verify` | `CompleteVerificationRequest` + `Idempotency-Key` | `PurchaseRequestResponse` |
| `POST` | `/{id}/return` | Kembalikan ke pengusul | `ProcurementRequestVerification : Verify` | `ReturnPurchaseRequestRequest` | `PurchaseRequestResponse` |
| `POST` | `/{id}/reject` | Tolak **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `ProcurementRequestVerification : Verify` | `RejectPurchaseRequestRequest` | `PurchaseRequestResponse` |
| `POST` | `/{id}/cancel-approved` | Batalkan PR `APPROVED` yang belum masuk paket **[DIKONFIRMASI `PBJ-DEC-033`, `G-04`]** | `ProcurementRequestVerification : CancelApproved` | `CancelPurchaseRequestRequest` | `PurchaseRequestResponse` |

**Contoh `PUT /{id}/stock-checks`:**
```json
{ "expectedRowVersion": "<versi>",
  "lines": [ { "lineId": "<baris 1>", "stockAvailableQuantity": 5, "stockFulfilledQuantity": 5,
               "stockSourceNote": "Gudang Umum lantai 1" } ] }
```

Hasil: kebutuhan beli baris 1 = 15 dan `purchaseTotal` = Rp 45.000.000.

**Persetujuan PR tidak punya endpoint di Pengadaan.** Penyetuju bertindak lewat kotak masuk `Wfl`
(`api/v1/corporate/human-resource/approval-inbox`, sudah tersedia; izin `ApprovalInbox : Approve/Reject/Return`).
Hasilnya disinkronkan ke PR oleh `ProcurementWorkflowLifecycleHandler`.

### Corporate / Procurement Management / Planning / Need Plan

Base URL: `api/v1/corporate/procurement-management/need-plans`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar rencana (versi berlaku bawaan; `includeHistory` untuk semua versi) | `ProcurementNeedPlan : Read` | `NeedPlanQuery` | `PagedResult<NeedPlanResponse>` |
| `GET` | `/{id}` | Detail satu versi beserta pemakaian per baris (dipakai PR, dialokasikan paket) | `ProcurementNeedPlan : Read` | — | `NeedPlanResponse` |
| `POST` | `/` | Buat rencana versi 1 | `ProcurementNeedPlan : Create` | `CreateNeedPlanRequest` | `NeedPlanResponse` (201) |
| `PUT` | `/{id}` | Ubah `DRAFT`/`RETURNED` | `ProcurementNeedPlan : Update` | `UpdateNeedPlanRequest` | `NeedPlanResponse` |
| `POST` | `/{id}/submit` | Ajukan ke approval | `ProcurementNeedPlan : Submit` | `Idempotency-Key` | `NeedPlanResponse` |
| `POST` | `/{id}/cancel` | Batalkan draft | `ProcurementNeedPlan : Update` | `{ reason }` | `NeedPlanResponse` |
| `POST` | `/{id}/revisions` | Buat versi baru dari versi `APPROVED` berlaku (2.5) | `ProcurementNeedPlan : Revise` | `{ reason }` | `NeedPlanResponse` (201) |
| `GET` | `/{id}/versions` | Daftar seluruh versi rencana unit-tahun yang sama | `ProcurementNeedPlan : Read` | — | `List<NeedPlanResponse>` |
| `GET` | `/{id}/compare/{otherId}` | Perbandingan sebelum–sesudah per baris | `ProcurementNeedPlan : Read` | — | `NeedPlanVersionCompareResponse` |
| `GET` | `/{id}/history` | Riwayat | `ProcurementNeedPlan : Read` | — | `List<DocumentHistoryResponse>` |

### Corporate / Procurement Management / Planning / Consolidation

Base URL: `api/v1/corporate/procurement-management/plan-consolidations`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Gabungan baris rencana berlaku per barang, beserta rincian unit asal dan sisa belum dialokasikan (2.2) | `ProcurementPackagePlan : Read` | `fiscalYear`, `itemCategoryId?`, `procurementTypeId?` | `List<PlanConsolidationRow>` |

**Contoh baris:** `KTL-000045 Kursi Roda Standar` → total 30 (IGD 3, Rawat Inap 5, Poli 22), sudah
dialokasikan 0, sisa 30.

### Corporate / Procurement Management / Planning / Package Plan

Base URL: `api/v1/corporate/procurement-management/package-plans`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar rencana paket | `ProcurementPackagePlan : Read` | `PackagePlanQuery` | `PagedResult<PackagePlanResponse>` |
| `GET` | `/{id}` | Detail beserta baris dan unit asal | `ProcurementPackagePlan : Read` | — | `PackagePlanResponse` |
| `POST` | `/` | Buat dari baris konsolidasi | `ProcurementPackagePlan : Create` | `CreatePackagePlanRequest` | `PackagePlanResponse` (201) |
| `PUT` | `/{id}` | Ubah `DRAFT`/`RETURNED` | `ProcurementPackagePlan : Update` | `UpdatePackagePlanRequest` | `PackagePlanResponse` |
| `POST` | `/{id}/submit` | Ajukan | `ProcurementPackagePlan : Submit` | `Idempotency-Key` | `PackagePlanResponse` |
| `POST` | `/{id}/cancel` | Batalkan | `ProcurementPackagePlan : Cancel` | `{ reason }` | `PackagePlanResponse` |
| `GET` | `/{id}/history` | Riwayat | `ProcurementPackagePlan : Read` | — | `List<DocumentHistoryResponse>` |

### Corporate / Procurement Management / Planning / Budget

Base URL: `api/v1/corporate/procurement-management/budgets`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pagu berlaku dengan komitmen dan sisa | `ProcurementBudget : Read` | `BudgetQuery` | `PagedResult<BudgetResponse>` |
| `GET` | `/{id}` | Detail satu versi | `ProcurementBudget : Read` | — | `BudgetResponse` |
| `GET` | `/remaining` | Sisa pagu untuk tahun × unit × kategori × sumber dana (dipakai layar PR) | `ProcurementRequest : Read` | `BudgetRemainingQuery` | `List<BudgetRemainingResponse>` |
| `POST` | `/` | Buat pagu | `ProcurementBudget : Create` | `CreateBudgetRequest` | `BudgetResponse` (201) |
| `PUT` | `/{id}` | Ubah `DRAFT`/`RETURNED` | `ProcurementBudget : Update` | `UpdateBudgetRequest` | `BudgetResponse` |
| `POST` | `/{id}/submit` | Ajukan | `ProcurementBudget : Submit` | `Idempotency-Key` | `BudgetResponse` |
| `POST` | `/{id}/revisions` | Buat versi revisi | `ProcurementBudget : Revise` | `{ reason }` | `BudgetResponse` (201) |
| `GET` | `/{id}/history` | Riwayat | `ProcurementBudget : Read` | — | `List<DocumentHistoryResponse>` |

### Corporate / Procurement Management / Master Data / Item

Base URL: `api/v1/corporate/procurement-management/master-data/items` — **1.1**: sembilan endpoint baku master data (`PBJ-DEC-037`). Sudah ada di source (`BE-PBJ-003`), belum diuji runtime.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Konfigurasi saringan + pilihan jenis | `ProcurementItem : Read` | — | `ProcurementItemFilterMetadataResponse` |
| `GET` | `/summary` | Total, aktif, nonaktif, barang, jasa, terkait obat | `ProcurementItem : Read` | — | `ProcurementItemSummaryResponse` |
| `GET` | `/` | Daftar katalog | `ProcurementItem : Read` | `ProcurementItemQuery` | `PagedResult<ProcurementItemResponse>` |
| `GET` | `/options` | Pilihan barang aktif untuk form PR/rencana (ber-pagination) | `ProcurementItem : Read` | `ProcurementItemOptionsQuery` | `PagedResult<ProcurementItemOptionResponse>` |
| `GET` | `/{id}` | Detail | `ProcurementItem : Read` | — | `ProcurementItemResponse` |
| `POST` | `/` | Tambah; kode `KTL-…` dibuat server | `ProcurementItem : Create` | `UpsertProcurementItemRequest` | `ProcurementItemResponse` |
| `PUT` | `/{id}` | Ubah penuh | `ProcurementItem : Update` | `UpsertProcurementItemRequest` | `ProcurementItemResponse` |
| `PATCH` | `/{id}/status` | Aktif/nonaktif (menggantikan `activate`/`deactivate` versi 1.0) | `ProcurementItem : Update` | `UpdateProcurementMasterStatusRequest` | `ProcurementItemResponse` |
| `DELETE` | `/{id}` | Hapus lunak; ditolak 400 bila dipakai dokumen (`PBJ-VAL-071`) | `ProcurementItem : Delete` | — | `ProcurementMasterDeleteResponse` |

### Corporate / Procurement Management / Master Data / Item Category

Base URL: `api/v1/corporate/procurement-management/master-data/item-categories` — **1.1**, menggantikan rute `item-categories` di bawah controller "Reference" 1.0. Sembilan endpoint yang sama bentuknya dengan Item; resource `ProcurementItemCategory : Read/Create/Update/Delete`; request `UpsertProcurementReferenceRequest` (`code`, `name`, `description`, `isActive`), response `ProcurementReferenceResponse`.

### Corporate / Procurement Management / Master Data / Fund Source

Base URL: `api/v1/corporate/procurement-management/master-data/fund-sources` — **1.1**, sama seperti Item Category; resource `ProcurementFundSource`.

### Corporate / Procurement Management / Master Data / Procurement Type

Base URL: `api/v1/corporate/procurement-management/master-data/procurement-types` — **1.1**, hanya baca: `GET /`, `GET /options`, `GET /{id}`; resource `ProcurementType : Read`. Jenis diisi lewat data awal (`PBJ-DEC-024`).

Kode status master: 404 tidak ada/terhapus; 409 kode atau barang serupa sudah ada (`PBJ-VAL-070`); 422 isian tidak sah (`PBJ-VAL-072`); 400 masih dipakai data lain.

### Corporate / Procurement Management / Common / Attachment

Base URL: `api/v1/corporate/procurement-management/attachments`

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Lampiran sebuah dokumen (versi berlaku; `includeHistory` untuk semua versi) | `ProcurementAttachment : Read` **dan** hak baca dokumen induk | `documentType`, `documentId` | `List<AttachmentResponse>` |
| `POST` | `/` | Unggah (multipart); isi `replacesAttachmentId` untuk membuat versi baru | `ProcurementAttachment : Upload` **dan** hak ubah dokumen induk | `UploadAttachmentRequest` | `AttachmentResponse` (201) |
| `GET` | `/{id}/download` | Unduh | `ProcurementAttachment : Read` **dan** hak baca dokumen induk | — | berkas (`application/octet-stream`, `Content-Disposition: attachment`) |
| `POST` | `/{id}/deactivate` | Tarik lampiran dari dokumen yang masih draft | `ProcurementAttachment : Upload` | — | `AttachmentResponse` |

Kode 422 `PBJ-VAL-080`..`082` untuk berkas ditolak; 409 `PBJ-VAL-083` bila dokumen sudah final.

---

## Perubahan internal `Wfl` (tanpa endpoint baru)

| Perubahan | Dampak bagi konsumen API existing |
| --- | --- |
| `RequestContext` menerima `conditionCodes: string[]` | Tidak ada. Konteks lama tanpa field ini bekerja seperti sekarang |
| `MstWorkflowStep.ActivationConditionCode` | Endpoint pengaturan langkah workflow existing ikut menampilkan dan menerima field ini. **Perlu dicek** pada DTO langkah saat implementasi; bila DTO tidak memuatnya, langkah ber-kode diatur lewat data awal |
| Penangan `PBJ_*` terdaftar | Kotak masuk menampilkan instance Pengadaan bersama HR. Saringan `WorkflowCategory = "Procurement"` tersedia lewat definisi |
