# Laporan Penyelesaian Task: BE-HRD-254 Migrasi Skema DB 10 Tabel Relasional Performance & Seeding Master Data Katalog KPI / Core Values BerAKHLAK

## 1. Ringkasan Eksekutif
* **ID Task**: `BE-HRD-254`
* **Nama Task**: Migrasi Skema DB 10 Tabel Relasional Performance & Seeding Master Data Katalog KPI / Core Values BerAKHLAK
* **Modul / Sub-Modul**: Human Resource / Sub-modul 17: Performance Management & Clinical Career
* **Slice Delivery**: `S-C3` (Kontrak API `v21`)
* **Status**: `✅ Selesai (2026-11-09)`
* **File Implementasi Utama**:
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/MstKpiMetricCatalog.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceCycle.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceGoal.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceAppraisal.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceAppraisalDetail.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceCalibrationSession.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceImprovementPlan.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformancePipActionLog.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxClinicalCareerLadderReview.cs`
  - `NewQuilvianSystemBackend/Areas/Corporate/HumanResource/PerformanceManagement/Models/TrxPerformanceAppraisalDispute.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/MstKpiMetricCatalogConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceCycleConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceGoalConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceAppraisalConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceAppraisalDetailConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceCalibrationSessionConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceImprovementPlanConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformancePipActionLogConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxClinicalCareerLadderReviewConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/Configurations/Corporate/HumanResource/PerformanceManagement/TrxPerformanceAppraisalDisputeConfiguration.cs`
  - `NewQuilvianSystemBackend/Repositories/ApplicationDbContext.cs`
  - `NewQuilvianSystemBackend/Migrations/20261109160000_AddPerformanceManagementCoreEntities.cs`

---

## 2. Backend Governance Preflight
* **Area**: `Corporate`
* **Module**: `HumanResource`
* **Sub-modul**: `PerformanceManagement` (Sub-modul 17: Performance Management & Clinical Career)
* **Prefix Registry**: `Hrd` (aktif / canonical) & `Trx`/`Mst` (kompatibilitas blueprint to-be dan data dictionary Bagian 22)
* **Klasifikasi Kode**: `NEW CODE` (entitas baru dan tabel relasional EF Core baru)
* **Wewenang**: Implementasi skema model EF Core C#, konfigurasi mapping `IEntityTypeConfiguration<T>`, registrasi `DbSet` di `ApplicationDbContext`, dan penulisan berkas migrasi EF Core (`Migrations/`).
* **Batasan Database**: Mengikuti protokol keselamatan database Quilvian, tidak dilakukan eksekusi DDL fisik langsung (`dotnet ef database update`) ke database server development atau master tanpa otorisasi tertulis terpisah.

---

## 3. Latar Belakang & Fondasi Basis Data Manajemen Kinerja dan Jenjang Karir Klinis Rumah Sakit
Dalam operasional rumah sakit modern yang mengacu pada standar akreditasi KARS / STARKES (KPS 11 untuk Evaluasi Kinerja Staf Medis Berkelanjutan/OPPE dan KPS 12 untuk Penilaian Kinerja Tenaga Keperawatan/Nakes Terintegrasi), pengelolaan kinerja memerlukan pemisahan yang jelas antara:
1. **Sasaran Kinerja Pegawai Kuantitatif (KPI Rumah Sakit)**: Mengukur kepatuhan klinis, mutu layanan, dan keselamatan pasien (misal: waktu tanggap IGD < 5 menit, jam visite dokter spesialis, kelengkapan rekam medis elektronik < 24 jam).
2. **Evaluasi Perilaku Kerja Kualitatif (Core Values BerAKHLAK)**: Standar nasional PermenPAN-RB No. 6/2022 (Berorientasi Pelayanan, Akuntabel, Kompeten, Harmonis, Loyal, Adaptif, Kolaboratif) dengan skala rubrik 1–5.
3. **Kalibrasi Penilaian Kinerja (Calibration Session)**: Sidang kurva normal performa untuk memastikan penilaian objektif dan bebas dari bias penilai (*leniency* atau *strictness*).
4. **Program Pembinaan Kinerja (Performance Improvement Plan / PIP)**: Program pembinaan berdurasi 30/60/90 hari bagi pegawai berkinerja rendah (`< 70.00`) dengan log aksi terstruktur mingguan.
5. **Mitigasi Sengketa Nilai (Appraisal Dispute)**: Hak banding pegawai terhadap nilai akhir atasan dengan gerbang investigasi HRD independen.
6. **Jenjang Karir Profesional Klinis (Clinical Career Ladder Review)**: Kenaikan jenjang klinis dokter (Klinis I–IV) dan perawat/bidan (PK I–V) yang mensyaratkan nilai SKP minimal, logbook tindakan, jam pelatihan wajib, dan rekomendasi Komite Medik / Keperawatan.

---

## 4. Rincian 10 Tabel Relasional yang Diimplementasikan
Sesuai dengan `data-dictionary.md` Bagian 22 dan blueprint arsitektur backend, 10 tabel relasional telah dipetakan secara lengkap:

| No | Nama Tabel PostgreSQL | Kelas Entity C# | Deskripsi & Invarian Bisnis Utama |
|---|---|---|---|
| 1 | `public.mst_kpi_metric_catalogs` | `MstKpiMetricCatalog` | Katalog master metrik KPI kuantitatif & perilaku ASN BerAKHLAK. Menyimpan rubrik skor JSONB dan target spesifik profesi (`VR-PRF-01`, `VR-PRF-02`). |
| 2 | `public.trx_performance_cycles` | `TrxPerformanceCycle` | Siklus periode penilaian kinerja (Semesteran / Tahunan) dengan status lifecycle `DRAFT`, `GOAL_SETTING`, `ACTIVE_ASSESSMENT`, `CALIBRATION`, `CLOSED`. |
| 3 | `public.trx_performance_goals` | `TrxPerformanceGoal` | Penetapan sasaran kerja individu (KPI) per siklus. Total bobot target wajib tepat 100.00% (`VR-PRF-03`). |
| 4 | `public.trx_performance_appraisals` | `TrxPerformanceAppraisal` | Rangkuman lembar penilaian kinerja pegawai (Self-assessment 20%, Evaluasi Atasan 80%, skor kalibrasi). Memiliki composite unique index pada `(PerformanceCycleId, WorkforceProfileId)` terfilter `IsDelete = false`. |
| 5 | `public.trx_performance_appraisal_details` | `TrxPerformanceAppraisalDetail` | Rincian skor butir per metrik katalog dalam lembar penilaian, kalkulasi nilai tertimbang, dan justifikasi bukti capaian. |
| 6 | `public.trx_performance_calibration_sessions` | `TrxPerformanceCalibrationSession` | Sesi sidang komite kalibrasi untuk menyeimbangkan distribusi nilai pegawai unit kerja sebelum finalisasi (`VR-PRF-08`). |
| 7 | `public.trx_performance_improvement_plans` | `TrxPerformanceImprovementPlan` | Surat penetapan PIP bagi pegawai dengan skor akhir `< 70.00` dengan target perbaikan dan tenggat waktu 30/60/90 hari (`VR-PRF-09`). |
| 8 | `public.trx_performance_pip_action_logs` | `TrxPerformancePipActionLog` | Log aktivitas harian/mingguan selama masa PIP beserta catatan pendampingan mentor/atasan. |
| 9 | `public.trx_clinical_career_ladder_reviews` | `TrxClinicalCareerLadderReview` | Rekam pengajuan dan sidang kenaikan jenjang karir klinis (PK I-V / Klinis I-IV) terintegrasi dengan rekomendasi komite medis/nakes (`VR-PRF-12`). |
| 10 | `public.trx_performance_appraisal_disputes` | `TrxPerformanceAppraisalDispute` | Pengajuan sanggah/banding nilai kinerja oleh pegawai dengan batas waktu maksimal 7 hari kalender pasca publikasi skor (`VR-PRF-10`). |

---

## 5. Seeding Master Data Katalog Metrik (HRD-DEC-164)
Dalam migrasi EF Core `20261109160000_AddPerformanceManagementCoreEntities.cs`, ditambahkan seeding otomatis SQL `ON CONFLICT ("MetricCode") DO NOTHING` untuk 12 indikator awal:
1. **7 Core Values ASN BerAKHLAK (PermenPAN-RB No. 6/2022)**:
   - `BEH-AKH-01`: Berorientasi Pelayanan (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-02`: Akuntabel (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-03`: Kompeten (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-04`: Harmonis (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-05`: Loyal (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-06`: Adaptif (Skala 1–5, Bobot Standar 5.00)
   - `BEH-AKH-07`: Kolaboratif (Skala 1–5, Bobot Standar 5.00)
2. **5 Indikator Kuantitatif Kinerja Rumah Sakit MMC**:
   - `KPI-MED-01`: Kepatuhan Jam Visite Dokter Spesialis (< 09:00 WIB) – Target 90% (Medis)
   - `KPI-MED-02`: Kelengkapan & Ketepatan Pengisian EMR Rawat Inap (< 24 Jam) – Target 95% (Medis)
   - `KPI-NSG-01`: Angka Kejadian Pasien Jatuh di Ruang Rawat Inap – Target 0 Kasus (Keperawatan)
   - `KPI-PHR-01`: Waktu Tunggu Pelayanan Resep Obat Racikan (< 30 Menit) – Target 85% (Penunjang Medis)
   - `KPI-EMG-01`: Response Time Pelayanan IGD Kasus Triage P1 (< 5 Menit) – Target 100% (Semua Staf Terkait)

---

## 6. Bukti Verifikasi & Hasil Uji Teknis
* **Kompilasi C# (`dotnet build --no-restore`)**:
  - Waktu Uji: 2026-11-09T16:52:28+07:00
  - Hasil: `0 Error(s)`, `509 Warning(s)` (hanya peringatan dokumentasi XML dan kerentanan paket legacy non-blocking).
  - Status: **LULUS / BERSIH**.
* **Audit Kepatuhan Arsitektur QBE (`tooling/qbe/Invoke-QbeConformanceCheck.ps1`)**:
  - Mode: `ReportOnly`
  - Hasil: `PASS` (exit code 0).
  - Catatan QBE-NAM-001: Model mengikuti nomenklatur blueprint dan tabel basis data kanonikal modul Performance.
* **Automated Test Backend**:
  - `AUTOMATED TEST: NOT APPLICABLE — backend tidak memelihara project test otomatis (rules/backend/TEST_POLICY.md)`.

---

## 7. Status Penyelesaian & Tindak Lanjut
Task `BE-HRD-254` telah selesai secara paripurna sesuai Definition of Done (DoD). Skema basis data siap digunakan oleh task business logic berikutnya pada Slice `S-C3`:
* **Task Berikutnya**: `BE-HRD-255` – *Service & Controller Katalog KPI & Core Values BerAKHLAK (6 Endpoint Master Data Standar)*.
