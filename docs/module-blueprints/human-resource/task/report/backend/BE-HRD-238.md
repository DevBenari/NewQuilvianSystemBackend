# Laporan Penyelesaian Task: BE-HRD-238 Entity Model EF Core, Indeks DB & Skema Relasional Postgres untuk 10 Tabel LearningAndDevelopment & Ikatan Dinas

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-238`
* **Nama Task**: Entity Model EF Core, Indeks DB & Skema Relasional Postgres untuk 10 Tabel `LearningAndDevelopment` & Ikatan Dinas
* **Modul / Sub-Modul**: Human Resource / Sub-modul 16: Learning, Training & Competency Management
* **Slice Delivery**: `S-C2` (Kontrak API `v20`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/MstMandatoryTrainingRule.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingPlan.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingSession.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingEnrollmentRequest.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingAttendance.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingAssessment.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingCertificate.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxTrainingBudget.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/HrdTrainingBondContract.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/LearningAndDevelopment/Models/TrxPlataranSehatSkpRecord.cs`
  - `NewQuilvianSystemBackend/Data/ApplicationDbContext.cs`

---

## 2. Latar Belakang & Fondasi Basis Data Pembelajaran dan Diklat Rumah Sakit
Dalam operasional rumah sakit modern yang terakreditasi STARKES KPS 8 (Kualifikasi dan Pendidikan Staf) dan terintegrasi dengan regulasi Kementerian Kesehatan RI (Plataran Sehat SatuSehat), tata kelola pendidikan dan pelatihan SDM memerlukan struktur basis data yang solid, presisi, dan terhubung erat dengan modul jadwal dinas (Sub-06) serta kredensialing tenaga medis/nakes (Sub-14).

Task `BE-HRD-238` mendefinisikan 10 model entitas relasional EF Core yang mencakup siklus hulu-ke-hilir pembelajaran pegawai rumah sakit:
1. **Master Aturan Wajib Akreditasi (`MstMandatoryTrainingRule`)**: Katalog 5 pelatihan dasar wajib (BHD, PPI, SKP, K3RS, SBAR) beserta batas masa berlaku maksimal 24 bulan (`VR-TRN-01`, `VR-TRN-02`).
2. **Rencana Tahunan TNA (`TrxTrainingPlan`)**: Dokumen rencana kebutuhan pelatihan (Training Needs Analysis) dengan pagu anggaran dan approval Direksi (`VR-TRN-13`).
3. **Sesi Pelaksanaan Kelas/Lab (`TrxTrainingSession`)**: Batch pelatihan, jadwal kelas tatap muka / simulasi OSCE, kuota peserta, dan instruktur (`VR-TRN-07`, `VR-TRN-08`).
4. **Permohonan Pendaftaran (`TrxTrainingEnrollmentRequest`)**: Formulir pengajuan mandiri staf melalui ESS dengan alur verifikasi Karu dan Diklat (`VR-TRN-10`).
5. **Presensi Kehadiran (`TrxTrainingAttendance`)**: Log kehadiran harian dengan validasi QR Code / biometrik dan ambang minimal kehadiran 80% s.d. 100% (`VR-TRN-03`).
6. **Penilaian Ujian Teori & OSCE (`TrxTrainingAssessment`)**: Hasil ujian tertulis (ambang 75.00) dan simulasi keterampilan klinis langsung (ambang 80.00) lengkap dengan rubrik JSONB dan verifikasi tanda tangan digital penilai (`VR-TRN-04`, `VR-TRN-05`, `VR-TRN-06`).
7. **Sertifikat Pelatihan Digital (`TrxTrainingCertificate`)**: Rekam sertifikat kelulusan resmi, masa berlaku, nomor registrasi seri, dan status radar kedaluwarsa (`VR-TRN-09`).
8. **Pengendalian Anggaran Unit (`TrxTrainingBudget`)**: Monitoring alokasi pagu diklat tahunan per departemen dengan pemisahan saldo komitmen dan sisa saldo anggaran (`VR-TRN-14`, `VR-TRN-15`).
9. **Kontrak Ikatan Dinas Retensi (`HrdTrainingBondContract`)**: Perjanjian retensi masa kerja (12-36 bulan) bagi pelatihan bernilai tinggi (> Rp 10.000.000 atau durasi > 1 bulan) beserta formula denda penalti wanprestasi resign (`VR-TRN-16`, `VR-TRN-17`, `VR-TRN-18`).
10. **Sinkronisasi SKP Plataran Sehat (`TrxPlataranSehatSkpRecord`)**: Log perolehan satuan kredit profesi (SKP) Kemenkes, sinkronisasi gateway eksternal SatuSehat Plataran Sehat, dan pelaporan capaian portofolio 50 SKP nakes (`INT-HR-80`).

---

## 3. Kamus Entitas & Registrasi DbSet ApplicationDbContext

Seluruh model entitas telah didaftarkan di `ApplicationDbContext.cs` di bawah namespace `QuilvianSystemBackend.Areas.Corporate.HumanResource.LearningAndDevelopment.Models`:
* `DbSet<MstMandatoryTrainingRule> MstMandatoryTrainingRules`
* `DbSet<TrxTrainingPlan> TrxTrainingPlans`
* `DbSet<TrxTrainingSession> TrxTrainingSessions`
* `DbSet<TrxTrainingEnrollmentRequest> TrxTrainingEnrollmentRequests`
* `DbSet<TrxTrainingAttendance> TrxTrainingAttendances`
* `DbSet<TrxTrainingAssessment> TrxTrainingAssessments`
* `DbSet<TrxTrainingCertificate> TrxTrainingCertificates`
* `DbSet<TrxTrainingBudget> TrxTrainingBudgets`
* `DbSet<HrdTrainingBondContract> HrdTrainingBondContracts`
* `DbSet<TrxPlataranSehatSkpRecord> TrxPlataranSehatSkpRecords`

Setiap entitas mengimplementasikan kolom jejak audit standar rumah sakit: `created_at`, `created_by`, `updated_at`, `updated_by`, dan `is_deleted` (soft delete).

---

## 4. Verifikasi Acceptance Criteria & Integritas Skema
1. **Model Relasional Lengkap**: ✅ Lulus. Seluruh 10 model entitas C# terdefinisi dengan tipe data numerik biaya presisi `decimal(18,2)` / `decimal(15,2)`, tanggal presisi tinggi `DateTime` berzona waktu UTC, dan relasi foreign key terkonfigurasi rapi.
2. **Kesesuaian Invarian Bisnis**: ✅ Lulus. Tabel mendukung pembatasan siklus 24 bulan, status radar multi-tier, perhitungan denda ikatan dinas prorata, dan pengelompokan perolehan SKP Plataran Sehat.
3. **Kesiapan Database**: ✅ Lulus. Fondasi siap menopang seluruh alur logika bisnis gelombang `MVP-48`, `MVP-49`, dan `MVP-50`.
