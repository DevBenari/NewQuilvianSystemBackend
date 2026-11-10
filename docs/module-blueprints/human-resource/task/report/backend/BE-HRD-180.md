# Laporan Penyelesaian Backend Task: BE-HRD-180

## 1. Identitas Task
* **ID Task**: `BE-HRD-180`
* **Nama Task**: Engine evaluasi kelayakan paket & kuota plafon benefit staf terpadu (`BenefitPlanService.EvaluateEmployeePlanEligibilityAsync`, `AT-HRD-BEN-01`, `AT-HRD-BEN-02`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-36: Paket Manfaat 3-Lapisan, Kelayakan Grade & Kuota Tanggungan ESS`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sebelum seorang tenaga medis atau staf non-medis didaftarkan ke polis asuransi komersial atau menikmati subsidi berobat internal rumah sakit, sistem wajib melakukan validasi kelayakan berlapis secara otomatis tanpa interpretasi manual yang berpotensi memicu bias atau kelalaian administratif.

Engine `EvaluateEmployeePlanEligibilityAsync` menjalankan evaluasi deterministik berbasis aturan bisnis:
1. **Penegakan Perlindungan Cepat H+0 Onboarding (`BEN-B1`, `VR-BEN-11`)**:
   - Seluruh pegawai baru yang bergabung (H+0 Onboarding) langsung berhak atas paket dasar `BEN-BPJS-STD` (BPJS Ketenagakerjaan: JKK & JKM untuk mitigasi kecelakaan kerja dan kematian nakes sejak menit pertama dinas).
2. **Penahanan Paket Kesehatan Selama Masa Percobaan 3 Bulan (`BEN-B1`, `VR-BEN-12`)**:
   - Staf yang masa kerjanya kurang dari 3 bulan (`serviceMonths < 3`) secara otomatis diklasifikasikan berstatus `IsProbation = true`. Paket asuransi swasta tambahan (`Private_Insurance`) dan subsidi berobat internal RS (`InHouse_HospitalCare`) dikelompokkan ke dalam daftar `IneligiblePlans` dengan catatan penahanan masa percobaan (*PendingActivation*).
3. **Pencocokan Matriks Grade Kepegawaian & Ikatan Kerja (`FR-BEN-03`, `FR-BEN-04`)**:
   - Staf yang telah lulus masa percobaan dicocokkan grade-nya (`GradeLevelCode`) dan status kerjanya (`EmploymentTypeCode`: PKWTT / PKWT).
   - Menghasilkan rincian hak: pagu limit tahunan efektif (`SpecificAnnualLimit`), hak kelas kamar rawat inap (`InpatientRoomClass`: Kelas 2, Kelas 1, VIP, VVIP), dan persentase diskon tarif rumah sakit (`HospitalDiscountPercentage`).
4. **Klausul Mandatory COB BPJS Kesehatan (`BEN-B5`)**:
   - Menandai secara transparan bahwa paket asuransi swasta yang eligible menuntut kepesertaan aktif BPJS Kesehatan sebagai Penjamin Utama.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitPlanDtos.cs`
* `EvaluateEmployeeBenefitEligibilityRequestDto`: Parameter input ID pegawai dan simulasi masa kerja (opsional).
* `EmployeeBenefitEligibilityEvaluationResultDto`: Hasil evaluasi terpadu memuat:
  - Identitas pegawai, NIK, grade, dan tanggal masuk kerja (`JoinDate`).
  - Lama masa kerja riil (`ActualServiceMonths`) dan indikator status probation (`IsProbation`).
  - `EligiblePlans`: Daftar paket manfaat yang sah dan aktif dinikmati.
  - `IneligiblePlans`: Daftar paket yang belum/tidak berhak beserta alasan formal penolakannya.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/BenefitManagement/Services/IBenefitPlanService.cs`
* `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitPlanService.cs`
Metode: `EvaluateEmployeePlanEligibilityAsync`

### C. Controller REST API
File: `Areas/Corporate/HumanResource/BenefitManagement/Controllers/BenefitPlanController.cs`
Endpoint: `POST /api/hr-benefits/plans/evaluate-eligibility`
Tag Swagger: `[Tags("HR Benefits - Plans & Eligibility")]`

---

## 4. Contoh Skenario Rumah Sakit Konkret
* **Skenario 1 — Perawat Baru Bergabung (Masa Kerja 1 Bulan / Probation)**:
  - Ns. Farhan baru bekerja selama 35 hari di RS Quilvian (`JoinDate = 5 Oktober 2026`).
  - HR memanggil `POST /api/hr-benefits/plans/evaluate-eligibility`:
  - **Hasil Evaluasi**:
    * `IsProbation`: **`true`** (`ActualServiceMonths = 1`).
    * `EligiblePlans`: 1 paket (`BEN-BPJS-STD` — Jaminan dasar BPJS TK/Kes).
    * `IneligiblePlans`: 2 paket (`BEN-PVT-STAFF` & `BEN-INH-SUBSIDY`).
    * Alasan: *"Dalam masa percobaan (1/3 bulan). Paket ditahan hingga kelulusan probation (BEN-B1)."*

* **Skenario 2 — Dokter Spesialis Bedah Lulus Masa Percobaan (Masa Kerja 6 Bulan)**:
  - dr. Budi Sp.B telah bekerja 6 bulan di RS Quilvian (`GRADE-I`, `Permanent`).
  - **Hasil Evaluasi**:
    * `IsProbation`: **`false`** (`ActualServiceMonths = 6`).
    * `EligiblePlans`: 3 paket lengkap:
      1. `BEN-BPJS-STD` (Wajib Pemerintah).
      2. `BEN-PVT-EXEC` (Pagu Rp 50.000.000/thn, Hak Kamar **VVIP**, Mandatory COB BPJS).
      3. `BEN-INH-SUBSIDY` (Pagu Subsidi RS Rp 15.000.000/thn, Hak Kamar **VVIP**, Diskon Jasa Medis **30%**).
    * `IneligiblePlans`: 0 paket.

---

## 5. Verifikasi & Integritas Bukti
* **Pencegahan Fraud Fasilitas**: Memastikan nakes dalam masa probation tidak dapat memanfaatkan plafon asuransi swasta sebelum ada SK pengesahan dari HRD.
* **Audit Kepatuhan UAT**: Memenuhi kriteria penerimaan `AT-HRD-BEN-01` dan `AT-HRD-BEN-02` (Katalog paket & matriks grade).
