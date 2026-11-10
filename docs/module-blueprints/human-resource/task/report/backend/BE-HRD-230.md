# Laporan Penyelesaian Task: BE-HRD-230 Inisiasi Penugasan FPPE Terarah & Penunjukan Proctor Senior

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-230`
* **Nama Task**: Pembangunan Layanan Inisiasi Penugasan FPPE Terarah, Penunjukan Proctor Senior, dan Penguncian Ambang Target Kasus
* **Modul / Sub-Modul**: Human Resource / Sub-modul 15: OPPE, FPPE & Recredentialing
* **Slice Delivery**: `S-C1-OPPE` (Kontrak API `v19`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxFppeEngagement.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/DTOs/FppeEvaluationDtos.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/IProctoringWorkflowService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Services/ProctoringWorkflowService.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Controllers/FppeFocusedEvaluationController.cs`

---

## 2. Aturan Bisnis & Tata Kelola Penunjukan Proctor Senior
Sesuai regulasi kredensialing rumah sakit dan KARS/STARKES:
1. **Kualifikasi Senioritas Dokter Proctor (`VR-OPP-08`)**:
   - Dokter yang ditunjuk sebagai pendamping proctoring FPPE wajib merupakan dokter spesialis dengan pengalaman masa kerja klinis minimal 3 tahun ($\ge 3$ tahun).
   - Penunjukan dokter yang belum memenuhi masa kerja 3 tahun secara otomatis ditolak sistem demi menjamin kompetensi supervisi dan keselamatan pasien (*patient safety*).
2. **Target Kasus & Durasi Proctoring Staf Dokter Baru (`VR-OPP-09`)**:
   - Dokter spesialis baru bergabung wajib menjalani masa observasi klinis dengan target minimal 10 kasus tindakan pertama dalam kurun waktu 3 bulan (90 hari).
   - Pengaturan target kasus $< 10$ atau durasi $< 90$ hari ditolak validasi sistem.
3. **Target Kasus & Durasi Prosedur Klinis Baru (`VR-OPP-10`)**:
   - Dokter lama yang mengajukan penambahan rincian kewenangan klinis (prosedur baru yang belum pernah dikerjakan di RS setempat) wajib menjalani proctoring minimal 5 kasus tindakan dalam durasi observasi minimal 6 bulan (180 hari).
4. **Pemberian Nomor Registrasi Unik**:
   - Sistem menerbitkan nomor identifikasi resmi berformat `FPPE-{YYYYMM}-{XXXX}` untuk memastikan keterlacakan audit (*traceability*).

---

## 3. Spesifikasi Endpoint API Bergaya Swagger

### Tag Grup Swagger: `[Tags("Corporate - HRD FPPE Incident & Focused Evaluation")]`
**Base Route**: `/api/v1/corporate/hrd/clinical-performance/fppe`

| Method | Path Endpoint | Deskripsi Bisnis & Fungsional | Otorisasi & Role | Request Payload | Response Model |
| :---: | :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/hrd/clinical-performance/fppe/engagements` | Inisiasi penugasan FPPE terarah & penunjukan Dokter Proctor pendamping. | `[Authorize]` Subkomite Mutu / Ketua Komite | `CreateFppeEngagementRequestDto` | `201 Created` (`ApiResponse<FppeEngagementResponseDto>`) |
| `GET` | `/api/v1/corporate/hrd/clinical-performance/fppe/engagements/{id}` | Mengambil rincian penugasan FPPE, kuota target kasus, dan progres pemenuhan. | `[Authorize]` Proctor / Dokter / Komite | - | `200 OK` (`ApiResponse<FppeEngagementResponseDto>`) |

---

## 4. Contoh Konkret Respons Inisiasi Penugasan FPPE

* **Endpoint**: `POST /api/v1/corporate/hrd/clinical-performance/fppe/engagements`
* **Request Body**:
```json
{
  "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
  "triggerSource": "NEW_STAFF",
  "initialReasonJustification": "Dokter spesialis bedah saraf baru bergabung. Wajib menjalani observasi 10 prosedur trepanasi/kraniotomi.",
  "assignedProctorWorkforceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "startDate": "2026-11-09T00:00:00Z",
  "targetEndDate": "2027-02-09T00:00:00Z",
  "targetCaseCount": 10
}
```
* **Response Body (`201 Created`)**:
```json
{
  "code": 201,
  "status": "Created",
  "message": "Penugasan FPPE berhasil diinisiasi dan proctor telah ditunjuk.",
  "data": {
    "id": "c3d4e5f6-a7b8-9012-cdef-123456789012",
    "engagementNumber": "FPPE-202611-7392",
    "workforceProfileId": "e1f2a3b4-5678-90ab-cdef-1234567890ab",
    "staffName": "dr. Robert Sp.BS",
    "staffCode": "DOC-BS-009",
    "triggerSource": "NEW_STAFF",
    "assignedProctorWorkforceId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "proctorName": "dr. Hendra Sp.BS(K)",
    "startDate": "2026-11-09T00:00:00Z",
    "targetEndDate": "2027-02-09T00:00:00Z",
    "targetCaseCount": 10,
    "completedCaseCount": 0,
    "completionPercentage": 0.0,
    "status": "ACTIVE"
  }
}
```

---

## 5. Verifikasi Acceptance Criteria & Integritas Bisnis
1. **Inisiasi Proctoring Staf Baru (`VR-OPP-08`, `VR-OPP-09`, `AT-HRD-OPP-13`)**: ✅ Lulus. Kualifikasi masa kerja proctor minimal 3 tahun tervalidasi; kuota 10 kasus / 90 hari terkunci.
2. **Inisiasi Proctoring Prosedur Baru (`VR-OPP-10`, `AT-HRD-OPP-14`)**: ✅ Lulus. Kuota minimal 5 kasus dalam kurun waktu 180 hari ditegakkan ketat.
