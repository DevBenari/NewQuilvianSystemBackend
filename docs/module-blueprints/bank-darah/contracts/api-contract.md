# Bank Darah — API Contract

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v1` — `draft` |
| `last_changed_in` | `v1` |
| Owner | Pemilik arsitektur backend (bentuk kontrak) · pemilik proses BDRS (perilaku) |
| `approved_by` / `approved_at` | Kosong — `draft` |
| Sumber | `02-backend-architecture.md` (controller) · `contracts/state-transition-matrix.md` · `contracts/validation-matrix.md` |

**Seluruh endpoint di bawah berstatus `Rencana (belum tersedia)`** — belum ada di kode. Route & grup
Swagger mengikuti pola `LabOrderController` (`BD-CAP-014`); jangan menyimpulkan route dari URL frontend.
Respons dibungkus `ApiResponse<T>`; daftar memakai `PagedResult<T>` (`BD-CAP-012`).

Kolom **Hak akses** di sini adalah **satu-satunya** tempat pemetaan endpoint→hak akses hidup;
`permission-audit-matrix.md` tidak mendaftar ulang endpoint. Nilai ditulis `Resource : Action`, setara
`[AccessPermission("Resource", "Action")]`.

Kode status yang berlaku untuk seluruh grup: `200` berhasil · `400` isian tidak lengkap/format salah ·
`401` belum masuk · `403` tidak berhak · `404` tidak ditemukan · `409` bentrok konkurensi/status sudah
berubah · `422` melanggar aturan bisnis (kode `VAL-BD-*`). Diulang ringkas per grup hanya bila khas.

---

### Health Services / Blood Bank Management / Blood Order

Base URL: `api/v1/health-services/blood-bank-management/blood-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar order darah (daftar kerja #1, `DEC-BD-023`) | `BloodOrder : Read` | `BloodOrderPagedQuery` | `ApiResponse<PagedResult<BloodOrderListDto>>` | Rencana |
| `GET` | `/{id}` | Detail satu order beserta baris & pemenuhannya | `BloodOrder : Read` | — | `ApiResponse<BloodOrderDetailDto>` | Rencana |
| `GET` | `/{id}/fulfillment` | Ringkasan pemenuhan (diminta/diberikan/sisa), menghormati koreksi (`BD-DOM-17`) | `BloodOrder : Read` | — | `ApiResponse<FulfillmentSummaryDto>` | Rencana |
| `POST` | `/` | Buat order elektronik dari unit pelayanan | `BloodOrder : Create` | `CreateBloodOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `422 VAL-BD-001/013` |
| `POST` | `/manual` | Buat order manual oleh Bank Darah | `BloodOrder : Create` | `CreateManualBloodOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `400 VAL-BD-010` |
| `POST` | `/confirm-duplicate` | Lanjutkan order ganda dengan alasan tertulis (`ASM-BD-001`) | `BloodOrder : Create` | `ConfirmDuplicateOrderRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana |
| `POST` | `/{id}/cancel` | Batalkan order dengan alasan terkendali | `BloodOrder : Update` | `CancelWithReasonRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `422 VAL-BD-016` |

Kedaluwarsa order (`Expired`) tidak punya endpoint — dipicu sistem dari sinyal kunjungan (`DEC-BD-014`).

---

### Health Services / Blood Bank Management / Provider Request

Base URL: `api/v1/health-services/blood-bank-management/provider-requests`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar permintaan ke PMI | `BloodProviderRequest : Read` | `ProviderRequestPagedQuery` | `ApiResponse<PagedResult<ProviderRequestListDto>>` | Rencana |
| `GET` | `/{id}` | Detail permintaan + riwayat penerimaan | `BloodProviderRequest : Read` | — | `ApiResponse<ProviderRequestDetailDto>` | Rencana |
| `POST` | `/` | Buat permintaan atas nama satu pasien | `BloodProviderRequest : Create` | `CreateProviderRequestRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana · `422 VAL-BD-006` |
| `POST` | `/{id}/receipts` | Catat penerimaan kantong (termasuk kelebihan) | `BloodProviderRequest : Process` | `RecordReceiptRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana · `200 VAL-BD-014` |
| `POST` | `/{id}/cancel` | Batalkan permintaan | `BloodProviderRequest : Update` | `CancelWithReasonRequest` | `ApiResponse<ProviderRequestDetailDto>` | Rencana |

Penutupan administratif (`ClosedEncounter`) dipicu sistem, bukan endpoint (`DEC-BD-020`).

---

### Health Services / Blood Bank Management / Blood Unit

Base URL: `api/v1/health-services/blood-bank-management/blood-units`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar kantong; filter `status=PendingReview` = daftar kerja #2; filter `emergencyPendingEvidence=true` = daftar kerja #3 | `BloodUnit : Read` | `BloodUnitPagedQuery` | `ApiResponse<PagedResult<BloodUnitListDto>>` | Rencana |
| `GET` | `/{id}` | Detail kantong + riwayat alokasi/bukti/koreksi | `BloodUnit : Read` | — | `ApiResponse<BloodUnitDetailDto>` | Rencana |
| `POST` | `/{id}/allocate` | Alokasikan kantong ke satu baris kebutuhan | `BloodUnit : Allocate` | `AllocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `409 VAL-BD-018c` · `422 VAL-BD-033` |
| `POST` | `/{id}/cancel-allocation` | Batalkan alokasi keliru sebelum pemberian (`DEC-BD-029`) | `BloodUnit : Allocate` | `CancelWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-023` |
| `POST` | `/{id}/compatibility-evidence` | Catat bukti kecocokan terhadap pasien tujuan | `BloodUnit : Compatibility` | `RecordEvidenceRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana |
| `POST` | `/{id}/issue` | Berikan kantong kepada pasien | `BloodUnit : Issue` | `IssueUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-017/018/019/020` |
| `POST` | `/{id}/emergency-issue` | Berikan lewat jalur darurat (`DEC-BD-017`) | `BloodUnit : EmergencyIssue` | `EmergencyIssueRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-021` |
| `POST` | `/{id}/correction` | Catat koreksi pencatatan pemberian (`DEC-BD-030`) | `BloodUnit : Correct` | `IssuanceCorrectionRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-024` · `422 VAL-BD-025/049` |
| `POST` | `/{id}/reallocate` | Alihkan kantong `PendingReview` ke pasien lain | `BloodUnit : Resolve` | `ReallocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-016` |
| `POST` | `/{id}/return-to-provider` | Kembalikan kantong ke PMI | `BloodUnit : Resolve` | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana |
| `POST` | `/{id}/mark-not-usable` | Nyatakan kantong tidak layak | `BloodUnit : Resolve` | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana |

Pemberian (`issue`/`emergency-issue`) tidak dapat dibatalkan — status terminal. Koreksi tidak
memindahkan kantong keluar dari `Issued` (`VAL-BD-049`).

---

### Health Services / Blood Bank Management / Blood Group Exam

Base URL: `api/v1/health-services/blood-bank-management/blood-group-exams`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar pemeriksaan golongan darah | `BloodGroupExam : Read` | `BloodGroupExamPagedQuery` | `ApiResponse<PagedResult<BloodGroupExamListDto>>` | Rencana |
| `GET` | `/{id}` | Detail pemeriksaan + status konflik | `BloodGroupExam : Read` | — | `ApiResponse<BloodGroupExamDetailDto>` | Rencana |
| `GET` | `/patient/{patientId}/valid` | Golongan darah sah pasien / penanda konflik (`BD-DOM-21`) | `BloodGroupExam : Read` | — | `ApiResponse<ValidBloodGroupDto>` | Rencana |
| `POST` | `/` | Catat pengambilan sampel | `BloodGroupExam : Create` | `RecordSampleRequest` | `ApiResponse<BloodGroupExamDetailDto>` | Rencana |
| `POST` | `/{id}/result` | Catat hasil ABO & Rhesus | `BloodGroupExam : Update` | `RecordResultRequest` | `ApiResponse<BloodGroupExamDetailDto>` | Rencana · `400 VAL-BD-030` |
| `POST` | `/{id}/validate` | Validasi hasil (deteksi konflik `BD-XINV-04`) | `BloodGroupExam : Validate` | — | `ApiResponse<BloodGroupExamDetailDto>` | Rencana · `403 VAL-BD-037` |
| `POST` | `/conflict-resolution` | Selesaikan konflik dengan menunjuk pemeriksaan ulang tervalidasi (`DEC-BD-031`) | `BloodGroupExam : Validate` | `ResolveConflictRequest` | `ApiResponse<ValidBloodGroupDto>` | Rencana · `422 VAL-BD-051/054` |

Penyelesaian konflik dilakukan di layar pemeriksaan, **bukan** daftar kerja keempat (`DEC-BD-033`).

---

### Health Services / Blood Bank Management / Blood Bank Procedure

Base URL: `api/v1/health-services/blood-bank-management/blood-bank-procedures`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar tindakan Bank Darah | `BloodBankProcedure : Read` | `ProcedurePagedQuery` | `ApiResponse<PagedResult<ProcedureListDto>>` | Rencana |
| `GET` | `/{id}` | Detail tindakan | `BloodBankProcedure : Read` | — | `ApiResponse<ProcedureDetailDto>` | Rencana |
| `POST` | `/` | Catat tindakan atas satu order | `BloodBankProcedure : Create` | `CreateProcedureRequest` | `ApiResponse<ProcedureDetailDto>` | Rencana · `400 VAL-BD-026` |
| `POST` | `/{id}/complete` | Nyatakan tindakan selesai | `BloodBankProcedure : Update` | — | `ApiResponse<ProcedureDetailDto>` | Rencana |

**Tidak ada endpoint penyaluran biaya ke Billing** — tertahan `DEC-BD-016`. Fakta biaya boleh dirancang
sebagai kejadian domain nanti, tetapi kontraknya belum dibekukan.

---

### Health Services / Master Data / Blood Component

Base URL: `api/v1/health-services/master-data/blood-components`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar komponen darah | `BloodComponent : Read` | `PagedQuery` | `ApiResponse<PagedResult<BloodComponentDto>>` | Rencana |
| `GET` | `/options` | Opsi komponen untuk dropdown | `BloodComponent : Read` | — | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail komponen | `BloodComponent : Read` | — | `ApiResponse<BloodComponentDto>` | Rencana |
| `POST` | `/` | Tambah komponen | `BloodComponent : Create` | `UpsertBloodComponentRequest` | `ApiResponse<BloodComponentDto>` | Rencana |
| `PUT` | `/{id}` | Ubah komponen (termasuk `CompatibilityEvidenceValidityHours`) | `BloodComponent : Update` | `UpsertBloodComponentRequest` | `ApiResponse<BloodComponentDto>` | Rencana |
| `DELETE` | `/{id}` | Nonaktifkan komponen (soft delete) | `BloodComponent : Delete` | — | `ApiResponse<bool>` | Rencana |

---

### Health Services / Master Data / Blood Bank Reason

Base URL: `api/v1/health-services/master-data/blood-bank-reasons`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar alasan terkendali | `BloodBankReason : Read` | `PagedQuery` | `ApiResponse<PagedResult<BloodBankReasonDto>>` | Rencana |
| `GET` | `/options` | Opsi alasan per kategori (`?category=`) | `BloodBankReason : Read` | `category` | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail alasan | `BloodBankReason : Read` | — | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `POST` | `/` | Tambah alasan | `BloodBankReason : Create` | `UpsertReasonRequest` | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `PUT` | `/{id}` | Ubah alasan | `BloodBankReason : Update` | `UpsertReasonRequest` | `ApiResponse<BloodBankReasonDto>` | Rencana |
| `DELETE` | `/{id}` | Nonaktifkan alasan | `BloodBankReason : Delete` | — | `ApiResponse<bool>` | Rencana |

---

## Catatan kontrak

- **Kewenangan unit** (`DEC-BD-012`) tidak punya endpoint tersendiri; ia kolom `IsAvailableForBloodOrder`
  pada `MstServiceUnit`, dikelola lewat kontrak Master Data unit pelayanan yang sudah ada.
- **Tiga daftar kerja MVP** (`DEC-BD-023`) semuanya endpoint `GET` dengan filter, bukan modul laporan.
- DTO didaftar di `02-backend-architecture.md`/`data-dictionary.md`; field lengkap dibekukan saat
  implementasi. Nomor bisnis dialokasikan service lewat number-series, **tidak** dikirim klien.
