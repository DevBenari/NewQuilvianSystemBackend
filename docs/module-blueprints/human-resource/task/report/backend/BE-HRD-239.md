# Laporan Penyelesaian Task: BE-HRD-239 Konfigurasi Master Aturan Pelatihan Wajib Akreditasi STARKES KPS 8

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-239`
* **Nama Task**: Konfigurasi Master Aturan Pelatihan Wajib Akreditasi STARKES KPS 8 (`MandatoryTrainingRuleService` & Controller)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/MandatoryHospitalTrainingDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/IMandatoryTrainingRuleService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/MandatoryTrainingRuleService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/MandatoryHospitalTrainingController.cs`
  - `NewQuilvianSystemBackend/Program.cs`

---

## 2. Latar Belakang & Amanat Standar Akreditasi Rumah Sakit (STARKES KPS 8)
Standar Akreditasi Rumah Sakit Kementerian Kesehatan Republik Indonesia (STARKES KPS 8) mengamanatkan bahwa rumah sakit wajib menyelenggarakan pendidikan dan pelatihan berkelanjutan bagi seluruh pegawai dan tenaga kesehatan. Setiap staf rumah sakit tanpa terkecuali wajib memiliki sertifikat kompetensi pelatihan dasar yang masih berlaku (`HRD-DEC-158`).

### 1. Invarian Katalog 5 Pelatihan Dasar Wajib RS (`VR-TRN-01`, `AT-HRD-TRN-01`)
Sistem memvalidasi dan memelihara 5 kurikulum wajib rumah sakit:
1. **BHD (Bantuan Hidup Dasar / Basic Life Support)**: Keterampilan resusitasi jantung paru (RJP) bagi korban henti jantung mendadak di lingkungan rumah sakit.
2. **PPI (Pencegahan & Pengendalian Infeksi)**: Kewaspadaan standar, kepatuhan kebersihan tangan (6 langkah hand hygiene WHO), dan penggunaan APD yang benar.
3. **SKP (Sasaran Keselamatan Pasien)**: Penegakan 6 sasaran keselamatan pasien internasional (identifikasi pasien, komunikasi efektif, kewaspadaan obat high alert, kepastian tepat lokasi/prosedur bedah, penurunan risiko infeksi, dan pencegahan pasien jatuh).
4. **K3RS (Kesehatan & Keselamatan Kerja Rumah Sakit)**: Pencegahan kecelakaan kerja, simulasi evakuasi bencana, tanggap darurat api (*Code Red*), dan teknik penggunaan Alat Pemadam Api Ringan (APAR).
5. **Komunikasi Efektif (Metode SBAR)**: Standar serah terima pasien dan pelaporan perburukan kondisi klinis berbasis *Situation, Background, Assessment, Recommendation*.

### 2. Batas Masa Berlaku Maksimal 24 Bulan (`VR-TRN-02`, `AT-HRD-TRN-02`)
Sesuai regulasi mutu dan keselamatan pasien:
$$\text{ValidityPeriodMonths} \le 24\text{ Bulan}$$
Sistem menolak secara mutlak setiap upaya pendaftaran aturan pelatihan wajib dengan masa berlaku melebihi 24 bulan (2 tahun). Pengajuan dengan durasi > 24 bulan akan menghasilkan respons penolakan `400 Bad Request` dengan pesan error eksplisit bahwa standar akreditasi STARKES membatasi siklus resertifikasi maksimal 2 tahun.

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD Mandatory Hospital Training & Certification")]`
**Base Route**: `/api/v1/corporate/hrd/learning/mandatory-rules`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/learning/mandatory-rules` | Mengambil seluruh katalog master aturan pelatihan wajib STARKES dan regulasi RS. | `[Authorize]` Staff / Diklat | Filter: `category`, `isActive` | `200 OK` (`ApiResponse<List<MandatoryTrainingRuleDto>>`) |
| `GET` | `/api/v1/corporate/hrd/learning/mandatory-rules/{id}` | Mengambil detail spesifikasi aturan pelatihan wajib berdasarkan ID. | `[Authorize]` Staff / Diklat | - | `200 OK` (`ApiResponse<MandatoryTrainingRuleDto>`) |
| `POST` | `/api/v1/corporate/hrd/learning/mandatory-rules` | Mendaftarkan konfigurasi aturan pelatihan wajib baru dengan validasi batas 24 bulan. | `[Authorize]` Diklat / SuperAdmin | `CreateMandatoryTrainingRuleRequestDto` | `201 Created` (`ApiResponse<MandatoryTrainingRuleDto>`) |
| `PUT` | `/api/v1/corporate/hrd/learning/mandatory-rules/{id}` | Memperbarui parameter aturan pelatihan wajib, target profesi, dan jam pelajaran. | `[Authorize]` Diklat / SuperAdmin | `UpdateMandatoryTrainingRuleRequestDto` | `200 OK` (`ApiResponse<MandatoryTrainingRuleDto>`) |
| `DELETE` | `/api/v1/corporate/hrd/learning/mandatory-rules/{id}` | Menghapus (soft-delete) aturan pelatihan wajib dari sistem. | `[Authorize]` Diklat / SuperAdmin | - | `200 OK` (`ApiResponse<bool>`) |

---

## 4. Contoh Konkret Request & Response API

### Skenario 1: Registrasi Aturan BHD Sukses (`201 Created`)
* **Endpoint**: `POST /api/v1/corporate/hrd/learning/mandatory-rules`
* **Request Body**:
```json
{
  "trainingCode": "TRN-BHD-001",
  "trainingName": "Bantuan Hidup Dasar (BHD / BLS) STARKES KPS 8",
  "trainingCategory": "BasicHospitalMandatory",
  "description": "Pelatihan wajib resusitasi jantung paru bagi seluruh pegawai rumah sakit.",
  "validityPeriodMonths": 24,
  "targetProfessionCategory": "AllProfessions",
  "passingGradeTheory": 75.0,
  "passingGradePractice": 80.0,
  "effectiveHoursJpl": 8
}
```
* **Response Body (`201 Created`)**:
```json
{
  "code": 201,
  "status": "Created",
  "message": "Aturan pelatihan wajib berhasil didaftarkan.",
  "data": {
    "mandatoryRuleId": "a1b2c3d4-e5f6-7890-abcd-111122223333",
    "trainingCode": "TRN-BHD-001",
    "trainingName": "Bantuan Hidup Dasar (BHD / BLS) STARKES KPS 8",
    "trainingCategory": "BasicHospitalMandatory",
    "validityPeriodMonths": 24,
    "targetProfessionCategory": "AllProfessions",
    "passingGradeTheory": 75.0,
    "passingGradePractice": 80.0,
    "effectiveHoursJpl": 8,
    "isActive": true
  }
}
```

### Skenario 2: Penolakan Validasi Batas 24 Bulan (`400 Bad Request`, `VR-TRN-02`)
* **Request Body**:
```json
{
  "trainingCode": "TRN-BHD-INVALID",
  "trainingName": "Bantuan Hidup Dasar Ilegal",
  "trainingCategory": "BasicHospitalMandatory",
  "validityPeriodMonths": 36
}
```
* **Response Body (`400 Bad Request`)**:
```json
{
  "code": 400,
  "status": "Bad Request",
  "message": "Validasi gagal: Masa berlaku pelatihan wajib tidak boleh melebihi 24 bulan (2 tahun) sesuai standar akreditasi STARKES KPS 8 (VR-TRN-02)."
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Penyediaan Katalog 5 Pelatihan Wajib (`VR-TRN-01`, `AT-HRD-TRN-01`)**: ✅ Lulus. Seluruh pelatihan dasar STARKES (BHD, PPI, SKP, K3RS, Komunikasi SBAR) dan peminatan intensif terakomodasi secara terstruktur.
2. **Penegakan Batas Maksimal 24 Bulan (`VR-TRN-02`, `AT-HRD-TRN-02`)**: ✅ Lulus. Permintaan dengan masa berlaku > 24 bulan secara deterministik ditolak dengan status HTTP 400.
3. **Audit Jejak Perubahan**: ✅ Lulus. Setiap penambahan dan pengubahan aturan mencatat username pembuat/pengubah serta stempel waktu presisi.
