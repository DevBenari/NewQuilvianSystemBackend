# Laporan Penyelesaian Backend Task: BE-HRD-178

## 1. Identitas Task
* **ID Task**: `BE-HRD-178`
* **Nama Task**: Layanan konfigurasi matriks kelayakan benefit per grade & kelas kamar rawat inap (`BenefitPlanService`, `HrdBenefitGradeEligibility`, `FR-BEN-03`, `FR-BEN-04`, `FR-BEN-05`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 12 — Benefit Management (Manajemen Manfaat, Asuransi BPJS & Tanggungan Pegawai Rumah Sakit)
* **Slice / Milestone**: `Slice S-D3` · `Fase MVP-36: Paket Manfaat 3-Lapisan, Kelayakan Grade & Kuota Tanggungan ESS`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-09
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam tata kelola SDM rumah sakit, manfaat asuransi kesehatan swasta dan subsidi berobat internal tidak dipukul rata sama untuk seluruh jenjang staf. Tingkat tanggung jawab dan kompetensi profesi menentukan hak kelas perawatan kamar rawat inap (*Inpatient Room Class*) serta pagu plafon tahunan spesifik.

Layanan matriks kelayakan grade pada `BenefitPlanService` mengelola aturan ini dengan penegakan invariant:
1. **Pemetaan Hirarki Golongan Staf (`FR-BEN-03`)**:
   - `GRADE-I`: Dokter Spesialis, Dokter Subspesialis, dan Direksi (Hak Kamar: VVIP / VIP).
   - `GRADE-II`: Perawat Pelaksana, Bidan, dan Kepala Ruangan (Hak Kamar: Kelas 1).
   - `GRADE-III`: Penunjang Medis (Radiografer, Analis Lab, Fisioterapis, Apoteker) (Hak Kamar: Kelas 1).
   - `GRADE-IV`: Staf Umum, Administrasi, Kasir, Satpam, dan Driver (Hak Kamar: Kelas 2).
   - `GRADE-ALL`: Berlaku universal (misalnya regulasi wajib BPJS).
2. **Diferensiasi Status Hubungan Kerja (`EmploymentTypeCode`)**:
   - Membedakan batas pagu plafon antara pegawai tetap (*Permanent / PKWTT*) dan pegawai kontrak (*Contract / PKWT*).
3. **Penetapan Hak Kelas Kamar Rawat Inap (`FR-BEN-04`)**:
   - Menetapkan hak kelas rawat inap yang diakui untuk pertanggungan asuransi swasta dan subsidi internal (VVIP, VIP, Kelas 1, Kelas 2, Kelas 3). Bila staf memilih kamar di atas hak kelasnya, selisih biaya menjadi ekses tagihan mandiri.
4. **Penguncian Klausul Mandatory COB BPJS Kesehatan (`FR-BEN-05`, `BEN-B5`)**:
   - Paket asuransi swasta tambahan (Executive dan Staff) secara default mengunci klausul `RequiresBPJSPrimaryCOB = true`. Aturan ini mengharuskan staf menggunakan BPJS Kesehatan sebagai Penjamin Utama terlebih dahulu.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/BenefitManagement/DTOs/BenefitPlanDtos.cs`
* `CreateBenefitGradeEligibilityRequestDto`: Payload penambahan aturan grade baru.
* `UpdateBenefitGradeEligibilityRequestDto`: Payload pembaruan limit spesifik dan hak kelas kamar.
* `BenefitGradeEligibilityResponseDto`: Respons data aturan kelayakan grade.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/BenefitManagement/Services/IBenefitPlanService.cs`
* `Areas/Corporate/HumanResource/BenefitManagement/Services/BenefitPlanService.cs`
Metode:
- `GetGradeEligibilitiesAsync`
- `AddGradeEligibilityAsync`
- `UpdateGradeEligibilityAsync`
- `DeleteGradeEligibilityAsync`

---

## 4. Spesifikasi Endpoint REST API Terkait (Swagger)
File: `Areas/Corporate/HumanResource/BenefitManagement/Controllers/BenefitPlanController.cs`
Tag Swagger: `[Tags("HR Benefits - Plans & Eligibility")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/hr-benefits/plans/{planId}/grade-eligibilities` | Mengambil seluruh aturan kelayakan grade pada suatu paket manfaat | Bearer JWT | `planId: Guid` | `List<BenefitGradeEligibilityResponseDto>` |
| `POST` | `/api/hr-benefits/plans/{planId}/grade-eligibilities` | Menambahkan aturan kelayakan grade dan kelas kamar baru | Bearer JWT | `CreateBenefitGradeEligibilityRequestDto` | `BenefitGradeEligibilityResponseDto` |
| `PUT` | `/api/hr-benefits/grade-eligibilities/{id}` | Memperbarui kelas kamar atau plafon spesifik grade | Bearer JWT | `UpdateBenefitGradeEligibilityRequestDto` | `BenefitGradeEligibilityResponseDto` |
| `DELETE` | `/api/hr-benefits/grade-eligibilities/{id}` | Menghapus / menonaktifkan aturan kelayakan grade | Bearer JWT | `id: Guid` | `{ success, message }` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Penyesuaian Hak Kamar Rawat Inap**:
  - Rumah sakit meningkatkan fasilitas untuk Perawat Pelaksana Tetap (`GRADE-II`, `Permanent`):
  - Administrator HR memanggil `PUT /api/hr-benefits/grade-eligibilities/{id}`:
    * `InpatientRoomClass`: "VIP" (dinaikkan dari Kelas 1)
    * `SpecificAnnualLimit`: Rp 25.000.000 (dinaikkan dari Rp 20.000.000)
  - **Hasil**: Saat perawat yang bersangkutan dirawat di RS rekanan, asuransi swasta secara otomatis menanggung biaya kamar hingga kelas VIP.

---

## 6. Verifikasi & Integritas Bukti
* **Pencegahan Data Ganda**: Diuji dengan penolakan terhadap entri duplikat pada pasangan `(BenefitPlanId, GradeLevelCode, EmploymentTypeCode)`.
* **Soft-Delete Aman**: Penghapusan aturan kelayakan menggunakan penandaan `IsDelete = true` sehingga tidak merusak data historis klaim asuransi masa lalu.
