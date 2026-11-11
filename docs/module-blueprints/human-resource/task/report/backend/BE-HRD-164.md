# Laporan Penyelesaian Backend Task: BE-HRD-164

## 1. Identitas Task
* **ID Task**: `BE-HRD-164`
* **Nama Task**: Engine validasi hard-cap maksimal pemotongan upah tepat 50% Gross Pay (`PayrollDeductionEngineService`, `DED-B1`, PP 36/2021)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 11 — Restitusi Klaim & Integrasi Pemotongan Penggajian
* **Slice / Milestone**: `Slice S-B6` · `Fase MVP-33: Fondasi Database, Kebijakan Plafon, Engine Guard 50% & Sinkronisasi Finance`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sesuai amanat **Pasal 65 Peraturan Pemerintah (PP) No. 36 Tahun 2021 tentang Pengupahan jo PP No. 49 Tahun 2025**, pemotongan upah pekerja untuk ganti rugi, pinjaman, dan tagihan lainnya tidak boleh melebihi **50% (lima puluh persen)** dari setiap pembayaran upah yang menjadi hak pekerja.

Dalam operasional rumah sakit, seorang staf (misalnya perawat atau dokter) sering kali memiliki beberapa kewajiban finansial bersamaan: cicilan kasbon RS, pinjaman koperasi karyawan, hingga potongan iuran paguyuban. Tanpa mesin proteksi upah (*Hard-Cap 50%*), risiko gaji staf terpotong habis atau bahkan minus (gaji minus) sangat tinggi, yang dapat mengganggu kesejahteraan dan fokus nakes dalam melayani pasien (`HRD-DEC-128`, `DED-B1`).

Layanan `PayrollDeductionEngineService` mengimplementasikan guard kalkulasi ketat untuk menghitung pagu batas maksimal potongan ($50\% \times \text{GrossPay}$) sebelum alokasi cicilan dieksekusi.

---

## 3. Komponen Teknis yang Dibuat

### A. Data Transfer Objects (DTO)
File: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollDeductionEngineDtos.cs`
* `DeductionSimulationResultDto`: Memuat informasi `GrossPay`, pagu batas maksimal `MaxAllowedDeduction` (50% Gross Pay), akumulasi tagihan terjadwal `TotalScheduledDeduction`, total potongan aktual `TotalActualDeduction`, total nominal yang ditunda `TotalDeferredDeduction`, flag status `IsCapped`, serta rincian item.
* `DeductionItemResultDto`: Rincian per item pemotongan cicilan.

### B. Interface & Service Implementasi
File:
* `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollDeductionEngineService.cs`
* `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollDeductionEngineService.cs`
Metode Kunci untuk BE-HRD-164:
1. `CalculateSalaryCapLimit`:
   Menghitung pagu maksimal pemotongan secara deterministik menggunakan pembulatan perbankan `MidpointRounding.AwayFromZero`:
   $$\text{MaxAllowedDeduction} = \text{Math.Round}\left(\text{GrossPay} \times \frac{\text{MaxPercentage}}{100}, 2, \text{MidpointRounding.AwayFromZero}\right)$$
2. `SimulateEmployeeDeductionsAsync`:
   Menjalankan simulasi pra-penggajian tanpa mengubah database untuk mengevaluasi apakah total cicilan staf melebihi 50% gaji kotornya, dan memberikan peringatan dini (*warning*) jika mentok plafon regulasi.

---

## 4. Spesifikasi Endpoint REST API (Swagger)
File Controller: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollDeductionScheduleController.cs`
Tag Swagger: `[Tags("HR Deductions - Engine")]`

| Method | Path | Deskripsi | Otorisasi | Request / Query | Response Model |
| :--- | :--- | :--- | :--- | :--- | :--- |
| `GET` | `/api/hr-deductions/engine/simulate` | Simulasi evaluasi batas hard-cap 50% Gross Pay pegawai | Bearer JWT | Query `employeeId: Guid, grossPay: decimal` | `DeductionSimulationResultDto` |

---

## 5. Contoh Skenario Rumah Sakit Konkret
* **Skenario Evaluasi Hard-Cap Perawat Rawat Inap**:
  Suster Dewi (Gaji Kotor / Gross Pay bulan ini: Rp 6.000.000).
  Total cicilan terdaftar:
  - Kasbon Rumah Sakit: Rp 2.500.000
  - Cicilan Koperasi: Rp 1.500.000
  - Total Tagihan: Rp 4.000.000
  
  **Evaluasi Engine**:
  - Pagu Hard-Cap 50%: $50\% \times \text{Rp } 6.000.000 = \text{\bf Rp 3.000.000}$.
  - Tagihan Rp 4.000.000 melebihi pagu Rp 3.000.000 $\rightarrow$ `IsCapped = true`.
  - Sistem mengunci batas potong maksimal di Rp 3.000.000, sehingga gaji bersih yang dibawa pulang Suster Dewi tetap terjamin minimal Rp 3.000.000 untuk kebutuhan hidup layak.
