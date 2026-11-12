# Laporan Penyelesaian Task: BE-HRD-244 Pendaftaran Diklat Mandiri via ESS & Alur Persetujuan Berjenjang Karu/Diklat

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-244`
* **Nama Task**: Pendaftaran Diklat Mandiri via ESS & Alur Persetujuan Berjenjang Karu/Diklat (`TrainingEnrollmentService` & Controller)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingEnrollmentDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingEnrollmentService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingEnrollmentService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingEnrollmentsController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Alur Pendaftaran Mandiri Rumah Sakit
Dalam tata kelola kepegawaian rumah sakit modern, partisipasi pelatihan tidak hanya berasal dari penugasan langsung Diklat (misal via radar kedaluwarsa STARKES), melainkan juga melalui pengajuan mandiri oleh staf melalui modul *Employee Self-Service* (ESS) untuk program peningkatan kompetensi klinis atau sertifikasi keahlian khusus. Pendaftaran mandiri memerlukan alur persetujuan berjenjang guna memastikan ketersediaan pengganti shift dinas oleh Kepala Ruangan (*Supervisor*) dan validasi kesesuaian anggaran oleh Tim Diklat (`AT-HRD-TRN-10`).

### 1. Invarian Pendaftaran & Pengendalian Kuota (`VR-TRN-07`)
1. **Validasi Kuota Real-Time**: Ketika staf memilih sesi pelatihan tertentu, sistem menghitung kuota terisi. Bila kuota kelas telah tercapai, pengajuan ditolak seketika dengan status `409 Conflict`.
2. **Pencegahan Registrasi Duplikat**: Pegawai yang telah memiliki permohonan berstatus aktif (`PendingSupervisor`, `PendingDiklat`, `Approved`) dilarang mendaftar ulang pada sesi yang sama.
3. **Format Nomor Registrasi Baku**:
   $$\text{ENR-TRN}-\{\text{Year}\}-\{\text{Sequence:D5}\} \quad (\text{Contoh: } \text{ENR-TRN-2026-00001})$$

### 2. Alur Persetujuan Berjenjang (*Multi-Level Workflow*)
Proses persetujuan dijalankan dalam 2 gerbang otorisasi bertahap:
1. **Tahap 1 — Kepala Ruangan / Supervisor (`PendingSupervisor` $\rightarrow$ `PendingDiklat`)**:
   - Kepala Ruangan memverifikasi apakah pelepasan staf untuk mengikuti diklat tidak mengganggu keselamatan pelayanan pasien dan rasio perawat-pasien di ruangan rawat inap/IGD.
   - Jika disetujui, status berpindah ke `PendingDiklat`.
2. **Tahap 2 — Unit Diklat Rumah Sakit (`PendingDiklat` $\rightarrow$ `Approved`)**:
   - Tim Diklat memvalidasi kualifikasi peserta, kuota peserta unit, serta ketersediaan pagu anggaran departemen.
   - Pengesahan mengubah status menjadi `Approved` dan mencatat peserta ke dalam daftar manifes resmi kelas.
   - Bila ditolak pada tahap mana pun, status pendaftaran berubah menjadi `Rejected` disertai catatan alasan tertulis.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Training Enrollment & Budget Governance")]`
**Base Route**: `/api/v1/corporate/hrd/learning/enrollments`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/enrollments` | Mengambil seluruh permohonan pendaftaran diklat dengan filter sesi, staf, dan status. | `[Authorize]` Pegawai / Diklat | Filter: `sessionId`, `workforceProfileId`, `status` | `200 OK` (`ApiResponse<List<TrainingEnrollmentDto>>`) |
| `GET` | `/api/v1/corporate/hrd/learning/enrollments/{id}` | Mengambil detail spesifikasi permohonan pendaftaran berdasarkan ID. | `[Authorize]` Pegawai / Diklat | - | `200 OK` (`ApiResponse<TrainingEnrollmentDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/enrollments/ess-submit` | Pengajuan pendaftaran diklat internal/eksternal mandiri oleh staf via ESS beserta justifikasi dinas (`AT-HRD-TRN-10`). | `[Authorize]` Pegawai ESS | `SubmitEnrollmentRequestDto` | `201 Created` (`ApiResponse<TrainingEnrollmentDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/enrollments/{id}/approval-workflow` | Memproses keputusan persetujuan berjenjang (Kepala Ruangan & Diklat RS) secara terstruktur. | `[Authorize]` Karu / Diklat | `ProcessEnrollmentApprovalDto` | `200 OK` (`ApiResponse<TrainingEnrollmentDto>`) |
| `DELETE` | `/api/v1/corporate/hrd/learning/enrollments/{id}` | Membatalkan permohonan pendaftaran diklat sebelum disetujui resmi. | `[Authorize]` Pegawai / Diklat | - | `200 OK` (`ApiResponse<bool>`) |

---

## 4. Contoh Konkret Request & Response API

### Skenario 1: Pengajuan Pendaftaran Mandiri via ESS (`201 Created`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/enrollments/ess-submit`
* **Request Body**:
```json
{
  "trainingPlanId": "f1f2f3f4-1111-2222-3333-444455556666",
  "trainingSessionId": "e1e2e3e4-aaaa-bbbb-cccc-111122223333",
  "workforceProfileId": "w1w2w3w4-5555-6666-7777-888899990000",
  "enrollmentType": "Internal",
  "developmentNeed": "Penyegaran kompetensi resusitasi jantung paru BHD untuk persiapan akreditasi STARKES KPS 8.",
  "requestReason": "Sertifikat BHD akan habis masa berlaku dalam tempo 30 hari ke depan.",
  "requestedCost": 0.0,
  "currencyCode": "IDR"
}
```
* **Response Body (`201 Created`)**:
```json
{
  "success": true,
  "message": "Permohonan pendaftaran pelatihan via ESS berhasil diajukan dan diteruskan ke atasan langsung.",
  "data": {
    "id": "a1a2a3a4-9999-8888-7777-666655554444",
    "requestNumber": "ENR-TRN-2026-00001",
    "requestDate": "2026-11-09T15:52:00Z",
    "trainingPlanId": "f1f2f3f4-1111-2222-3333-444455556666",
    "sessionCode": "SES-2026-B1",
    "sessionName": "Workshop Simulasi Resusitasi BHD Gelombang 1",
    "workforceProfileId": "w1w2w3w4-5555-6666-7777-888899990000",
    "staffName": "Ns. Ratna Wulandari, S.Kep",
    "enrollmentType": "Internal",
    "requestStatus": "PendingSupervisor",
    "developmentNeed": "Penyegaran kompetensi resusitasi jantung paru BHD untuk persiapan akreditasi STARKES KPS 8.",
    "requestReason": "Sertifikat BHD akan habis masa berlaku dalam tempo 30 hari ke depan.",
    "submittedAt": "2026-11-09T15:52:00Z",
    "isActive": true
  }
}
```

### Skenario 2: Persetujuan Kepala Ruangan (`200 OK`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/enrollments/a1a2a3a4-9999-8888-7777-666655554444/approval-workflow`
* **Request Body**:
```json
{
  "stage": "Supervisor",
  "action": "Approved",
  "actionNote": "Disetujui. Pengaturan shift jaga pengganti di IGD telah diatur."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "success": true,
  "message": "Persetujuan pendaftaran tahap Supervisor berhasil diproses dengan status PendingDiklat.",
  "data": {
    "id": "a1a2a3a4-9999-8888-7777-666655554444",
    "requestNumber": "ENR-TRN-2026-00001",
    "requestStatus": "PendingDiklat",
    "managerActionNote": "Disetujui. Pengaturan shift jaga pengganti di IGD telah diatur.",
    "managerActionAt": "2026-11-09T15:53:00Z"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Pengajuan Mandiri ESS (`AT-HRD-TRN-10`)**: ✅ Lulus. Permohonan tercatat lengkap dengan kebutuhan kompetensi, alasan dinas, stempel waktu, dan nomor registrasi `ENR-TRN-...`.
2. **Persetujuan Berjenjang Multilevel**: ✅ Lulus. Transisi status dari `PendingSupervisor` $\rightarrow$ `PendingDiklat` $\rightarrow$ `Approved` terbukti deterministik dan aman.
3. **Pencegahan Pendaftaran Kuota Penuh (`VR-TRN-07`)**: ✅ Lulus. Pendaftaran tertolak seketika jika kuota kelas terpenuhi.
