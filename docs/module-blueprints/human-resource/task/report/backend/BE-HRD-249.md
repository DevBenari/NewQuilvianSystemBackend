# Laporan Task Tracked: BE-HRD-249 — Penerbitan e-Sertifikat Resmi & e-Dossier Sub-02

**ID Task**: `BE-HRD-249`  
**Modul/Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management  
**Slice**: `S-C2` (Kontrak API `v20`)  
**Gelombang**: `MVP-49` (Pelaksanaan Pelatihan, Penilaian OSCE & Sertifikasi)  
**Tanggal Eksekusi**: 2026-11-09  
**Status**: `✅ Selesai`

---

## 1. Ringkasan Eksekutif
Task ini mengimplementasikan generator penerbitan e-sertifikat pelatihan resmi rumah sakit (`TrxTrainingCertificate`) beserta integrasi otomatis ke rekam jejak pelatihan portofolio e-Dossier tenaga kerja pada Sub-02 Workforce Profile (`WfpTrainingRecord`, `VR-TRN-19`):
- **Syarat Kelulusan Evaluasi**: e-Sertifikat hanya dapat diterbitkan bagi peserta yang telah dinyatakan lulus ujian post-test teori atau OSCE klinis.
- **Standar Penomoran Sertifikat**: Format seri deterministik `CERT-{TypeCode}-{Year}-{Sequence:D6}` (contoh: `CERT-BLS-2026-000001`).
- **Integritas & Verifikasi Publik**: Dilengkapi hash checksum SHA-256 dan tautan verifikasi keabsahan publik (`verificationUrl`) untuk keperluan audit kepatuhan STARKES KPS 8.
- **Sinkronisasi Otomatis e-Dossier**: Menghasilkan rekaman `WfpTrainingRecord` terverifikasi secara instan tanpa perlu unggah ulang manual oleh staf/HRD.

---

## 2. File yang Dibuat / Dimodifikasi
1. **DTO**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/DTOs/TrainingCertificationDtos.cs`
     - `IssueCertificateRequestDto`, `TrainingCertificateDto`.
2. **Service**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/ITrainingCertificationService.cs`
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Services/TrainingCertificationService.cs`
     - Validasi kelulusan evaluasi peserta.
     - Penomoran seri berurutan per tahun berjalan.
     - Pembuatan rekaman hasil `TrxTrainingResult` dan sinkronisasi instan ke `WfpTrainingRecord` (`VR-TRN-19`).
     - Pembuatan e-sertifikat dengan hash integritas SHA-256.
     - Pembaruan status peserta menjadi `Certified`.
3. **Controller**:
   - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Controllers/TrainingCertificatesController.cs`
     - `POST /api/v1/corporate/hrd/training/certificates/issue`
     - `GET /api/v1/corporate/hrd/training/certificates/verify/{certificateNumber}` (AllowAnonymous untuk audit publik)
     - `GET /api/v1/corporate/hrd/training/certificates/participants/{participantId}`
     - `GET /api/v1/corporate/hrd/training/certificates/staff/{workforceProfileId}`
4. **Pendaftaran Dependency Injection**:
   - `NewQuilvianSystemBackend/Program.cs` (`builder.Services.AddScoped<ITrainingCertificationService, TrainingCertificationService>()`).

---

## 3. Verifikasi Acceptance Criteria & Invarian
- **`VR-TRN-19`**: Sinkronisasi e-Dossier Sub-02 Workforce Profile terintegrasi secara otomatis saat penerbitan sertifikat.
- **`AT-HRD-TRN-16`**: Alur penerbitan e-sertifikat tervalidasi lengkap beserta tautan verifikasi keabsahan publik.
