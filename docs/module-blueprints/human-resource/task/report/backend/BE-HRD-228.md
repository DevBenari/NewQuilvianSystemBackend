# Laporan Penyelesaian Task: BE-HRD-228 Kalkulator Rapor Akhir Komposit OPPE 60:40 & Sidang Pengesahan Subkomite Mutu

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-228`
* **Nama Task**: Pembangunan Kalkulator Rapor Akhir Komposit OPPE 60:40, Penentuan Grade Mutu Klinis, Sidang Pengesahan Kepala KSM, dan Alur Konfirmasi Baca Dokter
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE`
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxOppeStaffEvaluation.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/OppeEvaluationDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IOppeDataAggregationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/OppeDataAggregationService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/OppeOngoingEvaluationController.cs`

---

## 2. Latar Belakang & Formula Kalkulator Baku `OPP-B1`
1. **Formula Komposit Deterministik `OPP-B1` (`HRD-DEC-152`)**:
   $$\text{FinalCompositeScore} = \text{QuantitativeScore (Maks 60.00)} + \text{QualitativeScore (Maks 40.00)}$$
   Nilai akhir komposit berada dalam rentang $0.00$ sampai $100.00$.

2. **Matriks Penentuan Grade Mutu Klinis**:
   - **Grade A (Istimewa / *Outstanding*)**: Skor $\ge 85.00$. Memenuhi syarat promosi wewenang Level 4 Mandiri penuh dan penunjukan sebagai Dokter Proctor.
   - **Grade B (Baik / *Standard Passing*)**: Skor $75.00 - 84.99$. Memenuhi ambang batas standar kelulusan praktik klinis rumah sakit.
   - **Grade C (Cukup / *Early Warning*)**: Skor $65.00 - 74.99$. Sistem menerbitkan peringatan dini pemantauan performa KSM.
   - **Grade D (Kurang / *Underperformance*)**: Skor $< 65.00$. Memicu otomatis rekomendasi evaluasi terfokus intensif (FPPE) dan pembatasan wewenang klinis.

3. **Alur Sidang Pengesahan KSM & Hak Sanggah Dokter**:
   - Kepala KSM memeriksa hasil kalkulasi komposit, menambahkan catatan rekomendasi pembinaan, lalu melakukan finalisasi status menjadi `FINALIZED`.
   - Dokter yang bersangkutan menerima notifikasi di portal ESS untuk menelaah rapor dan memberikan konfirmasi pengakuan (`ACKNOWLEDGED`) atau sanggahan tertulis dalam tenggat 7 hari kerja (`OPP-W5`).

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD OPPE Clinical Performance Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/oppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/{id}` | Mengambil rincian rapor komposit OPPE, nilai kuantitatif 60%, kualitatif 40%, dan grade mutu. | `[Authorize]` Dokter / Komite Medik | - | `200 OK` (`ApiResponse<OppeStaffEvaluationDto>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/{id}/finalize` | Pengesahan hasil penilaian rapor OPPE oleh Kepala KSM dan Subkomite Mutu. | `[Authorize]` Kepala KSM / Ketua Komite | `FinalizeOppeStaffEvaluationDto` | `200 OK` (`ApiResponse<OppeStaffEvaluationDto>`) |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/{id}/acknowledge` | Konfirmasi pengakuan penerimaan rapor OPPE oleh dokter yang dievaluasi via portal ESS. | `[Authorize]` Dokter Pemilik Rapor | `AcknowledgeOppeStaffEvaluationDto` | `200 OK` (`ApiResponse<OppeStaffEvaluationDto>`) |

---

## 4. Contoh Konkret Respons Pengesahan Rapor OPPE

* **Endpoint**: `POST /api/v1/corporate/hrd/clinical-performance/oppe/staff-evaluations/f4a5b6c7-d8e9-0123-4567-890abcdef123/finalize`
* **Request Body**:
```json
{
  "evaluatorKsmHeadId": "c5d6e7f8-a9b0-1234-5678-90abcdef1234",
  "evaluatorRecommendationNotes": "Rapor OPPE Semester II 2026 disahkan dengan predikat Grade A (Skor 91.50). KSM merekomendasikan perpanjangan wewenang klinis mandiri Level 4 pada siklus rekredensialing."
}
```
* **Response Body (`200 OK`)**:
```json
{
  "success": true,
  "statusCode": 200,
  "message": "Rapor evaluasi kinerja OPPE berhasil disahkan oleh Kepala KSM.",
  "data": {
    "id": "f4a5b6c7-d8e9-0123-4567-890abcdef123",
    "periodId": "e3f4a5b6-c7d8-9012-3456-7890abcdef12",
    "periodName": "OPPE Semester II 2026",
    "workforceProfileId": "b1a2c3d4-e5f6-7890-abcd-ef1234567890",
    "employeeName": "dr. Hendra Wijaya, Sp.B-KBD",
    "employeeCode": "DOC-2026-0042",
    "specialtyName": "Spesialis Bedah Digestif",
    "quantitativeScore": 55.50,
    "qualitativeScore": 36.00,
    "finalCompositeScore": 91.50,
    "performanceGrade": "A",
    "evaluatorKsmHeadId": "c5d6e7f8-a9b0-1234-5678-90abcdef1234",
    "evaluatorKsmHeadName": "Prof. Dr. dr. Bambang Sudiro, Sp.B",
    "evaluatorRecommendationNotes": "Rapor OPPE Semester II 2026 disahkan dengan predikat Grade A...",
    "isAcknowledgedByStaff": false,
    "acknowledgedAt": null,
    "status": "FINALIZED"
  },
  "errors": null,
  "timestamp": "2026-11-09T14:25:30Z"
}
```

---

## 5. Verifikasi Kompilasi & Konformansi
* **Status Kompilasi C#**: Lolos kompilasi dengan **`0 Error(s)`**.
* **Keamanan Akses**: Sistem memverifikasi bahwa konfirmasi penerimaan rapor pada endpoint `acknowledge` hanya dapat dieksekusi oleh akun dokter pemilik rapor yang bersangkutan.
