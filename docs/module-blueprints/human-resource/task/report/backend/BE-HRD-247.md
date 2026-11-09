# Laporan Task Tracked: BE-HRD-247 — Penilaian Post-Test Teori Tertulis & Remedial

**ID Task**: `BE-HRD-247`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-49` (Pelaksanaan Pelatihan, Penilaian OSCE & Sertifikasi)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan pencatatan dan evaluasi hasil ujian teori tertulis (Post-Test) peserta diklat (`TrxTrainingAssessment`) dengan aturan bisnis:
- **Passing Grade**: Ambang kelulusan standar minimal **75.00** (`VR-TRN-04`, `AT-HRD-TRN-13`).
- **Prasyarat Ujian**: Memverifikasi kelayakan presensi kehadiran (`VR-TRN-03`) sebelum ujian diizinkan.
- **Kebijakan Remedial Bertingkat**: Peserta yang belum mencapai nilai 75.00 berhak mengikuti remedial maksimal **2 kali** (total 3 kali percobaan) (`VR-TRN-20`, `AT-HRD-TRN-15`).
- **Konsekuensi Kegagalan Permanen**: Jika peserta gagal pada percobaan ketiga (Attempt 3), kuota remedial dinyatakan habis (`RemainingRemedialAttempts = 0`), status peserta menjadi `Failed`, dan diwajibkan mengulang kelas pelatihan dari awal.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TheoryPostTestDtos.cs`
     - `SubmitTheoryPostTestRequestDto`, `TheoryPostTestResultDto`, `ParticipantAssessmentSummaryDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITheoryPostTestAssessmentService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TheoryPostTestAssessmentService.cs`
     - Validasi sesi, staf, dan kelayakan presensi via `ITrainingAttendanceService`.
     - Penghitungan nomor percobaan otomatis (`AttemptNumber = previousAttempts.Count + 1`).
     - Pemblokiran ujian jika telah lulus sebelumnya atau kuota remedial telah habis (> 3 percobaan).
     - Pembaruan status kelulusan peserta (`ParticipantStatus = "PassedExam"`).
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TheoryPostTestAssessmentsController.cs`
     - `POST /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/theory-post-test`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/theory-post-test/participants/{workforceProfileId}`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/assessments/theory-post-test`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<ITheoryPostTestAssessmentService, TheoryPostTestAssessmentService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-04`**: Passing grade minimal 75.00 dievaluasi secara presisi.
- **`VR-TRN-20`**: Batasan maksimal 2 kali remedial ditegakkan secara ketat; peserta dengan 3 kali gagal diblokir dari ujian ulang.
- **`AT-HRD-TRN-13`**: Evaluasi post-test teori menghasilkan status kelulusan yang valid.
- **`AT-HRD-TRN-15`**: Percobaan remedial ketiga yang gagal mengunci status gugur permanen.
