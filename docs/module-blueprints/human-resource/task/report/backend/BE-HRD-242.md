# Laporan Penyelesaian Task: BE-HRD-242 Master Rencana Kerja Tahunan Diklat (TNA) & Alur Persetujuan Direksi

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-242`
* **Nama Task**: Master Rencana Kerja Tahunan Diklat (TNA) & Alur Persetujuan Direksi (`AnnualTrainingPlanService` & Controller)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/AnnualTrainingPlanDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/IAnnualTrainingPlanService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/AnnualTrainingPlanService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/AnnualTrainingPlansController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Tata Kelola Perencanaan Pembelajaran Rumah Sakit (TNA)
Perencanaan program pendidikan dan pelatihan (*Learning & Development*) rumah sakit berbasis kebutuhan nyata (*Training Needs Analysis* - TNA) merupakan salah satu instrumen utama pemenuhan standar mutu, akreditasi rumah sakit (STARKES KPS 8), dan optimalisasi serapan anggaran tahunan. Dokumen Rencana Program Pelatihan Tahunan RS (*Annual Training Master Plan*) memerlukan tata kelola berjenjang dari penyusunan draf oleh tim Diklat, pengajuan ke Direksi, hingga pengesahan resmi yang membuka pagu anggaran dan kuota peserta (`VR-TRN-13`, `AT-HRD-TRN-08`).

### 1. Invarian Kelengkapan Usulan Program Diklat TNA (`VR-TRN-13`, `AT-HRD-TRN-08`)
Setiap dokumen usulan rencana pelatihan tahunan wajib memenuhi syarat kelengkapan:
1. **Target Kompetensi & Tujuan Pembelajaran (`Objectives`)**: Wajib diisi secara komprehensif, mencakup sasaran kompetensi teknis/klinis atau manajerial yang hendak dicapai.
2. **Estimasi Anggaran (`EstimatedCost > 0`)**: Estimasi total biaya pelatihan wajib bernilai positif dan realistis untuk diverifikasi terhadap pagu anggaran departemen.
3. **Target Jumlah Peserta (`PlannedParticipantCount >= 1`)**: Minimal ditargetkan untuk 1 orang staf.
4. **Validitas Rentang Waktu**: Tanggal akhir pelaksanaan (`PlannedEndDate`) tidak boleh mendahului tanggal mulai (`PlannedStartDate`).
5. **Nomor Seri Dokumen Baku**: Sistem menghasilkan nomor identitas dokumen rencana kerja dengan format:
   $$\text{TNA}-\{\text{Year}\}-\{\text{Sequence:D4}\} \quad (\text{Contoh: } \text{TNA-2026-0001})$$

### 2. Siklus Hidup & Alur Pengesahan Berjenjang Direksi
Siklus hidup status rencana diklat (`PlanStatus`) dikendalikan secara deterministik:
- **`Draft`**: Tim Diklat menyusun proposal program dan rincian alokasi.
- **`Submitted`**: Pengajuan resmi ke Direktur SDM/Pimpinan RS (`POST .../submit`).
- **`Approved`**: Pengesahan resmi oleh Direksi (`POST .../approve`), mencatat `ApprovedBudget`, `ApprovedAt`, `ApprovedByUserId`, serta membuka pagu anggaran pelatihan untuk batch kelas.
- **Pengembalian Revisi (`POST .../reject`)**: Bila terdapat penyesuaian materi/biaya, Direksi mengembalikan proposal ke status `Draft` disertai catatan justifikasi revisi tanpa menghapus draf usulan.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Training Enrollment & Budget Governance")]`
**Base Route**: `/api/v1/corporate/hrd/learning/plans/annual`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/plans/annual` | Mengambil seluruh daftar program rencana tahunan diklat (TNA) dengan filter tahun, status, dan departemen. | `[Authorize]` Staff / Diklat | Filter: `planYear`, `planStatus`, `departmentId` | `200 OK` (`ApiResponse<List<AnnualTrainingPlanDto>>`) |
| `GET` | `/api/v1/corporate/hrd/learning/plans/annual/{id}` | Mengambil detail rencana kerja tahunan diklat berdasarkan ID. | `[Authorize]` Staff / Diklat | - | `200 OK` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/plans/annual` | Menyusun usulan program pelatihan tahunan baru dengan validasi ketat kelengkapan TNA (`VR-TRN-13`). | `[Authorize]` Diklat / HRD | `CreateAnnualTrainingPlanRequestDto` | `201 Created` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `PUT` | `/api/v1/corporate/hrd/learning/plans/annual/{id}` | Memperbarui parameter usulan program tahunan (hanya diizinkan pada status Draft). | `[Authorize]` Diklat / HRD | `UpdateAnnualTrainingPlanRequestDto` | `200 OK` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/plans/annual/{id}/submit` | Mengajukan proposal rencana kerja tahunan ke Direktur SDM / Direksi RS. | `[Authorize]` Diklat Manager | `SubmitTrainingPlanRequestDto` | `200 OK` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/plans/annual/{id}/approve` | Pengesahan resmi rencana program tahunan oleh Direksi dan pengesahan pagu anggaran. | `[Authorize]` Direksi / Direktur SDM | `ApproveTrainingPlanRequestDto` | `200 OK` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/plans/annual/{id}/reject` | Mengembalikan proposal rencana tahunan ke status Draft dengan alasan revisi tertulis. | `[Authorize]` Direksi / Direktur SDM | `RejectTrainingPlanRequestDto` | `200 OK` (`ApiResponse<AnnualTrainingPlanDto>`) |
| `DELETE` | `/api/v1/corporate/hrd/learning/plans/annual/{id}` | Menghapus usulan rencana diklat (hanya berlaku untuk status Draft). | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<bool>`) |

---

## 4. Contoh Konkret Request & Response API

### Skenario 1: Penyusunan Program TNA Tahunan (`201 Created`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/plans/annual`
* **Request Body**:
```json
{
  "trainingPlanName": "Program Pelatihan Kegawatdaruratan Maternal & Neonatal (PONEK) 2026",
  "planYear": 2026,
  "plannedStartDate": "2026-03-01T00:00:00Z",
  "plannedEndDate": "2026-11-30T00:00:00Z",
  "deliveryMode": "InHouseWorkshop",
  "providerName": "Tim PONEK & Diklat Internal RS",
  "targetAudience": "Dokter Umum IGD, Bidan VK Bersalin, dan Perawat Perinatologi",
  "isMandatory": true,
  "isExternalTraining": false,
  "plannedParticipantCount": 45,
  "estimatedCost": 75000000.0,
  "currencyCode": "IDR",
  "objectives": "Meningkatkan kesiapsiagaan tim klinis dalam penatalaksanaan komplikasi obstetri dan asfiksia neonatus sesuai standar PONEK 24 Jam.",
  "departmentId": "b1b2b3b4-4444-5555-6666-777788889999"
}
```
* **Response Body (`201 Created`)**:
```json
{
  "code": 201,
  "status": "Created",
  "message": "Rencana program tahunan diklat (TNA) berhasil didaftarkan.",
  "data": {
    "id": "f1f2f3f4-1111-2222-3333-444455556666",
    "trainingPlanNumber": "TNA-2026-0001",
    "trainingPlanName": "Program Pelatihan Kegawatdaruratan Maternal & Neonatal (PONEK) 2026",
    "planYear": 2026,
    "planType": "Annual",
    "planStatus": "Draft",
    "plannedStartDate": "2026-03-01T00:00:00Z",
    "plannedEndDate": "2026-11-30T00:00:00Z",
    "deliveryMode": "InHouseWorkshop",
    "plannedParticipantCount": 45,
    "approvedParticipantCount": 0,
    "estimatedCost": 75000000.0,
    "approvedBudget": 0.0,
    "currencyCode": "IDR",
    "objectives": "Meningkatkan kesiapsiagaan tim klinis dalam penatalaksanaan komplikasi obstetri dan asfiksia neonatus sesuai standar PONEK 24 Jam.",
    "departmentName": "Instalasi Kebidanan & Kandungan",
    "createdAt": "2026-11-09T15:35:00Z",
    "isActive": true
  }
}
```

### Skenario 2: Pengesahan Resmi oleh Direksi RS (`200 OK`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/plans/annual/f1f2f3f4-1111-2222-3333-444455556666/approve`
* **Request Body**:
```json
{
  "approvedBudget": 75000000.0,
  "approvedParticipantCount": 45,
  "approvalNotes": "Disetujui penuh untuk mendukung pencapaian indikator mutu nasional PONEK RS 2026."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "code": 200,
  "status": "OK",
  "message": "Rencana program tahunan diklat resmi DISAHKAN oleh Direksi dan pagu anggaran tahunan dibuka.",
  "data": {
    "id": "f1f2f3f4-1111-2222-3333-444455556666",
    "trainingPlanNumber": "TNA-2026-0001",
    "trainingPlanName": "Program Pelatihan Kegawatdaruratan Maternal & Neonatal (PONEK) 2026",
    "planStatus": "Approved",
    "plannedParticipantCount": 45,
    "approvedParticipantCount": 45,
    "estimatedCost": 75000000.0,
    "approvedBudget": 75000000.0,
    "approvedAt": "2026-11-09T15:36:00Z",
    "notes": "Persetujuan Direksi: Disetujui penuh untuk mendukung pencapaian indikator mutu nasional PONEK RS 2026."
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Penegakan Kelengkapan TNA (`VR-TRN-13`, `AT-HRD-TRN-08`)**: ✅ Lulus. Pengajuan divalidasi ketat; tanggal terbalik, estimasi biaya 0, peserta 0, atau objectives kosong ditolak seketika dengan status HTTP 400.
2. **Penomoran Otomatis Berurutan**: ✅ Lulus. Pola `TNA-{Year}-{Sequence:D4}` terkonfirmasi deterministik dan konsisten.
3. **Workflow Pengesahan Direksi**: ✅ Lulus. Transisi status dari `Draft` -> `Submitted` -> `Approved` tercatat lengkap dengan stempel waktu, user approver, dan pagu anggaran yang disahkan.
