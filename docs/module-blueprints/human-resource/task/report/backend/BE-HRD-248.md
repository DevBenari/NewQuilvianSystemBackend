# Laporan Task Tracked: BE-HRD-248 — Asesmen Simulasi OSCE Klinis & Digital Signature Assessor

**ID Task**: `BE-HRD-248`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-49` (Pelaksanaan Pelatihan, Penilaian OSCE & Sertifikasi)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan pencatatan dan evaluasi asesmen simulasi praktik klinis langsung (*Objective Structured Clinical Examination* / OSCE) pada tenaga medis/keperawatan rumah sakit:
- **Passing Grade OSCE**: Ambang kelulusan praktik klinis ditetapkan minimal **80.00** (`VR-TRN-05`, `AT-HRD-TRN-14`).
- **Validasi Tanda Tangan Digital Dokter Assessor**: Wajib menyertakan digital signature dokter spesialis/assessor penguji yang sah secara kriptografis (`VR-TRN-06`). Pengiriman asesmen tanpa digital signature sah ditolak otomatis oleh sistem.
- **Rubrik Penilaian & Langkah Keselamatan Kritis**: Penilaian mencakup checklist rincian rubrik per stase ujian (`OsceRubricItemDto`), di mana kegagalan pada langkah kritis (*critical steps*) secara otomatis menggugurkan kelulusan peserta.
- **Kondisi Prasyarat Presensi**: Mewajibkan kehadiran 100% untuk staf klinis (`VR-TRN-03`) sebelum asesmen OSCE dapat dilaksanakan.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/ClinicalOsceDtos.cs`
     - `OsceRubricItemDto`, `SubmitClinicalOsceRequestDto`, `ClinicalOsceResultDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/IClinicalOsceAssessmentService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ClinicalOsceAssessmentService.cs`
     - Verifikasi sesi, peserta, dan kehadiran penuh (100%).
     - Validasi keabsahan digital signature hash penguji (`VR-TRN-06`).
     - Evaluasi passing grade minimal 80.00 dan evaluasi langkah kritis rubrik (`VR-TRN-05`).
     - Penyimpanan payload checklist rubrik ke dalam `AssessmentResultJson`.
     - Pembaruan status peserta menjadi `PassedOsce`.
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/ClinicalOsceAssessmentsController.cs`
     - `POST /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/clinical-osce`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/clinical-osce/participants/{workforceProfileId}`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/clinical-osce`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<IClinicalOsceAssessmentService, ClinicalOsceAssessmentService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-05`**: Passing grade minimal 80.00 untuk OSCE klinis diterapkan tanpa toleransi.
- **`VR-TRN-06`**: Digital signature assessor wajib ada dan tervalidasi sebelum asesmen disimpan.
- **`AT-HRD-TRN-14`**: Asesmen OSCE klinis berhasil mencatat rubrik penilaian, digital signature, dan menghasilkan keputusan kelulusan yang valid.
