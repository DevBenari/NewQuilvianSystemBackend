# Laporan Penyelesaian Backend Task: BE-HRD-166

## 1. Identitas Task
* **ID Task**: `BE-HRD-166`
* **Nama Task**: Layanan evaluasi kuota & sisa plafon tahunan pegawai untuk klaim kacamata/medis di ESS (`VR-DED-10`, `VR-DED-11`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-34: Portal ESS Klaim Restitusi Mandiri & Alur Approval 3-Tier`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Dalam operasional rumah sakit modern, tenaga medis (dokter, perawat, analis lab) dan staf pendukung berhak memperoleh manfaat bantuan kacamata kerja anti-radiasi dan penggantian pengobatan darurat fasilitas kesehatan luar (*Faskes Luar Darurat*). Namun, hak ini terikat pada pagu plafon tahunan (*Annual Limit*) per golongan/grade staf serta masa kerja minimum yang diatur dalam perjanjian kerja bersama rumah sakit (`HRD-DEC-130`).

Sebelum pegawai mengajukan klaim di portal Employee Self-Service (ESS), sistem harus menyediakan visibilitas transparan mengenai:
1. Masa kerja pegawai saat ini dan status kelayakan (*eligibility*).
2. Total pagu plafon tahun berjalan berdasarkan golongan profesi.
3. Total nominal klaim yang telah terpakai/disetujui (*Accumulated Used Amount*).
4. Sisa kuota yang masih dapat diklaim (*Remaining Quota*).
5. Peringatan dini (*Safety Warning* `DED-W2`) jika kuota tersisa sudah kurang dari 20% agar staf dapat merencanakan pengeluaran kesehatannya secara bijak.

Layanan `GetEmployeeRemainingQuotaAsync` pada `RestitutionClaimService` dibangun untuk menyajikan data evaluasi ini secara *real-time*.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Object (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/RestitutionClaimDtos.cs`
* `EmployeeRemainingQuotaDto`:
  - `EmployeeId`, `EmployeeName`, `EmployeeNik`
  - `GradeLevelCode` & `BenefitCategory`
  - `AnnualLimitAmount`: Plafon tahunan resmi (misal Rp 1.500.000 untuk perawat)
  - `MinimumServiceMonths` & `CurrentServiceMonths`: Masa kerja syarat vs riil
  - `IsEligibleByServiceMonths`: Status lolos masa kerja
  - `AccumulatedUsedAmount`: Akumulasi klaim approved tahun berjalan
  - `RemainingQuotaAmount`: Sisa kuota aman
  - `PercentageRemaining`: Persentase kuota tersisa
  - `IsWarningLowQuota`: Indikator oranye jika sisa kuota $< 20\%$ (`DED-W2`)
  - `AllowEmergencyCitoCashier`: Hak akses pencairan cepat cito kasir

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IRestitutionClaimService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/RestitutionClaimService.cs`
Metode: `GetEmployeeRemainingQuotaAsync`
Alur Logika:
1. Membaca profil kepegawaian `MstEmployee` dan tanggal bergabung `JoinDate`.
2. Menghitung lama masa kerja dalam satuan bulan:
   $$\text{CurrentServiceMonths} = ((\text{now.Year} - \text{emp.JoinDate.Year}) \times 12) + \text{now.Month} - \text{emp.JoinDate.Month}$$
3. Membaca kebijakan benefit aktif dari `HrdRestitutionSetting` sesuai golongan (`GradeLevel`). Jika masa kerja belum memenuhi syarat, plafon otomatis ditetapkan Rp 0 (`IsEligibleByServiceMonths = false`).
4. Menjumlahkan seluruh klaim approved pada tahun kalender berjalan:
   `c.ExpenseDate.Year == now.Year` dan `c.ClaimStatus in ['Approved', 'QueuedForPayroll', 'ProcessedInPayroll', 'VoucherIssued', 'DisbursedViaCashier', 'Closed']`.
5. Menghitung sisa kuota: $\text{RemainingQuotaAmount} = \max(0, \text{AnnualLimitAmount} - \text{UsedAmount})$.
6. Memicu flag `IsWarningLowQuota = true` jika $\text{RemainingQuotaAmount} < 20\% \times \text{AnnualLimitAmount}$.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/RestitutionClaimController.cs`
Tag Swagger: `[Tags("HR Deductions - Restitution Claims")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/hr-deductions/claims/quota` | Cek kuota dan sisa plafon tahunan pegawai real-time | Bearer JWT | Query `employeeId: Guid, benefitCategory: string` | `EmployeeRemainingQuotaDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Evaluasi Plafon Kacamata Perawat IGD**:
  - Suster Ratna (Grade II - Perawat Pelaksana, Join Date: 1 Januari 2025).
  - Pengecekan per 6 November 2026: Masa kerja = 22 bulan $\ge$ syarat 6 bulan (**Eligible**).
  - Plafon Tahunan Kacamata Grade II: **Rp 1.500.000**.
  - Riwayat Klaim Tahun 2026: Pernah klaim frame kacamata Rp 1.250.000 pada Maret 2026.
  - **Hasil Evaluasi**:
    * Sisa Kuota: Rp 1.500.000 - Rp 1.250.000 = **Rp 250.000** (16.67%).
    * Karena sisa kuota $16.67\% < 20\%$, indikator peringatan menyala oranye (`IsWarningLowQuota = true`).
    * Suster Ratna diinfokan di ESS bahwa jatah sisa kacamata tahun ini tinggal Rp 250.000.
