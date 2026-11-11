# Laporan Penyelesaian Task: BE-HRD-245 Kontrol Pagu Anggaran Diklat Unit & Pemindahan Saldo Komitmen Otomatis

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-245`
* **Nama Task**: Kontrol Pagu Anggaran Diklat Unit & Pemindahan Saldo Komitmen Otomatis (`TrainingBudgetControlService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingBudgetDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingBudgetControlService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingBudgetControlService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingBudgetsController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Tata Kelola Keuangan Anggaran Pembelajaran Rumah Sakit
Dalam penyelenggaraan diklat rumah sakit (terutama pelatihan spesialisasi eksternal bernilai tinggi seperti fellowship, kursus laparoskopi, atau sertifikasi resusitasi lanjutan), biaya pendaftaran dan akomodasi harus dikendalikan secara ketat agar tidak melampaui batas pagu anggaran tahunan yang telah ditetapkan untuk masing-masing unit kerja/departemen (`VR-TRN-14`, `VR-TRN-15`, `AT-HRD-TRN-11`).

### 1. Invarian Anti-Defisit Pagu Anggaran Unit (`VR-TRN-14`, `AT-HRD-TRN-11`)
Sistem memastikan ketersediaan dana sebelum pendaftaran disahkan:
$$\text{ProjectedRemaining} = \text{RemainingAmount} - \text{RequestedCost} \ge 0$$
Apabila estimasi biaya pendaftaran yang diajukan melebihi sisa pagu anggaran departemen ($\text{RemainingAmount} < \text{RequestedCost}$), sistem menolak proses pendaftaran dengan mengembalikan kode status **`422 Unprocessable Entity`** dan pesan detail bahwa sisa pagu anggaran unit tidak mencukupi.

### 2. Mekanisme Pemindahan Saldo Komitmen Otomatis (`VR-TRN-15`)
Untuk mencegah *over-budgeting* akibat persetujuan beberapa pendaftaran pelatihan secara simultan:
1. Saat pendaftaran diklat disetujui pada tahap Diklat, sistem seketika memindahkan saldo dari `RemainingAmount` ke `CommittedAmount`:
   $$\text{CommittedAmount} \leftarrow \text{CommittedAmount} + \text{CommitmentAmount}$$
   $$\text{RemainingAmount} \leftarrow \text{RemainingAmount} - \text{CommitmentAmount}$$
2. Saldo komitmen ini dikunci dan tidak dapat digunakan oleh pendaftar lain di unit yang sama.
3. Pasca pelatihan selesai dan bukti pembayaran diverifikasi oleh Bagian Keuangan, sistem mencairkan komitmen menjadi realisasi riil (`UsedAmount`) melalui endpoint `POST .../budgets/realize`.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Training Enrollment & Budget Governance")]`
**Base Route**: `/api/v1/corporate/hrd/learning/budgets`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/budgets/department/{departmentId}/status` | Memeriksa status alokasi pagu, komitmen berjalan, realisasi riil, dan sisa pagu anggaran diklat unit kerja. | `[Authorize]` Staff / Diklat | Filter: `fiscalYear` | `200 OK` (`ApiResponse<TrainingBudgetSummaryDto>`) |
| `GET` | `/api/v1/corporate/hrd/learning/budgets/plan/{trainingPlanId}` | Menampilkan rincian alokasi anggaran pada suatu dokumen program tahunan TNA. | `[Authorize]` Diklat / Keuangan | - | `200 OK` (`ApiResponse<List<TrainingBudgetSummaryDto>>`) |
| `POST` | `/api/v1/corporate/hrd/learning/budgets/allocate` | Mengalokasikan plafon pagu anggaran diklat tahunan untuk suatu departemen/unit kerja. | `[Authorize]` Diklat Manager | `AllocateDepartmentBudgetRequestDto` | `201 Created` (`ApiResponse<TrainingBudgetSummaryDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/budgets/commit` | Memvalidasi kecukupan saldo pagu (`VR-TRN-14`) dan memindahkan saldo komitmen berjalan secara otomatis (`VR-TRN-15`). | `[Authorize]` Diklat / Sistem | `CommitTrainingBudgetRequestDto` | `200 OK`, `422 Unprocessable Entity` |
| `POST` | `/api/v1/corporate/hrd/learning/budgets/realize` | Mencatat realisasi pencairan biaya pelatihan aktual dan pelepasan saldo komitmen berjalan. | `[Authorize]` Keuangan / Diklat | `RealizeTrainingBudgetRequestDto` | `200 OK` (`ApiResponse<TrainingBudgetSummaryDto>`) |

---

## 4. Contoh Konkret Request & Response API

### Skenario 1: Penolakan Pendaftaran Akibat Saldo Pagu Defisit (`422 Unprocessable Entity`, `VR-TRN-14`)
* **Kasus Uji**: Sisa pagu anggaran IGD = Rp 5.000.000, staf mengajukan pendaftaran workshop senilai Rp 8.000.000.
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/budgets/commit`
* **Request Body**:
```json
{
  "trainingPlanId": "f1f2f3f4-1111-2222-3333-444455556666",
  "departmentId": "b1b2b3b4-4444-5555-6666-777788889999",
  "enrollmentRequestId": "a1a2a3a4-9999-8888-7777-666655554444",
  "commitmentAmount": 8000000.0,
  "commitmentReference": "Pendaftaran Workshop Ultrasound Darurat IGD"
}
```
* **Response Body (`422 Unprocessable Entity`)**:
```json
{
  "success": false,
  "message": "Sisa pagu anggaran diklat departemen tidak mencukupi untuk membiayai pelatihan yang diajukan (VR-TRN-14). Sisa pagu: Rp 5.000.000, dibutuhkan: Rp 8.000.000.",
  "data": {
    "isEligible": false,
    "allocatedAmount": 25000000.0,
    "committedAmount": 20000000.0,
    "remainingAmount": 5000000.0,
    "requestedAmount": 8000000.0,
    "projectedRemainingAmount": -3000000.0,
    "statusMessage": "Sisa pagu anggaran diklat departemen tidak mencukupi untuk membiayai pelatihan yang diajukan (VR-TRN-14). Sisa pagu: Rp 5.000.000, dibutuhkan: Rp 8.000.000."
  }
}
```

### Skenario 2: Pemindahan Saldo Komitmen Sukses (`200 OK`, `VR-TRN-15`)
* **Kasus Uji**: Sisa pagu anggaran Departemen Bedah = Rp 15.000.000, staf mengajukan pelatihan senilai Rp 6.000.000.
* **Response Body (`200 OK`)**:
```json
{
  "success": true,
  "message": "Komitmen pagu anggaran diklat berhasil dibukukan secara otomatis (VR-TRN-15).",
  "data": {
    "isEligible": true,
    "allocatedAmount": 30000000.0,
    "committedAmount": 6000000.0,
    "remainingAmount": 9000000.0,
    "requestedAmount": 6000000.0,
    "projectedRemainingAmount": 9000000.0,
    "statusMessage": "Pagu anggaran mencukupi dan komitmen biaya berhasil dialokasikan secara otomatis (VR-TRN-15)."
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Validasi Anti-Defisit Anggaran (`VR-TRN-14`, `AT-HRD-TRN-11`)**: ✅ Lulus. Pengajuan dengan biaya melebihi sisa pagu ditolak deterministik dengan kode status 422.
2. **Pemindahan Saldo Komitmen Otomatis (`VR-TRN-15`)**: ✅ Lulus. Saldo komitmen bertambah dan saldo sisa berkurang seketika secara atomik pada saat persetujuan dibukukan.
3. **Realisasi & Rekonsiliasi Keuangan**: ✅ Lulus. Pencairan aktual memperbarui saldo `UsedAmount` dan melepas saldo komitmen terkait.
