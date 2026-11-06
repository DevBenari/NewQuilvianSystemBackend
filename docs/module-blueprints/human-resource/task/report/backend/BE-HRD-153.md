# Laporan Penyelesaian Backend Task: BE-HRD-153

## 1. Identitas Task
* **ID Task**: `BE-HRD-153`
* **Nama Task**: Engine kalkulasi pemotongan PPh 21 bulanan Tarif Efektif Rata-Rata (TER) PMK 168/2023 (`PayrollTaxCalculationService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-30: Fondasi Siklus Penggajian & Penghasilan Tetap`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Sesuai Peraturan Menteri Keuangan Nomor 168 Tahun 2023 (PMK 168/2023) dan Peraturan Pemerintah Nomor 58 Tahun 2023 yang berlaku efektif mulai 1 Januari 2024, penghitungan Pajak Penghasilan Pasal 21 (PPh 21) bagi pegawai tetap menggunakan skema **Tarif Efektif Rata-Rata (TER)** untuk masa pajak bulanan (Januari sampai dengan November), dan penyesuaian tarif progresif **Pasal 17 ayat (1) huruf a UU PPh** pada masa pajak terakhir (Desember).

### Klasifikasi Kategori TER Berdasarkan Status PTKP
1. **Kategori TER A**:
   - Status PTKP: **TK/0** (Rp 54 jt), **TK/1** (Rp 58,5 jt), **K/0** (Rp 58,5 jt).
   - Memiliki 44 bracket tarif (0% s.d. 34%), batas tidak kena pajak bulanan s.d. Rp 5.400.000,00 (0%).
2. **Kategori TER B**:
   - Status PTKP: **TK/2** (Rp 63 jt), **TK/3** (Rp 67,5 jt), **K/1** (Rp 63 jt), **K/2** (Rp 67,5 jt).
   - Memiliki 40 bracket tarif (0% s.d. 34%), batas tidak kena pajak bulanan s.d. Rp 6.200.000,00 (0%).
3. **Kategori TER C**:
   - Status PTKP: **K/3** (Rp 72 jt).
   - Memiliki 40 bracket tarif (0% s.d. 34%), batas tidak kena pajak bulanan s.d. Rp 6.600.000,00 (0%).

### Skema Penyesuaian Masa Pajak Desember (Pasal 17 UU PPh / UU HPP)
Pada bulan Desember (masa pajak ke-12):
1. Penghasilan bruto disetahunkan (atau diakumulasi riil setahun).
2. Dikurangkan Biaya Jabatan 5% (maksimum Rp 6.000.000,00 setahun).
3. Dikurangkan PTKP tahunan sesuai status.
4. Nilai Penghasilan Kena Pajak (PKP) dikenakan tarif progresif Pasal 17:
   - s.d. Rp 60.000.000,00 : 5%
   - > Rp 60.000.000 s.d. Rp 250.000.000,00 : 15%
   - > Rp 250.000.000 s.d. Rp 500.000.000,00 : 25%
   - > Rp 500.000.000 s.d. Rp 5.000.000.000,00 : 30%
   - > Rp 5.000.000.000,00 : 35%

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollTaxDtos.cs`
* DTO yang dibuat:
  - `CalculatePph21ResultDto`: Hasil kalkulasi batch pemotongan PPh 21, total bruto kena pajak, total pajak PPh 21 terpotong, total gaji bersih final, dan daftar rincian per pegawai.
  - `EmployeePph21CalculationDto`: Rincian per pegawai (ID pegawai, nama, status PTKP, Kategori TER [A/B/C], nominal bruto bulanan, persentase tarif TER yang dikenakan, nominal potongan PPh 21, total seluruh potongan, dan gaji bersih).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollTaxCalculationService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollTaxCalculationService.cs`
* Logika Bisnis:
  1. Validasi status batch: Menolak penghitungan jika batch telah difinalisasi (`Approved`, `Paid`, `Closed`) atau terkunci (`IsLocked == true`).
  2. Resolusi Status PTKP: Mengecek `TaxStatusSnapshot` pada baris pegawai, jika kosong mengambil dari profil master `WfpTax` aktif, atau default `TK/0`.
  3. Evaluasi Kategori TER: Memetakan PTKP ke kategori TER A, B, atau C secara deterministik.
  4. Lookup Tabel TER PMK 168/2023: Mencari tarif efektif bulanan yang presisi berdasarkan nilai `GrossPay`.
  5. Penanganan Masa Pajak Desember: Menjalankan algoritma tarif progresif Pasal 17 dengan biaya jabatan dan PTKP tahunan.
  6. Pembentukan Komponen Payroll:
     - Dibuat baris `TrxPayrollEmployeeComponent` dengan kode `DED_PPH21`, jenis `Deduction`, sumber `Tax`, metode `Percentage`.
  7. Pembaruan Akumulasi:
     - `emp.TotalTax = pph21Amount`
     - `emp.TotalDeduction = otherDeductions + pph21Amount`
     - `emp.NetPay = emp.GrossPay - emp.TotalDeduction`
     - `emp.PaymentAmount = emp.NetPay`
  8. Agregasi Total Batch pada `TrxPayrollRun`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/calculate-pph21` | Hitung pemotongan PPh 21 TER PMK 168/2023 seluruh pegawai dalam batch | 200 OK / 422 Unprocessable | Update |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/employees/{employeeRunId}/calculate-pph21` | Hitung pemotongan PPh 21 untuk 1 pegawai spesifik | 200 OK / 404 Not Found | Update |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Staf Farmasi Rumah Sakit**:
  - Nama: Siti Aminah, S.Farm.
  - Status PTKP: Menikah dengan 1 anak (**K/1**) $\rightarrow$ **Kategori TER B**
  - Penghasilan Bruto Bulan Berjalan (`GrossPay`): Rp 9.500.000,00
* **Hasil Evaluasi TER PMK 168/2023**:
  1. Pada Kategori B, rentang penghasilan > Rp 8.850.000 s.d. Rp 9.800.000 dikenakan **Tarif TER: 2,00%**.
  2. Pemotongan PPh 21 Sebulan:
     $$\text{PPh 21} = 2,00\% \times \text{Rp } 9.500.000,00 = \text{Rp } 190.000,00$$
  3. Komponen `DED_PPH21` tersimpan: Rp 190.000,00.
  4. Total potongan bertambah Rp 190.000,00, dan gaji bersih (*Net Pay*) diperbarui secara instan dan akurat.

---

## 5. Verifikasi Acceptance Criteria
* [x] **Klasifikasi Otomatis TER A, B, C**: Memetakan seluruh kombinasi PTKP (TK/0 s.d. K/3) ke kategori TER yang benar.
* [x] **Lookup Bracket Presisi PMK 168/2023**: Menerapkan 44 bracket Kategori A, 40 bracket Kategori B, dan 40 bracket Kategori C.
* [x] **Dukungan Masa Pajak Desember**: Menerapkan perhitungan tarif progresif Pasal 17 UU PPh / UU HPP dengan biaya jabatan 5%.
* [x] **Komponen Transparan di Slip Gaji**: Membentuk komponen `DED_PPH21` dengan persentase tarif tertera jelas untuk transparansi pegawai.
* [x] **Zero Warning Kompilasi Baru**: Solusi backend `NewQuilvianSystemBackend` lulus `dotnet build` dengan **0 Error(s)**.
