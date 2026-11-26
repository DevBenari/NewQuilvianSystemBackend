# Pemilihan Penyedia — Kontrak API

| Field | Nilai |
| --- | --- |
| Kontrak | `PBJ-PS-API` · `last_changed_in: 1.0` · status `draft` |
| Pemilik | Pengguna sesi · `approved_by: —` · `approved_at: —` |
| Masukan | `02-backend-architecture.md` (draft); `PBJ-PS-STATE` 1.0; `PBJ-PS-VAL` 1.0; konvensi `PBJ-PP-API` 1.4 dan `PBJ-VD-API` 1.1 |
| Kompatibilitas | Seluruh endpoint Sourcing baru. Perubahan pada sub-modul lain dicatat di bagian akhir (PR, lampiran) |

**Status endpoint:** seluruh endpoint di dokumen ini **Rencana (belum tersedia)**.

Konvensi bersama: `ApiResponse<T>`, `PagedResult<T>`, `Idempotency-Key` wajib pada aksi status, `expectedRowVersion` pada
perubahan, saringan daftar `startDate`/`endDate`/`customPeriod` + 1–2 saringan relevan + status + pencarian + jumlah baris
(`PBJ-DEC-055`, `PBJ-FE-004`). Base URL: `api/v1/corporate/procurement-management`.

**Harga terbatas** (`PBJ-DEC-079`): pada setiap respons di bawah, field harga (HPS, harga satuan, total, PPN, peringkat
bernilai) berisi `null` bila pengguna tidak punya `ProcurementPackage : ReadPrice`. Endpoint `technical-evaluations` tidak
memuat field harga sama sekali.

| Kode | Arti bagi pengguna |
| --- | --- |
| 200 / 201 | Berhasil |
| 400 | Isian tidak lengkap atau formatnya salah |
| 401 | Belum masuk |
| 403 | Tidak berizin, atau aturan pemisahan tugas (`PBJ-VAL-135`) |
| 404 | Data tidak ada atau di luar cakupan Anda |
| 409 | Status tidak mengizinkan, data basi, atau bentrok keunikan |
| 422 | Aturan bisnis menolak; `errors.code` menyebut aturannya |

---

### Corporate / Procurement Management / Sourcing / Procurement Package

Base URL: `/procurement-packages` — **Rencana (belum tersedia)**.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status, jenis pengadaan, tahun, periode | `ProcurementPackage : Read` | — | `ApiResponse<PackageFilterMetadata>` |
| `GET` | `/` | Daftar paket; saringan jenis pengadaan dan tahun | `ProcurementPackage : Read` | `PackageQuery` | `PagedResult<PackageListItem>` |
| `GET` | `/packageable-request-lines` | Baris PR `APPROVED` yang belum dipaketkan | `ProcurementPackage : Create` | `PackageableRequestLineQuery` | `PagedResult<PackageableRequestLineResponse>` |
| `POST` | `/` | Buat paket dari baris PR (`PBJ-DEC-071`) | `ProcurementPackage : Create` | `CreatePackageRequest` | `ApiResponse<PackageResponse>` 201 |
| `GET` | `/{id}` | Detail paket | `ProcurementPackage : Read` | — | `ApiResponse<PackageResponse>` |
| `PUT` | `/{id}` | Ubah judul, spesifikasi, HPS satuan, metode, alasan metode | `ProcurementPackage : Update` | `UpdatePackagePreparationRequest` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/lines` | Tambah baris PR | `ProcurementPackage : Update` | `AddPackageLinesRequest` | `ApiResponse<PackageResponse>` |
| `DELETE` | `/{id}/sources/{sourceId}` | Lepas satu baris PR (hanya `DRAFT`/`PREPARATION_RETURNED`) | `ProcurementPackage : Update` | `expectedRowVersion` | `ApiResponse<PackageResponse>` |
| `GET` | `/{id}/method-suggestion` | Metode yang rentangnya mencakup HPS | `ProcurementPackage : Read` | — | `ApiResponse<MethodSuggestionResponse>` |
| `PUT` | `/{id}/items/{itemId}/criteria` | Tetapkan kriteria teknis barang | `ProcurementPackage : Update` | `UpdateTechnicalCriteriaRequest` | `ApiResponse<PackageResponse>` |
| `PUT` | `/{id}/evaluators` | Tetapkan evaluator | `ProcurementPackage : Update` | `UpdateEvaluatorsRequest` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/preparation/submit` | Ajukan persiapan ke `Wfl` | `ProcurementPackage : Submit` | `expectedRowVersion` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/preparation/withdraw` | Tarik pengajuan persiapan | `ProcurementPackage : Submit` | `expectedRowVersion` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/preparation/reopen` | Buka ulang persiapan | `ProcurementPackage : Revise` | `ReasonRequest` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/split` | Pecah paket (`PBJ-DEC-083`) | `ProcurementPackage : Update` | `SplitPackageRequest` | `ApiResponse<SplitPackageResponse>` (paket sumber + paket baru) |
| `POST` | `/{id}/cancel` | Batalkan paket (`PBJ-DEC-077`) | `ProcurementPackage : Cancel` | `CancelPackageRequest` | `ApiResponse<PackageResponse>` |
| `POST` | `/{id}/purchase-order` | Catat nomor PO BLM (`PBJ-ASM-009`) | `ProcurementPackage : RecordOrder` | `RecordPurchaseOrderRequest` | `ApiResponse<PackageResponse>` |
| `GET` | `/{id}/approval` | Linimasa persetujuan persiapan dan penetapan (pola `PBJ-DEC-040`) | `ProcurementPackage : Read` | — | `ApiResponse<ProcurementApprovalTimeline>` |
| `POST` | `/{id}/workflow/synchronize` | Sinkron ulang hasil `Wfl` yang gagal diterapkan | `ProcurementPackage : Submit` | — | `ApiResponse<PackageResponse>` |
| `GET` | `/{id}/history` | Riwayat paket | `ProcurementPackage : Read` | — | `PagedResult<DocumentHistoryItem>` |
| `GET` | `/request-summary` | Ringkasan paket untuk baris PR milik unit pengguna (`PBJ-DEC-079`, `080`) | `ProcurementPackage : ReadSummary` | `purchaseRequestId` | `ApiResponse<List<PackageRequestSummary>>` (nomor paket, status, vendor terpilih; tanpa harga) |

Contoh respons `/request-summary` untuk Rina (IGD): `[{ "purchaseRequestLineId": "…", "packageNumber": "PKT-000012",
"status": "AWARDED", "awardedSupplierName": "PT Sehat Medika" }]`.

### Corporate / Procurement Management / Sourcing / Procurement RFQ

Base URL: `/procurement-rfqs` — **Rencana (belum tersedia)**.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/filters/metadata` | Pilihan status, periode | `ProcurementRfq : Read` | — | `ApiResponse<RfqFilterMetadata>` |
| `GET` | `/` | Daftar RFQ; saringan status, paket | `ProcurementRfq : Read` | `RfqQuery` | `PagedResult<RfqListItem>` |
| `GET` | `/eligible-vendors` | Vendor layak untuk **semua** kategori paket (`PBJ-ASM-006`) | `ProcurementRfq : Create` | `packageId` | `ApiResponse<List<EligibleVendorResponse>>` (layak, dibatasi, peringatan) |
| `POST` | `/` | Buat RFQ draf | `ProcurementRfq : Create` | `CreateRfqRequest` | `ApiResponse<RfqResponse>` 201 |
| `GET` | `/{id}` | Detail RFQ, undangan, penawaran | `ProcurementRfq : Read` | — | `ApiResponse<RfqResponse>` |
| `PUT` | `/{id}` | Ubah draf | `ProcurementRfq : Update` | `UpdateRfqRequest` | `ApiResponse<RfqResponse>` |
| `POST` | `/{id}/send` | Kirim (nomor `RFQ` dialokasikan) | `ProcurementRfq : Send` | `expectedRowVersion` | `ApiResponse<RfqResponse>` |
| `POST` | `/{id}/extend-deadline` | Perpanjang batas waktu (`PBJ-DEC-081`) | `ProcurementRfq : Update` | `ExtendRfqDeadlineRequest` | `ApiResponse<RfqResponse>` |
| `POST` | `/{id}/close` | Tutup penerimaan; paket ke `EVALUATION` | `ProcurementRfq : Close` | `expectedRowVersion` | `ApiResponse<RfqResponse>` |
| `POST` | `/{id}/fail` | Nyatakan gagal (`PBJ-DEC-075`) | `ProcurementRfq : Close` | `ReasonRequest` | `ApiResponse<RfqResponse>` |
| `GET` | `/{id}/document` | Unduh dokumen RFQ untuk dikirim ke vendor **[USULAN `G-P06`]** | `ProcurementRfq : Read` | — | Berkas PDF |

### Corporate / Procurement Management / Sourcing / Procurement Quotation

Base URL: `/procurement-quotations` — **Rencana (belum tersedia)**.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Penawaran per RFQ atau paket | `ProcurementQuotation : Read` | `rfqId` / `packageId` | `ApiResponse<List<QuotationListItem>>` |
| `GET` | `/{id}` | Detail penawaran, baris, negosiasi | `ProcurementQuotation : Read` | — | `ApiResponse<QuotationResponse>` |
| `POST` | `/` | Catat penawaran (`PBJ-DEC-067`, `084`) | `ProcurementQuotation : Create` | `RecordQuotationRequest` | `ApiResponse<QuotationResponse>` 201 |
| `PUT` | `/{id}` | Ubah catatan (sebelum evaluasi/negosiasi) | `ProcurementQuotation : Update` | `UpdateQuotationRequest` | `ApiResponse<QuotationResponse>` |
| `POST` | `/{id}/void` | Batalkan catatan salah | `ProcurementQuotation : Cancel` | `ReasonRequest` | `ApiResponse<QuotationResponse>` |
| `POST` | `/{id}/negotiations` | Catat putaran negosiasi (`PBJ-ASM-005`) | `ProcurementQuotation : Negotiate` | `RecordNegotiationRequest` | `ApiResponse<QuotationResponse>` |
| `GET` | `/comparison` | Perbandingan harga dan peringkat (`PBJ-DEC-085`) | `ProcurementQuotation : Read` | `packageId` | `ApiResponse<QuotationComparisonResponse>` |

Contoh `POST /`: `{ "rfqId": "…", "supplierId": "…", "receivedAt": "2027-04-10T15:20:00+07:00", "taxPercent": 11,
"lines": [{ "packageItemId": "…", "unitPriceBeforeTax": 2500000 }] }` → `finalUnitPrice` 2.775.000,00 (2.500.000 × 1,11),
`isLate: false` bila batas waktu RFQ 10-04-2027 pukul 16.00.

### Corporate / Procurement Management / Sourcing / Technical Evaluation

Base URL: `/technical-evaluations` — **Rencana (belum tersedia)**. Hanya paket yang menugaskan pengguna login (`PBJ-DEC-074`).

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Paket yang ditugaskan kepada saya | `ProcurementTechnicalEvaluation : Read` | `TechnicalEvaluationQuery` | `PagedResult<EvaluationPackageListItem>` |
| `GET` | `/{packageId}` | Barang, spesifikasi, kriteria, penawaran **tanpa harga** | `ProcurementTechnicalEvaluation : Read` | — | `ApiResponse<EvaluationPackageResponse>` |
| `PUT` | `/{packageId}/quotations/{quotationId}` | Isi hasil per baris × kriteria | `ProcurementTechnicalEvaluation : Evaluate` | `SubmitEvaluationResultRequest` | `ApiResponse<EvaluationPackageResponse>` |

Evaluator membuka lampiran lewat `ProcurementAttachmentsController` yang sudah ada; lampiran `IsPriceSensitive` disaring
(bagian akhir).

### Corporate / Procurement Management / Sourcing / Package Award

Base URL: `/procurement-package-awards` — **Rencana (belum tersedia)**.

| Method | Path | Kegunaan | Hak akses | Request | Response |
| --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar penetapan; saringan status, tahun | `ProcurementPackageAward : Read` | `AwardQuery` | `PagedResult<AwardListItem>` |
| `GET` | `/{id}` | Detail penetapan dan peringkat tersimpan | `ProcurementPackageAward : Read` | — | `ApiResponse<AwardResponse>` |
| `POST` | `/` | Ajukan penetapan ke `Wfl` | `ProcurementPackageAward : Submit` | `SubmitAwardRequest` | `ApiResponse<AwardResponse>` 201 |
| `POST` | `/{id}/resubmit` | Ajukan ulang penetapan `RETURNED` | `ProcurementPackageAward : Submit` | `SubmitAwardRequest` | `ApiResponse<AwardResponse>` |
| `POST` | `/{id}/withdraw` | Tarik pengajuan | `ProcurementPackageAward : Submit` | `expectedRowVersion` | `ApiResponse<AwardResponse>` |
| `POST` | `/{id}/revoke` | Cabut penetapan disetujui sebelum PO **[USULAN `G-P09`]** | `ProcurementPackageAward : Revoke` | `ReasonRequest` | `ApiResponse<AwardResponse>` |
| `POST` | `/{id}/workflow/synchronize` | Sinkron ulang hasil `Wfl` | `ProcurementPackageAward : Submit` | — | `ApiResponse<AwardResponse>` |

### Corporate / Procurement Management / Master Data / Procurement Method

Base URL: `/master-data/procurement-methods` — **Rencana (belum tersedia)**. Sembilan endpoint baku master data (pola
`PBJ-DEC-037`): `GET /filters/metadata`, `GET /summary`, `GET /`, `GET /options`, `GET /{id}`, `POST /`, `PUT /{id}`,
`PATCH /{id}/status`, `DELETE /{id}`. Resource `ProcurementMethod : Read/Create/Update/Delete`. Hapus metode yang sudah
dipakai paket → 400 `errors.code` `PBJ-VAL-071`. `GET /options` hanya metode aktif.

### Corporate / Procurement Management / Master Data / Package Document Type

Base URL: `/master-data/package-document-types` — **Rencana (belum tersedia)**. Sembilan endpoint baku. Resource
`ProcurementPackageDocumentType : Read/Create/Update/Delete`. Hapus jenis yang dipakai checklist atau lampiran → 400
`PBJ-VAL-071`.

### Corporate / Procurement Management / Master Data / Package Document Requirement

Base URL: `/master-data/package-document-requirements` — **Rencana (belum tersedia)**. Sembilan endpoint baku. Resource
`ProcurementPackageDocumentRequirement : Read/Create/Update/Delete`. Isian wajib tepat satu dari `methodId` atau
`procurementTypeId` (400 bila keduanya atau tidak satu pun).

---

## Perubahan pada sub-modul lain (sudah ada — perilaku berubah)

| Endpoint | Pemilik | Perubahan |
| --- | --- | --- |
| `GET /purchase-requests/{id}` | `perencanaan-permintaan` | Baris membawa `packageId` (aditif). Nomor dan status paket dibaca layar PR lewat `/procurement-packages/request-summary` |
| `GET /purchase-requests/budget-remaining`, `GET /budgets/*` | `perencanaan-permintaan` | Label nilai menjadi "termasuk PPN" (`PBJ-DEC-084`); bentuk data tidak berubah |
| `GET /attachments` (Common) | `perencanaan-permintaan` | Untuk `PBJ_PACKAGE`/`PBJ_RFQ`: lampiran berkategori jenis dokumen `IsPriceSensitive` tidak dikembalikan, dan unduhannya 404, bagi pengguna tanpa `ProcurementPackage : ReadPrice` |
