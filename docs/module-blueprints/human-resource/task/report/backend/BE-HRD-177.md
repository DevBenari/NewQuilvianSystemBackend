# Laporan Penyelesaian Backend Task: BE-HRD-177

## 1. Identitas Task
* **ID Task**: `BE-HRD-177`
* **Nama Task**: Layanan konfigurasi master paket manfaat 3-lapisan, pagu anggaran & persentase diskon berobat RS (`BenefitPlanService`, `FR-BEN-01`, `FR-BEN-02`, `HRD-DEC-134`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-36: Paket Manfaat 3-Lapisan, Kelayakan Grade & Kuota Tanggungan ESS`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam operasional rumah sakit swasta maupun vertikal, program kesejahteraan kesehatan karyawan harus dikelola secara terstruktur melalui katalog paket yang fleksibel dan terukur. Layanan `BenefitPlanService` mengelola siklus hidup data master paket manfaat (`MstBenefitPlan`) dengan kemampuan:
1. **Pengelompokan 3-Lapisan Paket (`FR-BEN-01`, `HRD-DEC-134`)**:
   - `BPJS_Mandatory`: Paket regulasi pemerintah (JKK, JKM, JHT, JP, dan BPJS Kesehatan) tanpa batasan limit tahunan individual (mengikuti tarif INA-CBGs).
   - `Private_Insurance`: Asuransi swasta tambahan (Executive, Gold, Silver) untuk rawat inap di rumah sakit rekanan.
   - `InHouse_HospitalCare`: Subsidi perawatan langsung di poliklinik dan rawat inap RS Quilvian sendiri.
2. **Konfigurasi Diskon Tarif Pelayanan Internal RS (`FR-BEN-02`, `HRD-ASM-35`)**:
   - Paket subsidi internal RS Quilvian dilengkapi atribut `HospitalDiscountPercentage` (default 30% untuk jasa medis/tindakan poliklinik dan 20% obat-obatan farmasi) yang dipotong otomatis sebelum memotong sisa pagu anggaran tahunan staf.
3. **Penyemaian Otomatis 4 Paket Standar RS (`SeedDefaultHospitalBenefitPlansAsync`)**:
   - Sistem menyediakan inisialisasi otomatis satu kali klik untuk 4 paket utama rumah sakit:
     * `BEN-BPJS-STD`: Wajib regulasi (H+0 Onboarding, limit Rp 0/INA-CBGs).
     * `BEN-PVT-EXEC`: Swasta Executive Direksi & Spesialis (limit Rp 50jt/thn, mandatory COB).
     * `BEN-PVT-STAFF`: Swasta Staf Klinis & Penunjang (limit Rp 20jt/thn, mandatory COB).
     * `BEN-INH-SUBSIDY`: Subsidi Berobat Internal RS Quilvian (diskon 30%, limit Rp 10jt/thn).

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitPlanDtos.cs`
* `BenefitPlanFilterDto`: Filter kategori, tier, status aktif, dan kata kunci pencarian.
* `CreateBenefitPlanRequestDto`: Payload registrasi paket manfaat baru.
* `UpdateBenefitPlanRequestDto`: Payload pemutakhiran konfigurasi paket.
* `BenefitPlanResponseDto`: Data paket lengkap beserta relasi tipe dan matriks kelayakan grade.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/BenefitManagement/Services/IBenefitPlanService.cs`
* `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitPlanService.cs`
Metode:
- `GetBenefitPlansAsync`
- `GetBenefitPlanByIdAsync`
- `CreateBenefitPlanAsync`
- `UpdateBenefitPlanAsync`
- `SeedDefaultHospitalBenefitPlansAsync`

---

## 4. Spesifikasi Endpoint REST API Terkait (Swagger)
File: `Areas/Corporate/HumanResource/BenefitManagement/Controllers/BenefitPlanController.cs`
Tag Swagger: `[Tags("HR Benefits - Plans & Eligibility")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/hr-benefits/plans` | Mengambil katalog paket manfaat 3-lapisan dengan filter pencarian | Bearer JWT | `BenefitPlanFilterDto` | `List<BenefitPlanResponseDto>` |
| `GET` | `/api/hr-benefits/plans/{id}` | Mengambil detail konfigurasi paket manfaat berdasarkan ID | Bearer JWT | `id: Guid` | `BenefitPlanResponseDto` |
| `POST` | `/api/hr-benefits/plans` | Membuat master paket manfaat baru | Bearer JWT | `CreateBenefitPlanRequestDto` | `BenefitPlanResponseDto` |
| `PUT` | `/api/hr-benefits/plans/{id}` | Memperbarui parameter pagu limit dan diskon paket manfaat | Bearer JWT | `UpdateBenefitPlanRequestDto` | `BenefitPlanResponseDto` |
| `POST` | `/api/hr-benefits/plans/seed-defaults` | Penyemaian otomatis 4 master paket manfaat default rumah sakit | Bearer JWT | - | `{ message, count }` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Penyemaian Master Paket Manfaat Baru**:
  - Administrator HR memanggil endpoint `POST /api/hr-benefits/plans/seed-defaults`.
  - Sistem mendeteksi master `MstBenefitType` default (Kesehatan & Asuransi RS).
  - Sistem secara otomatis menerbitkan 4 paket manfaat baku (`BEN-BPJS-STD`, `BEN-PVT-EXEC`, `BEN-PVT-STAFF`, `BEN-INH-SUBSIDY`).
  - Staf HR tidak perlu menginput ulang parameter regulasi satu per satu secara manual.

---

## 6. Verifikasi & Integritas Bukti
* **Validasi Kode Unik**: Sistem menolak pembuatan paket dengan `BenefitPlanCode` yang sudah terdaftar (`uq_benefit_plan_code`).
* **Presisi Keuangan**: Nilai pagu tahunan (`CoverageLimitAmount`) dan persentase diskon terproteksi dengan validasi rentang batas non-negatif.
