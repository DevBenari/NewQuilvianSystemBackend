# Laporan Penyelesaian Backend Task: BE-HRD-155

## 1. Identitas Task
* **ID Task**: `BE-HRD-155`
* **Nama Task**: Payroll Variance Guard & deteksi otomatis deviasi net pay >= 20% / Rp 2jt (`PayrollVarianceGuardService`)
* **Modul / Sub-Modul**: Human Resource / Sub-modul 10 — Pemrosesan Penggajian Pegawai Rumah Sakit (Payroll V2)
* **Slice / Milestone**: `Slice S-B5` · `Fase MVP-31: Integrasi Potongan Koperasi, Variance Guard, & Alur Otorisasi 3-Jenjang`
* **Status Penyelesaian**: ✅ Selesai (100%)
* **Tanggal Implementasi**: 2026-11-06
* **Branch Git**: `MHamzah`

---

## 2. Deskripsi & Latar Belakang Bisnis
Kesalahan perhitungan remunerasi tenaga kesehatan di rumah sakit dapat berdampak fatal terhadap kepatuhan regulasi, kepuasan kerja tenaga medis, maupun kebocoran anggaran keuangan rumah sakit. Untuk mencegah anomali finansial lolos ke tahap pencairan bank, sistem menerapkan pengawas otomatis independen: **Payroll Variance Guard (`PAY-W1`, `PAY-W2`, `PAY-W3`, `PAY-W4`, `VR-PAY-13`)**.

Engine ini melakukan komparasi analitis gaji bersih (*Net Pay*) setiap pegawai pada batch berjalan terhadap gaji bersih periode sebelumnya yang telah dibayarkan:
1. **Ambang Batas Peringatan Standar (`PAY-W1`)**: Apabila gaji bersih mengalami kenaikan atau penurunan $\ge 20\%$ ATAU selisih nominal $\ge$ **Rp 2.000.000,00**, sistem menerbitkan tiket `TrxPayrollVarianceAlert` bertingkat keparahan `Warning`.
2. **Lonjakan Drastis Kritis (`PAY-W2`)**: Apabila gaji bersih melonjak $\ge 50\%$ (misalnya akibat penumpukan jam lembur cito atau rapel retrospektif masif), tingkat keparahan dinaikkan menjadi `Critical`.
3. **Proteksi Take-Home Pay Rendah (`PAY-W3`)**: Jika gaji bersih staf tersisa $\le 30\%$ dari gaji pokok akibat tingginya beban potongan, tiket variansi diterbitkan dengan keparahan `High`.
4. **Baseline Pegawai Baru (`PAY-W4`)**: Pegawai baru yang pertama kali menerima gaji (belum ada histori bulan lalu) otomatis ditandai sebagai anomali jenis `NewEmployeeBaseline` untuk diverifikasi penugasan SK-nya.

Seluruh alert yang terbit disetel dengan status awal `ResolutionStatus = "PendingReview"`.

---

## 3. Komponen yang Diimplementasikan

### A. Data Transfer Objects (DTO)
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/DTOs/PayrollVarianceDtos.cs`
* DTO yang dibuat:
  - `EvaluateVarianceResultDto`: Ringkasan hasil audit variansi batch (total pegawai diperiksa, total alert yang dibangkitkan, jumlah alert Critical, High, Warning, dan jumlah alert yang masih memerlukan justifikasi tertulis).
  - `PayrollVarianceAlertDto`: Rincian tiket anomali (identitas pegawai, unit kerja, tahun/bulan, gaji periode lalu, gaji saat ini, selisih nominal rupiah, persentase deviasi, tingkat keparahan, alasan alert, dan status resolusi).

### B. Antarmuka & Layanan Bisnis
* Berkas:
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/IPayrollVarianceGuardService.cs`
  - `Areas/Corporate/HumanResource/PayrollManagement/Services/PayrollVarianceGuardService.cs`
* Logika Bisnis:
  1. Resolusi Parameter Toleransi: Mengambil ambang `VarianceThresholdPercentage` (default 20%) dan `VarianceThresholdAmount` (default Rp 2.000.000) dari konfigurasi batch atau master setting.
  2. Kueri Komparasi Histori: Menarik data gaji bersih terakhir dari batch berstatus `Paid`, `Approved`, atau `Closed` untuk seluruh pegawai yang bersangkutan.
  3. Pembersihan Idempoten: Menghapus alert variansi lama yang masih berstatus `PendingReview` pada batch tersebut agar evaluasi ulang selalu mencerminkan data perhitungan mutakhir, sementara alert yang telah di-justifikasi tetap dipertahankan.
  4. Evaluasi Deviasi Berjenjang:
     - Jika tidak ada histori $\rightarrow$ Terbitkan alert `PAY-W4` (`Warning`).
     - Jika take-home pay $\le 30\%$ gaji pokok $\rightarrow$ Terbitkan alert `PAY-W3` (`High`).
     - Jika lonjakan $\ge 50\%$ $\rightarrow$ Terbitkan alert `PAY-W2` (`Critical`).
     - Jika deviasi $\ge 20\%$ atau selisih $\ge$ Rp 2jt $\rightarrow$ Terbitkan alert `PAY-W1` (`Warning`).
  5. Penyimpanan Tiket ke Entitas `TrxPayrollVarianceAlert`.

### C. Controller REST API Bergaya Swagger
* Berkas: `Areas/Corporate/HumanResource/PayrollManagement/Controllers/PayrollRunModernController.cs`
* Tag Swagger: `[Tags("HR Payroll - Runs & Batches")]`

| Method | Path | Deskripsi | Status HTTP | Otorisasi |
| :--- | :--- | :--- | :--- | :--- |
| `POST` | `/api/v1/corporate/human-resource/payroll/runs/{id}/evaluate-variances` | Jalankan audit deteksi anomali deviasi gaji bersih batch | 200 OK / 404 Not Found | Update |
| `GET` | `/api/v1/corporate/human-resource/payroll/runs/{id}/variances` | Tampilkan seluruh daftar tiket anomali variansi pada batch | 200 OK | Read |

---

## 4. Alur Proses Bisnis & Contoh Kasus Nyata Rumah Sakit
* **Kasus Dokter Ruangan Rawat Inap**:
  - Nama: dr. Anisa Larasati
  - Gaji Bersih Bulan Lalu: Rp 8.000.000,00
  - Gaji Bersih Bulan Berjalan: Rp 12.500.000,00 (Ada tambahan rapel lembur dan insentif jaga bangsal khusus)
* **Hasil Evaluasi Variance Guard**:
  1. Selisih Nominal Deviasi: $\text{Rp } 12.500.000 - \text{Rp } 8.000.000 = \text{Rp } 4.500.000,00$ (Melampaui batas toleransi Rp 2.000.000,00).
  2. Rasio Persentase Kenaikan: $\frac{4.500.000}{8.000.000} \times 100\% = +56,25\%$ (Melampaui ambang batas 50%).
  3. **Tiket Alert Otomatis Terbit**:
     - Status: `PendingReview`
     - Tingkat Keparahan: `Critical` (`PAY-W2`)
     - Alasan: *"Lonjakan drastis gaji bersih staf >= 50% (+56.3%, +Rp 4,500,000.00) dibanding bulan lalu (PAY-W2 CriticalSurge)."*
     - Dampak Operasional: Tiket ini wajib diisi justifikasi audit sebelum batch dapat diajukan ke manajer.

---

## 5. Verifikasi Acceptance Criteria
* [x] **Deteksi Otomatis Deviasi $\ge 20\%$ / Rp 2jt (`VR-PAY-13`)**: Menerbitkan tiket alert jika salah satu ambang batas terlampaui.
* [x] **Pemberian Skor Keparahan (Severity Multi-Tier)**: Membedakan tingkat `Critical` ($\ge 50\%$), `High` ($\le 30\%$ gaji pokok), dan `Warning`.
* [x] **Pencatatan Baseline Pegawai Baru (`PAY-W4`)**: Menandai staf baru tanpa histori bulan lalu secara otomatis.
* [x] **Audit Trail Rinci**: Mencatat nilai sebelum, sesudah, nominal selisih, dan persentase deviasi pada entitas database.
* [x] **Zero Warning Kompilasi Baru**: Solusi backend lulus `dotnet build` dengan **0 Error(s)**.
