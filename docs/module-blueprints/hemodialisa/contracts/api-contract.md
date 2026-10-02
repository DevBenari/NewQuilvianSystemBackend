# Hemodialisa — Kontrak API

| Field | Value |
|---|---|
| Blueprint ID | `HMD-BP-001` |
| Contract version | `HMD-CONTRACT-v1` — `last_changed_in: HMD-CONTRACT-v1`, status `approved` |
| Owner | Muhammad Hamzah |
| `approved_by` / `approved_at` | Muhammad Hamzah / 2026-09-18T15:40:07+07:00 |
| Backend SHA | `190c91a0` |
| `input_revision` | `02-backend-architecture.md` r1 |

> **Seluruh endpoint pada dokumen ini berstatus `Rencana (belum tersedia)`.** Tidak ada satu pun
> yang sudah dapat dipanggil. Pencarian `hemodial` pada seluruh berkas `.cs` backend menghasilkan
> nol berkas (`HMD-FACT-007`).

Kolom **Hak akses** pada dokumen ini adalah **satu-satunya** tempat pemetaan endpoint ke hak
akses hidup di seluruh blueprint. `contracts/permission-audit-matrix.md` tidak mendaftar ulang.

---

## Health Services / Hemodialysis Management / Hemodialysis Order

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-orders`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar permintaan HD dengan saringan status, prioritas, dan tanggal | `HemodialysisOrder : Read` | `HmdOrderPagedQuery` | `ApiResponse<PagedResult<HmdOrderResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian satu permintaan | `HemodialysisOrder : Read` | — | `ApiResponse<HmdOrderDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat permintaan HD dari bangsal, poliklinik, atau IGD | `HemodialysisOrder : Create` | `CreateHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/accept` | Koordinator menerima permintaan | `HemodialysisOrder : Accept` | `AcceptHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/hold` | Menahan permintaan karena alasan operasional | `HemodialysisOrder : Hold` | `HoldHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/release-hold` | Melepas penahanan, kembali ke status sebelumnya | `HemodialysisOrder : Hold` | — | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/reject` | Dokter menolak permintaan karena alasan klinis | `HemodialysisOrder : Reject` | `RejectHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Pembuat membatalkan permintaannya sendiri | `HemodialysisOrder : Cancel` | `CancelHmdOrderRequest` | `ApiResponse<HmdOrderResponse>` | **Rencana (belum tersedia)** |

Arti kode status bagi pengguna:

- **200** — permintaan berhasil dibaca atau diubah statusnya.
- **201** — permintaan baru berhasil dibuat.
- **400** — isian yang dikirim tidak lengkap atau tidak masuk akal, misalnya alasan penolakan
  kosong.
- **403** — pengguna tidak punya hak untuk tindakan ini. Menolak permintaan, misalnya, hanya
  boleh dokter.
- **404** — permintaan tidak ditemukan, atau sudah dihapus.
- **409** — status permintaan sudah berubah di tangan orang lain. Contoh: koordinator menerima
  permintaan yang barusan dibatalkan pembuatnya.

---

## Health Services / Hemodialysis Management / Hemodialysis Episode

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-episodes`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar pasien HD beserta status episodenya | `HemodialysisEpisode : Read` | `HmdEpisodePagedQuery` | `ApiResponse<PagedResult<HmdEpisodeListResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Ringkasan satu episode beserta konteks pasien | `HemodialysisEpisode : Read` | — | `ApiResponse<HmdEpisodeDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuka episode HD baru untuk seorang pasien | `HemodialysisEpisode : Create` | `CreateHmdEpisodeRequest` | `ApiResponse<HmdEpisodeResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Memperbarui data administratif episode | `HemodialysisEpisode : Update` | `UpdateHmdEpisodeRequest` | `ApiResponse<HmdEpisodeResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengaktifkan, menangguhkan, atau menutup episode | `HemodialysisEpisode : ChangeStatus` | `ChangeHmdEpisodeStatusRequest` | `ApiResponse<HmdEpisodeResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/eligibility-assessments` | Riwayat penilaian kelayakan | `HemodialysisEligibility : Read` | — | `ApiResponse<PagedResult<HmdEligibilityResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/eligibility-assessments` | Dokter mencatat keputusan kelayakan | `HemodialysisEligibility : Decide` | `CreateHmdEligibilityRequest` | `ApiResponse<HmdEligibilityResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/vascular-accesses` | Daftar akses vaskular pasien | `HemodialysisVascularAccess : Read` | — | `ApiResponse<PagedResult<HmdVascularAccessResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/vascular-accesses` | Mencatat akses vaskular baru | `HemodialysisVascularAccess : Create` | `CreateHmdVascularAccessRequest` | `ApiResponse<HmdVascularAccessResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/vascular-accesses/{accessId}/status` | Mengubah kondisi akses vaskular | `HemodialysisVascularAccess : Update` | `ChangeHmdVascularAccessStatusRequest` | `ApiResponse<HmdVascularAccessResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/serology-reviews` | Status serologi terakhir pasien | `HemodialysisSerology : Read` | — | `ApiResponse<PagedResult<HmdSerologyReviewResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/serology-reviews` | Mencatat rujukan hasil serologi dan hasil tinjauannya | `HemodialysisSerology : Create` | `CreateHmdSerologyReviewRequest` | `ApiResponse<HmdSerologyReviewResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/isolation-decisions` | Riwayat keputusan isolasi | `HemodialysisIsolation : Read` | — | `ApiResponse<PagedResult<HmdIsolationDecisionResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/isolation-decisions` | Menetapkan kebutuhan isolasi pasien | `HemodialysisIsolation : Decide` | `CreateHmdIsolationDecisionRequest` | `ApiResponse<HmdIsolationDecisionResponse>` | **Rencana (belum tersedia)** |

Arti kode status yang khas bagian ini:

- **409** — pasien sudah punya episode HD yang aktif, sehingga episode kedua ditolak.
- **422** — episode tidak dapat ditutup karena masih ada sesi yang belum difinalisasi.

---

## Health Services / Hemodialysis Management / Hemodialysis Prescription

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-prescriptions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Riwayat resep pada satu episode | `HemodialysisPrescription : Read` | `HmdPrescriptionPagedQuery` | `ApiResponse<PagedResult<HmdPrescriptionResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian satu resep | `HemodialysisPrescription : Read` | — | `ApiResponse<HmdPrescriptionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membuat draf resep HD | `HemodialysisPrescription : Create` | `CreateHmdPrescriptionRequest` | `ApiResponse<HmdPrescriptionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Menyunting resep yang masih berstatus draf | `HemodialysisPrescription : Update` | `UpdateHmdPrescriptionRequest` | `ApiResponse<HmdPrescriptionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/activate` | Mengaktifkan resep; resep aktif sebelumnya otomatis digantikan | `HemodialysisPrescription : Activate` | — | `ApiResponse<HmdPrescriptionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan resep draf atau aktif | `HemodialysisPrescription : Cancel` | `CancelHmdPrescriptionRequest` | `ApiResponse<HmdPrescriptionResponse>` | **Rencana (belum tersedia)** |

- **409** — resep aktif lain masih ada dan penggantian tidak diizinkan pada konteks itu.
- **422** — resep tidak dapat diaktifkan karena akses vaskular yang dirujuk sudah tidak layak.

---

## Health Services / Hemodialysis Management / Hemodialysis Schedule

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/worklist` | Daftar kerja unit pada satu tanggal dan shift | `HemodialysisSchedule : Read` | `HmdWorklistQuery` | `ApiResponse<PagedResult<HmdWorklistItemResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menjadwalkan sesi baru | `HemodialysisSchedule : Create` | `CreateHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/schedule` | Mengubah tanggal, shift, mesin, station, atau petugas | `HemodialysisSchedule : Update` | `RescheduleHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/cancel` | Membatalkan sesi sebelum dimulai | `HemodialysisSchedule : Cancel` | `CancelHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/staff-assignments` | Daftar petugas pada satu sesi beserta status kewenangannya | `HemodialysisSchedule : Read` | — | `ApiResponse<List<HmdStaffAssignmentResponse>>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/staff-assignments` | Menetapkan petugas pada satu sesi | `HemodialysisSchedule : Update` | `AssignHmdStaffRequest` | `ApiResponse<List<HmdStaffAssignmentResponse>>` | **Rencana (belum tersedia)** |

- **409** — mesin, station, atau pasien sudah terpakai pada rentang waktu yang bertumpang tindih.
- **422** — mesin tidak boleh dipakai karena statusnya bukan siap, atau tidak memenuhi kebutuhan
  isolasi pasien.

---

## Health Services / Hemodialysis Management / Hemodialysis Session

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-sessions`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{id}` | Konteks lengkap satu sesi | `HemodialysisSession : Read` | — | `ApiResponse<HmdSessionDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/check-in` | Menandai pasien sudah datang | `HemodialysisSession : CheckIn` | `CheckInHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/checklist` | Isi checklist Pra-HD beserta hasilnya | `HemodialysisSession : Read` | — | `ApiResponse<List<HmdSessionChecklistResponse>>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/checklist` | Menyimpan hasil pemeriksaan checklist Pra-HD | `HemodialysisSession : Update` | `SaveHmdChecklistRequest` | `ApiResponse<List<HmdSessionChecklistResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/checklist/{itemId}/override` | Melewati satu butir checklist dengan alasan tertulis | `HemodialysisSession : OverrideChecklist` | `OverrideHmdChecklistRequest` | `ApiResponse<HmdSessionChecklistResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/pre-hd` | Menyimpan penilaian Pra-HD termasuk berat badan dan tanda vital | `HemodialysisSession : Update` | `SaveHmdPreAssessmentRequest` | `ApiResponse<HmdSessionAssessmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/ready` | Menyatakan sesi siap dimulai | `HemodialysisSession : DeclareReady` | — | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/hold` | Menahan sesi karena ada yang belum siap | `HemodialysisSession : Hold` | `HoldHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/start` | Memulai cuci darah | `HemodialysisSession : Start` | `StartHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/observations` | Seluruh riwayat pemantauan pada satu sesi | `HemodialysisObservation : Read` | `HmdObservationQuery` | `ApiResponse<PagedResult<HmdObservationResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/observations` | Mencatat satu baris pemantauan | `HemodialysisObservation : Create` | `CreateHmdObservationRequest` | `ApiResponse<HmdObservationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/medications` | Mencatat pemberian obat | `HemodialysisMedication : Administer` | `CreateHmdMedicationRequest` | `ApiResponse<HmdMedicationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/complications` | Mencatat komplikasi beserta penanganannya | `HemodialysisComplication : Create` | `CreateHmdComplicationRequest` | `ApiResponse<HmdComplicationResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/stop` | Menghentikan sesi sebelum selesai | `HemodialysisSession : Stop` | `StopHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/complete` | Menyatakan cuci darah selesai secara fisik | `HemodialysisSession : Complete` | `CompleteHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/post-hd` | Menyimpan penilaian Pasca-HD dan disposisi pasien | `HemodialysisSession : Update` | `SaveHmdPostAssessmentRequest` | `ApiResponse<HmdSessionAssessmentResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/submit-documentation` | Perawat menyatakan dokumentasi selesai | `HemodialysisSession : SubmitDocumentation` | — | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/finalize` | Dokter mengesahkan dan mengunci catatan sesi | `HemodialysisRecord : Finalize` | `FinalizeHmdSessionRequest` | `ApiResponse<HmdSessionResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/billing-handoff` | Status penyerahan tindakan ke Billing | `HemodialysisSession : Read` | — | `ApiResponse<HmdBillingHandoffResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/billing-handoff/retry` | Mengulang penyerahan yang gagal | `HemodialysisSession : RetryBillingHandoff` | — | `ApiResponse<HmdBillingHandoffResponse>` | **Rencana (belum tersedia)** |

Arti kode status yang khas bagian ini:

- **409** — sesi sudah dimulai atau sudah dikunci oleh orang lain, sehingga permintaan yang sama
  ditolak. Inilah yang membuat tombol Mulai yang ditekan dua kali tetap menghasilkan satu sesi.
- **422** — sesi belum boleh berpindah status karena syaratnya belum terpenuhi, misalnya masih
  ada butir checklist wajib yang belum terpenuhi, atau dokter penanggung jawab belum ditetapkan.
- **423** — catatan sesi sudah terkunci. Perubahan langsung ditolak; koreksi memakai *addendum*
  lewat endpoint Rekam Medis.

---

## Health Services / Hemodialysis Management / Hemodialysis Unit Readiness

Base URL: `api/v1/health-services/hemodialysis-management/hemodialysis-unit-readiness`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Status kesiapan unit per tanggal dan shift | `HemodialysisUnitReadiness : Read` | `HmdReadinessQuery` | `ApiResponse<PagedResult<HmdUnitReadinessResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian satu pemeriksaan kesiapan beserta butirnya | `HemodialysisUnitReadiness : Read` | — | `ApiResponse<HmdUnitReadinessDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Membentuk lembar pemeriksaan kesiapan baru | `HemodialysisUnitReadiness : Create` | `CreateHmdUnitReadinessRequest` | `ApiResponse<HmdUnitReadinessResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}/items` | Menyimpan hasil pemeriksaan tiap butir | `HemodialysisUnitReadiness : Update` | `SaveHmdReadinessItemsRequest` | `ApiResponse<HmdUnitReadinessDetailResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/declare-ready` | Menyatakan unit siap melayani | `HemodialysisUnitReadiness : DeclareReady` | — | `ApiResponse<HmdUnitReadinessResponse>` | **Rencana (belum tersedia)** |
| `POST` | `/{id}/declare-not-ready` | Menyatakan unit tidak siap beserta alasannya | `HemodialysisUnitReadiness : DeclareNotReady` | `DeclareNotReadyRequest` | `ApiResponse<HmdUnitReadinessResponse>` | **Rencana (belum tersedia)** |

- **422** — unit tidak dapat dinyatakan siap karena masih ada butir wajib yang belum terpenuhi.

---

## Health Services / Hemodialysis Management / Master Data / Hemodialysis Machine

Base URL: `api/v1/health-services/hemodialysis-management/master-data/hemodialysis-machines`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar mesin beserta statusnya | `HemodialysisMachine : Read` | `HmdMachinePagedQuery` | `ApiResponse<PagedResult<HmdMachineResponse>>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}` | Rincian satu mesin | `HemodialysisMachine : Read` | — | `ApiResponse<HmdMachineResponse>` | **Rencana (belum tersedia)** |
| `GET` | `/{id}/status-history` | Riwayat perubahan status mesin | `HemodialysisMachine : Read` | — | `ApiResponse<PagedResult<HmdMachineStatusHistoryResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mendaftarkan mesin baru | `HemodialysisMachine : Create` | `CreateHmdMachineRequest` | `ApiResponse<HmdMachineResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Memperbarui data mesin | `HemodialysisMachine : Update` | `UpdateHmdMachineRequest` | `ApiResponse<HmdMachineResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengubah status laik pakai mesin beserta alasannya | `HemodialysisMachine : ChangeStatus` | `ChangeHmdMachineStatusRequest` | `ApiResponse<HmdMachineResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan mesin | `HemodialysisMachine : Delete` | — | `ApiResponse<bool>` | **Rencana (belum tersedia)** |

- **409** — mesin tidak dapat diubah statusnya menjadi tidak siap karena sedang dipakai sesi yang
  berjalan.

---

## Health Services / Hemodialysis Management / Master Data / Hemodialysis Station

Base URL: `api/v1/health-services/hemodialysis-management/master-data/hemodialysis-stations`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar station | `HemodialysisStation : Read` | `HmdStationPagedQuery` | `ApiResponse<PagedResult<HmdStationResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Mendaftarkan station baru | `HemodialysisStation : Create` | `CreateHmdStationRequest` | `ApiResponse<HmdStationResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Memperbarui data station | `HemodialysisStation : Update` | `UpdateHmdStationRequest` | `ApiResponse<HmdStationResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/status` | Mengubah status station | `HemodialysisStation : ChangeStatus` | `ChangeHmdStationStatusRequest` | `ApiResponse<HmdStationResponse>` | **Rencana (belum tersedia)** |
| `DELETE` | `/{id}` | Menonaktifkan station | `HemodialysisStation : Delete` | — | `ApiResponse<bool>` | **Rencana (belum tersedia)** |

---

## Health Services / Hemodialysis Management / Master Data / Hemodialysis Setting

Base URL: `api/v1/health-services/hemodialysis-management/master-data/hemodialysis-settings`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/{serviceUnitId}` | Membaca pengaturan unit HD | `HemodialysisSetting : Read` | — | `ApiResponse<HmdSettingResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{serviceUnitId}` | Memperbarui pengaturan unit HD | `HemodialysisSetting : Update` | `UpdateHmdSettingRequest` | `ApiResponse<HmdSettingResponse>` | **Rencana (belum tersedia)** |

Pengaturan yang disimpan di sini — batas pasien per perawat, masa berlaku hasil pemeriksaan air,
dan sakelar penegakan kewenangan — **tidak boleh** ditanam di kode maupun di frontend.

---

## Health Services / Hemodialysis Management / Master Data / Hemodialysis Checklist Item

Base URL: `api/v1/health-services/hemodialysis-management/master-data/hemodialysis-checklist-items`

| Method | Path | Kegunaan | Hak akses | Request | Response | Status |
|---|---|---|---|---|---|---|
| `GET` | `/` | Daftar butir checklist Pra-HD | `HemodialysisChecklistItem : Read` | — | `ApiResponse<List<HmdChecklistItemResponse>>` | **Rencana (belum tersedia)** |
| `POST` | `/` | Menambah butir checklist | `HemodialysisChecklistItem : Create` | `CreateHmdChecklistItemRequest` | `ApiResponse<HmdChecklistItemResponse>` | **Rencana (belum tersedia)** |
| `PUT` | `/{id}` | Memperbarui butir checklist | `HemodialysisChecklistItem : Update` | `UpdateHmdChecklistItemRequest` | `ApiResponse<HmdChecklistItemResponse>` | **Rencana (belum tersedia)** |
| `PATCH` | `/{id}/overridable` | Menetapkan apakah butir ini boleh dilewati dokter | `HemodialysisChecklistItem : SetOverridable` | `SetOverridableRequest` | `ApiResponse<HmdChecklistItemResponse>` | **Rencana (belum tersedia)** |

Endpoint terakhir adalah yang membuat `HMD-ASM-001` dapat dicabut **tanpa mengubah kode**.
Selama badan klinis belum menetapkan, seluruh butir bernilai tidak boleh dilewati, dan hak akses
`HemodialysisChecklistItem : SetOverridable` hanya dipegang pemegang akun tata kelola klinis.

---

## Endpoint modul lain yang dipakai apa adanya

Ketiga grup berikut **sudah ada** dan tidak diubah. Hemodialisa memanggilnya sebagai pemakai.

### Health Services / Medical Record Management / Clinical Document Integrity

Base URL: `api/v1/health-services/medical-record-management/clinical-document-integrities`

| Method | Path | Kegunaan bagi Hemodialisa | Status |
|---|---|---|---|
| `POST` | `/by-document/{documentKind}/{documentId}/sign` | Menandatangani dan mengunci catatan sesi HD | **Sudah ada** |
| `GET` | `/by-document/{documentKind}/{documentId}` | Membaca status keutuhan catatan sesi | **Sudah ada** |

`{documentKind}` diisi `HemodialysisSession`. Agar endpoint ini benar-benar menegakkan
penguncian, jenis dokumen itu wajib didaftarkan di **dua** tempat — lihat
`02-backend-architecture.md` bagian 7.

### Health Services / Medical Record Management / Clinical Note Addendum

Base URL: `api/v1/health-services/medical-record-management/clinical-note-addendums`

| Method | Path | Kegunaan bagi Hemodialisa | Status |
|---|---|---|---|
| `GET` | `/by-document/{documentKind}/{documentId}` | Daftar koreksi pada catatan sesi HD | **Sudah ada** |
| `POST` | `/by-document/{documentKind}/{documentId}` | Membuat koreksi pada catatan sesi yang sudah final | **Sudah ada** |
| `GET` | `/authority/{documentKind}/{documentId}` | Memeriksa apakah pengguna berwenang mengoreksi | **Sudah ada** |

### Health Services / Laboratory Management / Lab Examination

Base URL: `api/v1/health-services/laboratory-management/lab-examinations`

| Method | Path | Kegunaan bagi Hemodialisa | Status |
|---|---|---|---|
| `GET` | `/by-order/{labOrderId}` | Membaca hasil serologi yang rujukannya sudah diketahui | **Sudah ada** |
| `GET` | `/by-specimen/{specimenId}` | Sama, lewat spesimen | **Sudah ada** |
| `GET` | `/by-patient/{patientId}` | Hasil terakhir per jenis pemeriksaan untuk seorang pasien | **Rencana (belum tersedia)** — milik Laboratorium, `HMD-DEP-001` |

Baris terakhir **bukan** pekerjaan Hemodialisa. Selama belum tersedia, petugas HD mencatat
rujukan hasil secara manual (`RCG-P-08`).
