# Laporan Task Tracked: BE-HRD-246 — Presensi Harian Sesi & Validasi Ambang 80/100%

**ID Task**: `BE-HRD-246`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-49` (Pelaksanaan Pelatihan, Penilaian OSCE & Sertifikasi)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan pencatatan presensi kehadiran harian kelas diklat (`TrxTrainingAttendance`) dengan mekanisme Check-In dan Check-Out otomatis, serta engine validasi ambang kehadiran berbasis regulasi keselamatan klinis rumah sakit:
- **Tenaga Non-Klinis**: Ambang kehadiran minimal **80.00%** dari total jam/sesi yang dijadwalkan.
- **Tenaga Klinis / Medis**: Ambang kehadiran wajib mutlak **100.00%** (`VR-TRN-03`).

Peserta yang tidak memenuhi ambang batas kehadiran secara otomatis diblokir hak kelayakannya (`IsEligibleForExam = false`) untuk mengikuti ujian post-test teori maupun asesmen simulasi klinis OSCE.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingAttendanceDtos.cs`
     - `TrainingAttendanceDto`, `RecordAttendanceCheckInRequestDto`, `RecordAttendanceCheckOutRequestDto`, `ParticipantAttendanceEligibilityDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingAttendanceService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingAttendanceService.cs`
     - Validasi sesi diklat dan peserta terdaftar.
     - Pencatatan Check-In harian dengan stempel waktu dan sumber presensi (QRCode/Manual).
     - Pencatatan Check-Out harian beserta perhitungan akumulasi menit hadir (`AttendedMinutes`).
     - Engine evaluasi kelayakan ujian berbasis kategori staf klinis vs non-klinis (`VR-TRN-03`).
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingAttendancesController.cs`
     - `POST /api/v1/corporate/hrd/training/sessions/{sessionId}/attendances/check-in`
     - `POST /api/v1/corporate/hrd/training/sessions/{sessionId}/attendances/check-out`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/attendances`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/attendances/eligibility/{workforceProfileId}`
     - `GET /api/v1/corporate/hrd/training/sessions/{sessionId}/attendances/eligibilities`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<ITrainingAttendanceService, TrainingAttendanceService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-03`**: Ambang batas kehadiran terverifikasi otomatis. Staf klinis diwajibkan 100% presensi; staf non-klinis minimal 80%.
- **`AT-HRD-TRN-12`**: Evaluasi presensi memvalidasi kelayakan ujian dan memblokir akses evaluasi jika jam hadir tidak mencukupi target.
