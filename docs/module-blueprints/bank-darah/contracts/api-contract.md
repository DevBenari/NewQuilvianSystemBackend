# Bank Darah — API Contract

| Field | Value |
| --- | --- |
| Blueprint ID | `BD-BP-001` · Contract version `v4` — **`approved`** |
| `last_changed_in` | `v4` |
| Owner | Pemilik arsitektur backend (bentuk kontrak) · pemilik proses BDRS (perilaku) |
| `approved_by` / `approved_at` | `Sukmagp` / `2026-09-03` |
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
| `POST` | `/{id}/cancel` | Batalkan order dengan alasan terkendali — oleh **dokter peminta** atau **petugas BDRS** (`DEC-BD-044`) | **`BloodOrder : Cancel`** | `CancelWithReasonRequest` | `ApiResponse<BloodOrderDetailDto>` | Rencana · `422 VAL-BD-016/083` |

Kedaluwarsa order (`Expired`) tidak punya endpoint — dipicu sistem dari sinyal kunjungan (`DEC-BD-014`).

**`BloodOrder : Cancel` dipisah dari `BloodOrder : Update` (`DEC-BD-044`).** Pemisahan ini membuat
wewenang membatalkan dapat diberikan kepada dokter peminta **tanpa** ikut memberikan wewenang menyunting
order secara umum. Keduanya memakai **satu** butir yang sama — dokter maupun petugas BDRS — dan yang
membedakan sebabnya pada rekam adalah **kategori alasan** yang wajib diisi: pembatalan klinis atau
pembatalan operasional. Tidak ada pembatalan order tanpa audit (`INV-BD-035`).

---

### Health Services / Master Data / Blood Storage Location

Base URL: `api/v1/health-services/master-data/blood-storage-locations`

Master lokasi penyimpanan darah milik BDRS (`DEC-BD-035`). **Bukan** cold storage farmasi —
`MstDrugStorageLocation` punya controller sendiri dan tidak disentuh modul ini.

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
| --- | --- | --- | --- | --- | --- | --- |
| `GET` | `/` | Daftar lokasi penyimpanan darah | `BloodStorageLocation : Read` | `BloodStorageLocationPagedQuery` | `ApiResponse<PagedResult<BloodStorageLocationListDto>>` | Rencana |
| `GET` | `/options` | Pilihan lokasi untuk dropdown; **hanya yang `IsActive = true`** | `BloodStorageLocation : Read` | — | `ApiResponse<List<OptionDto>>` | Rencana |
| `GET` | `/{id}` | Detail satu lokasi | `BloodStorageLocation : Read` | — | `ApiResponse<BloodStorageLocationDto>` | Rencana |
| `POST` | `/` | Tambah lokasi penyimpanan darah | `BloodStorageLocation : Create` | `CreateBloodStorageLocationRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `422 VAL-BD-067` |
| `PUT` | `/{id}` | Ubah kode, nama, urutan, keterangan | `BloodStorageLocation : Update` | `UpdateBloodStorageLocationRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `422 VAL-BD-067` |
| `PATCH` | `/{id}/status` | **Aktifkan atau nonaktifkan lokasi** (`DEC-BD-037`) | `BloodStorageLocation : Update` | `SetActiveStatusRequest` | `ApiResponse<BloodStorageLocationDto>` | Rencana · `200 VAL-BD-068` |

`GET /options` sengaja menyaring hanya lokasi aktif, sehingga frontend tidak perlu menyaring sendiri dan
tidak mungkin menawarkan lokasi nonaktif sebagai tujuan penyimpanan (`INV-BD-027`).

`PATCH /{id}/status` **tidak** memindahkan kantong apa pun dan **tidak** mengembalikan daftar kantong
terdampak sebagai akibat. Respons `200` disertai peringatan `VAL-BD-068` yang menyebut **berapa banyak**
kantong kini tertahan, supaya petugas tahu ada pekerjaan yang menunggu — pekerjaan itu dikerjakan lewat
`PUT /blood-units/{id}/storage-location`, satu per satu, oleh manusia (`DEC-BD-037`).

Lokasi **tidak dapat dihapus**; hanya dinonaktifkan. Penempatan lama menunjuk ke sini lewat `Restrict`,
dan riwayat kantong wajib tetap terbaca.

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
| `GET` | `/{id}/placements` | Riwayat penempatan kantong: di kulkas mana, sejak kapan, oleh siapa | `BloodUnit : Read` | — | `ApiResponse<List<BloodUnitPlacementDto>>` | Rencana |
| `POST` | `/{id}/storage-location` | **Tetapkan lokasi penyimpanan pertama** — membawa kantong `Received`→`Stored`→`Available` (`DEC-BD-036`) | `BloodUnit : Store` | `AssignStorageLocationRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-060/061` |
| `PUT` | `/{id}/storage-location` | **Pindahkan kantong ke lokasi lain** — status **tidak** berubah (`INV-BD-026`) | `BloodUnit : Store` | `MoveStorageLocationRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-060/062` |
| `POST` | `/{id}/allocate` | Alokasikan kantong ke satu baris kebutuhan | `BloodUnit : Allocate` | `AllocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `409 VAL-BD-018c` · `422 VAL-BD-033/063/064` |
| `POST` | `/{id}/cancel-allocation` | Batalkan alokasi keliru sebelum pemberian (`DEC-BD-029`) | `BloodUnit : Allocate` | `CancelWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-023` |
| `POST` | `/{id}/compatibility-evidence` | Catat bukti kecocokan terhadap pasien tujuan, **beserta hasil keputusannya** (`DEC-BD-042`) | `BloodUnit : Compatibility` | `RecordEvidenceRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-078` · `422 VAL-BD-079` |
| `POST` | `/{id}/issue` | Berikan kantong kepada pasien | `BloodUnit : Issue` | `IssueUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `422 VAL-BD-017/018/019/020/065/079` |
| `POST` | `/{id}/emergency-issue` | Berikan lewat jalur darurat, melewati gerbang bukti dan/atau lokasi nonaktif (`DEC-BD-017`, `DEC-BD-038`). Penerbit **Dokter BDRS atau DPJP** (`DEC-BD-040`) | `BloodUnit : EmergencyIssue` | `EmergencyIssueRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-021/072` · `422 VAL-BD-066/070/071` |
| `POST` | `/{id}/corrections` | **Ajukan** koreksi pencatatan pemberian; koreksi belum berlaku (`DEC-BD-041`) | `BloodUnit : Correct` | `RequestIssuanceCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | Rencana · `403 VAL-BD-024` · `422 VAL-BD-025/049/076` |
| `POST` | `/{id}/corrections/{correctionId}/approve` | **Setujui** koreksi; sejak saat ini koreksi berlaku dan pemenuhan dihitung ulang | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | Rencana · `403 VAL-BD-074` · `422 VAL-BD-073/075` |
| `POST` | `/{id}/corrections/{correctionId}/reject` | **Tolak** koreksi; rekam tidak berubah sama sekali | `BloodUnit : ApproveCorrection` | `DecideCorrectionRequest` | `ApiResponse<IssuanceCorrectionDto>` | Rencana · `403 VAL-BD-074` · `422 VAL-BD-073/075/077` |
| `GET` | `/{id}/corrections` | Daftar koreksi pada kantong ini beserta keadaannya | `BloodUnit : Read` | — | `ApiResponse<List<IssuanceCorrectionDto>>` | Rencana |
| `POST` | `/{id}/reallocate` | Alihkan kantong `PendingReview` ke pasien lain | **`BloodUnit : ResolveReallocate`** | `ReallocateUnitRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-080` · `422 VAL-BD-016/064` |
| `POST` | `/{id}/return-to-provider` | Kembalikan kantong ke PMI | **`BloodUnit : ResolveReturn`** | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-081` |
| `POST` | `/{id}/mark-not-usable` | Nyatakan kantong tidak layak | **`BloodUnit : ResolveNotUsable`** | `ResolveWithReasonRequest` | `ApiResponse<BloodUnitDetailDto>` | Rencana · `403 VAL-BD-082` |

Pemberian (`issue`/`emergency-issue`) tidak dapat dibatalkan — status terminal. Koreksi tidak
memindahkan kantong keluar dari `Issued` (`VAL-BD-049`).

**Tiga endpoint penyelesaian, tiga butir hak akses berbeda (`DEC-BD-043`).** Ketiganya berangkat dari
`PendingReview` tetapi arah risikonya berlawanan: pengalihan **memasukkan** darah ke tubuh pasien baru,
sedangkan pengembalian dan penetapan tidak layak **mengeluarkan** darah dari peredaran. Satu butir
`Resolve` untuk ketiganya berarti siapa pun yang boleh membuang kantong rusak otomatis boleh
mengalihkan darah ke pasien lain — dan itu justru tindakan paling berisiko di antara ketiganya.
Endpoint-nya sendiri **tidak berubah**; yang berubah hanya penjaganya.

**Bukti kecocokan kini menyimpan hasil keputusan.** `RecordEvidenceRequest` bertambah satu isian wajib:
hasilnya cocok atau tidak cocok. Bukti bertanda tidak cocok **tetap tersimpan** dan **tidak** membuka
gerbang pemberian — karena itu `POST /{id}/issue` bertambah kemungkinan penolakan `VAL-BD-079`.

**Koreksi memakai tiga endpoint, bukan satu, dan itu bukan pemecahan kosmetik.** `DEC-BD-041`
menjadikan koreksi proses dua tahap dengan dua pelaku berbeda dan dua butir hak akses berbeda.
Menyatukannya menjadi satu endpoint bersaklar akan membuat satu butir hak akses menjaga dua tindakan
yang justru sengaja dipisah, sehingga pemisahan wewenangnya hilang di lapisan API.

`correctionId` muncul pada path karena satu kantong dapat punya lebih dari satu koreksi sepanjang
riwayatnya — sebagian disetujui, sebagian ditolak. Keputusan selalu menunjuk satu permintaan tertentu.

**Respons koreksi mengembalikan `IssuanceCorrectionDto`, bukan `BloodUnitDetailDto`.** Berbeda dengan
`v2`, mengajukan koreksi **tidak mengubah keadaan kantong** — kantong tetap `Issued` dan angka
pemenuhan tidak bergerak sampai persetujuan turun. Mengembalikan detail kantong akan menyiratkan
sesuatu telah berubah pada kantong, padahal belum.

**Dua endpoint penyimpanan memakai method berbeda dengan sengaja.** `POST /{id}/storage-location`
adalah penetapan **pertama** dan satu-satunya yang memindahkan status (`Received`→`Stored`→`Available`);
ia gagal bila kantong sudah pernah ditempatkan. `PUT /{id}/storage-location` adalah **perpindahan** dan
tidak pernah menyentuh status; ia gagal bila kantong belum pernah ditempatkan. Keduanya sama-sama
menambah satu baris riwayat yang tidak pernah dihapus (`INV-BD-026`), dan keduanya menolak lokasi
tujuan yang nonaktif (`INV-BD-027`).

**Tidak ada endpoint untuk memindahkan kantong secara massal saat lokasi dinonaktifkan.** Itu disengaja:
`DEC-BD-037` menetapkan sistem tidak memindahkan kantong dengan sendirinya. Petugas memindahkan satu per
satu lewat `PUT /{id}/storage-location`, dan setiap perpindahan menyimpan pelaku serta waktunya sendiri.

**Tidak ada endpoint untuk "menutup gerbang".** Gerbang dinilai saat alokasi dan pemberian dicoba
(`ARCH-BD-POS-06`, `ARCH-BD-POS-07`); menonaktifkan lokasi lewat endpoint master sudah cukup untuk
menutupnya pada saat yang sama.

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
| `POST` | `/{id}/validate` | Validasi hasil **rutin** (deteksi konflik `BD-XINV-04`) | `BloodGroupExam : Validate` | — | `ApiResponse<BloodGroupExamDetailDto>` | Rencana · `403 VAL-BD-037` |
| `POST` | `/conflict-resolution` | Selesaikan konflik dengan menunjuk pemeriksaan ulang tervalidasi (`DEC-BD-031`) | **`BloodGroupExam : ResolveConflict`** | `ResolveConflictRequest` | `ApiResponse<ValidBloodGroupDto>` | Rencana · `403 VAL-BD-069` · `422 VAL-BD-051/054` |

**Dua butir hak akses, bukan satu (`DEC-BD-039`).** `Validate` menjaga validasi hasil rutin dan boleh
dipegang petugas BDRS yang ditunjuk; `ResolveConflict` menjaga penyelesaian konflik dan hanya dipegang
validator klinis. Pemisahan ini ada di lapisan hak akses, bukan hanya di dokumen: satu butir yang
menjaga keduanya membuat siapa pun yang boleh memvalidasi hasil rutin otomatis boleh menutup konflik.

Gerbang wewenang **tidak** menggantikan prasyarat. `DEC-BD-031` tetap berlaku penuh — penyelesaian
konflik wajib menunjuk pemeriksaan ulang tervalidasi (`VAL-BD-051`), dan validator klinis sekalipun
tidak dapat menutup konflik tanpa itu.

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
